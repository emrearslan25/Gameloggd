using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLoggd.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCredits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Credits",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDailyBonus",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Credits",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastDailyBonus",
                table: "AspNetUsers");
        }
    }
}
