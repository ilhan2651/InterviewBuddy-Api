using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Buddy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionDifficultyAndUserApiKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "InterviewSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "InterviewSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodeSnippet",
                table: "InterviewQuestions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "InterviewQuestions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "InterviewQuestions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioFeedback",
                table: "InterviewAnswers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudioScore",
                table: "InterviewAnswers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoFeedback",
                table: "InterviewAnswers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VideoScore",
                table: "InterviewAnswers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SimliApiKey = table.Column<string>(type: "text", nullable: true),
                    ElevenLabsApiKey = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserApiKeys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewQuestions_ParentId",
                table: "InterviewQuestions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApiKeys_UserId",
                table: "UserApiKeys",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewQuestions_InterviewQuestions_ParentId",
                table: "InterviewQuestions",
                column: "ParentId",
                principalTable: "InterviewQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewQuestions_InterviewQuestions_ParentId",
                table: "InterviewQuestions");

            migrationBuilder.DropTable(
                name: "UserApiKeys");

            migrationBuilder.DropIndex(
                name: "IX_InterviewQuestions_ParentId",
                table: "InterviewQuestions");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "Profession",
                table: "InterviewSessions");

            migrationBuilder.DropColumn(
                name: "CodeSnippet",
                table: "InterviewQuestions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "InterviewQuestions");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "InterviewQuestions");

            migrationBuilder.DropColumn(
                name: "AudioFeedback",
                table: "InterviewAnswers");

            migrationBuilder.DropColumn(
                name: "AudioScore",
                table: "InterviewAnswers");

            migrationBuilder.DropColumn(
                name: "VideoFeedback",
                table: "InterviewAnswers");

            migrationBuilder.DropColumn(
                name: "VideoScore",
                table: "InterviewAnswers");
        }
    }
}
