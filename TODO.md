# EduCore — Project TODO

## Done
- [x] EF Core data layer, migrations, SQL Server
- [x] **Teacher:** Courses CRUD, Classes CRUD (ownership checks)
- [x] **Teacher:** Videos, Quizzes (+ normalized Choice table), Exams
- [x] **Auth:** cookie auth for teachers & students, PBKDF2 hashing, roles, per-table unique email, login role selector, show-password toggle
- [x] **Profile** pages (teacher & student): view, edit, change password
- [x] **Student:** browse catalog, course details
- [x] **Student:** free enrollment, My Courses dashboard, view enrolled course + class content (videos, PDFs, quiz/exam lists)
- [x] **Assessments:** taking quizzes & exams, auto-grading, saved attempts (QuizAttempt/ExamAttempt) + per-question feedback

## Pending action
- [x] Apply the Phase 5 migration `AddAssessmentAttempts` — done; QuizAttempts/ExamAttempts tables exist and quiz attempts are persisting.

## Remaining (not yet built)
- [ ] **Teacher sees student results** — attempts are saved but teachers have no page to view them (who took each quiz/exam, scores, averages per quiz/exam/student).
- [ ] **Teacher dashboard / home** — stats landing page (active courses, total students, etc.); teachers currently land on the Courses list.

## Deferred by choice
- [ ] **Payment & cards** — enrollment is free for now. Add `Card` management + a payment step before enrolling (and the Profile "Payment Card" tab).
- [ ] **Paid-video protection** — currently only the enrollment gate; raw video URL is still shareable. Options: Vimeo domain-privacy links (no code) or a streaming provider with signed/expiring URLs (Bunny/Cloudflare Stream).

## Cleanup / nice-to-have
- [ ] **Assistant role** — schema has `Assistant` + `TeacherAssistant`, but login/UI don't support assistants. Build only if required.
- [ ] **Quiz/exam timers** — prototype showed countdown timers; not implemented.
- [ ] Remove the seeded demo teacher (teacher@educore.local) once real teacher onboarding exists.
- [x] Rename all "EduArc" → "EduCore" (done across frontend prototypes; MVC app already clean).

## Conventions
- After any model change: `Add-Migration <Name>` + `Update-Database` in Package Manager Console.
- Reference nav properties trigger spurious "required" validation (Nullable enabled) — use `ModelState.Remove(nameof(X.NavProp))` in POST actions.
- Teacher controllers: `[Authorize(Roles="Teacher")]`; student controllers: `[Authorize(Roles="Student")]`; `User.GetUserId()` for the current user.
- Views use `_EduCoreLayout` + `wwwroot/css/shared.css`.
