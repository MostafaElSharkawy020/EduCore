using System.ComponentModel.DataAnnotations;

namespace EduCore.Models
{
    public class Video
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Video URL is required.")]
        [Url(ErrorMessage = "Enter a valid URL (YouTube, Vimeo, or a direct video link).")]
        [Display(Name = "Video URL")]
        public string URL { get; set; }

        [Required(ErrorMessage = "Video title is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 150 characters.")]
        [Display(Name = "Video Title")]
        public string Title { get; set; }

        [Display(Name = "Class")]
        public int ClassID { get; set; }

        // Navigation
        public Class Class { get; set; }
    }
}
