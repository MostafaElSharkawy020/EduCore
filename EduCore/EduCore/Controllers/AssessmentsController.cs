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
    public class AssessmentsController : Controller
    {
        private readonly AppDbContext _context;

        private int CurrentStudentId => User.GetUserId();

        public AssessmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Assessments/Quiz/5
        public async Task<IActionResult> Quiz(int? id)
        {
            if (id == null) return NotFound();

            var quiz = await LoadQuizAsync(id.Value);
            if (quiz == null) return NotFound();
            if (!await IsEnrolled(quiz.Class.CourseID))
                return RedirectToAction("Details", "Catalog", new { id = quiz.Class.CourseID });

            var questions = quiz.QuizQuestions.Select(qq => qq.Question);
            return View("Take", BuildTake(quiz.Title, isExam: false, quiz.ID, quiz.ClassID, quiz.DurationMinutes, questions));
        }

        // POST: /Assessments/SubmitQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(int quizId, [FromForm] Dictionary<int, int>? answers)
        {
            var quiz = await LoadQuizAsync(quizId);
            if (quiz == null) return NotFound();
            if (!await IsEnrolled(quiz.Class.CourseID))
                return RedirectToAction("Details", "Catalog", new { id = quiz.Class.CourseID });

            var questions = quiz.QuizQuestions.Select(qq => qq.Question).ToList();
            var result = Grade(quiz.Title, isExam: false, quiz.ID, quiz.ClassID, questions, answers ?? new());

            _context.QuizAttempts.Add(new QuizAttempt
            {
                StudentID = CurrentStudentId,
                QuizID = quiz.ID,
                Score = result.Score,
                TotalQuestions = result.Total,
                SubmittedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return View("Result", result);
        }

        // GET: /Assessments/Exam/5
        public async Task<IActionResult> Exam(int? id)
        {
            if (id == null) return NotFound();

            var exam = await LoadExamAsync(id.Value);
            if (exam == null) return NotFound();
            if (!await IsEnrolled(exam.CourseID))
                return RedirectToAction("Details", "Catalog", new { id = exam.CourseID });

            var questions = exam.ExamQuestions.Select(eq => eq.Question);
            return View("Take", BuildTake(exam.Title, isExam: true, exam.ID, exam.CourseID, exam.DurationMinutes, questions));
        }

        // POST: /Assessments/SubmitExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExam(int examId, [FromForm] Dictionary<int, int>? answers)
        {
            var exam = await LoadExamAsync(examId);
            if (exam == null) return NotFound();
            if (!await IsEnrolled(exam.CourseID))
                return RedirectToAction("Details", "Catalog", new { id = exam.CourseID });

            var questions = exam.ExamQuestions.Select(eq => eq.Question).ToList();
            var result = Grade(exam.Title, isExam: true, exam.ID, exam.CourseID, questions, answers ?? new());

            _context.ExamAttempts.Add(new ExamAttempt
            {
                StudentID = CurrentStudentId,
                ExamID = exam.ID,
                Score = result.Score,
                TotalQuestions = result.Total,
                SubmittedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return View("Result", result);
        }

        // ── Helpers ──

        private Task<Quiz?> LoadQuizAsync(int id) =>
            _context.Quizzes
                .Include(q => q.Class).ThenInclude(c => c.Course)
                .Include(q => q.QuizQuestions).ThenInclude(qq => qq.Question).ThenInclude(qn => qn.Choices)
                .FirstOrDefaultAsync(q => q.ID == id);

        private Task<Exam?> LoadExamAsync(int id) =>
            _context.Exams
                .Include(e => e.Course)
                .Include(e => e.ExamQuestions).ThenInclude(eq => eq.Question).ThenInclude(qn => qn.Choices)
                .FirstOrDefaultAsync(e => e.ID == id);

        private async Task<bool> IsEnrolled(int courseId) =>
            await _context.StudentCourses.AnyAsync(sc => sc.StudentID == CurrentStudentId && sc.CourseID == courseId);

        private static TakeAssessmentViewModel BuildTake(string title, bool isExam, int id, int backId, int durationMinutes, IEnumerable<Question> questions)
        {
            var vm = new TakeAssessmentViewModel { Title = title, IsExam = isExam, AssessmentId = id, BackId = backId, DurationMinutes = durationMinutes };
            foreach (var q in questions)
            {
                vm.Questions.Add(new TakeQuestion
                {
                    QuestionId = q.ID,
                    Text = q.QuestionText,
                    Choices = q.Choices.OrderBy(c => c.ID)
                        .Select(c => new TakeChoice { ChoiceId = c.ID, Text = c.Text })
                        .ToList()
                });
            }
            return vm;
        }

        private static AssessmentResultViewModel Grade(
            string title, bool isExam, int id, int backId, List<Question> questions, Dictionary<int, int> answers)
        {
            var vm = new AssessmentResultViewModel
            {
                Title = title, IsExam = isExam, AssessmentId = id, BackId = backId, Total = questions.Count
            };

            foreach (var q in questions)
            {
                answers.TryGetValue(q.ID, out var selectedId);   // 0 if unanswered
                var correctId = q.Choices.FirstOrDefault(c => c.IsCorrect)?.ID ?? -1;
                var isCorrect = selectedId != 0 && selectedId == correctId;
                if (isCorrect) vm.Score++;

                var item = new ResultItem { QuestionText = q.QuestionText, IsCorrect = isCorrect };
                foreach (var c in q.Choices.OrderBy(c => c.ID))
                {
                    item.Choices.Add(new ResultChoice
                    {
                        Text = c.Text,
                        IsCorrect = c.IsCorrect,
                        IsSelected = c.ID == selectedId
                    });
                }
                vm.Items.Add(item);
            }

            return vm;
        }
    }
}
