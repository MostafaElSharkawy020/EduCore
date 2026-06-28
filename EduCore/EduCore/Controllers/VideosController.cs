using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class VideosController : Controller
    {
        private readonly AppDbContext _context;

        // The signed-in teacher's id (from the auth cookie).
        private int CurrentTeacherId => User.GetUserId();

        public VideosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Videos?classId=5
        public async Task<IActionResult> Index(int? classId)
        {
            if (classId == null)
                return NotFound();

            var @class = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ID == classId);

            if (@class == null || @class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            var videos = await _context.Videos
                .Where(v => v.ClassID == classId)
                .ToListAsync();

            ViewBag.Class = @class;
            return View(videos);
        }

        // GET: /Videos/Create?classId=5
        public async Task<IActionResult> Create(int? classId)
        {
            if (classId == null || !await TeacherOwnsClass(classId.Value))
                return NotFound();

            ViewBag.Class = await _context.Classes.FindAsync(classId);
            return View(new Video { ClassID = classId.Value });
        }

        // POST: /Videos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,URL,ClassID")] Video video)
        {
            ModelState.Remove(nameof(Video.Class));

            if (!await TeacherOwnsClass(video.ClassID))
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Add(video);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { classId = video.ClassID });
            }

            ViewBag.Class = await _context.Classes.FindAsync(video.ClassID);
            return View(video);
        }

        // GET: /Videos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var video = await _context.Videos
                .Include(v => v.Class).ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(v => v.ID == id);

            if (video == null || video.Class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            ViewBag.Class = video.Class;
            return View(video);
        }

        // POST: /Videos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,URL,ClassID")] Video video)
        {
            if (id != video.ID)
                return NotFound();

            // Confirm the teacher owns the class this video currently belongs to
            var currentClassId = await _context.Videos
                .AsNoTracking()
                .Where(v => v.ID == id)
                .Select(v => (int?)v.ClassID)
                .FirstOrDefaultAsync();

            if (currentClassId == null || !await TeacherOwnsClass(currentClassId.Value))
                return NotFound();

            ModelState.Remove(nameof(Video.Class));

            // The (possibly changed) target class must also belong to the teacher
            if (!await TeacherOwnsClass(video.ClassID))
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(video);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { classId = video.ClassID });
            }

            ViewBag.Class = await _context.Classes.FindAsync(video.ClassID);
            return View(video);
        }

        // GET: /Videos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var video = await _context.Videos
                .Include(v => v.Class).ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(v => v.ID == id);

            if (video == null || video.Class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            return View(video);
        }

        // POST: /Videos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var video = await _context.Videos
                .Include(v => v.Class).ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(v => v.ID == id);

            int? classId = video?.ClassID;

            if (video != null && video.Class.Course.TeacherID == CurrentTeacherId)
            {
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { classId });
        }

        private async Task<bool> TeacherOwnsClass(int classId) =>
            await _context.Classes.AnyAsync(c => c.ID == classId && c.Course.TeacherID == CurrentTeacherId);
    }
}
