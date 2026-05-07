using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    /// <summary>
    /// 通知服务 - 处理系统通知相关逻辑
    /// </summary>
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 发送作品审核结果通知
        /// </summary>
        public async Task SendWorkApprovalNotification(int userId, int workId, string workTitle, bool isApproved, string? reason = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = "work_approval",
                WorkId = workId,
                Title = isApproved ? "作品审核通过" : "作品审核未通过",
                Content = isApproved 
                    ? $"您的作品「{workTitle}」已通过审核，现在可以在展厅中展示。"
                    : $"您的作品「{workTitle}」未通过审核，原因：{reason ?? "未提供具体原因"}。请修改后重新提交。",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 发送系统公告
        /// </summary>
        public async Task SendSystemAnnouncement(string title, string content, List<int>? targetUserIds = null)
        {
            List<User> targetUsers;
            
            if (targetUserIds != null && targetUserIds.Any())
            {
                // 发送给指定用户
                targetUsers = await _context.Users
                    .Where(u => targetUserIds.Contains(u.Id))
                    .ToListAsync();
            }
            else
            {
                // 发送给所有用户
                targetUsers = await _context.Users.ToListAsync();
            }

            var notifications = targetUsers.Select(user => new Notification
            {
                UserId = user.Id,
                Type = "system",
                Title = title,
                Content = content,
                IsRead = false,
                CreatedAt = DateTime.Now
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 获取用户通知列表
        /// </summary>
        public async Task<List<Notification>> GetUserNotifications(int userId, bool includeUnreadOnly = false)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (includeUnreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// 获取未读通知数量
        /// </summary>
        public async Task<int> GetUnreadCount(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        public async Task MarkAsRead(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 标记所有通知为已读
        /// </summary>
        public async Task MarkAllAsRead(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        public async Task DeleteNotification(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}
