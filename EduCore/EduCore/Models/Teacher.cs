namespace EduCore.Models
{
    public class Teacher
    {
        public int ID { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Biography { get; set; }

        // Navigation
        public List<Course> Courses { get; set; } = new();
        public List<TeacherAssistant> TeacherAssistants { get; set; } = new();
    }
}
