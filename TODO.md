# EduCore — Project TODO

## Done
- [x] EF Core data layer, migrations, SQL Server
- [x] **Teacher:** Courses CRUD, Classes CRUD, Videos, Quizzes (+ normalized Choice table), Exams
- [x] **Auth:** cookie auth for teachers & students, PBKDF2 hashing, roles, per-table unique email, login role selector, show-password toggle
- [x] **Profile** pages (teacher & student): view, edit, change password
- [x] **Student:** browse catalog, course details, My Courses dashboard, view enrolled course + class content (videos, PDFs)
- [x] **Assessments:** taking quizzes & exams, timers (auto-submit), auto-grading, saved attempts + per-question feedback
- [x] **Teacher:** sees student results per quiz/exam; dashboard home with stats + recent activity
- [x] **Payment:** simulated checkout, saved cards + purchase history, Payment table
- [x] **Class enrollment:** à-la-carte class purchase; per-class access gating
- [x] **Class-only courses:** teacher can disable whole-course enrollment so a course sells by class only
- [x] **Teacher revenue view:** earnings from sales with an 80/20 teacher/platform split (constant in `PlatformSettings`)
- [x] App opens on the login page

## Pending action (migration)
- [ ] Apply the pending migration (adds `Class.Enrollable`, `StudentClasses`, `Payments` rework incl. `Payment.TeacherID`, `Course.AllowCourseEnrollment`): `Add-Migration RevenueAndClassEnrollment` + `Update-Database`. **This also fixes the current teacher-login exception.**
- [x] Backfilled existing courses: `AllowCourseEnrollment = 1` (4 courses).

## Deferred by choice
- [ ] **Paid-video protection** — currently only the enrollment gate; raw video URL is still shareable. Options: Vimeo domain-privacy links (no code) or a streaming provider with signed/expiring URLs (Bunny/Cloudflare Stream).

## Cleanup / nice-to-have
- [ ] **Assistant role** — schema has `Assistant` + `TeacherAssistant`, but login/UI don't support assistants. Build only if required.
- [ ] Remove the seeded demo teacher (teacher@educore.local) once real teacher onboarding exists.

## Conventions
- After any model change: `Add-Migration <Name>` + `Update-Database` in Package Manager Console.
- Reference nav properties trigger spurious "required" validation (Nullable enabled) — use `ModelState.Remove(nameof(X.NavProp))` in POST actions.
- Teacher controllers: `[Authorize(Roles="Teacher")]`; student controllers: `[Authorize(Roles="Student")]`; `User.GetUserId()` for the current user.
- Views use `_EduCoreLayout` + `wwwroot/css/shared.css`.
