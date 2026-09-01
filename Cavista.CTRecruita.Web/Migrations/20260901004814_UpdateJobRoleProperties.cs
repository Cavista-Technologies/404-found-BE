using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cavista.CTRecruita.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobRoleProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStageHistory_Applications_ApplicationId",
                table: "ApplicationStageHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStageHistory_AspNetUsers_ChangedById",
                table: "ApplicationStageHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationStageHistory",
                table: "ApplicationStageHistory");

            migrationBuilder.RenameTable(
                name: "ApplicationStageHistory",
                newName: "ApplicationStageHistories");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationStageHistory_ChangedById",
                table: "ApplicationStageHistories",
                newName: "IX_ApplicationStageHistories_ChangedById");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationStageHistory_ApplicationId",
                table: "ApplicationStageHistories",
                newName: "IX_ApplicationStageHistories_ApplicationId");

            migrationBuilder.AddColumn<DateTime>(
                name: "FilledAt",
                table: "JobRoles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "JobRoles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationStageHistories",
                table: "ApplicationStageHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStageHistories_Applications_ApplicationId",
                table: "ApplicationStageHistories",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStageHistories_AspNetUsers_ChangedById",
                table: "ApplicationStageHistories",
                column: "ChangedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStageHistories_Applications_ApplicationId",
                table: "ApplicationStageHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStageHistories_AspNetUsers_ChangedById",
                table: "ApplicationStageHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationStageHistories",
                table: "ApplicationStageHistories");

            migrationBuilder.DropColumn(
                name: "FilledAt",
                table: "JobRoles");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "JobRoles");

            migrationBuilder.RenameTable(
                name: "ApplicationStageHistories",
                newName: "ApplicationStageHistory");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationStageHistories_ChangedById",
                table: "ApplicationStageHistory",
                newName: "IX_ApplicationStageHistory_ChangedById");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationStageHistories_ApplicationId",
                table: "ApplicationStageHistory",
                newName: "IX_ApplicationStageHistory_ApplicationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationStageHistory",
                table: "ApplicationStageHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStageHistory_Applications_ApplicationId",
                table: "ApplicationStageHistory",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStageHistory_AspNetUsers_ChangedById",
                table: "ApplicationStageHistory",
                column: "ChangedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
