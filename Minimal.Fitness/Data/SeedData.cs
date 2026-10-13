using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Models;
using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        if (await db.Exercises.AnyAsync())
            return;

        var exercises = CreateExercises();
        db.Exercises.AddRange(exercises);
        await db.SaveChangesAsync();

        var lookup = exercises.ToDictionary(e => e.Name);
        var plans = CreatePlans(lookup);
        db.WorkoutPlans.AddRange(plans);
        await db.SaveChangesAsync();
    }

    private static List<Exercise> CreateExercises() =>
    [
        new() { Name = "Decline Press", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Chest, Equipment = "Machine" },
        new() { Name = "Lat Pulldown", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Back, Equipment = "Cable Machine" },
        new() { Name = "Converging Shoulder Press", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Shoulders, Equipment = "Machine" },
        new() { Name = "Tricep Extensions", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Triceps, Equipment = "Cable Machine" },
        new() { Name = "Biceps Curls", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Biceps, Equipment = "Dumbbells" },
        new() { Name = "Lat Row", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Back, Equipment = "Machine" },
        new() { Name = "Pull Ups", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Back, Equipment = "Bodyweight" },
        new() { Name = "Pallof Press", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Core, Equipment = "Cable Machine" },
        new() { Name = "Toe to Toe Crunch", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Core, Equipment = "Bodyweight" },
        new() { Name = "V-Crunch", TrackingType = TrackingType.Timer, MuscleGroup = MuscleGroup.Core, Equipment = "Bodyweight" },
        new() { Name = "Squats", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Legs, Equipment = "Bodyweight" },
        new() { Name = "Leg Press", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Legs, Equipment = "Machine" },
        new() { Name = "Hamstring Curls", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Legs, Equipment = "Machine" },
        new() { Name = "Leg Extensions", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Legs, Equipment = "Machine" },
        new() { Name = "Plank", TrackingType = TrackingType.Timer, MuscleGroup = MuscleGroup.Core, Equipment = "Bodyweight" },
        new() { Name = "Hip Thrusts", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Glutes, Equipment = "Bodyweight" },
        new() { Name = "Hanging Knee Raises", TrackingType = TrackingType.Reps, MuscleGroup = MuscleGroup.Core, Equipment = "Pull-up Bar" },
    ];

    private static List<WorkoutPlan> CreatePlans(Dictionary<string, Exercise> ex) =>
    [
        new()
        {
            Name = "Workout A (Upper Body)",
            Description = "Upper body compound and isolation movements with core work",
            IsSeeded = true,
            Exercises =
            [
                new() { Exercise = ex["Decline Press"], Section = "Upper Body", SortOrder = 0, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 25 },
                new() { Exercise = ex["Lat Pulldown"], Section = "Upper Body", SortOrder = 1, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 60 },
                new() { Exercise = ex["Converging Shoulder Press"], Section = "Upper Body", SortOrder = 2, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 20 },
                new() { Exercise = ex["Tricep Extensions"], Section = "Upper Body", SortOrder = 3, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 60 },
                new() { Exercise = ex["Biceps Curls"], Section = "Upper Body", SortOrder = 4, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 30 },
                new() { Exercise = ex["Lat Row"], Section = "Upper Body", SortOrder = 5, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 55 },
                new() { Exercise = ex["Pull Ups"], Section = "Upper Body", SortOrder = 6, DefaultSets = 3, DefaultReps = 5 },
                new() { Exercise = ex["Pallof Press"], Section = "Abs", SortOrder = 7, DefaultSets = 3, DefaultReps = 12, DefaultWeight = 15 },
                new() { Exercise = ex["Toe to Toe Crunch"], Section = "Abs", SortOrder = 8, DefaultSets = 3, DefaultReps = 20 },
                new() { Exercise = ex["V-Crunch"], Section = "Abs", SortOrder = 9, DefaultSets = 3, DefaultDurationSeconds = 60 },
            ]
        },
        new()
        {
            Name = "Workout B (Lower Body)",
            Description = "Lower body compound movements with core work",
            IsSeeded = true,
            Exercises =
            [
                new() { Exercise = ex["Squats"], Section = "Lower Body", SortOrder = 0, DefaultSets = 3, DefaultReps = 20 },
                new() { Exercise = ex["Leg Press"], Section = "Lower Body", SortOrder = 1, DefaultSets = 3, DefaultReps = 15, DefaultWeight = 55 },
                new() { Exercise = ex["Hamstring Curls"], Section = "Lower Body", SortOrder = 2, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 35 },
                new() { Exercise = ex["Leg Extensions"], Section = "Lower Body", SortOrder = 3, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 40 },
                new() { Exercise = ex["Plank"], Section = "Abs", SortOrder = 4, DefaultSets = 3, DefaultDurationSeconds = 60 },
                new() { Exercise = ex["Hip Thrusts"], Section = "Abs", SortOrder = 5, DefaultSets = 3, DefaultReps = 20 },
                new() { Exercise = ex["Hanging Knee Raises"], Section = "Abs", SortOrder = 6, DefaultSets = 3, DefaultReps = 15 },
            ]
        }
    ];
}
