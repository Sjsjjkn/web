<template>
  <div class="announcement-container">
    <div class="page-header">
      <h2 class="page-title">系统公告</h2>
      <p class="page-subtitle">查看系统发布的最新公告信息</p>
    </div>

    <div class="announcement-card">
      <div class="card-body">
        <div v-if="announcements.length === 0" class="empty-state">
          <div class="empty-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
            </svg>
          </div>
          <p>暂无公告</p>
        </div>

        <div v-else class="announcement-list">
          <div 
            v-for="announcement in announcements" 
            :key="announcement.id"
            class="announcement-item"
            @click="showDetail(announcement)"
          >
            <div class="item-header">
              <div class="item-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
                </svg>
              </div>
              <div class="item-info">
                <h3 class="item-title">{{ announcement.title }}</h3>
                <div class="item-meta">
                  <span class="meta-item">发布时间：{{ formatTime(announcement.createdAt) }}</span>
                  <span class="meta-item" v-if="announcement.isPinned">
                    <span class="priority-tag high">置顶</span>
                  </span>
                </div>
              </div>
            </div>
            <p class="item-content">{{ truncateContent(announcement.content) }}</p>
          </div>
        </div>
      </div>

      <!-- 分页 -->
      <div v-if="total > pageSize" class="card-footer">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          @current-change="handlePageChange"
          layout="prev, pager, next, jumper, ->, total"
        />
      </div>
    </div>

    <!-- 详情弹窗 -->
    <el-dialog
      title="公告详情"
      :visible.sync="showDetailModal"
      width="600px"
    >
      <div v-if="selectedAnnouncement" class="detail-content">
        <div class="detail-header">
          <h3 class="detail-title">{{ selectedAnnouncement.title }}</h3>
          <div class="detail-meta">
            <span>{{ formatTime(selectedAnnouncement.createdAt) }}</span>
            <span v-if="selectedAnnouncement.priority === 'high'" class="priority-tag high">重要公告</span>
          </div>
        </div>
        <div class="detail-body">
          <p>{{ selectedAnnouncement.content }}</p>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import { announcementApi } from '../../services/api'

export default {
  name: 'Announcements',
  data() {
    return {
      announcements: [],
      currentPage: 1,
      pageSize: 10,
      total: 0,
      showDetailModal: false,
      selectedAnnouncement: null
    }
  },
  mounted() {
    this.loadAnnouncements()
  },
  methods: {
    async loadAnnouncements() {
      try {
        const res = await announcementApi.getAnnouncements({
          page: this.currentPage,
          limit: this.pageSize
        })
        this.announcements = res.data.items || []
        this.total = res.data.total || 0
      } catch (error) {
        console.error('加载公告失败:', error)
        this.$message.error('加载公告失败')
      }
    },

    handlePageChange(page) {
      this.currentPage = page
      this.loadAnnouncements()
    },

    showDetail(announcement) {
      this.selectedAnnouncement = announcement
      this.showDetailModal = true
    },

    formatTime(dateString) {
      if (!dateString) return ''
      const date = new Date(dateString)
      return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      })
    },

    truncateContent(content) {
      if (!content) return ''
      return content.length > 100 ? content.substring(0, 100) + '...' : content
    }
  }
}
</script>

<style scoped>
.announcement-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 32px;
}

.page-header {
  margin-bottom: 28px;
  animation: fadeInUp 0.6s var(--ease-out-expo) both;
}

.page-title {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 6px 0;
  letter-spacing: -0.3px;
}

.page-subtitle {
  font-size: 14px;
  color: var(--text-light);
  margin: 0;
}

.announcement-card {
  background: var(--card-bg);
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  animation: fadeInUp 0.6s var(--ease-out-expo) 0.1s both;
}

.card-body {
  padding: 24px;
}

.empty-state {
  text-align: center;
  padding: 64px 0;
  color: var(--text-light);
}

.empty-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 16px;
  color: var(--text-placeholder);
}

.empty-icon svg {
  width: 100%;
  height: 100%;
}

.announcement-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.announcement-item {
  padding: 20px 24px;
  border-radius: var(--radius-md);
  background: var(--app-bg);
  border: 1px solid var(--border-light);
  cursor: pointer;
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.announcement-item:hover {
  background: var(--bg-hover);
  border-color: var(--primary);
  box-shadow: var(--shadow-sm);
  transform: translateY(-1px);
}

.item-header {
  display: flex;
  gap: 14px;
  margin-bottom: 12px;
}

.item-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--primary-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary);
  flex-shrink: 0;
}

.item-icon svg {
  width: 22px;
  height: 22px;
}

.item-info {
  flex: 1;
  min-width: 0;
}

.item-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main);
  margin: 0 0 6px 0;
}

.item-meta {
  display: flex;
  align-items: center;
  gap: 12px;
}

.meta-item {
  font-size: 13px;
  color: var(--text-light);
}

.priority-tag {
  padding: 2px 10px;
  border-radius: var(--radius-full);
  font-size: 11px;
  font-weight: 600;
}

.priority-tag.high {
  background: #FDF0EB;
  color: var(--accent);
}

.item-content {
  font-size: 14px;
  color: var(--text-regular);
  line-height: 1.6;
  margin: 0;
}

.card-footer {
  padding: 16px 24px;
  border-top: 1px solid var(--border-color);
  display: flex;
  justify-content: center;
}

.detail-content {
  padding: 12px 0;
}

.detail-header {
  margin-bottom: 20px;
}

.detail-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 12px 0;
}

.detail-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 13px;
  color: var(--text-light);
}

.detail-body {
  font-size: 15px;
  color: var(--text-regular);
  line-height: 1.8;
}

.detail-body p {
  margin: 0;
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
