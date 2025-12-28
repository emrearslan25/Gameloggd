using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLoggd.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline migration - tables already exist.
            // migrationBuilder.CreateTable(...);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Baseline migration - do nothing on revert to avoid data loss on existing DB.
        }
    }
}
