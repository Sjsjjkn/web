using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    /// <summary>
    /// 更新作品请求 DTO
    /// </summary>
    public class WorkUpdateRequest
    {
        /// <summary>作品标题</summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>作品分类</summary>
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>作品描述</summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>作品状态：草稿/待审核/已发布/审核拒绝</summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "草稿";

        /// <summary>文件存储路径</summary>
        [StringLength(500)]
        public string? FilePath { get; set; }

        /// <summary>原始文件名</summary>
        [StringLength(255)]
        public string? FileName { get; set; }

        /// <summary>文件大小（字节）</summary>
        public long FileSize { get; set; }

        /// <summary>文件MD5校验值</summary>
        [StringLength(32)]
        public string? FileMD5 { get; set; }

        /// <summary>预览图路径</summary>
        [StringLength(500)]
        public string? PreviewImage { get; set; }

        /// <summary>上传日期</summary>
        public DateTime? UploadDate { get; set; }

        /// <summary>文件上传时间</summary>
        public DateTime? FileUploadTime { get; set; }
    }
}
