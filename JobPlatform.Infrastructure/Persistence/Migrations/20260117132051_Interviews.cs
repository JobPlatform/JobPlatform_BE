using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Interviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewParticipant");

            migrationBuilder.DropColumn(
                name: "MeetingLink",
                table: "Interviews");

            migrationBuilder.RenameColumn(
                name: "ScheduledAt",
                table: "Interviews",
                newName: "StartAt");

            migrationBuilder.RenameColumn(
                name: "MeetingRoomId",
                table: "Interviews",
                newName: "Note");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConfirmedAt",
                table: "Interviews",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Interviews",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingUrl",
                table: "Interviews",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReminderAt",
                table: "Interviews",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "Interviews",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "MeetingUrl",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "ReminderAt",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "Interviews");

            migrationBuilder.RenameColumn(
                name: "StartAt",
                table: "Interviews",
                newName: "ScheduledAt");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "Interviews",
                newName: "MeetingRoomId");

            migrationBuilder.AddColumn<string>(
                name: "MeetingLink",
                table: "Interviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InterviewParticipant",
                columns: table => new
                {
                    InterviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewParticipant", x => new { x.InterviewId, x.UserId });
                    table.ForeignKey(
                        name: "FK_InterviewParticipant_Interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
