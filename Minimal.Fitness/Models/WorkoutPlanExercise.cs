namespace Minimal.Fitness.Models;

public class WorkoutPlanExercise
{
    public int Id { get; set; }
    public int WorkoutPlanId { get; set; }
    public int ExerciseId { get; set; }
    public string Section { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int DefaultSets { get; set; }
    public int? DefaultReps { get; set; }
    public decimal? DefaultWeight { get; set; }
    public int? DefaultDurationSeconds { get; set; }

    public WorkoutPlan WorkoutPlan { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
