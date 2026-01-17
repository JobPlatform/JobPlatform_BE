using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NotificationEmailSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailSentAt",
                table: "Notifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_EmailSentAt",
                table: "Notifications",
                columns: new[] { "UserId", "EmailSentAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_JobMatches_JobPosts_JobPostId",
                table: "JobMatches",
                column: "JobPostId",
                principalTable: "JobPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobMatches_JobPosts_JobPostId",
                table: "JobMatches");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_EmailSentAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailSentAt",
                table: "Notifications");
        }
    }
}
