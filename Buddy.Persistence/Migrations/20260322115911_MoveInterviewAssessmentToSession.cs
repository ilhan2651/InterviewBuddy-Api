using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buddy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveInterviewAssessmentToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommunicationFeedback",
                table: "InterviewSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CommunicationScore",
                table: "InterviewSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfidenceFeedback",
                table: "InterviewSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfidenceScore",
                table: "InterviewSessions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommunicationFeedback",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "CommunicationScore",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "ConfidenceFeedback",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "InterviewSessions");
        }
    }
}
