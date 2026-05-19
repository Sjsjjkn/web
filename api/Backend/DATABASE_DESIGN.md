# 数据库详细设计文档

## 数字作品管理与教学协作平台

---

## 一、数据库概述

| 项目 | 说明 |
|------|------|
| **系统名称** | 数字作品管理与教学协作平台（毕设项目） |
| **数据库类型** | MySQL（关系型数据库） |
| **ORM 框架** | Entity Framework Core |
| **MySQL 驱动** | Pomelo.EntityFrameworkCore.MySql |
| **编码字符集** | utf8mb4 |
| **核心表数量** | 11 张 |
| **设计范式** | 第三范式（3NF），局部合理冗余 |

---

## 二、实体关系图 (ER Diagram)

```
                            ┌──────────────────┐
                            │  ModerationItem  │
                            │  ─────────────── │
                            │  Id (PK)         │
                            │  UserId (FK) ────┼──────────┐
                            │  Title           │          │
                            │  Status          │          │
                            │  RiskLevel       │          │
                            └──────────────────┘          │
                                                          │
┌──────────────────┐       ┌──────────────────┐          │
│   Announcement   │       │     Feedback     │          │
│  ─────────────── │       │  ─────────────── │          │
│  Id (PK)         │       │  Id (PK)         │          │
│  PublisherId(FK)─┼───┐   │  UserId (FK) ────┼──────────┤
│  Title           │   │   │  Type            │          │
│  Content         │   │   │  Status          │          │
│  IsPinned        │   │   │  Reply           │          │
└──────────────────┘   │   └──────────────────┘          │
                        │                                  │
                        │   ┌──────────────────────────────────────┐
                        │   │               User                   │
                        │   │  ─────────────────────────────────── │
                        │   │  Id (PK)                            │
                        ├───┤  Username, Password, Role            │
                        │   │  Name, WorkId, Department            │
                        │   │  Phone, Email, Bio                   │
                        │   │  Title, ResearchArea (教师)          │
                        │   │  Avatar, Theme, Language             │
                        │   │  NotificationEnabled                 │
                        │   │  FavoritesVisibility                 │
                        │   └──────┬──────┬───────┬───────┬───────┘
                        │          │      │       │       │
                        │    ┌─────┘      │       │       └──────────────┐
                        │    │            │       │                      │
                        │    │  ┌─────────┘       └─────────┐            │
                        │    │  │                           │            │
                        │    │  │  1:n              n:m     │    n:m     │
                        │    │  │                           │            │
                        ▼    ▼  ▼                           ▼            ▼
              ┌──────────┐ ┌──────────────┐ ┌──────────────┐ ┌─────────────────┐
              │   Work   │ │ WorkFavorite │ │ ViewHistory  │ │  StudentTeacher │
              │──────────│ │──────────────│ │──────────────│ │─────────────────│
              │ Id (PK)  │ │ Id (PK)      │ │ Id (PK)      │ │ Id (PK)         │
              │ UserId ──┼►│ UserId (FK)  │ │ UserId (FK)  │ │ StudentId (FK)  │
              │ Title    │ │ WorkId (FK) ─┼►│ WorkId (FK) ─┼►│ TeacherId (FK)  │
              │ Category │ │ FavoriteDate │ │ ViewedAt     │ │ CreatedAt       │
              │ Status   │ └──────────────┘ └──────────────┘ └─────────────────┘
              │ FilePath │
              │ Views    │ ┌──────────────┐ ┌──────────────────┐
              │ Favorites│ │ WorkStudent  │ │   WorkReview     │
              │ Preview  │ │──────────────│ │──────────────────│
              └──────────┘ │ Id (PK)      │ │ Id (PK)          │
                     ▲     │ WorkId (FK)──┼►│ WorkId (FK) ─────┼──┐
                     │     │ StudentId(FK)│ │ ReviewerId (FK) ─┼──┤(User)
                     │     └──────────────┘ │ Comment          │  │
                     │                      │ Type             │  │
                     └──────────────────────│ CreatedAt        │  │
                                            └──────────────────┘  │
                                                                   │
              ┌──────────────────┐                                 │
              │  Notification    │                                 │
              │──────────────────│                                 │
              │ Id (PK)          │                                 │
              │ UserId (FK) ─────┼─────────────────────────────────┘
              │ WorkId (FK) ─────┤
              │ Type, Title      │
              │ Content, IsRead  │
              │ CreatedAt        │
              └──────────────────┘
```

### 关系类型汇总

| 源表 | 目标表 | 关系类型 | 中间表/外键 | 说明 |
|------|--------|----------|-------------|------|
| User | Work | **1 : n** | Work.UserId | 一个用户上传多个作品 |
| User | Work | **n : m** | WorkFavorite | 用户收藏作品 |
| User | Work | **n : m** | ViewHistory | 用户浏览作品 |
| User | Work | **n : m** | WorkStudent | 学生参与作品 |
| User | User | **n : m** | StudentTeacher | 师生指导关系 |
| User | Work | **1 : n** | WorkReview | 用户（教师/学生）对作品评语 |
| User | Notification | **1 : n** | Notification.UserId | 用户接收通知 |
| User | Announcement | **1 : n** | Announcement.PublisherId | 用户发布公告 |
| User | Feedback | **1 : n** | Feedback.UserId | 用户提交反馈 |
| User | ModerationItem | **1 : n** | ModerationItem.UserId | 用户提交审核项（可选） |
| Work | Notification | **1 : n** | Notification.WorkId | 作品关联通知（可选） |
| Work | WorkReview | **1 : n** | WorkReview.WorkId | 作品被评语 |
| Work | ModerationItem | **n : 1** | — | 审核项关联作品（逻辑关联） |

---

## 三、逐表详细设计

> **表头说明**：在以下字段定义表格中：
> - **数据类型**：数据库列类型（INT、VARCHAR、DATETIME 等）
> - **最大长度**：仅适用于 VARCHAR/字符串类型，表示该字段允许的最大字符数（对应 C# 模型中 `[StringLength(n)]` 注解）。标 `—` 表示该类型（如 INT、DATETIME、TINYINT）无字符长度概念
> - **允许空**：YES 表示可为 NULL，NO 表示不可为空
> - **默认值**：插入新记录时若未提供该字段值，数据库自动填充的默认值
> - **约束**：该字段上的数据约束（PRIMARY KEY、FOREIGN KEY、REQUIRED、UNIQUE 等）

---

### 3.1 Users（用户表）

**业务说明**：存储系统所有用户的基础信息。通过 `Role` 字段区分三种角色：`Admin`（管理员）、`Teacher`（教师）、`Student`（学生）。教师用户额外有职称和研究领域字段。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 用户唯一标识 |
| 2 | **Username** | VARCHAR | 50 | NO | — | **REQUIRED, UNIQUE** | 登录用户名，用于认证 |
| 3 | **Password** | VARCHAR | 100 | NO | — | **REQUIRED** | 密码（加密存储，如 BCrypt/SHA256） |
| 4 | **CreatedAt** | DATETIME | — | NO | — | — | 账号创建时间 |
| 5 | **Remember** | TINYINT(1) | — | NO | 0 | — | 是否记住登录状态 |
| 6 | **Role** | VARCHAR | 20 | NO | 'Student' | **REQUIRED** | 角色：`Student` / `Teacher` / `Admin` |
| 7 | **Name** | VARCHAR | 50 | YES | '' | — | 用户真实姓名 |
| 8 | **WorkId** | VARCHAR | 20 | YES | '' | — | 学号（学生）/ 工号（教师） |
| 9 | **Department** | VARCHAR | 100 | YES | '' | — | 院系 / 部门名称 |
| 10 | **Phone** | VARCHAR | 20 | YES | '' | — | 联系电话 |
| 11 | **Email** | VARCHAR | 100 | YES | '' | — | 电子邮箱 |
| 12 | **Bio** | VARCHAR | 500 | YES | '' | — | 个人简介 / 自我介绍 |
| 13 | **Title** | VARCHAR | 50 | YES | '' | — | 职称（教师使用，如"教授"、"副教授"） |
| 14 | **ResearchArea** | VARCHAR | 200 | YES | '' | — | 研究领域（教师使用） |
| 15 | **Position** | VARCHAR | 50 | YES | '' | — | 职务 / 职位 |
| 16 | **Avatar** | VARCHAR | 500 | YES | NULL | — | 头像文件路径 |
| 17 | **Theme** | VARCHAR | 20 | NO | 'light' | — | 界面主题偏好：`light` / `dark` |
| 18 | **Language** | VARCHAR | 20 | NO | 'zh-CN' | — | 界面语言：`zh-CN` / `en` |
| 19 | **NotificationEnabled** | TINYINT(1) | — | NO | 1 | — | 是否开启站内消息通知 |
| 20 | **EmailNotification** | TINYINT(1) | — | NO | 1 | — | 是否开启邮件通知 |
| 21 | **WorkModerationRequired** | TINYINT(1) | — | NO | 0 | — | 上传作品是否需要管理员审核 |
| 22 | **ProfilePublic** | TINYINT(1) | — | NO | 1 | — | 个人主页是否公开可见 |
| 23 | **ShowContactInfo** | TINYINT(1) | — | NO | 0 | — | 是否公开显示联系方式 |
| 24 | **FavoritesVisibility** | VARCHAR | 20 | NO | 'public' | — | 收藏列表可见性：`public` / `followers` / `private` |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_Users | PRIMARY KEY | Id | 主键索引 |
| IX_Users_Username | UNIQUE INDEX | Username | 用户名唯一，用于登录查找 |
| IX_Users_Role | INDEX | Role | 按角色查询用户列表 |
| IX_Users_Department | INDEX | Department | 按院系筛选用户 |

**导航属性一览：**

| 导航属性 | 关联实体 | 关系基数 | 说明 |
|----------|----------|----------|------|
| `UploadedWorks` | Work | 1 : n | 该用户上传的所有作品 |
| `WorkFavorites` | WorkFavorite | 1 : n | 该用户的所有收藏记录 |
| `ViewHistories` | ViewHistory | 1 : n | 该用户的浏览历史 |
| `WorkStudents` | WorkStudent | 1 : n | 该学生参与的作品记录 |
| `SupervisedStudents` | StudentTeacher | 1 : n | 该教师指导的学生（TeacherId） |
| `Teachers` | StudentTeacher | 1 : n | 该学生的指导教师（StudentId） |

---

### 3.2 Works（作品表）

**业务说明**：存储用户上传的数字作品/项目文件的核心信息。包含文件元数据、状态流转（草稿→待审核→已发布/审核拒绝）以及浏览量、收藏数的冗余统计。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 作品唯一标识 |
| 2 | **Title** | VARCHAR | 100 | NO | — | **REQUIRED** | 作品标题 |
| 3 | **Category** | VARCHAR | 50 | NO | — | **REQUIRED** | 作品分类（如"三维模型"、"平面设计"、"动画"等） |
| 4 | **Description** | VARCHAR | 1000 | YES | NULL | — | 作品描述 / 简介 |
| 5 | **UploadDate** | DATETIME | — | NO | — | — | 上传日期时间 |
| 6 | **Status** | VARCHAR | 20 | NO | '草稿' | **REQUIRED** | 作品状态：`草稿` / `待审核` / `已发布` / `审核拒绝` |
| 7 | **FilePath** | VARCHAR | 500 | YES | NULL | — | 文件存储路径（服务器相对路径） |
| 8 | **FileName** | VARCHAR | 255 | YES | NULL | — | 原始文件名（上传时的文件名） |
| 9 | **FileSize** | BIGINT | — | NO | 0 | — | 文件大小（单位：字节） |
| 10 | **FileMD5** | VARCHAR | 32 | YES | NULL | — | 文件 MD5 校验值（用于去重和完整性验证） |
| 11 | **FileUploadTime** | DATETIME | — | YES | NULL | — | 文件上传完成时间 |
| 12 | **PreviewImage** | VARCHAR | 500 | YES | NULL | — | 预览图存储路径 |
| 13 | **UserId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 上传者 ID（作品所有者） |
| 14 | **IsExcellent** | TINYINT(1) | — | NO | 0 | — | 是否为优秀作品（管理员/教师标记） |
| 15 | **Views** | INT | — | NO | 0 | — | 浏览次数（冗余计数，可定期从 ViewHistory 重算） |
| 16 | **Favorites** | INT | — | NO | 0 | — | 收藏次数（冗余计数，与 WorkFavorite 表同步维护） |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_Works | PRIMARY KEY | Id | 主键索引 |
| IX_Works_UserId | INDEX | UserId | 按上传者查询作品 |
| IX_Works_Status | INDEX | Status | 按状态筛选（草稿/待审核/已发布） |
| IX_Works_Category | INDEX | Category | 按分类浏览作品 |
| IX_Works_UploadDate | INDEX | UploadDate | 按上传时间排序 |
| IX_Works_Views | INDEX | Views | 按热度排序 |
| IX_Works_IsExcellent | INDEX | IsExcellent | 筛选优秀作品 |

**外键与级联策略：**

| 外键字段 | 引用表 | 引用字段 | 级联删除 | 级联更新 | 说明 |
|----------|--------|----------|----------|----------|------|
| UserId | Users | Id | **RESTRICT** | CASCADE | 有作品时禁止删除用户 |

**状态流转图：**

```
  ┌────────┐    提交审核    ┌──────────┐    审核通过    ┌────────┐
  │  草稿  │ ────────────→ │  待审核  │ ────────────→ │ 已发布 │
  └────────┘               └──────────┘               └────────┘
       ↑                        │                         │
       │        审核拒绝        │                         │
       └────────────────────────┘                         │
       │                                                  │
       └──────────────────── 重新编辑 ─────────────────────┘
                            (可退回草稿)
```

**导航属性一览：**

| 导航属性 | 关联实体 | 关系基数 | 说明 |
|----------|----------|----------|------|
| `Uploader` | User | n : 1 | 上传此作品的用户 |
| `WorkFavorites` | WorkFavorite | 1 : n | 哪些用户收藏了此作品 |
| `ViewHistories` | ViewHistory | 1 : n | 哪些用户浏览过此作品 |
| `WorkStudents` | WorkStudent | 1 : n | 哪些学生参与了此作品 |

---

### 3.3 WorkFavorites（作品收藏表）

**业务说明**：记录用户收藏作品的关联关系，是 User 与 Work 之间的多对多中间表。建立联合唯一索引防止重复收藏。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 收藏记录唯一标识 |
| 2 | **UserId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 收藏者 ID |
| 3 | **WorkId** | INT | — | NO | — | **FOREIGN KEY** → Works.Id | 被收藏的作品 ID |
| 4 | **FavoriteDate** | DATETIME | — | NO | NOW() | — | 收藏时间 |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_WorkFavorites | PRIMARY KEY | Id | 主键索引 |
| IX_WorkFavorites_UserId_WorkId | **UNIQUE INDEX** | UserId, WorkId | 联合唯一索引，防止用户重复收藏同一作品 |
| IX_WorkFavorites_WorkId | INDEX | WorkId | 按作品查询被哪些用户收藏 |

**外键与级联策略：**

| 外键字段 | 引用表 | 引用字段 | 级联删除 | 说明 |
|----------|--------|----------|----------|------|
| UserId | Users | Id | **CASCADE** | 删除用户时级联删除其收藏记录 |
| WorkId | Works | Id | **CASCADE** | 删除作品时级联删除对应收藏记录 |

**维护说明**：收藏/取消收藏操作需同步更新 `Works.Favorites` 冗余计数字段，通过事务保证一致性。

---

### 3.4 ViewHistories（浏览历史表）

**业务说明**：记录用户浏览作品的历史轨迹，支持分析用户兴趣偏好和作品热度趋势。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 浏览记录唯一标识 |
| 2 | **UserId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 浏览者 ID |
| 3 | **WorkId** | INT | — | NO | — | **FOREIGN KEY** → Works.Id | 被浏览的作品 ID |
| 4 | **ViewedAt** | DATETIME | — | NO | NOW() | — | 浏览时间 |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_ViewHistories | PRIMARY KEY | Id | 主键索引 |
| IX_ViewHistories_UserId_ViewedAt | INDEX | UserId, ViewedAt | 按用户+时间查询浏览历史 |
| IX_ViewHistories_WorkId_ViewedAt | INDEX | WorkId, ViewedAt | 按作品+时间统计浏览量趋势 |

**外键与级联策略：**

| 外键字段 | 引用表 | 引用字段 | 级联删除 | 说明 |
|----------|--------|----------|----------|------|
| UserId | Users | Id | **CASCADE** | 删除用户时级联删除其浏览历史 |
| WorkId | Works | Id | **CASCADE** | 删除作品时级联删除对应浏览记录 |

---

### 3.5 WorkStudents（作品参与表）

**业务说明**：记录多个学生参与同一作品的协作关系，是 User(Student) 与 Work 之间的多对多中间表。区别于收藏（喜欢）和上传（拥有），参与表示学生实际参与了作品的制作。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 参与记录唯一标识 |
| 2 | **WorkId** | INT | — | NO | — | **FOREIGN KEY** → Works.Id | 参与的作品 ID |
| 3 | **StudentId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 参与的学生 ID |
| 4 | **CreatedAt** | DATETIME | — | NO | NOW() | — | 添加参与关系的时间 |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_WorkStudents | PRIMARY KEY | Id | 主键索引 |
| IX_WorkStudents_WorkId_StudentId | **UNIQUE INDEX** | WorkId, StudentId | 联合唯一索引，防止同一学生重复参与同一作品 |

**外键与级联策略：**

| 外键字段 | 引用表 | 引用字段 | 级联删除 | 说明 |
|----------|--------|----------|----------|------|
| WorkId | Works | Id | **CASCADE** | 删除作品时级联删除参与记录 |
| StudentId | Users | Id | **RESTRICT** | 有关联参与记录时不允许删除学生用户 |

---

### 3.6 StudentTeachers（师生关系表）

**业务说明**：记录教师指导学生的一对多/多对多关系。一个教师可以指导多个学生，一个学生也可以有多个指导教师（如主导师+副导师）。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 师生关系唯一标识 |
| 2 | **StudentId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 学生 ID |
| 3 | **TeacherId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 教师 ID |
| 4 | **CreatedAt** | DATETIME | — | NO | NOW() | — | 建立指导关系的时间 |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_StudentTeachers | PRIMARY KEY | Id | 主键索引 |
| IX_StudentTeachers_StudentId_TeacherId | **UNIQUE INDEX** | StudentId, TeacherId | 联合唯一索引，防止重复建立指导关系 |
| IX_StudentTeachers_TeacherId | INDEX | TeacherId | 按教师查询其指导的所有学生 |

**外键与级联策略：**

| 外键字段 | 引用表 | 引用字段 | 级联删除 | 说明 |
|----------|--------|----------|----------|------|
| StudentId | Users | Id | **CASCADE** | 删除学生时级联删除师生关系 |
| TeacherId | Users | Id | **RESTRICT** | 有关联学生时不允许删除教师用户 |

---

### 3.7 WorkReviews（作品评语表）

**业务说明**：记录教师对作品的评语反馈，以及学生重新提交时的说明。通过 `Type` 字段区分：`review`（教师评语）和 `resubmit`（学生重新提交说明）。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 评语记录唯一标识 |
| 2 | **WorkId** | INT | — | NO | — | **FOREIGN KEY** → Works.Id | 关联作品 ID |
| 3 | **ReviewerId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 审阅者 ID（教师或学生） |
| 4 | **Comment** | VARCHAR | 2000 | NO | — | **REQUIRED** | 评语 / 反馈正文 |
| 5 | **Type** | VARCHAR | 20 | NO | 'review' | **REQUIRED** | 类型：`review`（教师评语）/ `resubmit`（学生重新提交说明） |
| 6 | **CreatedAt** | DATETIME | — | NO | NOW() | — | 评语创建时间 |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_WorkReviews | PRIMARY KEY | Id | 主键索引 |
| IX_WorkReviews_WorkId | INDEX | WorkId | 按作品查询其所有评语记录 |
| IX_WorkReviews_ReviewerId | INDEX | ReviewerId | 按评语人查询其所有评语 |

**外键与级联策略：**

| 外键字段 | 引用表 | 引用字段 | 级联删除 | 说明 |
|----------|--------|----------|----------|------|
| WorkId | Works | Id | **CASCADE** | 删除作品时级联删除评语记录 |
| ReviewerId | Users | Id | **RESTRICT** | 有关联评语时不允许删除用户 |

---

### 3.8 Notifications（通知表）

**业务说明**：存储系统推送给用户的消息通知，支持作品审核通知、系统公告通知、私信等类型。通过 `IsRead` 字段标记已读/未读状态。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 通知唯一标识 |
| 2 | **UserId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 接收通知的用户 ID |
| 3 | **Type** | VARCHAR | 50 | NO | 'system' | **REQUIRED** | 通知类型：`work_approval` / `system` / `message` |
| 4 | **Title** | VARCHAR | 100 | NO | — | **REQUIRED** | 通知标题 |
| 5 | **Content** | VARCHAR | 500 | NO | — | **REQUIRED** | 通知正文内容 |
| 6 | **WorkId** | INT | — | YES | NULL | **FOREIGN KEY** → Works.Id | 关联的作品 ID（可选，如审核通知） |
| 7 | **IsRead** | TINYINT(1) | — | NO | 0 | — | 是否已读：0 未读 / 1 已读 |
| 8 | **CreatedAt** | DATETIME | — | NO | NOW() | — | 通知创建时间 |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_Notifications | PRIMARY KEY | Id | 主键索引 |
| IX_Notifications_UserId_IsRead_CreatedAt | INDEX | UserId, IsRead, CreatedAt | 按用户 + 未读状态 + 时间查询未读通知 |

---

### 3.9 Announcements（公告表）

**业务说明**：存储系统管理员或教师发布的公共公告，支持置顶和启用/禁用控制。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 公告唯一标识 |
| 2 | **Title** | VARCHAR | 200 | NO | — | **REQUIRED** | 公告标题 |
| 3 | **Content** | TEXT | — | NO | — | **REQUIRED** | 公告正文内容 |
| 4 | **PublisherId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 发布者 ID |
| 5 | **IsPinned** | TINYINT(1) | — | NO | 0 | — | 是否置顶（置顶公告优先显示） |
| 6 | **IsEnabled** | TINYINT(1) | — | NO | 1 | — | 是否启用（0 表示已下架/隐藏） |
| 7 | **CreatedAt** | DATETIME | — | NO | NOW() | — | 发布时间 |
| 8 | **UpdatedAt** | DATETIME | — | NO | NOW() | — | 最后更新时间 |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_Announcements | PRIMARY KEY | Id | 主键索引 |
| IX_Announcements_IsPinned_CreatedAt | INDEX | IsPinned, CreatedAt | 按置顶+时间排序公告列表 |

---

### 3.10 Feedbacks（用户反馈表）

**业务说明**：存储用户对系统的意见和建议，支持管理员审核处理并回复。反馈状态流转：`pending` → `processing` → `resolved` / `rejected`。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 反馈唯一标识 |
| 2 | **UserId** | INT | — | NO | — | **FOREIGN KEY** → Users.Id | 提交反馈的用户 ID |
| 3 | **Type** | VARCHAR | 20 | NO | 'suggestion' | **REQUIRED** | 反馈类型：`suggestion` / `complaint` / `question` / `other` |
| 4 | **Title** | VARCHAR | 100 | NO | — | **REQUIRED** | 反馈标题 |
| 5 | **Content** | VARCHAR | 1000 | NO | — | **REQUIRED** | 反馈正文 |
| 6 | **Contact** | VARCHAR | 100 | YES | NULL | — | 用户联系方式（可选） |
| 7 | **Status** | VARCHAR | 20 | NO | 'pending' | **REQUIRED** | 处理状态：`pending` / `processing` / `resolved` / `rejected` |
| 8 | **Reply** | VARCHAR | 500 | YES | NULL | — | 管理员回复内容 |
| 9 | **RepliedAt** | DATETIME | — | YES | NULL | — | 管理员回复时间 |
| 10 | **CreatedAt** | DATETIME | — | NO | NOW() | — | 反馈提交时间 |

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_Feedbacks | PRIMARY KEY | Id | 主键索引 |
| IX_Feedbacks_UserId | INDEX | UserId | 按用户查询反馈历史 |
| IX_Feedbacks_Status | INDEX | Status | 按状态筛选待处理反馈 |

**状态流转图：**

```
  ┌──────────┐    开始处理    ┌────────────┐    解决完毕    ┌──────────┐
  │ pending  │ ────────────→ │ processing │ ────────────→ │ resolved │
  │ (待处理) │               │ (处理中)   │               │ (已解决) │
  └──────────┘               └────────────┘               └──────────┘
                                   │
                                   │         驳回
                                   └─────────────────────→ ┌──────────┐
                                                           │ rejected │
                                                           │ (已驳回) │
                                                           └──────────┘
```

---

### 3.11 ModerationItems（内容审核表）

**业务说明**：存储需要管理员审核的内容记录，支持风险等级评估（低/中/高）和审核状态流转（待审核→通过/拒绝）。`UserId` 为可空外键，删除用户时设为 NULL 保留审核记录。

| 序号 | 字段名 | 数据类型 | 最大长度 | 允许空 | 默认值 | 约束 | 说明 |
|------|--------|----------|----------|--------|--------|------|------|
| 1 | **Id** | INT | — | NO | AUTO_INCREMENT | **PRIMARY KEY** | 审核记录唯一标识 |
| 2 | **Title** | VARCHAR | — | NO | '' | — | 审核内容标题 |
| 3 | **Author** | VARCHAR | — | NO | '' | — | 作者姓名 |
| 4 | **Category** | VARCHAR | — | NO | '' | — | 内容分类 |
| 5 | **Type** | VARCHAR | — | NO | '' | — | 内容类型：`image` / `video` / `document` / `text` |
| 6 | **FileName** | VARCHAR | — | NO | '' | — | 关联的文件名 |
| 7 | **Preview** | VARCHAR | — | NO | '' | — | 内容预览文本 |
| 8 | **Content** | VARCHAR | — | NO | '' | — | 审核正文内容 |
| 9 | **SubmitTime** | DATETIME | — | NO | NOW() | — | 提交时间 |
| 10 | **RiskLevel** | VARCHAR | — | NO | '' | — | 风险等级：`low` / `medium` / `high` |
| 11 | **RiskDetails** | VARCHAR | — | NO | '' | — | 风险详情（用 `\|` 分隔多个风险项） |
| 12 | **Status** | VARCHAR | — | NO | 'pending' | — | 审核状态：`pending` / `approved` / `rejected` |
| 13 | **UserId** | INT | — | YES | NULL | **FOREIGN KEY** → Users.Id | 提交者 ID（可空） |

> **注**：ModerationItem 表中部分 VARCHAR 字段在 C# 模型中未标注 `[StringLength]`，EF Core 将使用数据库默认的 VARCHAR 最大长度（通常为 255 或 65535，取决于 MySQL 版本和配置）。

**索引设计：**

| 索引名 | 索引类型 | 字段 | 说明 |
|--------|----------|------|------|
| PK_ModerationItems | PRIMARY KEY | Id | 主键索引 |
| IX_ModerationItems_Status | INDEX | Status | 按审核状态筛选 |
| IX_ModerationItems_RiskLevel | INDEX | RiskLevel | 按风险等级筛选 |

**外键与级联策略：**

| 外键字段 | 引用表 | 引用字段 | 级联删除 | 说明 |
|----------|--------|----------|----------|------|
| UserId | Users | Id | **SET NULL** | 删除用户时保留审核记录，UserId 设为 NULL |

---

## 四、索引设计汇总

| 表名 | 索引名 | 索引类型 | 字段 | 用途 |
|------|--------|----------|------|------|
| Users | PK_Users | PRIMARY KEY | Id | 主键 |
| Users | IX_Users_Username | UNIQUE | Username | 登录查找，防重名 |
| Users | IX_Users_Role | INDEX | Role | 按角色查询 |
| Users | IX_Users_Department | INDEX | Department | 按院系筛选 |
| Works | PK_Works | PRIMARY KEY | Id | 主键 |
| Works | IX_Works_UserId | INDEX | UserId | 按上传者查询 |
| Works | IX_Works_Status | INDEX | Status | 按状态筛选 |
| Works | IX_Works_Category | INDEX | Category | 按分类浏览 |
| Works | IX_Works_UploadDate | INDEX | UploadDate | 按时间排序 |
| Works | IX_Works_Views | INDEX | Views | 热度排序 |
| Works | IX_Works_IsExcellent | INDEX | IsExcellent | 优秀作品筛选 |
| WorkFavorites | PK_WorkFavorites | PRIMARY KEY | Id | 主键 |
| WorkFavorites | IX_WF_UserId_WorkId | **UNIQUE** | UserId, WorkId | 防重复收藏 |
| WorkFavorites | IX_WF_WorkId | INDEX | WorkId | 查作品被收藏情况 |
| ViewHistories | PK_ViewHistories | PRIMARY KEY | Id | 主键 |
| ViewHistories | IX_VH_UserId_ViewedAt | INDEX | UserId, ViewedAt | 用户浏览历史 |
| ViewHistories | IX_VH_WorkId_ViewedAt | INDEX | WorkId, ViewedAt | 作品浏览趋势 |
| WorkStudents | PK_WorkStudents | PRIMARY KEY | Id | 主键 |
| WorkStudents | IX_WS_WorkId_StudentId | **UNIQUE** | WorkId, StudentId | 防重复参与 |
| StudentTeachers | PK_StudentTeachers | PRIMARY KEY | Id | 主键 |
| StudentTeachers | IX_ST_StudentId_TeacherId | **UNIQUE** | StudentId, TeacherId | 防重复指导关系 |
| StudentTeachers | IX_ST_TeacherId | INDEX | TeacherId | 查教师的学生列表 |
| WorkReviews | PK_WorkReviews | PRIMARY KEY | Id | 主键 |
| WorkReviews | IX_WR_WorkId | INDEX | WorkId | 查作品评语 |
| WorkReviews | IX_WR_ReviewerId | INDEX | ReviewerId | 查评语人记录 |
| Notifications | PK_Notifications | PRIMARY KEY | Id | 主键 |
| Notifications | IX_Notif_UserId_IsRead_Time | INDEX | UserId, IsRead, CreatedAt | 查未读通知 |
| Announcements | PK_Announcements | PRIMARY KEY | Id | 主键 |
| Announcements | IX_Ann_Pinned_CreatedAt | INDEX | IsPinned, CreatedAt | 公告排序 |
| Feedbacks | PK_Feedbacks | PRIMARY KEY | Id | 主键 |
| Feedbacks | IX_FB_UserId | INDEX | UserId | 查用户反馈 |
| Feedbacks | IX_FB_Status | INDEX | Status | 筛选待处理反馈 |
| ModerationItems | PK_ModerationItems | PRIMARY KEY | Id | 主键 |
| ModerationItems | IX_MI_Status | INDEX | Status | 按状态筛选 |
| ModerationItems | IX_MI_RiskLevel | INDEX | RiskLevel | 按风险等级筛选 |

**索引总数**：11 个主键 + 5 个唯一索引 + 17 个普通索引 = **33 个索引**

---

## 五、外键与级联策略汇总

| 子表 | 外键字段 | 父表 | 父表字段 | DELETE 策略 | 业务理由 |
|------|----------|------|----------|-------------|----------|
| Works | UserId | Users | Id | **RESTRICT** | 用户有作品时不可删除，需先清理作品 |
| WorkFavorites | UserId | Users | Id | **CASCADE** | 用户注销后收藏记录无意义 |
| WorkFavorites | WorkId | Works | Id | **CASCADE** | 作品删除后收藏关系自动清除 |
| ViewHistories | UserId | Users | Id | **CASCADE** | 用户注销后浏览历史无意义 |
| ViewHistories | WorkId | Works | Id | **CASCADE** | 作品删除后浏览记录自动清除 |
| WorkStudents | WorkId | Works | Id | **CASCADE** | 作品删除后参与关系自动清除 |
| WorkStudents | StudentId | Users | Id | **RESTRICT** | 学生有参与记录时不可删除 |
| StudentTeachers | StudentId | Users | Id | **CASCADE** | 学生注销后师生关系自动清除 |
| StudentTeachers | TeacherId | Users | Id | **RESTRICT** | 教师有学生时不可删除 |
| WorkReviews | WorkId | Works | Id | **CASCADE** | 作品删除后评语自动清除 |
| WorkReviews | ReviewerId | Users | Id | **RESTRICT** | 用户有评语记录时不可删除 |
| Notifications | UserId | Users | Id | **(默认 CASCADE)** | 用户注销后通知自动清除 |
| Notifications | WorkId | Works | Id | **(默认 SET NULL)** | 作品删除后通知保留但取消关联 |
| Announcements | PublisherId | Users | Id | **(默认 CASCADE)** | — |
| Feedbacks | UserId | Users | Id | **(默认 CASCADE)** | — |
| ModerationItems | UserId | Users | Id | **SET NULL** | 保留审核记录，仅断开用户关联 |

---

## 六、数据字典

### 6.1 角色枚举 (Users.Role)

| 值 | 说明 | 权限范围 |
|----|------|----------|
| `Admin` | 管理员 | 全部权限：用户管理、内容审核、公告发布、数据分析 |
| `Teacher` | 教师 | 作品评语、学生指导、团队管理、教学作品发布 |
| `Student` | 学生 | 作品上传、收藏浏览、参与协作、反馈提交 |

### 6.2 作品状态枚举 (Works.Status)

| 值 | 说明 |
|----|------|
| `草稿` | 初始状态，仅上传者可见 |
| `待审核` | 提交审核，等待管理员/教师审批 |
| `已发布` | 审核通过，公开可见 |
| `审核拒绝` | 审核未通过，需修改后重新提交 |

### 6.3 通知类型枚举 (Notifications.Type)

| 值 | 说明 |
|----|------|
| `work_approval` | 作品审核结果通知 |
| `system` | 系统公告/维护通知 |
| `message` | 私信/个人消息 |

### 6.4 反馈类型枚举 (Feedbacks.Type)

| 值 | 说明 |
|----|------|
| `suggestion` | 改进建议 |
| `complaint` | 投诉举报 |
| `question` | 问题咨询 |
| `other` | 其他 |

### 6.5 反馈状态枚举 (Feedbacks.Status)

| 值 | 说明 |
|----|------|
| `pending` | 待处理 |
| `processing` | 处理中 |
| `resolved` | 已解决 |
| `rejected` | 已驳回 |

### 6.6 审核状态枚举 (ModerationItems.Status)

| 值 | 说明 |
|----|------|
| `pending` | 待审核 |
| `approved` | 审核通过 |
| `rejected` | 审核拒绝 |

### 6.7 风险等级枚举 (ModerationItems.RiskLevel)

| 值 | 说明 |
|----|------|
| `low` | 低风险 |
| `medium` | 中风险 |
| `high` | 高风险 |

### 6.8 内容类型枚举 (ModerationItems.Type)

| 值 | 说明 |
|----|------|
| `image` | 图片 |
| `video` | 视频 |
| `document` | 文档 |
| `text` | 文本 |

### 6.9 主题偏好枚举 (Users.Theme)

| 值 | 说明 |
|----|------|
| `light` | 浅色模式 |
| `dark` | 深色模式 |

### 6.10 收藏可见性枚举 (Users.FavoritesVisibility)

| 值 | 说明 |
|----|------|
| `public` | 所有人可见 |
| `followers` | 仅粉丝/师生可见 |
| `private` | 仅自己可见 |

### 6.11 评语类型枚举 (WorkReviews.Type)

| 值 | 说明 |
|----|------|
| `review` | 教师评语 |
| `resubmit` | 学生重新提交说明 |

---

## 七、关键设计决策说明

### 7.1 User ↔ Work 关系三分类

User 与 Work 之间原本可能存在模糊的多对多关系，设计中将其拆解为三种明确语义：

| 关系 | 存储位置 | 语义 | 典型场景 |
|------|----------|------|----------|
| **上传 (Upload)** | Works.UserId (1:n) | 作品归属 | 用户创建/拥有作品 |
| **收藏 (Favorite)** | WorkFavorites (n:m) | 个人喜好 | 用户标记喜欢的作品 |
| **参与 (Participate)** | WorkStudents (n:m) | 协作贡献 | 学生实际参与了某作品制作 |

### 7.2 冗余计数器

`Works.Views` 和 `Works.Favorites` 为冗余计数字段，目的是避免高频的 `COUNT` 聚合查询影响性能：

- `Views`：每次浏览时递增，可从 ViewHistory 表定期重算校准
- `Favorites`：收藏/取消收藏时通过事务同步更新，与 WorkFavorites 表保持一致

### 7.3 师生关系设计

通过 `StudentTeachers` 中间表实现多对多关系，而非在 User 表中使用单一外键，原因是：

- 学生可能有主导师 + 副导师（多教师指导一学生）
- 教师指导多个学生（一教师指导多学生）
- 关系可追溯建立时间，支持历史查询

### 7.4 文件存储策略

文件元数据（文件名、大小、MD5、路径）存储在 Works 表中，而实际文件存储在服务器磁盘的 `Uploads/` 目录下。通过 MD5 校验可实现文件去重。

### 7.5 通知机制

系统通过 Notifications 表实现站内通知，`IsRead` 标记支持未读/已读状态管理。通知与用户直接关联，可选关联作品（如审核结果通知）。

---

## 八、SQL 建表语句（参考）

> 以下为各表 DDL 参考语句，实际建表由 EF Core Migration 自动生成。

```sql
-- 1. Users
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL,
    Password VARCHAR(100) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    Remember TINYINT(1) NOT NULL DEFAULT 0,
    Role VARCHAR(20) NOT NULL DEFAULT 'Student',
    Name VARCHAR(50) NULL,
    WorkId VARCHAR(20) NULL,
    Department VARCHAR(100) NULL,
    Phone VARCHAR(20) NULL,
    Email VARCHAR(100) NULL,
    Bio VARCHAR(500) NULL,
    Title VARCHAR(50) NULL,
    ResearchArea VARCHAR(200) NULL,
    Position VARCHAR(50) NULL,
    Avatar VARCHAR(500) NULL,
    Theme VARCHAR(20) NOT NULL DEFAULT 'light',
    Language VARCHAR(20) NOT NULL DEFAULT 'zh-CN',
    NotificationEnabled TINYINT(1) NOT NULL DEFAULT 1,
    EmailNotification TINYINT(1) NOT NULL DEFAULT 1,
    WorkModerationRequired TINYINT(1) NOT NULL DEFAULT 0,
    ProfilePublic TINYINT(1) NOT NULL DEFAULT 1,
    ShowContactInfo TINYINT(1) NOT NULL DEFAULT 0,
    FavoritesVisibility VARCHAR(20) NOT NULL DEFAULT 'public',
    UNIQUE INDEX IX_Users_Username (Username),
    INDEX IX_Users_Role (Role),
    INDEX IX_Users_Department (Department)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Works
CREATE TABLE Works (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(100) NOT NULL,
    Category VARCHAR(50) NOT NULL,
    Description VARCHAR(1000) NULL,
    UploadDate DATETIME NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT '草稿',
    FilePath VARCHAR(500) NULL,
    FileName VARCHAR(255) NULL,
    FileSize BIGINT NOT NULL DEFAULT 0,
    FileMD5 VARCHAR(32) NULL,
    FileUploadTime DATETIME NULL,
    PreviewImage VARCHAR(500) NULL,
    UserId INT NOT NULL,
    IsExcellent TINYINT(1) NOT NULL DEFAULT 0,
    Views INT NOT NULL DEFAULT 0,
    Favorites INT NOT NULL DEFAULT 0,
    INDEX IX_Works_UserId (UserId),
    INDEX IX_Works_Status (Status),
    INDEX IX_Works_Category (Category),
    INDEX IX_Works_UploadDate (UploadDate),
    INDEX IX_Works_Views (Views),
    INDEX IX_Works_IsExcellent (IsExcellent),
    CONSTRAINT FK_Works_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. WorkFavorites
CREATE TABLE WorkFavorites (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    WorkId INT NOT NULL,
    FavoriteDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE INDEX IX_WF_UserId_WorkId (UserId, WorkId),
    INDEX IX_WF_WorkId (WorkId),
    CONSTRAINT FK_WF_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WF_Works_WorkId FOREIGN KEY (WorkId) REFERENCES Works(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. ViewHistories
CREATE TABLE ViewHistories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    WorkId INT NOT NULL,
    ViewedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_VH_UserId_ViewedAt (UserId, ViewedAt),
    INDEX IX_VH_WorkId_ViewedAt (WorkId, ViewedAt),
    CONSTRAINT FK_VH_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_VH_Works_WorkId FOREIGN KEY (WorkId) REFERENCES Works(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. WorkStudents
CREATE TABLE WorkStudents (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkId INT NOT NULL,
    StudentId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE INDEX IX_WS_WorkId_StudentId (WorkId, StudentId),
    CONSTRAINT FK_WS_Works_WorkId FOREIGN KEY (WorkId) REFERENCES Works(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WS_Users_StudentId FOREIGN KEY (StudentId) REFERENCES Users(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. StudentTeachers
CREATE TABLE StudentTeachers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    TeacherId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE INDEX IX_ST_StudentId_TeacherId (StudentId, TeacherId),
    INDEX IX_ST_TeacherId (TeacherId),
    CONSTRAINT FK_ST_Users_StudentId FOREIGN KEY (StudentId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ST_Users_TeacherId FOREIGN KEY (TeacherId) REFERENCES Users(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 7. WorkReviews
CREATE TABLE WorkReviews (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkId INT NOT NULL,
    ReviewerId INT NOT NULL,
    Comment VARCHAR(2000) NOT NULL,
    Type VARCHAR(20) NOT NULL DEFAULT 'review',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_WR_WorkId (WorkId),
    INDEX IX_WR_ReviewerId (ReviewerId),
    CONSTRAINT FK_WR_Works_WorkId FOREIGN KEY (WorkId) REFERENCES Works(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WR_Users_ReviewerId FOREIGN KEY (ReviewerId) REFERENCES Users(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8. Notifications
CREATE TABLE Notifications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Type VARCHAR(50) NOT NULL DEFAULT 'system',
    Title VARCHAR(100) NOT NULL,
    Content VARCHAR(500) NOT NULL,
    WorkId INT NULL,
    IsRead TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_Notif_UserId_IsRead_CreatedAt (UserId, IsRead, CreatedAt),
    CONSTRAINT FK_Notif_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Notif_Works_WorkId FOREIGN KEY (WorkId) REFERENCES Works(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 9. Announcements
CREATE TABLE Announcements (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Content TEXT NOT NULL,
    PublisherId INT NOT NULL,
    IsPinned TINYINT(1) NOT NULL DEFAULT 0,
    IsEnabled TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_Ann_IsPinned_CreatedAt (IsPinned, CreatedAt),
    CONSTRAINT FK_Ann_Users_PublisherId FOREIGN KEY (PublisherId) REFERENCES Users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 10. Feedbacks
CREATE TABLE Feedbacks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Type VARCHAR(20) NOT NULL DEFAULT 'suggestion',
    Title VARCHAR(100) NOT NULL,
    Content VARCHAR(1000) NOT NULL,
    Contact VARCHAR(100) NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'pending',
    Reply VARCHAR(500) NULL,
    RepliedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_FB_UserId (UserId),
    INDEX IX_FB_Status (Status),
    CONSTRAINT FK_FB_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 11. ModerationItems
CREATE TABLE ModerationItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL DEFAULT '',
    Author VARCHAR(255) NOT NULL DEFAULT '',
    Category VARCHAR(255) NOT NULL DEFAULT '',
    Type VARCHAR(255) NOT NULL DEFAULT '',
    FileName VARCHAR(255) NOT NULL DEFAULT '',
    Preview VARCHAR(255) NOT NULL DEFAULT '',
    Content VARCHAR(255) NOT NULL DEFAULT '',
    SubmitTime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    RiskLevel VARCHAR(255) NOT NULL DEFAULT '',
    RiskDetails VARCHAR(255) NOT NULL DEFAULT '',
    Status VARCHAR(255) NOT NULL DEFAULT 'pending',
    UserId INT NULL,
    INDEX IX_MI_Status (Status),
    INDEX IX_MI_RiskLevel (RiskLevel),
    CONSTRAINT FK_MI_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

---

## 九、表空间估算

假设系统运行一年后的数据规模预估：

| 表名 | 预估行数 | 单行大小 | 预估总大小 | 增长模式 |
|------|----------|----------|------------|----------|
| Users | 5,000 | ~800 B | ~4 MB | 线性增长（每学期新增用户） |
| Works | 20,000 | ~1 KB | ~20 MB | 线性增长（用户持续上传） |
| WorkFavorites | 100,000 | ~30 B | ~3 MB | 快速增长（高频操作） |
| ViewHistories | 500,000 | ~30 B | ~15 MB | 最快增长（每次浏览记录） |
| WorkStudents | 40,000 | ~30 B | ~1.2 MB | 线性增长 |
| StudentTeachers | 8,000 | ~30 B | ~0.24 MB | 缓慢增长 |
| WorkReviews | 30,000 | ~2.2 KB | ~66 MB | 线性增长 |
| Notifications | 200,000 | ~550 B | ~110 MB | 快速增长 |
| Announcements | 500 | ~1 KB | ~0.5 MB | 缓慢增长 |
| Feedbacks | 2,000 | ~1.2 KB | ~2.4 MB | 缓慢增长 |
| ModerationItems | 10,000 | ~1.5 KB | ~15 MB | 线性增长 |
| **合计** | **~915,500** | — | **~237 MB** | — |

> 注：ViewHistories 和 Notifications 为高增长表，建议定期归档或设置 TTL 清理策略（如保留最近 6 个月数据）。

---

## 十、变更记录

| 版本 | 日期 | 修改内容 | 修改人 |
|------|------|----------|--------|
| v1.0 | 2026-05-19 | 初始版本，完整数据库详细设计 | — |

---

*文档基于项目 Models 定义和 AppDbContext 配置分析生成*