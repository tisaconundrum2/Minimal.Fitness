using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Data;
using Minimal.Fitness.Models;
using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Controllers;

[Authorize]
public class ProfileController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        profile ??= await CreateDefaultProfile(userId);
        return View(profile);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Gender gender, int defaultRestSeconds)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        profile ??= await CreateDefaultProfile(userId);

        profile.Gender = gender;
        profile.DefaultRestSeconds = Math.Clamp(defaultRestSeconds, 15, 180);
        await db.SaveChangesAsync();

        TempData["Message"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<UserProfile> CreateDefaultProfile(string userId)
    {
        var profile = new UserProfile { UserId = userId, Gender = Gender.Other, DefaultRestSeconds = 60 };
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile;
    }
}
