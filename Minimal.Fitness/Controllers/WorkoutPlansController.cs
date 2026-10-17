using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal.Fitness.Data;

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
}
