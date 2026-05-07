using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AnnouncementController> _logger;

        public AnnouncementController(AppDbContext dbContext, ILogger<AnnouncementController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取公告列表（公开接口）
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAnnouncements([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var query = _dbContext.Announcements
                .Include(a => a.Publisher)
                .Where(a => a.IsEnabled)
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Content,
                    a.IsPinned,
                    a.IsEnabled,
                    a.CreatedAt,
                    Publisher = new
                    {
                        a.Publisher.Id,
                        a.Publisher.Name,
                        a.Publisher.Username,
                        a.Publisher.Role
                    }
                })
                .ToListAsync();

            return Ok(new { items, total });
        }

        /// <summary>
        /// 获取单条公告详情（公开接口）
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAnnouncement(int id)
        {
            var announcement = await _dbContext.Announcements
                .Include(a => a.Publisher)
                .Where(a => a.Id == id && a.IsEnabled)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Content,
                    a.IsPinned,
                    a.IsEnabled,
                    a.CreatedAt,
                    a.UpdatedAt,
                    Publisher = new
                    {
                        a.Publisher.Id,
                        a.Publisher.Name,
                        a.Publisher.Username,
                        a.Publisher.Role
                    }
                })
                .FirstOrDefaultAsync();

            if (announcement == null)
                return NotFound(new { message = "公告不存在或已下架" });

            return Ok(announcement);
        }

        /// <summary>
        /// 创建公告（管理员）
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateAnnouncement([FromBody] AnnouncementCreateRequest request)
        {
            try
            {
                _logger.LogInformation("开始创建公告，标题: {Title}", request.Title);
                
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("创建公告失败：未授权");
                    return Unauthorized(new { message = "未授权" });
                }

                _logger.LogInformation("当前用户ID: {UserId}", userId);

                var announcement = new Announcement
                {
                    Title = request.Title,
                    Content = request.Content,
                    IsPinned = request.IsPinned ?? false,
                    IsEnabled = true,
                    PublisherId = userId.Value,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbContext.Announcements.Add(announcement);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("公告创建成功，ID: {Id}", announcement.Id);

                // 自动发送通知给所有非管理员用户（教师和学生）
                await SendAnnouncementNotifications(announcement);

                return Ok(new { message = "公告发布成功，已通知所有用户", id = announcement.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建公告失败，标题: {Title}", request.Title);
                return StatusCode(500, new { message = "创建公告失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 更新公告（管理员）
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateAnnouncement(int id, [FromBody] AnnouncementUpdateRequest request)
        {
            var announcement = await _dbContext.Announcements.FindAsync(id);
            if (announcement == null)
                return NotFound(new { message = "公告不存在" });

            if (request.Title != null)
                announcement.Title = request.Title;

            if (request.Content != null)
                announcement.Content = request.Content;

            if (request.IsPinned.HasValue)
                announcement.IsPinned = request.IsPinned.Value;

            if (request.IsEnabled.HasValue)
                announcement.IsEnabled = request.IsEnabled.Value;

            announcement.UpdatedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "公告更新成功" });
        }

        /// <summary>
        /// 删除公告（管理员）
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await _dbContext.Announcements.FindAsync(id);
            if (announcement == null)
                return NotFound(new { message = "公告不存在" });

            _dbContext.Announcements.Remove(announcement);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "公告已删除" });
        }

        /// <summary>
        /// 发送公告通知给所有非管理员用户
        /// </summary>
        private async Task SendAnnouncementNotifications(Announcement announcement)
        {
            try
            {
                // 获取所有非管理员用户（教师和学生）
                var nonAdminUsers = await _dbContext.Users
                    .Where(u => u.Role != "Admin")
                    .Select(u => u.Id)
                    .ToListAsync();

                _logger.LogInformation("找到 {Count} 个非管理员用户需要发送通知", nonAdminUsers.Count);

                // 为每个用户创建通知
                var notifications = nonAdminUsers.Select(userId => new Notification
                {
                    UserId = userId,
                    Type = "system",
                    Title = $"新公告：{announcement.Title}",
                    Content = announcement.Content.Length > 200 
                        ? announcement.Content.Substring(0, 200) + "..." 
                        : announcement.Content,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                }).ToList();

                _dbContext.Notifications.AddRange(notifications);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("已向 {Count} 个用户发送公告通知", nonAdminUsers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送公告通知失败");
                // 通知发送失败不影响公告发布
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return null;
            return userId;
        }
    }

    public class AnnouncementCreateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool? IsPinned { get; set; }
    }

    public class AnnouncementUpdateRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsPinned { get; set; }
        public bool? IsEnabled { get; set; }
    }
}