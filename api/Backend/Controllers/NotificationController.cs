using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// 获取当前用户的通知列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetNotifications(
            [FromQuery] bool unreadOnly = false,
            [FromQuery] bool? isRead = null,
            [FromQuery] string? type = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "未授权" });

            var notifications = await _notificationService.GetUserNotifications(userId.Value, unreadOnly);

            // 按类型筛选
            if (!string.IsNullOrEmpty(type))
            {
                notifications = notifications.Where(n => n.Type == type).ToList();
            }

            // 按已读状态筛选
            if (isRead.HasValue)
            {
                notifications = notifications.Where(n => n.IsRead == isRead.Value).ToList();
            }

            // 分页处理
            var total = notifications.Count;
            var items = notifications
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return Ok(new { items, total });
        }

        /// <summary>
        /// 获取未读通知数量
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "未授权" });

            var count = await _notificationService.GetUnreadCount(userId.Value);
            return Ok(new { count });
        }

        /// <summary>
        /// 标记单个通知为已读
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "未授权" });

            await _notificationService.MarkAsRead(id, userId.Value);
            return Ok(new { message = "已标记为已读" });
        }

        /// <summary>
        /// 标记所有通知为已读
        /// </summary>
        [HttpPut("all/read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "未授权" });

            await _notificationService.MarkAllAsRead(userId.Value);
            return Ok(new { message = "所有通知已标记为已读" });
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "未授权" });

            await _notificationService.DeleteNotification(id, userId.Value);
            return Ok(new { message = "通知已删除" });
        }

        /// <summary>
        /// 发送系统公告（管理员）
        /// </summary>
        [HttpPost("announcement")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendAnnouncement([FromBody] AnnouncementRequest request)
        {
            await _notificationService.SendSystemAnnouncement(request.Title, request.Content, request.TargetUserIds);
            return Ok(new { message = "系统公告已发送" });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return null;
            return userId;
        }
    }

    public class AnnouncementRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<int>? TargetUserIds { get; set; }
    }
}
