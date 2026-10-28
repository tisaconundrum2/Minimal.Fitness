using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimal.Fitness.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutHistorySnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlanNameSnapshot",
                schema: "minimal-fitness",
                table: "WorkoutSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExerciseNameSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlannedDurationSecondsSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlannedRepsSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlannedSetsSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlannedWeightSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanNameSnapshot",
                schema: "minimal-fitness",
                table: "WorkoutSessions");

            migrationBuilder.DropColumn(
                name: "ExerciseNameSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises");

            migrationBuilder.DropColumn(
                name: "PlannedDurationSecondsSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises");

            migrationBuilder.DropColumn(
                name: "PlannedRepsSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises");

            migrationBuilder.DropColumn(
                name: "PlannedSetsSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises");

            migrationBuilder.DropColumn(
                name: "PlannedWeightSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises");

            migrationBuilder.DropColumn(
                name: "SectionSnapshot",
                schema: "minimal-fitness",
                table: "SessionExercises");
        }
    }
}
