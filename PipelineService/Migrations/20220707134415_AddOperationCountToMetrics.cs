using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipelineService.Migrations
{
    public partial class AddOperationCountToMetrics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Aborted",
                table: "CandidateProcessingMetrics",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OperationCount",
                table: "CandidateProcessingMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aborted",
                table: "CandidateProcessingMetrics");

            migrationBuilder.DropColumn(
                name: "OperationCount",
                table: "CandidateProcessingMetrics");
        }
    }
}
