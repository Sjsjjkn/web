using Backend.Data;
using Backend.Models;
using Backend.Models.DTO;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public WorkController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

        // GET: api/Work
        // 首页和作品展厅使用，显示所有已发布作品，不限制权限
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetWorks(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] int? user_id = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                // 首页和作品展厅显示所有已发布作品，不限制权限
                IQueryable<Work> query = _context.Works.Where(w => w.Status == "已发布");

                // 按用户筛选（用于公开主页展示该用户的所有作品）
                if (user_id.HasValue)
                {
                    query = query.Where(w => w.UserId == user_id.Value);
                }

                // 搜索筛选
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(w => w.Title.Contains(search));
                }

                // 分类筛选
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(w => w.Category == category);
                }

                // 状态筛选
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(w => w.Status == status);
                }

                // 排序处理
                switch (sortBy)
                {
                    case "latest":
                        query = query.OrderByDescending(w => w.FileUploadTime);
                        break;
                    case "views":
                        query = query.OrderByDescending(w => w.Views);
                        break;
                    case "favorites":
                        query = query.OrderByDescending(w => w.Favorites);
                        break;
                    default:
                        query = query.OrderByDescending(w => w.FileUploadTime);
                        break;
                }

                // 计算总数
                var total = await query.CountAsync();

                // 分页并关联用户表
                var worksWithUserInfo = await query
                    .Join(
                        _context.Users,
                        w => w.UserId,
                        u => u.Id,
                        (w, u) => new {
                            w.Id,
                            w.Title,
                            uploadUserName = u.Name,
                            w.Category,
                            w.Description,
                            w.UploadDate,
                            w.Status,
                            w.FilePath,
                            w.FileName,
                            w.FileSize,
                            w.FileMD5,
                            w.FileUploadTime,
                            w.PreviewImage,
                            w.UserId,
                            w.Views,
                            w.Favorites,
                            w.IsExcellent,
                            uploadUserUsername = u.Username,
                            uploadUserRole = u.Role,
                            uploadUserProfilePublic = u.ProfilePublic,
                            uploadUserAvatar = u.Avatar
                        }
                    )
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new {
                    items = worksWithUserInfo,
                    total = total,
                    isAdmin = false
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取作品列表失败: {ex.Message}");
                return StatusCode(500, new { message = "获取作品列表失败" });
            }
        }

        // GET: api/Work/my
        // 用户中心使用，查看自己的作品（带权限验证）
        [HttpGet("my")]
        public async Task<ActionResult> GetMyWorks(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
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

                var isAdmin = User.IsInRole("Admin");
                var isTeacher = User.IsInRole("Teacher");
                IQueryable<Work> query;

                if (isAdmin)
                {
                    // 管理员可以查看所有作品
                    query = _context.Works;
                }
                else if (isTeacher)
                {
                    // 教师可以查看：自己的作品 + 自己管理的学生的所有作品（包括草稿、待审核、已发布）
                    var supervisedStudentIds = _context.StudentTeachers
                        .Where(st => st.TeacherId == userId.Value)
                        .Select(st => st.StudentId)
                        .ToList();

                    query = _context.Works.Where(w =>
                        w.UserId == userId.Value ||
                        supervisedStudentIds.Contains(w.UserId)
                    );
                }
                else
                {
                    // 学生只能查看自己的作品（包括草稿、待审核、已发布）
                    query = _context.Works.Where(w =>
                        w.UserId == userId.Value
                    );
                }

                // 搜索筛选
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(w => w.Title.Contains(search));
                }

                // 分类筛选
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(w => w.Category == category);
                }

                // 状态筛选
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(w => w.Status == status);
                }

                // 计算总数
                var total = await query.CountAsync();

                // 分页并关联用户表
                var worksWithUserInfo = await query
                    .Join(
                        _context.Users,
                        w => w.UserId,
                        u => u.Id,
                        (w, u) => new {
                            w.Id,
                            w.Title,
                            uploadUserName = u.Name,
                            w.Category,
                            w.Description,
                            w.UploadDate,
                            w.Status,
                            w.FilePath,
                            w.FileName,
                            w.FileSize,
                            w.FileMD5,
                            w.FileUploadTime,
                            w.PreviewImage,
                            w.UserId,
                            w.Views,
                            w.Favorites,
                            w.IsExcellent,
                            uploadUserUsername = u.Username,
                            uploadUserRole = u.Role,
                            uploadUserProfilePublic = u.ProfilePublic,
                            uploadUserAvatar = u.Avatar
                        }
                    )
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new {
                    items = worksWithUserInfo,
                    total = total,
                    isAdmin = isAdmin
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取作品列表失败: {ex.Message}");
                return StatusCode(500, new { message = "获取作品列表失败" });
            }
        }

        // GET: api/Work/public
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPublicWorks(
            [FromQuery] int limit = 10)
        {
            try
            {
                // 只返回已发布的作品
                var query = _context.Works.Where(w => w.Status == "已发布");

                // 关联用户表
                var worksWithUserInfo = await query
                    .Join(
                        _context.Users,
                        w => w.UserId,
                        u => u.Id,
                        (w, u) => new {
                            w.Id,
                            w.Title,
                            uploadUserName = u.Name,
                            w.Category,
                            w.Description,
                            w.UploadDate,
                            w.Status,
                            w.FilePath,
                            w.FileName,
                            w.FileSize,
                            w.FileMD5,
                            w.FileUploadTime,
                            w.PreviewImage,
                            w.UserId,
                            w.Views,
                            w.Favorites,
                            w.IsExcellent,
                            uploadUserUsername = u.Username,
                            uploadUserRole = u.Role,
                            uploadUserProfilePublic = u.ProfilePublic,
                            uploadUserAvatar = u.Avatar
                        }
                    )
                    .OrderByDescending(w => w.FileUploadTime)
                    .Take(limit)
                    .ToListAsync();

                return Ok(new {
                    items = worksWithUserInfo,
                    total = worksWithUserInfo.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取公开作品列表失败: {ex.Message}");
                return StatusCode(500, new { message = "获取作品列表失败" });
            }
        }

        // GET: api/Work/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Work>> GetWork(int id)
        {
            var currentUserId = GetCurrentUserId();

            var workWithUserInfo = await _context.Works
                .Where(w => w.Id == id)
                .Join(
                    _context.Users,
                    w => w.UserId,
                    u => u.Id,
                    (w, u) => new {
                        w.Id,
                        w.Title,
                        uploadUserName = u.Name,
                        w.Category,
                        w.Description,
                        w.UploadDate,
                        w.Status,
                        w.FilePath,
                        w.FileName,
                        w.FileSize,
                        w.FileMD5,
                        w.FileUploadTime,
                        w.PreviewImage,
                        w.UserId,
                        w.Views,
                        w.Favorites,
                        w.IsExcellent,
                        uploadUserUsername = u.Username,
                        uploadUserRole = u.Role,
                        uploadUserProfilePublic = u.ProfilePublic,
                        uploadUserAvatar = u.Avatar
                    }
                )
                .FirstOrDefaultAsync();

            if (workWithUserInfo == null)
            {
                return NotFound(new { message = "作品不存在" });
            }

            // 检查访问权限
            bool isOwner = currentUserId.HasValue && workWithUserInfo.UserId == currentUserId.Value;
            bool isPublic = workWithUserInfo.Status == "已发布";
            
            // 检查是否为管理员
            bool isAdmin = User.IsInRole("Admin");
            
            // 检查是否为该作品作者的指导教师
            bool isSupervisingTeacher = false;
            if (currentUserId.HasValue && User.IsInRole("Teacher"))
            {
                isSupervisingTeacher = _context.StudentTeachers
                    .Any(st => st.TeacherId == currentUserId.Value && st.StudentId == workWithUserInfo.UserId);
            }
            
            // 检查作品作者是否为管理员（非管理员用户不能查看管理员的作品）
            bool isWorkAuthorAdmin = false;
            if (workWithUserInfo.UserId > 0)
            {
                isWorkAuthorAdmin = _context.Users.Any(u => u.Id == workWithUserInfo.UserId && u.Role == "Admin");
            }
            
            // 如果作品作者是管理员，非管理员用户无权访问
            if (isWorkAuthorAdmin && !isAdmin)
            {
                return Forbid("无权访问该作品");
            }

            if (!isOwner && !isPublic && !isAdmin && !isSupervisingTeacher)
            {
                return Forbid("无权访问该作品");
            }

            return Ok(workWithUserInfo);
        }

        // PUT: api/Work/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWork(int id, [FromBody] WorkUpdateRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                // 检查作品是否存在
                var existingWork = await _context.Works.FindAsync(id);
                if (existingWork == null)
                {
                    return NotFound();
                }

                // 检查作品是否属于当前用户
                if (existingWork.UserId != userId)
                {
                    return Forbid();
                }

                // 更新字段
                existingWork.Title = request.Title;
                existingWork.Category = request.Category;
                existingWork.Description = request.Description;
                existingWork.UploadDate = request.UploadDate ?? DateTime.Now;
                existingWork.Status = request.Status;
                existingWork.FilePath = request.FilePath;
                existingWork.FileName = request.FileName;
                existingWork.FileSize = request.FileSize;
                existingWork.FileMD5 = request.FileMD5;
                existingWork.FileUploadTime = request.FileUploadTime ?? DateTime.Now;
                existingWork.PreviewImage = request.PreviewImage;
                // 保持原始的UserId
                // existingWork.UserId = work.UserId;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // 如果作品状态变为待审核，通知所有管理员
                if (existingWork.Status == "待审核" && existingWork.Uploader == null)
                {
                    existingWork.Uploader = await _context.Users.FindAsync(existingWork.UserId);
                }
                if (existingWork.Status == "待审核")
                {
                    await NotifyAdminForReview(existingWork);
                }

                return Ok(new { message = "更新成功", work = existingWork });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新作品失败: {ex.Message}");
                return StatusCode(500, new { message = "更新作品失败", error = ex.Message });
            }
        }

        // POST: api/Work
        [HttpPost]
        public async Task<ActionResult<Work>> PostWork([FromBody] WorkCreateRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "未授权" });
            }

            // 获取用户实体用于导航属性
            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return Unauthorized(new { message = "用户不存在" });
            }

            // 确保 FilePath 和 FileName 有值，互相补充
            string filePath = request.FilePath ?? request.FileName ?? string.Empty;
            string fileName = request.FileName ?? request.FilePath ?? string.Empty;

            var work = new Work
            {
                Title = request.Title,
                Category = request.Category,
                Description = request.Description,
                Status = request.Status,
                FilePath = filePath,
                FileName = fileName,
                FileSize = request.FileSize,
                FileMD5 = request.FileMD5,
                PreviewImage = request.PreviewImage,
                UploadDate = request.UploadDate ?? DateTime.Now,
                FileUploadTime = DateTime.Now,
                UserId = userId.Value,
                Uploader = user
            };

            _context.Works.Add(work);
            await _context.SaveChangesAsync();

            // 如果作品状态是待审核，通知所有管理员
            if (work.Status == "待审核")
            {
                await NotifyAdminForReview(work);
            }

            return Ok(new 
            {
                message = "作品提交成功",
                data = new 
                {
                    Id = work.Id,
                    Title = work.Title,
                    Category = work.Category,
                    Description = work.Description,
                    Status = work.Status,
                    FilePath = work.FilePath,
                    FileName = work.FileName,
                    FileSize = work.FileSize,
                    FileMD5 = work.FileMD5,
                    PreviewImage = work.PreviewImage,
                    UploadDate = work.UploadDate,
                    FileUploadTime = work.FileUploadTime,
                    UserId = work.UserId
                }
            });
        }

        // DELETE: api/Work/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWork(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "未授权" });
            }

            var work = await _context.Works.FindAsync(id);
            if (work == null)
            {
                return NotFound();
            }

            // 检查作品是否属于当前用户
        if (work.UserId != userId)
        {
            return Forbid();
        }

            _context.Works.Remove(work);
            await _context.SaveChangesAsync();

            return Ok(new { message = "删除成功" });
        }

        private bool WorkExists(int id)
        {
            return _context.Works.Any(e => e.Id == id);
        }

        /// <summary>
        /// 通知管理员有作品待审核
        /// </summary>
        private async Task NotifyAdminForReview(Work work)
        {
            try
            {
                // 获取所有管理员用户
                var adminUsers = await _context.Users
                    .Where(u => u.Role == "Admin")
                    .Select(u => u.Id)
                    .ToListAsync();

                // 为每个管理员创建通知
                var notifications = adminUsers.Select(adminId => new Notification
                {
                    UserId = adminId,
                    Type = "work_approval",
                    Title = $"新作品待审核：{work.Title}",
                    Content = $"用户 {work.Uploader?.Name} 提交了作品《{work.Title}》，请尽快审核。",
                    WorkId = work.Id,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                }).ToList();

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // 通知发送失败不影响作品提交
                Console.WriteLine($"发送审核通知失败: {ex.Message}");
            }
        }

        // POST: api/Work/{id}/review
        [HttpPost("{id}/review")]
        public async Task<IActionResult> ReviewWork(int id, [FromBody] ReviewRequest request)
        {
            try
            {
                // 检查当前用户是否为管理员
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                // 更新作品状态
                work.Status = request.Status;
                
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = $"作品审核成功，状态已更新为：{request.Status}",
                    work = work
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"审核作品失败: {ex.Message}");
                return StatusCode(500, new { message = "审核作品失败", error = ex.Message });
            }
        }

        // GET: api/Work/recent
        [HttpGet("recent")]
        public async Task<ActionResult> GetRecentWorks([FromQuery] int hours = 6)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var isAdmin = User.IsInRole("Admin");
            var isTeacher = User.IsInRole("Teacher");
            IQueryable<Work> query;
            
            if (isAdmin)
            {
                // 管理员可以查看所有作品
                query = _context.Works;
            }
            else if (isTeacher)
            {
                // 教师可以查看：自己的作品 + 自己管理的学生的所有作品 + 其他普通用户的已发布作品
                // 获取教师管理的学生ID列表
                var supervisedStudentIds = _context.StudentTeachers
                    .Where(st => st.TeacherId == userId.Value)
                    .Select(st => st.StudentId)
                    .ToList();
                
                // 获取所有非管理员用户ID（排除管理员）
                var nonAdminUserIds = _context.Users
                    .Where(u => u.Role != "Admin")
                    .Select(u => u.Id)
                    .ToList();
                
                query = _context.Works.Where(w => 
                    w.UserId == userId.Value || // 自己的作品
                    supervisedStudentIds.Contains(w.UserId) || // 自己管理的学生的作品
                    (w.Status == "已发布" && nonAdminUserIds.Contains(w.UserId)) // 其他非管理员用户的已发布作品
                );
            }
            else
            {
                // 学生可以查看：自己的所有作品（无论状态）和其他非管理员用户的已发布作品
                // 获取所有非管理员用户ID（排除管理员）
                var nonAdminUserIds = _context.Users
                    .Where(u => u.Role != "Admin")
                    .Select(u => u.Id)
                    .ToList();
                
                query = _context.Works.Where(w => 
                    w.UserId == userId.Value || // 自己的作品
                    (w.Status == "已发布" && nonAdminUserIds.Contains(w.UserId)) // 其他非管理员用户的已发布作品
                );
            }

                // 筛选最近hours小时内上传的作品
                var cutoffTime = DateTime.Now.AddHours(-hours);
                
                // 先获取所有作品，查看FileUploadTime字段
                var allWorks = await query.ToListAsync();
                
                // 筛选最近hours小时内上传的作品
                query = query.Where(w => w.FileUploadTime.HasValue && w.FileUploadTime >= cutoffTime);

                // 按上传时间倒序排序
                var works = await query
                    .OrderByDescending(w => w.FileUploadTime)
                    .ToListAsync();
                
                return Ok(new {
                    items = works,
                    count = works.Count,
                    hours = hours,
                    cutoffTime = cutoffTime
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取最近作品失败" });
            }
        }

        // GET: api/Work/{id}/view
        [HttpGet("{id}/view")]
        [AllowAnonymous]
        public async Task<ActionResult> IncrementView(int id)
        {
            try
            {
                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                // 增加浏览量
                work.Views++;
                
                // 如果用户已登录，记录浏览历史
                var userId = GetCurrentUserId();
                if (userId != null)
                {
                    // 检查是否已经浏览过这个作品（如果浏览过，只更新浏览时间）
                    var existingHistory = await _context.ViewHistories
                        .FirstOrDefaultAsync(vh => vh.UserId == userId.Value && vh.WorkId == id);
                    
                    if (existingHistory != null)
                    {
                        // 更新浏览时间
                        existingHistory.ViewedAt = DateTime.Now;
                    }
                    else
                    {
                        // 添加新的浏览记录
                        var viewHistory = new ViewHistory
                        {
                            UserId = userId.Value,
                            WorkId = id,
                            ViewedAt = DateTime.Now
                        };
                        _context.ViewHistories.Add(viewHistory);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new {
                    message = "浏览量增加成功",
                    views = work.Views
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"增加浏览量失败: {ex.Message}");
                return StatusCode(500, new { message = "增加浏览量失败" });
            }
        }

        // POST: api/Work/{id}/favorite
        [HttpPost("{id}/favorite")]
        public async Task<ActionResult> AddFavorite(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                // 检查是否已经收藏
                var existingFavorite = await _context.WorkFavorites
                    .Where(f => f.UserId == userId.Value && f.WorkId == id)
                    .FirstOrDefaultAsync();

                if (existingFavorite != null)
                {
                    return BadRequest(new { message = "已经收藏过该作品" });
                }

                // 添加收藏记录
                var favorite = new WorkFavorite
                {
                    UserId = userId.Value,
                    WorkId = id,
                    FavoriteDate = DateTime.Now
                };

                _context.WorkFavorites.Add(favorite);
                
                // 增加收藏量
                work.Favorites++;
                
                await _context.SaveChangesAsync();

                return Ok(new {
                    message = "收藏成功",
                    favorites = work.Favorites
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加收藏失败: {ex.Message}");
                return StatusCode(500, new { message = "添加收藏失败" });
            }
        }

        // DELETE: api/Work/{id}/favorite
        [HttpDelete("{id}/favorite")]
        public async Task<ActionResult> RemoveFavorite(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                // 查找收藏记录
                var favorite = await _context.WorkFavorites
                    .Where(f => f.UserId == userId.Value && f.WorkId == id)
                    .FirstOrDefaultAsync();

                if (favorite == null)
                {
                    return BadRequest(new { message = "未收藏该作品" });
                }

                // 删除收藏记录
                _context.WorkFavorites.Remove(favorite);
                
                // 减少收藏量
                if (work.Favorites > 0)
                {
                    work.Favorites--;
                }
                
                await _context.SaveChangesAsync();

                return Ok(new {
                    message = "取消收藏成功",
                    favorites = work.Favorites
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取消收藏失败: {ex.Message}");
                return StatusCode(500, new { message = "取消收藏失败" });
            }
        }

        // POST: api/Work/{id}/resubmit
        /// <summary>
        /// 学生重新提交作品（附修改说明，通知教师）
        /// </summary>
        [HttpPost("{id}/resubmit")]
        public async Task<IActionResult> ResubmitWork(int id, [FromBody] ResubmitRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var work = await _context.Works.FindAsync(id);
                if (work == null)
                {
                    return NotFound(new { message = "作品不存在" });
                }

                if (work.UserId != userId.Value)
                {
                    return Forbid();
                }

                // 更新作品状态为待审核
                work.Status = "待审核";
                work.FileUploadTime = DateTime.Now;

                // 保存学生的修改说明
                if (!string.IsNullOrWhiteSpace(request.Comment))
                {
                    var review = new WorkReview
                    {
                        WorkId = work.Id,
                        ReviewerId = userId.Value,
                        Comment = request.Comment,
                        Type = "resubmit",
                        CreatedAt = DateTime.Now
                    };
                    _context.WorkReviews.Add(review);
                }

                // 通知所有管理员和该学生的指导教师
                var user = await _context.Users.FindAsync(userId.Value);
                var studentName = user?.Name ?? user?.Username ?? "学生";

                // 通知管理员
                var adminUsers = await _context.Users
                    .Where(u => u.Role == "Admin")
                    .ToListAsync();

                foreach (var admin in adminUsers)
                {
                    await _notificationService.SendWorkResubmitNotification(
                        admin.Id, work.Id, work.Title, studentName);
                }

                // 通知指导教师
                var teacherIds = await _context.StudentTeachers
                    .Where(st => st.StudentId == userId.Value)
                    .Select(st => st.TeacherId)
                    .ToListAsync();

                foreach (var teacherId in teacherIds)
                {
                    await _notificationService.SendWorkResubmitNotification(
                        teacherId, work.Id, work.Title, studentName);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "作品已重新提交审核",
                    work = new
                    {
                        id = work.Id,
                        status = work.Status
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重新提交作品失败: {ex.Message}");
                return StatusCode(500, new { message = "重新提交作品失败", error = ex.Message });
            }
        }

        // GET: api/Work/{id}/is-favorite
        [HttpGet("{id}/is-favorite")]
        public async Task<ActionResult> CheckIsFavorite(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "未授权" });
                }

                var isFavorite = await _context.WorkFavorites
                    .AnyAsync(f => f.UserId == userId.Value && f.WorkId == id);

                return Ok(new {
                    isFavorite = isFavorite
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查收藏状态失败: {ex.Message}");
                return StatusCode(500, new { message = "检查收藏状态失败" });
            }
        }

        // GET: api/Work/favorites
        [HttpGet("favorites")]
        public async Task<ActionResult> GetMyFavorites()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "未授权" });

                var favorites = await _context.WorkFavorites
                    .Where(f => f.UserId == userId.Value)
                    .Include(f => f.Work)
                        .ThenInclude(w => w.Uploader)
                    .OrderByDescending(f => f.FavoriteDate)
                    .Select(fw => new
                    {
                        id = fw.Id,
                        workId = fw.Work.Id,
                        title = fw.Work.Title,
                        description = fw.Work.Description,
                        fileType = Path.GetExtension(fw.Work.FileName ?? "").TrimStart('.').ToLower(),
                        previewImage = fw.Work.PreviewImage,
                        fileName = fw.Work.FileName,
                        filePath = fw.Work.FilePath,
                        authorName = fw.Work.Uploader.Name ?? fw.Work.Uploader.Username,
                        views = (int?)fw.Work.Views ?? 0,
                        favorites = (int?)fw.Work.Favorites ?? 0,
                        favoritedAt = fw.FavoriteDate
                    })
                    .ToListAsync();

                return Ok(favorites);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取收藏列表失败: {ex.Message}");
                return StatusCode(500, new { message = "获取收藏列表失败" });
            }
        }

        // GET: api/Work/history
        [HttpGet("history")]
        public async Task<ActionResult> GetViewHistory()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "未授权" });

                // 获取用户的浏览历史，按浏览时间倒序排列
                var histories = await _context.ViewHistories
                    .Where(vh => vh.UserId == userId.Value)
                    .Include(vh => vh.Work)
                        .ThenInclude(w => w.Uploader)
                    .OrderByDescending(vh => vh.ViewedAt)
                    .Take(20)
                    .Select(vh => new
                    {
                        id = vh.Id,
                        workId = vh.WorkId,
                        title = vh.Work.Title,
                        description = vh.Work.Description,
                        fileType = Path.GetExtension(vh.Work.FileName ?? "").TrimStart('.').ToLower(),
                        previewImage = vh.Work.PreviewImage,
                        fileName = vh.Work.FileName,
                        filePath = vh.Work.FilePath,
                        authorName = vh.Work.Uploader != null ? (vh.Work.Uploader.Name ?? vh.Work.Uploader.Username) : "未知作者",
                        authorAvatar = vh.Work.Uploader != null ? vh.Work.Uploader.Avatar : null,
                        views = (int?)vh.Work.Views ?? 0,
                        favorites = (int?)vh.Work.Favorites ?? 0,
                        viewedAt = vh.ViewedAt
                    })
                    .ToListAsync();

                return Ok(histories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取浏览历史失败: {ex.Message}");
                return StatusCode(500, new { message = "获取浏览历史失败" });
            }
        }
    }

    /// <summary>
    /// 审核作品请求模型
    /// </summary>
    public class ReviewRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 重新提交作品请求模型
    /// </summary>
    public class ResubmitRequest
    {
        public string? Comment { get; set; }
    }
}
