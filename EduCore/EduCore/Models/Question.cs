namespace EduCore.Models
{
    public class Question
    {
        public int ID { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public string Choices { get; set; }

        // Navigation
        public List<ExamQuestion> ExamQuestions { get; set; } = new();
        public List<QuizQuestion> QuizQuestions { get; set; } = new();
    }
}
