namespace Minimal.Fitness.Models;

public class ExerciseSet
{
    public int Id { get; set; }
    public int SessionExerciseId { get; set; }
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public decimal? Weight { get; set; }
    public int? DurationSeconds { get; set; }
    public DateTime CompletedAtUtc { get; set; }

    public SessionExercise SessionExercise { get; set; } = null!;
}
