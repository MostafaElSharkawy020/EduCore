using System.Security.Claims;
using EduCore.Data;
using EduCore.Helpers;
using EduCore.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize]   // any signed-in user (teacher or student)
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        private int UserId => User.GetUserId();
        private bool IsTeacher => User.IsInRole("Teacher");

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            await SetHeaderAsync();

            if (IsTeacher)
            {
                var t = await _context.Teachers.FindAsync(UserId);
                if (t == null) return NotFound();
                return View(new ProfileViewModel
                {
                    FName = t.FName, LName = t.LName, Email = t.Email,
                    PhoneNumber = t.PhoneNumber, Biography = t.Biography,
                    Role = "Teacher", IsTeacher = true
                });
            }
            else
            {
                var s = await _context.Students.FindAsync(UserId);
                if (s == null) return NotFound();
                return View(new ProfileViewModel
                {
                    FName = s.FName, LName = s.LName, Email = s.Email,
                    PhoneNumber = s.PhoneNumber, Role = "Student", IsTeacher = false
                });
            }
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            await SetHeaderAsync();

            if (IsTeacher)
            {
                var t = await _context.Teachers.FindAsync(UserId);
                if (t == null) return NotFound();
                return View(new EditProfileViewModel
                {
                    FName = t.FName, LName = t.LName, Email = t.Email,
                    PhoneNumber = t.PhoneNumber, Biography = t.Biography
                });
            }
            else
            {
                var s = await _context.Students.FindAsync(UserId);
                if (s == null) return NotFound();
                return View(new EditProfileViewModel
                {
                    FName = s.FName, LName = s.LName, Email = s.Email,
                    PhoneNumber = s.PhoneNumber
                });
            }
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var email = model.Email.Trim();

            if (IsTeacher)
            {
                var t = await _context.Teachers.FindAsync(UserId);
                if (t == null) return NotFound();

                if (await _context.Teachers.AnyAsync(x => x.Email == email && x.ID != t.ID))
                    ModelState.AddModelError(nameof(model.Email), "Another teacher already uses this email.");

                if (!ModelState.IsValid) { await SetHeaderAsync(); return View(model); }

                t.FName = model.FName;
                t.LName = model.LName;
                t.Email = email;
                t.PhoneNumber = model.PhoneNumber ?? string.Empty;
                t.Biography = model.Biography ?? string.Empty;
                await _context.SaveChangesAsync();
                await RefreshSignInAsync(t.ID, $"{t.FName} {t.LName}", "Teacher");
            }
            else
            {
                var s = await _context.Students.FindAsync(UserId);
                if (s == null) return NotFound();

                if (await _context.Students.AnyAsync(x => x.Email == email && x.ID != s.ID))
                    ModelState.AddModelError(nameof(model.Email), "Another student already uses this email.");

                if (!ModelState.IsValid) { await SetHeaderAsync(); return View(model); }

                s.FName = model.FName;
                s.LName = model.LName;
                s.Email = email;
                s.PhoneNumber = model.PhoneNumber ?? string.Empty;
                await _context.SaveChangesAsync();
                await RefreshSignInAsync(s.ID, $"{s.FName} {s.LName}", "Student");
            }

            TempData["ProfileMessage"] = "Profile updated.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Profile/Password
        public async Task<IActionResult> Password()
        {
            await SetHeaderAsync();
            return View(new ChangePasswordViewModel());
        }

        // POST: /Profile/Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Password(ChangePasswordViewModel model)
        {
            await SetHeaderAsync();
            if (!ModelState.IsValid)
                return View(model);

            if (IsTeacher)
            {
                var t = await _context.Teachers.FindAsync(UserId);
                if (t == null) return NotFound();
                if (!PasswordHasher.Verify(model.CurrentPassword, t.Password))
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
                    return View(model);
                }
                t.Password = PasswordHasher.Hash(model.NewPassword);
            }
            else
            {
                var s = await _context.Students.FindAsync(UserId);
                if (s == null) return NotFound();
                if (!PasswordHasher.Verify(model.CurrentPassword, s.Password))
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
                    return View(model);
                }
                s.Password = PasswordHasher.Hash(model.NewPassword);
            }

            await _context.SaveChangesAsync();
            TempData["ProfileMessage"] = "Password updated.";
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──

        private async Task SetHeaderAsync()
        {
            string fname = "", lname = "", email = "";
            if (IsTeacher)
            {
                var t = await _context.Teachers.FindAsync(UserId);
                if (t != null) { fname = t.FName; lname = t.LName; email = t.Email; }
            }
            else
            {
                var s = await _context.Students.FindAsync(UserId);
                if (s != null) { fname = s.FName; lname = s.LName; email = s.Email; }
            }

            ViewBag.HeaderName = $"{fname} {lname}".Trim();
            ViewBag.HeaderEmail = email;
            ViewBag.HeaderRole = IsTeacher ? "Teacher" : "Student";
            ViewBag.HeaderInitials =
                $"{(string.IsNullOrEmpty(fname) ? "" : fname[0].ToString())}{(string.IsNullOrEmpty(lname) ? "" : lname[0].ToString())}".ToUpper();
        }

        private async Task RefreshSignInAsync(int id, string name, string role)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, id.ToString()),
                new(ClaimTypes.Name, name),
                new(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true });
        }
    }
}
