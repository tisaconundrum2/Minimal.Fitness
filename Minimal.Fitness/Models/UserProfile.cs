using Microsoft.AspNetCore.Identity;
using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Models;

public class UserProfile
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public Gender Gender { get; set; }
    public int DefaultRestSeconds { get; set; } = 75;

    public IdentityUser User { get; set; } = null!;
}
