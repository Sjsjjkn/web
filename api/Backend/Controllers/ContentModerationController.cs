using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Backend.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContentModerationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public ContentModerationController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// 获取审核项目列表
        /// </summary>
        /// <param name="status">状态筛选</param>
        /// <param name="search">搜索关键词</param>
        /// <returns>审核项目列表</returns>
        [HttpGet("items")]
        public async Task<IActionResult> GetModerationItems(string status = "", string search = "")
        {
            try
            {
                var query = _context.Works
                    .Join(
                        _context.Users,
                        work => work.UserId,
                        user => user.Id,
                        (work, user) => new { work, user }
                    )
                    .AsQueryable();

                // 状态筛选
                if (!string.IsNullOrEmpty(status))
                {
                    if (status == "已审核")
                    {
                        query = query.Where(item => item.work.Status == "已发布" || item.work.Status == "已拒绝");
                    }
                    else if (status == "优秀作品")
                    {
                        query = query.Where(item => item.work.IsExcellent);
                    }
                    else
                    {
                        query = query.Where(item => item.work.Status == status);
                    }
                }
                else
                {
                    // 默认只显示待审核的作品
                    query = query.Where(item => item.work.Status == "待审核");
                }

                // 搜索筛选
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(item => 
                        item.work.Title.Contains(search) || 
                        item.user.Name.Contains(search)
                    );
                }

                var items = await query.ToListAsync();

                // 转换为前端需要的格式
                var result = items.Select(item => new {
                    id = item.work.Id,
                    title = item.work.Title,
                    author = item.user.Name,
                    category = item.work.Category,
                    type = GetFileType(item.work.FileName),
                    fileName = item.work.FileName,
                    preview = item.work.FilePath,
                    content = item.work.Description,
                    submitTime = item.work.UploadDate.ToString("yyyy-MM-dd HH:mm"),
                    riskLevel = "low",
                    riskDetails = new string[] {},
                    status = item.work.Status,
                    isExcellent = item.work.IsExcellent,
                    approving = false,
                    rejecting = false,
                    excelling = false
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get moderation items error: {ex.Message}");
                return StatusCode(500, new { message = $"获取审核项目失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 根据文件名获取文件类型
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件类型</returns>
        private string GetFileType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "other";
            
            var extension = Path.GetExtension(fileName).ToLower();
            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp")
                return "image";
            else if (extension == ".mp4" || extension == ".avi" || extension == ".mov" || extension == ".wmv" || extension == ".flv")
                return "video";
            else if (extension == ".pdf" || extension == ".doc" || extension == ".docx" || extension == ".txt" || extension == ".zip" || extension == ".rar")
                return "document";
            else
                return "other";
        }

        /// <summary>
        /// 获取审核统计数据
        /// </summary>
        /// <returns>统计数据</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetModerationStats()
        {
            try
            {
                var items = await _context.Works.ToListAsync();

                var stats = new [] {
                    new { label = "待审核", value = items.Count(item => item.Status == "待审核"), icon = "el-icon-time", color = "#fffbe6" },
                    new { label = "已审核", value = items.Count(item => item.Status == "已发布" || item.Status == "已拒绝"), icon = "el-icon-check", color = "#f6ffed" },
                    new { label = "优秀作品", value = items.Count(item => item.IsExcellent), icon = "el-icon-star-on", color = "#fff7e6" }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get moderation stats error: {ex.Message}");
                return StatusCode(500, new { message = $"获取统计数据失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 审核通过项目
        /// </summary>
        /// <param name="id">项目ID</param>
        /// <returns>操作结果</returns>
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveItem(int id)
        {
            try
            {
                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                work.Status = "已发布";
                await _context.SaveChangesAsync();

                // 发送审核通过通知
                await _notificationService.SendWorkApprovalNotification(
                    work.UserId, 
                    work.Id, 
                    work.Title, 
                    isApproved: true);

                return Ok(new { message = "审核通过" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Approve item error: {ex.Message}");
                return StatusCode(500, new { message = $"操作失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 拒绝审核项目
        /// </summary>
        /// <param name="id">项目ID</param>
        /// <returns>操作结果</returns>
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> RejectItem(int id, [FromBody] RejectRequest request)
        {
            try
            {
                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                work.Status = "已拒绝";
                await _context.SaveChangesAsync();

                // 发送审核拒绝通知
                await _notificationService.SendWorkApprovalNotification(
                    work.UserId,
                    work.Id,
                    work.Title,
                    isApproved: false,
                    reason: request?.Reason);

                return Ok(new { message = "已拒绝" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reject item error: {ex.Message}");
                return StatusCode(500, new { message = $"操作失败: {ex.Message}" });
            }
        }

        public class RejectRequest
        {
            public string? Reason { get; set; }
        }

        /// <summary>
        /// 设置优秀作品
        /// </summary>
        /// <param name="id">项目ID</param>
        /// <returns>操作结果</returns>
        [HttpPut("excellent/{id}")]
        public async Task<IActionResult> SetExcellentWork(int id)
        {
            try
            {
                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                work.IsExcellent = !work.IsExcellent;
                await _context.SaveChangesAsync();

                return Ok(new { message = work.IsExcellent ? "已设置为优秀作品" : "已取消优秀作品" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Set excellent work error: {ex.Message}");
                return StatusCode(500, new { message = $"操作失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取项目详情
        /// </summary>
        /// <param name="id">项目ID</param>
        /// <returns>项目详情</returns>
        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetItemDetails(int id)
        {
            try
            {
                var item = await _context.Works
                    .Join(
                        _context.Users,
                        work => work.UserId,
                        user => user.Id,
                        (work, user) => new { work, user }
                    )
                    .FirstOrDefaultAsync(item => item.work.Id == id);

                if (item == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                var result = new {
                    id = item.work.Id,
                    title = item.work.Title,
                    author = item.user.Name,
                    category = item.work.Category,
                    type = GetFileType(item.work.FileName),
                    fileName = item.work.FileName,
                    preview = item.work.FilePath,
                    content = item.work.Description,
                    submitTime = item.work.UploadDate.ToString("yyyy-MM-dd HH:mm"),
                    riskLevel = "low",
                    riskDetails = new string[] {},
                    status = item.work.Status,
                    isExcellent = item.work.IsExcellent
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get item details error: {ex.Message}");
                return StatusCode(500, new { message = $"获取项目详情失败: {ex.Message}" });
            }
        }
    }
}