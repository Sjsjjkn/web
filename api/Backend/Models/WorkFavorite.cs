using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class WorkFavorite
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int WorkId { get; set; }

        public DateTime FavoriteDate { get; set; } = DateTime.Now;

        // 导航属性
        public User User { get; set; }
        public Work Work { get; set; }
    }
}