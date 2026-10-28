using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Data;
using Minimal.Fitness.Models;
using Minimal.Fitness.Models.Enums;
using System.Security.Claims;

namespace Minimal.Fitness.Controllers;

[Authorize]
public class WorkoutSessionController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Start(int planId)
    {
        var plan = await db.WorkoutPlans
            .Include(p => p.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == planId);
        if (plan == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = new WorkoutSession
        {
            UserId = userId,
            WorkoutPlanId = planId,
            PlanNameSnapshot = plan.Name,
            StartedAtUtc = DateTime.UtcNow
        };
        db.WorkoutSessions.Add(session);
        await db.SaveChangesAsync();

        foreach (var pe in plan.Exercises)
        {
            db.SessionExercises.Add(new SessionExercise
            {
                WorkoutSessionId = session.Id,
                ExerciseId = pe.ExerciseId,
                ExerciseNameSnapshot = pe.Exercise.Name,
                SectionSnapshot = pe.Section,
                PlannedSetsSnapshot = pe.DefaultSets,
                PlannedRepsSnapshot = pe.DefaultReps,
                PlannedWeightSnapshot = pe.DefaultWeight,
                PlannedDurationSecondsSnapshot = pe.DefaultDurationSeconds,
                SortOrder = pe.SortOrder
            });
        }
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Active), new { sessionId = session.Id, exerciseIndex = 0 });
    }

    public async Task<IActionResult> Active(int sessionId, int exerciseIndex = 0)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = await db.WorkoutSessions
            .Include(s => s.WorkoutPlan)
            .Include(s => s.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(se => se.Exercise)
            .Include(s => s.Exercises)
                .ThenInclude(se => se.Sets.OrderBy(s => s.SetNumber))
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null) return NotFound();
        if (session.CompletedAtUtc.HasValue) return RedirectToAction(nameof(Summary), new { sessionId });

        var exercises = session.Exercises.OrderBy(e => e.SortOrder).ToList();
        if (exerciseIndex < 0 || exerciseIndex >= exercises.Count)
        {
            await MarkCompletedIfFinishedAsync(session);
            return RedirectToAction(nameof(Summary), new { sessionId });
        }

        var current = exercises[exerciseIndex];
        var planExercise = await db.WorkoutPlanExercises
            .FirstOrDefaultAsync(pe => pe.WorkoutPlanId == session.WorkoutPlanId && pe.ExerciseId == current.ExerciseId);

        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        var restSeconds = session.WorkoutPlan.RestSeconds > 0 ? session.WorkoutPlan.RestSeconds : (profile?.DefaultRestSeconds ?? 75);

        ViewBag.SessionId = sessionId;
        ViewBag.ExerciseIndex = exerciseIndex;
        ViewBag.TotalExercises = exercises.Count;
        ViewBag.PlanExercise = planExercise;
        ViewBag.DefaultRestSeconds = restSeconds;
        ViewBag.PlanName = session.PlanNameSnapshot ?? session.WorkoutPlan.Name;

        return View(current);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteSet(int sessionExerciseId, int setNumber, int? reps, decimal? weight, int? durationSeconds)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var se = await db.SessionExercises
            .Include(x => x.WorkoutSession)
            .FirstOrDefaultAsync(x => x.Id == sessionExerciseId && x.WorkoutSession.UserId == userId);
        if (se == null) return NotFound();

        var existing = await db.ExerciseSets
            .FirstOrDefaultAsync(s => s.SessionExerciseId == sessionExerciseId && s.SetNumber == setNumber);
        if (existing != null)
            return Json(new { ok = true, alreadyDone = true });

        db.ExerciseSets.Add(new ExerciseSet
        {
            SessionExerciseId = sessionExerciseId,
            SetNumber = setNumber,
            Reps = reps,
            Weight = weight,
            DurationSeconds = durationSeconds,
            CompletedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return Json(new { ok = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RateExercise(int sessionExerciseId, Difficulty difficulty, int sessionId, int nextIndex)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var se = await db.SessionExercises
            .Include(x => x.WorkoutSession)
            .FirstOrDefaultAsync(x => x.Id == sessionExerciseId && x.WorkoutSession.UserId == userId);
        if (se == null) return NotFound();

        se.Difficulty = difficulty;
        se.CompletedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Active), new { sessionId, exerciseIndex = nextIndex });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SkipExercise(int sessionExerciseId, int sessionId, int nextIndex, bool confirm = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var se = await db.SessionExercises
            .Include(x => x.WorkoutSession)
            .FirstOrDefaultAsync(x => x.Id == sessionExerciseId && x.WorkoutSession.UserId == userId);
        if (se == null) return NotFound();

        if (!se.Deferred)
        {
            // First skip: move exercise to the end of the line
            var maxSort = await db.SessionExercises
                .Where(x => x.WorkoutSessionId == sessionId)
                .MaxAsync(x => x.SortOrder);
            se.SortOrder = maxSort + 1;
            se.Deferred = true;
            await db.SaveChangesAsync();

            // Stay at the same index (the next exercise has shifted into this slot)
            return RedirectToAction(nameof(Active), new { sessionId, exerciseIndex = nextIndex - 1 });
        }

        // Second skip: confirmed removal
        if (!confirm)
        {
            // Should not happen (modal handles this client-side), but guard anyway
            return RedirectToAction(nameof(Active), new { sessionId, exerciseIndex = nextIndex - 1 });
        }

        se.Skipped = true;
        se.CompletedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Recalculate the correct index since this exercise is at the end
        var exercises = await db.SessionExercises
            .Where(x => x.WorkoutSessionId == sessionId)
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Id)
            .ToListAsync();
        var currentIndex = exercises.IndexOf(sessionExerciseId);
        var newNextIndex = currentIndex >= 0 ? currentIndex : exercises.Count;

        return RedirectToAction(nameof(Active), new { sessionId, exerciseIndex = newNextIndex });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int sessionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = await db.WorkoutSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session == null) return NotFound();

        session.CompletedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Summary), new { sessionId });
    }

    public async Task<IActionResult> Summary(int sessionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = await db.WorkoutSessions
            .Include(s => s.WorkoutPlan)
            .Include(s => s.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(se => se.Exercise)
            .Include(s => s.Exercises)
                .ThenInclude(se => se.Sets.OrderBy(s => s.SetNumber))
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session == null) return NotFound();

        await MarkCompletedIfFinishedAsync(session);

        // Build per-exercise weight suggestions based on recent history
        var planExercises = await db.WorkoutPlanExercises
                .Where(pe => pe.WorkoutPlanId == session.WorkoutPlanId)
                .ToDictionaryAsync(pe => pe.ExerciseId);

        var exerciseIds = session.Exercises.Select(e => e.ExerciseId).ToList();

        // Get recent completed session ratings for this plan per exercise.
        var recentRatings = await db.SessionExercises
            .Where(se => se.WorkoutSession.UserId == userId
                && se.WorkoutSession.WorkoutPlanId == session.WorkoutPlanId
                && se.WorkoutSession.CompletedAtUtc != null
                && exerciseIds.Contains(se.ExerciseId))
            .OrderByDescending(se => se.WorkoutSession.StartedAtUtc)
            .Select(se => new { se.ExerciseId, se.Difficulty, se.Skipped, se.WorkoutSession.StartedAtUtc })
            .ToListAsync();

        // suggestBump: +1 = suggest bump up, -1 = suggest bump down, 0 = no suggestion
        var bumpSuggestions = new Dictionary<int, int>();
        foreach (var exerciseId in exerciseIds)
        {
            var ratings = recentRatings
                .Where(r => r.ExerciseId == exerciseId && !r.Skipped && r.Difficulty.HasValue)
                .Take(3)
                .Select(r => r.Difficulty!.Value)
                .ToList();

            if (ratings.Count >= 2 && ratings.Take(2).All(d => d == Difficulty.TooEasy || d == Difficulty.GotIt))
                bumpSuggestions[exerciseId] = 1;
            else if (ratings.Count >= 1 && ratings[0] == Difficulty.Struggled)
                bumpSuggestions[exerciseId] = -1;
            else
                bumpSuggestions[exerciseId] = 0;
        }

        ViewBag.PlanExercises = planExercises;
        ViewBag.BumpSuggestions = bumpSuggestions;

        return View(session);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BumpWeight(int sessionId, int workoutPlanExerciseId, int direction)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = await db.WorkoutSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session == null) return NotFound();

        var pe = await db.WorkoutPlanExercises
            .Include(x => x.WorkoutPlan)
            .FirstOrDefaultAsync(x => x.Id == workoutPlanExerciseId && x.WorkoutPlanId == session.WorkoutPlanId);
        if (pe == null) return NotFound();

        var increment = direction > 0 ? 5m : -5m;
        pe.DefaultWeight = Math.Max(0, (pe.DefaultWeight ?? 0) + increment);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Summary), new { sessionId });
    }

    public async Task<IActionResult> History()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sessions = await db.WorkoutSessions
            .Include(s => s.WorkoutPlan)
            .Include(s => s.Exercises)
                .ThenInclude(e => e.Sets)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAtUtc)
            .ToListAsync();
        return View(sessions);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = await db.WorkoutSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (session == null)
            return NotFound();

        db.WorkoutSessions.Remove(session);
        await db.SaveChangesAsync();

        return Ok();
    }

    private async Task MarkCompletedIfFinishedAsync(WorkoutSession session)
    {
        if (session.CompletedAtUtc.HasValue)
            return;

        var allExercisesFinished = session.Exercises.All(e => e.Skipped || e.Difficulty.HasValue);
        if (!allExercisesFinished)
            return;

        session.CompletedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
