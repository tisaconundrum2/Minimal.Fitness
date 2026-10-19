using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimal.Fitness.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutPlanRestSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestSeconds",
                schema: "minimal-fitness",
                table: "WorkoutPlans",
                type: "integer",
                nullable: false,
                defaultValue: 75);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestSeconds",
                schema: "minimal-fitness",
                table: "WorkoutPlans");
        }
    }
}
