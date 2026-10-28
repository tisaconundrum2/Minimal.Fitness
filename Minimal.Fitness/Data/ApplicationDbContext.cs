using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Models;

namespace Minimal.Fitness.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<WorkoutPlanExercise> WorkoutPlanExercises => Set<WorkoutPlanExercise>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<SessionExercise> SessionExercises => Set<SessionExercise>();
    public DbSet<ExerciseSet> ExerciseSets => Set<ExerciseSet>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("minimal-fitness");
        base.OnModelCreating(builder);

        builder.Entity<Exercise>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.TrackingType).HasConversion<string>();
            e.Property(x => x.MuscleGroup).HasConversion<string>();
        });

        builder.Entity<WorkoutPlan>(e =>
        {
            e.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<WorkoutPlanExercise>(e =>
        {
            e.HasOne(x => x.WorkoutPlan)
                .WithMany(p => p.Exercises)
                .HasForeignKey(x => x.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Exercise)
                .WithMany(ex => ex.WorkoutPlanExercises)
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.DefaultWeight).HasPrecision(8, 2);
        });

        builder.Entity<WorkoutSession>(e =>
        {
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.WorkoutPlan)
                .WithMany(p => p.Sessions)
                .HasForeignKey(x => x.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SessionExercise>(e =>
        {
            e.HasOne(x => x.WorkoutSession)
                .WithMany(s => s.Exercises)
                .HasForeignKey(x => x.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Exercise)
                .WithMany()
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.Difficulty).HasConversion<string>();
            e.Property(x => x.PlannedWeightSnapshot).HasPrecision(8, 2);
            e.HasIndex(x => new { x.WorkoutSessionId, x.ExerciseId });
        });

        builder.Entity<ExerciseSet>(e =>
        {
            e.HasOne(x => x.SessionExercise)
                .WithMany(se => se.Sets)
                .HasForeignKey(x => x.SessionExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.Weight).HasPrecision(8, 2);
        });

        builder.Entity<UserProfile>(e =>
        {
            e.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<UserProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId).IsUnique();
            e.Property(x => x.Gender).HasConversion<string>();
        });
    }
}
