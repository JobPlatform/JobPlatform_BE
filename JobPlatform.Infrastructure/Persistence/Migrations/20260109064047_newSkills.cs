using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class newSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkillCategory_SkillDomain_DomainId",
                table: "SkillCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_Skills_SkillCategory_CategoryId",
                table: "Skills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SkillDomain",
                table: "SkillDomain");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SkillCategory",
                table: "SkillCategory");

            migrationBuilder.RenameTable(
                name: "SkillDomain",
                newName: "SkillDomains");

            migrationBuilder.RenameTable(
                name: "SkillCategory",
                newName: "SkillCategories");

            migrationBuilder.RenameIndex(
                name: "IX_SkillDomain_Code",
                table: "SkillDomains",
                newName: "IX_SkillDomains_Code");

            migrationBuilder.RenameIndex(
                name: "IX_SkillCategory_DomainId_Code",
                table: "SkillCategories",
                newName: "IX_SkillCategories_DomainId_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkillDomains",
                table: "SkillDomains",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkillCategories",
                table: "SkillCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SkillCategories_SkillDomains_DomainId",
                table: "SkillCategories",
                column: "DomainId",
                principalTable: "SkillDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_SkillCategories_CategoryId",
                table: "Skills",
                column: "CategoryId",
                principalTable: "SkillCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkillCategories_SkillDomains_DomainId",
                table: "SkillCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Skills_SkillCategories_CategoryId",
                table: "Skills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SkillDomains",
                table: "SkillDomains");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SkillCategories",
                table: "SkillCategories");

            migrationBuilder.RenameTable(
                name: "SkillDomains",
                newName: "SkillDomain");

            migrationBuilder.RenameTable(
                name: "SkillCategories",
                newName: "SkillCategory");

            migrationBuilder.RenameIndex(
                name: "IX_SkillDomains_Code",
                table: "SkillDomain",
                newName: "IX_SkillDomain_Code");

            migrationBuilder.RenameIndex(
                name: "IX_SkillCategories_DomainId_Code",
                table: "SkillCategory",
                newName: "IX_SkillCategory_DomainId_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkillDomain",
                table: "SkillDomain",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkillCategory",
                table: "SkillCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SkillCategory_SkillDomain_DomainId",
                table: "SkillCategory",
                column: "DomainId",
                principalTable: "SkillDomain",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_SkillCategory_CategoryId",
                table: "Skills",
                column: "CategoryId",
                principalTable: "SkillCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
