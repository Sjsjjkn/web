using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Work> Works { get; set; }
        public DbSet<UserCollection> UserCollections { get; set; }
        public DbSet<StudentTeacher> StudentTeachers { get; set; }
        public DbSet<WorkStudent> WorkStudents { get; set; }
        public DbSet<ModerationItem> ModerationItems { get; set; }
        public DbSet<WorkFavorite> WorkFavorites { get; set; }
    }
}