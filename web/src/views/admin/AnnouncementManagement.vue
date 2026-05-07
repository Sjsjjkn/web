<template>
  <div class="admin-container">
    <!-- 顶部导航 -->
    <header class="admin-header slide-down">
      <div class="header-content">
        <div class="logo-area">
          <div class="logo-icon">BDU</div>
          <h1 class="system-title">公告管理</h1>
        </div>
        <div class="header-actions">
          <el-button 
            type="primary" 
            round 
            class="add-btn"
            icon="el-icon-plus"
            @click="openCreateModal"
          >
            发布公告
          </el-button>
        </div>
      </div>
    </header>

    <!-- 公告表格区域 -->
    <section class="announcements-section">
      <el-table
        :data="announcements"
        style="width: 100%"
        border
        @row-click="handleRowClick"
      >
        <el-table-column
          prop="id"
          label="ID"
          width="60"
        ></el-table-column>
        <el-table-column
          prop="title"
          label="标题"
          min-width="200"
        ></el-table-column>
        <el-table-column
          label="置顶"
          width="80"
        >
          <template slot-scope="scope">
            <el-tag :type="scope.row.isPinned ? 'danger' : ''">
              {{ scope.row.isPinned ? '是' : '否' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column
          label="状态"
          width="80"
        >
          <template slot-scope="scope">
            <el-tag :type="scope.row.isEnabled ? 'success' : 'warning'">
              {{ scope.row.isEnabled ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column
          prop="publisher.name"
          label="发布者"
          width="120"
        ></el-table-column>
        <el-table-column
          prop="createdAt"
          label="发布时间"
          width="180"
        >
          <template slot-scope="scope">
            {{ formatDate(scope.row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column
          label="操作"
          width="220"
          fixed="right"
        >
          <template slot-scope="scope">
            <el-button 
              type="primary" 
              size="small"
              @click.stop="openEditModal(scope.row)"
            >
              编辑
            </el-button>
            <el-button 
              type="danger" 
              size="small"
              @click.stop="handleDelete(scope.row)"
            >
              删除
            </el-button>
            <el-button 
              :type="scope.row.isEnabled ? 'warning' : 'success'" 
              size="small"
              @click.stop="toggleStatus(scope.row)"
            >
              {{ scope.row.isEnabled ? '禁用' : '启用' }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </section>

    <!-- 分页区域 -->
    <div class="pagination-section">
      <el-pagination
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
        :current-page="pagination.page"
        :page-sizes="[10, 20, 50, 100]"
        :page-size="pagination.limit"
        layout="total, sizes, prev, pager, next, jumper"
        :total="pagination.total"
        background
      ></el-pagination>
    </div>

    <!-- 创建/编辑弹窗 -->
    <el-dialog
      :title="isEdit ? '编辑公告' : '发布公告'"
      :visible.sync="showModal"
      width="600px"
      class="modern-dialog"
    >
      <el-form :model="form" label-width="80px">
        <el-form-item label="标题" required>
          <el-input 
            v-model="form.title" 
            placeholder="请输入公告标题"
            maxlength="200"
          ></el-input>
        </el-form-item>
        <el-form-item label="内容" required>
          <textarea
            v-model="form.content"
            placeholder="请输入公告内容"
            rows="6"
            class="custom-textarea"
          ></textarea>
        </el-form-item>
        <el-form-item label="置顶">
          <el-switch v-model="form.isPinned" active-color="var(--primary)"></el-switch>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button @click="showModal = false" round>取消</el-button>
        <el-button type="primary" @click="saveAnnouncement" round>
          {{ isEdit ? '保存修改' : '发布公告' }}
        </el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import { announcementApi } from '@/services/api'

export default {
  name: 'AnnouncementManagement',
  data() {
    return {
      announcements: [],
      showModal: false,
      isEdit: false,
      form: {
        id: null,
        title: '',
        content: '',
        isPinned: false
      },
      pagination: {
        page: 1,
        limit: 10,
        total: 0
      }
    }
  },
  mounted() {
    this.loadAnnouncements()
  },
  methods: {
    async loadAnnouncements() {
      try {
        const res = await announcementApi.getAnnouncements({
          page: this.pagination.page,
          limit: this.pagination.limit
        })
        this.announcements = res.data.items || []
        this.pagination.total = res.data.total || 0
      } catch (error) {
        console.error('加载公告失败:', error)
        this.$message.error('加载公告失败')
      }
    },

    openCreateModal() {
      this.isEdit = false
      this.resetForm()
      this.showModal = true
    },

    openEditModal(row) {
      this.isEdit = true
      this.form = {
        id: row.id,
        title: row.title,
        content: row.content,
        isPinned: row.isPinned
      }
      this.showModal = true
    },

    resetForm() {
      this.form = {
        id: null,
        title: '',
        content: '',
        isPinned: false
      }
    },

    async saveAnnouncement() {
      if (!this.form.title.trim() || !this.form.content.trim()) {
        this.$message.warning('请填写完整信息')
        return
      }

      try {
        if (this.isEdit) {
          await announcementApi.updateAnnouncement(this.form.id, {
            title: this.form.title,
            content: this.form.content,
            isPinned: this.form.isPinned
          })
          this.$message.success('公告更新成功')
        } else {
          await announcementApi.createAnnouncement({
            title: this.form.title,
            content: this.form.content,
            isPinned: this.form.isPinned
          })
          this.$message.success('公告发布成功')
        }
        this.showModal = false
        this.loadAnnouncements()
      } catch (error) {
        console.error('保存公告失败:', error)
        this.$message.error('保存公告失败')
      }
    },

    async handleDelete(row) {
      this.$confirm('确定要删除这条公告吗？', '提示', {
        type: 'warning'
      }).then(async () => {
        try {
          await announcementApi.deleteAnnouncement(row.id)
          this.$message.success('删除成功')
          this.loadAnnouncements()
        } catch (error) {
          console.error('删除公告失败:', error)
          this.$message.error('删除失败')
        }
      }).catch(() => {
        // 用户取消
      })
    },

    async toggleStatus(row) {
      try {
        await announcementApi.updateAnnouncement(row.id, {
          isEnabled: !row.isEnabled
        })
        row.isEnabled = !row.isEnabled
        this.$message.success(row.isEnabled ? '已启用' : '已禁用')
      } catch (error) {
        console.error('切换状态失败:', error)
        this.$message.error('操作失败')
      }
    },

    handleRowClick(row) {
      // 可以添加点击查看详情的逻辑
    },

    handleSizeChange(val) {
      this.pagination.limit = val
      this.loadAnnouncements()
    },

    handleCurrentChange(val) {
      this.pagination.page = val
      this.loadAnnouncements()
    },

    formatDate(dateStr) {
      const date = new Date(dateStr)
      return date.toLocaleDateString('zh-CN')
    }
  }
}
</script>

<style scoped>
/* 基础设置 */
.admin-container {
  min-height: 100vh;
  background-color: #f8fafc;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  overflow-x: hidden;
}

/* 1. 顶部导航 (滑入动画) */
.admin-header {
  display: flex;
  justify-content: center;
  padding: 0 40px;
  height: 64px;
  background-color: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(0,0,0,0.05);
  position: sticky;
  top: 0;
  z-index: 100;
}

.slide-down {
  animation: slideDown 0.5s ease-out;
}

.header-content {
  width: 100%;
  max-width: 1200px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo-area { 
  display: flex; 
  align-items: center; 
  gap: 12px; 
}

.logo-icon { 
  background: var(--primary); 
  color: white; 
  font-weight: bold; 
  padding: 4px 8px; 
  border-radius: 6px; 
}

.system-title { 
  font-size: 20px; 
  font-weight: 600; 
  color: #1a1a1a; 
  margin: 0; 
}

/* 2. 添加按钮悬停动画 */
.add-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.add-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(45, 138, 110, 0.3);
}

/* 3. 公告表格区域 */
.announcements-section {
  max-width: 1200px;
  margin: 40px auto;
  padding: 0 24px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.02);
  overflow: hidden;
}

::v-deep .el-table {
  border-radius: 12px 12px 0 0;
  overflow: hidden;
}

::v-deep .el-table__header-wrapper {
  background: #f8fafc;
}

::v-deep .el-table th {
  background: #f8fafc !important;
  font-weight: 600;
  color: #333;
}

::v-deep .el-table tr:hover {
  background: #f8fafc !important;
}

/* 分页区域 */
.pagination-section {
  max-width: 1200px;
  margin: 0 auto 40px;
  padding: 0 24px;
  display: flex;
  justify-content: center;
}

/* 对话框样式 */
::v-deep .modern-dialog .el-dialog {
  border-radius: 12px;
  overflow: visible !important;
}

::v-deep .modern-dialog .el-dialog__header {
  background: #f8fafc;
  padding: 20px 24px;
  border-bottom: 1px solid #e4e7ed;
}

::v-deep .modern-dialog .el-dialog__body {
  padding: 24px;
  display: block !important;
  overflow: visible !important;
}

::v-deep .modern-dialog .el-form-item {
  margin-bottom: 20px;
}

::v-deep .modern-dialog .el-textarea {
  display: block !important;
  width: 100% !important;
  min-height: 150px !important;
}

::v-deep .modern-dialog .el-textarea__inner {
  width: 100% !important;
  min-height: 150px !important;
  height: 150px !important;
  border: 1px solid #dcdfe6 !important;
  border-radius: 4px !important;
  padding: 8px !important;
  box-sizing: border-box !important;
  visibility: visible !important;
  opacity: 1 !important;
}

/* 自定义textarea样式 */
.custom-textarea {
  width: 100% !important;
  min-height: 150px !important;
  height: 150px !important;
  border: 1px solid #dcdfe6 !important;
  border-radius: 4px !important;
  padding: 8px !important;
  box-sizing: border-box !important;
  font-size: 14px !important;
  line-height: 1.5 !important;
  resize: vertical !important;
  visibility: visible !important;
  opacity: 1 !important;
  display: block !important;
}

.custom-textarea:focus {
  outline: none !important;
  border-color: var(--primary) !important;
  box-shadow: 0 0 0 2px rgba(45, 138, 110, 0.2) !important;
}

.dialog-footer {
  text-align: right;
  padding: 12px 24px;
  border-top: 1px solid #e4e7ed;
}

/* 关键帧定义 */
@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@keyframes fadeInUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}
</style>