using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Data;
using Minimal.Fitness.Models;

namespace Minimal.Fitness.Controllers;

public class WorkoutPlansController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var plans = await db.WorkoutPlans
            .Where(p => p.IsSeeded || p.CreatedByUserId == userId)
            .OrderBy(p => p.IsSeeded ? 0 : 1)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return View(plans);
    }

    public async Task<IActionResult> Details(int id)
    {
        var plan = await db.WorkoutPlans
            .Include(p => p.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null)
            return NotFound();

        return View(plan);
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        ViewBag.Exercises = await db.Exercises.OrderBy(e => e.Name).ToListAsync();
        return View(new WorkoutPlan { Name = "" });
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkoutPlan plan, int[] exerciseIds, string[] sections, int[] sortOrders, int[] defaultSets, int?[] defaultReps, decimal?[] defaultWeights, int?[] defaultDurations)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        plan.CreatedByUserId = userId;
        plan.IsSeeded = false;

        for (int i = 0; i < exerciseIds.Length; i++)
        {
            plan.Exercises.Add(new WorkoutPlanExercise
            {
                ExerciseId = exerciseIds[i],
                Section = i < sections.Length ? sections[i] : "Main",
                SortOrder = i < sortOrders.Length ? sortOrders[i] : i,
                DefaultSets = i < defaultSets.Length ? defaultSets[i] : 3,
                DefaultReps = i < defaultReps.Length ? defaultReps[i] : null,
                DefaultWeight = i < defaultWeights.Length ? defaultWeights[i] : null,
                DefaultDurationSeconds = i < defaultDurations.Length ? defaultDurations[i] : null,
            });
        }

        db.WorkoutPlans.Add(plan);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = plan.Id });
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var plan = await db.WorkoutPlans
            .Include(p => p.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsSeeded && p.CreatedByUserId == userId);

        if (plan is null)
            return NotFound();

        ViewBag.Exercises = await db.Exercises.OrderBy(e => e.Name).ToListAsync();
        return View(plan);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name, string? description, int[] exerciseIds, string[] sections, int[] sortOrders, int[] defaultSets, int?[] defaultReps, decimal?[] defaultWeights, int?[] defaultDurations)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var plan = await db.WorkoutPlans
            .Include(p => p.Exercises)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsSeeded && p.CreatedByUserId == userId);

        if (plan is null)
            return NotFound();

        plan.Name = name;
        plan.Description = description;
        db.WorkoutPlanExercises.RemoveRange(plan.Exercises);

        for (int i = 0; i < exerciseIds.Length; i++)
        {
            plan.Exercises.Add(new WorkoutPlanExercise
            {
                ExerciseId = exerciseIds[i],
                Section = i < sections.Length ? sections[i] : "Main",
                SortOrder = i < sortOrders.Length ? sortOrders[i] : i,
                DefaultSets = i < defaultSets.Length ? defaultSets[i] : 3,
                DefaultReps = i < defaultReps.Length ? defaultReps[i] : null,
                DefaultWeight = i < defaultWeights.Length ? defaultWeights[i] : null,
                DefaultDurationSeconds = i < defaultDurations.Length ? defaultDurations[i] : null,
            });
        }

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var plan = await db.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsSeeded && p.CreatedByUserId == userId);

        if (plan is null)
            return NotFound();

        db.WorkoutPlans.Remove(plan);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
