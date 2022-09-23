using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PipelineService.Migrations
{
    public partial class StoreOperationExecutionError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "OperationExecutionRecords",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "OperationExecutionRecords");
        }
    }
}
