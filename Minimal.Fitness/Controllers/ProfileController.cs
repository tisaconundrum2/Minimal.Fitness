using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Data;
using Minimal.Fitness.Models;
using Minimal.Fitness.Models.Enums;
using System.Security.Claims;

namespace Minimal.Fitness.Controllers;

[Authorize]
public class ProfileController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new UserProfile { UserId = userId, Gender = Gender.Other, DefaultRestSeconds = 60 };
            db.UserProfiles.Add(profile);
            await db.SaveChangesAsync();
        }
        return View(profile);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Gender gender, int defaultRestSeconds)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound();
        profile.Gender = gender;
        profile.DefaultRestSeconds = Math.Clamp(defaultRestSeconds, 15, 180);
        await db.SaveChangesAsync();
        TempData["Message"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }
}
