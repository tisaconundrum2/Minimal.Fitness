using Microsoft.AspNetCore.Identity;

namespace Minimal.Fitness.Models;

public class WorkoutSession
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int WorkoutPlanId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public IdentityUser User { get; set; } = null!;
    public WorkoutPlan WorkoutPlan { get; set; } = null!;
    public ICollection<SessionExercise> Exercises { get; set; } = [];
}
