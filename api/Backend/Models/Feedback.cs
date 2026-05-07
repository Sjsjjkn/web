using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// 用户反馈表 - 存储用户对系统的意见和建议
    /// </summary>
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        /// <summary>提交用户ID</summary>
        public int UserId { get; set; }

        /// <summary>反馈类型：suggestion(建议), complaint(投诉), question(问题咨询), other(其他)</summary>
        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "suggestion";

        /// <summary>反馈标题</summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>反馈内容</summary>
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>联系方式（可选）</summary>
        [StringLength(100)]
        public string? Contact { get; set; }

        /// <summary>处理状态：pending(待处理), processing(处理中), resolved(已解决), rejected(已驳回)</summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        /// <summary>管理员回复（可选）</summary>
        [StringLength(500)]
        public string? Reply { get; set; }

        /// <summary>回复时间</summary>
        public DateTime? RepliedAt { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>导航属性：提交用户</summary>
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
