using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Data;
using Minimal.Fitness.Models;
using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Controllers;

[Authorize]
public class WorkoutSessionController(ApplicationDbContext db) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int planId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var plan = await db.WorkoutPlans
            .Include(p => p.Exercises.OrderBy(e => e.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan is null)
            return NotFound();

        var session = new WorkoutSession
        {
            UserId = userId,
            WorkoutPlanId = planId,
            StartedAtUtc = DateTime.UtcNow,
        };

        foreach (var pe in plan.Exercises)
        {
            session.Exercises.Add(new SessionExercise
            {
                ExerciseId = pe.ExerciseId,
                SortOrder = pe.SortOrder,
            });
        }

        db.WorkoutSessions.Add(session);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Active), new { id = session.Id });
    }

    public async Task<IActionResult> Active(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var session = await db.WorkoutSessions
            .Include(s => s.WorkoutPlan)
                .ThenInclude(p => p.Exercises.OrderBy(e => e.SortOrder))
                    .ThenInclude(pe => pe.Exercise)
            .Include(s => s.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(se => se.Exercise)
            .Include(s => s.Exercises)
                .ThenInclude(se => se.Sets)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (session is null)
            return NotFound();

        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        ViewBag.RestSeconds = profile?.DefaultRestSeconds ?? 75;

        var planExercises = session.WorkoutPlan.Exercises.OrderBy(e => e.SortOrder).ToList();
        ViewBag.PlanExercises = planExercises;

        return View(session);
    }

    [HttpPost]
    public async Task<IActionResult> CompleteSet([FromBody] CompleteSetRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionExercise = await db.SessionExercises
            .Include(se => se.Sets)
            .Include(se => se.WorkoutSession)
            .FirstOrDefaultAsync(se => se.Id == request.SessionExerciseId && se.WorkoutSession.UserId == userId);

        if (sessionExercise is null)
            return NotFound();

        var set = new ExerciseSet
        {
            SessionExerciseId = sessionExercise.Id,
            SetNumber = sessionExercise.Sets.Count + 1,
            Reps = request.Reps,
            Weight = request.Weight,
            DurationSeconds = request.DurationSeconds,
            CompletedAtUtc = DateTime.UtcNow,
        };

        db.ExerciseSets.Add(set);
        await db.SaveChangesAsync();

        return Json(new { setNumber = set.SetNumber, id = set.Id });
    }

    [HttpPost]
    public async Task<IActionResult> RateExercise([FromBody] RateExerciseRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionExercise = await db.SessionExercises
            .Include(se => se.WorkoutSession)
            .FirstOrDefaultAsync(se => se.Id == request.SessionExerciseId && se.WorkoutSession.UserId == userId);

        if (sessionExercise is null)
            return NotFound();

        sessionExercise.Difficulty = request.Difficulty;
        sessionExercise.CompletedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var session = await db.WorkoutSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (session is null)
            return NotFound();

        session.CompletedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(History));
    }

    public async Task<IActionResult> History()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessions = await db.WorkoutSessions
            .Include(s => s.WorkoutPlan)
            .Include(s => s.Exercises.OrderBy(e => e.SortOrder))
                .ThenInclude(se => se.Exercise)
            .Include(s => s.Exercises)
                .ThenInclude(se => se.Sets)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAtUtc)
            .ToListAsync();

        return View(sessions);
    }
}

public record CompleteSetRequest(int SessionExerciseId, int? Reps, decimal? Weight, int? DurationSeconds);
public record RateExerciseRequest(int SessionExerciseId, Difficulty Difficulty);
