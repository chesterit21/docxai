# AI Chat Integration - REVAMPED UI 🚀

## 🎨 What's New in This Version?

### Major UI Changes

1. ✅ **Removed header** - No more "AI Assistant Powered by Qwen 2.5" header
2. ✅ **Removed Clear button** - Cleaner interface
3. ✅ **Natural background** - Chat messages with transparent/gradient background
4. ✅ **Modern input area** - Clean, minimal design like ChatGPT/Claude
5. ✅ **Document selector** - Beautiful modal popup with searchable table
6. ✅ **Drag & Drop** - Upload files by dragging to the page
7. ✅ **Auto-resize textarea** - Input grows with content (max 5 rows)

## 📁 Files to Install

### New Files

1. **ChatAI_new.tsx** → `src/pages/ChatAI.tsx` (replace existing)
2. **ChatAI_new.css** → `src/pages/ChatAI.css` (replace existing)
3. **DocumentSelector.tsx** → `src/components/DocumentSelector.tsx` (new)
4. **DocumentSelector.css** → `src/components/DocumentSelector.css` (new)
5. **aiService_new.ts** → `src/services/aiService.ts` (replace existing)

### Keep These (from previous)

- **Menu.tsx** → `src/components/molecule/layouts/Menu.tsx`
- **index.tsx** → `src/router/index.tsx`

## 🚀 Installation Steps

### Step 1: Backup Existing Files

```bash
cd E:\inetpub\wwwroot\release-dms-frontend

# Backup old files (optional)
cp src/pages/ChatAI.tsx src/pages/ChatAI.tsx.backup
cp src/pages/ChatAI.css src/pages/ChatAI.css.backup
cp src/services/aiService.ts src/services/aiService.ts.backup
```

### Step 2: Copy New Files

```bash
# Copy revamped ChatAI
cp ChatAI_new.tsx src/pages/ChatAI.tsx
cp ChatAI_new.css src/pages/ChatAI.css

# Copy DocumentSelector component
cp DocumentSelector.tsx src/components/
cp DocumentSelector.css src/components/

# Copy updated AI service
cp aiService_new.ts src/services/aiService.ts
```

### Step 3: Rebuild & Restart

```bash
npm run build
pm2 restart dms-fe-staging
```

## 🎯 New Features Explained

### 1. Document Selector Modal

**Triggered by:** Click paperclip icon (📎)

**Features:**

- Searchable document list
- Shows: Document name, file name, type, date
- Dummy data included (8 sample documents)
- "Upload New File" button
- Clean table design with icons

**To Replace with Real API:**
Edit `src/components/DocumentSelector.tsx`:

```typescript
// Line 17-52: Replace DUMMY_DOCUMENTS with API call
useEffect(() => {
  fetchDocuments();
}, []);

const fetchDocuments = async () => {
  const response = await apiClient.get('/documents/list');
  setDocuments(response.data.data);
};
```

### 2. Drag & Drop File Upload

**How it works:**

- Drag any file to the chat page
- Blue overlay appears: "Drop file here to attach"
- File preview shows as a tag above input
- Max file size: 10MB
- Supported: PDF, DOC, DOCX, XLS, XLSX, TXT, JPG, PNG

**File is sent as base64** to AI server in payload.

### 3. Auto-resize Textarea

- Starts at 1 row
- Grows automatically as you type
- Max 5 rows, then scrolls
- Press Enter to send
- Shift+Enter for new line

### 4. Message Display

**User messages:**

- White bubble
- Right-aligned
- Shows attached file/document info

**AI responses:**

- White bubble with slight transparency
- Left-aligned
- Avatar with robot icon

### 5. Typing Indicator

Animated dots while AI is processing.

## 📊 Payload Structure

### Sending to AI Server

```json
{
  "messages": [
    {
      "role": "system",
      "content": "System prompt with context..."
    },
    {
      "role": "user", 
      "content": "User's message"
    }
  ],
  "temperature": 0.7,
  "max_tokens": 1024,
  "stream": false
}
```

### System Prompt Includes

1. **User context:**
   - Name, email, company, user type

2. **Uploaded file info** (if any):
   - File name, type, base64 content

3. **Selected document info** (if any):
   - Document ID, name, file name

## 🎨 UI Design Details

### Color Scheme

- **Primary gradient:** Purple (#667eea to #764ba2)
- **User messages:** White background
- **AI messages:** White with transparency
- **Accents:** Blue for user, Green for AI

### Typography

- **Message text:** 15px
- **Document list:** 13px (compact)
- **Timestamps:** 11px
- **Input:** 15px

### Animations

- Message slide-in: 0.3s ease-out
- Typing indicator bounce
- Float animation on empty state
- Smooth hover effects

## 🔧 Customization Guide

### Change Colors

Edit `ChatAI.css`:

```css
/* Line 7: Main gradient */
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);

/* Line 199: Send button gradient */
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
```

### Change Max File Size

Edit `ChatAI.tsx` line 68:

```typescript
const maxSize = 10 * 1024 * 1024; // Change 10 to desired MB
```

### Change Message History Length

Edit `aiService.ts` line 82:

```typescript
...conversationHistory.slice(-10) // Change -10 to desired number
```

### Add More File Types

Edit `ChatAI.tsx` line 264:

```typescript
accept=".pdf,.doc,.docx,.xls,.xlsx,.txt,.jpg,.jpeg,.png,.mp4,.zip"
```

## 🔌 Backend Integration Points

### 1. Document List API

**Endpoint needed:** `GET /api/documents/list`

**Expected response:**

```json
{
  "data": [
    {
      "id": "doc-001",
      "name": "Annual Report",
      "fileName": "report.pdf",
      "fileType": "PDF",
      "createdDate": "2024-01-15"
    }
  ]
}
```

### 2. File Upload API

**Endpoint needed:** `POST /api/files/upload`

**Request format:** `multipart/form-data`

```
file: [binary]
userId: [string]
```

**Expected response:**

```json
{
  "success": true,
  "fileId": "file-123",
  "message": "File uploaded successfully"
}
```

### 3. AI Chat API (Already Working)

**Endpoint:** `http://103.16.117.119:8080/v1/chat/completions`
**Status:** ✅ Working with llama-server

## 📱 Responsive Design

- ✅ Desktop: Full width, 70% max message width
- ✅ Tablet: 85% max message width
- ✅ Mobile: Compact padding, smaller icons

## 🐛 Troubleshooting

### Issue: Document list not showing

**Solution:** Replace dummy data with API call in `DocumentSelector.tsx`

### Issue: File upload not working

**Solution:** Implement `uploadFileToBackend()` in `aiService.ts`

### Issue: Drag & drop not responding

**Solution:** Check browser console for errors, ensure event handlers are working

### Issue: UI looks broken

**Solution:**

```bash
# Clear cache and rebuild
rm -rf dist node_modules/.cache
npm run build
```

## ✨ What's Different from Previous Version?

| Feature | Old Version | New Version |
|---------|------------|-------------|
| Header | Card with title | No header, gradient background |
| Clear button | Top right | Removed |
| Background | White card | Gradient purple with transparency |
| Document selector | N/A | Beautiful modal with table |
| File upload | N/A | Drag & drop + manual upload |
| Input area | Simple textarea | Auto-resize with action buttons |
| Messages | Card wrapped | Natural floating bubbles |

## 🎉 Ready to Use

After installation, you'll have a modern, ChatGPT-style AI interface with:

- 🎨 Beautiful gradient design
- 📎 Document selection from existing files
- 📤 Drag & drop file upload
- 💬 Natural conversation flow
- 🤖 Smart AI responses with context

---

**Version:** 2.0 (Revamped)
**Created:** January 23, 2026
**Author:** Claude AI Helper
**For:** DMS Frontend - AI Assistant Feature
