using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientCpf",
                table: "meetings",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientEmail",
                table: "meetings",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<Guid>(type: "uuid", nullable: false),
                    PrincipalType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_PrincipalType_EnterpriseId",
                table: "sessions",
                columns: new[] { "PrincipalType", "EnterpriseId" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_PrincipalType_UserId",
                table: "sessions",
                columns: new[] { "PrincipalType", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_Token",
                table: "sessions",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropColumn(
                name: "ClientCpf",
                table: "meetings");

            migrationBuilder.DropColumn(
                name: "ClientEmail",
                table: "meetings");
        }
    }
}
