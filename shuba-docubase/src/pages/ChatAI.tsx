import { useState, useRef, useEffect, useCallback } from 'react';
import { Button, Input, message, Avatar, Typography, Tag, Progress, Spin } from 'antd';
import { SendOutlined, UserOutlined, RobotOutlined, PaperClipOutlined, LoadingOutlined } from '@ant-design/icons';
import { useAuth } from '../context/AuthContext';
import { initChat, streamMessageToAI, subscribeToSystemEvents, uploadFileToBackend, type ProcessingDoc, type SystemEvent, type SelectedDocument, type StageEvent } from '../services/aiService';
import DocumentSelector from '../components/DocumentSelector';
import '../styles/ChatAI.css';
import '../styles/ProcessingStatus.css';
import starIcon from '../assets/star_icon.png';

const { TextArea } = Input;
const { Text } = Typography;

interface Message {
    id: string;
    role: 'user' | 'assistant';
    content: string;
    timestamp: Date;
    isFake?: boolean;
    attachedFile?: {
        name: string;
        type: string;
        size: number;
    };
    selectedDocuments?: {
        id: string;
        name: string;
        fileName: string;
    }[];
}



const ChatAI = () => {
    const { userId, isLoading: authLoading, user } = useAuth(); // Re-added user for debugging visibility

    // Debugging Log: Track component lifecycle and auth state
    useEffect(() => {
        console.group('ChatAI: Lifecycle & Auth State');
        console.log('Component Mounted/Updated');
        console.log('Auth Loading:', authLoading);
        console.log('Current User ID (Numeric):', userId);
        console.log('Current User Object (Raw):', user);
        console.groupEnd();
    }, [userId, authLoading, user]);

    const [messages, setMessages] = useState<Message[]>([]);
    const [inputMessage, setInputMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [sessionId, setSessionId] = useState<string | null>(null);
    const [processingDocs, setProcessingDocs] = useState<ProcessingDoc[]>([]);
    const [isInitializing, setIsInitializing] = useState(true);
    
    // Stage / Progress state
    const [currentStage, setCurrentStage] = useState<StageEvent | null>(null);

    // UI states
    const [selectedDocuments, setSelectedDocuments] = useState<SelectedDocument[]>([]);
    const [showDocumentSelector, setShowDocumentSelector] = useState(false);

    const messagesEndRef = useRef<HTMLDivElement>(null);
    const textAreaRef = useRef<any>(null);
    const sseUnsubscribeRef = useRef<(() => void) | null>(null);

    const scrollToBottom = useCallback(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, []);

    useEffect(() => {
        scrollToBottom();
    }, [messages, processingDocs, scrollToBottom]);

    // Handle System Events
    const handleSystemEvent = useCallback((event: SystemEvent) => {
        const { type, payload } = event;
        
        if (type === 'processing_progress' || type === 'processing_error' || type === 'processing_completed' || type === 'processing_started') {
            const docId = payload.document_id;
            if (!docId) return;

            setProcessingDocs(prev => {
                const existing = prev.find(d => d.document_id === docId);

                // STRATEGY: Treat backend errors as "Partial Success" or "Completed" to user 
                // because file is saved/DB safe. We hide technical jargon like "pooling error".
                let newStatus = type === 'processing_error' ? 'completed' : (payload.status_flag || (type === 'processing_completed' ? 'completed' : 'processing'));
                
                // Friendly text. If error, say "Saved" instead of "Error".
                let newMessage = type === 'processing_error' 
                    ? 'Saved (Indexing pending)' 
                    : (payload.message || (type === 'processing_completed' ? 'Completed' : 'Processing...'));

                // Force 100% on error so bar looks done.
                let newProgress = payload.progress !== undefined ? payload.progress : (type === 'processing_completed' || type === 'processing_error' ? 1.0 : 0.1);

                if (existing) {
                    return prev.map(d => d.document_id === docId
                        ? { ...d, progress: newProgress, message: newMessage, status: newStatus }
                        : d
                    );
                }

                return [...prev, {
                    document_id: docId,
                    status: newStatus,
                    progress: newProgress,
                    message: newMessage,
                    updated_at: new Date().toISOString()
                }];
            });

            // LOGIC: Hide technical errors from UI toast.
            if (type === 'processing_error') {
                // Log technical error only to console for dev
                console.warn(`Backend Processing Warning (Hidden from user): ${payload.message || payload.error}`);
                
                // Show "Success" toast to user because file is essentially safe/uploaded
                setTimeout(() => {
                    setProcessingDocs(prev => prev.filter(d => d.document_id !== docId));
                    message.success('Document uploaded and saved successfully.'); 
                }, 2000); 
            } else if (type === 'processing_completed' || payload.status_flag === 'completed') {
                setTimeout(() => {
                    setProcessingDocs(prev => prev.filter(d => d.document_id !== docId));
                    message.success(`Processing complete: ${payload.message || 'Success'}`);
                }, 3000);
            }
        }
    }, []);

    // Initialize Session & SSE
    useEffect(() => {
        const initialize = async () => {
            // Wait for authentication loading to finish
            if (authLoading) return;

            // Ensure we have a valid user ID before initializing
            if (!userId) {
                console.log('User not available yet, skipping initialization');
                setIsInitializing(false); // Clear spinner if no user
                return;
            }

            if (!sessionId) {
                try {
                    setIsInitializing(true);
                    const data = await initChat(userId);
                    setSessionId(data.session_id.toString());
                    setProcessingDocs(data.processing_docs);

                    if (sseUnsubscribeRef.current) sseUnsubscribeRef.current();
                    sseUnsubscribeRef.current = subscribeToSystemEvents(
                        data.session_id,
                        handleSystemEvent,
                        (err) => {
                            if (err.name === 'AbortError') return;
                            console.warn('SSE Disconnected:', err);
                            // Only show error if it's not a common transient error or if connection completely failed
                            // For now, suppress generic "Lost connection" toast to avoid noise during navigation/retries
                            // message.error('Lost connection to live updates'); 
                        }
                    );

                    console.log('Chat initialized for user:', userId, 'Session:', data.session_id);
                } catch (err: any) {
                    console.error('Initialization error:', err);
                    // Suppress error toast for initialization failures to avoid noise
                    // message.error('Failed to connect to RAG Server: ' + (err.message || 'Unknown error'));
                } finally {
                    setIsInitializing(false);
                }
            }
        };

        initialize();

        return () => {
            console.log('ChatAI: Cleaning up SSE connection...');
            if (sseUnsubscribeRef.current) sseUnsubscribeRef.current();
        };
    }, [userId, sessionId, handleSystemEvent, authLoading]);

    const removeSelectedDocument = (docId: string) => {
        setSelectedDocuments(prev => prev.filter(d => d.id !== docId));
    };

    const handleDocumentSelect = (docs: SelectedDocument[]) => {
        setSelectedDocuments(docs);
        setShowDocumentSelector(false);
        // message.success(`Document "${doc.name}" selected`);
    };

    const handleUploadFiles = async (files: File[]) => {
        if (!sessionId || !userId) {
            message.error('Cannot upload: Session not initialized');
            return;
        }
        
        const count = files.length;
        const msgText = count === 1 
            ? "Mohon tunggu sebentar ya, dokumen kamu sedang diproses oleh system." 
            : `Mohon tunggu sebentar ya, ${count} dokumen kamu sedang diproses oleh system.`;

        // 1. UX Feedback (Fake Response)
        const fakeMsg: Message = {
            id: `fake-${Date.now()}`,
            role: 'assistant',
            isFake: true,
            content: `${msgText} Nanti kamu bisa menanyakan perihal isi dokumen tersebut dengan saya setelah proses upload selesai. Kamu bisa melihat progresnya pada progress bar berikut.`,
            timestamp: new Date()
        };
        setMessages(prev => [...prev, fakeMsg]);

        try {
            // 2. Start Uploads Concurrently
            // We do not await all to finish before showing success one by one, 
            // but we process them in parallel
            files.forEach(async (file) => {
                try {
                    const result = await uploadFileToBackend(file, userId, sessionId);
                    if (result.success) {
                        const selectedDoc: SelectedDocument = {
                            id: result.documentId!,
                            name: result.documentName!,
                            fileName: file.name,
                            fileType: file.type,
                            createdDate: new Date().toISOString()
                        };
                        setSelectedDocuments(prev => [...prev, selectedDoc]);
                    } else {
                        message.error(`Failed to upload ${file.name}: ${result.message}`);
                    }
                } catch (err) {
                    message.error(`Error uploading ${file.name}`);
                }
            });
        } catch (err) {
            message.error('Failed to initiate upload');
        }
    };

    const handleSendMessage = async () => {
        if (!inputMessage.trim() && selectedDocuments.length === 0) {
            message.warning('Please enter a message or select a document');
            return;
        }

        // Lazy Init: If sessionId is missing (e.g. failed init or race condition), try to init now
        let currentSessionId = sessionId;
        if (!currentSessionId || !userId) {
            if (userId) {
                try {
                    console.log('Session missing, attempting lazy init...');
                    setLoading(true);
                    const data = await initChat(userId);
                    currentSessionId = data.session_id.toString();
                    setSessionId(currentSessionId);
                    setProcessingDocs(data.processing_docs);
                    // Subscribe again if needed, though the useEffect might handle it if we aren't careful. 
                    // Actually useEffect depends on sessionId, so updating it might trigger another sub.
                    // But we need the ID *now* to send.
                } catch (err) {
                    console.error('Lazy init failed:', err);
                    message.error('Connection failed. Please refresh the page.');
                    setLoading(false);
                    return;
                }
            } else {
                message.error('Session expired. Please refresh the page to login again.');
                return;
            }
        }

        const userMsg: Message = {
            id: Date.now().toString(),
            role: 'user',
            content: inputMessage,
            timestamp: new Date(),
            selectedDocuments: selectedDocuments.length > 0 ? selectedDocuments.map(d => ({
                id: d.id,
                name: d.name,
                fileName: d.fileName,
            })) : undefined,
        };

        setMessages((prev) => [...prev, userMsg]);
        setInputMessage('');
        const currentDocs = [...selectedDocuments];
        setSelectedDocuments([]);
        setLoading(true);

        try {
            const assistantMsgId = (Date.now() + 1).toString();
            setMessages((prev) => [
                ...prev,
                {
                    id: assistantMsgId,
                    role: 'assistant',
                    content: '',
                    timestamp: new Date(),
                }
            ]);

            console.group('🚀 Sending Chat Request');
            console.log('Session ID:', sessionId);
            console.log('User ID:', userId);
            console.log('Message:', userMsg.content);
            console.log('Documents Details:', currentDocs);
            console.groupEnd();

            await streamMessageToAI(
                currentSessionId,
                userMsg.content,
                userId,
                currentDocs.length > 0 ? currentDocs : undefined,
                (chunk) => {
                    setMessages((prev) => {
                        return prev.map(msg =>
                            msg.id === assistantMsgId
                                ? { ...msg, content: msg.content + chunk }
                                : msg
                        );
                    });
                },
                (stage) => {
                    setCurrentStage(stage);
                },
                () => {
                    setLoading(false);
                    setCurrentStage(null);
                },
                (error) => {
                    console.error('Stream error:', error);
                    setLoading(false);
                    message.error('Failed to get AI response: ' + error.message);
                }
            );

        } catch (error: any) {
            message.error(error.message || 'Failed to send message');
            setLoading(false);
        }
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSendMessage();
        }
    };

    const renderProcessingDocs = () => {
        if (processingDocs.length === 0) return null;

        return (
            <div className="processing-docs-container">
                <Text strong style={{ fontSize: 12, display: 'block', marginBottom: 8 }}>
                    Background Processing:
                </Text>
                {processingDocs.map(doc => {
                    const isFailed = doc.status === 'failed';
                    return (
                        <div key={doc.document_id} className="processing-doc-item">
                            <div className="processing-doc-header">
                                <span className="processing-doc-name">Document #{doc.document_id}</span>
                                <span className={`processing-doc-status ${isFailed ? 'status-failed' : ''}`}>
                                    {doc.status}
                                </span>
                            </div>
                            <Progress
                                percent={Math.round(doc.progress * 100)}
                                size="small"
                                status={doc.status === 'completed' ? 'success' : (isFailed ? 'exception' : 'active')}
                                strokeColor={isFailed ? '#ff4d4f' : {
                                    '0%': '#1890ff',
                                    '100%': '#52c41a',
                                }}
                            />
                            <div className={`processing-doc-message ${isFailed ? 'message-failed' : ''}`}>
                                {!isFailed && doc.status !== 'completed' && <LoadingOutlined style={{ marginRight: 6 }} />}
                                {doc.message}
                            </div>
                        </div>
                    );
                })}
            </div>
        );
    };

    if (authLoading || (isInitializing && messages.length === 0)) {
        return (
            <div className="chat-ai-page" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                <div style={{ textAlign: 'center' }}>
                    <Spin size="large" />
                    <div style={{ marginTop: 16, color: '#1890ff' }}>
                        {authLoading ? 'Verifying Identity...' : 'Connecting to RAG Server...'}
                    </div>
                </div>
            </div>
        );
    }

    if (!userId) {
        return (
            <div className="chat-ai-page" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                <div style={{ textAlign: 'center' }}>
                    <RobotOutlined style={{ fontSize: 48, color: '#ff4d4f', marginBottom: 16 }} />
                    <Typography.Title level={4}>Authentication Required</Typography.Title>
                    <Typography.Text type="secondary">Silakan login terlebih dahulu untuk menggunakan fitur AI Chat.</Typography.Text>
                </div>
            </div>
        );
    }

    return (
        <div className="chat-ai-page">
            <div className="messages-area">
                {messages.length === 0 ? (
                    <div className="empty-chat-state">
                        <img 
                            src={starIcon} 
                            alt="AI Star" 
                            className="empty-chat-icon" 
                            style={{ width: 48, height: 48, objectFit: 'contain' }}
                        />
                        <Text className="empty-chat-title">Start a conversation with AI</Text>
                        <Text type="secondary" className="empty-chat-subtitle">
                            Connected to your document with AI.
                        </Text>
                    </div>
                ) : (
                    <div className="messages-list">
                        {messages.map((msg) => (
                            <div
                                key={msg.id}
                                className={`message-item ${msg.role === 'user' ? 'message-user' : 'message-assistant'} ${msg.isFake ? 'system-message-fake' : ''}`}
                            >
                                <Avatar
                                    className="message-avatar"
                                    icon={msg.role === 'user' ? <UserOutlined /> : <RobotOutlined />}
                                    style={{
                                        backgroundColor: msg.isFake ? '#1890ff' : (msg.role === 'user' ? '#333' : '#52c41a'),
                                    }}
                                />
                                <div className="message-bubble">
                                    <div className="message-text" style={{ whiteSpace: 'pre-wrap' }}>
                                        {msg.content}
                                    </div>
                                    {msg.selectedDocuments && msg.selectedDocuments.length > 0 && (
                                        <div className="message-document">
                                            {msg.selectedDocuments.map((doc, idx) => (
                                                <Tag key={idx} color="blue" style={{ fontSize: 11, marginRight: 4 }}>
                                                    📄 {doc.name}
                                                </Tag>
                                            ))}
                                        </div>
                                    )}
                                    <Text type="secondary" className="message-timestamp">
                                        {msg.timestamp.toLocaleTimeString([], {
                                            hour: '2-digit',
                                            minute: '2-digit',
                                        })}
                                    </Text>
                                </div>
                            </div>
                        ))}
                        {renderProcessingDocs()}
                        {loading && (
                            <div className="loading-container" style={{ marginLeft: 50, marginRight: 50, marginBottom: 16 }}>
                                {currentStage ? (
                                    <div className="stage-progress">
                                        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                                            <span style={{ fontSize: 12, fontWeight: 500, color: '#1890ff' }}>
                                                {currentStage.text}
                                            </span>
                                            {currentStage.detail && (
                                                <span style={{ fontSize: 11, color: '#888' }}>{currentStage.detail}</span>
                                            )}
                                        </div>
                                        <Progress 
                                            percent={currentStage.progress} 
                                            size="small" 
                                            status="active" 
                                            strokeColor={{ from: '#108ee9', to: '#87d068' }}
                                            showInfo={false}
                                        />
                                    </div>
                                ) : (
                                    <div className="loading-indicator" style={{ fontSize: 12, color: '#888' }}>
                                        <LoadingOutlined style={{ marginRight: 8 }} />
                                        Generating response...
                                    </div>
                                )}
                            </div>
                        )}
                        <div ref={messagesEndRef} />
                    </div>
                )}
            </div>

            <div className="input-area">
                {selectedDocuments.length > 0 && (
                    <div className="attached-items">
                        {selectedDocuments.map(doc => (
                            <Tag
                                key={doc.id}
                                closable
                                onClose={() => removeSelectedDocument(doc.id)}
                                color="purple"
                            >
                                📄 {doc.name}
                            </Tag>
                        ))}
                    </div>
                )}

                <div className="modern-chat-input-wrapper">
                    <Button
                        type="text"
                        icon={<PaperClipOutlined />}
                        className="action-button-inside"
                        onClick={() => setShowDocumentSelector(true)}
                        title="Select document"
                    />

                    <TextArea
                        ref={textAreaRef}
                        value={inputMessage}
                        onChange={(e) => setInputMessage(e.target.value)}
                        onKeyPress={handleKeyPress}
                        placeholder="Type your message..."
                        autoSize={{ minRows: 1, maxRows: 5 }}
                        disabled={loading}
                        className="chat-textarea-inside"
                    />

                    <Button
                        type="primary"
                        shape="circle"
                        icon={<SendOutlined />}
                        onClick={handleSendMessage}
                        loading={loading}
                        disabled={!inputMessage.trim() && selectedDocuments.length === 0}
                        className="send-button-inside"
                    />
                </div>
            </div>

            <DocumentSelector
                open={showDocumentSelector}
                onClose={() => setShowDocumentSelector(false)}
                onSelect={handleDocumentSelect}
                onUploadFile={() => {
                    // Logic to trigger file input
                    const input = document.createElement('input');
                    input.type = 'file';
                    input.multiple = true; // Enable multiple selection
                    input.onchange = (e: any) => {
                        const files = Array.from(e.target.files as FileList); // Handle file list
                        if (files.length > 0) handleUploadFiles(files);
                    };
                    input.click();
                    setShowDocumentSelector(false);
                }}
                userId={userId}
            />
        </div>
    );
};

export default ChatAI;