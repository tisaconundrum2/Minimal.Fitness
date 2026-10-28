using Microsoft.AspNetCore.Identity;

namespace Minimal.Fitness.Models;

public class WorkoutPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RestSeconds { get; set; } = 75;
    public string? CreatedByUserId { get; set; }
    public bool IsSeeded { get; set; }
    public int? LastBumpedSessionId { get; set; }

    public IdentityUser? CreatedByUser { get; set; }
    public ICollection<WorkoutPlanExercise> Exercises { get; set; } = [];
    public ICollection<WorkoutSession> Sessions { get; set; } = [];
}
