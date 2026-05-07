using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// 通知表 - 存储系统通知消息
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        /// <summary>接收用户ID</summary>
        public int UserId { get; set; }

        /// <summary>通知类型：work_approval(作品审核), system(系统公告), message(私信)</summary>
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "system";

        /// <summary>通知标题</summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>通知内容</summary>
        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;

        /// <summary>关联的作品ID（可选）</summary>
        public int? WorkId { get; set; }

        /// <summary>是否已读</summary>
        public bool IsRead { get; set; } = false;

        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>导航属性：接收用户</summary>
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        /// <summary>导航属性：关联作品</summary>
        [ForeignKey("WorkId")]
        public Work? Work { get; set; }
    }
}
