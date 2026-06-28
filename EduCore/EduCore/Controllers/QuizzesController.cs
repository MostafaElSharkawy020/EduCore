using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class QuizzesController : Controller
    {
        private readonly AppDbContext _context;

        // The signed-in teacher's id (from the auth cookie).
        private int CurrentTeacherId => User.GetUserId();

        public QuizzesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Quizzes?classId=5
        public async Task<IActionResult> Index(int? classId)
        {
            if (classId == null)
                return NotFound();

            var @class = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ID == classId);

            if (@class == null || @class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            var quizzes = await _context.Quizzes
                .Where(q => q.ClassID == classId)
                .Include(q => q.QuizQuestions)
                .ToListAsync();

            ViewBag.Class = @class;
            return View(quizzes);
        }

        // GET: /Quizzes/Details/5  — the quiz with its questions (question hub)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Class).ThenInclude(c => c.Course)
                .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question).ThenInclude(qn => qn.Choices)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (quiz == null || quiz.Class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            return View(quiz);
        }

        // GET: /Quizzes/Create?classId=5
        public async Task<IActionResult> Create(int? classId)
        {
            if (classId == null || !await TeacherOwnsClass(classId.Value))
                return NotFound();

            ViewBag.Class = await _context.Classes.FindAsync(classId);
            return View(new Quiz { ClassID = classId.Value });
        }

        // POST: /Quizzes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,ClassID")] Quiz quiz)
        {
            ModelState.Remove(nameof(Quiz.Class));

            if (!await TeacherOwnsClass(quiz.ClassID))
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = quiz.ID });
            }

            ViewBag.Class = await _context.Classes.FindAsync(quiz.ClassID);
            return View(quiz);
        }

        // GET: /Quizzes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Class).ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (quiz == null || quiz.Class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            ViewBag.Class = quiz.Class;
            return View(quiz);
        }

        // POST: /Quizzes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,ClassID")] Quiz quiz)
        {
            if (id != quiz.ID)
                return NotFound();

            var ownedClassId = await _context.Quizzes
                .AsNoTracking()
                .Where(q => q.ID == id)
                .Select(q => (int?)q.ClassID)
                .FirstOrDefaultAsync();

            if (ownedClassId == null || !await TeacherOwnsClass(ownedClassId.Value))
                return NotFound();

            ModelState.Remove(nameof(Quiz.Class));

            // Keep the quiz attached to its original class (don't allow moving it via the form)
            quiz.ClassID = ownedClassId.Value;

            if (ModelState.IsValid)
            {
                _context.Update(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { classId = quiz.ClassID });
            }

            ViewBag.Class = await _context.Classes.FindAsync(quiz.ClassID);
            return View(quiz);
        }

        // GET: /Quizzes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Class).ThenInclude(c => c.Course)
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (quiz == null || quiz.Class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            return View(quiz);
        }

        // POST: /Quizzes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Class).ThenInclude(c => c.Course)
                .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (quiz == null || quiz.Class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            var classId = quiz.ClassID;

            // Remove the quiz, its question links, and the (per-quiz) questions.
            // Question -> Choice is a cascade delete, so choices go automatically.
            var questions = quiz.QuizQuestions
                .Select(qq => qq.Question)
                .Where(q => q != null)
                .ToList();

            _context.QuizQuestions.RemoveRange(quiz.QuizQuestions);
            _context.Questions.RemoveRange(questions);
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { classId });
        }

        private async Task<bool> TeacherOwnsClass(int classId) =>
            await _context.Classes.AnyAsync(c => c.ID == classId && c.Course.TeacherID == CurrentTeacherId);
    }
}
