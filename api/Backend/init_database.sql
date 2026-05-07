-- =============================================
-- 数据库初始化脚本 (完整版)
-- 包含建表 + 种子数据
-- 使用方法: mysql -u root -p < init_database.sql
-- 所有用户密码均为: 123456 (明文存储, PasswordHasher.Verify 兼容明文)
-- =============================================

DROP TABLE IF EXISTS `WorkFavorites`;
DROP TABLE IF EXISTS `WorkStudents`;
DROP TABLE IF EXISTS `StudentTeachers`;
DROP TABLE IF EXISTS `ModerationItems`;
DROP TABLE IF EXISTS `Works`;
DROP TABLE IF EXISTS `Users`;
DROP TABLE IF EXISTS `__EFMigrationsHistory`;

CREATE TABLE `Users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) NOT NULL,
  `Password` text NOT NULL,
  `Role` varchar(20) NOT NULL DEFAULT 'Student',
  `Name` varchar(50) NOT NULL DEFAULT '',
  `WorkId` varchar(20) NOT NULL DEFAULT '',
  `Department` varchar(100) NOT NULL DEFAULT '',
  `Phone` varchar(20) NOT NULL DEFAULT '',
  `Email` varchar(100) NOT NULL DEFAULT '',
  `Bio` varchar(500) NOT NULL DEFAULT '',
  `Title` varchar(50) NOT NULL DEFAULT '',
  `ResearchArea` varchar(200) NOT NULL DEFAULT '',
  `Position` varchar(50) NOT NULL DEFAULT '',
  `Avatar` varchar(500) NULL,
  `Theme` varchar(20) NOT NULL DEFAULT 'light',
  `Language` varchar(20) NOT NULL DEFAULT 'zh-CN',
  `NotificationEnabled` tinyint(1) NOT NULL DEFAULT 1,
  `EmailNotification` tinyint(1) NOT NULL DEFAULT 1,
  `WorkModerationRequired` tinyint(1) NOT NULL DEFAULT 0,
  `ProfilePublic` tinyint(1) NOT NULL DEFAULT 1,
  `ShowContactInfo` tinyint(1) NOT NULL DEFAULT 0,
  `FavoritesVisibility` varchar(20) NOT NULL DEFAULT 'public',
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE INDEX `IX_Users_Username` (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Works` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Title` varchar(100) NOT NULL,
  `Category` varchar(50) NOT NULL,
  `Description` varchar(1000) NULL,
  `UploadDate` datetime(6) NOT NULL,
  `Status` varchar(20) NOT NULL DEFAULT '草稿',
  `FilePath` varchar(500) NULL,
  `FileName` varchar(255) NULL,
  `FileSize` bigint NOT NULL DEFAULT 0,
  `FileMD5` varchar(32) NULL,
  `FileUploadTime` datetime(6) NULL,
  `PreviewImage` varchar(500) NULL,
  `UserId` int NOT NULL,
  `IsExcellent` tinyint(1) NOT NULL DEFAULT 0,
  `Views` int NOT NULL DEFAULT 0,
  `Favorites` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  INDEX `IX_Works_UserId` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `StudentTeachers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `StudentId` int NOT NULL,
  `TeacherId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_StudentTeachers_StudentId` (`StudentId`),
  INDEX `IX_StudentTeachers_TeacherId` (`TeacherId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `WorkStudents` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `WorkId` int NOT NULL,
  `StudentId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_WorkStudents_WorkId` (`WorkId`),
  INDEX `IX_WorkStudents_StudentId` (`StudentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `WorkFavorites` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `WorkId` int NOT NULL,
  `UserId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `IX_WorkFavorites_WorkId` (`WorkId`),
  INDEX `IX_WorkFavorites_UserId` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `ModerationItems` (
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
  `UserId` int NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =============================================
-- 种子数据 - 所有用户密码: 123456
-- =============================================

INSERT INTO `Users` (`Username`, `Password`, `Role`, `Name`, `WorkId`, `Department`, `Email`, `Title`, `ResearchArea`, `CreatedAt`) VALUES
('admin', '123456', 'Admin', '系统管理员', 'A0001', '信息中心', 'admin@bdu.local', '', '', NOW()),
('teacher_zhang', '123456', 'Teacher', '张老师', 'T2026001', '数字媒体教研室', 'teacher_zhang@bdu.local', '讲师', '数字作品指导', NOW()),
('teacher_li', '123456', 'Teacher', '李老师', 'T2026002', '人工智能教研室', 'teacher_li@bdu.local', '副教授', '智能创作', NOW()),
('student_wang', '123456', 'Student', '王晨', '20261001', '数字媒体技术', 'student_wang@bdu.local', '', '', NOW()),
('student_liu', '123456', 'Student', '刘悦', '20261002', '数字媒体技术', 'student_liu@bdu.local', '', '', NOW()),
('student_chen', '123456', 'Student', '陈思', '20261003', '人工智能', 'student_chen@bdu.local', '', '', NOW()),
('student_zhao', '123456', 'Student', '赵宁', '20261004', '人工智能', 'student_zhao@bdu.local', '', '', NOW());

INSERT INTO `StudentTeachers` (`StudentId`, `TeacherId`, `CreatedAt`) VALUES
(4, 2, NOW()),
(5, 2, NOW()),
(6, 3, NOW()),
(7, 3, NOW());

INSERT INTO `Works` (`Title`, `Category`, `Description`, `UploadDate`, `Status`, `FilePath`, `FileName`, `FileSize`, `FileMD5`, `FileUploadTime`, `UserId`, `IsExcellent`, `Views`, `Favorites`) VALUES
('校园导览小程序', '前端开发', '用于展示校园建筑与导览路线的小程序作品。', DATE_SUB(NOW(), INTERVAL 8 DAY), '已发布', 'student_wang_校园导览小程序.zip', '校园导览小程序.zip', 14680064, 'a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6', DATE_SUB(NOW(), INTERVAL 8 DAY), 4, 1, 60, 8),
('毕业作品管理后台', '后端开发', '面向毕业展的作品管理后台。', DATE_SUB(NOW(), INTERVAL 5 DAY), '待审核', 'student_wang_毕业作品管理后台.zip', '毕业作品管理后台.zip', 11534336, 'b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7', DATE_SUB(NOW(), INTERVAL 5 DAY), 4, 0, 45, 5),
('交互海报设计', '设计', '用于毕业展宣传的交互海报与视觉方案。', DATE_SUB(NOW(), INTERVAL 7 DAY), '已发布', 'student_liu_交互海报设计.zip', '交互海报设计.zip', 13631488, 'c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8', DATE_SUB(NOW(), INTERVAL 7 DAY), 5, 1, 55, 7),
('课堂作业提交系统', '前端开发', '支持阶段性提交与状态跟踪的课堂作业系统。', DATE_SUB(NOW(), INTERVAL 3 DAY), '草稿', 'student_liu_课堂作业提交系统.zip', '课堂作业提交系统.zip', 9437184, 'd4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9', DATE_SUB(NOW(), INTERVAL 3 DAY), 5, 0, 35, 3),
('智能答疑助手', '人工智能', '面向课程学习场景的问答助手原型。', DATE_SUB(NOW(), INTERVAL 6 DAY), '已发布', 'student_chen_智能答疑助手.zip', '智能答疑助手.zip', 12582912, 'e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0', DATE_SUB(NOW(), INTERVAL 6 DAY), 6, 1, 50, 6),
('作品标签推荐模型', '人工智能', '为学生作品自动打标签的推荐模型。', DATE_SUB(NOW(), INTERVAL 2 DAY), '待审核', 'student_chen_作品标签推荐模型.zip', '作品标签推荐模型.zip', 8388608, 'f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1', DATE_SUB(NOW(), INTERVAL 2 DAY), 6, 0, 30, 2),
('校园文化短片', '视频动画', '用于展示校园文化的短视频作品。', DATE_SUB(NOW(), INTERVAL 4 DAY), '已发布', 'student_zhao_校园文化短片.zip', '校园文化短片.zip', 10485760, 'a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2', DATE_SUB(NOW(), INTERVAL 4 DAY), 7, 0, 40, 4),
('三维展厅原型', '设计', '线上毕业展三维展厅交互原型。', DATE_SUB(NOW(), INTERVAL 1 DAY), '草稿', 'student_zhao_三维展厅原型.zip', '三维展厅原型.zip', 7340032, 'b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3', DATE_SUB(NOW(), INTERVAL 1 DAY), 7, 0, 25, 1);

INSERT INTO `WorkStudents` (`WorkId`, `StudentId`, `CreatedAt`) VALUES
(1, 4, DATE_SUB(NOW(), INTERVAL 8 DAY)),
(2, 4, DATE_SUB(NOW(), INTERVAL 5 DAY)),
(3, 5, DATE_SUB(NOW(), INTERVAL 7 DAY)),
(4, 5, DATE_SUB(NOW(), INTERVAL 3 DAY)),
(5, 6, DATE_SUB(NOW(), INTERVAL 6 DAY)),
(6, 6, DATE_SUB(NOW(), INTERVAL 2 DAY)),
(7, 7, DATE_SUB(NOW(), INTERVAL 4 DAY)),
(8, 7, DATE_SUB(NOW(), INTERVAL 1 DAY));

INSERT INTO `ModerationItems` (`Title`, `Author`, `Category`, `Type`, `FileName`, `Preview`, `Content`, `SubmitTime`, `RiskLevel`, `RiskDetails`, `Status`, `UserId`) VALUES
('保定学院航拍微电影', '张同学', '视频动画', 'video', 'campus-aerial.mp4', '', '展示校园景观与建筑风貌的短片作品。', DATE_SUB(NOW(), INTERVAL 2 DAY), 'low', '', 'pending', NULL),
('校园二手交易平台', '李同学', '软件源码', 'document', 'second-hand-platform.zip', '', '基于 Vue 与 Node 的校园二手交易平台。', DATE_SUB(NOW(), INTERVAL 2 DAY), 'medium', '包含可执行文件，需要人工确认安全性', 'pending', NULL),
('学情分析报告', '赵同学', '文档报告', 'document', 'learning-analytics.pdf', '', '围绕学生学习数据的分析报告。', DATE_SUB(NOW(), INTERVAL 3 DAY), 'high', '包含学生个人信息，需要脱敏处理', 'pending', NULL);