using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// 浏览历史表 - 记录用户浏览作品的历史
    /// </summary>
    public class ViewHistory
    {
        [Key]
        public int Id { get; set; }

        /// <summary>浏览者ID</summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>被浏览的作品ID</summary>
        [Required]
        public int WorkId { get; set; }

        /// <summary>浏览时间</summary>
        public DateTime ViewedAt { get; set; } = DateTime.Now;

        // 导航属性
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("WorkId")]
        public Work Work { get; set; } = null!;
    }
}
