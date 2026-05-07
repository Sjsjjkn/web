# 保定学院数字作品展厅系统 - 优化后的ER图关系设计

## 一、关系设计优化说明

### 优化前的问题

| 问题类型 | 具体问题 | 影响 |
|---------|---------|------|
| 重复设计 | WorkFavorite 和 UserCollection 功能重复 | 数据冗余、维护复杂 |
| 命名模糊 | 关系名称不够直观 | 理解困难 |
| 关系过多 | User-Work 之间有3个关系 | 逻辑复杂 |

### 优化策略

1. **合并重复表**: 删除 `UserCollection`，保留 `WorkFavorite` 作为唯一收藏表
2. **清晰命名**: 使用更有业务语义的关系名称
3. **简化关系**: 明确每种关系的业务含义

---

## 二、优化后的实体关系

### 实体清单

| 序号 | 实体名称 | 说明 | 属性数 |
|------|---------|------|--------|
| 1 | User | 用户（学生/教师/管理员） | 16 |
| 2 | Work | 作品 | 16 |
| 3 | WorkFavorite | 作品收藏 | 4 |
| 4 | WorkStudent | 作品学生关联（团队参与） | 4 |
| 5 | StudentTeacher | 师生关系 | 4 |
| 6 | ModerationItem | 审核项目 | 12 |

### 关系清单

| 序号 | 关系名称 | 实体1 | 类型 | 实体2 | 说明 |
|------|---------|------|------|------|------|
| 1 | 上传 | User | 1:n | Work | 用户上传作品 |
| 2 | 收藏 | User | m:n | Work | 用户收藏作品（通过WorkFavorite） |
| 3 | 参与 | User(Student) | m:n | Work | 学生参与团队作品（通过WorkStudent） |
| 4 | 指导 | User(Teacher) | m:n | User(Student) | 教师指导学生（通过StudentTeacher） |
| 5 | 提交审核 | User | 1:n | ModerationItem | 用户提交审核项目 |

---

## 三、详细关系说明

### 1. 上传关系（Uploads）

**符号表示**: ◇ **上传**

**关联实体**: User ↔ Work

**联系类型**: **1:n**（一对多）

**说明**: 一个用户可以上传多个作品，但一个作品只能由一个用户（创建者）上传。

```
User 1 ───◇ 上传 ───n Work
```

**实现方式**: Work 表中的 `UserId` 外键直接关联到 User 表

---

### 2. 收藏关系（Favorites）

**符号表示**: ◇ **收藏**

**关联实体**: User ↔ Work

**联系类型**: **m:n**（多对多）

**说明**: 一个用户可以收藏多个作品，一个作品可以被多个用户收藏。

```
User m ───◇ 收藏 ───n Work
```

**实现方式**: 通过 `WorkFavorite` 中间表实现
- WorkFavorite.UserId → User.Id
- WorkFavorite.WorkId → Work.Id

**关系属性**: FavoriteDate（收藏时间）

---

### 3. 参与关系（Participates）

**符号表示**: ◇ **参与**

**关联实体**: User(Student) ↔ Work

**联系类型**: **m:n**（多对多）

**说明**: 一个学生可以参与多个团队作品，一个作品可以有多个学生参与。

```
User(Student) m ───◇ 参与 ───n Work
```

**实现方式**: 通过 `WorkStudent` 中间表实现
- WorkStudent.StudentId → User.Id（学生）
- WorkStudent.WorkId → Work.Id

**关系属性**: CreatedAt（参与时间）

---

### 4. 指导关系（Guides）

**符号表示**: ◇ **指导**

**关联实体**: User(Teacher) ↔ User(Student)

**联系类型**: **m:n**（多对多自关联）

**说明**: 一个教师可以指导多个学生，一个学生可以有多个指导教师。

```
User(Teacher) m ───◇ 指导 ───n User(Student)
```

**实现方式**: 通过 `StudentTeacher` 中间表实现
- StudentTeacher.TeacherId → User.Id（教师）
- StudentTeacher.StudentId → User.Id（学生）

**关系属性**: CreatedAt（指导关系建立时间）

---

### 5. 提交审核关系（Submits）

**符号表示**: ◇ **提交审核**

**关联实体**: User ↔ ModerationItem

**联系类型**: **1:n**（一对多）

**说明**: 一个用户可以提交多个审核项目，一个审核项目由一个用户提交。

```
User 1 ───◇ 提交审核 ───n ModerationItem
```

**实现方式**: ModerationItem 表中可添加 `UserId` 外键（当前表结构未显式定义，但业务逻辑隐含此关系）

---

## 四、关系图（简化版）

```
                        ┌──────────────────┐
                        │    User (用户)    │
                        │  ┌────────────┐  │
                        │  │ Role字段区分 │  │
                        │  │ Student/    │  │
                        │  │ Teacher/    │  │
                        │  │ Admin       │  │
                        │  └────────────┘  │
                        └────────┬─────────┘
                                 │
           ┌─────────────────────┼─────────────────────┐
           │                     │                     │
           │ 1                   │ m                   │ m
           ▼                     ▼                     ▼
    ┌─────────────┐      ┌─────────────┐      ┌─────────────┐
    │   ◇ 上传    │      │   ◇ 收藏    │      │   ◇ 参与    │
    │   1:n       │      │   m:n       │      │   m:n       │
    └──────┬──────┘      └──────┬──────┘      └──────┬──────┘
           │ n                  │ n                  │ n
           ▼                    │                    │
    ┌──────────────────────────────────────────────────────────┐
    │                      Work (作品)                        │
    └──────────────────────────────────────────────────────────┘
                                    │
                                    │ n
                                    ▼
                            ┌─────────────┐
                            │   ◇ 提交审核 │
                            │    1:n      │
                            └──────┬──────┘
                                   │ n
                                   ▼
                            ┌──────────────────┐
                            │ ModerationItem   │
                            │   (审核项目)      │
                            └──────────────────┘

                        ┌─────────────────────┐
                        │   ◇ 指导 (自关联)   │
                        │    m:n             │
                        │ Teacher ↔ Student   │
                        └─────────────────────┘
```

---

## 五、业务逻辑说明

### 权限控制逻辑

```
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│   Admin     │      │   Teacher   │      │   Student   │
│  (管理员)    │      │   (教师)     │      │   (学生)     │
├─────────────┤      ├─────────────┤      ├─────────────┤
│ 查看所有用户  │      │ 查看自己指导 │      │ 查看自己作品  │
│ 查看所有作品  │      │ 的学生       │      │ 查看公共作品  │
│ 管理审核      │      │ 查看学生作品  │      │ 收藏作品     │
└─────────────┘      └─────────────┘      └─────────────┘
```

### 作品状态流转

```
草稿 → 待审核 → 已发布
  ↓              ↑
删除            ← 审核通过
                ↓
              审核拒绝 → 返回修改
```

### 审核流程

```
用户上传作品 → 自动内容审核 → 风险评估 → 人工审核（如需）→ 发布/拒绝
                        ↓
                    ModerationItem
```

---

## 六、优化前后对比

### 优化前

| 表名 | 用途 | 问题 |
|------|------|------|
| WorkFavorite | 收藏 | 功能重复 |
| UserCollection | 收藏 | 功能重复，与WorkFavorite重复 |

### 优化后

| 表名 | 用途 | 说明 |
|------|------|------|
| WorkFavorite | 收藏 | 唯一收藏表，删除UserCollection |

### 表数量变化

| 阶段 | 实体数 | 关系数 |
|------|--------|--------|
| 优化前 | 7 | 5 |
| 优化后 | 6 | 5 |

---

## 七、建议的数据库调整

### 1. 删除重复表（推荐）

```sql
-- 删除UserCollection表（与WorkFavorite功能重复）
DROP TABLE IF EXISTS UserCollection;
```

### 2. 添加审核提交者字段

```sql
-- 为ModerationItem添加提交者字段
ALTER TABLE ModerationItem 
ADD COLUMN UserId INTEGER REFERENCES User(Id);
```

### 3. 添加唯一约束

```sql
-- 防止重复收藏
CREATE UNIQUE INDEX idx_workfavorite_unique ON WorkFavorite(UserId, WorkId);

-- 防止重复参与
CREATE UNIQUE INDEX idx_workstudent_unique ON WorkStudent(WorkId, StudentId);

-- 防止重复指导关系
CREATE UNIQUE INDEX idx_studentteacher_unique ON StudentTeacher(StudentId, TeacherId);
```

---

## 八、总结

### ✅ 优化要点

1. **消除重复**: 删除 `UserCollection`，保留 `WorkFavorite`
2. **清晰命名**: 关系名称更具业务语义
3. **简化模型**: 减少不必要的表和关系
4. **明确约束**: 添加唯一约束防止重复数据

### 📋 推荐最终实体清单

| 序号 | 实体名称 | 说明 |
|------|---------|------|
| 1 | User | 用户信息 |
| 2 | Work | 作品信息 |
| 3 | WorkFavorite | 作品收藏（用户-作品） |
| 4 | WorkStudent | 团队参与（学生-作品） |
| 5 | StudentTeacher | 师生关系（教师-学生） |
| 6 | ModerationItem | 审核项目 |

### 🔗 推荐关系清单

| 关系 | 类型 | 说明 |
|------|------|------|
| User → Work | 1:n | 上传 |
| User ↔ Work | m:n | 收藏（通过WorkFavorite） |
| User ↔ Work | m:n | 参与（通过WorkStudent） |
| User ↔ User | m:n | 指导（通过StudentTeacher） |
| User → ModerationItem | 1:n | 提交审核 |

---

**优化建议**: 如果业务上确实需要两个收藏表（如"收藏"和"收藏夹"概念不同），建议重新命名以区分其用途，例如 `WorkFavorite`（直接收藏）和 `WorkCollection`（收藏到特定收藏夹）。

**文档版本**: v1.0  
**创建日期**: 2026年5月6日  
**适用系统**: 保定学院数字作品展厅系统
