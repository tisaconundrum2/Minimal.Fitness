# Minimal Fitness

A workout tracker web app built with ASP.NET Core MVC, PostgreSQL, and exercise illustrations from [workout-guide](https://github.com/bryllim/workout-guide).

## Features

- **120 exercises** with animated illustrations (3-frame ping-pong) from the workout-guide library
- **Workout plans** — use the starter plans or build your own with a visual card picker
- **Quick Mix** — generate a random full-body workout with one click
- **Active workout session** — track sets, reps, and weight with a rest timer between sets
- **Difficulty rating** — rate each exercise as Hard / Made it / Easy after completing all sets
- **Rest timer** — configurable countdown with audio beep (default 60–90s based on profile)
- **Drag-and-drop reorder** — arrange exercises in your preferred order
- **Mobile-friendly** — responsive layout, bound to `0.0.0.0` for phone access on LAN

## Tech Stack

- **ASP.NET Core 10** MVC with Razor views
- **PostgreSQL** via Npgsql Entity Framework Core (uses `minimal-fitness` schema)
- **ASP.NET Identity** for user auth
- **Exercise images** from [bryllim/workout-guide](https://github.com/bryllim/workout-guide) (CC BY-SA 4.0) via jsDelivr CDN
- **Vanilla JS** for timers, drag-and-drop, and exercise picker — no frontend framework

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL database (or use the connection string for a remote instance)

### Setup

```bash
cd Minimal.Fitness

# Set your PostgreSQL connection string (stored in user-secrets, not in source)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=minimal-fitness;Username=your_user;Password=your_pass"

# Apply migrations
dotnet ef database update

# Run
dotnet run
```

Open `http://localhost:5117` (or `http://<your-ip>:5117` from your phone).

### Seed Data

On first run, the app seeds 120 exercises and 2 starter workout plans (Upper Body + Lower Body). If the `Exercises` table already has data, seeding is skipped.

To force a reseed in Development, set `SeedData:ResetOnStartup` to `true` in [appsettings.Development.json](./Minimal.Fitness/appsettings.Development.json).  
This clears workout/session/exercise tables before seeding so the starter data can be recreated.

## Project Structure

```
Minimal.Fitness/
├── Controllers/         # MVC controllers (Home, Exercises, WorkoutPlans, WorkoutSession, Profile)
├── Data/                # ApplicationDbContext + SeedData
├── Migrations/          # EF Core PostgreSQL migrations
├── Models/              # Exercise, WorkoutPlan, WorkoutSession, ExerciseSet, etc.
│   └── Enums/           # TrackingType, MuscleGroup, Gender, Difficulty
├── Views/
│   ├── Exercises/       # Exercise library (card grid with animations)
│   ├── WorkoutPlans/    # Plan list, details, create (card picker), edit (drag reorder)
│   ├── WorkoutSession/  # Active workout, summary, history
│   ├── Profile/         # Gender + rest timer settings
│   └── Shared/          # Layout, login partial
└── wwwroot/
    ├── css/site.css     # Exercise card styles, frame animation, drag-and-drop
    └── js/site.js
```

## Exercise Images

All exercise illustrations come from the [workout-guide](https://github.com/bryllim/workout-guide) npm package by Bryl Lim, licensed under **CC BY-SA 4.0**. Images are loaded from the jsDelivr CDN — nothing is stored locally.

Each exercise has 3 frames that animate in a 1→2→3→2 ping-pong loop on dark navy backgrounds.

## License

Exercise artwork: [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/) — Bryl Lim / Everkinetic.
