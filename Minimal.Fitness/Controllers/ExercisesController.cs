using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Data;
using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Controllers;

public class ExercisesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(MuscleGroup? muscleGroup)
    {
        var query = db.Exercises.AsQueryable();
        if (muscleGroup.HasValue)
            query = query.Where(e => e.MuscleGroup == muscleGroup.Value);

        ViewBag.MuscleGroup = muscleGroup;
        return View(await query.OrderBy(e => e.MuscleGroup).ThenBy(e => e.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var exercise = await db.Exercises.FindAsync(id);
        if (exercise is null)
            return NotFound();

        return View(exercise);
    }
}
