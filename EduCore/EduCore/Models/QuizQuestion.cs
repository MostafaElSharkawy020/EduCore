namespace EduCore.Models
{
    public class QuizQuestion
    {
        public int ID { get; set; }
        public int QuizID { get; set; }
        public int QuestionID { get; set; }

        // Navigation
        public Quiz Quiz { get; set; }
        public Question Question { get; set; }
    }
}
