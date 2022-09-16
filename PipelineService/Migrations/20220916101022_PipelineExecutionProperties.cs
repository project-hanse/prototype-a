using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipelineService.Migrations
{
    public partial class PipelineExecutionProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowCachingResults",
                table: "PipelineExecutionRecords",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Strategy",
                table: "PipelineExecutionRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCachingResults",
                table: "PipelineExecutionRecords");

            migrationBuilder.DropColumn(
                name: "Strategy",
                table: "PipelineExecutionRecords");
        }
    }
}
