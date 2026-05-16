using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// 作品评语/反馈表 - 教师对学生作品的评语，学生重新提交说明
    /// </summary>
    public class WorkReview
    {
        [Key]
        public int Id { get; set; }

        /// <summary>关联作品ID</summary>
        public int WorkId { get; set; }

        /// <summary>审阅者ID（教师或学生）</summary>
        public int ReviewerId { get; set; }

        /// <summary>评语/反馈内容</summary>
        [Required]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        /// <summary>类型：review（教师评语）/ resubmit（学生重新提交说明）</summary>
        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "review";

        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 导航属性
        [ForeignKey("WorkId")]
        public Work? Work { get; set; }

        [ForeignKey("ReviewerId")]
        public User? Reviewer { get; set; }
    }
}