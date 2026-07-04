using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using EduCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Student")]
    public class CardsController : Controller
    {
        private readonly AppDbContext _context;

        private int CurrentStudentId => User.GetUserId();

        public CardsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Cards  — saved cards + purchase history
        public async Task<IActionResult> Index()
        {
            var cards = await _context.Cards
                .Where(c => c.StudentID == CurrentStudentId)
                .ToListAsync();

            ViewBag.Payments = await _context.Payments
                .Where(p => p.StudentID == CurrentStudentId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();

            return View(cards);
        }

        // GET: /Cards/Create
        public IActionResult Create()
        {
            return View(new CardFormViewModel());
        }

        // POST: /Cards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CardFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var digits = new string(model.CardNumber.Where(char.IsDigit).ToArray());

            // NOTE: storing the full card number and CVV is NOT PCI-compliant.
            // This is a simulated academic project — never do this in production.
            _context.Cards.Add(new Card
            {
                StudentID = CurrentStudentId,
                CardholderName = model.CardholderName,
                CardNumber = digits,
                CVV = model.CVV,
                ExpiryDate = ParseExpiry(model.Expiry)
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cards/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var card = await _context.Cards
                .FirstOrDefaultAsync(c => c.ID == id && c.StudentID == CurrentStudentId);

            if (card != null)
            {
                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private static DateTime ParseExpiry(string mmYy)
        {
            var parts = mmYy.Split('/');
            var month = int.Parse(parts[0]);
            var year = 2000 + int.Parse(parts[1]);
            var lastDay = DateTime.DaysInMonth(year, month);
            return new DateTime(year, month, lastDay);
        }
    }
}
