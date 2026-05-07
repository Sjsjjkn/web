# 保定学院数字作品展厅系统 - 陈氏ER图设计

## 一、ER图概述

本文档采用**陈氏ER图（Chen Notation）**规范，为保定学院数字作品展厅系统设计数据库概念模型。

### 陈氏ER图符号规范

| 符号类型 | 图形表示 | 说明 |
|---------|---------|------|
| 实体（Entity） | ▭ 矩形 | 表示现实世界中的事物或概念 |
| 属性（Attribute） | ○ 椭圆 | 表示实体的特性或性质 |
| 关系（Relationship） | ◇ 菱形 | 表示实体之间的关联 |
| 主键（Primary Key） | 下划线 | 属性名称下方加下划线 |
| 联系类型 | 数字标记 | 1:1、1:n、m:n |
| 连接线 | ─ 直线 | 连接实体、属性和关系 |

---

## 二、实体定义

### 1. 用户实体（User）

**符号表示**: ▭ **User**

**属性列表**（○ 椭圆）:

| 属性名称 | 类型 | 是否主键 | 说明 |
|---------|------|---------|------|
| **Id** | Integer | ○ **PK** | 用户唯一标识 |
| Username | String(50) | ○ NOT NULL | 登录用户名 |
| Password | String(100) | ○ NOT NULL | 密码（加密存储） |
| Role | String(20) | ○ NOT NULL | 角色类型（Student/Teacher/Admin） |
| Name | String(50) | | 用户姓名 |
| WorkId | String(20) | | 学号/工号 |
| Department | String(100) | | 院系/部门 |
| Phone | String(20) | | 联系电话 |
| Email | String(100) | | 电子邮箱 |
| Bio | String(500) | | 个人简介 |
| Title | String(50) | | 职称 |
| ResearchArea | String(200) | | 研究领域 |
| Position | String(50) | | 职位 |
| Avatar | String(500) | | 头像路径 |
| Remember | Boolean | | 记住登录状态 |
| CreatedAt | DateTime | | 创建时间 |

---

### 2. 作品实体（Work）

**符号表示**: ▭ **Work**

**属性列表**（○ 椭圆）:

| 属性名称 | 类型 | 是否主键 | 说明 |
|---------|------|---------|------|
| **Id** | Integer | ○ **PK** | 作品唯一标识 |
| Title | String(100) | ○ NOT NULL | 作品标题 |
| Category | String(50) | ○ NOT NULL | 作品分类 |
| Description | String(1000) | | 作品描述 |
| UploadDate | DateTime | | 上传日期 |
| Status | String(20) | ○ NOT NULL | 状态（草稿/待审核/已发布） |
| FilePath | String(500) | | 文件存储路径 |
| FileName | String(255) | | 原始文件名 |
| FileSize | Long | | 文件大小（字节） |
| FileMD5 | String(32) | | 文件MD5校验值 |
| FileUploadTime | DateTime | | 文件上传时间 |
| PreviewImage | String(500) | | 预览图路径 |
| UserId | Integer | ○ FK | 上传用户ID（外键） |
| IsExcellent | Boolean | | 是否优秀作品 |
| Views | Integer | | 浏览次数 |
| Favorites | Integer | | 收藏次数 |

---

### 3. 审核项目实体（ModerationItem）

**符号表示**: ▭ **ModerationItem**

**属性列表**（○ 椭圆）:

| 属性名称 | 类型 | 是否主键 | 说明 |
|---------|------|---------|------|
| **Id** | Integer | ○ **PK** | 审核项唯一标识 |
| Title | String(100) | | 审核内容标题 |
| Author | String(50) | | 作者姓名 |
| Category | String(50) | | 内容分类 |
| Type | String(20) | | 内容类型（image/video/document/text） |
| FileName | String(255) | | 关联文件名 |
| Preview | String(500) | | 预览内容 |
| Content | String(1000) | | 审核内容 |
| RiskLevel | String(20) | | 风险等级（low/medium/high） |
| RiskDetails | String(500) | | 风险详情（用\|分隔多个风险） |
| Status | String(20) | | 审核状态（pending/approved/rejected） |
| SubmitTime | DateTime | | 提交时间 |

---

### 4. 作品学生关联实体（WorkStudent）

**符号表示**: ▭ **WorkStudent**

**属性列表**（○ 椭圆）:

| 属性名称 | 类型 | 是否主键 | 说明 |
|---------|------|---------|------|
| **Id** | Integer | ○ **PK** | 关联ID |
| WorkId | Integer | ○ FK | 作品ID（外键） |
| StudentId | Integer | ○ FK | 学生ID（外键） |
| CreatedAt | DateTime | | 关联时间 |

---

### 5. 学生教师关联实体（StudentTeacher）

**符号表示**: ▭ **StudentTeacher**

**属性列表**（○ 椭圆）:

| 属性名称 | 类型 | 是否主键 | 说明 |
|---------|------|---------|------|
| **Id** | Integer | ○ **PK** | 关联ID |
| StudentId | Integer | ○ FK | 学生ID（外键） |
| TeacherId | Integer | ○ FK | 教师ID（外键） |
| CreatedAt | DateTime | | 关联时间 |

---

### 6. 作品收藏实体（WorkFavorite）

**符号表示**: ▭ **WorkFavorite**

**属性列表**（○ 椭圆）:

| 属性名称 | 类型 | 是否主键 | 说明 |
|---------|------|---------|------|
| **Id** | Integer | ○ **PK** | 收藏ID |
| UserId | Integer | ○ FK | 用户ID（外键） |
| WorkId | Integer | ○ FK | 作品ID（外键） |
| FavoriteDate | DateTime | | 收藏时间 |

---

### 7. 用户收藏实体（UserCollection）

**符号表示**: ▭ **UserCollection**

**属性列表**（○ 椭圆）:

| 属性名称 | 类型 | 是否主键 | 说明 |
|---------|------|---------|------|
| **Id** | Integer | ○ **PK** | 收藏ID |
| WorkId | Integer | ○ FK | 作品ID（外键） |
| UserId | Integer | ○ FK | 用户ID（外键） |
| CollectionDate | DateTime | | 收藏时间 |

---

## 三、关系定义

### 1. 上传关系（Uploads）

**符号表示**: ◇ **Uploads**

**关联实体**: User ↔ Work

**联系类型**: **1:n**（一对多）

**说明**: 一个用户可以上传多个作品，但一个作品只能由一个用户上传。

```
User 1 ─── Uploads ─── n Work
```

---

### 2. 收藏关系（Favorites）

**符号表示**: ◇ **Favorites**

**关联实体**: User ↔ Work

**联系类型**: **m:n**（多对多）

**说明**: 一个用户可以收藏多个作品，一个作品可以被多个用户收藏。

**关系属性**:
- FavoriteDate（收藏时间）

```
User m ─── Favorites ─── n Work
```

---

### 3. 参与关系（Participates）

**符号表示**: ◇ **Participates**

**关联实体**: User ↔ Work

**联系类型**: **m:n**（多对多）

**说明**: 一个学生可以参与多个作品，一个作品可以有多个学生参与（团队作品）。

**关系属性**:
- CreatedAt（关联时间）

```
User(Student) m ─── Participates ─── n Work
```

---

### 4. 指导关系（Guides）

**符号表示**: ◇ **Guides**

**关联实体**: User(Teacher) ↔ User(Student)

**联系类型**: **m:n**（多对多）

**说明**: 一个教师可以指导多个学生，一个学生可以有多个指导教师。

**关系属性**:
- CreatedAt（指导关系建立时间）

```
User(Teacher) m ─── Guides ─── n User(Student)
```

---

### 5. 审核关系（Moderates）

**符号表示**: ◇ **Moderates**

**关联实体**: User ↔ ModerationItem

**联系类型**: **1:n**（一对多）

**说明**: 一个审核项目由一个用户提交，一个用户可以提交多个审核项目。

```
User 1 ─── Moderates ─── n ModerationItem
```

---

## 四、完整陈氏ER图

```
                        ┌─────────────────────────────────────┐
                        │          User (用户)                │
                        │  ┌─────────────────────────────┐    │
                        │  │ ○ Id ─── PK (用户ID)        │    │
                        │  │ ○ Username (用户名)         │    │
                        │  │ ○ Password (密码)           │    │
                        │  │ ○ Role (角色)               │    │
                        │  │ ○ Name (姓名)               │    │
                        │  │ ○ WorkId (学号/工号)        │    │
                        │  │ ○ Department (院系)         │    │
                        │  │ ○ Phone (电话)              │    │
                        │  │ ○ Email (邮箱)              │    │
                        │  │ ○ Avatar (头像)             │    │
                        │  │ ○ CreatedAt (创建时间)      │    │
                        │  └─────────────────────────────┘    │
                        └──────────────────┬──────────────────┘
                                           │
          ┌────────────────────────────────┼────────────────────────────────┐
          │                                │                                │
          │ 1                              │ 1                              │ m
          ▼                                ▼                                │
┌───────────────────┐            ┌───────────────────┐                     │
│   ◇ Uploads      │            │   ◇ Moderates     │                     │
│   (上传)         │            │   (审核)          │                     │
└────────┬──────────┘            └────────┬──────────┘                     │
         │ 1                              │ 1                              │
         │                                │                                │
         n                                n                                │
         ▼                                ▼                                │
┌─────────────────────────────────────┐   ┌─────────────────────────────┐   │
│          Work (作品)                │   │    ModerationItem (审核)     │   │
│  ┌─────────────────────────────┐    │   │  ┌─────────────────────┐    │   │
│  │ ○ Id ─── PK (作品ID)        │    │   │  │ ○ Id ─── PK         │    │   │
│  │ ○ Title (标题)              │    │   │  │ ○ Title             │    │   │
│  │ ○ Category (分类)           │    │   │  │ ○ Author            │    │   │
│  │ ○ Description (描述)        │    │   │  │ ○ Category          │    │   │
│  │ ○ UploadDate (上传日期)     │    │   │  │ ○ Type              │    │   │
│  │ ○ Status (状态)             │    │   │  │ ○ FileName          │    │   │
│  │ ○ FilePath (文件路径)       │    │   │  │ ○ RiskLevel         │    │   │
│  │ ○ FileName (文件名)         │    │   │  │ ○ RiskDetails       │    │   │
│  │ ○ FileSize (文件大小)       │    │   │  │ ○ Status            │    │   │
│  │ ○ FileMD5 (MD5值)          │    │   │  │ ○ SubmitTime        │    │   │
│  │ ○ IsExcellent (优秀标记)    │    │   │  └─────────────────────┘    │   │
│  │ ○ Views (浏览量)            │    │   └─────────────────────────────┘   │
│  │ ○ Favorites (收藏数)        │    │                                    │
│  └─────────────────────────────┘    │                                    │
└──────────────────┬──────────────────┘                                    │
                   │                                                      │
         ┌─────────┼─────────┐                                            │
         │         │         │                                            │
         n         n         │                                            │
         ▼         ▼         │                                            │
┌───────────────────┐ ┌───────────────────┐                                │
│   ◇ Favorites    │ │ ◇ Participates    │                                │
│   (收藏)         │ │   (参与)          │                                │
│   ┌───────────┐  │ │   ┌───────────┐  │                                │
│   │ ○ FavoriteDate│ │   │ ○ CreatedAt│  │                                │
│   └───────────┘  │ │   └───────────┘  │                                │
└────────┬──────────┘ └────────┬──────────┘                                │
         │ m                   │ m                                        │
         │                     │                                        │
         └─────────────────────┴─────────────────────────────────────────┘
```

---

## 五、关系类型汇总表

| 关系名称 | 实体1 | 联系类型 | 实体2 | 说明 |
|---------|------|---------|------|------|
| Uploads | User | 1:n | Work | 用户上传作品 |
| Favorites | User | m:n | Work | 用户收藏作品 |
| Participates | User(Student) | m:n | Work | 学生参与作品 |
| Guides | User(Teacher) | m:n | User(Student) | 教师指导学生 |
| Moderates | User | 1:n | ModerationItem | 用户提交审核 |

---

## 六、实体-属性汇总表

### 1. User（用户）实体

| 属性名 | 类型 | 约束 | 说明 |
|-------|------|------|------|
| Id | Integer | PK | 用户唯一标识 |
| Username | String(50) | NOT NULL | 登录用户名 |
| Password | String(100) | NOT NULL | 加密密码 |
| Role | String(20) | NOT NULL, DEFAULT='Student' | 角色：Student/Teacher/Admin |
| Name | String(50) | | 用户姓名 |
| WorkId | String(20) | | 学号或工号 |
| Department | String(100) | | 所属院系 |
| Phone | String(20) | | 联系电话 |
| Email | String(100) | | 电子邮箱 |
| Avatar | String(500) | | 头像文件路径 |
| CreatedAt | DateTime | DEFAULT=NOW() | 创建时间 |

### 2. Work（作品）实体

| 属性名 | 类型 | 约束 | 说明 |
|-------|------|------|------|
| Id | Integer | PK | 作品唯一标识 |
| Title | String(100) | NOT NULL | 作品标题 |
| Category | String(50) | NOT NULL | 分类：设计/编程/论文等 |
| Description | String(1000) | | 作品描述 |
| UploadDate | DateTime | DEFAULT=NOW() | 上传日期 |
| Status | String(20) | DEFAULT='草稿' | 状态：草稿/待审核/已发布 |
| FilePath | String(500) | | 文件存储路径 |
| FileName | String(255) | | 原始文件名 |
| FileSize | Long | | 文件大小 |
| FileMD5 | String(32) | | 文件校验值 |
| IsExcellent | Boolean | DEFAULT=FALSE | 是否优秀作品 |
| Views | Integer | DEFAULT=0 | 浏览次数 |
| Favorites | Integer | DEFAULT=0 | 收藏次数 |

### 3. ModerationItem（审核项目）实体

| 属性名 | 类型 | 约束 | 说明 |
|-------|------|------|------|
| Id | Integer | PK | 审核项唯一标识 |
| Title | String(100) | NOT NULL | 审核内容标题 |
| Author | String(50) | | 作者姓名 |
| Category | String(50) | | 内容分类 |
| Type | String(20) | | 类型：image/video/document/text |
| FileName | String(255) | | 关联文件名 |
| RiskLevel | String(20) | DEFAULT='low' | 风险等级：low/medium/high |
| RiskDetails | String(500) | | 风险详情，用\|分隔 |
| Status | String(20) | DEFAULT='pending' | 状态：pending/approved/rejected |
| SubmitTime | DateTime | DEFAULT=NOW() | 提交时间 |

---

## 七、数据完整性约束

### 1. 实体完整性约束
- **主键约束**: 所有实体的Id字段为主键，确保唯一性
- **非空约束**: 关键字段（如Username、Password、Role等）不允许为空

### 2. 参照完整性约束
- **外键约束**: 关系表中的外键必须引用对应实体的主键
- **级联删除**: 删除用户时，级联删除其上传的作品和审核记录

### 3. 域完整性约束
- **Role字段**: 只能取 'Student'、'Teacher'、'Admin'
- **Status字段**: 只能取 '草稿'、'待审核'、'已发布'
- **RiskLevel字段**: 只能取 'low'、'medium'、'high'

---

## 八、ER图设计原则

1. **唯一性原则**: 每个实体必须有唯一的标识（主键）
2. **最小化原则**: 属性应尽量精简，避免冗余
3. **一致性原则**: 命名规范统一，语义明确
4. **完整性原则**: 确保所有业务需求都能被ER图表达
5. **可扩展性原则**: 设计应考虑未来的功能扩展

---

## 九、使用说明

本ER图适用于以下场景：
- 毕业设计论文中的数据库设计章节
- 系统架构文档
- 数据库开发参考

如需将此ER图转换为SQL DDL语句或其他格式，请告知。

---

**文档版本**: v1.0  
**创建日期**: 2026年5月6日  
**适用系统**: 保定学院数字作品展厅系统
