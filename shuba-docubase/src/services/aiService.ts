import axios from 'axios';
import { fetchEventSource } from '@microsoft/fetch-event-source';
import CryptoJS from 'crypto-js';
import apiClient from './apiClient';

// AI Server Configuration
const AI_SERVER_URL = import.meta.env.VITE_API_URL || 'http://localhost:5265';
const SECURITY_CONFIG = {
    APP_ID: 'DMS-CLIENT-APP-2026',
    API_KEY: 'SHUBA-APP-DMS-RAG',
    SIGNATURE_ENABLED: true,
};

const ENDPOINTS = {
    SESSION_NEW: '/api/chat/session/new',
    CHAT_STREAM: '/api/chat/stream',
    GET_DOCUMENTS: '/top/chat-ai-search',
    GET_DOCUMENT_DETAILS_AI: '/top/document-details-ai',
    MODELS: '/v1/models',
    HEALTH: '/health',
    DOCUMENTS: '/api/documents',
    UPLOAD: '/api/upload',
    INIT: '/api/chat/init',
    EVENTS: '/api/chat/events',
};



export interface SelectedDocument {
    id: string;
    name: string;
    fileName: string;
    fileType?: string;
    createdDate?: string;
    originalId?: number;
    fileSize?: number;
}

export interface DocumentItem {
    documentId: number;
    documentName: string;
    description: string;
    insertedAt: string;
    document_id: number; // Keep for backward compatibility if any
    title: string;       // Keep for backward compatibility
    created_at: string;  // Keep for backward compatibility
}

export interface ProcessingDoc {
    document_id: number;
    status: string;
    progress: number;
    message: string;
    updated_at: string;
}

interface DocumentListResponse {
    documents: DocumentItem[];
    total: number;
}

export interface InitResponse {
    session_id: number;
    documents: DocumentItem[];
    processing_docs: ProcessingDoc[];
}

export type SystemEventType =
    | 'processing_started'
    | 'processing_progress'
    | 'processing_completed'
    | 'processing_error'
    | 'error'
    | 'info';

export interface SystemEventPayload {
    progress?: number;
    message?: string;
    status_flag?: string;
    document_id?: number;
    filename?: string;
    chunks_count?: number;
    error?: string;
}

export interface SystemEvent {
    type: SystemEventType;
    payload: SystemEventPayload;
}

/**
 * Generate security headers with HMAC signature
 */
// Global offset to handle clock skew between client and server
let serverTimeOffset = 0;

/**
 * Generate security headers with HMAC signature
 */
const generateHeaders = async (): Promise<Record<string, string>> => {
    // Apply offset to current time
    const adjustedTime = Date.now() + serverTimeOffset;
    const timestamp = Math.floor(adjustedTime / 1000).toString();

    const headers: Record<string, string> = {
        'Content-Type': 'application/json',
        'X-App-ID': SECURITY_CONFIG.APP_ID,
        'X-API-Key': SECURITY_CONFIG.API_KEY,
        'X-Request-Timestamp': timestamp,
    };

    if (SECURITY_CONFIG.SIGNATURE_ENABLED) {
        try {
            // Use CryptoJS for robust HMAC-SHA256 signing (Works on HTTP/Localhost)
            // Format: HMAC-SHA256(APP_ID + TIMESTAMP, API_KEY)
            const message = SECURITY_CONFIG.APP_ID + timestamp;
            const signature = CryptoJS.HmacSHA256(message, SECURITY_CONFIG.API_KEY).toString();

            headers['X-Request-Signature'] = signature;
        } catch (error) {
            console.error('Failed to generate signature:', error);
        }
    }

    return headers;
};

/**
 * Helper to handle API calls with automatic retry on 401 (Time Skew Sync)
 */
const safeApiCall = async <T>(
    apiFn: () => Promise<T>,
    retryCount = 0
): Promise<T> => {
    try {
        return await apiFn();
    } catch (error: any) {
        // If 401 Unauthorized and we haven't retried too many times
        if (error.response?.status === 401 && retryCount < 2) {
            console.warn(`Hit 401. Attempting time sync... (Retry ${retryCount + 1})`);

            // Try to extract server time from headers
            const serverDate = error.response.headers['date'];
            if (serverDate) {
                const serverTime = new Date(serverDate).getTime();
                const clientTime = Date.now();
                serverTimeOffset = serverTime - clientTime;
                console.log(`Time skew detected. Offset adjusted by ${serverTimeOffset}ms`);
            } else {
                // Fallback: simple heuristic, maybe just nudge it forward?
                // For now, assume slight delay if no header (though usually standard servers send Date)
            }

            // Retry the operation (it will generate NEW headers with updated offset)
            return safeApiCall(apiFn, retryCount + 1);
        }
        throw error;
    }
};


/**
 * Fetch document details for AI display
 */
export const getDocumentDetailsForAi = async (documentId: number) => {
    const response = await apiClient.get(`${ENDPOINTS.GET_DOCUMENT_DETAILS_AI}/${documentId}`);
    return response.data?.data;
};

/**
 * Initialize chat session and fetch documents
 */
export const initChat = async (_userId: number, sessionId?: string | number): Promise<InitResponse> => {
    // Mock the backend init response as it's no longer present
    return {
        session_id: sessionId ? (typeof sessionId === 'string' ? parseInt(sessionId) : sessionId) : Math.floor(Math.random() * 1000000),
        documents: [],
        processing_docs: []
    };
};

/**
 * Send message to AI server with streaming response
 */
export interface StageEvent {
    phase: string;
    progress: number;
    text: string;
    detail?: string;
}

/**
 * Send message to AI server with streaming response
 */
export const streamMessageToAI = async (
    sessionId: string,
    userMessage: string,
    userId: number,
    selectedDocuments: SelectedDocument[] | undefined,
    onMessage: (chunk: string) => void,
    onStage: (stage: StageEvent) => void,
    onDone: () => void,
    onError: (error: any) => void
) => {
    try {
        const documentIds = selectedDocuments?.map(d => parseInt(d.id)).filter(id => !isNaN(id)) || [];
        const documentSizes = selectedDocuments?.map(d => d.fileSize || 0) || [];

        const headers = await generateHeaders();
        const dataBody = JSON.stringify({
            session_id: parseInt(sessionId),
            user_id: userId,
            message: userMessage,
            document_ids: documentIds.length > 0 ? documentIds : undefined,
            document_sizes: documentIds.length > 0 ? documentSizes : undefined
        });

        console.log('dataBody :', dataBody);

        await fetchEventSource(`${AI_SERVER_URL}${ENDPOINTS.CHAT_STREAM}`, {
            method: 'POST',
            headers: headers,
            body: dataBody,
            onmessage(msg) {
                try {
                    if (msg.event === 'stage') {
                        const stageData: StageEvent = JSON.parse(msg.data);
                        onStage(stageData);
                    } else if (msg.event === 'message') {
                        // V2 Protocol: {"delta": "..."}
                        // Fallback: raw text if not JSON
                        let textChunk = msg.data;
                        if (msg.data.startsWith('{')) {
                            try {
                                const parsed = JSON.parse(msg.data);
                                if (parsed.delta) textChunk = parsed.delta;
                            } catch (e) {
                                // ignore, use raw
                            }
                        }
                        onMessage(textChunk);
                    } else if (msg.event === 'done') {
                        // Stream finished
                    } else if (msg.event === 'error') {
                        const errData = JSON.parse(msg.data);
                        onError(new Error(errData.message || 'Unknown server error'));
                    }
                } catch (e) {
                    console.error('Error parsing SSE message:', e);
                }
            },
            onclose() {
                onDone();
            },
            onerror(err) {
                onError(err);
                throw err;
            }
        });
    } catch (error) {
        console.error('Stream error:', error);
        onError(error);
    }
};

/**
 * Subscribe to system events via SSE
 */
export const subscribeToSystemEvents = (
    _sessionId: string | number,
    _onEvent: (event: SystemEvent) => void,
    _onError?: (error: any) => void
): () => void => {
    // Disabled event subscription since C# backend doesn't implement /api/chat/events
    return () => {};
};

/**
 * Fetch documents for a user
 */
/**
 * Fetch documents for a user
 */
export const getDocuments = async (userId: number): Promise<DocumentItem[]> => {
    return safeApiCall(async () => {
        const headers = await generateHeaders();

        const response = await axios.post<DocumentListResponse>(
            `${AI_SERVER_URL}${ENDPOINTS.DOCUMENTS}`,
            { user_id: userId },
            { headers }
        );

        if (response.data && response.data.documents) {
            return response.data.documents;
        }

        return [];
    });
};

/**
 * Upload file to backend
 */
export const uploadFileToBackend = async (
    file: File,
    userId: number,
    sessionId: string | number
): Promise<{ success: boolean; documentId?: string; documentName?: string; message?: string }> => {
    return safeApiCall(async () => {
        const headers = await generateHeaders();
        delete headers['Content-Type'];

        const formData = new FormData();
        formData.append('user_id', userId.toString());
        formData.append('session_id', sessionId.toString());
        formData.append('file', file);

        const response = await axios.post(
            `${AI_SERVER_URL}${ENDPOINTS.UPLOAD}`,
            formData,
            { headers }
        );

        if (response.data && response.data.documentId !== undefined) {
            return {
                success: true,
                documentId: response.data.documentId.toString(),
                documentName: response.data.documentName,
                message: response.data.message
            };
        }

        return {
            success: false,
            message: response.data.message || 'Upload failed'
        };
    }).catch((error: any) => {
        console.error('File upload failed (after retry):', error);
        return {
            success: false,
            message: error.message || 'File upload failed due to network error'
        };
    });
};

export const testAIConnection = async (): Promise<boolean> => {
    try {
        const response = await axios.get(`${AI_SERVER_URL}${ENDPOINTS.HEALTH}`, {
            timeout: 5000,
        });
        return response.status === 200;
    } catch (error) {
        console.error('AI server health check failed:', error);
        return false;
    }
};

export const getAvailableModels = async (): Promise<string[]> => {
    return ['rag-default'];
};

/**
 * Fetch documents from main .NET backend (TopController)
 */
export const getDocumentsFromBackend = async (searchText: string = '', page: number = 1, limit: number = 100): Promise<DocumentListResponse> => {
    const response = await apiClient.get(`/Top/chat-ai-search?Textsearch=${encodeURIComponent(searchText)}&Page=${page}&Limit=${limit}`);
    return {
        documents: response.data?.data?.data || [],
        total: response.data?.data?.totalRecords || 0
    };
};