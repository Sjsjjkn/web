# 数据库模型设计说明

## 概述

本文档描述了毕业设计项目的数据库模型设计。

## 实体关系图

```
┌──────────┐       ┌──────────────┐       ┌──────────┐
│   User   │       │ WorkFavorite │       │   Work   │
│          │1    n │              │ n    1│          │
│    Id    │◄──────┤    UserId    ├──────►│    Id    │
│   Name   │       │    WorkId    │       │  Title   │
│ Username │       │  FavoriteDate│       │ Category │
│  Email   │       └──────────────┘       │  Status  │
│Password  │                              │  Views   │
│  Role    │       ┌──────────────┐       │Favorites │
│ Avatar   │       │ WorkStudent  │       │File...   │
│Bio/School│1    n │              │ n    1│          │
│ Lab/...  │◄──────┤  StudentId   ├──────►│          │
└──────────┘       │   WorkId     │       └──────────┘
     │             └──────────────┘
     │             ┌──────────────┐
     │             │StudentTeacher│
     │1          n │              │
     ├────────────►│  StudentId   │
     │             │  TeacherId   │
     │1          n └──────────────┘
     │                  ▲
     │                  │
     │             ┌────┴─────────┐
     │             │ModerationItem│
     └────────────►│   UserId     │
                   │  WorkId      │
                   │  ...         │
                   └──────────────┘
```

## 核心表说明

### 1. Users（用户表）
- **业务含义**: 系统所有用户的基础信息
- **角色**: Admin（管理员）、Teacher（教师）、Student（学生）

### 2. Works（作品表）
- **业务含义**: 用户上传的作品/项目文件
- **关键关系**:
  - `UserId` → `Users.Id`：上传者（一对多）
  - `WorkFavorites`：被哪些用户收藏（多对多，通过 WorkFavorite）
  - `WorkStudents`：哪些学生参与协作（多对多，通过 WorkStudent）

### 3. WorkFavorite（作品收藏表）
- **业务含义**: 用户收藏作品的关系（唯一收藏表）
- **关联**: `UserId` → `Users.Id`，`WorkId` → `Works.Id`
- **注意**: 删除了原本重复的 `UserCollection` 表，`WorkFavorite` 是唯一收藏关系表

### 4. WorkStudent（作品参与表）
- **业务含义**: 记录学生参与了哪些作品
- **关联**: `StudentId` → `Users.Id`（学生用户），`WorkId` → `Works.Id`
- **与 WorkFavorite 的区别**: 收藏是"我喜欢"，参与是"我做了"

### 5. StudentTeacher（师生关系表）
- **业务含义**: 记录教师指导学生的关系
- **关联**: `StudentId` → `Users.Id`，`TeacherId` → `Users.Id`

### 6. ModerationItem（审核记录表）
- **业务含义**: 作品审核/举报记录
- **关联**: `UserId` → `Users.Id`（审核员），`WorkId` → `Works.Id`（被审核作品）

## 关键设计决策

1. **消除重复**: 删除了与 `WorkFavorite` 功能完全重复的 `UserCollection` 表
2. **清晰命名**: 关系名称直接反映业务语义
   - `WorkFavorite` = 收藏
   - `WorkStudent` = 参与协作
   - `StudentTeacher` = 师生指导
3. **User与Work的关系精简为3类**:
   - 上传（Upload）: Works.UserId → User.Id（作品所有者）
   - 收藏（Favorite）: WorkFavorite（用户喜欢作品）
   - 参与（Participate）: WorkStudent（学生参与了作品制作）

## 迁移说明

如需重建数据库，执行以下步骤：
1. 运行 `drop_tables.sql` 删除旧表
2. 重新运行 EF Core 迁移或直接启动应用（自动建表）