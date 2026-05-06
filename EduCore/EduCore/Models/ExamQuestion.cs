namespace EduCore.Models
{
    public class ExamQuestion
    {
        public int ID { get; set; }
        public int ExamID { get; set; }
        public int QuestionID { get; set; }

        // Navigation
        public Exam Exam { get; set; }
        public Question Question { get; set; }
    }
}
