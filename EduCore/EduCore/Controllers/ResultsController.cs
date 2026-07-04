using EduCore.Data;
using EduCore.Helpers;
using EduCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class ResultsController : Controller
    {
        private readonly AppDbContext _context;

        private int CurrentTeacherId => User.GetUserId();

        public ResultsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Results/Quiz/5  — attempts for one of the teacher's quizzes
        public async Task<IActionResult> Quiz(int? id)
        {
            if (id == null) return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Class).ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(q => q.ID == id);

            if (quiz == null || quiz.Class.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            var attempts = await _context.QuizAttempts
                .Where(a => a.QuizID == id)
                .Include(a => a.Student)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            var rows = attempts.Select(ToRow).ToList();
            return View("Index", Build(quiz.Title, isExam: false, quiz.ID, quiz.Class.Name, rows,
                attempts.Select(a => a.StudentID).Distinct().Count()));
        }

        // GET: /Results/Exam/5  — attempts for one of the teacher's exams
        public async Task<IActionResult> Exam(int? id)
        {
            if (id == null) return NotFound();

            var exam = await _context.Exams
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (exam == null || exam.Course.TeacherID != CurrentTeacherId)
                return NotFound();

            var attempts = await _context.ExamAttempts
                .Where(a => a.ExamID == id)
                .Include(a => a.Student)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            var rows = attempts.Select(ToRow).ToList();
            return View("Index", Build(exam.Title, isExam: true, exam.ID, exam.Course.Name, rows,
                attempts.Select(a => a.StudentID).Distinct().Count()));
        }

        // ── Helpers ──

        private static AttemptRow ToRow(dynamic a) => new AttemptRow
        {
            StudentName = $"{a.Student.FName} {a.Student.LName}".Trim(),
            StudentEmail = a.Student.Email,
            Score = a.Score,
            Total = a.TotalQuestions,
            Percent = a.TotalQuestions > 0 ? 100.0 * a.Score / a.TotalQuestions : 0,
            SubmittedAt = a.SubmittedAt
        };

        private static AssessmentResultsViewModel Build(
            string title, bool isExam, int id, string parentName, List<AttemptRow> rows, int studentCount)
            => new AssessmentResultsViewModel
            {
                Title = title,
                IsExam = isExam,
                AssessmentId = id,
                ParentName = parentName,
                AttemptCount = rows.Count,
                StudentCount = studentCount,
                AveragePercent = rows.Count > 0 ? rows.Average(r => r.Percent) : 0,
                Attempts = rows
            };
    }
}
