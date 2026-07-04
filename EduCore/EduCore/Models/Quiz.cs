using System.ComponentModel.DataAnnotations;

namespace EduCore.Models
{
    public class Quiz
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Quiz title is required.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
        [Display(Name = "Quiz Title")]
        public string Title { get; set; }

        [Range(0, 600, ErrorMessage = "Time limit must be between 0 and 600 minutes (0 = no limit).")]
        [Display(Name = "Time Limit (minutes)")]
        public int DurationMinutes { get; set; }

        [Display(Name = "Class")]
        public int ClassID { get; set; }

        // Navigation
        public Class Class { get; set; }
        public List<QuizQuestion> QuizQuestions { get; set; } = new();
    }
}
