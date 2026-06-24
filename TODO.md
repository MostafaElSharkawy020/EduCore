# Teacher Course Management — TODO

Remaining features for the teacher-facing course management area.
(Built so far: Courses CRUD, Classes CRUD — both with teacher ownership checks.)

## Backlog

- [ ] **Add Videos to a class**
  - Model: `Video` (URL, Title, ClassID) — already exists.
  - Likely a `VideosController` (or nested actions under a class) with Create/Edit/Delete.
  - Ownership: a video belongs to a class → course → teacher (`Video.Class.Course.TeacherID`).

- [ ] **Add Quizzes to a class**
  - Models: `Quiz` (Title, ClassID), `QuizQuestion` (QuizID, QuestionID), `Question` (QuestionText, CorrectAnswer, Choices) — all exist.
  - Quiz editor: create a quiz under a class, then add questions.
  - Ownership: `Quiz.Class.Course.TeacherID == currentTeacher`.
  - Frontend reference: `frontend/editor-exam.html?type=quiz`.

- [ ] **Add Exams to a course**
  - Models: `Exam` (Title, CourseID), `ExamQuestion` (ExamID, QuestionID), `Question` — all exist.
  - Same pattern as quizzes but linked to a course instead of a class.
  - Ownership: `Exam.Course.TeacherID == currentTeacher`.
  - Frontend reference: `frontend/editor-exam.html?type=exam`.

- [ ] **View Class Data (Class Details page)**
  - A read-only `Details` view for a class showing its videos, quizzes, and PDF links.
  - Frontend reference: `frontend/class-view.html`.

## Notes / conventions to follow (match the Courses/Classes work)
- Hardcoded `CurrentTeacherId = 1` for now — swap for real auth later (one place per controller).
- After any model change, run `Add-Migration <Name>` + `Update-Database` in the Package Manager Console.
- Reference navigation properties trigger spurious "required" validation (Nullable enabled) — use `ModelState.Remove(nameof(X.NavProp))` in POST actions, as done in Courses/Classes.
- Views use the `_EduCoreLayout` layout and `wwwroot/css/shared.css` design system.

## Related deferred items (not part of this checklist, noted earlier)
- [ ] Replace hardcoded `TeacherId` with the logged-in teacher once auth is built; remove the seeded demo teacher.
- [ ] Finish renaming remaining "EduArc" references to "EduCore" (e.g. the `<title>` in `_EduCoreLayout.cshtml`).
