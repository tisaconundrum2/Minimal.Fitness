using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Minimal.Fitness.Models;
using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Data;

public static class SeedData
{
    private const string StarterPlansUserId = "00000000-0000-0000-0000-000000000001";

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var resetOnStartup = configuration.GetValue<bool>("SeedData:ResetOnStartup");

        if (resetOnStartup)
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "SeedData:ResetOnStartup is only supported in Development to prevent destructive data loss.");
            }

            await ResetSeedDataAsync(db);
        }

        var starterPlansUser = await db.Users.FirstOrDefaultAsync(u => u.Id == StarterPlansUserId);
        if (starterPlansUser == null)
        {
            db.Users.Add(new IdentityUser
            {
                Id = StarterPlansUserId,
                UserName = "starter-plans",
                NormalizedUserName = "STARTER-PLANS",
                Email = "starter-plans@local",
                NormalizedEmail = "STARTER-PLANS@LOCAL",
                EmailConfirmed = true
            });
            await db.SaveChangesAsync();
        }

        if (await db.Exercises.AnyAsync())
            return;

        var declinePress = new Exercise { Name = "Decline Press", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "decline-bench-press" };
        var latPulldown = new Exercise { Name = "Lat Pulldown", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "lat-pulldown" };
        var shoulderPress = new Exercise { Name = "Converging Shoulder Press", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "machine-shoulder-press" };
        var tricepExtensions = new Exercise { Name = "Tricep Extensions", MuscleGroup = MuscleGroup.Triceps, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "overhead-tricep-extension" };
        var bicepsCurls = new Exercise { Name = "Biceps Curls", MuscleGroup = MuscleGroup.Biceps, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "bicep-curl" };
        var latRow = new Exercise { Name = "Lat Row", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "seated-row" };
        var pullUps = new Exercise { Name = "Pull Ups", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "pull-up" };
        var pallofPress = new Exercise { Name = "Pallof Press", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "pallof-press" };
        var toeToToeCrunch = new Exercise { Name = "Toe to Toe Crunch", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "crunch" };
        var vCrunch = new Exercise { Name = "V-Crunch", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "bicycle-crunch" };
        var squats = new Exercise { Name = "Squats", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "bodyweight-squat" };
        var legPress = new Exercise { Name = "Leg Press", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "leg-press" };
        var hamstringCurls = new Exercise { Name = "Hamstring Curls", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "leg-curl" };
        var legExtensions = new Exercise { Name = "Leg Extensions", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "leg-extension" };
        var plank = new Exercise { Name = "Plank", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "plank" };
        var hipThrusts = new Exercise { Name = "Hip Thrusts", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "hip-thrust" };
        var hangingKneeRaises = new Exercise { Name = "Hanging Knee Raises", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Pull-up Bar", AnimationPath = "hanging-knee-raise" };

        db.Exercises.AddRange(declinePress, latPulldown, shoulderPress, tricepExtensions, bicepsCurls, latRow, pullUps, pallofPress, toeToToeCrunch, vCrunch, squats, legPress, hamstringCurls, legExtensions, plank, hipThrusts, hangingKneeRaises);

        // All remaining exercises from bryllim/workout-guide (CC BY-SA 4.0)
        db.Exercises.AddRange(
            new() { Name = "Bench Press", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "bench-press" },
            new() { Name = "Incline Bench Press", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "incline-bench-press" },
            new() { Name = "Incline Dumbbell Press", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "incline-dumbbell-press" },
            new() { Name = "Dumbbell Bench Press", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "dumbbell-bench-press" },
            new() { Name = "Machine Chest Press", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "machine-chest-press" },
            new() { Name = "Pec Deck", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "pec-deck" },
            new() { Name = "Cable Fly", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "cable-fly" },
            new() { Name = "Push-up", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "push-up" },
            new() { Name = "Dumbbell Fly", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "dumbbell-fly" },
            new() { Name = "Decline Dumbbell Press", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "decline-dumbbell-press" },
            new() { Name = "Chest Dip", MuscleGroup = MuscleGroup.Chest, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "chest-dip" },
            new() { Name = "Overhead Press", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "overhead-press" },
            new() { Name = "Arnold Press", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "arnold-press" },
            new() { Name = "Lateral Raise", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "lateral-raise" },
            new() { Name = "Cable Lateral Raise", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "cable-lateral-raise" },
            new() { Name = "Front Raise", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "front-raise" },
            new() { Name = "Upright Row", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "upright-row" },
            new() { Name = "Face Pull", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "face-pull" },
            new() { Name = "Rear Delt Fly", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "rear-delt-fly" },
            new() { Name = "Pike Push-up", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "pike-push-up" },
            new() { Name = "Barbell Row", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "barbell-row" },
            new() { Name = "T-Bar Row", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "t-bar-row" },
            new() { Name = "Dumbbell Bent Over Row", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "dumbbell-bent-over-row" },
            new() { Name = "One-Arm Dumbbell Row", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "one-arm-dumbbell-row" },
            new() { Name = "Chest Supported Row", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "chest-supported-row" },
            new() { Name = "Machine Row", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "machine-row" },
            new() { Name = "Close-Grip Lat Pulldown", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "close-grip-lat-pulldown" },
            new() { Name = "Straight-Arm Pulldown", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "straight-arm-pulldown" },
            new() { Name = "Weighted Pull-up", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "weighted-pull-up" },
            new() { Name = "Inverted Row", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "inverted-row" },
            new() { Name = "Back Extension", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "back-extension" },
            new() { Name = "Chin-up", MuscleGroup = MuscleGroup.Biceps, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "chin-up" },
            new() { Name = "Hammer Curl", MuscleGroup = MuscleGroup.Biceps, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "hammer-curl" },
            new() { Name = "Preacher Curl", MuscleGroup = MuscleGroup.Biceps, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "preacher-curl" },
            new() { Name = "Cable Curl", MuscleGroup = MuscleGroup.Biceps, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "cable-curl" },
            new() { Name = "EZ-Bar Curl", MuscleGroup = MuscleGroup.Biceps, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "ez-bar-curl" },
            new() { Name = "Concentration Curl", MuscleGroup = MuscleGroup.Biceps, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "concentration-curl" },
            new() { Name = "Incline Dumbbell Curl", MuscleGroup = MuscleGroup.Biceps, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "incline-dumbbell-curl" },
            new() { Name = "Tricep Pushdown", MuscleGroup = MuscleGroup.Triceps, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "tricep-pushdown" },
            new() { Name = "Skull Crusher", MuscleGroup = MuscleGroup.Triceps, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "skull-crusher" },
            new() { Name = "Close-Grip Bench Press", MuscleGroup = MuscleGroup.Triceps, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "close-grip-bench-press" },
            new() { Name = "Dip", MuscleGroup = MuscleGroup.Triceps, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "dip" },
            new() { Name = "Tricep Kickback", MuscleGroup = MuscleGroup.Triceps, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "tricep-kickback" },
            new() { Name = "Diamond Push-up", MuscleGroup = MuscleGroup.Triceps, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "diamond-push-up" },
            new() { Name = "Bench Dip", MuscleGroup = MuscleGroup.Triceps, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "bench-dip" },
            new() { Name = "Squat", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "squat" },
            new() { Name = "Front Squat", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "front-squat" },
            new() { Name = "Hack Squat", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "hack-squat" },
            new() { Name = "Bulgarian Split Squat", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "bulgarian-split-squat" },
            new() { Name = "Goblet Squat", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "goblet-squat" },
            new() { Name = "Walking Lunge", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "walking-lunge" },
            new() { Name = "Romanian Deadlift", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "romanian-deadlift" },
            new() { Name = "Seated Leg Curl", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "seated-leg-curl" },
            new() { Name = "Lying Leg Curl", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "lying-leg-curl" },
            new() { Name = "Nordic Hamstring Curl", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "nordic-hamstring-curl" },
            new() { Name = "Standing Calf Raise", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "standing-calf-raise" },
            new() { Name = "Seated Calf Raise", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "seated-calf-raise" },
            new() { Name = "Good Morning", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "good-morning" },
            new() { Name = "Step-Up", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "step-up" },
            new() { Name = "Jump Squat", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "jump-squat" },
            new() { Name = "Wall Sit", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "wall-sit" },
            new() { Name = "Pistol Squat", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "pistol-squat" },
            new() { Name = "Forward Lunge", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "forward-lunge" },
            new() { Name = "Reverse Lunge", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "reverse-lunge" },
            new() { Name = "Cossack Squat", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "cossack-squat" },
            new() { Name = "Glute Bridge", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "glute-bridge" },
            new() { Name = "Dumbbell Hip Thrust", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "dumbbell-hip-thrust" },
            new() { Name = "Cable Kickback", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "cable-kickback" },
            new() { Name = "Hip Abduction Machine", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "hip-abduction-machine" },
            new() { Name = "Kettlebell Swing", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Kettlebell", AnimationPath = "kettlebell-swing" },
            new() { Name = "Donkey Kick", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "donkey-kick" },
            new() { Name = "Fire Hydrant", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "fire-hydrant" },
            new() { Name = "Clamshell", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "clamshell" },
            new() { Name = "Curtsy Lunge", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "curtsy-lunge" },
            new() { Name = "Frog Pump", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "frog-pump" },
            new() { Name = "Reverse Hyperextension", MuscleGroup = MuscleGroup.Glutes, TrackingType = TrackingType.Reps, Equipment = "Machine", AnimationPath = "reverse-hyperextension" },
            new() { Name = "Side Plank", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "side-plank" },
            new() { Name = "Hanging Leg Raise", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "hanging-leg-raise" },
            new() { Name = "Cable Crunch", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "cable-crunch" },
            new() { Name = "Ab Wheel Rollout", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "ab-wheel" },
            new() { Name = "Reverse Crunch", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "reverse-crunch" },
            new() { Name = "Russian Twist", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "russian-twist" },
            new() { Name = "Mountain Climber", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "mountain-climber" },
            new() { Name = "Dead Bug", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "dead-bug" },
            new() { Name = "Bird Dog", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "bird-dog" },
            new() { Name = "Cable Woodchop", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Cable", AnimationPath = "cable-woodchop" },
            new() { Name = "V-Up", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "v-up" },
            new() { Name = "Flutter Kick", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "flutter-kick" },
            new() { Name = "Hollow Body Hold", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "hollow-body-hold" },
            new() { Name = "Bear Crawl", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "bear-crawl" },
            new() { Name = "L-Sit Hold", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "l-sit-hold" },
            new() { Name = "Copenhagen Plank", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Timer, Equipment = "Bodyweight", AnimationPath = "copenhagen-plank" },
            new() { Name = "Dragon Flag", MuscleGroup = MuscleGroup.Core, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "dragon-flag" },
            new() { Name = "Deadlift", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "deadlift" },
            new() { Name = "Sumo Deadlift", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "sumo-deadlift" },
            new() { Name = "Trap Bar Deadlift", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Barbell", AnimationPath = "trap-bar-deadlift" },
            new() { Name = "Shrug", MuscleGroup = MuscleGroup.Back, TrackingType = TrackingType.Reps, Equipment = "Dumbbell", AnimationPath = "shrug" },
            new() { Name = "Farmer Carry", MuscleGroup = MuscleGroup.FullBody, TrackingType = TrackingType.Timer, Equipment = "Dumbbell", AnimationPath = "farmer-carry" },
            new() { Name = "Battle Ropes", MuscleGroup = MuscleGroup.Shoulders, TrackingType = TrackingType.Timer, Equipment = "Cardio", AnimationPath = "battle-ropes" },
            new() { Name = "Jump Rope", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Timer, Equipment = "Cardio", AnimationPath = "jump-rope" },
            new() { Name = "Treadmill Incline Walk", MuscleGroup = MuscleGroup.Legs, TrackingType = TrackingType.Timer, Equipment = "Cardio", AnimationPath = "treadmill-incline-walk" },
            new() { Name = "Burpee", MuscleGroup = MuscleGroup.FullBody, TrackingType = TrackingType.Reps, Equipment = "Bodyweight", AnimationPath = "burpee" },
            new() { Name = "Rowing", MuscleGroup = MuscleGroup.FullBody, TrackingType = TrackingType.Timer, Equipment = "Machine", AnimationPath = "rowing" },
            new() { Name = "Assault Bike", MuscleGroup = MuscleGroup.FullBody, TrackingType = TrackingType.Timer, Equipment = "Machine", AnimationPath = "assault-bike" }
        );

        await db.SaveChangesAsync();

        var workoutA = new WorkoutPlan
        {
            Name = "Workout A (Upper Body)",
            Description = "Upper body compound and isolation movements with core work",
            CreatedByUserId = StarterPlansUserId,
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
            CreatedByUserId = StarterPlansUserId,
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

    private static async Task ResetSeedDataAsync(ApplicationDbContext db)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        await db.ExerciseSets.ExecuteDeleteAsync();
        await db.SessionExercises.ExecuteDeleteAsync();
        await db.WorkoutSessions.ExecuteDeleteAsync();
        await db.WorkoutPlanExercises.ExecuteDeleteAsync();
        await db.WorkoutPlans.ExecuteDeleteAsync();
        await db.Exercises.ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }
}
