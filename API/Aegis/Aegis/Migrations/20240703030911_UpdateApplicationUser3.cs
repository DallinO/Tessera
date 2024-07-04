using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aegis.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationUser3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnedOrganizations",
                table: "AspNetUsers",
                newName: "OwnedOrgs");

            migrationBuilder.RenameColumn(
                name: "OrganizationsJoined",
                table: "AspNetUsers",
                newName: "JoinedOrgs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnedOrgs",
                table: "AspNetUsers",
                newName: "OwnedOrganizations");

            migrationBuilder.RenameColumn(
                name: "JoinedOrgs",
                table: "AspNetUsers",
                newName: "OrganizationsJoined");
        }
    }
}
