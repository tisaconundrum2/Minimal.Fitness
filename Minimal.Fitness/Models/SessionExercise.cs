using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Models;

public class SessionExercise
{
    public int Id { get; set; }
    public int WorkoutSessionId { get; set; }
    public int ExerciseId { get; set; }
    public string? ExerciseNameSnapshot { get; set; }
    public string? SectionSnapshot { get; set; }
    public int? PlannedSetsSnapshot { get; set; }
    public int? PlannedRepsSnapshot { get; set; }
    public decimal? PlannedWeightSnapshot { get; set; }
    public int? PlannedDurationSecondsSnapshot { get; set; }
    public int SortOrder { get; set; }
    public Difficulty? Difficulty { get; set; }
    public bool Skipped { get; set; }
    public bool Deferred { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public WorkoutSession WorkoutSession { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
    public ICollection<ExerciseSet> Sets { get; set; } = [];
}
