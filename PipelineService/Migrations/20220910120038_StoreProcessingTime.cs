using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipelineService.Migrations
{
    public partial class StoreProcessingTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ProcessingDurationP",
                table: "CandidateProcessingMetrics",
                type: "double",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingDurationP",
                table: "CandidateProcessingMetrics");
        }
    }
}
