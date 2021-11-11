using Microsoft.EntityFrameworkCore.Migrations;

namespace SPNewsData.Data.Migrations
{
    public partial class parsingLayout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ParsingLayout",
                table: "UrlExtracteds",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParsingLayout",
                table: "UrlExtracteds");
        }
    }
}
