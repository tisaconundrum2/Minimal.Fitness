using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimal.Fitness.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionExerciseSkipped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Skipped",
                schema: "minimal-fitness",
                table: "SessionExercises",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Skipped",
                schema: "minimal-fitness",
                table: "SessionExercises");
        }
    }
}
