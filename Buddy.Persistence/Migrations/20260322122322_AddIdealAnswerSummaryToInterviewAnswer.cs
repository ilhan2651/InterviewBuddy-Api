using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buddy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdealAnswerSummaryToInterviewAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdealAnswerSummary",
                table: "InterviewAnswers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdealAnswerSummary",
                table: "InterviewAnswers");
        }
    }
}
