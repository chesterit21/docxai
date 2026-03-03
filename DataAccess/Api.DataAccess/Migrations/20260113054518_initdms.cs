using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace Api.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class initdms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "TblAttributeCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    CollectionName = table.Column<string>(type: "varchar(150)", nullable: false),
                    CollectionDescription = table.Column<string>(type: "varchar(500)", nullable: false),
                    AttributeElementCollection = table.Column<string>(type: "json", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblAttributeCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttributeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    AttributeElement = table.Column<string>(type: "json", nullable: true),
                    AttributeType = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblDocumentFavorite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    Owner = table.Column<int>(type: "integer", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocumentFavorite", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblDocumentReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentID = table.Column<int>(type: "integer", nullable: false),
                    ReminderDateTime = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ReminderDesc = table.Column<string>(type: "text", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocumentReminders", x => x.Id);
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblHistoryTrEmail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblLogActivityLogin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    IPAddress = table.Column<string>(type: "varchar(39)", nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(200)", nullable: true),
                    LoginTime = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ActorUserId = table.Column<int>(type: "integer", nullable: false),
                    UserAction = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblLogActivityLogin", x => x.Id);
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblLogApplication", x => x.Id);
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsCompany", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "TblMsGroup",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupName = table.Column<string>(type: "varchar(150)", nullable: false),
                    GroupDescription = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsGroup", x => x.GroupId);
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
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
                    Description = table.Column<string>(type: "varchar(100)", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsRole", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "TblMsUserResetPasswordVerificationCodes",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VerificationCode = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsUserResetPasswordVerificationCodes", x => x.UserId);
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
                    StatusMessage = table.Column<string>(type: "varchar(1000)", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
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
                    CompanyId = table.Column<string>(type: "varchar(15)", nullable: true),
                    FullName = table.Column<string>(type: "varchar(150)", nullable: false),
                    EmailAddress = table.Column<string>(type: "varchar(150)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(18)", nullable: false),
                    UserPassword = table.Column<string>(type: "text", nullable: true),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsADUser = table.Column<bool>(type: "boolean", nullable: false),
                    IsLogin = table.Column<bool>(type: "boolean", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp", nullable: true),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    UserType = table.Column<string>(type: "text", nullable: false),
                    UserStatus = table.Column<int>(type: "integer", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsUser", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_TblMsUser_TblMsCompany_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "TblMsCompany",
                        principalColumn: "CompanyId");
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
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
                name: "TblCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "varchar(100)", nullable: false),
                    CategoryDesc = table.Column<string>(type: "text", nullable: true),
                    Owner = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblCategories_TblCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TblCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TblCategories_TblMsUser_Owner",
                        column: x => x.Owner,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblLogAuditTrail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblLogAuditTrail_TblLogTransaction_TransactionLogId",
                        column: x => x.TransactionLogId,
                        principalTable: "TblLogTransaction",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TblLogAuditTrail_TblMsUser_InsertedBy",
                        column: x => x.InsertedBy,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblMsUserCompany",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<string>(type: "varchar(15)", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
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
                name: "TblMsUserGroups",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsUserGroups", x => new { x.UserId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_TblMsUserGroups_TblMsGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "TblMsGroup",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblMsUserGroups_TblMsUser_UserId",
                        column: x => x.UserId,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
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
                name: "TblMsUserMedia",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PhotoName = table.Column<string>(type: "text", nullable: true),
                    PhotoImage = table.Column<byte[]>(type: "bytea", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblMsUserMedia", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_TblMsUserMedia_TblMsUser_UserId",
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
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "TblWatermarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "varchar(150)", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblWatermarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblWatermarks_TblMsUser_InsertedBy",
                        column: x => x.InsertedBy,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblWatermarks_TblMsUser_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TblCategoriesFavorite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    CategoryID = table.Column<int>(type: "integer", nullable: false),
                    Owner = table.Column<int>(type: "integer", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCategoriesFavorite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblCategoriesFavorite_TblCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "TblCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblCategoriesShared",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryID = table.Column<int>(type: "integer", nullable: true),
                    ShareType = table.Column<string>(type: "varchar(5)", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: true),
                    GroupID = table.Column<int>(type: "integer", nullable: true),
                    IsView = table.Column<bool>(type: "boolean", nullable: false),
                    IsEdit = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCategoriesShared", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblCategoriesShared_TblCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "TblCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TblCategoriesShared_TblMsGroup_GroupID",
                        column: x => x.GroupID,
                        principalTable: "TblMsGroup",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_TblCategoriesShared_TblMsUser_UserID",
                        column: x => x.UserID,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TblDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryID = table.Column<int>(type: "integer", nullable: false),
                    DocumentTitle = table.Column<string>(type: "varchar(250)", nullable: false),
                    DocumentDesc = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Owner = table.Column<int>(type: "integer", nullable: false),
                    FileSize = table.Column<int>(type: "integer", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ReminderDays = table.Column<short>(type: "smallint", nullable: true),
                    ReminderDateTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    WatermarkID = table.Column<int>(type: "integer", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblDocuments_TblCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "TblCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblDocuments_TblMsUser_Owner",
                        column: x => x.Owner,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblDocuments_TblWatermarks_WatermarkID",
                        column: x => x.WatermarkID,
                        principalTable: "TblWatermarks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TblApproval",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryID = table.Column<int>(type: "integer", nullable: true),
                    DocumentID = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "varchar(2000)", nullable: false),
                    MaxStep = table.Column<int>(type: "integer", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: true),
                    CurrentApproverUserID = table.Column<int>(type: "integer", nullable: true),
                    NextApproverUserID = table.Column<int>(type: "integer", nullable: true),
                    LastActivityDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    LastRemark = table.Column<string>(type: "varchar(2000)", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblApproval_TblCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "TblCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TblApproval_TblDocuments_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "TblDocuments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TblApproval_TblMsUser_CurrentApproverUserID",
                        column: x => x.CurrentApproverUserID,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_TblApproval_TblMsUser_InsertedBy",
                        column: x => x.InsertedBy,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblApproval_TblMsUser_NextApproverUserID",
                        column: x => x.NextApproverUserID,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TblDocumentAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentID = table.Column<int>(type: "integer", nullable: false),
                    AttributeValues = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocumentAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblDocumentAttributes_TblDocuments_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "TblDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblDocumentFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentID = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "varchar(15)", nullable: false),
                    DocumentFileName = table.Column<string>(type: "varchar(150)", nullable: false),
                    NewDocumentFileName = table.Column<string>(type: "varchar(150)", nullable: true),
                    DocumentFileSize = table.Column<int>(type: "integer", nullable: false),
                    DocumentFileContent = table.Column<string>(type: "text", nullable: true),
                    DocumentFilePath = table.Column<string>(type: "varchar(1000)", nullable: false),
                    IsMainDocumentFile = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentSummary = table.Column<string>(type: "text", nullable: true),
                    Attributtes = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "DocumentFileContent" }),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocumentFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblDocumentFiles_TblDocuments_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "TblDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblDocumentFiles_TblMsUser_InsertedBy",
                        column: x => x.InsertedBy,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblDocumentFiles_TblMsUser_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TblDocumentItemList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentID = table.Column<int>(type: "integer", nullable: false),
                    ItemUser = table.Column<int>(type: "integer", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocumentItemList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblDocumentItemList_TblDocuments_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "TblDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblDocumentItemList_TblMsUser_ItemUser",
                        column: x => x.ItemUser,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblDocumentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    LogAuditTrailID = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentID = table.Column<int>(type: "integer", nullable: false),
                    ActionLogDocument = table.Column<string>(type: "varchar(150)", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocumentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblDocumentLogs_TblDocuments_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "TblDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblDocumentLogs_TblLogAuditTrail_LogAuditTrailID",
                        column: x => x.LogAuditTrailID,
                        principalTable: "TblLogAuditTrail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TblDocumentRelated",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentID = table.Column<int>(type: "integer", nullable: false),
                    RelatedDocumentID = table.Column<int>(type: "integer", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocumentRelated", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblDocumentRelated_TblDocuments_RelatedDocumentID",
                        column: x => x.RelatedDocumentID,
                        principalTable: "TblDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblDocumentShared",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentID = table.Column<int>(type: "integer", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocumentShared", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblDocumentShared_TblDocuments_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "TblDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    DocumentID = table.Column<int>(type: "integer", nullable: true),
                    NotificationType = table.Column<short>(type: "smallint", nullable: false),
                    NotifDescription = table.Column<string>(type: "varchar(255)", nullable: true),
                    NotifAction = table.Column<string>(type: "varchar(50)", nullable: true),
                    TargetActor = table.Column<int>(type: "integer", nullable: false),
                    NotifContent = table.Column<string>(type: "text", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblNotification_TblDocuments_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "TblDocuments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TblApprovalActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovalID = table.Column<int>(type: "integer", nullable: false),
                    ApprovalActivity = table.Column<int>(type: "integer", nullable: true),
                    ApprovalActivityName = table.Column<string>(type: "varchar(50)", nullable: true),
                    Remark = table.Column<string>(type: "varchar(2000)", nullable: true),
                    Reason = table.Column<string>(type: "varchar(2000)", nullable: true),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    NextStep = table.Column<int>(type: "integer", nullable: true),
                    RelatedDocumentID = table.Column<int>(type: "integer", nullable: true),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblApprovalActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblApprovalActivities_TblApproval_ApprovalID",
                        column: x => x.ApprovalID,
                        principalTable: "TblApproval",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblApprovalActivities_TblDocuments_RelatedDocumentID",
                        column: x => x.RelatedDocumentID,
                        principalTable: "TblDocuments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TblApprovalActivities_TblMsUser_InsertedBy",
                        column: x => x.InsertedBy,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblApprovalFlows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovalID = table.Column<int>(type: "integer", nullable: false),
                    Step = table.Column<int>(type: "integer", nullable: false),
                    IsFinalStep = table.Column<bool>(type: "boolean", nullable: false),
                    ApproverUserID = table.Column<int>(type: "integer", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblApprovalFlows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblApprovalFlows_TblApproval_ApprovalID",
                        column: x => x.ApprovalID,
                        principalTable: "TblApproval",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblApprovalFlows_TblMsUser_ApproverUserID",
                        column: x => x.ApproverUserID,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblDocumentSharedPrivillege",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentSharedID = table.Column<int>(type: "integer", nullable: false),
                    ShareType = table.Column<string>(type: "varchar(5)", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: true),
                    GroupID = table.Column<int>(type: "integer", nullable: true),
                    IsView = table.Column<bool>(type: "boolean", nullable: false),
                    IsEdit = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    InsertedBy = table.Column<int>(type: "integer", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblDocumentSharedPrivillege", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblDocumentSharedPrivillege_TblDocumentShared_DocumentShare~",
                        column: x => x.DocumentSharedID,
                        principalTable: "TblDocumentShared",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblDocumentSharedPrivillege_TblMsGroup_GroupID",
                        column: x => x.GroupID,
                        principalTable: "TblMsGroup",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_TblDocumentSharedPrivillege_TblMsUser_UserID",
                        column: x => x.UserID,
                        principalTable: "TblMsUser",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblApproval_CategoryID",
                table: "TblApproval",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_TblApproval_CurrentApproverUserID",
                table: "TblApproval",
                column: "CurrentApproverUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TblApproval_DocumentID",
                table: "TblApproval",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TblApproval_InsertedBy",
                table: "TblApproval",
                column: "InsertedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TblApproval_NextApproverUserID",
                table: "TblApproval",
                column: "NextApproverUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TblApprovalActivities_ApprovalID",
                table: "TblApprovalActivities",
                column: "ApprovalID");

            migrationBuilder.CreateIndex(
                name: "IX_TblApprovalActivities_InsertedBy",
                table: "TblApprovalActivities",
                column: "InsertedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TblApprovalActivities_RelatedDocumentID",
                table: "TblApprovalActivities",
                column: "RelatedDocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TblApprovalFlows_ApprovalID",
                table: "TblApprovalFlows",
                column: "ApprovalID");

            migrationBuilder.CreateIndex(
                name: "IX_TblApprovalFlows_ApproverUserID",
                table: "TblApprovalFlows",
                column: "ApproverUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TblCategories_Owner",
                table: "TblCategories",
                column: "Owner");

            migrationBuilder.CreateIndex(
                name: "IX_TblCategories_ParentId",
                table: "TblCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCategoriesFavorite_CategoryID",
                table: "TblCategoriesFavorite",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_TblCategoriesShared_CategoryID",
                table: "TblCategoriesShared",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_TblCategoriesShared_GroupID",
                table: "TblCategoriesShared",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_TblCategoriesShared_UserID",
                table: "TblCategoriesShared",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentAttributes_DocumentID",
                table: "TblDocumentAttributes",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentFiles_DocumentID",
                table: "TblDocumentFiles",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentFiles_InsertedBy",
                table: "TblDocumentFiles",
                column: "InsertedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentFiles_SearchVector",
                table: "TblDocumentFiles",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentFiles_UpdatedBy",
                table: "TblDocumentFiles",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentItemList_DocumentID",
                table: "TblDocumentItemList",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentItemList_ItemUser",
                table: "TblDocumentItemList",
                column: "ItemUser");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentLogs_DocumentID",
                table: "TblDocumentLogs",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentLogs_LogAuditTrailID",
                table: "TblDocumentLogs",
                column: "LogAuditTrailID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentRelated_RelatedDocumentID",
                table: "TblDocumentRelated",
                column: "RelatedDocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocuments_CategoryID",
                table: "TblDocuments",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocuments_Owner",
                table: "TblDocuments",
                column: "Owner");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocuments_WatermarkID",
                table: "TblDocuments",
                column: "WatermarkID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentShared_DocumentID",
                table: "TblDocumentShared",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentSharedPrivillege_DocumentSharedID",
                table: "TblDocumentSharedPrivillege",
                column: "DocumentSharedID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentSharedPrivillege_GroupID",
                table: "TblDocumentSharedPrivillege",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_TblDocumentSharedPrivillege_UserID",
                table: "TblDocumentSharedPrivillege",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TblLogAuditTrail_InsertedBy",
                table: "TblLogAuditTrail",
                column: "InsertedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TblLogAuditTrail_TransactionLogId",
                table: "TblLogAuditTrail",
                column: "TransactionLogId",
                unique: true);

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
                name: "IX_TblMsUserGroups_GroupId",
                table: "TblMsUserGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TblMsUserMatrix_MenuId",
                table: "TblMsUserMatrix",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_TblMsUserRoles_RoleId",
                table: "TblMsUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TblNotification_DocumentID",
                table: "TblNotification",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TblWatermarks_InsertedBy",
                table: "TblWatermarks",
                column: "InsertedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TblWatermarks_UpdatedBy",
                table: "TblWatermarks",
                column: "UpdatedBy");

			// 1. Membuat Fungsi dengan Dynamic SQL (EXECUTE) 
			// Ini mencegah error "relation old_table does not exist"
			migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_sync_tbl_document_stats_bulk()
                RETURNS TRIGGER AS $$
                DECLARE
                    affected_ids_query TEXT;
                BEGIN
                    -- Buat tabel temporary untuk menampung ID Document yang terdampak
                    CREATE TEMP TABLE IF NOT EXISTS affected_doc_ids (doc_id INT) ON COMMIT DROP;
                    TRUNCATE TABLE affected_doc_ids;

                    -- Gunakan EXECUTE agar Postgres tidak validasi tabel transisi saat pembuatan fungsi
                    IF (TG_OP = 'INSERT') THEN
                        EXECUTE 'INSERT INTO affected_doc_ids SELECT ""DocumentID"" FROM new_table WHERE ""DocumentID"" IS NOT NULL';
                    ELSIF (TG_OP = 'DELETE') THEN
                        EXECUTE 'INSERT INTO affected_doc_ids SELECT ""DocumentID"" FROM old_table WHERE ""DocumentID"" IS NOT NULL';
                    ELSIF (TG_OP = 'UPDATE') THEN
                        EXECUTE 'INSERT INTO affected_doc_ids 
                                 SELECT ""DocumentID"" FROM old_table WHERE ""DocumentID"" IS NOT NULL
                                 UNION 
                                 SELECT ""DocumentID"" FROM new_table WHERE ""DocumentID"" IS NOT NULL';
                    END IF;

                    -- Update TblDocuments: Hitung ulang FileSize dan update UpdatedAt
                    UPDATE ""TblDocuments""
                    SET 
                        ""FileSize"" = (SELECT COALESCE(SUM(df.""DocumentFileSize""), 0) 
                                      FROM ""TblDocumentFiles"" df 
                                      WHERE df.""DocumentID"" = ""TblDocuments"".""Id""),
                        ""UpdatedAt"" = LOCALTIMESTAMP
                    WHERE ""Id"" IN (SELECT doc_id FROM affected_doc_ids);

                    -- Update TblCategories: Update UpdatedAt kategori terkait
                    UPDATE ""TblCategories""
                    SET ""UpdatedAt"" = LOCALTIMESTAMP
                    WHERE ""Id"" IN (
                        SELECT ""CategoryID"" FROM ""TblDocuments"" 
                        WHERE ""Id"" IN (SELECT doc_id FROM affected_doc_ids)
                    );

                    RETURN NULL;
                END;
                $$ LANGUAGE plpgsql;
            ");

			            // 2. Membuat 3 Trigger Terpisah (Wajib di Postgres untuk klausa REFERENCING)
		 migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS tr_tbl_doc_files_insert ON ""TblDocumentFiles"";
                CREATE TRIGGER tr_tbl_doc_files_insert
                AFTER INSERT ON ""TblDocumentFiles""
                REFERENCING NEW TABLE AS new_table
                FOR EACH STATEMENT EXECUTE FUNCTION fn_sync_tbl_document_stats_bulk();

                DROP TRIGGER IF EXISTS tr_tbl_doc_files_update ON ""TblDocumentFiles"";
                CREATE TRIGGER tr_tbl_doc_files_update
                AFTER UPDATE ON ""TblDocumentFiles""
                REFERENCING OLD TABLE AS old_table NEW TABLE AS new_table
                FOR EACH STATEMENT EXECUTE FUNCTION fn_sync_tbl_document_stats_bulk();

                DROP TRIGGER IF EXISTS tr_tbl_doc_files_delete ON ""TblDocumentFiles"";
                CREATE TRIGGER tr_tbl_doc_files_delete
                AFTER DELETE ON ""TblDocumentFiles""
                REFERENCING OLD TABLE AS old_table
                FOR EACH STATEMENT EXECUTE FUNCTION fn_sync_tbl_document_stats_bulk();
            ");

		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblApprovalActivities");

            migrationBuilder.DropTable(
                name: "TblApprovalFlows");

            migrationBuilder.DropTable(
                name: "TblAttributeCollections");

            migrationBuilder.DropTable(
                name: "TblAttributes");

            migrationBuilder.DropTable(
                name: "TblCategoriesFavorite");

            migrationBuilder.DropTable(
                name: "TblCategoriesShared");

            migrationBuilder.DropTable(
                name: "TblDocumentAttributes");

            migrationBuilder.DropTable(
                name: "TblDocumentFavorite");

            migrationBuilder.DropTable(
                name: "TblDocumentFiles");

            migrationBuilder.DropTable(
                name: "TblDocumentItemList");

            migrationBuilder.DropTable(
                name: "TblDocumentLogs");

            migrationBuilder.DropTable(
                name: "TblDocumentRelated");

            migrationBuilder.DropTable(
                name: "TblDocumentReminders");

            migrationBuilder.DropTable(
                name: "TblDocumentSharedPrivillege");

            migrationBuilder.DropTable(
                name: "TblHistoryLogAuditTrail");

            migrationBuilder.DropTable(
                name: "TblHistoryLogTransaction");

            migrationBuilder.DropTable(
                name: "TblHistoryTrEmail");

            migrationBuilder.DropTable(
                name: "TblLogActivityLogin");

            migrationBuilder.DropTable(
                name: "TblLogApplication");

            migrationBuilder.DropTable(
                name: "TblMsLanguage");

            migrationBuilder.DropTable(
                name: "TblMsRoleMatrix");

            migrationBuilder.DropTable(
                name: "TblMsUserCompany");

            migrationBuilder.DropTable(
                name: "TblMsUserGroups");

            migrationBuilder.DropTable(
                name: "TblMsUserMatrix");

            migrationBuilder.DropTable(
                name: "TblMsUserMedia");

            migrationBuilder.DropTable(
                name: "TblMsUserResetPasswordVerificationCodes");

            migrationBuilder.DropTable(
                name: "TblMsUserRoles");

            migrationBuilder.DropTable(
                name: "TblNotification");

            migrationBuilder.DropTable(
                name: "TblTrEmail");

            migrationBuilder.DropTable(
                name: "TblApproval");

            migrationBuilder.DropTable(
                name: "TblLogAuditTrail");

            migrationBuilder.DropTable(
                name: "TblDocumentShared");

            migrationBuilder.DropTable(
                name: "TblMsGroup");

            migrationBuilder.DropTable(
                name: "TblMsMenu");

            migrationBuilder.DropTable(
                name: "TblMsRole");

            migrationBuilder.DropTable(
                name: "TblLogTransaction");

            migrationBuilder.DropTable(
                name: "TblDocuments");

            migrationBuilder.DropTable(
                name: "TblCategories");

            migrationBuilder.DropTable(
                name: "TblWatermarks");

            migrationBuilder.DropTable(
                name: "TblMsUser");

            migrationBuilder.DropTable(
                name: "TblMsCompany");

			migrationBuilder.Sql("DROP TRIGGER IF EXISTS tr_tbl_doc_files_insert ON \"TblDocumentFiles\";");
			migrationBuilder.Sql("DROP TRIGGER IF EXISTS tr_tbl_doc_files_update ON \"TblDocumentFiles\";");
			migrationBuilder.Sql("DROP TRIGGER IF EXISTS tr_tbl_doc_files_delete ON \"TblDocumentFiles\";");

			// Menghapus Fungsi
			migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_sync_tbl_document_stats_bulk();");
		}
    }
}
