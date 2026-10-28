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
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        IQueryable<WorkoutPlan> query = db.WorkoutPlans
            .Include(p => p.Exercises);

        if (isAuthenticated)
        {
            query = query.Where(p => p.IsSeeded || p.CreatedByUserId == userId);
        }
        else
        {
            query = query.Where(p => p.IsSeeded);
        }

        var plans = await query
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(plans);
    }

    public async Task<IActionResult> Details(int id)
    {
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var plan = await db.WorkoutPlans
            .Include(p => p.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan == null) return NotFound();

        if (!isAuthenticated && !plan.IsSeeded)
            return NotFound();

        if (isAuthenticated && !plan.IsSeeded && plan.CreatedByUserId != userId)
            return Forbid();

        if (isAuthenticated)
        {
            var exerciseIds = plan.Exercises.Select(e => e.ExerciseId).ToList();

            // Most recent completed session's ratings for this plan
            var latestSession = await db.WorkoutSessions
                .Where(s => s.UserId == userId && s.WorkoutPlanId == plan.Id && s.CompletedAtUtc != null)
                .OrderByDescending(s => s.StartedAtUtc)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            var bumpSuggestions = new Dictionary<int, int>();
            if (latestSession != 0 && plan.LastBumpedSessionId != latestSession)
            {
                var ratings = await db.SessionExercises
                    .Where(se => se.WorkoutSessionId == latestSession && !se.Skipped && se.Difficulty.HasValue)
                    .Select(se => new { se.ExerciseId, se.Difficulty })
                    .ToListAsync();

                foreach (var r in ratings)
                {
                    bumpSuggestions[r.ExerciseId] = r.Difficulty switch
                    {
                        Difficulty.TooEasy => 1,
                        Difficulty.Struggled => -1,
                        _ => 0
                    };
                }
            }

            ViewBag.BumpSuggestions = bumpSuggestions;
        }

        return View(plan);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> BumpLoad(int planId, int workoutPlanExerciseId, int direction)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var pe = await db.WorkoutPlanExercises
            .Include(x => x.Exercise)
            .Include(x => x.WorkoutPlan)
            .FirstOrDefaultAsync(x => x.Id == workoutPlanExerciseId && x.WorkoutPlanId == planId);
        if (pe == null) return NotFound();

        if (!pe.WorkoutPlan.IsSeeded && pe.WorkoutPlan.CreatedByUserId != userId)
            return Forbid();

        ApplyBump(pe, direction);

        var latestSessionId = await db.WorkoutSessions
            .Where(s => s.UserId == userId && s.WorkoutPlanId == planId && s.CompletedAtUtc != null)
            .OrderByDescending(s => s.StartedAtUtc)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();
        if (latestSessionId != 0)
            pe.WorkoutPlan.LastBumpedSessionId = latestSessionId;

        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = planId });
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> BumpAll(int planId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var plan = await db.WorkoutPlans.FindAsync(planId);
        if (plan == null) return NotFound();
        if (!plan.IsSeeded && plan.CreatedByUserId != userId) return Forbid();

        var latestSessionId = await db.WorkoutSessions
            .Where(s => s.UserId == userId && s.WorkoutPlanId == planId && s.CompletedAtUtc != null)
            .OrderByDescending(s => s.StartedAtUtc)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();
        if (latestSessionId == 0) return RedirectToAction(nameof(Details), new { id = planId });

        var ratings = await db.SessionExercises
            .Where(se => se.WorkoutSessionId == latestSessionId && !se.Skipped && se.Difficulty.HasValue)
            .Select(se => new { se.ExerciseId, se.Difficulty })
            .ToListAsync();

        var planExercises = await db.WorkoutPlanExercises
            .Include(x => x.Exercise)
            .Where(x => x.WorkoutPlanId == planId)
            .ToListAsync();

        foreach (var r in ratings)
        {
            var direction = r.Difficulty switch { Difficulty.TooEasy => 1, Difficulty.Struggled => -1, _ => 0 };
            if (direction == 0) continue;
            var pe = planExercises.FirstOrDefault(x => x.ExerciseId == r.ExerciseId);
            if (pe != null) ApplyBump(pe, direction);
        }

        plan.LastBumpedSessionId = latestSessionId;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = planId });
    }

    private static void ApplyBump(WorkoutPlanExercise pe, int direction)
    {
        if (pe.Exercise.TrackingType == TrackingType.Timer)
        {
            var inc = direction > 0 ? 5 : -5;
            pe.DefaultDurationSeconds = Math.Max(5, (pe.DefaultDurationSeconds ?? 30) + inc);
        }
        else if (pe.DefaultWeight.GetValueOrDefault() > 0)
        {
            var inc = direction > 0 ? 5m : -5m;
            pe.DefaultWeight = Math.Max(0, pe.DefaultWeight!.Value + inc);
        }
        else
        {
            var reps = pe.DefaultReps ?? 10;
            var inc = direction > 0 ? (reps >= 10 ? 5 : 1) : (reps > 10 ? -5 : -1);
            pe.DefaultReps = Math.Max(1, reps + inc);
        }
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        ViewBag.Exercises = await db.Exercises.OrderBy(e => e.Name).ToListAsync();
        return View(new WorkoutPlan());
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, int[] exerciseIds, string[] sections, int[] sets, int?[] reps, decimal?[] weights, int?[] durations, int? restSeconds)
    {
        if (string.IsNullOrWhiteSpace(name) || exerciseIds.Length == 0)
        {
            ViewBag.Exercises = await db.Exercises.OrderBy(e => e.Name).ToListAsync();
            ModelState.AddModelError("", "Name and at least one exercise are required.");
            return View(new WorkoutPlan { Name = name ?? "", Description = description, RestSeconds = restSeconds ?? 75 });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var plan = new WorkoutPlan
        {
            Name = name,
            Description = description,
            RestSeconds = Math.Clamp(restSeconds ?? 75, 15, 180),
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
    public async Task<IActionResult> Edit(int id, string name, string? description, int[] exerciseIds, string[] sections, int[] sets, int?[] reps, decimal?[] weights, int?[] durations, int? restSeconds)
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
        plan.RestSeconds = Math.Clamp(restSeconds ?? plan.RestSeconds, 15, 180);
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

    [Authorize]
    public async Task<IActionResult> Duplicate(int id)
    {
        var source = await db.WorkoutPlans
            .Include(p => p.Exercises.OrderBy(e => e.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id);
        if (source == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var copy = new WorkoutPlan
        {
            Name = source.Name + " (Copy)",
            Description = source.Description,
            RestSeconds = source.RestSeconds,
            CreatedByUserId = userId,
            IsSeeded = false
        };
        db.WorkoutPlans.Add(copy);
        await db.SaveChangesAsync();

        foreach (var pe in source.Exercises)
        {
            db.WorkoutPlanExercises.Add(new WorkoutPlanExercise
            {
                WorkoutPlanId = copy.Id,
                ExerciseId = pe.ExerciseId,
                Section = pe.Section,
                SortOrder = pe.SortOrder,
                DefaultSets = pe.DefaultSets,
                DefaultReps = pe.DefaultReps,
                DefaultWeight = pe.DefaultWeight,
                DefaultDurationSeconds = pe.DefaultDurationSeconds
            });
        }
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = copy.Id });
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var plan = await db.WorkoutPlans
            .Include(p => p.Sessions)
                .ThenInclude(s => s.Exercises)
            .FirstOrDefaultAsync(p => p.Id == id && (p.IsSeeded || p.CreatedByUserId == userId));
        if (plan == null) return NotFound();

        db.SessionExercises.RemoveRange(plan.Sessions.SelectMany(s => s.Exercises));
        db.WorkoutSessions.RemoveRange(plan.Sessions);
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
