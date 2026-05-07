using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// 作品收藏表 - 记录用户收藏作品的关系
    /// </summary>
    public class WorkFavorite
    {
        [Key]
        public int Id { get; set; }

        /// <summary>收藏者ID</summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>被收藏的作品ID</summary>
        [Required]
        public int WorkId { get; set; }

        /// <summary>收藏时间</summary>
        public DateTime FavoriteDate { get; set; } = DateTime.Now;

        // 导航属性
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("WorkId")]
        public Work Work { get; set; } = null!;
    }
}
