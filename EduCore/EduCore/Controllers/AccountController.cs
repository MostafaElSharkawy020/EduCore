using System.Security.Claims;
using EduCore.Data;
using EduCore.Helpers;
using EduCore.Models;
using EduCore.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim();

            // Look up only the table for the chosen role
            if (model.Role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == email);
                if (teacher != null && PasswordHasher.Verify(model.Password, teacher.Password))
                {
                    await SignInAsync(teacher.ID, $"{teacher.FName} {teacher.LName}", "Teacher");
                    return RedirectToLocal(returnUrl, "Teacher");
                }
            }
            else
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
                if (student != null && PasswordHasher.Verify(model.Password, student.Password))
                {
                    await SignInAsync(student.ID, $"{student.FName} {student.LName}", "Student");
                    return RedirectToLocal(returnUrl, "Student");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var role = model.Role == "Teacher" ? "Teacher" : "Student";
            var email = model.Email.Trim();

            // Email must be unique within the chosen role's table
            var taken = role == "Teacher"
                ? await _context.Teachers.AnyAsync(t => t.Email == email)
                : await _context.Students.AnyAsync(s => s.Email == email);
            if (taken)
            {
                ModelState.AddModelError(nameof(model.Email), $"A {role.ToLower()} account with this email already exists.");
                return View(model);
            }

            var hash = PasswordHasher.Hash(model.Password);
            int newId;
            string name;

            if (role == "Teacher")
            {
                var teacher = new Teacher
                {
                    FName = model.FName,
                    LName = model.LName,
                    Email = email,
                    Password = hash,
                    PhoneNumber = model.PhoneNumber ?? string.Empty,
                    Biography = string.Empty
                };
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();
                newId = teacher.ID;
                name = $"{teacher.FName} {teacher.LName}";
            }
            else
            {
                var student = new Student
                {
                    FName = model.FName,
                    LName = model.LName,
                    Email = email,
                    Password = hash,
                    PhoneNumber = model.PhoneNumber ?? string.Empty
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                newId = student.ID;
                name = $"{student.FName} {student.LName}";
            }

            await SignInAsync(newId, name, role);
            return RedirectToLocal(null, role);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ── Helpers ──

        private async Task SignInAsync(int id, string name, string role)
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

        private IActionResult RedirectToLocal(string? returnUrl, string role)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return role == "Teacher"
                ? RedirectToAction("Index", "Courses")
                : RedirectToAction("Index", "Learn");
        }
    }
}
