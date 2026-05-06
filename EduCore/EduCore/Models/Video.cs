namespace EduCore.Models
{
    public class Video
    {
        public int ID { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }

        public int ClassID { get; set; }

        // Navigation
        public Class Class { get; set; }
    }
}
