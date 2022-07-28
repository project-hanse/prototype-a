using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipelineService.Migrations
{
    public partial class RecordExecutionAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecutionAttempts",
                table: "CandidateProcessingMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OperationsRandomizedCount",
                table: "CandidateProcessingMetrics",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionAttempts",
                table: "CandidateProcessingMetrics");

            migrationBuilder.DropColumn(
                name: "OperationsRandomizedCount",
                table: "CandidateProcessingMetrics");
        }
    }
}
