using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// 审核项目表 - 记录内容审核信息
    /// </summary>
    public class ModerationItem
    {
        [Key]
        public int Id { get; set; }

        /// <summary>审核内容标题</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>作者姓名</summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>内容分类</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>内容类型：image / video / document / text</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>关联文件名</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>预览内容</summary>
        public string Preview { get; set; } = string.Empty;

        /// <summary>审核内容</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>提交时间</summary>
        public DateTime SubmitTime { get; set; } = DateTime.Now;

        /// <summary>风险等级：low / medium / high</summary>
        public string RiskLevel { get; set; } = string.Empty;

        /// <summary>风险详情（用 | 分隔多个风险）</summary>
        public string RiskDetails { get; set; } = string.Empty;

        /// <summary>审核状态：pending / approved / rejected</summary>
        public string Status { get; set; } = "pending";

        /// <summary>提交者ID（外键 → User）</summary>
        public int? UserId { get; set; }

        // 导航属性
        /// <summary>提交者</summary>
        [ForeignKey("UserId")]
        public User? Submitter { get; set; }
    }
}
