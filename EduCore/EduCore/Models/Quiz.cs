namespace EduCore.Models
{
    public class Quiz
    {
        public int ID { get; set; }
        public string Title { get; set; }

        public int ClassID { get; set; }

        // Navigation
        public Class Class { get; set; }
        public List<QuizQuestion> QuizQuestions { get; set; } = new();
    }
}
