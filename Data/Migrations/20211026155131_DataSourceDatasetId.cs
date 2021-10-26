using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class DataSourceDatasetId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DatasetId",
                table: "DataSources",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSources_DatasetId",
                table: "DataSources",
                column: "DatasetId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSources_Datasets_DatasetId",
                table: "DataSources",
                column: "DatasetId",
                principalTable: "Datasets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataSources_Datasets_DatasetId",
                table: "DataSources");

            migrationBuilder.DropIndex(
                name: "IX_DataSources_DatasetId",
                table: "DataSources");

            migrationBuilder.DropColumn(
                name: "DatasetId",
                table: "DataSources");
        }
    }
}
