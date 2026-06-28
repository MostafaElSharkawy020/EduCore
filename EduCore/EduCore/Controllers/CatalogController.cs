using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Student")]
    public class CatalogController : Controller
    {
        private readonly AppDbContext _context;

        // The signed-in student's id (from the auth cookie).
        private int CurrentStudentId => User.GetUserId();

        public CatalogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Catalog  — browse all enrollable courses
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Where(c => c.Enrollable)
                .Include(c => c.Teacher)
                .Include(c => c.Classes)
                .Include(c => c.StudentCourses)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.EnrolledIds = await _context.StudentCourses
                .Where(sc => sc.StudentID == CurrentStudentId)
                .Select(sc => sc.CourseID)
                .ToListAsync();

            return View(courses);
        }

        // GET: /Catalog/Details/5  — a single course's page
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Classes)
                .Include(c => c.Exams)
                .Include(c => c.StudentCourses)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (course == null)
                return NotFound();

            ViewBag.IsEnrolled = await _context.StudentCourses
                .AnyAsync(sc => sc.StudentID == CurrentStudentId && sc.CourseID == id);

            return View(course);
        }

        // POST: /Catalog/Enroll  — free enrollment (payment comes later)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.ID == courseId);
            if (course == null)
                return NotFound();

            if (!course.Enrollable)
            {
                TempData["CatalogMessage"] = "This course is not open for enrollment.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            var already = await _context.StudentCourses
                .AnyAsync(sc => sc.StudentID == CurrentStudentId && sc.CourseID == courseId);

            if (!already)
            {
                _context.StudentCourses.Add(new StudentCourse
                {
                    StudentID = CurrentStudentId,
                    CourseID = courseId
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Course", "Learn", new { id = courseId });
        }
    }
}
