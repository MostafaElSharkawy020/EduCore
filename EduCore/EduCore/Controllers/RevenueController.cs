using EduCore.Data;
using EduCore.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class RevenueController : Controller
    {
        private readonly AppDbContext _context;

        private int CurrentTeacherId => User.GetUserId();

        public RevenueController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Revenue  — the teacher's earnings from course/class sales
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Where(p => p.TeacherID == CurrentTeacherId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();

            var gross = payments.Sum(p => p.Amount);

            ViewBag.Gross = gross;
            ViewBag.Fee = gross * PlatformSettings.PlatformFeeRate;
            ViewBag.Net = gross * PlatformSettings.TeacherShareRate;
            ViewBag.SalesCount = payments.Count;

            return View(payments);
        }
    }
}
