namespace EduCore.Models
{
    public class Exam
    {
        public int ID { get; set; }
        public string Title { get; set; }

        public int CourseID { get; set; }

        // Navigation
        public Course Course { get; set; }
        public List<ExamQuestion> ExamQuestions { get; set; } = new();
    }
}
