using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLoggd.Migrations
{
    /// <inheritdoc />
    public partial class AddDatePlayed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatePlayed",
                table: "UserGameLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePlayed",
                table: "UserGameLogs");
        }
    }
}
