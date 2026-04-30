using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DataController(AppDbContext context)
        {
            _context = context;
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

        // GET: api/Data/overview
        [HttpGet("overview")]
        public async Task<ActionResult> GetOverview()
        {
            try
            {
                // 所有用户都可以查看统计数据
                // if (!User.IsInRole("Admin") && !User.IsInRole("Teacher"))
                // {
                //     return Forbid("只有管理员和教师可以查看数据统计");
                // }

                // 总作品数
                var totalWorks = await _context.Works.CountAsync();

                // 总用户数
                var totalUsers = await _context.Users.CountAsync();

                // 今日上传数量
                var today = DateTime.Now.Date;
                var todayWorks = await _context.Works.CountAsync(w => w.UploadDate.Date == today);

                // 总浏览量（假设未来会有浏览量字段，暂时返回0）
                var totalViews = 0;

                return Ok(new {
                    totalWorks,
                    totalUsers,
                    todayWorks,
                    totalViews
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取统计概览失败: {ex.Message}");
                return StatusCode(500, new { message = "获取统计概览失败" });
            }
        }

        // GET: api/Data/daily-uploads
        [HttpGet("daily-uploads")]
        public async Task<ActionResult> GetDailyUploads([FromQuery] int days = 7)
        {
            try
            {
                // 检查用户是否为管理员或教师
                if (!User.IsInRole("Admin") && !User.IsInRole("Teacher"))
                {
                    return Forbid("只有管理员和教师可以查看数据统计");
                }

                // 生成最近days天的日期
                var dates = new List<string>();
                var uploads = new List<int>();

                for (int i = days - 1; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i).Date;
                    dates.Add(date.ToString("yyyy-MM-dd"));

                    // 统计当天的上传数量
                    var count = await _context.Works.CountAsync(w => w.UploadDate.Date == date);
                    uploads.Add(count);
                }

                return Ok(new {
                    dates,
                    uploads
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取每日上传数据失败: {ex.Message}");
                return StatusCode(500, new { message = "获取每日上传数据失败" });
            }
        }

        // GET: api/Data/category-distribution
        [HttpGet("category-distribution")]
        public async Task<ActionResult> GetCategoryDistribution()
        {
            try
            {
                // 检查用户是否为管理员或教师
                if (!User.IsInRole("Admin") && !User.IsInRole("Teacher"))
                {
                    return Forbid("只有管理员和教师可以查看数据统计");
                }

                // 统计各分类的作品数量
                var categories = await _context.Works
                    .GroupBy(w => w.Category)
                    .Select(g => new {
                        category = g.Key,
                        count = g.Count()
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取分类分布数据失败: {ex.Message}");
                return StatusCode(500, new { message = "获取分类分布数据失败" });
            }
        }

        // GET: api/Data/status-distribution
        [HttpGet("status-distribution")]
        public async Task<ActionResult> GetStatusDistribution()
        {
            try
            {
                // 检查用户是否为管理员或教师
                if (!User.IsInRole("Admin") && !User.IsInRole("Teacher"))
                {
                    return Forbid("只有管理员和教师可以查看数据统计");
                }

                // 统计各状态的作品数量
                var statuses = await _context.Works
                    .GroupBy(w => w.Status)
                    .Select(g => new {
                        status = g.Key,
                        count = g.Count()
                    })
                    .ToListAsync();

                return Ok(statuses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取状态分布数据失败: {ex.Message}");
                return StatusCode(500, new { message = "获取状态分布数据失败" });
            }
        }

        // GET: api/Data/monthly-uploads
        [HttpGet("monthly-uploads")]
        public async Task<ActionResult> GetMonthlyUploads()
        {
            try
            {
                // 检查用户是否为管理员或教师
                if (!User.IsInRole("Admin") && !User.IsInRole("Teacher"))
                {
                    return Forbid("只有管理员和教师可以查看数据统计");
                }

                // 生成全年12个月
                var months = new List<string>();
                var uploads = new List<int>();

                for (int i = 1; i <= 12; i++)
                {
                    var monthStr = $"{i}月";
                    months.Add(monthStr);

                    // 统计当月的上传数量
                    var startDate = new DateTime(DateTime.Now.Year, i, 1);
                    var endDate = new DateTime(DateTime.Now.Year, i, DateTime.DaysInMonth(DateTime.Now.Year, i));

                    var count = await _context.Works.CountAsync(w => w.UploadDate >= startDate && w.UploadDate <= endDate);
                    uploads.Add(count);
                }

                return Ok(new {
                    months,
                    uploads
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取月度上传数据失败: {ex.Message}");
                return StatusCode(500, new { message = "获取月度上传数据失败" });
            }
        }

        /// <summary>
        /// 导出数据分析报表（Excel）。
        /// </summary>
        [HttpGet("export")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> ExportReport([FromQuery] int days = 7)
        {
            try
            {
                // EPPlus 需要设置许可上下文（非商业用途）
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var overview = await GetOverviewInternal();
                var daily = await GetDailyUploadsInternal(days);
                var category = await GetCategoryDistributionInternal();
                var status = await GetStatusDistributionInternal();
                var monthly = await GetMonthlyUploadsInternal();

                using var package = new ExcelPackage();

                // 1) 概览
                var wsOverview = package.Workbook.Worksheets.Add("概览");
                wsOverview.Cells[1, 1].Value = "指标";
                wsOverview.Cells[1, 2].Value = "数值";
                wsOverview.Cells[2, 1].Value = "总作品数";
                wsOverview.Cells[2, 2].Value = overview.totalWorks;
                wsOverview.Cells[3, 1].Value = "总用户数";
                wsOverview.Cells[3, 2].Value = overview.totalUsers;
                wsOverview.Cells[4, 1].Value = "今日上传";
                wsOverview.Cells[4, 2].Value = overview.todayWorks;
                wsOverview.Cells[5, 1].Value = "总浏览量";
                wsOverview.Cells[5, 2].Value = overview.totalViews;
                wsOverview.Cells.AutoFitColumns();

                // 2) 最近每日上传
                var wsDaily = package.Workbook.Worksheets.Add("每日上传");
                wsDaily.Cells[1, 1].Value = "日期";
                wsDaily.Cells[1, 2].Value = "上传数量";
                for (int i = 0; i < daily.dates.Count; i++)
                {
                    wsDaily.Cells[i + 2, 1].Value = daily.dates[i];
                    wsDaily.Cells[i + 2, 2].Value = daily.uploads[i];
                }
                wsDaily.Cells.AutoFitColumns();

                // 3) 分类分布
                var wsCategory = package.Workbook.Worksheets.Add("分类分布");
                wsCategory.Cells[1, 1].Value = "分类";
                wsCategory.Cells[1, 2].Value = "数量";
                for (int i = 0; i < category.Count; i++)
                {
                    wsCategory.Cells[i + 2, 1].Value = category[i].category;
                    wsCategory.Cells[i + 2, 2].Value = category[i].count;
                }
                wsCategory.Cells.AutoFitColumns();

                // 4) 状态分布
                var wsStatus = package.Workbook.Worksheets.Add("状态分布");
                wsStatus.Cells[1, 1].Value = "状态";
                wsStatus.Cells[1, 2].Value = "数量";
                for (int i = 0; i < status.Count; i++)
                {
                    wsStatus.Cells[i + 2, 1].Value = status[i].status;
                    wsStatus.Cells[i + 2, 2].Value = status[i].count;
                }
                wsStatus.Cells.AutoFitColumns();

                // 5) 月度上传
                var wsMonthly = package.Workbook.Worksheets.Add("月度上传");
                wsMonthly.Cells[1, 1].Value = "月份";
                wsMonthly.Cells[1, 2].Value = "上传数量";
                for (int i = 0; i < monthly.months.Count; i++)
                {
                    wsMonthly.Cells[i + 2, 1].Value = monthly.months[i];
                    wsMonthly.Cells[i + 2, 2].Value = monthly.uploads[i];
                }
                wsMonthly.Cells.AutoFitColumns();

                var bytes = package.GetAsByteArray();
                var fileName = $"data-report-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出数据分析报表失败: {ex.Message}");
                return StatusCode(500, new { message = "导出报表失败" });
            }
        }

        #region 内部查询（复用逻辑，避免导出接口重复写SQL）

        private async Task<(int totalWorks, int totalUsers, int todayWorks, int totalViews)> GetOverviewInternal()
        {
            var totalWorks = await _context.Works.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var today = DateTime.Now.Date;
            var todayWorks = await _context.Works.CountAsync(w => w.UploadDate.Date == today);
            var totalViews = 0;
            return (totalWorks, totalUsers, todayWorks, totalViews);
        }

        private async Task<(List<string> dates, List<int> uploads)> GetDailyUploadsInternal(int days)
        {
            var dates = new List<string>();
            var uploads = new List<int>();
            for (int i = days - 1; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i).Date;
                dates.Add(date.ToString("yyyy-MM-dd"));
                var count = await _context.Works.CountAsync(w => w.UploadDate.Date == date);
                uploads.Add(count);
            }
            return (dates, uploads);
        }

        private async Task<List<(string category, int count)>> GetCategoryDistributionInternal()
        {
            var raw = await _context.Works
                .GroupBy(w => w.Category)
                .Select(g => new { category = g.Key, count = g.Count() })
                .ToListAsync();
            return raw.Select(x => (x.category ?? "未分类", x.count)).ToList();
        }

        private async Task<List<(string status, int count)>> GetStatusDistributionInternal()
        {
            var raw = await _context.Works
                .GroupBy(w => w.Status)
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToListAsync();
            return raw.Select(x => (x.status ?? "未知", x.count)).ToList();
        }

        private async Task<(List<string> months, List<int> uploads)> GetMonthlyUploadsInternal()
        {
            var months = new List<string>();
            var uploads = new List<int>();
            for (int i = 1; i <= 12; i++)
            {
                months.Add($"{i}月");
                var startDate = new DateTime(DateTime.Now.Year, i, 1);
                var endDate = new DateTime(DateTime.Now.Year, i, DateTime.DaysInMonth(DateTime.Now.Year, i));
                var count = await _context.Works.CountAsync(w => w.UploadDate >= startDate && w.UploadDate <= endDate);
                uploads.Add(count);
            }
            return (months, uploads);
        }

        #endregion
    }
}