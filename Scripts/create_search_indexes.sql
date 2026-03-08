-- =============================================================
-- SQL Script: Index Baru untuk Optimasi Search
-- Jalankan di PostgreSQL database "dms"
-- =============================================================

-- 1. Aktifkan extension pg_trgm (trigram) jika belum ada
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- 2. Trigram index untuk ILIKE search pada DocumentTitle
CREATE INDEX CONCURRENTLY IF NOT EXISTS "IX_TblDocuments_DocumentTitle_trgm" 
ON public."TblDocuments" USING gin ("DocumentTitle" gin_trgm_ops);

-- 3. Trigram index untuk ILIKE search pada DocumentDesc
CREATE INDEX CONCURRENTLY IF NOT EXISTS "IX_TblDocuments_DocumentDesc_trgm" 
ON public."TblDocuments" USING gin ("DocumentDesc" gin_trgm_ops);

-- 4. Trigram index untuk ILIKE search pada DocumentSummary (partial — hanya yang NOT NULL)
CREATE INDEX CONCURRENTLY IF NOT EXISTS "IX_TblDocumentFiles_DocumentSummary_trgm" 
ON public."TblDocumentFiles" USING gin ("DocumentSummary" gin_trgm_ops)
WHERE "DocumentSummary" IS NOT NULL;

-- 5. Trigram index untuk ILIKE search pada AttributeValues
CREATE INDEX CONCURRENTLY IF NOT EXISTS "IX_TblDocumentAttributes_AttributeValues_trgm" 
ON public."TblDocumentAttributes" USING gin ("AttributeValues" gin_trgm_ops)
WHERE "AttributeValues" IS NOT NULL;

-- 6. Composite index untuk access control filter
CREATE INDEX CONCURRENTLY IF NOT EXISTS "IX_TblDocuments_IsActive_Owner" 
ON public."TblDocuments" ("IsActive", "Owner") 
WHERE "IsActive" = true;

-- NOTE: Index "IX_TblDocumentFiles_SearchVector" (GIN) sudah ada dari migrasi awal.
-- Tidak perlu dibuat ulang.
