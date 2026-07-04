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
            // Courses the student can access: full course enrollment OR owns at least one class in it.
            var courses = await _context.Courses
                .Where(c => c.StudentCourses.Any(sc => sc.StudentID == CurrentStudentId)
                         || c.Classes.Any(cl => cl.StudentClasses.Any(scl => scl.StudentID == CurrentStudentId)))
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

            var courseEnrolled = await IsEnrolled(id.Value);
            var ownedClassIds = await _context.StudentClasses
                .Where(sc => sc.StudentID == CurrentStudentId && sc.Class.CourseID == id)
                .Select(sc => sc.ClassID)
                .ToListAsync();

            // Access requires course enrollment or owning at least one class in it.
            if (!courseEnrolled && ownedClassIds.Count == 0)
                return RedirectToAction("Details", "Catalog", new { id });

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Classes)
                .Include(c => c.Exams)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (course == null)
                return NotFound();

            ViewBag.CourseEnrolled = courseEnrolled;
            ViewBag.OwnedClassIds = ownedClassIds;

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

            // Access requires course enrollment OR owning this specific class.
            if (!await HasClassAccess(cls.ID, cls.CourseID))
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

        private async Task<bool> HasClassAccess(int classId, int courseId) =>
            await _context.StudentCourses.AnyAsync(sc => sc.StudentID == CurrentStudentId && sc.CourseID == courseId)
            || await _context.StudentClasses.AnyAsync(sc => sc.StudentID == CurrentStudentId && sc.ClassID == classId);
    }
}
