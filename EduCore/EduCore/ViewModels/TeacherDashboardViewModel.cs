using EduCore.Models;

namespace EduCore.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public int CourseCount { get; set; }
        public int StudentCount { get; set; }
        public int ClassCount { get; set; }
        public int AssessmentCount { get; set; }   // quizzes + exams
        public List<Course> Courses { get; set; } = new();
        public List<RecentAttempt> RecentActivity { get; set; } = new();
    }

    public class RecentAttempt
    {
        public string StudentName { get; set; }
        public string AssessmentTitle { get; set; }
        public string Type { get; set; }   // "Quiz" or "Exam"
        public int Score { get; set; }
        public int Total { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
