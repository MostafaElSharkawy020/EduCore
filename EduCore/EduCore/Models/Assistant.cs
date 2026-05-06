namespace EduCore.Models
{
    public class Assistant
    {
        public int ID { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LName { get; set; }
        public string FName { get; set; }
        public string Biography { get; set; }
        public string Password { get; set; }

        // Navigation
        public List<TeacherAssistant> TeacherAssistants { get; set; } = new();
    }
}
