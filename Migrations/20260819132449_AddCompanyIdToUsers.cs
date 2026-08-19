using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Otw.Puzzli.Api.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing users are backfilled with this company id.
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Users",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("c12dd532-481b-4ff2-a4f1-a1b2ee417f75"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Users");
        }
    }
}
