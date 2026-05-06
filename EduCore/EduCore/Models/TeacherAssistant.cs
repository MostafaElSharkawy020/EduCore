namespace EduCore.Models
{
    public class TeacherAssistant
    {
        public int ID { get; set; }
        public int TeacherID { get; set; }
        public int AssistantID { get; set; }

        // Navigation
        public Teacher Teacher { get; set; }
        public Assistant Assistant { get; set; }
    }
}
