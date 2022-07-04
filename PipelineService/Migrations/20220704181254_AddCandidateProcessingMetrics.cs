using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipelineService.Migrations
{
    public partial class AddCandidateProcessingMetrics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateProcessingMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Error = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CandidateCreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessingStartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ImportStartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ImportEndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessingEndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ActionCount = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<long>(type: "bigint", maxLength: 256, nullable: false),
                    BatchNumber = table.Column<int>(type: "int", nullable: false),
                    PipelineId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ImportSuccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateProcessingMetrics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateProcessingMetrics");
        }
    }
}
