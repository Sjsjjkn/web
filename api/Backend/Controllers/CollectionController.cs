using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    /// <summary>
    /// 收藏控制器 - 管理用户对作品的收藏操作
    /// 统一使用 WorkFavorite 表
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CollectionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CollectionController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取当前用户的所有收藏（带作品信息）
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetCollections(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string sortBy = "favoriteDate",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "未授权" });

                var query = _context.WorkFavorites
                    .Include(f => f.Work)
                    .Where(f => f.UserId == userId);

                // 搜索筛选
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(f =>
                        f.Work != null && f.Work.Title.Contains(search));
                }

                // 分类筛选
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(f =>
                        f.Work != null && f.Work.Category == category);
                }

                // 排序
                query = sortBy.ToLower() == "uploaddate"
                    ? query.OrderByDescending(f =>
                        f.Work != null && f.Work.FileUploadTime.HasValue
                            ? f.Work.FileUploadTime.Value
                            : DateTime.MinValue)
                    : query.OrderByDescending(f => f.FavoriteDate);

                // 分页
                var total = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new
                    {
                        id = f.Id,
                        workId = f.WorkId,
                        userId = f.UserId,
                        collectionDate = f.FavoriteDate,
                        work = f.Work == null ? null : new
                        {
                            id = f.Work.Id,
                            title = f.Work.Title,
                            category = f.Work.Category,
                            description = f.Work.Description,
                            status = f.Work.Status,
                            filePath = f.Work.FilePath,
                            fileName = f.Work.FileName,
                            previewImage = f.Work.PreviewImage,
                            userId = f.Work.UserId,
                            views = f.Work.Views,
                            favorites = f.Work.Favorites,
                            uploadUserName = _context.Users
                                .Where(u => u.Id == f.Work.UserId)
                                .Select(u => u.Name)
                                .FirstOrDefault()
                        }
                    })
                    .ToListAsync();

                return Ok(new { items, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取收藏列表失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 添加收藏
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> AddCollection([FromBody] AddCollectionRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "未授权" });

                var workId = request.workId;

                // 检查作品是否存在
                var work = await _context.Works.FindAsync(workId);
                if (work == null)
                    return NotFound(new { message = "作品不存在" });

                // 检查是否已收藏
                var existing = await _context.WorkFavorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.WorkId == workId);
                if (existing != null)
                    return BadRequest(new { message = "该作品已在收藏列表中" });

                var favorite = new WorkFavorite
                {
                    UserId = userId.Value,
                    WorkId = workId,
                    FavoriteDate = DateTime.Now
                };

                _context.WorkFavorites.Add(favorite);

                // 同步更新 Works 表的 Favorites 冗余计数
                work.Favorites++;
                await _context.SaveChangesAsync();

                return Ok(new { message = "收藏成功", id = favorite.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "添加收藏失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 删除收藏
        /// </summary>
        [HttpDelete("{workId}")]
        public async Task<IActionResult> DeleteCollection(int workId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "未授权" });

                var favorite = await _context.WorkFavorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.WorkId == workId);
                if (favorite == null)
                    return NotFound(new { message = "收藏不存在" });

                _context.WorkFavorites.Remove(favorite);

                // 同步更新 Works 表的 Favorites 冗余计数
                var work = await _context.Works.FindAsync(workId);
                if (work != null && work.Favorites > 0)
                    work.Favorites--;

                await _context.SaveChangesAsync();
                return Ok(new { message = "删除收藏成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除收藏失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 检查作品是否已被当前用户收藏
        /// </summary>
        [HttpGet("check/{workId}")]
        public async Task<ActionResult> CheckFavorite(int workId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "未授权" });

                var exists = await _context.WorkFavorites
                    .AnyAsync(f => f.UserId == userId && f.WorkId == workId);

                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "检查收藏状态失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 从JWT token中获取当前用户ID
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return null;
            return userId;
        }
    }

    /// <summary>
    /// 添加收藏请求模型
    /// </summary>
    public class AddCollectionRequest
    {
        public int workId { get; set; }
    }
}
