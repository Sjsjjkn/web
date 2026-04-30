using Backend.Data;
using Backend.Models;
using Backend.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
Directory.CreateDirectory(dataProtectionPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("Backend");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(
                "http://localhost:8080",
                "http://localhost:8081",
                "http://localhost:8082",
                "http://localhost:8083",
                "http://localhost:8084")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Console.WriteLine("开始初始化数据库...");
        dbContext.Database.EnsureCreated();
        EnsureSupportTables(dbContext);
        SeedUsersAndTeachingDemoData(dbContext);
        SeedModerationDemoData(dbContext);
        Console.WriteLine("数据库初始化完成");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"数据库初始化错误: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void EnsureSupportTables(AppDbContext dbContext)
{
    dbContext.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS `StudentTeachers` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `StudentId` int NOT NULL,
            `TeacherId` int NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            INDEX `IX_StudentTeachers_StudentId` (`StudentId`),
            INDEX `IX_StudentTeachers_TeacherId` (`TeacherId`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
    ");

    dbContext.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS `WorkStudents` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `WorkId` int NOT NULL,
            `StudentId` int NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            INDEX `IX_WorkStudents_WorkId` (`WorkId`),
            INDEX `IX_WorkStudents_StudentId` (`StudentId`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
    ");

    dbContext.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS `ModerationItems` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `Title` varchar(255) NOT NULL,
            `Author` varchar(100) NOT NULL,
            `Category` varchar(100) NOT NULL,
            `Type` varchar(50) NOT NULL,
            `FileName` varchar(255) NOT NULL,
            `Preview` text NOT NULL,
            `Content` text NOT NULL,
            `SubmitTime` datetime(6) NOT NULL,
            `RiskLevel` varchar(20) NOT NULL,
            `RiskDetails` text NOT NULL,
            `Status` varchar(20) NOT NULL,
            PRIMARY KEY (`Id`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
    ");
}

static void SeedUsersAndTeachingDemoData(AppDbContext dbContext)
{
    EnsureUser(
        dbContext,
        username: "admin",
        role: "Admin",
        name: "系统管理员",
        workId: "A0001",
        department: "信息中心",
        email: "admin@bdu.local");

    EnsureUser(
        dbContext,
        username: "teacher_zhang",
        role: "Teacher",
        name: "张老师",
        workId: "T2026001",
        department: "数字媒体教研室",
        email: "teacher_zhang@bdu.local",
        title: "讲师",
        researchArea: "数字作品指导");

    EnsureUser(
        dbContext,
        username: "teacher_li",
        role: "Teacher",
        name: "李老师",
        workId: "T2026002",
        department: "人工智能教研室",
        email: "teacher_li@bdu.local",
        title: "副教授",
        researchArea: "智能创作");

    EnsureUser(
        dbContext,
        username: "student_wang",
        role: "Student",
        name: "王晨",
        workId: "20261001",
        department: "数字媒体技术",
        email: "student_wang@bdu.local");

    EnsureUser(
        dbContext,
        username: "student_liu",
        role: "Student",
        name: "刘悦",
        workId: "20261002",
        department: "数字媒体技术",
        email: "student_liu@bdu.local");

    EnsureUser(
        dbContext,
        username: "student_chen",
        role: "Student",
        name: "陈思",
        workId: "20261003",
        department: "人工智能",
        email: "student_chen@bdu.local");

    EnsureUser(
        dbContext,
        username: "student_zhao",
        role: "Student",
        name: "赵宁",
        workId: "20261004",
        department: "人工智能",
        email: "student_zhao@bdu.local");

    dbContext.SaveChanges();

    var teacherZhang = dbContext.Users.First(u => u.Username == "teacher_zhang");
    var teacherLi = dbContext.Users.First(u => u.Username == "teacher_li");
    var studentWang = dbContext.Users.First(u => u.Username == "student_wang");
    var studentLiu = dbContext.Users.First(u => u.Username == "student_liu");
    var studentChen = dbContext.Users.First(u => u.Username == "student_chen");
    var studentZhao = dbContext.Users.First(u => u.Username == "student_zhao");

    EnsureTeacherBinding(dbContext, teacherZhang.Id, studentWang.Id);
    EnsureTeacherBinding(dbContext, teacherZhang.Id, studentLiu.Id);
    EnsureTeacherBinding(dbContext, teacherLi.Id, studentChen.Id);
    EnsureTeacherBinding(dbContext, teacherLi.Id, studentZhao.Id);
    dbContext.SaveChanges();

    EnsureWork(
        dbContext,
        title: "校园导览小程序",
        category: "前端开发",
        status: "已发布",
        isExcellent: true,
        student: studentWang,
        description: "用于展示校园建筑与导览路线的小程序作品。",
        daysAgo: 8);

    EnsureWork(
        dbContext,
        title: "毕业作品管理后台",
        category: "后端开发",
        status: "待审核",
        isExcellent: false,
        student: studentWang,
        description: "面向毕业展的作品管理后台。",
        daysAgo: 5);

    EnsureWork(
        dbContext,
        title: "交互海报设计",
        category: "设计",
        status: "已发布",
        isExcellent: true,
        student: studentLiu,
        description: "用于毕业展宣传的交互海报与视觉方案。",
        daysAgo: 7);

    EnsureWork(
        dbContext,
        title: "课堂作业提交系统",
        category: "前端开发",
        status: "草稿",
        isExcellent: false,
        student: studentLiu,
        description: "支持阶段性提交与状态跟踪的课堂作业系统。",
        daysAgo: 3);

    EnsureWork(
        dbContext,
        title: "智能答疑助手",
        category: "人工智能",
        status: "已发布",
        isExcellent: true,
        student: studentChen,
        description: "面向课程学习场景的问答助手原型。",
        daysAgo: 6);

    EnsureWork(
        dbContext,
        title: "作品标签推荐模型",
        category: "人工智能",
        status: "待审核",
        isExcellent: false,
        student: studentChen,
        description: "为学生作品自动打标签的推荐模型。",
        daysAgo: 2);

    EnsureWork(
        dbContext,
        title: "校园文化短片",
        category: "视频动画",
        status: "已发布",
        isExcellent: false,
        student: studentZhao,
        description: "用于展示校园文化的短视频作品。",
        daysAgo: 4);

    EnsureWork(
        dbContext,
        title: "三维展厅原型",
        category: "设计",
        status: "草稿",
        isExcellent: false,
        student: studentZhao,
        description: "线上毕业展三维展厅交互原型。",
        daysAgo: 1);

    dbContext.SaveChanges();
}

static void EnsureUser(
    AppDbContext dbContext,
    string username,
    string role,
    string name,
    string workId,
    string department,
    string email,
    string title = "",
    string researchArea = "")
{
    var user = dbContext.Users.FirstOrDefault(u => u.Username == username);
    if (user == null)
    {
        user = new User
        {
            Username = username,
            Password = PasswordHasher.Hash("123456"),
            CreatedAt = DateTime.Now
        };
        dbContext.Users.Add(user);
    }

    user.Role = role;
    user.Name = name;
    user.WorkId = workId;
    user.Department = department;
    user.Email = email;
    user.Title = title;
    user.ResearchArea = researchArea;
}

static void EnsureTeacherBinding(AppDbContext dbContext, int teacherId, int studentId)
{
    if (dbContext.StudentTeachers.Any(x => x.TeacherId == teacherId && x.StudentId == studentId))
    {
        return;
    }

    dbContext.StudentTeachers.Add(new StudentTeacher
    {
        TeacherId = teacherId,
        StudentId = studentId,
        CreatedAt = DateTime.Now
    });
}

static void EnsureWork(
    AppDbContext dbContext,
    string title,
    string category,
    string status,
    bool isExcellent,
    User student,
    string description,
    int daysAgo)
{
    var existingWork = dbContext.Works.FirstOrDefault(w => w.Title == title && w.UserId == student.Id);
    if (existingWork == null)
    {
        var uploadTime = DateTime.Now.AddDays(-daysAgo);
        existingWork = new Work
        {
            Title = title,
            Category = category,
            Description = description,
            UploadDate = uploadTime,
            FileUploadTime = uploadTime,
            Status = status,
            FilePath = $"{student.Username}_{title}.zip",
            FileName = $"{title}.zip",
            FileSize = 1024L * 1024L * (daysAgo + 6),
            FileMD5 = Guid.NewGuid().ToString("N"),
            UserId = student.Id,
            IsExcellent = isExcellent,
            Views = 20 + daysAgo * 5,
            Favorites = daysAgo
        };
        dbContext.Works.Add(existingWork);
        dbContext.SaveChanges();
    }
    else
    {
        existingWork.Status = status;
        existingWork.IsExcellent = isExcellent;
    }

    if (!dbContext.WorkStudents.Any(x => x.WorkId == existingWork.Id && x.StudentId == student.Id))
    {
        dbContext.WorkStudents.Add(new WorkStudent
        {
            WorkId = existingWork.Id,
            StudentId = student.Id,
            CreatedAt = DateTime.Now
        });
    }
}

static void SeedModerationDemoData(AppDbContext dbContext)
{
    if (dbContext.ModerationItems.Any())
    {
        return;
    }

    dbContext.ModerationItems.AddRange(new List<ModerationItem>
    {
        new ModerationItem
        {
            Title = "保定学院航拍微电影",
            Author = "张同学",
            Category = "视频动画",
            Type = "video",
            FileName = "campus-aerial.mp4",
            Preview = "",
            Content = "展示校园景观与建筑风貌的短片作品。",
            SubmitTime = DateTime.Now.AddDays(-2),
            RiskLevel = "low",
            RiskDetails = "",
            Status = "pending"
        },
        new ModerationItem
        {
            Title = "校园二手交易平台",
            Author = "李同学",
            Category = "软件源码",
            Type = "document",
            FileName = "second-hand-platform.zip",
            Preview = "",
            Content = "基于 Vue 与 Node 的校园二手交易平台。",
            SubmitTime = DateTime.Now.AddDays(-2),
            RiskLevel = "medium",
            RiskDetails = "包含可执行文件，需要人工确认安全性",
            Status = "pending"
        },
        new ModerationItem
        {
            Title = "学情分析报告",
            Author = "赵同学",
            Category = "文档报告",
            Type = "document",
            FileName = "learning-analytics.pdf",
            Preview = "",
            Content = "围绕学生学习数据的分析报告。",
            SubmitTime = DateTime.Now.AddDays(-3),
            RiskLevel = "high",
            RiskDetails = "包含学生个人信息，需要脱敏处理",
            Status = "pending"
        }
    });

    dbContext.SaveChanges();
}
