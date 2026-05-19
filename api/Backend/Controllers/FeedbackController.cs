using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 从JWT token中获取当前用户ID
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }

        /// <summary>
        /// 检查当前用户是否为管理员
        /// </summary>
        private bool IsAdmin()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return role == "Admin";
        }

        // GET: api/Feedback
        /// <summary>
        /// 获取反馈列表（管理员可查看所有，普通用户只能查看自己的）
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetFeedbacks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? type = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                IQueryable<Feedback> query = _context.Feedbacks
                    .Include(f => f.User)
                    .OrderByDescending(f => f.CreatedAt);

                // 管理员可以查看所有反馈，普通用户只能查看自己的
                if (!IsAdmin())
                {
                    query = query.Where(f => f.UserId == userId);
                }

                // 状态筛选
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(f => f.Status == status);
                }

                // 类型筛选
                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(f => f.Type == type);
                }

                var total = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new
                    {
                        f.Id,
                        f.Title,
                        f.Type,
                        f.Content,
                        f.Status,
                        f.Reply,
                        f.Contact,
                        f.CreatedAt,
                        f.RepliedAt,
                        UserId = f.UserId,
                        UserName = f.User.Name,
                        UserUsername = f.User.Username
                    })
                    .ToListAsync();

                return Ok(new { items, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取反馈列表失败", error = ex.Message });
            }
        }

        // GET: api/Feedback/5
        /// <summary>
        /// 获取单个反馈详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetFeedback(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var feedback = await _context.Feedbacks
                    .Include(f => f.User)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (feedback == null)
                {
                    return NotFound(new { message = "反馈不存在" });
                }

                // 验证权限：只能查看自己的反馈或管理员可查看所有
                if (!IsAdmin() && feedback.UserId != userId)
                {
                    return Forbid();
                }

                return Ok(new
                {
                    feedback.Id,
                    feedback.Title,
                    feedback.Type,
                    feedback.Content,
                    feedback.Status,
                    feedback.Reply,
                    feedback.Contact,
                    feedback.CreatedAt,
                    feedback.RepliedAt,
                    UserId = feedback.UserId,
                    UserName = feedback.User.Name,
                    UserUsername = feedback.User.Username
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取反馈详情失败", error = ex.Message });
            }
        }

        // POST: api/Feedback
        /// <summary>
        /// 提交反馈
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> PostFeedback([FromBody] FeedbackCreateRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var feedback = new Feedback
                {
                    UserId = userId.Value,
                    Type = request.Type ?? "suggestion",
                    Title = request.Title,
                    Content = request.Content,
                    Contact = request.Contact,
                    Status = "pending",
                    CreatedAt = DateTime.Now
                };

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                // 通知管理员有新反馈
                await NotifyAdminForFeedback(feedback);

                return Ok(new
                {
                    message = "反馈提交成功",
                    data = new
                    {
                        feedback.Id,
                        feedback.Title,
                        feedback.Type,
                        feedback.Content,
                        feedback.Status,
                        feedback.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "提交反馈失败", error = ex.Message });
            }
        }

        // PUT: api/Feedback/5/reply
        /// <summary>
        /// 管理员回复反馈
        /// </summary>
        [HttpPut("{id}/reply")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ReplyFeedback(int id, [FromBody] FeedbackReplyRequest request)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null)
                {
                    return NotFound(new { message = "反馈不存在" });
                }

                feedback.Reply = request.Reply;
                feedback.Status = "resolved";
                feedback.RepliedAt = DateTime.Now;

                _context.Entry(feedback).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // 给提交反馈的用户发送通知
                var notification = new Notification
                {
                    UserId = feedback.UserId,
                    Type = "feedback",
                    Title = "反馈已回复",
                    Content = $"您的反馈「{feedback.Title}」已收到回复：{request.Reply}",
                    WorkId = feedback.Id,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return Ok(new { message = "回复成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "回复失败", error = ex.Message });
            }
        }

        // PUT: api/Feedback/5/status
        /// <summary>
        /// 更新反馈状态（管理员）
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateFeedbackStatus(int id, [FromBody] FeedbackStatusRequest request)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null)
                {
                    return NotFound(new { message = "反馈不存在" });
                }

                feedback.Status = request.Status;

                _context.Entry(feedback).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "状态更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新状态失败", error = ex.Message });
            }
        }

        // DELETE: api/Feedback/5
        /// <summary>
        /// 删除反馈（管理员）
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteFeedback(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null)
                {
                    return NotFound(new { message = "反馈不存在" });
                }

                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取反馈类型中文标签
        /// </summary>
        private string GetFeedbackTypeLabel(string type)
        {
            switch (type)
            {
                case "suggestion":
                    return "功能建议";
                case "complaint":
                    return "问题投诉";
                case "question":
                    return "问题咨询";
                case "other":
                    return "其他";
                default:
                    return type;
            }
        }

        /// <summary>
        /// 通知管理员有新反馈
        /// </summary>
        private async Task NotifyAdminForFeedback(Feedback feedback)
        {
            try
            {
                var adminUsers = await _context.Users
                    .Where(u => u.Role == "Admin")
                    .Select(u => u.Id)
                    .ToListAsync();

                // 截取内容预览，最多显示200个字符
                var contentPreview = feedback.Content.Length > 200 
                    ? feedback.Content.Substring(0, 200) + "..." 
                    : feedback.Content;

                // 添加联系方式信息
                var contactInfo = string.IsNullOrEmpty(feedback.Contact) 
                    ? "（未提供联系方式）" 
                    : $"，联系方式：{feedback.Contact}";

                var notifications = adminUsers.Select(adminId => new Notification
                {
                    UserId = adminId,
                    Type = "feedback",
                    Title = $"新反馈：{feedback.Title}",
                    Content = $"用户提交了新的反馈，类型：{GetFeedbackTypeLabel(feedback.Type)}{contactInfo}。内容预览：{contentPreview}",
                    WorkId = feedback.Id, // 存储反馈ID用于跳转
                    CreatedAt = DateTime.Now,
                    IsRead = false
                }).ToList();

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送反馈通知失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 创建反馈请求
    /// </summary>
    public class FeedbackCreateRequest
    {
        public string? Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Contact { get; set; }
    }

    /// <summary>
    /// 回复反馈请求
    /// </summary>
    public class FeedbackReplyRequest
    {
        public string Reply { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新状态请求
    /// </summary>
    public class FeedbackStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
