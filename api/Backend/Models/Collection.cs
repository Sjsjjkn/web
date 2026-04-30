using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    /// <summary>
/// 收藏模型
/// </summary>
public class UserCollection
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// 作品ID
        /// </summary>
        [Required]
        public int WorkId { get; set; }
        
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public int UserId { get; set; }
        
        /// <summary>
        /// 收藏时间
        /// </summary>
        [Required]
        public DateTime CollectionDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 导航属性：关联的作品
        /// </summary>
        public Work? Work { get; set; }
        
        /// <summary>
        /// 导航属性：关联的用户
        /// </summary>
        public User? User { get; set; }
    }
}