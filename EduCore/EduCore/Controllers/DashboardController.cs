using EduCore.Data;
using EduCore.Helpers;
using EduCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        private int CurrentTeacherId => User.GetUserId();

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            var teacherId = CurrentTeacherId;

            var courses = await _context.Courses
                .Where(c => c.TeacherID == teacherId)
                .Include(c => c.Classes)
                .Include(c => c.StudentCourses)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var studentCount = await _context.StudentCourses
                .Where(sc => sc.Course.TeacherID == teacherId)
                .Select(sc => sc.StudentID)
                .Distinct()
                .CountAsync();

            var quizCount = await _context.Quizzes.CountAsync(q => q.Class.Course.TeacherID == teacherId);
            var examCount = await _context.Exams.CountAsync(e => e.Course.TeacherID == teacherId);

            // Recent student activity across the teacher's quizzes + exams
            var quizActivity = await _context.QuizAttempts
                .Where(a => a.Quiz.Class.Course.TeacherID == teacherId)
                .OrderByDescending(a => a.SubmittedAt)
                .Take(8)
                .Select(a => new RecentAttempt
                {
                    StudentName = a.Student.FName + " " + a.Student.LName,
                    AssessmentTitle = a.Quiz.Title,
                    Type = "Quiz",
                    Score = a.Score,
                    Total = a.TotalQuestions,
                    SubmittedAt = a.SubmittedAt
                })
                .ToListAsync();

            var examActivity = await _context.ExamAttempts
                .Where(a => a.Exam.Course.TeacherID == teacherId)
                .OrderByDescending(a => a.SubmittedAt)
                .Take(8)
                .Select(a => new RecentAttempt
                {
                    StudentName = a.Student.FName + " " + a.Student.LName,
                    AssessmentTitle = a.Exam.Title,
                    Type = "Exam",
                    Score = a.Score,
                    Total = a.TotalQuestions,
                    SubmittedAt = a.SubmittedAt
                })
                .ToListAsync();

            var vm = new TeacherDashboardViewModel
            {
                CourseCount = courses.Count,
                StudentCount = studentCount,
                ClassCount = courses.Sum(c => c.Classes.Count),
                AssessmentCount = quizCount + examCount,
                Courses = courses,
                RecentActivity = quizActivity.Concat(examActivity)
                    .OrderByDescending(r => r.SubmittedAt)
                    .Take(8)
                    .ToList()
            };

            return View(vm);
        }
    }
}
