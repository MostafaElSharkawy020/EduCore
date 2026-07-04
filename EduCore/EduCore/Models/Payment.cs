namespace EduCore.Models
{
    // A (simulated) purchase record for a course or a class.
    // Item details are stored as a snapshot so history survives if the course/class is deleted.
    public class Payment
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public int TeacherID { get; set; }      // the teacher who earned this sale (snapshot)
        public string ItemType { get; set; }   // "Course" or "Class"
        public string ItemName { get; set; }   // snapshot of the course/class name
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string CardLast4 { get; set; }

        // Navigation
        public Student Student { get; set; }
    }
}
