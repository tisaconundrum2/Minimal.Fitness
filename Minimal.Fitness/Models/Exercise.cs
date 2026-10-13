using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Models;

public class Exercise
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? AnimationPath { get; set; }
    public TrackingType TrackingType { get; set; }
    public MuscleGroup MuscleGroup { get; set; }
    public string? Equipment { get; set; }

    public ICollection<WorkoutPlanExercise> WorkoutPlanExercises { get; set; } = [];
}
