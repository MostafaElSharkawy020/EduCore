namespace EduCore.Models
{
    public class Card
    {
        public int ID { get; set; }
        public string CardholderName { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CVV { get; set; }
        public string CardNumber { get; set; }

        public int StudentID { get; set; }

        // Navigation
        public Student Student { get; set; }
    }
}
