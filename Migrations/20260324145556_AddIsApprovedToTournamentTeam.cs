using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCTournament.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToTournamentTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "TournamentTeams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "TournamentTeams");
        }
    }
}
