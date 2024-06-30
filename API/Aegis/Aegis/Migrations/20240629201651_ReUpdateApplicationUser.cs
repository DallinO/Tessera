using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aegis.Migrations
{
    /// <inheritdoc />
    public partial class ReUpdateApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizationsJoined",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnedOrganizations",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationsJoined",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OwnedOrganizations",
                table: "AspNetUsers");
        }
    }
}
