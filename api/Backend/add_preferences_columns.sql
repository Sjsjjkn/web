-- ====================================================
-- 用户偏好设置字段迁移脚本
-- 在 Users 表中添加偏好设置相关列
-- ====================================================

-- 添加字段（如果已存在则忽略）
ALTER TABLE Users ADD Theme VARCHAR(20) DEFAULT 'light' COMMENT '界面主题：light / dark';
ALTER TABLE Users ADD Language VARCHAR(20) DEFAULT 'zh-CN' COMMENT '界面语言：zh-CN / en';
ALTER TABLE Users ADD NotificationEnabled BOOLEAN DEFAULT TRUE COMMENT '是否开启消息通知';
ALTER TABLE Users ADD EmailNotification BOOLEAN DEFAULT TRUE COMMENT '是否开启邮件通知';
ALTER TABLE Users ADD WorkModerationRequired BOOLEAN DEFAULT FALSE COMMENT '是否需要作品审核';
ALTER TABLE Users ADD ProfilePublic BOOLEAN DEFAULT TRUE COMMENT '个人主页是否公开';
ALTER TABLE Users ADD ShowContactInfo BOOLEAN DEFAULT FALSE COMMENT '是否展示联系方式';
ALTER TABLE Users ADD FavoritesVisibility VARCHAR(20) DEFAULT 'public' COMMENT '收藏可见性：public / followers / private';

-- 也可用单独一条一条执行（MySQL 示例，适应不同数据库）
-- MySQL:
/*
ALTER TABLE `Users` ADD COLUMN `Theme` VARCHAR(20) NOT NULL DEFAULT 'light';
ALTER TABLE `Users` ADD COLUMN `Language` VARCHAR(20) NOT NULL DEFAULT 'zh-CN';
ALTER TABLE `Users` ADD COLUMN `NotificationEnabled` TINYINT(1) NOT NULL DEFAULT 1;
ALTER TABLE `Users` ADD COLUMN `EmailNotification` TINYINT(1) NOT NULL DEFAULT 1;
ALTER TABLE `Users` ADD COLUMN `WorkModerationRequired` TINYINT(1) NOT NULL DEFAULT 0;
ALTER TABLE `Users` ADD COLUMN `ProfilePublic` TINYINT(1) NOT NULL DEFAULT 1;
ALTER TABLE `Users` ADD COLUMN `ShowContactInfo` TINYINT(1) NOT NULL DEFAULT 0;
ALTER TABLE `Users` ADD COLUMN `FavoritesVisibility` VARCHAR(20) NOT NULL DEFAULT 'public';
*/