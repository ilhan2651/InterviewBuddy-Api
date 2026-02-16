using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buddy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioUrlAndUserIdToInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "InterviewSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "InterviewQuestions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSessions_UserId",
                table: "InterviewSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewSessions_Users_UserId",
                table: "InterviewSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewSessions_Users_UserId",
                table: "InterviewSessions");

            migrationBuilder.DropIndex(
                name: "IX_InterviewSessions_UserId",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "InterviewQuestions");
        }
    }
}
