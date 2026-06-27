using System.ComponentModel.DataAnnotations;

namespace EduCore.Models
{
    public class Choice
    {
        public int ID { get; set; }

        [Required]
        [StringLength(300)]
        public string Text { get; set; }

        public bool IsCorrect { get; set; }

        public int QuestionID { get; set; }

        // Navigation
        public Question Question { get; set; }
    }
}
