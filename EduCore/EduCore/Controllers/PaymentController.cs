using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Student")]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;

        private int CurrentStudentId => User.GetUserId();

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Payment/Checkout?courseId=5
        public async Task<IActionResult> Checkout(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.ID == courseId);

            if (course == null)
                return NotFound();

            // Class-only course? Whole-course purchase is disabled.
            if (!course.AllowCourseEnrollment)
            {
                TempData["CatalogMessage"] = "This course can't be enrolled as a whole. Enroll in individual classes instead.";
                return RedirectToAction("Details", "Catalog", new { id = courseId });
            }

            // Free course? No payment needed.
            if (course.Price <= 0)
                return RedirectToAction("Enroll", "Catalog", new { courseId });

            // Already enrolled? Go straight in.
            if (await IsEnrolled(courseId))
                return RedirectToAction("Course", "Learn", new { id = courseId });

            ViewBag.Cards = await _context.Cards
                .Where(c => c.StudentID == CurrentStudentId)
                .ToListAsync();

            return View(course);
        }

        // POST: /Payment/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int courseId, int cardId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.ID == courseId);
            if (course == null)
                return NotFound();

            if (!course.Enrollable)
            {
                TempData["CatalogMessage"] = "This course is not open for enrollment.";
                return RedirectToAction("Details", "Catalog", new { id = courseId });
            }

            var card = await _context.Cards
                .FirstOrDefaultAsync(c => c.ID == cardId && c.StudentID == CurrentStudentId);
            if (card == null)
            {
                // No/invalid card selected — send back to checkout
                return RedirectToAction(nameof(Checkout), new { courseId });
            }

            // Only pay + enroll if not already enrolled
            if (!await IsEnrolled(courseId))
            {
                // Simulated payment: record it, then enroll.
                var last4 = card.CardNumber.Length >= 4
                    ? card.CardNumber[^4..]
                    : card.CardNumber;

                _context.Payments.Add(new Payment
                {
                    StudentID = CurrentStudentId,
                    TeacherID = course.TeacherID,
                    ItemType = "Course",
                    ItemName = course.Name,
                    Amount = course.Price,
                    PaidAt = DateTime.Now,
                    CardLast4 = last4
                });

                _context.StudentCourses.Add(new StudentCourse
                {
                    StudentID = CurrentStudentId,
                    CourseID = courseId
                });

                await _context.SaveChangesAsync();
            }

            TempData["LearnMessage"] = $"Payment successful — you're enrolled in {course.Name}.";
            return RedirectToAction("Course", "Learn", new { id = courseId });
        }

        // GET: /Payment/CheckoutClass?classId=5
        public async Task<IActionResult> CheckoutClass(int classId)
        {
            var cls = await _context.Classes
                .Include(c => c.Course).ThenInclude(co => co.Teacher)
                .FirstOrDefaultAsync(c => c.ID == classId);

            if (cls == null)
                return NotFound();

            if (!cls.Enrollable)
            {
                TempData["CatalogMessage"] = "This class is not open for enrollment.";
                return RedirectToAction("Details", "Catalog", new { id = cls.CourseID });
            }

            if (cls.Price <= 0)
                return RedirectToAction("EnrollClass", "Catalog", new { classId });

            if (await HasClassAccess(cls.ID, cls.CourseID))
                return RedirectToAction("Class", "Learn", new { id = classId });

            ViewBag.Cards = await _context.Cards
                .Where(c => c.StudentID == CurrentStudentId)
                .ToListAsync();

            return View(cls);
        }

        // POST: /Payment/PayClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayClass(int classId, int cardId)
        {
            var cls = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ID == classId);
            if (cls == null)
                return NotFound();

            if (!cls.Enrollable)
            {
                TempData["CatalogMessage"] = "This class is not open for enrollment.";
                return RedirectToAction("Details", "Catalog", new { id = cls.CourseID });
            }

            var card = await _context.Cards
                .FirstOrDefaultAsync(c => c.ID == cardId && c.StudentID == CurrentStudentId);
            if (card == null)
                return RedirectToAction(nameof(CheckoutClass), new { classId });

            if (!await HasClassAccess(cls.ID, cls.CourseID))
            {
                var last4 = card.CardNumber.Length >= 4 ? card.CardNumber[^4..] : card.CardNumber;

                _context.Payments.Add(new Payment
                {
                    StudentID = CurrentStudentId,
                    TeacherID = cls.Course.TeacherID,
                    ItemType = "Class",
                    ItemName = cls.Name,
                    Amount = cls.Price,
                    PaidAt = DateTime.Now,
                    CardLast4 = last4
                });

                _context.StudentClasses.Add(new StudentClass
                {
                    StudentID = CurrentStudentId,
                    ClassID = classId
                });

                await _context.SaveChangesAsync();
            }

            TempData["LearnMessage"] = $"Payment successful — you're enrolled in the class \"{cls.Name}\".";
            return RedirectToAction("Class", "Learn", new { id = classId });
        }

        private async Task<bool> IsEnrolled(int courseId) =>
            await _context.StudentCourses.AnyAsync(sc => sc.StudentID == CurrentStudentId && sc.CourseID == courseId);

        private async Task<bool> HasClassAccess(int classId, int courseId) =>
            await _context.StudentCourses.AnyAsync(sc => sc.StudentID == CurrentStudentId && sc.CourseID == courseId)
            || await _context.StudentClasses.AnyAsync(sc => sc.StudentID == CurrentStudentId && sc.ClassID == classId);
    }
}
