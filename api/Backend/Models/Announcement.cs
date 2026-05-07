using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// 公告表 - 存储系统公告信息
    /// </summary>
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        /// <summary>公告标题</summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>公告内容</summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>发布者ID</summary>
        public int PublisherId { get; set; }

        /// <summary>是否置顶</summary>
        public bool IsPinned { get; set; } = false;

        /// <summary>是否启用</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>发布时间</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>更新时间</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>导航属性：发布者</summary>
        [ForeignKey("PublisherId")]
        public User Publisher { get; set; } = null!;
    }
}