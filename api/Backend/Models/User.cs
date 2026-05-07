using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    /// <summary>
    /// 用户表 - 系统用户（学生/教师/管理员）
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        /// <summary>登录用户名</summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>密码（加密存储）</summary>
        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>记住登录状态</summary>
        public bool Remember { get; set; }

        /// <summary>角色：Student / Teacher / Admin</summary>
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Student";

        /// <summary>用户姓名</summary>
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>学号/工号</summary>
        [StringLength(20)]
        public string WorkId { get; set; } = string.Empty;

        /// <summary>院系/部门</summary>
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        /// <summary>联系电话</summary>
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        /// <summary>电子邮箱</summary>
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>个人简介</summary>
        [StringLength(500)]
        public string Bio { get; set; } = string.Empty;

        /// <summary>职称（教师用）</summary>
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;

        /// <summary>研究领域（教师用）</summary>
        [StringLength(200)]
        public string ResearchArea { get; set; } = string.Empty;

        /// <summary>职位</summary>
        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        /// <summary>头像路径</summary>
        [StringLength(500)]
        public string? Avatar { get; set; }

        // ===== 用户偏好设置 =====

        /// <summary>界面主题：light / dark</summary>
        [StringLength(20)]
        public string Theme { get; set; } = "light";

        /// <summary>界面语言：zh-CN / en</summary>
        [StringLength(20)]
        public string Language { get; set; } = "zh-CN";

        /// <summary>是否开启消息通知</summary>
        public bool NotificationEnabled { get; set; } = true;

        /// <summary>是否开启邮件通知</summary>
        public bool EmailNotification { get; set; } = true;

        /// <summary>是否需要作品审核</summary>
        public bool WorkModerationRequired { get; set; } = false;

        /// <summary>个人主页是否公开</summary>
        public bool ProfilePublic { get; set; } = true;

        /// <summary>是否展示联系方式</summary>
        public bool ShowContactInfo { get; set; } = false;

        /// <summary>收藏可见性：public / followers / private</summary>
        [StringLength(20)]
        public string FavoritesVisibility { get; set; } = "public";

        // 导航属性
        /// <summary>上传的作品（User 1→n Work）</summary>
        public ICollection<Work> UploadedWorks { get; set; } = new List<Work>();

        /// <summary>收藏记录（User m↔n Work）</summary>
        public ICollection<WorkFavorite> WorkFavorites { get; set; } = new List<WorkFavorite>();

        /// <summary>参与团队作品记录（Student m↔n Work）</summary>
        public ICollection<WorkStudent> WorkStudents { get; set; } = new List<WorkStudent>();

        /// <summary>被指导的学生（Teacher m↔n Student）</summary>
        public ICollection<StudentTeacher> SupervisedStudents { get; set; } = new List<StudentTeacher>();

        /// <summary>指导教师（Student m↔n Teacher）</summary>
        public ICollection<StudentTeacher> Teachers { get; set; } = new List<StudentTeacher>();
    }
}
