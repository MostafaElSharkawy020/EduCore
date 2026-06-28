using EduCore.Data;
using EduCore.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Student")]
    public class LearnController : Controller
    {
        private readonly AppDbContext _context;

        private int CurrentStudentId => User.GetUserId();

        public LearnController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Learn  — "My Courses" dashboard
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Where(c => c.StudentCourses.Any(sc => sc.StudentID == CurrentStudentId))
                .Include(c => c.Teacher)
                .Include(c => c.Classes)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(courses);
        }

        // GET: /Learn/Course/5  — an enrolled course (unlocked)
        public async Task<IActionResult> Course(int? id)
        {
            if (id == null)
                return NotFound();

            if (!await IsEnrolled(id.Value))
                return RedirectToAction("Details", "Catalog", new { id });

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Classes)
                .Include(c => c.Exams)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (course == null)
                return NotFound();

            var examIds = course.Exams.Select(e => e.ID).ToList();
            var examAttempts = await _context.ExamAttempts
                .Where(a => a.StudentID == CurrentStudentId && examIds.Contains(a.ExamID))
                .ToListAsync();
            ViewBag.ExamScores = examAttempts
                .GroupBy(a => a.ExamID)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.SubmittedAt).First());

            return View(course);
        }

        // GET: /Learn/Class/5  — a class's content (videos, PDFs, quizzes)
        public async Task<IActionResult> Class(int? id)
        {
            if (id == null)
                return NotFound();

            var cls = await _context.Classes
                .Include(c => c.Course).ThenInclude(co => co.Classes)
                .Include(c => c.Videos)
                .Include(c => c.Quizzes).ThenInclude(q => q.QuizQuestions)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (cls == null)
                return NotFound();

            if (!await IsEnrolled(cls.CourseID))
                return RedirectToAction("Details", "Catalog", new { id = cls.CourseID });

            var quizIds = cls.Quizzes.Select(q => q.ID).ToList();
            var quizAttempts = await _context.QuizAttempts
                .Where(a => a.StudentID == CurrentStudentId && quizIds.Contains(a.QuizID))
                .ToListAsync();
            ViewBag.QuizScores = quizAttempts
                .GroupBy(a => a.QuizID)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.SubmittedAt).First());

            return View(cls);
        }

        private async Task<bool> IsEnrolled(int courseId) =>
            await _context.StudentCourses
                .AnyAsync(sc => sc.StudentID == CurrentStudentId && sc.CourseID == courseId);
    }
}
