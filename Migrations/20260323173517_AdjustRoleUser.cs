using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCTournament.Migrations
{
    /// <inheritdoc />
    public partial class AdjustRoleUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Referees");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Organizers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Managers");

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NickName",
                table: "Players");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Referees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Organizers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
