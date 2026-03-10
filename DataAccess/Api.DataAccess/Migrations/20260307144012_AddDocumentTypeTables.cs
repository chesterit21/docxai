using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTypeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS ""TmDocumentType"" (
    ""Id""              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ""CategoryName""      VARCHAR(250) NOT NULL,
    ""SubCategoryName""      VARCHAR(250) NOT NULL,
    ""DocumentType""      VARCHAR(250) NOT NULL
);

CREATE INDEX ""IX_TmDocumentType_Category"" ON public.""TmDocumentType"" USING btree (""CategoryName"");
CREATE INDEX ""IX_TmDocumentType_SubCategory"" ON public.""TmDocumentType"" USING btree (""SubCategoryName"");
CREATE INDEX ""IX_TmDocumentType_DocType"" ON public.""TmDocumentType"" USING btree (""DocumentType"");

CREATE TABLE IF NOT EXISTS ""TmDocumentTypeAttributes"" (
    ""Id""              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ""DocumentTypeId""      UUID NOT NULL,
    ""AttributeName""      VARCHAR(250) NOT NULL,
    ""DataType""      VARCHAR(250) NOT NULL,
    CONSTRAINT FK_TmDocumentTypeAttributes_TmDocumentType_DocumentTypeId FOREIGN key(""DocumentTypeId"") REFERENCES ""TmDocumentType""(""Id"")
);

CREATE INDEX ""IX_TmDocumentTypeAttributes_typeId"" ON public.""TmDocumentTypeAttributes"" USING btree (""DocumentTypeId"");
CREATE INDEX ""IX_TmDocumentTypeAttributes_AttributeName"" ON public.""TmDocumentTypeAttributes"" USING btree (""AttributeName"");
CREATE INDEX ""IX_TmDocumentTypeAttributes_DataType"" ON public.""TmDocumentTypeAttributes"" USING btree (""DataType"");

CREATE TABLE IF NOT EXISTS ""TmAttributeSynonyms"" (
    ""Id""              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ""AttributeName""      VARCHAR(250) NOT NULL,
    ""Synonym""      VARCHAR(250) NOT NULL,
    ""Language""      VARCHAR(5) NOT NULL
);

CREATE INDEX ""IX_TmAttributeSynonyms_AttributeName"" ON public.""TmAttributeSynonyms"" USING btree (""AttributeName"");
CREATE INDEX ""IX_TmAttributeSynonyms_Synonym"" ON public.""TmAttributeSynonyms"" USING btree (""Synonym"");
CREATE INDEX ""IX_TmAttributeSynonyms_Language"" ON public.""TmAttributeSynonyms"" USING btree (""Language"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS ""TmAttributeSynonyms"";
DROP TABLE IF EXISTS ""TmDocumentTypeAttributes"";
DROP TABLE IF EXISTS ""TmDocumentType"";
            ");
        }
    }
}
