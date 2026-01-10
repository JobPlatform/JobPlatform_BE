using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class JobApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvFilePath",
                table: "JobApplications");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AppliedAt",
                table: "JobApplications",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CoverLetter",
                table: "JobApplications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StatusChangedAt",
                table: "JobApplications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusNote",
                table: "JobApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CoverLetter",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "StatusNote",
                table: "JobApplications");

            migrationBuilder.AddColumn<string>(
                name: "CvFilePath",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
