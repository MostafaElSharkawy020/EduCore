using System.ComponentModel.DataAnnotations;

namespace EduCore.Models
{
    public class Question
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; }

        // Navigation
        public List<Choice> Choices { get; set; } = new();
        public List<ExamQuestion> ExamQuestions { get; set; } = new();
        public List<QuizQuestion> QuizQuestions { get; set; } = new();
    }
}
