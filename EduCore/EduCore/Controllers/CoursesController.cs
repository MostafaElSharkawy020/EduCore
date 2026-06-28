using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;

        // The signed-in teacher's id (from the auth cookie).
        private int CurrentTeacherId => User.GetUserId();

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Courses
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Where(c => c.TeacherID == CurrentTeacherId)
                .Include(c => c.Classes)
                .ToListAsync();

            return View(courses);
        }

        // GET: /Courses/Create
        public IActionResult Create()
        {
            return View(new Course());
        }

        // POST: /Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,Enrollable")] Course course)
        {
            // The Teacher navigation property isn't posted by the form. With nullable
            // reference types enabled it is treated as required, which would make
            // ModelState invalid. Remove it so validation only checks the real inputs.
            ModelState.Remove(nameof(Course.Teacher));

            if (ModelState.IsValid)
            {
                course.TeacherID = CurrentTeacherId; // owner is the current teacher, never from the form
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        // GET: /Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FindAsync(id);

            // Only let a teacher edit a course they own
            if (course == null || course.TeacherID != CurrentTeacherId)
                return NotFound();

            return View(course);
        }

        // POST: /Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Price,Enrollable")] Course course)
        {
            if (id != course.ID)
                return NotFound();

            // Confirm the course exists and belongs to this teacher (don't trust the form)
            var ownsCourse = await _context.Courses
                .AsNoTracking()
                .AnyAsync(c => c.ID == id && c.TeacherID == CurrentTeacherId);

            if (!ownsCourse)
                return NotFound();

            // See note in Create: ignore the unposted navigation property during validation.
            ModelState.Remove(nameof(Course.Teacher));

            if (ModelState.IsValid)
            {
                course.TeacherID = CurrentTeacherId; // preserve ownership
                _context.Update(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        // GET: /Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.ID == id && c.TeacherID == CurrentTeacherId);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // POST: /Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.ID == id && c.TeacherID == CurrentTeacherId);

            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
