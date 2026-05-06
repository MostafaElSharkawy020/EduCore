namespace EduCore.Models
{
    public class StudentCourse
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public int CourseID { get; set; }

        // Navigation
        public Student Student { get; set; }
        public Course Course { get; set; }
    }
}
