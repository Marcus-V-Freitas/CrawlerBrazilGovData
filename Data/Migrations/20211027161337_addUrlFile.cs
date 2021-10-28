using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class addUrlFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlFile",
                table: "DataSourceAditionalInformations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlFile",
                table: "DataSourceAditionalInformations");
        }
    }
}