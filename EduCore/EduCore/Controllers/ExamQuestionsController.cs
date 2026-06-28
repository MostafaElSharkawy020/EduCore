using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using EduCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    // Manages the questions inside an exam (each question = text + 2-5 choices, one correct).
    [Authorize(Roles = "Teacher")]
    public class ExamQuestionsController : Controller
    {
        private readonly AppDbContext _context;

        // The signed-in teacher's id (from the auth cookie).
        private int CurrentTeacherId => User.GetUserId();

        public ExamQuestionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /ExamQuestions/Create?examId=5
        public async Task<IActionResult> Create(int? examId)
        {
            if (examId == null)
                return NotFound();

            var exam = await GetOwnedExamAsync(examId.Value);
            if (exam == null)
                return NotFound();

            ViewBag.Exam = exam;
            return View(new ExamQuestionFormViewModel { ExamId = exam.ID, CorrectNumber = 1 });
        }

        // POST: /ExamQuestions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamQuestionFormViewModel vm)
        {
            var exam = await GetOwnedExamAsync(vm.ExamId);
            if (exam == null)
                return NotFound();

            var choices = ParseChoices(vm.ChoicesText);
            ValidateChoices(choices, vm.CorrectNumber);

            if (ModelState.IsValid)
            {
                var question = new Question
                {
                    QuestionText = vm.QuestionText.Trim(),
                    Choices = choices
                        .Select((text, i) => new Choice { Text = text, IsCorrect = (i + 1) == vm.CorrectNumber })
                        .ToList()
                };
                question.ExamQuestions.Add(new ExamQuestion { ExamID = vm.ExamId });

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Exams", new { id = vm.ExamId });
            }

            ViewBag.Exam = exam;
            return View(vm);
        }

        // GET: /ExamQuestions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var question = await _context.Questions
                .Include(q => q.Choices)
                .Include(q => q.ExamQuestions)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (question == null)
                return NotFound();

            var examId = question.ExamQuestions.Select(eq => (int?)eq.ExamID).FirstOrDefault();
            var exam = examId == null ? null : await GetOwnedExamAsync(examId.Value);
            if (exam == null)
                return NotFound();

            var orderedChoices = question.Choices.OrderBy(c => c.ID).ToList();
            var correctIndex = orderedChoices.FindIndex(c => c.IsCorrect);

            ViewBag.Exam = exam;
            return View(new ExamQuestionFormViewModel
            {
                ID = question.ID,
                ExamId = exam.ID,
                QuestionText = question.QuestionText,
                ChoicesText = string.Join("\n", orderedChoices.Select(c => c.Text)),
                CorrectNumber = correctIndex >= 0 ? correctIndex + 1 : 1
            });
        }

        // POST: /ExamQuestions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamQuestionFormViewModel vm)
        {
            var question = await _context.Questions
                .Include(q => q.Choices)
                .Include(q => q.ExamQuestions)
                .FirstOrDefaultAsync(q => q.ID == vm.ID);

            if (question == null)
                return NotFound();

            var examId = question.ExamQuestions.Select(eq => (int?)eq.ExamID).FirstOrDefault();
            var exam = examId == null ? null : await GetOwnedExamAsync(examId.Value);
            if (exam == null)
                return NotFound();

            var choices = ParseChoices(vm.ChoicesText);
            ValidateChoices(choices, vm.CorrectNumber);

            if (ModelState.IsValid)
            {
                question.QuestionText = vm.QuestionText.Trim();

                _context.Choices.RemoveRange(question.Choices);
                question.Choices = choices
                    .Select((text, i) => new Choice { Text = text, IsCorrect = (i + 1) == vm.CorrectNumber })
                    .ToList();

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Exams", new { id = exam.ID });
            }

            ViewBag.Exam = exam;
            return View(vm);
        }

        // GET: /ExamQuestions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var question = await _context.Questions
                .Include(q => q.ExamQuestions)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (question == null)
                return NotFound();

            var examId = question.ExamQuestions.Select(eq => (int?)eq.ExamID).FirstOrDefault();
            var exam = examId == null ? null : await GetOwnedExamAsync(examId.Value);
            if (exam == null)
                return NotFound();

            ViewBag.ExamId = exam.ID;
            return View(question);
        }

        // POST: /ExamQuestions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions
                .Include(q => q.ExamQuestions)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (question == null)
                return NotFound();

            var examId = question.ExamQuestions.Select(eq => (int?)eq.ExamID).FirstOrDefault();
            var exam = examId == null ? null : await GetOwnedExamAsync(examId.Value);
            if (exam == null)
                return NotFound();

            _context.ExamQuestions.RemoveRange(question.ExamQuestions);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Exams", new { id = exam.ID });
        }

        // ── Helpers ──

        private async Task<Exam?> GetOwnedExamAsync(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ID == examId);

            return (exam != null && exam.Course.TeacherID == CurrentTeacherId) ? exam : null;
        }

        private static List<string> ParseChoices(string? text) =>
            (text ?? string.Empty)
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToList();

        private void ValidateChoices(List<string> choices, int correctNumber)
        {
            if (choices.Count < 2)
                ModelState.AddModelError(nameof(ExamQuestionFormViewModel.ChoicesText), "Enter at least two choices (one per line).");
            else if (choices.Count > 5)
                ModelState.AddModelError(nameof(ExamQuestionFormViewModel.ChoicesText), "A question can have at most five choices.");

            if (choices.Count >= 2 && (correctNumber < 1 || correctNumber > choices.Count))
                ModelState.AddModelError(nameof(ExamQuestionFormViewModel.CorrectNumber),
                    $"Correct choice number must be between 1 and {choices.Count}.");
        }
    }
}
