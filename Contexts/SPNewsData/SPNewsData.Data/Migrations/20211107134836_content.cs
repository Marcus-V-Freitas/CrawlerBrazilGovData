using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SPNewsData.Data.Migrations
{
    public partial class content : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CaptureDate",
                table: "GovNews",
                type: "datetime(6)",
                nullable: true,
                defaultValueSql: "(CURRENT_TIMESTAMP)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValueSql: "getdate()");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "GovNews",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "GovNews");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CaptureDate",
                table: "GovNews",
                type: "datetime(6)",
                nullable: true,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValueSql: "(CURRENT_TIMESTAMP)");
        }
    }
}
