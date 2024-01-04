using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipelineService.Migrations
{
    /// <inheritdoc />
    public partial class TrackCandiateModelCountsEtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpertPolicyModelName",
                table: "CandidateProcessingMetrics",
                type: "longtext",
                nullable: true,
                defaultValue: "composite")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<float>(
                name: "ExpertPolicyProbability",
                table: "CandidateProcessingMetrics",
                type: "float",
                nullable: false,
                defaultValue: 0.75f);

            migrationBuilder.AddColumn<int>(
                name: "MaxActionsPerPipeline",
                table: "CandidateProcessingMetrics",
                type: "int",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddColumn<int>(
                name: "MctsIterationLimit",
                table: "CandidateProcessingMetrics",
                type: "int",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddColumn<float>(
                name: "SleepTimeAfterNewActions",
                table: "CandidateProcessingMetrics",
                type: "float",
                nullable: false,
                defaultValue: 1.0f);

            migrationBuilder.AddColumn<int>(
                name: "TargetActionCount",
                table: "CandidateProcessingMetrics",
                type: "int",
                nullable: false,
                defaultValue: 13);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpertPolicyModelName",
                table: "CandidateProcessingMetrics");

            migrationBuilder.DropColumn(
                name: "ExpertPolicyProbability",
                table: "CandidateProcessingMetrics");

            migrationBuilder.DropColumn(
                name: "MaxActionsPerPipeline",
                table: "CandidateProcessingMetrics");

            migrationBuilder.DropColumn(
                name: "MctsIterationLimit",
                table: "CandidateProcessingMetrics");

            migrationBuilder.DropColumn(
                name: "SleepTimeAfterNewActions",
                table: "CandidateProcessingMetrics");

            migrationBuilder.DropColumn(
                name: "TargetActionCount",
                table: "CandidateProcessingMetrics");
        }
    }
}
