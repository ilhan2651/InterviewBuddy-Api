using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buddy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class adding_language_to_interview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "InterviewSessions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "InterviewSessions");
        }
    }
}
