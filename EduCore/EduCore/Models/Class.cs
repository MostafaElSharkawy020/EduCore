namespace EduCore.Models
{
    public class Class
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string PDF { get; set; }
        public string HomeworkPDF { get; set; }

        public int CourseID { get; set; }

        // Navigation
        public Course Course { get; set; }
        public List<Video> Videos { get; set; } = new();
        public List<Quiz> Quizzes { get; set; } = new();
    }
}
