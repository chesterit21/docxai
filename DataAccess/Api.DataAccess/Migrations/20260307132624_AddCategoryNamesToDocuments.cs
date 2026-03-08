using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryNamesToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "TblDocuments",
                type: "varchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentTypeName",
                table: "TblDocuments",
                type: "varchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubCategoryName",
                table: "TblDocuments",
                type: "varchar(250)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "TblDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentTypeName",
                table: "TblDocuments");

            migrationBuilder.DropColumn(
                name: "SubCategoryName",
                table: "TblDocuments");
        }
    }
}
