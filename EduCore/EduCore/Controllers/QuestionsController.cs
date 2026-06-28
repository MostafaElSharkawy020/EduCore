using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using EduCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    // Manages the questions inside a quiz (each question = text + 2-5 choices, one correct).
    [Authorize(Roles = "Teacher")]
    public class QuestionsController : Controller
    {
        private readonly AppDbContext _context;

        // The signed-in teacher's id (from the auth cookie).
        private int CurrentTeacherId => User.GetUserId();

        public QuestionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Questions/Create?quizId=5
        public async Task<IActionResult> Create(int? quizId)
        {
            if (quizId == null)
                return NotFound();

            var quiz = await GetOwnedQuizAsync(quizId.Value);
            if (quiz == null)
                return NotFound();

            ViewBag.Quiz = quiz;
            return View(new QuestionFormViewModel { QuizId = quiz.ID, CorrectNumber = 1 });
        }

        // POST: /Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionFormViewModel vm)
        {
            var quiz = await GetOwnedQuizAsync(vm.QuizId);
            if (quiz == null)
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
                question.QuizQuestions.Add(new QuizQuestion { QuizID = vm.QuizId });

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Quizzes", new { id = vm.QuizId });
            }

            ViewBag.Quiz = quiz;
            return View(vm);
        }

        // GET: /Questions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var question = await _context.Questions
                .Include(q => q.Choices)
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (question == null)
                return NotFound();

            var quizId = question.QuizQuestions.Select(qq => (int?)qq.QuizID).FirstOrDefault();
            var quiz = quizId == null ? null : await GetOwnedQuizAsync(quizId.Value);
            if (quiz == null)
                return NotFound();

            var orderedChoices = question.Choices.OrderBy(c => c.ID).ToList();
            var correctIndex = orderedChoices.FindIndex(c => c.IsCorrect);

            ViewBag.Quiz = quiz;
            return View(new QuestionFormViewModel
            {
                ID = question.ID,
                QuizId = quiz.ID,
                QuestionText = question.QuestionText,
                ChoicesText = string.Join("\n", orderedChoices.Select(c => c.Text)),
                CorrectNumber = correctIndex >= 0 ? correctIndex + 1 : 1
            });
        }

        // POST: /Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionFormViewModel vm)
        {
            var question = await _context.Questions
                .Include(q => q.Choices)
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.ID == vm.ID);

            if (question == null)
                return NotFound();

            var quizId = question.QuizQuestions.Select(qq => (int?)qq.QuizID).FirstOrDefault();
            var quiz = quizId == null ? null : await GetOwnedQuizAsync(quizId.Value);
            if (quiz == null)
                return NotFound();

            var choices = ParseChoices(vm.ChoicesText);
            ValidateChoices(choices, vm.CorrectNumber);

            if (ModelState.IsValid)
            {
                question.QuestionText = vm.QuestionText.Trim();

                // Replace all choices
                _context.Choices.RemoveRange(question.Choices);
                question.Choices = choices
                    .Select((text, i) => new Choice { Text = text, IsCorrect = (i + 1) == vm.CorrectNumber })
                    .ToList();

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Quizzes", new { id = quiz.ID });
            }

            ViewBag.Quiz = quiz;
            return View(vm);
        }

        // GET: /Questions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var question = await _context.Questions
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (question == null)
                return NotFound();

            var quizId = question.QuizQuestions.Select(qq => (int?)qq.QuizID).FirstOrDefault();
            var quiz = quizId == null ? null : await GetOwnedQuizAsync(quizId.Value);
            if (quiz == null)
                return NotFound();

            ViewBag.QuizId = quiz.ID;
            return View(question);
        }

        // POST: /Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (question == null)
                return NotFound();

            var quizId = question.QuizQuestions.Select(qq => (int?)qq.QuizID).FirstOrDefault();
            var quiz = quizId == null ? null : await GetOwnedQuizAsync(quizId.Value);
            if (quiz == null)
                return NotFound();

            // Remove the quiz link then the question (choices cascade).
            _context.QuizQuestions.RemoveRange(question.QuizQuestions);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Quizzes", new { id = quiz.ID });
        }

        // ── Helpers ──

        private async Task<Quiz?> GetOwnedQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Class).ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(q => q.ID == quizId);

            return (quiz != null && quiz.Class.Course.TeacherID == CurrentTeacherId) ? quiz : null;
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
                ModelState.AddModelError(nameof(QuestionFormViewModel.ChoicesText), "Enter at least two choices (one per line).");
            else if (choices.Count > 5)
                ModelState.AddModelError(nameof(QuestionFormViewModel.ChoicesText), "A question can have at most five choices.");

            if (choices.Count >= 2 && (correctNumber < 1 || correctNumber > choices.Count))
                ModelState.AddModelError(nameof(QuestionFormViewModel.CorrectNumber),
                    $"Correct choice number must be between 1 and {choices.Count}.");
        }
    }
}
