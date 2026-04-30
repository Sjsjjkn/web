using Backend.Data;
using Backend.Models;
using Backend.Security;
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

                // 注意：不要打印密码等敏感信息
                Console.WriteLine($"Login attempt: Username={request.Username}");

                // 查找用户
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
                Console.WriteLine($"User found: {user != null}");

                if (user == null)
                {
                    Console.WriteLine($"User not found: {request.Username}");
                    return Unauthorized(new { message = "用户名或密码错误" });
                }

                Console.WriteLine($"User found: Id={user.Id}, Username={user.Username}");

                // 密码校验：PBKDF2 哈希（兼容旧的明文数据）
                if (!PasswordHasher.Verify(request.Password, user.Password))
                {
                    Console.WriteLine($"Password mismatch for user: {request.Username}");
                    return Unauthorized(new { message = "用户名或密码错误" });
                }

                // 角色以数据库字段为准（避免按用户名硬编码导致权限混乱）
                var role = string.IsNullOrWhiteSpace(user.Role) ? "Student" : user.Role;

                // 生成JWT token
                try
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, role),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    // 从配置文件中读取密钥
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

                    // 登录成功，返回用户信息和token
                    Console.WriteLine($"Login successful: {request.Username}, Role: {role}");
                    return Ok(new {
                        message = "登录成功",
                        token = tokenString,
                        user = new {
                            id = user.Id,
                            username = user.Username,
                            role = role
                        }
                    });
                }
                catch (Exception tokenEx)
                {
                    Console.WriteLine($"Token generation error: {tokenEx.Message}");
                    Console.WriteLine($"Token generation stack trace: {tokenEx.StackTrace}");
                    // 如果token生成失败，仍然返回登录成功，但不包含token
                    return Ok(new {
                        message = "登录成功（Token生成失败）",
                        user = new {
                            id = user.Id,
                            username = user.Username,
                            role = role
                        }
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

            // 验证用户名格式
            if (request.Username.Length < 4 || request.Username.Length > 20 || !System.Text.RegularExpressions.Regex.IsMatch(request.Username, "^[a-zA-Z0-9_]+$"))
            {
                return BadRequest(new { message = "用户名格式不正确（4-20位字母、数字或下划线）" });
            }

            // 验证密码长度（基本长度检查）
            if (request.Password.Length < 6 || request.Password.Length > 20)
            {
                return BadRequest(new { message = "密码长度必须在6-20位之间" });
            }

            // 检查用户名是否已存在
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
            {
                return Conflict(new { message = "用户名已被注册" });
            }

            // 创建新用户
            // 创建新用户（密码哈希后存储）
            var newUser = new User
            {
                Username = request.Username,
                Password = PasswordHasher.Hash(request.Password),
                CreatedAt = DateTime.Now,
                Role = "Student" // 默认角色为学生
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new {
                message = "注册成功",
                user = new {
                    id = newUser.Id,
                    username = newUser.Username
                }
            });
        }

        // GET: api/Auth/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // 这里应该根据当前登录用户获取信息
                // 暂时返回第一个用户的信息作为示例
                var user = await _context.Users.FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                return Ok(new {
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
                    createdAt = user.CreatedAt
                });
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

                // 这里应该根据当前登录用户更新信息
                // 暂时更新第一个用户的信息作为示例
                var user = await _context.Users.FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

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
                    // 验证当前密码
                    if (!PasswordHasher.Verify(request.CurrentPassword, user.Password))
                    {
                        return Unauthorized(new { message = "当前密码错误" });
                    }

                    // 基本密码长度检查
                    if (request.NewPassword.Length < 6 || request.NewPassword.Length > 20)
                    {
                        return BadRequest(new { message = "新密码长度必须在6-20位之间" });
                    }

                    user.Password = PasswordHasher.Hash(request.NewPassword);
                }

                await _context.SaveChangesAsync();

                return Ok(new {
                    message = "个人信息更新成功",
                    user = new {
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
                        createdAt = user.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"更新个人信息失败: {ex.Message}" });
            }
        }

        // PUT: api/Auth/role/{id},需要手动更换权限情况。
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

                // 验证角色值
                if (!new[] { "Admin", "Teacher", "Student" }.Contains(request.Role))
                {
                    return BadRequest(new { message = "角色值无效，可选值: Admin, Teacher, Student" });
                }

                user.Role = request.Role;
                await _context.SaveChangesAsync();

                return Ok(new {
                    message = "角色更新成功",
                    user = new {
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

                // 这里应该根据当前登录用户更新密码
                // 暂时更新第一个用户的密码作为示例
                var user = await _context.Users.FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                // 验证当前密码
                if (!PasswordHasher.Verify(request.CurrentPassword, user.Password))
                {
                    return Unauthorized(new { message = "当前密码错误" });
                }

                // 基本密码长度检查
                if (request.NewPassword.Length < 6 || request.NewPassword.Length > 20)
                {
                    return BadRequest(new { message = "新密码长度必须在6-20位之间" });
                }

                user.Password = PasswordHasher.Hash(request.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new {
                    message = "密码修改成功"
                });
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

                // 搜索筛选
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.Username.Contains(search) || u.Name.Contains(search));
                }

                // 角色筛选
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                // 计算总数
                var total = await query.CountAsync();

                // 分页
                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new {
                        id = u.Id,
                        username = u.Username,
                        name = u.Name,
                        role = u.Role,
                        workId = u.WorkId,
                        department = u.Department,
                        createdAt = u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new {
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
                    // 解析Excel文件
                    importedUsers = await ParseExcelFile(file);
                }
                else if (fileType == "csv" || file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    // 解析CSV文件
                    importedUsers = await ParseCsvFile(file);
                }
                else
                {
                    return BadRequest(new { message = "不支持的文件类型" });
                }

                // 导入用户到数据库
                int importedCount = 0;
                int skippedCount = 0;

                foreach (var user in importedUsers)
                {
                    // 检查用户是否已存在
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

                return Ok(new {
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

        // 解析Excel文件
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

                    // 从第二行开始解析（第一行是表头）
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
                                    Password = workId ?? "123456", // 初始密码为工号/学号，为空时使用默认密码
                                    Name = name,
                                    Role = role, // Admin, Teacher, College, Student
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

        // 解析CSV文件
        private async Task<List<User>> ParseCsvFile(IFormFile file)
        {
            var users = new List<User>();

            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                // 跳过表头
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
                                    Password = workId ?? "123456", // 初始密码为工号/学号，为空时使用默认密码
                                    Name = name,
                                    Role = role, // Admin, Teacher, College, Student
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

                // 创建Excel文件
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Users");

                    // 设置表头
                    worksheet.Cells[1, 1].Value = "用户名";
                    worksheet.Cells[1, 2].Value = "姓名";
                    worksheet.Cells[1, 3].Value = "角色";
                    worksheet.Cells[1, 4].Value = "工号/学号";
                    worksheet.Cells[1, 5].Value = "部门/院系";
                    worksheet.Cells[1, 6].Value = "密码";

                    // 设置表头样式
                    using (var headerRange = worksheet.Cells[1, 1, 1, 6])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    // 填充数据
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

                    // 自动调整列宽
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // 保存到内存流
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    // 返回Excel文件
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

                // 防止删除admin用户
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

                return Ok(new {
                    message = "密码重置成功"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"密码重置失败: {ex.Message}" });
            }
        }

    }
}

// 修改密码请求模型
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

// 重置密码请求模型
public class ResetPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}

// 登录请求模型
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Remember { get; set; }
}

// 注册请求模型
public class RegisterRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

// 更新个人信息请求模型
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

// 更新角色请求模型
public class UpdateRoleRequest
{
    public string? Role { get; set; }
}