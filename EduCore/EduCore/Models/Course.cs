namespace EduCore.Models
{
    public class Course
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Enrollable { get; set; }
        public decimal Price { get; set; }

        public int TeacherID { get; set; }

        // Navigation
        public Teacher Teacher { get; set; }
        public List<Class> Classes { get; set; } = new();
        public List<Exam> Exams { get; set; } = new();
        public List<StudentCourse> StudentCourses { get; set; } = new();
    }
}
