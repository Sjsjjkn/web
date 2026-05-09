using Backend.Data;
using Backend.Models;
using Backend.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        // ==================== 辅助方法 ====================

        /// <summary>从 JWT Token 中获取当前登录用户的 Id</summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("无法识别当前用户身份");
            return int.Parse(userIdClaim);
        }

        /// <summary>获取当前登录用户实体</summary>
        private async Task<User> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("当前用户不存在");
            return user;
        }

        /// <summary>构建用户信息返回对象</summary>
        private static object BuildUserResponse(User user)
        {
            return new
            {
                id = user.Id,
                username = user.Username,
                role = user.Role,
                name = user.Name,
                workId = user.WorkId,
                department = user.Department,
                phone = user.Phone,
                email = user.Email,
                bio = user.Bio,
                title = user.Title,
                researchArea = user.ResearchArea,
                position = user.Position,
                avatar = user.Avatar,
                createdAt = user.CreatedAt,

                // 偏好设置
                theme = user.Theme,
                language = user.Language,
                notificationEnabled = user.NotificationEnabled,
                emailNotification = user.EmailNotification,
                workModerationRequired = user.WorkModerationRequired,
                profilePublic = user.ProfilePublic,
                showContactInfo = user.ShowContactInfo,
                favoritesVisibility = user.FavoritesVisibility
            };
        }

        // ==================== 登录 / 注册 ====================

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                Console.WriteLine("Login method called");

                if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    Console.WriteLine("Invalid request: username or password is empty");
                    return BadRequest(new { message = "用户名和密码不能为空" });
                }

                Console.WriteLine($"Login attempt: Username={request.Username}");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
                Console.WriteLine($"User found: {user != null}");

                if (user == null)
                {
                    Console.WriteLine($"User not found: {request.Username}");
                    return Unauthorized(new { message = "用户名或密码错误" });
                }

                Console.WriteLine($"User found: Id={user.Id}, Username={user.Username}");

                if (!PasswordHasher.Verify(request.Password, user.Password))
                {
                    Console.WriteLine($"Password mismatch for user: {request.Username}");
                    return Unauthorized(new { message = "用户名或密码错误" });
                }

                var role = string.IsNullOrWhiteSpace(user.Role) ? "Student" : user.Role;

                try
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, role),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    var jwtKey = _configuration["Jwt:Key"];
                    if (string.IsNullOrWhiteSpace(jwtKey))
                    {
                        return StatusCode(500, new { message = "服务端JWT配置缺失" });
                    }
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddHours(24),
                        signingCredentials: creds);

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                    Console.WriteLine($"Token generated: {tokenString.Substring(0, 50)}...");

                    Console.WriteLine($"Login successful: {request.Username}, Role: {role}");

                    // 登录成功返回完整用户信息（含偏好设置）
                    return Ok(new
                    {
                        message = "登录成功",
                        token = tokenString,
                        user = BuildUserResponse(user)
                    });
                }
                catch (Exception tokenEx)
                {
                    Console.WriteLine($"Token generation error: {tokenEx.Message}");
                    Console.WriteLine($"Token generation stack trace: {tokenEx.StackTrace}");
                    return Ok(new
                    {
                        message = "登录成功（Token生成失败）",
                        user = BuildUserResponse(user)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"登录失败: {ex.Message}" });
            }
        }

        // GET: api/Auth/test
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API测试成功", timestamp = DateTime.Now });
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "用户名和密码不能为空" });
            }

            if (request.Username.Length < 4 || request.Username.Length > 20 || !System.Text.RegularExpressions.Regex.IsMatch(request.Username, "^[a-zA-Z0-9_]+$"))
            {
                return BadRequest(new { message = "用户名格式不正确（4-20位字母、数字或下划线）" });
            }

            if (request.Password.Length < 6 || request.Password.Length > 20)
            {
                return BadRequest(new { message = "密码长度必须在6-20位之间" });
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
            {
                return Conflict(new { message = "用户名已被注册" });
            }

            var newUser = new User
            {
                Username = request.Username,
                Password = PasswordHasher.Hash(request.Password),
                CreatedAt = DateTime.Now,
                Role = "Student"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "注册成功",
                user = new
                {
                    id = newUser.Id,
                    username = newUser.Username
                }
            });
        }

        // ==================== 个人信息（需认证）====================

        // GET: api/Auth/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                return Ok(BuildUserResponse(user));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取用户信息失败: {ex.Message}" });
            }
        }

        // PUT: api/Auth/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "请求数据不能为空" });
                }

                var user = await GetCurrentUserAsync();

                // 更新用户基本信息
                if (!string.IsNullOrEmpty(request.Name))
                    user.Name = request.Name;
                if (!string.IsNullOrEmpty(request.WorkId))
                    user.WorkId = request.WorkId;
                if (!string.IsNullOrEmpty(request.Department))
                    user.Department = request.Department;
                if (!string.IsNullOrEmpty(request.Phone))
                    user.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.Email))
                    user.Email = request.Email;
                if (request.Bio != null)
                    user.Bio = request.Bio;
                if (!string.IsNullOrEmpty(request.Title))
                    user.Title = request.Title;
                if (!string.IsNullOrEmpty(request.ResearchArea))
                    user.ResearchArea = request.ResearchArea;
                if (!string.IsNullOrEmpty(request.Position))
                    user.Position = request.Position;

                // 更新密码（如果提供了当前密码和新密码）
                if (!string.IsNullOrEmpty(request.CurrentPassword) && !string.IsNullOrEmpty(request.NewPassword))
                {
                    if (!PasswordHasher.Verify(request.CurrentPassword, user.Password))
                    {
                        return Unauthorized(new { message = "当前密码错误" });
                    }
                    if (request.NewPassword.Length < 6 || request.NewPassword.Length > 20)
                    {
                        return BadRequest(new { message = "新密码长度必须在6-20位之间" });
                    }
                    user.Password = PasswordHasher.Hash(request.NewPassword);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "个人信息更新成功",
                    user = BuildUserResponse(user)
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"更新个人信息失败: {ex.Message}" });
            }
        }

        // ==================== 用户偏好设置（需认证）====================

        // GET: api/Auth/preferences
        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                return Ok(new
                {
                    theme = user.Theme,
                    language = user.Language,
                    notificationEnabled = user.NotificationEnabled,
                    emailNotification = user.EmailNotification,
                    workModerationRequired = user.WorkModerationRequired,
                    profilePublic = user.ProfilePublic,
                    showContactInfo = user.ShowContactInfo,
                    favoritesVisibility = user.FavoritesVisibility
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取偏好设置失败: {ex.Message}" });
            }
        }

        // PUT: api/Auth/preferences
        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "请求数据不能为空" });

                var user = await GetCurrentUserAsync();

                // 仅更新非 null / 非默认值的字段
                if (!string.IsNullOrEmpty(request.Theme))
                    user.Theme = request.Theme;
                if (!string.IsNullOrEmpty(request.Language))
                    user.Language = request.Language;
                if (request.NotificationEnabled.HasValue)
                    user.NotificationEnabled = request.NotificationEnabled.Value;
                if (request.EmailNotification.HasValue)
                    user.EmailNotification = request.EmailNotification.Value;
                if (request.WorkModerationRequired.HasValue)
                    user.WorkModerationRequired = request.WorkModerationRequired.Value;
                if (request.ProfilePublic.HasValue)
                    user.ProfilePublic = request.ProfilePublic.Value;
                if (request.ShowContactInfo.HasValue)
                    user.ShowContactInfo = request.ShowContactInfo.Value;
                if (!string.IsNullOrEmpty(request.FavoritesVisibility))
                    user.FavoritesVisibility = request.FavoritesVisibility;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "偏好设置更新成功",
                    preferences = new
                    {
                        theme = user.Theme,
                        language = user.Language,
                        notificationEnabled = user.NotificationEnabled,
                        emailNotification = user.EmailNotification,
                        workModerationRequired = user.WorkModerationRequired,
                        profilePublic = user.ProfilePublic,
                        showContactInfo = user.ShowContactInfo,
                        favoritesVisibility = user.FavoritesVisibility
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"更新偏好设置失败: {ex.Message}" });
            }
        }

        // ==================== 公开个人主页 ====================

        // GET: api/Auth/public-profile/{userId}
        [AllowAnonymous]
        [HttpGet("public-profile/{userId}")]
        public async Task<IActionResult> GetPublicProfile(int userId)
        {
            try
            {
                Console.WriteLine($"收到公开资料请求，userId: {userId}");
                
                var user = await _context.Users
                    .Include(u => u.UploadedWorks)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    Console.WriteLine($"用户不存在，userId: {userId}");
                    return NotFound(new { message = "用户不存在" });
                }

                if (!user.ProfilePublic)
                {
                    Console.WriteLine($"用户主页未公开，userId: {userId}, ProfilePublic: {user.ProfilePublic}");
                    return StatusCode(403, new { message = "该用户的个人主页未公开" });
                }

                Console.WriteLine($"成功获取用户资料，userId: {userId}, name: {user.Name}");
                
                return Ok(new
                {
                    id = user.Id,
                    name = user.Name,
                    role = user.Role,
                    workId = user.WorkId,
                    department = user.Department,
                    avatar = user.Avatar != null ? $"/api/File/download?fileName={Uri.EscapeDataString(user.Avatar)}" : null,
                    bio = user.Bio,
                    works = user.UploadedWorks
                        .Where(w => w.Status == "已发布")
                        .Select(w => new
                        {
                            id = w.Id,
                            title = w.Title,
                            description = w.Description,
                            thumbnailUrl = w.PreviewImage != null ? $"/api/File/download?fileName={Uri.EscapeDataString(w.PreviewImage)}" : null,
                            fileType = Path.GetExtension(w.FileName ?? "").TrimStart('.').ToUpper(),
                            createdAt = w.UploadDate
                        })
                        .Take(6)
                        .ToList(),
                    workCount = user.UploadedWorks.Count(w => w.Status == "已发布")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取公开资料失败: {ex.Message}" });
            }
        }

        // ==================== 头像上传 ====================

        // PUT: api/Auth/avatar
        // 接受 JSON body: { avatar: "fileName" }
        [HttpPut("avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] UpdateAvatarRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Avatar))
                    return BadRequest(new { message = "请提供头像文件名" });

                var user = await GetCurrentUserAsync();
                user.Avatar = request.Avatar;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "头像更新成功",
                    avatar = request.Avatar,
                    avatarUrl = $"/api/File/download?fileName={Uri.EscapeDataString(request.Avatar)}"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"头像更新失败: {ex.Message}" });
            }
        }

        // ==================== 个人统计 ====================

        // GET: api/Auth/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetPersonalStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users
                    .Include(u => u.UploadedWorks)
                    .Include(u => u.WorkFavorites)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound(new { message = "用户不存在" });

                var workCount = user.UploadedWorks?.Count(w => w.Status == "已发布") ?? 0;
                var favoriteCount = user.WorkFavorites?.Count ?? 0;
                var viewCount = user.UploadedWorks?.Sum(w => w.Views) ?? 0;

                return Ok(new
                {
                    workCount,
                    favoriteCount,
                    viewCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取统计数据失败: {ex.Message}" });
            }
        }

        // ==================== 管理接口 ====================

        // PUT: api/Auth/role/{id}
        [HttpPut("role/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                if (!new[] { "Admin", "Teacher", "Student" }.Contains(request.Role))
                {
                    return BadRequest(new { message = "角色值无效，可选值: Admin, Teacher, Student" });
                }

                user.Role = request.Role;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "角色更新成功",
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"更新角色失败: {ex.Message}" });
            }
        }

        // PUT: api/Auth/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { message = "请求数据不能为空" });
                }

                var user = await GetCurrentUserAsync();

                if (!PasswordHasher.Verify(request.CurrentPassword, user.Password))
                {
                    return Unauthorized(new { message = "当前密码错误" });
                }

                if (request.NewPassword.Length < 6 || request.NewPassword.Length > 20)
                {
                    return BadRequest(new { message = "新密码长度必须在6-20位之间" });
                }

                user.Password = PasswordHasher.Hash(request.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new { message = "密码修改成功" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"修改密码失败: {ex.Message}" });
            }
        }

        // GET: api/Auth/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search = null, [FromQuery] string? role = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.Username.Contains(search) || u.Name.Contains(search));
                }

                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                var total = await query.CountAsync();

                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        id = u.Id,
                        username = u.Username,
                        name = u.Name,
                        role = u.Role,
                        workId = u.WorkId,
                        department = u.Department,
                        createdAt = u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    items = users,
                    total = total
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取用户列表失败: {ex.Message}" });
            }
        }

        // POST: api/Auth/import
        [HttpPost("import")]
        public async Task<IActionResult> ImportUsers(IFormFile file, [FromForm] string fileType)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "请选择要上传的文件" });
                }

                Console.WriteLine($"Importing file: {file.FileName}, Type: {fileType}");

                var importedUsers = new List<User>();

                if (fileType == "excel" || file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    importedUsers = await ParseExcelFile(file);
                }
                else if (fileType == "csv" || file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    importedUsers = await ParseCsvFile(file);
                }
                else
                {
                    return BadRequest(new { message = "不支持的文件类型" });
                }

                int importedCount = 0;
                int skippedCount = 0;

                foreach (var user in importedUsers)
                {
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                    if (existingUser == null)
                    {
                        _context.Users.Add(user);
                        importedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "导入成功",
                    importedCount = importedCount,
                    skippedCount = skippedCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import error: {ex.Message}");
                return StatusCode(500, new { message = $"导入失败: {ex.Message}" });
            }
        }

        private async Task<List<User>> ParseExcelFile(IFormFile file)
        {
            var users = new List<User>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var username = worksheet.Cells[row, 1].Text?.Trim();
                            var name = worksheet.Cells[row, 2].Text?.Trim();
                            var role = worksheet.Cells[row, 3].Text?.Trim();
                            var workId = worksheet.Cells[row, 4].Text?.Trim();
                            var department = worksheet.Cells[row, 5].Text?.Trim();

                            if (!string.IsNullOrEmpty(username))
                            {
                                users.Add(new User
                                {
                                    Username = username,
                                    Password = workId ?? "123456",
                                    Name = name,
                                    Role = role,
                                    WorkId = workId,
                                    Department = department,
                                    CreatedAt = DateTime.Now
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing row {row}: {ex.Message}");
                        }
                    }
                }
            }

            return users;
        }

        private async Task<List<User>> ParseCsvFile(IFormFile file)
        {
            var users = new List<User>();

            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                await stream.ReadLineAsync();

                string line;
                while ((line = await stream.ReadLineAsync()) != null)
                {
                    try
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 5)
                        {
                            var username = parts[0]?.Trim();
                            var name = parts[1]?.Trim();
                            var role = parts[2]?.Trim();
                            var workId = parts[3]?.Trim();
                            var department = parts[4]?.Trim();

                            if (!string.IsNullOrEmpty(username))
                            {
                                users.Add(new User
                                {
                                    Username = username,
                                    Password = workId ?? "123456",
                                    Name = name,
                                    Role = role,
                                    WorkId = workId,
                                    Department = department,
                                    CreatedAt = DateTime.Now
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing CSV line: {ex.Message}");
                    }
                }
            }

            return users;
        }

        // GET: api/Auth/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportUsers()
        {
            try
            {
                Console.WriteLine("Exporting users");

                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Users");

                    worksheet.Cells[1, 1].Value = "用户名";
                    worksheet.Cells[1, 2].Value = "姓名";
                    worksheet.Cells[1, 3].Value = "角色";
                    worksheet.Cells[1, 4].Value = "工号/学号";
                    worksheet.Cells[1, 5].Value = "部门/院系";
                    worksheet.Cells[1, 6].Value = "密码";

                    using (var headerRange = worksheet.Cells[1, 1, 1, 6])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    var users = await _context.Users.ToListAsync();
                    for (int i = 0; i < users.Count; i++)
                    {
                        var user = users[i];
                        int row = i + 2;
                        worksheet.Cells[row, 1].Value = user.Username;
                        worksheet.Cells[row, 2].Value = user.Name;
                        worksheet.Cells[row, 3].Value = user.Role;
                        worksheet.Cells[row, 4].Value = user.WorkId;
                        worksheet.Cells[row, 5].Value = user.Department;
                        worksheet.Cells[row, 6].Value = user.Password;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"users-{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export error: {ex.Message}");
                return StatusCode(500, new { message = $"导出失败: {ex.Message}" });
            }
        }

        // DELETE: api/Auth/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                if (user.Username == "admin")
                {
                    return BadRequest(new { message = "不能删除管理员用户" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete error: {ex.Message}");
                return StatusCode(500, new { message = $"删除失败: {ex.Message}" });
            }
        }

        // PUT: api/Auth/password/{id}
        [HttpPut("password/{id}")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                user.Password = request.Password;
                await _context.SaveChangesAsync();

                return Ok(new { message = "密码重置成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"密码重置失败: {ex.Message}" });
            }
        }
    }

    // ==================== 请求模型 ====================

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Remember { get; set; }
    }

    public class RegisterRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? Name { get; set; }
        public string? WorkId { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public string? Title { get; set; }
        public string? ResearchArea { get; set; }
        public string? Position { get; set; }
    }

    public class UpdatePreferencesRequest
    {
        public string? Theme { get; set; }
        public string? Language { get; set; }
        public bool? NotificationEnabled { get; set; }
        public bool? EmailNotification { get; set; }
        public bool? WorkModerationRequired { get; set; }
        public bool? ProfilePublic { get; set; }
        public bool? ShowContactInfo { get; set; }
        public string? FavoritesVisibility { get; set; }
    }

    public class UpdateRoleRequest
    {
        public string? Role { get; set; }
    }

    public class UpdateAvatarRequest
    {
        public string? Avatar { get; set; }
    }
}
