using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class StudentTeacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public User Student { get; set; } = null!;

        [Required]
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public User Teacher { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}