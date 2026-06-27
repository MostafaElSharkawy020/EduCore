using System.ComponentModel.DataAnnotations;

namespace EduCore.ViewModels
{
    public class QuestionFormViewModel
    {
        public int? ID { get; set; }          // question id (null when creating)
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; }

        [Required(ErrorMessage = "Enter the answer choices, one per line.")]
        [Display(Name = "Answer Choices (one per line)")]
        public string ChoicesText { get; set; }

        [Display(Name = "Correct choice number")]
        [Range(1, 10, ErrorMessage = "Pick a valid choice number.")]
        public int CorrectNumber { get; set; } = 1;
    }
}
