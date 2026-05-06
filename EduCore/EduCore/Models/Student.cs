namespace EduCore.Models
{
    public class Student
    {
        public int ID { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }

        // Navigation
        public List<Card> Cards { get; set; } = new();
        public List<StudentCourse> StudentCourses { get; set; } = new();
    }
}
