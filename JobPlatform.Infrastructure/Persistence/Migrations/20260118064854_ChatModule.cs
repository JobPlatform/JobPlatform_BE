using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChatModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationMember_Conversations_ConversationId",
                table: "ConversationMember");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConversationMember",
                table: "ConversationMember");

            migrationBuilder.RenameTable(
                name: "ConversationMember",
                newName: "ConversationMembers");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Conversations",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "JoinedAt",
                table: "ConversationMembers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastReadAt",
                table: "ConversationMembers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastReadMessageId",
                table: "ConversationMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnreadCount",
                table: "ConversationMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConversationMembers",
                table: "ConversationMembers",
                columns: new[] { "ConversationId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_SentAt_Id",
                table: "Messages",
                columns: new[] { "ConversationId", "SentAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMembers_UserId",
                table: "ConversationMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMembers_UserId_UnreadCount",
                table: "ConversationMembers",
                columns: new[] { "UserId", "UnreadCount" });

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationMembers_Conversations_ConversationId",
                table: "ConversationMembers",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationMembers_Conversations_ConversationId",
                table: "ConversationMembers");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ConversationId_SentAt_Id",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConversationMembers",
                table: "ConversationMembers");

            migrationBuilder.DropIndex(
                name: "IX_ConversationMembers_UserId",
                table: "ConversationMembers");

            migrationBuilder.DropIndex(
                name: "IX_ConversationMembers_UserId_UnreadCount",
                table: "ConversationMembers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "JoinedAt",
                table: "ConversationMembers");

            migrationBuilder.DropColumn(
                name: "LastReadAt",
                table: "ConversationMembers");

            migrationBuilder.DropColumn(
                name: "LastReadMessageId",
                table: "ConversationMembers");

            migrationBuilder.DropColumn(
                name: "UnreadCount",
                table: "ConversationMembers");

            migrationBuilder.RenameTable(
                name: "ConversationMembers",
                newName: "ConversationMember");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConversationMember",
                table: "ConversationMember",
                columns: new[] { "ConversationId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationMember_Conversations_ConversationId",
                table: "ConversationMember",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
