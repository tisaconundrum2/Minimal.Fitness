using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimal.Fitness.Migrations
{
    /// <inheritdoc />
    public partial class SeedStarterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
INSERT INTO "minimal-fitness"."AspNetUsers"
    ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount")
SELECT '00000000-0000-0000-0000-000000000001', 'starter-plans', 'STARTER-PLANS', 'starter-plans@local', 'STARTER-PLANS@LOCAL', TRUE, FALSE, FALSE, FALSE, 0
WHERE NOT EXISTS (
    SELECT 1
    FROM "minimal-fitness"."AspNetUsers"
    WHERE "Id" = '00000000-0000-0000-0000-000000000001'
);
""");

            migrationBuilder.Sql("""
INSERT INTO "minimal-fitness"."Exercises" ("Name", "MuscleGroup", "TrackingType", "Equipment", "AnimationPath")
SELECT v."Name", v."MuscleGroup", v."TrackingType", v."Equipment", v."AnimationPath"
FROM (VALUES
    ('Decline Press', 'Chest', 'Reps', 'Barbell', 'decline-bench-press'),
    ('Lat Pulldown', 'Back', 'Reps', 'Cable', 'lat-pulldown'),
    ('Converging Shoulder Press', 'Shoulders', 'Reps', 'Machine', 'machine-shoulder-press'),
    ('Tricep Extensions', 'Triceps', 'Reps', 'Cable', 'overhead-tricep-extension'),
    ('Biceps Curls', 'Biceps', 'Reps', 'Dumbbell', 'bicep-curl'),
    ('Lat Row', 'Back', 'Reps', 'Cable', 'seated-row'),
    ('Pull Ups', 'Back', 'Reps', 'Bodyweight', 'pull-up'),
    ('Pallof Press', 'Core', 'Reps', 'Cable', 'pallof-press'),
    ('Toe to Toe Crunch', 'Core', 'Reps', 'Bodyweight', 'crunch'),
    ('V-Crunch', 'Core', 'Timer', 'Bodyweight', 'bicycle-crunch'),
    ('Squats', 'Legs', 'Reps', 'Bodyweight', 'bodyweight-squat'),
    ('Leg Press', 'Legs', 'Reps', 'Machine', 'leg-press'),
    ('Hamstring Curls', 'Legs', 'Reps', 'Machine', 'leg-curl'),
    ('Leg Extensions', 'Legs', 'Reps', 'Machine', 'leg-extension'),
    ('Plank', 'Core', 'Timer', 'Bodyweight', 'plank'),
    ('Hip Thrusts', 'Glutes', 'Reps', 'Barbell', 'hip-thrust'),
    ('Hanging Knee Raises', 'Core', 'Reps', 'Pull-up Bar', 'hanging-knee-raise'),
    ('Bench Press', 'Chest', 'Reps', 'Barbell', 'bench-press'),
    ('Incline Bench Press', 'Chest', 'Reps', 'Barbell', 'incline-bench-press'),
    ('Incline Dumbbell Press', 'Chest', 'Reps', 'Dumbbell', 'incline-dumbbell-press'),
    ('Dumbbell Bench Press', 'Chest', 'Reps', 'Dumbbell', 'dumbbell-bench-press'),
    ('Machine Chest Press', 'Chest', 'Reps', 'Machine', 'machine-chest-press'),
    ('Pec Deck', 'Chest', 'Reps', 'Machine', 'pec-deck'),
    ('Cable Fly', 'Chest', 'Reps', 'Cable', 'cable-fly'),
    ('Push-up', 'Chest', 'Reps', 'Bodyweight', 'push-up'),
    ('Dumbbell Fly', 'Chest', 'Reps', 'Dumbbell', 'dumbbell-fly'),
    ('Decline Dumbbell Press', 'Chest', 'Reps', 'Dumbbell', 'decline-dumbbell-press'),
    ('Chest Dip', 'Chest', 'Reps', 'Bodyweight', 'chest-dip'),
    ('Overhead Press', 'Shoulders', 'Reps', 'Barbell', 'overhead-press'),
    ('Arnold Press', 'Shoulders', 'Reps', 'Dumbbell', 'arnold-press'),
    ('Lateral Raise', 'Shoulders', 'Reps', 'Dumbbell', 'lateral-raise'),
    ('Cable Lateral Raise', 'Shoulders', 'Reps', 'Cable', 'cable-lateral-raise'),
    ('Front Raise', 'Shoulders', 'Reps', 'Dumbbell', 'front-raise'),
    ('Upright Row', 'Shoulders', 'Reps', 'Barbell', 'upright-row'),
    ('Face Pull', 'Shoulders', 'Reps', 'Cable', 'face-pull'),
    ('Rear Delt Fly', 'Shoulders', 'Reps', 'Dumbbell', 'rear-delt-fly'),
    ('Pike Push-up', 'Shoulders', 'Reps', 'Bodyweight', 'pike-push-up'),
    ('Barbell Row', 'Back', 'Reps', 'Barbell', 'barbell-row'),
    ('T-Bar Row', 'Back', 'Reps', 'Machine', 't-bar-row'),
    ('Dumbbell Bent Over Row', 'Back', 'Reps', 'Dumbbell', 'dumbbell-bent-over-row'),
    ('One-Arm Dumbbell Row', 'Back', 'Reps', 'Dumbbell', 'one-arm-dumbbell-row'),
    ('Chest Supported Row', 'Back', 'Reps', 'Machine', 'chest-supported-row'),
    ('Machine Row', 'Back', 'Reps', 'Machine', 'machine-row'),
    ('Close-Grip Lat Pulldown', 'Back', 'Reps', 'Cable', 'close-grip-lat-pulldown'),
    ('Straight-Arm Pulldown', 'Back', 'Reps', 'Cable', 'straight-arm-pulldown'),
    ('Weighted Pull-up', 'Back', 'Reps', 'Bodyweight', 'weighted-pull-up'),
    ('Inverted Row', 'Back', 'Reps', 'Bodyweight', 'inverted-row'),
    ('Back Extension', 'Back', 'Reps', 'Bodyweight', 'back-extension'),
    ('Chin-up', 'Biceps', 'Reps', 'Bodyweight', 'chin-up'),
    ('Hammer Curl', 'Biceps', 'Reps', 'Dumbbell', 'hammer-curl'),
    ('Preacher Curl', 'Biceps', 'Reps', 'Machine', 'preacher-curl'),
    ('Cable Curl', 'Biceps', 'Reps', 'Cable', 'cable-curl'),
    ('EZ-Bar Curl', 'Biceps', 'Reps', 'Barbell', 'ez-bar-curl'),
    ('Concentration Curl', 'Biceps', 'Reps', 'Dumbbell', 'concentration-curl'),
    ('Incline Dumbbell Curl', 'Biceps', 'Reps', 'Dumbbell', 'incline-dumbbell-curl'),
    ('Tricep Pushdown', 'Triceps', 'Reps', 'Cable', 'tricep-pushdown'),
    ('Skull Crusher', 'Triceps', 'Reps', 'Barbell', 'skull-crusher'),
    ('Close-Grip Bench Press', 'Triceps', 'Reps', 'Barbell', 'close-grip-bench-press'),
    ('Dip', 'Triceps', 'Reps', 'Bodyweight', 'dip'),
    ('Tricep Kickback', 'Triceps', 'Reps', 'Dumbbell', 'tricep-kickback'),
    ('Diamond Push-up', 'Triceps', 'Reps', 'Bodyweight', 'diamond-push-up'),
    ('Bench Dip', 'Triceps', 'Reps', 'Bodyweight', 'bench-dip'),
    ('Squat', 'Legs', 'Reps', 'Barbell', 'squat'),
    ('Front Squat', 'Legs', 'Reps', 'Barbell', 'front-squat'),
    ('Hack Squat', 'Legs', 'Reps', 'Machine', 'hack-squat'),
    ('Bulgarian Split Squat', 'Legs', 'Reps', 'Dumbbell', 'bulgarian-split-squat'),
    ('Goblet Squat', 'Legs', 'Reps', 'Dumbbell', 'goblet-squat'),
    ('Walking Lunge', 'Legs', 'Reps', 'Dumbbell', 'walking-lunge'),
    ('Romanian Deadlift', 'Legs', 'Reps', 'Barbell', 'romanian-deadlift'),
    ('Seated Leg Curl', 'Legs', 'Reps', 'Machine', 'seated-leg-curl'),
    ('Lying Leg Curl', 'Legs', 'Reps', 'Machine', 'lying-leg-curl'),
    ('Nordic Hamstring Curl', 'Legs', 'Reps', 'Bodyweight', 'nordic-hamstring-curl'),
    ('Standing Calf Raise', 'Legs', 'Reps', 'Machine', 'standing-calf-raise'),
    ('Seated Calf Raise', 'Legs', 'Reps', 'Machine', 'seated-calf-raise'),
    ('Good Morning', 'Legs', 'Reps', 'Barbell', 'good-morning'),
    ('Step-Up', 'Legs', 'Reps', 'Dumbbell', 'step-up'),
    ('Jump Squat', 'Legs', 'Reps', 'Bodyweight', 'jump-squat'),
    ('Wall Sit', 'Legs', 'Timer', 'Bodyweight', 'wall-sit'),
    ('Pistol Squat', 'Legs', 'Reps', 'Bodyweight', 'pistol-squat'),
    ('Forward Lunge', 'Legs', 'Reps', 'Bodyweight', 'forward-lunge'),
    ('Reverse Lunge', 'Legs', 'Reps', 'Dumbbell', 'reverse-lunge'),
    ('Cossack Squat', 'Legs', 'Reps', 'Bodyweight', 'cossack-squat'),
    ('Glute Bridge', 'Glutes', 'Reps', 'Bodyweight', 'glute-bridge'),
    ('Dumbbell Hip Thrust', 'Glutes', 'Reps', 'Dumbbell', 'dumbbell-hip-thrust'),
    ('Cable Kickback', 'Glutes', 'Reps', 'Cable', 'cable-kickback'),
    ('Hip Abduction Machine', 'Glutes', 'Reps', 'Machine', 'hip-abduction-machine'),
    ('Kettlebell Swing', 'Glutes', 'Reps', 'Kettlebell', 'kettlebell-swing'),
    ('Donkey Kick', 'Glutes', 'Reps', 'Bodyweight', 'donkey-kick'),
    ('Fire Hydrant', 'Glutes', 'Reps', 'Bodyweight', 'fire-hydrant'),
    ('Clamshell', 'Glutes', 'Reps', 'Bodyweight', 'clamshell'),
    ('Curtsy Lunge', 'Glutes', 'Reps', 'Bodyweight', 'curtsy-lunge'),
    ('Frog Pump', 'Glutes', 'Reps', 'Bodyweight', 'frog-pump'),
    ('Reverse Hyperextension', 'Glutes', 'Reps', 'Machine', 'reverse-hyperextension'),
    ('Side Plank', 'Core', 'Timer', 'Bodyweight', 'side-plank'),
    ('Hanging Leg Raise', 'Core', 'Reps', 'Bodyweight', 'hanging-leg-raise'),
    ('Cable Crunch', 'Core', 'Reps', 'Cable', 'cable-crunch'),
    ('Ab Wheel Rollout', 'Core', 'Reps', 'Bodyweight', 'ab-wheel'),
    ('Reverse Crunch', 'Core', 'Reps', 'Bodyweight', 'reverse-crunch'),
    ('Russian Twist', 'Core', 'Reps', 'Bodyweight', 'russian-twist'),
    ('Mountain Climber', 'Core', 'Timer', 'Bodyweight', 'mountain-climber'),
    ('Dead Bug', 'Core', 'Reps', 'Bodyweight', 'dead-bug'),
    ('Bird Dog', 'Core', 'Reps', 'Bodyweight', 'bird-dog'),
    ('Cable Woodchop', 'Core', 'Reps', 'Cable', 'cable-woodchop'),
    ('V-Up', 'Core', 'Reps', 'Bodyweight', 'v-up'),
    ('Flutter Kick', 'Core', 'Timer', 'Bodyweight', 'flutter-kick'),
    ('Hollow Body Hold', 'Core', 'Timer', 'Bodyweight', 'hollow-body-hold'),
    ('Bear Crawl', 'Core', 'Timer', 'Bodyweight', 'bear-crawl'),
    ('L-Sit Hold', 'Core', 'Timer', 'Bodyweight', 'l-sit-hold'),
    ('Copenhagen Plank', 'Core', 'Timer', 'Bodyweight', 'copenhagen-plank'),
    ('Dragon Flag', 'Core', 'Reps', 'Bodyweight', 'dragon-flag'),
    ('Deadlift', 'Back', 'Reps', 'Barbell', 'deadlift'),
    ('Sumo Deadlift', 'Back', 'Reps', 'Barbell', 'sumo-deadlift'),
    ('Trap Bar Deadlift', 'Back', 'Reps', 'Barbell', 'trap-bar-deadlift'),
    ('Shrug', 'Back', 'Reps', 'Dumbbell', 'shrug'),
    ('Farmer Carry', 'FullBody', 'Timer', 'Dumbbell', 'farmer-carry'),
    ('Battle Ropes', 'Shoulders', 'Timer', 'Cardio', 'battle-ropes'),
    ('Jump Rope', 'Legs', 'Timer', 'Cardio', 'jump-rope'),
    ('Treadmill Incline Walk', 'Legs', 'Timer', 'Cardio', 'treadmill-incline-walk'),
    ('Burpee', 'FullBody', 'Reps', 'Bodyweight', 'burpee'),
    ('Rowing', 'FullBody', 'Timer', 'Machine', 'rowing'),
    ('Assault Bike', 'FullBody', 'Timer', 'Machine', 'assault-bike')
) AS v("Name", "MuscleGroup", "TrackingType", "Equipment", "AnimationPath")
ON CONFLICT ("Name") DO NOTHING;
""");

            migrationBuilder.Sql("""
INSERT INTO "minimal-fitness"."WorkoutPlans" ("Name", "Description", "CreatedByUserId", "IsSeeded", "RestSeconds")
SELECT v."Name", v."Description", '00000000-0000-0000-0000-000000000001', TRUE, 75
FROM (VALUES
    ('Workout A (Upper Body)', 'Upper body compound and isolation movements with core work'),
    ('Workout B (Lower Body)', 'Lower body strength with core work')
) AS v("Name", "Description")
WHERE NOT EXISTS (
    SELECT 1
    FROM "minimal-fitness"."WorkoutPlans" wp
    WHERE wp."Name" = v."Name"
      AND wp."CreatedByUserId" = '00000000-0000-0000-0000-000000000001'
);
""");

            migrationBuilder.Sql("""
INSERT INTO "minimal-fitness"."WorkoutPlanExercises"
    ("WorkoutPlanId", "ExerciseId", "Section", "SortOrder", "DefaultSets", "DefaultReps", "DefaultWeight", "DefaultDurationSeconds")
SELECT wp."Id", e."Id", v."Section", v."SortOrder", v."DefaultSets", v."DefaultReps", v."DefaultWeight", v."DefaultDurationSeconds"
FROM (VALUES
    ('Workout A (Upper Body)', 'Decline Press', 'Upper Body', 0, 3, 10, 25.0, NULL),
    ('Workout A (Upper Body)', 'Lat Pulldown', 'Upper Body', 1, 3, 10, 60.0, NULL),
    ('Workout A (Upper Body)', 'Converging Shoulder Press', 'Upper Body', 2, 3, 10, 20.0, NULL),
    ('Workout A (Upper Body)', 'Tricep Extensions', 'Upper Body', 3, 3, 10, 60.0, NULL),
    ('Workout A (Upper Body)', 'Biceps Curls', 'Upper Body', 4, 3, 10, 30.0, NULL),
    ('Workout A (Upper Body)', 'Lat Row', 'Upper Body', 5, 3, 10, 55.0, NULL),
    ('Workout A (Upper Body)', 'Pull Ups', 'Upper Body', 6, 3, 5, NULL, NULL),
    ('Workout A (Upper Body)', 'Pallof Press', 'Abs', 7, 3, 12, 15.0, NULL),
    ('Workout A (Upper Body)', 'Toe to Toe Crunch', 'Abs', 8, 3, 20, NULL, NULL),
    ('Workout A (Upper Body)', 'V-Crunch', 'Abs', 9, 3, NULL, NULL, 60),
    ('Workout B (Lower Body)', 'Squats', 'Lower Body', 0, 3, 20, NULL, NULL),
    ('Workout B (Lower Body)', 'Leg Press', 'Lower Body', 1, 3, 15, 55.0, NULL),
    ('Workout B (Lower Body)', 'Hamstring Curls', 'Lower Body', 2, 3, 10, 35.0, NULL),
    ('Workout B (Lower Body)', 'Leg Extensions', 'Lower Body', 3, 3, 10, 40.0, NULL),
    ('Workout B (Lower Body)', 'Plank', 'Abs', 4, 3, NULL, NULL, 60),
    ('Workout B (Lower Body)', 'Hip Thrusts', 'Abs', 5, 3, 20, NULL, NULL),
    ('Workout B (Lower Body)', 'Hanging Knee Raises', 'Abs', 6, 3, 15, NULL, NULL)
) AS v("PlanName", "ExerciseName", "Section", "SortOrder", "DefaultSets", "DefaultReps", "DefaultWeight", "DefaultDurationSeconds")
INNER JOIN "minimal-fitness"."WorkoutPlans" wp ON wp."Name" = v."PlanName"
INNER JOIN "minimal-fitness"."Exercises" e ON e."Name" = v."ExerciseName"
WHERE NOT EXISTS (
    SELECT 1
    FROM "minimal-fitness"."WorkoutPlanExercises" wpe
    WHERE wpe."WorkoutPlanId" = wp."Id"
      AND wpe."ExerciseId" = e."Id"
      AND wpe."SortOrder" = v."SortOrder"
);
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DELETE FROM "minimal-fitness"."WorkoutPlanExercises"
WHERE "WorkoutPlanId" IN (
    SELECT "Id"
    FROM "minimal-fitness"."WorkoutPlans"
    WHERE "CreatedByUserId" = '00000000-0000-0000-0000-000000000001'
      AND "IsSeeded" = TRUE
      AND "Name" IN ('Workout A (Upper Body)', 'Workout B (Lower Body)')
);
""");

            migrationBuilder.Sql("""
DELETE FROM "minimal-fitness"."WorkoutPlans"
WHERE "CreatedByUserId" = '00000000-0000-0000-0000-000000000001'
  AND "IsSeeded" = TRUE
  AND "Name" IN ('Workout A (Upper Body)', 'Workout B (Lower Body)');
""");
        }
    }
}
