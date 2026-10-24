using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimal.Fitness.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionExerciseDeferred : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deferred",
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
                name: "Deferred",
                schema: "minimal-fitness",
                table: "SessionExercises");
        }
    }
}
