using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class ClassesController : Controller
    {
        private readonly AppDbContext _context;

        // The signed-in teacher's id (from the auth cookie).
        private int CurrentTeacherId => User.GetUserId();

        public ClassesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Classes
        public async Task<IActionResult> Index()
        {
            // Only classes whose parent course belongs to the current teacher
            var classes = await _context.Classes
                .Where(c => c.Course.TeacherID == CurrentTeacherId)
                .Include(c => c.Course)
                .OrderBy(c => c.Course.Name).ThenBy(c => c.Name)
                .ToListAsync();

            return View(classes);
        }

        // GET: /Classes/Create
        public async Task<IActionResult> Create(int? courseId)
        {
            await PopulateCoursesDropDown(courseId);
            return View(new Class { CourseID = courseId ?? 0, Enrollable = true });
        }

        // POST: /Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,Enrollable,PDF,HomeworkPDF,CourseID")] Class @class)
        {
            // Navigation property isn't posted; don't let it fail validation.
            ModelState.Remove(nameof(Class.Course));

            // The chosen course must belong to the current teacher.
            if (!await TeacherOwnsCourse(@class.CourseID))
                ModelState.AddModelError(nameof(Class.CourseID), "Please select one of your own courses.");

            if (ModelState.IsValid)
            {
                _context.Add(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateCoursesDropDown(@class.CourseID);
            return View(@class);
        }

        // GET: /Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var @class = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ID == id);

            // Must exist and belong (via its course) to the current teacher
            if (@class == null || @class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            await PopulateCoursesDropDown(@class.CourseID);
            return View(@class);
        }

        // POST: /Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Price,Enrollable,PDF,HomeworkPDF,CourseID")] Class @class)
        {
            if (id != @class.ID)
                return NotFound();

            // Confirm the teacher currently owns this class (via its existing course)
            var currentCourseId = await _context.Classes
                .AsNoTracking()
                .Where(c => c.ID == id)
                .Select(c => (int?)c.CourseID)
                .FirstOrDefaultAsync();

            if (currentCourseId == null || !await TeacherOwnsCourse(currentCourseId.Value))
                return NotFound();

            ModelState.Remove(nameof(Class.Course));

            // The (possibly changed) target course must also belong to the teacher
            if (!await TeacherOwnsCourse(@class.CourseID))
                ModelState.AddModelError(nameof(Class.CourseID), "Please select one of your own courses.");

            if (ModelState.IsValid)
            {
                _context.Update(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateCoursesDropDown(@class.CourseID);
            return View(@class);
        }

        // GET: /Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var @class = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (@class == null || @class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            return View(@class);
        }

        // POST: /Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @class = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (@class != null && @class.Course.TeacherID == CurrentTeacherId)
            {
                _context.Classes.Remove(@class);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──

        private async Task<bool> TeacherOwnsCourse(int courseId) =>
            await _context.Courses.AnyAsync(c => c.ID == courseId && c.TeacherID == CurrentTeacherId);

        private async Task PopulateCoursesDropDown(int? selected)
        {
            var courses = await _context.Courses
                .Where(c => c.TeacherID == CurrentTeacherId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Courses = new SelectList(courses, "ID", "Name", selected);
            ViewBag.HasCourses = courses.Count > 0;
        }
    }
}
