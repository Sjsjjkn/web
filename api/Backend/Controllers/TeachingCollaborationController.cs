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
    [Authorize(Roles = "Admin,Teacher")]
    public class TeachingCollaborationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeachingCollaborationController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out var userId))
            {
                return null;
            }

            return userId;
        }

        private async Task<List<int>> GetManagedStudentIdsAsync()
        {
            if (User.IsInRole("Admin"))
            {
                return await _context.Users
                    .Where(u => u.Role == "Student")
                    .Select(u => u.Id)
                    .ToListAsync();
            }

            var teacherId = GetCurrentUserId();
            if (teacherId == null)
            {
                return new List<int>();
            }

            return await _context.StudentTeachers
                .Where(x => x.TeacherId == teacherId.Value)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();
        }

        private IQueryable<Work> BuildManagedWorksQuery(List<int> studentIds)
        {
            var linkedWorkIds = _context.WorkStudents
                .Where(x => studentIds.Contains(x.StudentId))
                .Select(x => x.WorkId);

            return _context.Works
                .Where(w => studentIds.Contains(w.UserId) || linkedWorkIds.Contains(w.Id));
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            try
            {
                var studentIds = await GetManagedStudentIdsAsync();
                var worksQuery = BuildManagedWorksQuery(studentIds);
                var now = DateTime.Now;
                var recentCutoff = now.AddDays(-7);

                var overview = new
                {
                    studentCount = studentIds.Count,
                    workCount = await worksQuery.CountAsync(),
                    publishedCount = await worksQuery.CountAsync(w => w.Status == "已发布"),
                    excellentCount = await worksQuery.CountAsync(w => w.IsExcellent),
                    recentWorkCount = await worksQuery.CountAsync(w => (w.FileUploadTime ?? w.UploadDate) >= recentCutoff),
                    draftCount = await worksQuery.CountAsync(w => w.Status != "已发布")
                };

                return Ok(overview);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取教学协同总览失败: {ex.Message}");
                return StatusCode(500, new { message = "获取教学协同总览失败" });
            }
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents([FromQuery] string? search = null)
        {
            try
            {
                var studentIds = await GetManagedStudentIdsAsync();
                var works = await BuildManagedWorksQuery(studentIds).ToListAsync();

                var query = _context.Users
                    .Where(u => studentIds.Contains(u.Id))
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u =>
                        u.Username.Contains(search) ||
                        u.Name.Contains(search) ||
                        u.WorkId.Contains(search));
                }

                var students = await query
                    .OrderBy(u => u.Name)
                    .ThenBy(u => u.Username)
                    .Select(u => new
                    {
                        id = u.Id,
                        username = u.Username,
                        name = string.IsNullOrWhiteSpace(u.Name) ? u.Username : u.Name,
                        workId = u.WorkId,
                        department = u.Department,
                        email = u.Email,
                        phone = u.Phone,
                        createdAt = u.CreatedAt
                    })
                    .ToListAsync();

                var items = students.Select(student =>
                {
                    var studentWorks = works
                        .Where(w => w.UserId == student.id)
                        .OrderByDescending(w => w.FileUploadTime ?? w.UploadDate)
                        .ToList();

                    return new
                    {
                        student.id,
                        student.username,
                        student.name,
                        student.workId,
                        student.department,
                        student.email,
                        student.phone,
                        student.createdAt,
                        workCount = studentWorks.Count,
                        publishedCount = studentWorks.Count(w => w.Status == "已发布"),
                        excellentCount = studentWorks.Count(w => w.IsExcellent),
                        latestUploadTime = studentWorks.FirstOrDefault()?.FileUploadTime ?? studentWorks.FirstOrDefault()?.UploadDate
                    };
                });

                return Ok(new
                {
                    items,
                    total = students.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取教师学生列表失败: {ex.Message}");
                return StatusCode(500, new { message = "获取学生列表失败" });
            }
        }

        [HttpGet("works")]
        public async Task<IActionResult> GetStudentWorks(
            [FromQuery] int? studentId = null,
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                var studentIds = await GetManagedStudentIdsAsync();
                if (studentId.HasValue)
                {
                    if (!studentIds.Contains(studentId.Value))
                    {
                        return Forbid();
                    }

                    studentIds = new List<int> { studentId.Value };
                }

                var query = BuildManagedWorksQuery(studentIds);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(w =>
                        w.Title.Contains(search) ||
                        (w.Description != null && w.Description.Contains(search)));
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(w => w.Category == category);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(w => w.Status == status);
                }

                var total = await query.CountAsync();

                var items = await query
                    .Join(
                        _context.Users,
                        w => w.UserId,
                        u => u.Id,
                        (w, u) => new
                        {
                            id = w.Id,
                            title = w.Title,
                            category = w.Category,
                            description = w.Description,
                            uploadDate = w.UploadDate,
                            status = w.Status,
                            filePath = w.FilePath,
                            fileName = w.FileName,
                            fileSize = w.FileSize,
                            fileMD5 = w.FileMD5,
                            fileUploadTime = w.FileUploadTime,
                            userId = w.UserId,
                            views = w.Views,
                            favorites = w.Favorites,
                            isExcellent = w.IsExcellent,
                            studentName = string.IsNullOrWhiteSpace(u.Name) ? u.Username : u.Name,
                            studentUsername = u.Username,
                            studentWorkId = u.WorkId,
                            studentDepartment = u.Department
                        })
                    .OrderByDescending(x => x.fileUploadTime ?? x.uploadDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    items,
                    total
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取学生作品列表失败: {ex.Message}");
                return StatusCode(500, new { message = "获取学生作品列表失败" });
            }
        }

        [HttpPut("works/{id}/review")]
        public async Task<IActionResult> ReviewStudentWork(int id, [FromBody] TeachingWorkReviewRequest request)
        {
            try
            {
                var studentIds = await GetManagedStudentIdsAsync();
                var work = await BuildManagedWorksQuery(studentIds)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (work == null)
                {
                    return NotFound(new { message = "作品不存在或不属于你的学生" });
                }

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    work.Status = request.Status;
                }

                if (request.IsExcellent.HasValue)
                {
                    work.IsExcellent = request.IsExcellent.Value;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "学生作品更新成功",
                    work = new
                    {
                        id = work.Id,
                        status = work.Status,
                        isExcellent = work.IsExcellent
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新学生作品失败: {ex.Message}");
                return StatusCode(500, new { message = "更新学生作品失败" });
            }
        }
    }

    public class TeachingWorkReviewRequest
    {
        public string? Status { get; set; }
        public bool? IsExcellent { get; set; }
    }
}
