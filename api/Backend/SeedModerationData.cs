using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Backend
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 配置数据库连接
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql(
                "server=localhost;port=3306;user=root;password=123456;database=web_db",
                new MySqlServerVersion(new Version(8, 0, 36))
            );

            using (var context = new AppDbContext(optionsBuilder.Options))
            {
                // 检查是否已有数据
                var existingCount = await context.ModerationItems.CountAsync();
                if (existingCount > 0)
                {
                    Console.WriteLine("测试数据已存在，跳过创建");
                    return;
                }

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

                Console.WriteLine("测试数据创建完成，共插入5条审核项目");
            }
        }
    }
}