# EduCore

EduCore is an online **course management & learning platform** built with **ASP.NET Core MVC**.
Teachers create courses, classes, videos, quizzes and exams; students browse a catalog, enroll
(free or paid), watch content, take auto-graded timed assessments, and pay via a simulated
checkout. Teachers get results analytics, a dashboard, and a revenue view.

🔗 **Live demo:** http://educoreasp.runasp.net/
Sign in as the demo teacher — email `teacher@educore.local`, password `Teacher@123` (select the *Teacher* role).

---

## Table of Contents
1. [Features](#features)
2. [Tech Stack](#tech-stack)
3. [System Requirements](#system-requirements)
4. [Installation](#installation)
5. [Configuration](#configuration)
6. [Database Setup](#database-setup)
7. [Running the App](#running-the-app)
8. [Demo Accounts](#demo-accounts)
9. [Routes Reference](#routes-reference)
10. [Building an Executable & Deployment](#building-an-executable--deployment)
11. [Project Structure](#project-structure)
12. [Notes & Known Limitations](#notes--known-limitations)

---

## Features

**Authentication & accounts**
- Cookie-based auth for **Teachers** and **Students** (role-based)
- Passwords hashed with **PBKDF2 (SHA-256)**
- Register/login with role selection, show-password toggle, unique-email enforcement
- Profile pages: view, edit, change password

**Teacher**
- CRUD for Courses, Classes, Videos, Quizzes, Exams (ownership-checked)
- Multiple-choice questions with a normalized `Choice` table
- Time limits on quizzes & exams
- **Results** analytics per quiz/exam (scores, averages)
- **Dashboard** (courses, students, classes, assessments, recent activity)
- **Revenue** view (80/20 teacher/platform split — configurable)
- "Class-only" courses (disable whole-course enrollment)

**Student**
- Browse catalog, course details
- Enroll in a whole course **or** individual classes (à la carte)
- Free enrollment or **simulated paid checkout** with saved cards + purchase history
- My Courses dashboard; watch videos, open PDFs
- Take **timed, auto-graded** quizzes & exams with per-question feedback; saved attempts

---

## Tech Stack
- **.NET 10** / ASP.NET Core MVC
- **Entity Framework Core 10** (code-first migrations)
- **SQL Server** (LocalDB or full instance)
- Razor views + Bootstrap-based custom CSS (`wwwroot/css/shared.css`)
- Cookie authentication (`Microsoft.AspNetCore.Authentication.Cookies`)

---

## System Requirements

**Software dependencies**
- **.NET SDK 10.0+** — https://dotnet.microsoft.com/download
- **SQL Server** — any of: SQL Server Express, Developer, or LocalDB (ships with Visual Studio)
- **Visual Studio 2022/2026** (recommended) *or* the `dotnet` CLI
- OS: Windows 10/11 (developed on Windows 11). Cross-platform is possible but the connection
  string and SQL Server setup below assume Windows.

**NuGet packages** (restored automatically)
- `Microsoft.EntityFrameworkCore.SqlServer` 10.x
- `Microsoft.EntityFrameworkCore.Tools` 10.x

**Hardware** (typical dev machine)
- 4 GB RAM minimum (8 GB recommended)
- ~2 GB free disk (SDK + packages + database)

---

## Installation

```bash
# 1. Clone the repository
git clone <your-repo-url>
cd EduCore

# 2. Restore dependencies (from the project folder)
cd EduCore/EduCore
dotnet restore
```

Or simply open **`EduCore/EduCore.slnx`** in Visual Studio (packages restore on load).

---

## Configuration

Database connection is set in **`EduCore/EduCore/appsettings.json`**:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=LAPTOP-VQ4BN17M;Database=EduCore;Integrated Security=True;TrustServerCertificate=True;"
}
```

Change `Server=` to your SQL Server instance. Common values:
- LocalDB: `Server=(localdb)\\MSSQLLocalDB;Database=EduCore;Trusted_Connection=True;MultipleActiveResultSets=true`
- SQL Express: `Server=.\\SQLEXPRESS;Database=EduCore;Trusted_Connection=True;TrustServerCertificate=True`

Other configurable bits:
- **Revenue split** — `EduCore/EduCore/Helpers/PlatformSettings.cs` → `PlatformFeeRate` (default `0.20m` = 20% platform / 80% teacher)
- **Currency** — set to EGP globally in `Program.cs`

---

## Database Setup

The schema is created via **EF Core migrations**. Using the **Package Manager Console** in
Visual Studio (Tools → NuGet Package Manager → Package Manager Console, Default project = `EduCore`):

```powershell
Update-Database
```

Or with the .NET CLI (install the tool once with `dotnet tool install --global dotnet-ef`):

```bash
cd EduCore/EduCore
dotnet ef database update
```

This creates the `EduCore` database and all tables, and seeds a demo teacher.

> After changing any model, create a new migration: `Add-Migration <Name>` then `Update-Database`.

---

## Running the App

**Visual Studio:** press **F5** (or Ctrl+F5).

**CLI:**
```bash
cd EduCore/EduCore
dotnet run
```

The app launches at:
- HTTPS: **https://localhost:7056**
- HTTP: **http://localhost:5247**

It opens on the **login page**. (Ports are defined in `Properties/launchSettings.json`.)

---

## Demo Accounts

A demo **teacher** is seeded by the migrations:

| Role | Email | Password |
|------|-------|----------|
| Teacher | `teacher@educore.local` | `Teacher@123` |

Create **student** accounts via the **Sign up** page (choose the *Student* role).

---

## Routes Reference

EduCore is a **server-rendered MVC app**, not a JSON/REST API, so there is no separate API
schema. The main routes (all under `/{Controller}/{Action}`) are:

**Public / account**
- `GET /` → redirects to login (or the user's home if signed in)
- `GET|POST /Account/Login`, `/Account/Register`, `POST /Account/Logout`

**Teacher** (`[Authorize(Roles="Teacher")]`)
- `/Dashboard` — home/stats
- `/Courses`, `/Classes`, `/Videos`, `/Quizzes`, `/Exams` — content management (Index/Create/Edit/Delete/Details)
- `/Questions`, `/ExamQuestions` — quiz/exam questions
- `/Results/Quiz/{id}`, `/Results/Exam/{id}` — student results
- `/Revenue` — earnings

**Student** (`[Authorize(Roles="Student")]`)
- `/Catalog`, `/Catalog/Details/{id}` — browse & view courses
- `/Catalog/Enroll`, `/Catalog/EnrollClass` — free enrollment
- `/Payment/Checkout`, `/Payment/CheckoutClass`, `/Payment/Pay`, `/Payment/PayClass` — paid checkout
- `/Cards` — saved payment cards + purchase history
- `/Learn`, `/Learn/Course/{id}`, `/Learn/Class/{id}` — enrolled learning experience
- `/Assessments/Quiz/{id}`, `/Assessments/Exam/{id}` — take assessments

**Both roles**
- `/Profile`, `/Profile/Edit`, `/Profile/Password`

---

## Building an Executable & Deployment

### Produce a runnable build
```bash
cd EduCore/EduCore

# Framework-dependent build (requires .NET 10 runtime on the target machine)
dotnet publish -c Release -o ./publish
# Run it:
dotnet ./publish/EduCore.dll
```

Self-contained, single-file executable (no runtime needed on target):
```bash
# Windows x64 example → produces publish/EduCore.exe
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -o ./publish
```

The output folder contains the app (`EduCore.dll` / `EduCore.exe`) plus `appsettings.json`
and `wwwroot`. Update the connection string on the target machine before running.

### Deployment options
- **IIS (Windows Server)** — install the ASP.NET Core Hosting Bundle, publish, point an IIS site at the folder.
- **Azure App Service** — `dotnet publish` then deploy (VS "Publish", `az webapp up`, or GitHub Actions); use a SQL Azure connection string.
- **Docker** — containerize with the `mcr.microsoft.com/dotnet/aspnet:10.0` base image.

### Deployment link
> **Live deployment:** http://educoreasp.runasp.net/
>
> Hosted on **MonsterASP.NET** (ASP.NET Core + SQL Server). The app applies EF Core migrations
> automatically on startup (see `Program.cs`), so the schema and the seeded demo teacher are
> created on first run — no manual `Update-Database` against the remote database is needed.
> Log in with the demo teacher account above, or register a new student.

---

## Project Structure
```
EduCore/                         # repo root
├─ EduCore/                      # solution folder
│  ├─ EduCore.slnx               # solution
│  └─ EduCore/                   # ASP.NET Core MVC project
│     ├─ Controllers/            # Account, Courses, Classes, Videos, Quizzes, Exams,
│     │                          #   Catalog, Learn, Assessments, Payment, Cards,
│     │                          #   Results, Dashboard, Revenue, Profile, Home
│     ├─ Models/                 # EF entities (Course, Class, Video, Quiz, Question, Choice,
│     │                          #   Exam, Student, Teacher, StudentCourse, StudentClass,
│     │                          #   Payment, QuizAttempt, ExamAttempt, Card, ...)
│     ├─ ViewModels/             # form/display view models
│     ├─ Views/                  # Razor views (per controller) + shared layouts
│     ├─ Data/AppDbContext.cs    # EF Core DbContext
│     ├─ Helpers/                # PasswordHasher, VideoEmbedHelper, PlatformSettings, ...
│     ├─ Migrations/             # EF Core migrations (+ model snapshot)
│     ├─ wwwroot/                # css/js/lib static assets
│     ├─ appsettings.json        # connection string & config
│     └─ Program.cs              # startup, auth, DI, culture
├─ frontend/                     # original static HTML/CSS design prototypes
├─ Database Diagrams/            # ERD / schema diagrams
├─ TODO.md                       # backlog
└─ README.md
```

---

## Notes & Known Limitations
- **Payments are simulated** — no real gateway; card details are stored for the demo only.
  This is **not PCI-compliant** and must never be used with real card data.
- **Video protection** — content pages are gated by enrollment, but a raw video URL is still
  shareable (no signed/expiring URLs).
- The seeded demo teacher is for development; remove it before any real use.
