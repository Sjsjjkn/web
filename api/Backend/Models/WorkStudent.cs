using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class WorkStudent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkId { get; set; }

        [ForeignKey("WorkId")]
        public Work Work { get; set; } = null!;

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public User Student { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}