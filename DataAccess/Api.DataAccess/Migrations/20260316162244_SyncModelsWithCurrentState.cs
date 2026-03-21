using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelsWithCurrentState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentDesc",
                table: "TblDocuments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AgentPollingTaskDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    TaskCode = table.Column<string>(type: "varchar(100)", nullable: false),
                    Status = table.Column<string>(type: "varchar(100)", nullable: false),
                    FullPath = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentPollingTaskDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentPollingTaskDocument_TblDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "TblDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    ModelName = table.Column<string>(type: "varchar(200)", nullable: false),
                    UrlApi = table.Column<string>(type: "varchar(500)", nullable: false),
                    ApiKey = table.Column<string>(type: "varchar(500)", nullable: false),
                    Provider = table.Column<string>(type: "varchar(50)", nullable: false),
                    MaxToken = table.Column<int>(type: "integer", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentExtractedEntities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    AttributeName = table.Column<string>(type: "text", nullable: false),
                    ValueText = table.Column<string>(type: "text", nullable: true),
                    ValueNumber = table.Column<int>(type: "integer", nullable: true),
                    ValueDecimal = table.Column<decimal>(type: "numeric", nullable: true),
                    ValueBoolean = table.Column<bool>(type: "boolean", nullable: true),
                    ValueDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentExtractedEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentExtractedEntities_TblDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "TblDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentPollingTaskDocument_DocumentId",
                schema: "public",
                table: "AgentPollingTaskDocument",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentExtractedEntities_DocumentId",
                schema: "public",
                table: "DocumentExtractedEntities",
                column: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentPollingTaskDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AiModel");

            migrationBuilder.DropTable(
                name: "DocumentExtractedEntities",
                schema: "public");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentDesc",
                table: "TblDocuments",
                type: "varchar(1000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
