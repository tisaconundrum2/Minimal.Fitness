using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Data;
using Minimal.Fitness.Models.Enums;

namespace Minimal.Fitness.Controllers;

public class ExercisesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(MuscleGroup? group)
    {
        var query = db.Exercises.AsQueryable();
        if (group.HasValue)
            query = query.Where(e => e.MuscleGroup == group.Value);
        ViewBag.SelectedGroup = group;
        ViewBag.MuscleGroups = Enum.GetValues<MuscleGroup>();
        return View(await query.OrderBy(e => e.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var exercise = await db.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();
        return View(exercise);
    }
}
