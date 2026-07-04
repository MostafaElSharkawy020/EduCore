namespace EduCore.ViewModels
{
    public class AssessmentResultsViewModel
    {
        public string Title { get; set; }
        public bool IsExam { get; set; }
        public int AssessmentId { get; set; }
        public string ParentName { get; set; }   // class name (quiz) or course name (exam)
        public int AttemptCount { get; set; }
        public int StudentCount { get; set; }     // distinct students
        public double AveragePercent { get; set; }
        public List<AttemptRow> Attempts { get; set; } = new();
    }

    public class AttemptRow
    {
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public int Score { get; set; }
        public int Total { get; set; }
        public double Percent { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
