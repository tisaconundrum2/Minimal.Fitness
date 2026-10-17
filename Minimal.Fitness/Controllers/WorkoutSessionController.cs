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
            .FirstOrDefaultAsync(p => p.Id == planId);
        if (plan == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = new WorkoutSession
        {
            UserId = userId,
            WorkoutPlanId = planId,
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
            return RedirectToAction(nameof(Summary), new { sessionId });

        var current = exercises[exerciseIndex];
        var planExercise = await db.WorkoutPlanExercises
            .FirstOrDefaultAsync(pe => pe.WorkoutPlanId == session.WorkoutPlanId && pe.ExerciseId == current.ExerciseId);

        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        ViewBag.SessionId = sessionId;
        ViewBag.ExerciseIndex = exerciseIndex;
        ViewBag.TotalExercises = exercises.Count;
        ViewBag.PlanExercise = planExercise;
        ViewBag.DefaultRestSeconds = profile?.DefaultRestSeconds ?? 75;
        ViewBag.PlanName = session.WorkoutPlan.Name;

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

        return View(session);
    }

    public async Task<IActionResult> History()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sessions = await db.WorkoutSessions
            .Include(s => s.WorkoutPlan)
            .Include(s => s.Exercises)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAtUtc)
            .ToListAsync();
        return View(sessions);
    }
}
