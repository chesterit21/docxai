import { Modal, Input, Table, Button, Space, Tag, Typography, message, Spin } from 'antd';
import { SearchOutlined, FileTextOutlined, FilePdfOutlined, FileExcelOutlined, FileWordOutlined, FileOutlined, UploadOutlined } from '@ant-design/icons';
import { useState, useMemo, useEffect } from 'react';
import type { ColumnsType } from 'antd/es/table';
import { getDocuments } from '../services/aiService';
import '../styles/DocumentSelector.css';

const { Text } = Typography;

// Mapped document interface for UI
interface SelectorDocument {
    id: string;
    name: string;
    fileName: string;
    fileType: string;
    createdDate: string;
    originalId: number; // Keep track of original ID
}

interface DocumentSelectorProps {
    open: boolean;
    onClose: () => void;
    onSelect: (documents: SelectorDocument[]) => void;
    onUploadFile: () => void;
    userId?: number | null;
}

const DocumentSelector = ({ open, onClose, onSelect, onUploadFile, userId }: DocumentSelectorProps) => {
    const [searchText, setSearchText] = useState('');
    const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
    const [documents, setDocuments] = useState<SelectorDocument[]>([]);
    const [loading, setLoading] = useState(false);

    // Fetch documents when modal opens
    useEffect(() => {
        if (open && userId) {
            fetchDocuments();
        }
    }, [open, userId]);

    const fetchDocuments = async () => {
        if (!userId) return;

        setLoading(true);
        try {
            const apiDocs = await getDocuments(userId as number);

            // Map API response to UI model
            const mappedDocs: SelectorDocument[] = apiDocs.map(doc => {
                // Simple file type detection based on extension or title
                let fileType = 'Unknown';
                const titleLower = doc.title.toLowerCase();
                if (titleLower.endsWith('.pdf')) fileType = 'PDF';
                else if (titleLower.endsWith('.xlsx')) fileType = 'Excel';
                else if (titleLower.endsWith('.docx')) fileType = 'Word';
                else if (titleLower.endsWith('.pptx')) fileType = 'PowerPoint';
                else if (titleLower.includes('.txt')) fileType = 'Text';

                return {
                    id: doc.document_id.toString(),
                    name: doc.title, // Title is often the filename in this system
                    fileName: doc.title,
                    fileType: fileType,
                    createdDate: doc.created_at,
                    originalId: doc.document_id
                };
            });

            // Dedup by ID
            const uniqueDocs = mappedDocs.filter((doc, index, self) => 
                index === self.findIndex((t) => t.id === doc.id)
            );

            setDocuments(uniqueDocs);
        } catch (error) {
            console.error('Error fetching documents:', error);
            message.error('Failed to load documents');
        } finally {
            setLoading(false);
        }
    };

    // Filter documents based on search
    const filteredDocuments = useMemo(() => {
        if (!searchText) return documents;

        const search = searchText.toLowerCase();
        return documents.filter(
            (doc) =>
                doc.name.toLowerCase().includes(search) ||
                doc.fileName.toLowerCase().includes(search) ||
                doc.fileType.toLowerCase().includes(search)
        );
    }, [searchText, documents]);

    const getFileIcon = (fileType: string) => {
        switch (fileType.toLowerCase()) {
            case 'pdf':
                return <FilePdfOutlined style={{ color: '#ff4d4f', fontSize: 16 }} />;
            case 'excel':
                return <FileExcelOutlined style={{ color: '#52c41a', fontSize: 16 }} />;
            case 'word':
                return <FileWordOutlined style={{ color: '#1890ff', fontSize: 16 }} />;
            case 'powerpoint':
                return <FileTextOutlined style={{ color: '#fa8c16', fontSize: 16 }} />;
            default:
                return <FileOutlined style={{ color: '#8c8c8c', fontSize: 16 }} />;
        }
    };

    const getFileTypeColor = (fileType: string) => {
        switch (fileType.toLowerCase()) {
            case 'pdf':
                return 'red';
            case 'excel':
                return 'green';
            case 'word':
                return 'blue';
            case 'powerpoint':
                return 'orange';
            default:
                return 'default';
        }
    };

    const formatDate = (dateStr: string) => {
        try {
            const date = new Date(dateStr);
            return date.toLocaleDateString('id-ID', {
                day: '2-digit',
                month: 'short',
                year: 'numeric',
            });
        } catch (e) {
            return dateStr;
        }
    };

    const columns: ColumnsType<SelectorDocument> = [
        {
            title: 'Document Name',
            dataIndex: 'name',
            key: 'name',
            render: (text: string, record: SelectorDocument) => (
                <Space>
                    {getFileIcon(record.fileType)}
                    <div>
                        <div style={{ fontWeight: 500, fontSize: 13 }}>{text}</div>
                        <Text type="secondary" style={{ fontSize: 11 }}>
                            {record.fileName}
                        </Text>
                    </div>
                </Space>
            ),
        },
        {
            title: 'Type',
            dataIndex: 'fileType',
            key: 'fileType',
            width: 100,
            render: (type: string) => (
                <Tag color={getFileTypeColor(type)} style={{ fontSize: 11 }}>
                    {type}
                </Tag>
            ),
        },
        {
            title: 'Date',
            dataIndex: 'createdDate',
            key: 'createdDate',
            width: 120,
            render: (date: string) => (
                <Text style={{ fontSize: 12 }}>{formatDate(date)}</Text>
            ),
        },
    ];

    const onSelectChange = (newSelectedRowKeys: React.Key[]) => {
        setSelectedRowKeys(newSelectedRowKeys);
    };

    const rowSelection = {
        selectedRowKeys,
        onChange: onSelectChange,
    };

    const handleSelect = () => {
        const selectedDocs = documents.filter((doc) => selectedRowKeys.includes(doc.id));
        if (selectedDocs.length > 0) {
            onSelect(selectedDocs);
            setSearchText('');
            setSelectedRowKeys([]);
        }
    };

    const handleCancel = () => {
        onClose();
        setSearchText('');
        setSelectedRowKeys([]);
    };

    const handleUpload = () => {
        onUploadFile();
        setSearchText('');
        setSelectedRowKeys([]);
    };

    return (
        <Modal
            title={
                <div style={{ fontSize: 16, fontWeight: 600 }}>
                    Select Documents
                </div>
            }
            open={open}
            onCancel={handleCancel}
            width={700}
            footer={
                <Space>
                    <Button icon={<UploadOutlined />} onClick={handleUpload}>
                        Upload New File
                    </Button>
                    <Button onClick={handleCancel}>Cancel</Button>
                    <Button type="primary" onClick={handleSelect} disabled={selectedRowKeys.length === 0}>
                        Select ({selectedRowKeys.length})
                    </Button>
                </Space>
            }
            className="document-selector-modal"
        >
            <div className="document-selector-content">
                <Input
                    placeholder="Search documents..."
                    prefix={<SearchOutlined />}
                    value={searchText}
                    onChange={(e) => setSearchText(e.target.value)}
                    className="search-input"
                    allowClear
                />

                {loading ? (
                    <div style={{ textAlign: 'center', padding: '40px 0' }}>
                        <Spin tip="Loading documents..." />
                    </div>
                ) : (
                    <Table
                        rowSelection={rowSelection}
                        columns={columns}
                        dataSource={filteredDocuments}
                        rowKey="id"
                        size="small"
                        pagination={{
                            pageSize: 8,
                            showSizeChanger: false,
                            showTotal: (total) => `Total ${total} documents`,
                        }}
                        onRow={(record) => ({
                            onClick: () => {
                                // Toggle selection on row click
                                const key = record.id;
                                const index = selectedRowKeys.indexOf(key);
                                let newKeys = [...selectedRowKeys];
                                if (index >= 0) {
                                    newKeys.splice(index, 1);
                                } else {
                                    newKeys.push(key);
                                }
                                setSelectedRowKeys(newKeys);
                            },
                            style: {
                                cursor: 'pointer',
                            },
                        })}
                        className="documents-table"
                        scroll={{ y: 400 }}
                        locale={{ emptyText: 'No documents found' }}
                    />
                )}
            </div>
        </Modal>
    );
};

export default DocumentSelector;