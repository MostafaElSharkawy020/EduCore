using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class ExamsController : Controller
    {
        private readonly AppDbContext _context;

        // The signed-in teacher's id (from the auth cookie).
        private int CurrentTeacherId => User.GetUserId();

        public ExamsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Exams?courseId=5
        public async Task<IActionResult> Index(int? courseId)
        {
            if (courseId == null)
                return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.ID == courseId);

            if (course == null || course.TeacherID != CurrentTeacherId)
                return NotFound();

            var exams = await _context.Exams
                .Where(e => e.CourseID == courseId)
                .Include(e => e.ExamQuestions)
                .ToListAsync();

            ViewBag.Course = course;
            return View(exams);
        }

        // GET: /Exams/Details/5  — the exam with its questions (question hub)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var exam = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.ExamQuestions).ThenInclude(eq => eq.Question).ThenInclude(qn => qn.Choices)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (exam == null || exam.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            return View(exam);
        }

        // GET: /Exams/Create?courseId=5
        public async Task<IActionResult> Create(int? courseId)
        {
            if (courseId == null || !await TeacherOwnsCourse(courseId.Value))
                return NotFound();

            ViewBag.Course = await _context.Courses.FindAsync(courseId);
            return View(new Exam { CourseID = courseId.Value });
        }

        // POST: /Exams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,CourseID")] Exam exam)
        {
            ModelState.Remove(nameof(Exam.Course));

            if (!await TeacherOwnsCourse(exam.CourseID))
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Add(exam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = exam.ID });
            }

            ViewBag.Course = await _context.Courses.FindAsync(exam.CourseID);
            return View(exam);
        }

        // GET: /Exams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var exam = await _context.Exams
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (exam == null || exam.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            ViewBag.Course = exam.Course;
            return View(exam);
        }

        // POST: /Exams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,CourseID")] Exam exam)
        {
            if (id != exam.ID)
                return NotFound();

            var ownedCourseId = await _context.Exams
                .AsNoTracking()
                .Where(e => e.ID == id)
                .Select(e => (int?)e.CourseID)
                .FirstOrDefaultAsync();

            if (ownedCourseId == null || !await TeacherOwnsCourse(ownedCourseId.Value))
                return NotFound();

            ModelState.Remove(nameof(Exam.Course));

            // Keep the exam attached to its original course
            exam.CourseID = ownedCourseId.Value;

            if (ModelState.IsValid)
            {
                _context.Update(exam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { courseId = exam.CourseID });
            }

            ViewBag.Course = await _context.Courses.FindAsync(exam.CourseID);
            return View(exam);
        }

        // GET: /Exams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var exam = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.ExamQuestions)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (exam == null || exam.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            return View(exam);
        }

        // POST: /Exams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exam = await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.ExamQuestions).ThenInclude(eq => eq.Question)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (exam == null || exam.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            var courseId = exam.CourseID;

            // Remove the exam, its question links, and the (per-exam) questions.
            // Question -> Choice is a cascade delete, so choices go automatically.
            var questions = exam.ExamQuestions
                .Select(eq => eq.Question)
                .Where(q => q != null)
                .ToList();

            _context.ExamQuestions.RemoveRange(exam.ExamQuestions);
            _context.Questions.RemoveRange(questions);
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { courseId });
        }

        private async Task<bool> TeacherOwnsCourse(int courseId) =>
            await _context.Courses.AnyAsync(c => c.ID == courseId && c.TeacherID == CurrentTeacherId);
    }
}
