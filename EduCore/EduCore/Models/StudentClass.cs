namespace EduCore.Models
{
    // A student's enrollment in an individual class (à la carte purchase).
    public class StudentClass
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public int ClassID { get; set; }

        // Navigation
        public Student Student { get; set; }
        public Class Class { get; set; }
    }
}
