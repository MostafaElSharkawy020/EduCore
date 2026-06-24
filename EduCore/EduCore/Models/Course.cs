using System.ComponentModel.DataAnnotations;

namespace EduCore.Models
{
    public class Course
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Course name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 100 characters.")]
        [Display(Name = "Course Name")]
        public string Name { get; set; }

        [Display(Name = "Open for Enrollment")]
        public bool Enrollable { get; set; }

        [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Price (USD)")]
        public decimal Price { get; set; }

        public int TeacherID { get; set; }

        // Navigation
        public Teacher Teacher { get; set; }
        public List<Class> Classes { get; set; } = new();
        public List<Exam> Exams { get; set; } = new();
        public List<StudentCourse> StudentCourses { get; set; } = new();
    }
}
