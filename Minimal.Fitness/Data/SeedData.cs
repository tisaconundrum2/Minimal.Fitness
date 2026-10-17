using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Models;
using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.Exercises.AnyAsync())
            return;

        var declinePress = new Exercise { Name = "Decline Press", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "decline-bench-press" };
        var latPulldown = new Exercise { Name = "Lat Pulldown", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "lat-pulldown" };
        var shoulderPress = new Exercise { Name = "Converging Shoulder Press", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "machine-shoulder-press" };
        var tricepExtensions = new Exercise { Name = "Tricep Extensions", MuscleGroup = MuscleGroup.Triceps, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "overhead-tricep-extension" };
        var bicepsCurls = new Exercise { Name = "Biceps Curls", MuscleGroup = MuscleGroup.Biceps, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "bicep-curl" };
        var latRow = new Exercise { Name = "Lat Row", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "seated-row" };
        var pullUps = new Exercise { Name = "Pull Ups", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Pull-Up Bar", AnimationPath = "pull-up" };
        var pallofPress = new Exercise { Name = "Pallof Press", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Cable Machine", AnimationPath = "pallof-press" };
        var toeToToeCrunch = new Exercise { Name = "Toe to Toe Crunch", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, AnimationPath = "crunch" };
        var vCrunch = new Exercise { Name = "V-Crunch", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, AnimationPath = "bicycle-crunch" };
        var squats = new Exercise { Name = "Squats", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, AnimationPath = "bodyweight-squat" };
        var legPress = new Exercise { Name = "Leg Press", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "leg-press" };
        var hamstringCurls = new Exercise { Name = "Hamstring Curls", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "leg-curl" };
        var legExtensions = new Exercise { Name = "Leg Extensions", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "leg-extension" };
        var plank = new Exercise { Name = "Plank", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, AnimationPath = "plank" };
        var hipThrusts = new Exercise { Name = "Hip Thrusts", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, AnimationPath = "hip-thrust" };
        var hangingKneeRaises = new Exercise { Name = "Hanging Knee Raises", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Pull-Up Bar", AnimationPath = "hanging-knee-raise" };

        db.Exercises.AddRange(declinePress, latPulldown, shoulderPress, tricepExtensions, bicepsCurls, latRow, pullUps, pallofPress, toeToToeCrunch, vCrunch, squats, legPress, hamstringCurls, legExtensions, plank, hipThrusts, hangingKneeRaises);
        await db.SaveChangesAsync();

        var workoutA = new WorkoutPlan
        {
            Name = "Workout A (Upper Body)",
            Description = "Upper body compound and isolation movements with core work",
            IsSeeded = true,
            Exercises =
            [
                new() { Exercise = declinePress, Section = "Upper Body", SortOrder = 0, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 25.0m },
                new() { Exercise = latPulldown, Section = "Upper Body", SortOrder = 1, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 60.0m },
                new() { Exercise = shoulderPress, Section = "Upper Body", SortOrder = 2, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 20.0m },
                new() { Exercise = tricepExtensions, Section = "Upper Body", SortOrder = 3, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 60.0m },
                new() { Exercise = bicepsCurls, Section = "Upper Body", SortOrder = 4, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 30.0m },
                new() { Exercise = latRow, Section = "Upper Body", SortOrder = 5, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 55.0m },
                new() { Exercise = pullUps, Section = "Upper Body", SortOrder = 6, DefaultSets = 3, DefaultReps = 5 },
                new() { Exercise = pallofPress, Section = "Abs", SortOrder = 7, DefaultSets = 3, DefaultReps = 12, DefaultWeight = 15.0m },
                new() { Exercise = toeToToeCrunch, Section = "Abs", SortOrder = 8, DefaultSets = 3, DefaultReps = 20 },
                new() { Exercise = vCrunch, Section = "Abs", SortOrder = 9, DefaultSets = 3, DefaultDurationSeconds = 60 },
            ]
        };

        var workoutB = new WorkoutPlan
        {
            Name = "Workout B (Lower Body)",
            Description = "Lower body strength with core work",
            IsSeeded = true,
            Exercises =
            [
                new() { Exercise = squats, Section = "Lower Body", SortOrder = 0, DefaultSets = 3, DefaultReps = 20 },
                new() { Exercise = legPress, Section = "Lower Body", SortOrder = 1, DefaultSets = 3, DefaultReps = 15, DefaultWeight = 55.0m },
                new() { Exercise = hamstringCurls, Section = "Lower Body", SortOrder = 2, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 35.0m },
                new() { Exercise = legExtensions, Section = "Lower Body", SortOrder = 3, DefaultSets = 3, DefaultReps = 10, DefaultWeight = 40.0m },
                new() { Exercise = plank, Section = "Abs", SortOrder = 4, DefaultSets = 3, DefaultDurationSeconds = 60 },
                new() { Exercise = hipThrusts, Section = "Abs", SortOrder = 5, DefaultSets = 3, DefaultReps = 20 },
                new() { Exercise = hangingKneeRaises, Section = "Abs", SortOrder = 6, DefaultSets = 3, DefaultReps = 15 },
            ]
        };

        db.WorkoutPlans.AddRange(workoutA, workoutB);
        await db.SaveChangesAsync();
    }
}
