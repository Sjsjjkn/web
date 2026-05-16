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
        public DbSet<WorkFavorite> WorkFavorites { get; set; }
        public DbSet<WorkStudent> WorkStudents { get; set; }
        public DbSet<StudentTeacher> StudentTeachers { get; set; }
        public DbSet<ModerationItem> ModerationItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<WorkReview> WorkReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Work 配置 =====
            modelBuilder.Entity<Work>(entity =>
            {
                // Work.UserId → User.Id（上传关系，1:n）
                entity.HasOne(w => w.Uploader)
                    .WithMany(u => u.UploadedWorks)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // 有作品时不允许删除用户
            });

            // ===== WorkFavorite 配置 =====
            modelBuilder.Entity<WorkFavorite>(entity =>
            {
                // 联合唯一索引：防止重复收藏
                entity.HasIndex(f => new { f.UserId, f.WorkId }).IsUnique();

                // WorkFavorite.UserId → User.Id
                entity.HasOne(f => f.User)
                    .WithMany(u => u.WorkFavorites)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // WorkFavorite.WorkId → Work.Id
                entity.HasOne(f => f.Work)
                    .WithMany(w => w.WorkFavorites)
                    .HasForeignKey(f => f.WorkId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== WorkStudent 配置 =====
            modelBuilder.Entity<WorkStudent>(entity =>
            {
                // 联合唯一索引：防止重复参与
                entity.HasIndex(ws => new { ws.WorkId, ws.StudentId }).IsUnique();

                // WorkStudent.WorkId → Work.Id
                entity.HasOne(ws => ws.Work)
                    .WithMany(w => w.WorkStudents)
                    .HasForeignKey(ws => ws.WorkId)
                    .OnDelete(DeleteBehavior.Cascade);

                // WorkStudent.StudentId → User.Id
                entity.HasOne(ws => ws.Student)
                    .WithMany(u => u.WorkStudents)
                    .HasForeignKey(ws => ws.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== StudentTeacher 配置 =====
            modelBuilder.Entity<StudentTeacher>(entity =>
            {
                // 联合唯一索引：防止重复指导关系
                entity.HasIndex(st => new { st.StudentId, st.TeacherId }).IsUnique();

                // StudentTeacher.StudentId → User.Id
                entity.HasOne(st => st.Student)
                    .WithMany(u => u.Teachers)
                    .HasForeignKey(st => st.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // StudentTeacher.TeacherId → User.Id
                entity.HasOne(st => st.Teacher)
                    .WithMany(u => u.SupervisedStudents)
                    .HasForeignKey(st => st.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== WorkReview 配置 =====
            modelBuilder.Entity<WorkReview>(entity =>
            {
                // WorkReview.WorkId → Work.Id
                entity.HasOne(r => r.Work)
                    .WithMany()
                    .HasForeignKey(r => r.WorkId)
                    .OnDelete(DeleteBehavior.Cascade);

                // WorkReview.ReviewerId → User.Id
                entity.HasOne(r => r.Reviewer)
                    .WithMany()
                    .HasForeignKey(r => r.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== ModerationItem 配置 =====
            modelBuilder.Entity<ModerationItem>(entity =>
            {
                // ModerationItem.UserId → User.Id（可选）
                entity.HasOne(m => m.Submitter)
                    .WithMany()
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
