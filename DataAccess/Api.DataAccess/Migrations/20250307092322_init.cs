using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "varchar(100)", nullable: false),
                    CategoryDesc = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TblHistoryLogAuditTrail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    TransactionLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    TableName = table.Column<string>(type: "varchar(50)", nullable: true),
                    Command = table.Column<string>(type: "varchar(50)", nullable: true),
                    Before = table.Column<string>(type: "text", nullable: true),
                    After = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblHistoryLogAuditTrail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblHistoryLogTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    IPAddress = table.Column<string>(type: "varchar(39)", nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(200)", nullable: true),
                    Action = table.Column<string>(type: "varchar(50)", nullable: true),
                    Path = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblHistoryLogTransaction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblHistoryTrEmail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Subject = table.Column<string>(type: "varchar(100)", nullable: false),
                    To = table.Column<string>(type: "text", nullable: false),
                    Cc = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: false),
                    IsHtml = table.Column<bool>(type: "boolean", nullable: false),
                    SentStatus = table.Column<short>(type: "smallint", nullable: false),
                    StatusMessage = table.Column<string>(type: "varchar(100)", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblHistoryTrEmail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblLogApplication",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    IPAddress = table.Column<string>(type: "varchar(39)", nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(200)", nullable: true),
                    Type = table.Column<string>(type: "varchar(30)", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    Endpoint = table.Column<string>(type: "text", nullable: true),
                    Parameter = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblLogApplication", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblLogAuditTrail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    TransactionLogId = table.Column<Guid>(type: "uuid", nullable: true),
                    TableName = table.Column<string>(type: "varchar(50)", nullable: true),
                    Command = table.Column<string>(type: "varchar(50)", nullable: true),
                    Before = table.Column<string>(type: "text", nullable: true),
                    After = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblLogAuditTrail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblLogTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    IPAddress = table.Column<string>(type: "varchar(39)", nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(200)", nullable: true),
                    Action = table.Column<string>(type: "varchar(50)", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Path = table.Column<string>(type: "text", nullable: true),
                    Parameter = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblLogTransaction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblMsCompany",
                columns: table => new
                {
                    CompanyId = table.Column<string>(type: "varchar(15)", nullable: false),
                    ParentCompanyId = table.Column<string>(type: "varchar(15)", nullable: true),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Address = table.Column<string>(type: "varchar(100)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsCompany", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "TblMsLanguage",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(50)", nullable: false),
                    En = table.Column<string>(type: "varchar(500)", nullable: false),
                    Id = table.Column<string>(type: "varchar(500)", nullable: false),
                    Type = table.Column<string>(type: "varchar(10)", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsLanguage", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TblMsMenu",
                columns: table => new
                {
                    MenuId = table.Column<string>(type: "varchar(30)", nullable: false),
                    ParentMenuId = table.Column<string>(type: "varchar(30)", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", nullable: true),
                    Icon = table.Column<string>(type: "varchar(100)", nullable: true),
                    Url = table.Column<string>(type: "varchar(200)", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsMenu", x => x.MenuId);
                });

            migrationBuilder.CreateTable(
                name: "TblMsRole",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsRole", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "TblTrEmail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Subject = table.Column<string>(type: "varchar(100)", nullable: false),
                    To = table.Column<string>(type: "text", nullable: false),
                    Cc = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: false),
                    IsHtml = table.Column<bool>(type: "boolean", nullable: false),
                    SentStatus = table.Column<int>(type: "integer", nullable: false),
                    StatusMessage = table.Column<string>(type: "varchar(100)", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblTrEmail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblMsUser",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CompanyId = table.Column<string>(type: "varchar(15)", nullable: false),
                    FullName = table.Column<string>(type: "varchar(100)", nullable: false),
                    UserPassword = table.Column<string>(type: "varchar(64)", nullable: true),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsADUser = table.Column<bool>(type: "boolean", nullable: false),
                    IsLogin = table.Column<bool>(type: "boolean", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsUser", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_TblMsUser_TblMsCompany_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "TblMsCompany",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblMsRoleMatrix",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    MenuId = table.Column<string>(type: "varchar(30)", nullable: false),
                    IsInsert = table.Column<bool>(type: "boolean", nullable: false),
                    IsUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsRoleMatrix", x => new { x.RoleId, x.MenuId });
                    table.ForeignKey(
                        name: "FK_TblMsRoleMatrix_TblMsMenu_MenuId",
                        column: x => x.MenuId,
                        principalTable: "TblMsMenu",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblMsRoleMatrix_TblMsRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "TblMsRole",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblMsUserCompany",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<string>(type: "varchar(15)", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsUserCompany", x => new { x.UserId, x.CompanyId });
                    table.ForeignKey(
                        name: "FK_TblMsUserCompany_TblMsCompany_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "TblMsCompany",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_TblMsUserCompany_TblMsUser_UserId",
                        column: x => x.UserId,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TblMsUserMatrix",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MenuId = table.Column<string>(type: "varchar(30)", nullable: false),
                    IsInsert = table.Column<bool>(type: "boolean", nullable: false),
                    IsUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsUserMatrix", x => new { x.UserId, x.MenuId });
                    table.ForeignKey(
                        name: "FK_TblMsUserMatrix_TblMsMenu_MenuId",
                        column: x => x.MenuId,
                        principalTable: "TblMsMenu",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblMsUserMatrix_TblMsUser_UserId",
                        column: x => x.UserId,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblMsUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_TblMsUserRoles_TblMsRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "TblMsRole",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblMsUserRoles_TblMsUser_UserId",
                        column: x => x.UserId,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TblMsRoleMatrix_MenuId",
                table: "TblMsRoleMatrix",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_TblMsUser_CompanyId",
                table: "TblMsUser",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TblMsUserCompany_CompanyId",
                table: "TblMsUserCompany",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TblMsUserMatrix_MenuId",
                table: "TblMsUserMatrix",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_TblMsUserRoles_RoleId",
                table: "TblMsUserRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "TblHistoryLogAuditTrail");

            migrationBuilder.DropTable(
                name: "TblHistoryLogTransaction");

            migrationBuilder.DropTable(
                name: "TblHistoryTrEmail");

            migrationBuilder.DropTable(
                name: "TblLogApplication");

            migrationBuilder.DropTable(
                name: "TblLogAuditTrail");

            migrationBuilder.DropTable(
                name: "TblLogTransaction");

            migrationBuilder.DropTable(
                name: "TblMsLanguage");

            migrationBuilder.DropTable(
                name: "TblMsRoleMatrix");

            migrationBuilder.DropTable(
                name: "TblMsUserCompany");

            migrationBuilder.DropTable(
                name: "TblMsUserMatrix");

            migrationBuilder.DropTable(
                name: "TblMsUserRoles");

            migrationBuilder.DropTable(
                name: "TblTrEmail");

            migrationBuilder.DropTable(
                name: "TblMsMenu");

            migrationBuilder.DropTable(
                name: "TblMsRole");

            migrationBuilder.DropTable(
                name: "TblMsUser");

            migrationBuilder.DropTable(
                name: "TblMsCompany");
        }
    }
}
