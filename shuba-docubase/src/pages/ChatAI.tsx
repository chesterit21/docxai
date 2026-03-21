import { useState, useRef, useEffect, useCallback } from 'react';
import { Button, Input, message, Avatar, Typography, Tag, Progress, Spin, Row, Col, Card, List, Empty } from 'antd';
import { SendOutlined, UserOutlined, RobotOutlined, LoadingOutlined, SearchOutlined, CheckCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { useAuth } from '../context/AuthContext';
import { initChat, streamMessageToAI, subscribeToSystemEvents, uploadFileToBackend, getDocumentsFromBackend, getDocumentDetailsForAi, type ProcessingDoc, type SystemEvent, type SelectedDocument, type StageEvent, type DocumentItem } from '../services/aiService';
import '../styles/ChatAI.css';
import '../styles/ProcessingStatus.css';
import starIcon from '../assets/star_icon.png';
import dayjs from 'dayjs';

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
    const { userId: authUserId, isLoading: authLoading } = useAuth();
    const userId = authUserId || 1; // Bypass for dev
    const { config } = useAuth();
    const dateConfig = config?.dataGrid?.dateTimeFormat || "YYYY-MM-DD | HH:mm:ss";

    const [messages, setMessages] = useState<Message[]>([]);
    const [inputMessage, setInputMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [sessionId, setSessionId] = useState<string | null>(null);
    const [processingDocs, setProcessingDocs] = useState<ProcessingDoc[]>([]);
    const [isInitializing, setIsInitializing] = useState(true);
    const [currentStage, setCurrentStage] = useState<StageEvent | null>(null);

    // Sidebar States
    const [availableDocuments, setAvailableDocuments] = useState<DocumentItem[]>([]);
    const [searchQuery, setSearchQuery] = useState('');
    const [selectedDocuments, setSelectedDocuments] = useState<SelectedDocument[]>([]);
    const [isFetchingDocs, setIsFetchingDocs] = useState(false);

    const messagesEndRef = useRef<HTMLDivElement>(null);
    const textAreaRef = useRef<any>(null);
    const sseUnsubscribeRef = useRef<(() => void) | null>(null);

    const scrollToBottom = useCallback(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, []);

    useEffect(() => {
        scrollToBottom();
    }, [messages, processingDocs, scrollToBottom]);

    const fetchAvailableDocuments = useCallback(async (searchKey: string = '') => {
        setIsFetchingDocs(true);
        try {
            // Using new backend endpoint which supports all docs + searching
            const response = await getDocumentsFromBackend(searchKey);
            setAvailableDocuments(response.documents || []);
        } catch (err) {
            console.error('Failed to fetch documents from backend:', err);
        } finally {
            setIsFetchingDocs(false);
        }
    }, []);

    // Handle System Events (Upload/Processing status)
    const handleSystemEvent = useCallback((event: SystemEvent) => {
        const { type, payload } = event;
        
        if (type === 'processing_progress' || type === 'processing_error' || type === 'processing_completed' || type === 'processing_started') {
            const docId = payload.document_id;
            if (!docId) return;

            setProcessingDocs(prev => {
                const existing = prev.find(d => d.document_id === docId);
                let newStatus = type === 'processing_error' ? 'completed' : (payload.status_flag || (type === 'processing_completed' ? 'completed' : 'processing'));
                let newMessage = type === 'processing_error' ? 'Saved (Indexing pending)' : (payload.message || (type === 'processing_completed' ? 'Completed' : 'Processing...'));
                let newProgress = payload.progress !== undefined ? payload.progress : (type === 'processing_completed' || type === 'processing_error' ? 1.0 : 0.1);

                if (existing) {
                    return prev.map(d => d.document_id === docId ? { ...d, progress: newProgress, message: newMessage, status: newStatus } : d);
                }

                return [...prev, {
                    document_id: docId,
                    status: newStatus,
                    progress: newProgress,
                    message: newMessage,
                    updated_at: new Date().toISOString()
                }];
            });

            if (type === 'processing_error') {
                setTimeout(() => {
                    setProcessingDocs(prev => prev.filter(d => d.document_id !== docId));
                    message.success('Document uploaded and saved successfully.'); 
                }, 2000); 
            } else if (type === 'processing_completed' || payload.status_flag === 'completed') {
                setTimeout(() => {
                    setProcessingDocs(prev => prev.filter(d => d.document_id !== docId));
                    message.success(`Processing complete: ${payload.message || 'Success'}`);
                    fetchAvailableDocuments(); 
                }, 3000);
            }
        }
    }, [fetchAvailableDocuments]);

    // Initialize Session & SSE
    useEffect(() => {
        const initialize = async () => {
            if (authLoading) return;
            if (!userId) {
                setIsInitializing(false);
                return;
            }

            if (!sessionId) {
                try {
                    setIsInitializing(true);
                    const data = await initChat(userId);
                    setSessionId(data.session_id.toString());
                    setProcessingDocs(data.processing_docs);
                    fetchAvailableDocuments();

                    if (sseUnsubscribeRef.current) sseUnsubscribeRef.current();
                    sseUnsubscribeRef.current = subscribeToSystemEvents(
                        data.session_id,
                        handleSystemEvent,
                        (err) => {
                            if (err.name === 'AbortError') return;
                            console.warn('SSE Disconnected:', err);
                        }
                    );
                } catch (err: any) {
                    console.error('Initialization error:', err);
                } finally {
                    setIsInitializing(false);
                }
            }
        };

        initialize();

        return () => {
            if (sseUnsubscribeRef.current) sseUnsubscribeRef.current();
        };
    }, [userId, sessionId, handleSystemEvent, authLoading, fetchAvailableDocuments]);

    const handleSearch = () => {
        fetchAvailableDocuments(searchQuery);
    };

    // Use availableDocuments directly since filtering is done in backend
    const filteredDocuments = availableDocuments;

    const simulateStreaming = async (messageId: string, fullText: string) => {
        const tokens = fullText.split(' ');
        for (const token of tokens) {
            await new Promise(resolve => setTimeout(resolve, Math.random() * 50 + 50));
            setMessages((prev) => 
                prev.map(msg => 
                    msg.id === messageId 
                        ? { ...msg, content: msg.content + (msg.content ? ' ' : '') + token } 
                        : msg
                )
            );
        }
    };

    const toggleDocumentSelection = async (doc: DocumentItem) => {
        const docId = (doc.documentId || doc.document_id).toString();
        const isSelected = selectedDocuments.some(d => d.id === docId);
        
        if (isSelected) {
            setSelectedDocuments(prev => prev.filter(d => d.id !== docId));
        } else {
            const newDoc: SelectedDocument = {
                id: docId,
                name: doc.documentName || doc.title,
                fileName: doc.documentName || doc.title,
                createdDate: doc.insertedAt || doc.created_at
            };
            setSelectedDocuments(prev => [...prev, newDoc]);

            // Point 1: Trigger fetch and stream details
            try {
                const details = await getDocumentDetailsForAi(parseInt(docId));
                if (details) {
                    // Update the fileSize in selectedDocuments
                    setSelectedDocuments(prev => prev.map(d => 
                        d.id === docId ? { ...d, fileSize: details.fileSize } : d
                    ));

                    const assistantMsgId = `details-${Date.now()}`;
                    setMessages(prev => [...prev, {
                        id: assistantMsgId,
                        role: 'assistant',
                        content: '',
                        timestamp: new Date()
                    }]);

                    let formattedText = `**Detail Dokumen: ${details.title}**\n\n`;
                    formattedText += `* **Kategori:** ${details.categoryName || '-'}\n`;
                    formattedText += `* **Sub Kategori:** ${details.subCategoryName || '-'}\n`;
                    formattedText += `* **Tipe Dokumen:** ${details.documentTypeName || '-'}\n\n`;
                    
                    if (details.documentSummary) {
                        formattedText += `**Ringkasan:**\n${details.documentSummary}\n\n`;
                    }

                    if (details.entities && details.entities.length > 0) {
                        formattedText += `**Atribut Terdeteksi:**\n`;
                        details.entities.forEach((ent: any) => {
                            formattedText += `* ${ent.attributeName}: ${ent.value}\n`;
                        });
                    }

                    // Start streaming simulation
                    simulateStreaming(assistantMsgId, formattedText);
                }
            } catch (err) {
                console.error('Failed to fetch document details:', err);
            }
        }
    };

    // @ts-ignore: TS6133 - kept for future use (file upload feature)
    const handleUploadFiles = async (files: File[]) => {
        if (!sessionId || !userId) {
            message.error('Cannot upload: Session not initialized');
            return;
        }
        
        const count = files.length;
        const msgText = count === 1 
            ? "Mohon tunggu sebentar ya, dokumen kamu sedang diproses oleh system." 
            : `Mohon tunggu sebentar ya, ${count} dokumen kamu sedang diproses oleh system.`;

        const fakeMsg: Message = {
            id: `fake-${Date.now()}`,
            role: 'assistant',
            isFake: true,
            content: `${msgText} Nanti kamu bisa menanyakan perihal isi dokumen tersebut dengan saya setelah proses upload selesai.`,
            timestamp: new Date()
        };
        setMessages(prev => [...prev, fakeMsg]);

        files.forEach(async (file) => {
            try {
                const result = await uploadFileToBackend(file, userId, sessionId);
                if (result.success) {
                    // Handled by SSE
                } else {
                    message.error(`Failed to upload ${file.name}: ${result.message}`);
                }
            } catch (err) {
                message.error(`Error uploading ${file.name}`);
            }
        });
    };

    const handleSendMessage = async () => {
        if (!inputMessage.trim()) {
            return;
        }

        if (selectedDocuments.length === 0) {
            // Point 2: Custom Alert in chat message panel
            const alertMsg: Message = {
                id: `alert-${Date.now()}`,
                role: 'assistant',
                isFake: true,
                content: '⚠️ Silakan pilih dokumen terlebih dahulu pada sidebar sebelah kanan sebelum memulai percakapan.',
                timestamp: new Date()
            };
            setMessages(prev => [...prev, alertMsg]);
            return;
        }

        let currentSessionId = sessionId;
        if (!currentSessionId || !userId) {
            message.error('Session error. Please refresh the page.');
            return;
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
        // setSelectedDocuments([]); // Removed to persist document selection context across messages
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

    // We no longer block the entire UI with a spinner during initialization
    // to ensure Chat Input and main layout are visible.
    // If auth is still loading, we can show a minimal spinner.
    if (authLoading) {
        return (
            <div className="chat-ai-page" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                <Spin size="large" tip="Verifying Identity..." />
            </div>
        );
    }

    // We bypass userId check for now to allow UI development/debugging as requested.

    return (
        <div className="chat-ai-page" style={{ height: 'calc(100vh - 120px)' }}>
            <Row style={{ height: '100%', overflow: 'hidden' }}>
                {/* Chat Column (Left) */}
                <Col xs={24} md={16} lg={16} style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
                    <div className="messages-area">
                        {messages.length === 0 ? (
                            <div className="empty-chat-state">
                                {isInitializing ? (
                                    <>
                                        <Spin indicator={<LoadingOutlined style={{ fontSize: 24 }} spin />} />
                                        <Text style={{ marginTop: 12 }}>Connecting to RAG Server...</Text>
                                    </>
                                ) : (
                                    <>
                                        <img src={starIcon} alt="AI Star" className="empty-chat-icon" style={{ width: 48, height: 48 }} />
                                        <Text className="empty-chat-title">Start a conversation with AI</Text>
                                        <Text type="secondary" className="empty-chat-subtitle">Connected to your document with AI.</Text>
                                    </>
                                )}
                            </div>
                        ) : (
                            <div className="messages-list">
                                {messages.map((msg) => (
                                    <div key={msg.id} className={`message-item ${msg.role === 'user' ? 'message-user' : 'message-assistant'} ${msg.isFake ? 'system-message-fake' : ''}`}>
                                        <Avatar className="message-avatar" icon={msg.role === 'user' ? <UserOutlined /> : <RobotOutlined />} 
                                            style={{ backgroundColor: msg.isFake ? '#1890ff' : (msg.role === 'user' ? '#333' : '#52c41a') }} />
                                        <div className="message-bubble">
                                            <div className="message-text" style={{ whiteSpace: 'pre-wrap' }}>{msg.content}</div>
                                            {msg.selectedDocuments && msg.selectedDocuments.length > 0 && (
                                                <div className="message-document">
                                                    {msg.selectedDocuments.map((doc, idx) => (
                                                        <Tag key={idx} color="blue" style={{ fontSize: 11, marginRight: 4 }}>📄 {doc.name}</Tag>
                                                    ))}
                                                </div>
                                            )}
                                            <Text type="secondary" className="message-timestamp">
                                                {msg.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                            </Text>
                                        </div>
                                    </div>
                                ))}
                                
                                {processingDocs.length > 0 && (
                                    <div className="processing-docs-container" style={{ margin: '12px 0' }}>
                                        <Text strong style={{ fontSize: 12, display: 'block', marginBottom: 8 }}>Background Processing:</Text>
                                        {processingDocs.map(doc => (
                                            <div key={doc.document_id} className="processing-doc-item">
                                                <div className="processing-doc-header">
                                                    <span className="processing-doc-name">Doc #{doc.document_id}</span>
                                                    <span className={`processing-doc-status ${doc.status === 'failed' ? 'status-failed' : ''}`}>{doc.status}</span>
                                                </div>
                                                <Progress percent={Math.round(doc.progress * 100)} size="small" 
                                                    status={doc.status === 'completed' ? 'success' : (doc.status === 'failed' ? 'exception' : 'active')} />
                                                <div className="processing-doc-message">{doc.message}</div>
                                            </div>
                                        ))}
                                    </div>
                                )}

                                {loading && (
                                    <div className="loading-container" style={{ margin: '0 50px 16px' }}>
                                        {currentStage ? (
                                            <div className="stage-progress">
                                                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                                                    <span style={{ fontSize: 12, fontWeight: 500, color: '#1890ff' }}>{currentStage.text}</span>
                                                    {currentStage.detail && <span style={{ fontSize: 11, color: '#888' }}>{currentStage.detail}</span>}
                                                </div>
                                                <Progress percent={currentStage.progress} size="small" status="active" showInfo={false} />
                                            </div>
                                        ) : (
                                            <div style={{ fontSize: 12, color: '#888' }}><LoadingOutlined style={{ marginRight: 8 }} />Generating response...</div>
                                        )}
                                    </div>
                                )}
                                <div ref={messagesEndRef} />
                            </div>
                        )}
                    </div>

                    <div className="input-area">
                        <div className="modern-chat-input-wrapper">
                            <TextArea ref={textAreaRef} value={inputMessage} onChange={(e) => setInputMessage(e.target.value)}
                                onKeyPress={handleKeyPress} placeholder={isInitializing ? "Initializing server..." : "Type your message..."} autoSize={{ minRows: 1, maxRows: 5 }}
                                disabled={loading || isInitializing} className="chat-textarea-inside" />
                            <Button type="primary" shape="circle" icon={<SendOutlined />} onClick={handleSendMessage}
                                loading={loading} disabled={(isInitializing || !inputMessage.trim()) && selectedDocuments.length === 0} className="send-button-inside" />
                        </div>
                    </div>
                </Col>

                {/* Sidebar Column (Right) */}
                <Col xs={0} md={8} lg={8} className="chat-sidebar">
                    {/* Top: Search Section */}
                    <div className="sidebar-section search-section">
                        <div className="section-title">Cari Dokumen</div>
                        <Input.Search 
                            placeholder="Ketik untuk mencari..." 
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            onSearch={handleSearch}
                            enterButton={
                                <span>
                                    <SearchOutlined /> Cari
                                </span>
                            }
                            loading={isFetchingDocs}
                            allowClear 
                            style={{ backgroundColor: '#fff', borderRadius: '6px' }}
                        />
                    </div>

                    {/* Middle: Results Section */}
                    <div className="sidebar-section search-results-section" 
                        style={filteredDocuments.length === 0 ? { flex: '0 0 auto', height: '75px' } : {}}>
                        <div className="section-title">
                            <span>List Dokumen</span>
                            {isFetchingDocs && <Spin size="small" style={{ marginLeft: 8 }} />}
                        </div>
                        <List dataSource={filteredDocuments} loading={isFetchingDocs} locale={{ emptyText: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Dokumen tidak ditemukan" /> }}
                            renderItem={(doc) => {
                                const isSelected = selectedDocuments.some(d => d.id === (doc.documentId || doc.document_id).toString());
                                return (
                                    <Card size="small" 
                                        className={`doc-item-card ${isSelected ? 'selected-doc-card' : ''}`} 
                                        onClick={() => toggleDocumentSelection(doc)}
                                    >
                                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }} title={doc.documentName || doc.title}>
                                            <div style={{ flex: 1, overflow: 'hidden' }}>
                                                <div className="doc-title" style={{ whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{doc.documentName || doc.title}</div>
                                                <div className="doc-meta">{dayjs(doc.insertedAt || doc.created_at).format(dateConfig)}</div>
                                            </div>
                                            {isSelected ? <CheckCircleOutlined style={{ color: '#52c41a' }} /> : <PlusOutlined style={{ color: '#bfbfbf' }} />}
                                        </div>
                                    </Card>
                                );
                            }}
                        />
                    </div>

                    {/* Bottom: Selection Section */}
                    <div className="sidebar-section selected-section">
                        <div className="section-title">
                            <span>Dokumen Terpilih ({selectedDocuments.length})</span>
                            {selectedDocuments.length > 0 && <Button type="link" size="small" onClick={() => setSelectedDocuments([])} danger>Reset</Button>}
                        </div>
                        {selectedDocuments.length === 0 ? (
                            <div style={{ textAlign: 'center', padding: '20px 0', color: '#bfbfbf' }}>
                                <Text type="secondary" style={{ fontSize: 12 }}>Klik dokumen di atas untuk mulai mengobrol</Text>
                            </div>
                        ) : (
                            <div style={{ display: 'flex', flexDirection: 'column', gap: 6, maxHeight: '200px', overflowY: 'auto', paddingRight: '4px' }}>
                                {selectedDocuments.map(doc => (
                                    <Tag key={doc.id} closable onClose={() => toggleDocumentSelection({ documentId: parseInt(doc.id), documentName: doc.name } as any)} 
                                        color="purple" style={{ margin: 0, width: '100%', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                        <span style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }} title={doc.name}>
                                            📄 {doc.name}
                                        </span>
                                    </Tag>
                                ))}
                            </div>
                        )}
                    </div>
                </Col>
            </Row>
        </div>
    );
};

export default ChatAI;