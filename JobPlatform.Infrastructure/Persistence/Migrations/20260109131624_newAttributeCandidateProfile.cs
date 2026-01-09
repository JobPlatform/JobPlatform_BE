using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class newAttributeCandidateProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Years",
                table: "CandidateSkill",
                type: "decimal(4,1)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "CvContentType",
                table: "CandidateProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvFileName",
                table: "CandidateProfiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CvFileSize",
                table: "CandidateProfiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvStoragePath",
                table: "CandidateProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CvUploadedAt",
                table: "CandidateProfiles",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvContentType",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "CvFileName",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "CvFileSize",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "CvStoragePath",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "CvUploadedAt",
                table: "CandidateProfiles");

            migrationBuilder.AlterColumn<decimal>(
                name: "Years",
                table: "CandidateSkill",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,1)");
        }
    }
}
