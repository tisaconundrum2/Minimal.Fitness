using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Data;
using Minimal.Fitness.Models;
using Minimal.Fitness.Models.Enums;
using System.Security.Claims;

namespace Minimal.Fitness.Controllers;

public class WorkoutPlansController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var plans = await db.WorkoutPlans
            .Include(p => p.Exercises)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return View(plans);
    }

    public async Task<IActionResult> Details(int id)
    {
        var plan = await db.WorkoutPlans
            .Include(p => p.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null) return NotFound();
        return View(plan);
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        ViewBag.Exercises = await db.Exercises.OrderBy(e => e.Name).ToListAsync();
        return View(new WorkoutPlan());
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, int[] exerciseIds, string[] sections, int[] sets, int?[] reps, decimal?[] weights, int?[] durations)
    {
        if (string.IsNullOrWhiteSpace(name) || exerciseIds.Length == 0)
        {
            ViewBag.Exercises = await db.Exercises.OrderBy(e => e.Name).ToListAsync();
            ModelState.AddModelError("", "Name and at least one exercise are required.");
            return View(new WorkoutPlan { Name = name ?? "", Description = description });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var plan = new WorkoutPlan
        {
            Name = name,
            Description = description,
            CreatedByUserId = userId,
            IsSeeded = false
        };
        db.WorkoutPlans.Add(plan);
        await db.SaveChangesAsync();

        for (int i = 0; i < exerciseIds.Length; i++)
        {
            db.WorkoutPlanExercises.Add(new WorkoutPlanExercise
            {
                WorkoutPlanId = plan.Id,
                ExerciseId = exerciseIds[i],
                Section = i < sections.Length && !string.IsNullOrWhiteSpace(sections[i]) ? sections[i] : "Main",
                SortOrder = i,
                DefaultSets = i < sets.Length ? sets[i] : 3,
                DefaultReps = i < reps.Length ? reps[i] : null,
                DefaultWeight = i < weights.Length ? weights[i] : null,
                DefaultDurationSeconds = i < durations.Length ? durations[i] : null
            });
        }
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = plan.Id });
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var plan = await db.WorkoutPlans
            .Include(p => p.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsSeeded && p.CreatedByUserId == userId);
        if (plan == null) return NotFound();

        ViewBag.Exercises = await db.Exercises.OrderBy(e => e.Name).ToListAsync();
        return View(plan);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name, string? description, int[] exerciseIds, string[] sections, int[] sets, int?[] reps, decimal?[] weights, int?[] durations)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var plan = await db.WorkoutPlans
            .Include(p => p.Exercises)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsSeeded && p.CreatedByUserId == userId);
        if (plan == null) return NotFound();

        if (string.IsNullOrWhiteSpace(name) || exerciseIds.Length == 0)
        {
            ViewBag.Exercises = await db.Exercises.OrderBy(e => e.Name).ToListAsync();
            ModelState.AddModelError("", "Name and at least one exercise are required.");
            return View(plan);
        }

        plan.Name = name;
        plan.Description = description;
        db.WorkoutPlanExercises.RemoveRange(plan.Exercises);

        for (int i = 0; i < exerciseIds.Length; i++)
        {
            db.WorkoutPlanExercises.Add(new WorkoutPlanExercise
            {
                WorkoutPlanId = plan.Id,
                ExerciseId = exerciseIds[i],
                Section = i < sections.Length && !string.IsNullOrWhiteSpace(sections[i]) ? sections[i] : "Main",
                SortOrder = i,
                DefaultSets = i < sets.Length ? sets[i] : 3,
                DefaultReps = i < reps.Length ? reps[i] : null,
                DefaultWeight = i < weights.Length ? weights[i] : null,
                DefaultDurationSeconds = i < durations.Length ? durations[i] : null
            });
        }
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = plan.Id });
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var plan = await db.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsSeeded && p.CreatedByUserId == userId);
        if (plan == null) return NotFound();

        db.WorkoutPlans.Remove(plan);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> RandomPlan()
    {
        var exercises = await db.Exercises.ToListAsync();
        if (exercises.Count < 4)
            return RedirectToAction(nameof(Index));

        var rng = System.Random.Shared;
        var muscleGroups = exercises.Select(e => e.MuscleGroup).Distinct().ToList();

        // Pick 5-8 exercises spread across muscle groups
        var picked = new List<Exercise>();
        var shuffledGroups = muscleGroups.OrderBy(_ => rng.Next()).ToList();
        foreach (var group in shuffledGroups)
        {
            var candidates = exercises.Where(e => e.MuscleGroup == group && !picked.Contains(e)).ToList();
            if (candidates.Count > 0)
                picked.Add(candidates[rng.Next(candidates.Count)]);
            if (picked.Count >= 6) break;
        }
        // Fill to at least 5 if we have fewer groups
        while (picked.Count < 5 && picked.Count < exercises.Count)
        {
            var candidate = exercises[rng.Next(exercises.Count)];
            if (!picked.Contains(candidate))
                picked.Add(candidate);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var plan = new WorkoutPlan
        {
            Name = $"Quick Mix — {DateTime.UtcNow:MMM d}",
            Description = "Randomly generated full-body workout",
            CreatedByUserId = userId,
            IsSeeded = false
        };
        db.WorkoutPlans.Add(plan);
        await db.SaveChangesAsync();

        for (int i = 0; i < picked.Count; i++)
        {
            var ex = picked[i];
            db.WorkoutPlanExercises.Add(new WorkoutPlanExercise
            {
                WorkoutPlanId = plan.Id,
                ExerciseId = ex.Id,
                Section = "Workout",
                SortOrder = i,
                DefaultSets = 3,
                DefaultReps = ex.TrackingType == TrackingType.Timer ? null : 10,
                DefaultDurationSeconds = ex.TrackingType == TrackingType.Timer ? 60 : null
            });
        }
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = plan.Id });
    }
}
