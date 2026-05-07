using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Backend
{
    public static class InitializeDatabase
    {
        public static async Task Initialize(AppDbContext context)
        {
            Console.WriteLine("开始初始化数据库...");

            // 检查admin用户是否存在
            var adminExists = await context.Users.AnyAsync(u => u.Username == "admin");
            if (!adminExists)
            {
                // 创建admin用户
                var adminUser = new User
                {
                    Username = "admin",
                    Password = "123456", // 实际项目中应该加密
                    Name = "系统管理员",
                    Role = "Admin",
                    WorkId = "000000",
                    Department = "信息中心",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
                Console.WriteLine("admin用户创建完成");
            }
            else
            {
                Console.WriteLine("admin用户已存在，跳过创建");
            }

            // 检查教师用户是否存在
            var teacherZhangExists = await context.Users.AnyAsync(u => u.Username == "teacher_zhang");
            if (!teacherZhangExists)
            {
                var teacherZhang = new User
                {
                    Username = "teacher_zhang",
                    Password = "123456",
                    Name = "张老师",
                    Role = "Teacher",
                    WorkId = "T2026001",
                    Department = "数字媒体教研室",
                    Title = "讲师",
                    ResearchArea = "数字作品指导",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(teacherZhang);
            }

            var teacherLiExists = await context.Users.AnyAsync(u => u.Username == "teacher_li");
            if (!teacherLiExists)
            {
                var teacherLi = new User
                {
                    Username = "teacher_li",
                    Password = "123456",
                    Name = "李老师",
                    Role = "Teacher",
                    WorkId = "T2026002",
                    Department = "人工智能教研室",
                    Title = "副教授",
                    ResearchArea = "智能创作",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(teacherLi);
            }

            // 检查学生用户是否存在
            var studentWangExists = await context.Users.AnyAsync(u => u.Username == "student_wang");
            if (!studentWangExists)
            {
                var studentWang = new User
                {
                    Username = "student_wang",
                    Password = "123456",
                    Name = "王晨",
                    Role = "Student",
                    WorkId = "20261001",
                    Department = "数字媒体技术",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(studentWang);
            }

            var studentLiuExists = await context.Users.AnyAsync(u => u.Username == "student_liu");
            if (!studentLiuExists)
            {
                var studentLiu = new User
                {
                    Username = "student_liu",
                    Password = "123456",
                    Name = "刘悦",
                    Role = "Student",
                    WorkId = "20261002",
                    Department = "数字媒体技术",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(studentLiu);
            }

            var studentChenExists = await context.Users.AnyAsync(u => u.Username == "student_chen");
            if (!studentChenExists)
            {
                var studentChen = new User
                {
                    Username = "student_chen",
                    Password = "123456",
                    Name = "陈思",
                    Role = "Student",
                    WorkId = "20261003",
                    Department = "人工智能",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(studentChen);
            }

            var studentZhaoExists = await context.Users.AnyAsync(u => u.Username == "student_zhao");
            if (!studentZhaoExists)
            {
                var studentZhao = new User
                {
                    Username = "student_zhao",
                    Password = "123456",
                    Name = "赵宁",
                    Role = "Student",
                    WorkId = "20261004",
                    Department = "人工智能",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(studentZhao);
            }

            await context.SaveChangesAsync();
            Console.WriteLine("用户数据初始化完成");

            // 检查教师学生关联关系是否存在
            var tZhangId = await context.Users.Where(u => u.Username == "teacher_zhang").Select(u => u.Id).FirstOrDefaultAsync();
            var tLiId = await context.Users.Where(u => u.Username == "teacher_li").Select(u => u.Id).FirstOrDefaultAsync();
            var sWangId = await context.Users.Where(u => u.Username == "student_wang").Select(u => u.Id).FirstOrDefaultAsync();
            var sLiuId = await context.Users.Where(u => u.Username == "student_liu").Select(u => u.Id).FirstOrDefaultAsync();
            var sChenId = await context.Users.Where(u => u.Username == "student_chen").Select(u => u.Id).FirstOrDefaultAsync();
            var sZhaoId = await context.Users.Where(u => u.Username == "student_zhao").Select(u => u.Id).FirstOrDefaultAsync();
            
            // 张老师管理王晨和刘悦
            if (tZhangId > 0 && sWangId > 0)
            {
                var stExists1 = await context.StudentTeachers.AnyAsync(st => st.TeacherId == tZhangId && st.StudentId == sWangId);
                if (!stExists1)
                {
                    context.StudentTeachers.Add(new StudentTeacher { TeacherId = tZhangId, StudentId = sWangId, CreatedAt = DateTime.Now });
                }
            }

            if (tZhangId > 0 && sLiuId > 0)
            {
                var stExists2 = await context.StudentTeachers.AnyAsync(st => st.TeacherId == tZhangId && st.StudentId == sLiuId);
                if (!stExists2)
                {
                    context.StudentTeachers.Add(new StudentTeacher { TeacherId = tZhangId, StudentId = sLiuId, CreatedAt = DateTime.Now });
                }
            }

            // 李老师管理陈思和赵宁
            if (tLiId > 0 && sChenId > 0)
            {
                var stExists3 = await context.StudentTeachers.AnyAsync(st => st.TeacherId == tLiId && st.StudentId == sChenId);
                if (!stExists3)
                {
                    context.StudentTeachers.Add(new StudentTeacher { TeacherId = tLiId, StudentId = sChenId, CreatedAt = DateTime.Now });
                }
            }

            if (tLiId > 0 && sZhaoId > 0)
            {
                var stExists4 = await context.StudentTeachers.AnyAsync(st => st.TeacherId == tLiId && st.StudentId == sZhaoId);
                if (!stExists4)
                {
                    context.StudentTeachers.Add(new StudentTeacher { TeacherId = tLiId, StudentId = sZhaoId, CreatedAt = DateTime.Now });
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("教师学生关联关系初始化完成");

            // 检查内容审核测试数据是否存在
            var moderationDataExists = await context.ModerationItems.AnyAsync();
            if (!moderationDataExists)
            {
                // 创建测试数据
                var testItems = new List<ModerationItem>
                {
                    new ModerationItem
                    {
                        Title = "保定学院航拍微电影",
                        Author = "张同学",
                        Category = "视频动画",
                        Type = "video",
                        FileName = "campus-aerial.mp4",
                        Preview = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=campus%20aerial%20view%20of%20university&image_size=landscape_16_9",
                        Content = "保定学院校园航拍视频，展示了校园的美丽景色和建筑风貌。",
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
                        Content = "基于Vue.js和Node.js开发的校园二手交易平台源码，包含前端和后端完整代码。",
                        SubmitTime = DateTime.Now.AddDays(-2),
                        RiskLevel = "medium",
                        RiskDetails = "包含可执行文件|可能存在安全漏洞",
                        Status = "pending"
                    },
                    new ModerationItem
                    {
                        Title = "2026届毕业展海报设计",
                        Author = "王同学",
                        Category = "视觉设计",
                        Type = "image",
                        FileName = "graduation-poster.jpg",
                        Preview = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=university%20graduation%20exhibition%20poster%20design&image_size=square",
                        Content = "2026届毕业展海报设计，包含毕业主题和校园元素。",
                        SubmitTime = DateTime.Now.AddDays(-3),
                        RiskLevel = "low",
                        RiskDetails = "",
                        Status = "pending"
                    },
                    new ModerationItem
                    {
                        Title = "基于大数据的学情分析",
                        Author = "赵同学",
                        Category = "文档论述",
                        Type = "document",
                        FileName = "learning-analytics.pdf",
                        Preview = "",
                        Content = "基于大数据技术对学生学习情况进行分析，提出了改进教学的建议。",
                        SubmitTime = DateTime.Now.AddDays(-3),
                        RiskLevel = "high",
                        RiskDetails = "包含学生个人信息|数据处理不符合隐私要求",
                        Status = "pending"
                    },
                    new ModerationItem
                    {
                        Title = "迎新晚会混音剪辑",
                        Author = "刘同学",
                        Category = "视频动画",
                        Type = "video",
                        FileName = "welcome-party-mix.mp4",
                        Preview = "https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=university%20welcome%20party%20stage&image_size=landscape_16_9",
                        Content = "2026级迎新晚会的混音剪辑视频，记录了精彩瞬间。",
                        SubmitTime = DateTime.Now.AddDays(-4),
                        RiskLevel = "low",
                        RiskDetails = "",
                        Status = "pending"
                    }
                };

                // 插入数据
                await context.ModerationItems.AddRangeAsync(testItems);
                await context.SaveChangesAsync();
                Console.WriteLine("内容审核测试数据创建完成，共插入5条审核项目");
            }
            else
            {
                Console.WriteLine("内容审核测试数据已存在，跳过创建");
            }

            // 检查作品数据是否存在
            var worksDataExists = await context.Works.AnyAsync();
            if (!worksDataExists)
            {
                // 获取用户ID（使用前面已定义的变量）
                var adminId = await context.Users.Where(u => u.Username == "admin").Select(u => u.Id).FirstOrDefaultAsync();

                // 创建测试作品
                var testWorks = new List<Work>
                {
                    // 管理员的作品（教师和学生不应该看到）
                    new Work
                    {
                        Title = "系统管理后台示例",
                        Category = "后端开发",
                        Description = "系统管理后台的示例代码和文档，用于系统维护和用户管理。",
                        UploadDate = DateTime.Now.AddDays(-7),
                        Status = "草稿",
                        FilePath = "admin_system_backend.zip",
                        FileName = "系统管理后台示例.zip",
                        FileSize = 20971520,
                        FileMD5 = "admin1234567890abcdef",
                        FileUploadTime = DateTime.Now.AddDays(-7),
                        UserId = adminId,
                        Views = 0,
                        Favorites = 0
                    },
                    new Work
                    {
                        Title = "数据库设计文档",
                        Category = "文档报告",
                        Description = "系统数据库的设计文档和ER图，包含所有表结构和关系说明。",
                        UploadDate = DateTime.Now.AddDays(-5),
                        Status = "草稿",
                        FilePath = "admin_database_design.pdf",
                        FileName = "数据库设计文档.pdf",
                        FileSize = 5242880,
                        FileMD5 = "admin9876543210fedcba",
                        FileUploadTime = DateTime.Now.AddDays(-5),
                        UserId = adminId,
                        Views = 0,
                        Favorites = 0
                    },
                    new Work
                    {
                        Title = "API接口文档",
                        Category = "文档报告",
                        Description = "系统所有API接口的详细文档，包含请求参数和返回格式。",
                        UploadDate = DateTime.Now.AddDays(-3),
                        Status = "草稿",
                        FilePath = "admin_api_docs.docx",
                        FileName = "API接口文档.docx",
                        FileSize = 3145728,
                        FileMD5 = "adminabcdef123456789",
                        FileUploadTime = DateTime.Now.AddDays(-3),
                        UserId = adminId,
                        Views = 0,
                        Favorites = 0
                    },

                    // 张老师的作品
                    new Work
                    {
                        Title = "数字媒体课程教学大纲",
                        Category = "文档报告",
                        Description = "数字媒体技术专业的课程教学大纲，包含教学目标和课程安排。",
                        UploadDate = DateTime.Now.AddDays(-10),
                        Status = "已发布",
                        FilePath = "teacher_zhang_syllabus.pdf",
                        FileName = "数字媒体课程教学大纲.pdf",
                        FileSize = 1048576,
                        FileMD5 = "zhang1234567890",
                        FileUploadTime = DateTime.Now.AddDays(-10),
                        UserId = tZhangId,
                        Views = 120,
                        Favorites = 15,
                        IsExcellent = true
                    },
                    new Work
                    {
                        Title = "学生作品评价标准",
                        Category = "文档报告",
                        Description = "数字媒体专业学生作品的评价标准和评分细则。",
                        UploadDate = DateTime.Now.AddDays(-8),
                        Status = "已发布",
                        FilePath = "teacher_zhang_evaluation.docx",
                        FileName = "学生作品评价标准.docx",
                        FileSize = 524288,
                        FileMD5 = "zhang9876543210",
                        FileUploadTime = DateTime.Now.AddDays(-8),
                        UserId = tZhangId,
                        Views = 85,
                        Favorites = 12
                    },

                    // 李老师的作品
                    new Work
                    {
                        Title = "人工智能导论课件",
                        Category = "文档报告",
                        Description = "人工智能导论课程的完整课件，包含PPT和教学视频。",
                        UploadDate = DateTime.Now.AddDays(-12),
                        Status = "已发布",
                        FilePath = "teacher_li_ai_course.zip",
                        FileName = "人工智能导论课件.zip",
                        FileSize = 52428800,
                        FileMD5 = "li1234567890",
                        FileUploadTime = DateTime.Now.AddDays(-12),
                        UserId = tLiId,
                        Views = 200,
                        Favorites = 28,
                        IsExcellent = true
                    },
                    new Work
                    {
                        Title = "机器学习实验指导书",
                        Category = "文档报告",
                        Description = "机器学习课程的实验指导书，包含实验步骤和代码示例。",
                        UploadDate = DateTime.Now.AddDays(-9),
                        Status = "已发布",
                        FilePath = "teacher_li_ml_lab.pdf",
                        FileName = "机器学习实验指导书.pdf",
                        FileSize = 2097152,
                        FileMD5 = "li9876543210",
                        FileUploadTime = DateTime.Now.AddDays(-9),
                        UserId = tLiId,
                        Views = 150,
                        Favorites = 20
                    },

                    // 王晨的作品
                    new Work
                    {
                        Title = "校园导览小程序",
                        Category = "前端开发",
                        Description = "用于展示校园建筑与导览路线的小程序作品，支持地图导航和景点介绍。",
                        UploadDate = DateTime.Now.AddDays(-8),
                        Status = "已发布",
                        FilePath = "student_wang_campus_guide.zip",
                        FileName = "校园导览小程序.zip",
                        FileSize = 14680064,
                        FileMD5 = "wang1234567890",
                        FileUploadTime = DateTime.Now.AddDays(-8),
                        UserId = sWangId,
                        Views = 60,
                        Favorites = 8,
                        IsExcellent = true
                    },
                    new Work
                    {
                        Title = "毕业作品管理后台",
                        Category = "后端开发",
                        Description = "面向毕业展的作品管理后台，支持作品上传、审核和展示功能。",
                        UploadDate = DateTime.Now.AddDays(-5),
                        Status = "已发布",
                        FilePath = "student_wang_work_backend.zip",
                        FileName = "毕业作品管理后台.zip",
                        FileSize = 11534336,
                        FileMD5 = "wang9876543210",
                        FileUploadTime = DateTime.Now.AddDays(-5),
                        UserId = sWangId,
                        Views = 45,
                        Favorites = 5
                    },
                    new Work
                    {
                        Title = "个人作品集网站",
                        Category = "前端开发",
                        Description = "个人作品集展示网站，包含作品展示、个人介绍和联系方式。",
                        UploadDate = DateTime.Now.AddDays(-3),
                        Status = "已发布",
                        FilePath = "student_wang_portfolio.zip",
                        FileName = "个人作品集网站.zip",
                        FileSize = 8388608,
                        FileMD5 = "wangabcdef123456",
                        FileUploadTime = DateTime.Now.AddDays(-3),
                        UserId = sWangId,
                        Views = 0,
                        Favorites = 0
                    },

                    // 刘悦的作品
                    new Work
                    {
                        Title = "交互海报设计",
                        Category = "设计",
                        Description = "用于毕业展宣传的交互海报与视觉方案，包含动态效果和交互设计。",
                        UploadDate = DateTime.Now.AddDays(-7),
                        Status = "已发布",
                        FilePath = "student_liu_poster.zip",
                        FileName = "交互海报设计.zip",
                        FileSize = 13631488,
                        FileMD5 = "liu1234567890",
                        FileUploadTime = DateTime.Now.AddDays(-7),
                        UserId = sLiuId,
                        Views = 55,
                        Favorites = 7,
                        IsExcellent = true
                    },
                    new Work
                    {
                        Title = "课堂作业提交系统",
                        Category = "前端开发",
                        Description = "支持阶段性提交与状态跟踪的课堂作业系统，包含作业管理和评分功能。",
                        UploadDate = DateTime.Now.AddDays(-3),
                        Status = "已发布",
                        FilePath = "student_liu_homework.zip",
                        FileName = "课堂作业提交系统.zip",
                        FileSize = 9437184,
                        FileMD5 = "liu9876543210",
                        FileUploadTime = DateTime.Now.AddDays(-3),
                        UserId = sLiuId,
                        Views = 35,
                        Favorites = 3
                    },

                    // 陈思的作品
                    new Work
                    {
                        Title = "智能答疑助手",
                        Category = "人工智能",
                        Description = "面向课程学习场景的问答助手原型，支持自然语言问答和知识库检索。",
                        UploadDate = DateTime.Now.AddDays(-6),
                        Status = "已发布",
                        FilePath = "student_chen_qa_assistant.zip",
                        FileName = "智能答疑助手.zip",
                        FileSize = 12582912,
                        FileMD5 = "chen1234567890",
                        FileUploadTime = DateTime.Now.AddDays(-6),
                        UserId = sChenId,
                        Views = 50,
                        Favorites = 6,
                        IsExcellent = true
                    },
                    new Work
                    {
                        Title = "作品标签推荐模型",
                        Category = "人工智能",
                        Description = "为学生作品自动打标签的推荐模型，基于机器学习算法实现。",
                        UploadDate = DateTime.Now.AddDays(-2),
                        Status = "已发布",
                        FilePath = "student_chen_tag_model.zip",
                        FileName = "作品标签推荐模型.zip",
                        FileSize = 8388608,
                        FileMD5 = "chen9876543210",
                        FileUploadTime = DateTime.Now.AddDays(-2),
                        UserId = sChenId,
                        Views = 30,
                        Favorites = 2
                    },

                    // 赵宁的作品
                    new Work
                    {
                        Title = "校园文化短片",
                        Category = "视频动画",
                        Description = "用于展示校园文化的短视频作品，记录校园生活和活动场景。",
                        UploadDate = DateTime.Now.AddDays(-4),
                        Status = "已发布",
                        FilePath = "student_zhao_culture_video.zip",
                        FileName = "校园文化短片.zip",
                        FileSize = 10485760,
                        FileMD5 = "zhao1234567890",
                        FileUploadTime = DateTime.Now.AddDays(-4),
                        UserId = sZhaoId,
                        Views = 40,
                        Favorites = 4
                    },
                    new Work
                    {
                        Title = "三维展厅原型",
                        Category = "设计",
                        Description = "线上毕业展三维展厅交互原型，支持虚拟漫游和作品展示。",
                        UploadDate = DateTime.Now.AddDays(-1),
                        Status = "已发布",
                        FilePath = "student_zhao_3d_gallery.zip",
                        FileName = "三维展厅原型.zip",
                        FileSize = 7340032,
                        FileMD5 = "zhao9876543210",
                        FileUploadTime = DateTime.Now.AddDays(-1),
                        UserId = sZhaoId,
                        Views = 25,
                        Favorites = 1
                    }
                };

                // 插入作品数据
                await context.Works.AddRangeAsync(testWorks);
                await context.SaveChangesAsync();
                Console.WriteLine("作品测试数据创建完成，共插入" + testWorks.Count + "条作品");
            }
            else
            {
                Console.WriteLine("作品测试数据已存在，跳过创建");
            }

            Console.WriteLine("数据库初始化完成！");
        }
    }
}