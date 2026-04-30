using Backend.Data;
using Backend.Models;
using Backend.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
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
        /// 获取当前用户的所有收藏
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetCollections(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string sortBy = "collectionDate",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var query = _context.UserCollections
                    .Include(c => c.Work)
                    .Where(c => c.UserId == userId);

                // 搜索筛选
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c =>
                        c.Work != null &&
                        c.Work.Title.Contains(search));
                }

                // 分类筛选
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(c => c.Work != null && c.Work.Category == category);
                }

                // 排序
                if (sortBy.ToLower() == "uploaddate")
                {
                    query = query.OrderByDescending(c => c.Work != null && c.Work.FileUploadTime.HasValue ? c.Work.FileUploadTime.Value : DateTime.MinValue);
                }
                else
                {
                    query = query.OrderByDescending(c => c.CollectionDate);
                }

                // 分页
                var total = await query.CountAsync();
                var collections = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new { items = collections, total = total });
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
        public async Task<ActionResult<UserCollection>> AddCollection([FromBody] AddCollectionRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                int workId = request.workId;

                // 检查是否已经收藏
                var existing = await _context.UserCollections
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.WorkId == workId);

                if (existing != null)
                {
                    return BadRequest(new { message = "该作品已在收藏列表中" });
                }

                // 检查作品是否存在
                var work = await _context.Works.FindAsync(workId);
                if (work == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                var collection = new UserCollection
                {
                    WorkId = workId,
                    UserId = userId.Value,
                    CollectionDate = DateTime.Now
                };

                _context.UserCollections.Add(collection);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCollections), new { id = collection.Id }, collection);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "添加收藏失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 添加收藏请求模型
        /// </summary>
        public class AddCollectionRequest
        {
            public int workId { get; set; }
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
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var collection = await _context.UserCollections
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.WorkId == workId);

                if (collection == null)
                {
                    return NotFound(new { message = "收藏不存在" });
                }

                _context.UserCollections.Remove(collection);
                await _context.SaveChangesAsync();

                return Ok(new { message = "删除收藏成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除收藏失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 检查作品是否已收藏
        /// </summary>
        [HttpGet("check/{workId}")]
        public async Task<ActionResult<bool>> CheckFavorite(int workId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var exists = await _context.UserCollections
                    .AnyAsync(c => c.UserId == userId && c.WorkId == workId);

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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return null;
            }
            return userId;
        }
    }
}