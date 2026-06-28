namespace EduCore.Models
{
    public class ExamAttempt
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public int ExamID { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmittedAt { get; set; }

        // Navigation
        public Student Student { get; set; }
        public Exam Exam { get; set; }
    }
}
