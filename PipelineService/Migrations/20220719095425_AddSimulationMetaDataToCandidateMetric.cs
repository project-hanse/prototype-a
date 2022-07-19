using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipelineService.Migrations
{
    public partial class AddSimulationMetaDataToCandidateMetric : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RewardFunctionType",
                table: "CandidateProcessingMetrics",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "SimulationEndTime",
                table: "CandidateProcessingMetrics",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "SimulationStartTime",
                table: "CandidateProcessingMetrics",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RewardFunctionType",
                table: "CandidateProcessingMetrics");

            migrationBuilder.DropColumn(
                name: "SimulationEndTime",
                table: "CandidateProcessingMetrics");

            migrationBuilder.DropColumn(
                name: "SimulationStartTime",
                table: "CandidateProcessingMetrics");
        }
    }
}
