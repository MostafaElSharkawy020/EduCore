namespace EduCore.Models
{
    public class QuizAttempt
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public int QuizID { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmittedAt { get; set; }

        // Navigation
        public Student Student { get; set; }
        public Quiz Quiz { get; set; }
    }
}
