using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool Remember { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Student";

        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string WorkId { get; set; } = string.Empty;

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;

        [StringLength(50)]
        public string Title { get; set; } = string.Empty;

        [StringLength(200)]
        public string ResearchArea { get; set; } = string.Empty;

        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Avatar { get; set; }
    }
}