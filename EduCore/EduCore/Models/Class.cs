using System.ComponentModel.DataAnnotations;

namespace EduCore.Models
{
    public class Class
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Class name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Class name must be between 3 and 100 characters.")]
        [Display(Name = "Class Name")]
        public string Name { get; set; }

        [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Class Price")]
        public decimal Price { get; set; }

        [Display(Name = "Open for enrollment")]
        public bool Enrollable { get; set; }

        [Display(Name = "Lecture Notes PDF (URL or path)")]
        public string? PDF { get; set; }

        [Display(Name = "Homework PDF (URL or path)")]
        public string? HomeworkPDF { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a parent course.")]
        [Display(Name = "Parent Course")]
        public int CourseID { get; set; }

        // Navigation
        public Course Course { get; set; }
        public List<Video> Videos { get; set; } = new();
        public List<Quiz> Quizzes { get; set; } = new();
        public List<StudentClass> StudentClasses { get; set; } = new();
    }
}
