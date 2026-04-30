using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Work
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime UploadDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "草稿";

        [StringLength(500)]
        public string? FilePath { get; set; }

        [StringLength(255)]
        public string? FileName { get; set; }

        public long FileSize { get; set; }

        [StringLength(32)]
        public string? FileMD5 { get; set; }

        public DateTime? FileUploadTime { get; set; }

        [StringLength(500)]
        public string? PreviewImage { get; set; }

        public int UserId { get; set; }

        public bool IsExcellent { get; set; } = false;

        public int Views { get; set; } = 0;

        public int Favorites { get; set; } = 0;
    }
}