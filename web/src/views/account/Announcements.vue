<template>
  <div class="page-container">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-inner">
        <div class="header-left">
          <div class="header-icon-wrap">
            <svg class="header-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
          </div>
          <div class="header-text">
            <h1 class="page-title">系统公告</h1>
            <p class="page-subtitle">及时了解系统最新动态和重要通知</p>
          </div>
        </div>
        <div class="header-right">
          <span class="count-badge">{{ total }} 条公告</span>
        </div>
      </div>
    </div>

    <!-- 公告列表容器 -->
    <div class="content-wrapper">
      <!-- 置顶公告 -->
      <div v-if="pinnedAnnouncements.length > 0" class="section-block">
        <div class="section-title">
          <span class="title-dot"></span>
          <span class="title-text">置顶公告</span>
        </div>
        <div class="pinned-list">
          <div 
            v-for="announcement in pinnedAnnouncements" 
            :key="'pinned-' + announcement.id"
            class="pinned-card"
            @click="showDetail(announcement)"
          >
            <div class="pinned-badge">
              <span class="badge-text">置顶</span>
              <span v-if="announcement.priority === 'high'" class="badge-urgent">紧急</span>
            </div>
            <div class="pinned-content">
              <h3 class="pinned-title">{{ announcement.title }}</h3>
              <p class="pinned-preview">{{ truncateContent(announcement.content) }}</p>
              <div class="pinned-meta">
                <span class="meta-time">{{ formatTime(announcement.createdAt) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 最新公告 -->
      <div class="section-block">
        <div class="section-title">
          <span class="title-dot"></span>
          <span class="title-text">最新公告</span>
        </div>
        
        <!-- 空状态 -->
        <div v-if="normalAnnouncements.length === 0 && pinnedAnnouncements.length === 0" class="empty-state">
          <div class="empty-icon">📭</div>
          <h3>暂无公告</h3>
          <p>系统暂无新的公告信息</p>
        </div>

        <!-- 公告列表 -->
        <div v-else-if="normalAnnouncements.length > 0" class="announcement-list">
          <div 
            v-for="(announcement, index) in normalAnnouncements" 
            :key="announcement.id"
            class="announcement-item"
            :style="{ animationDelay: index * 0.06 + 's' }"
            @click="showDetail(announcement)"
          >
            <div class="item-time">
              <span class="time-day">{{ getDay(announcement.createdAt) }}</span>
              <span class="time-month">{{ getMonth(announcement.createdAt) }}</span>
            </div>
            <div class="item-content">
              <div class="item-header">
                <h3 class="item-title">{{ announcement.title }}</h3>
                <span v-if="announcement.priority === 'high'" class="urgent-indicator"></span>
              </div>
              <p class="item-preview">{{ truncateContent(announcement.content) }}</p>
              <span class="item-time-text">{{ formatTime(announcement.createdAt) }}</span>
            </div>
            <div class="item-arrow">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      <!-- 分页 -->
      <div v-if="total > pageSize" class="pagination-wrapper">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          @current-change="handlePageChange"
          layout="prev, pager, next, jumper, ->, total"
          background
          class="pagination"
        />
      </div>
    </div>

    <!-- 详情弹窗 -->
    <div class="modal-overlay" v-if="showDetailModal" @click.self="showDetailModal = false">
      <div class="modal-content">
        <div class="modal-header">
          <div class="modal-title">
            <div class="modal-icon-wrap">
              <svg class="modal-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <h2>{{ selectedAnnouncement?.title }}</h2>
          </div>
          <button class="modal-close" @click="showDetailModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <div class="modal-meta">
            <span>{{ formatTime(selectedAnnouncement?.createdAt) }}</span>
            <span v-if="selectedAnnouncement?.priority === 'high'" class="meta-urgent">重要公告</span>
          </div>
          <div class="modal-text">
            {{ selectedAnnouncement?.content }}
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn-close" @click="showDetailModal = false">关闭</button>
        </div>
      </div>
    </div>
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
  computed: {
    pinnedAnnouncements() {
      return this.announcements.filter(a => a.isPinned)
    },
    normalAnnouncements() {
      return this.announcements.filter(a => !a.isPinned)
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

    getDay(dateString) {
      if (!dateString) return ''
      const date = new Date(dateString)
      return date.getDate()
    },

    getMonth(dateString) {
      if (!dateString) return ''
      const date = new Date(dateString)
      const months = ['1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月']
      return months[date.getMonth()]
    },

    truncateContent(content) {
      if (!content) return ''
      return content.length > 120 ? content.substring(0, 120) + '...' : content
    }
  }
}
</script>

<style scoped>
.page-container {
  min-height: 100vh;
  background: var(--bg-page, #F8F9F5);
}

/* 页面头部 */
.page-header {
  padding: 0;
  margin-bottom: 20px;
}

.header-inner {
  max-width: 948px;
  margin: 0 auto;
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: linear-gradient(135deg, #4AAB8A 0%, #3D9A7B 100%);
  border-radius: 16px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-icon-wrap {
  width: 48px;
  height: 48px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.header-icon {
  width: 24px;
  height: 24px;
  color: white;
}

.header-text {
  display: flex;
  flex-direction: column;
}

.page-title {
  font-size: 26px;
  font-weight: 700;
  color: white;
  margin: 0;
}

.page-subtitle {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.85);
  margin: 4px 0 0;
}

.count-badge {
  padding: 8px 16px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  font-size: 13px;
  font-weight: 500;
  color: white;
}

/* 内容容器 */
.content-wrapper {
  max-width: 948px;
  margin: 0 auto;
  padding: 0 24px;
  position: relative;
  z-index: 10;
}

/* 区块 */
.section-block {
  background: white;
  border-radius: 16px;
  border: 1px solid var(--border-color, #E8E2D8);
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: var(--shadow-sm, 0 2px 8px rgba(0,0,0,.04));
}

.section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
}

.title-dot {
  width: 4px;
  height: 16px;
  background: var(--primary, #2D8A6E);
  border-radius: 2px;
}

.title-text {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
}

/* 置顶公告 */
.pinned-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.pinned-card {
  background: linear-gradient(135deg, #FFFDF6 0%, #FFF8E7 100%);
  border: 1px solid rgba(255, 179, 0, 0.3);
  border-radius: 12px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.pinned-card:hover {
  background: linear-gradient(135deg, #FFF9E6 0%, #FFF3D0 100%);
  box-shadow: 0 4px 12px rgba(255, 179, 0, 0.1);
}

.pinned-badge {
  display: flex;
  gap: 8px;
  margin-bottom: 10px;
}

.badge-text {
  padding: 3px 10px;
  background: var(--accent, #C8AA6E);
  color: white;
  font-size: 11px;
  font-weight: 600;
  border-radius: 4px;
}

.badge-urgent {
  padding: 3px 10px;
  background: #FF4D4F;
  color: white;
  font-size: 11px;
  font-weight: 600;
  border-radius: 4px;
}

.pinned-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 8px;
}

.pinned-preview {
  font-size: 14px;
  color: var(--text-regular, #555555);
  line-height: 1.6;
  margin: 0 0 10px;
}

.pinned-meta {
  display: flex;
  justify-content: flex-end;
}

.meta-time {
  font-size: 12px;
  color: var(--text-light, #888888);
}

/* 公告列表 */
.announcement-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.announcement-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  background: var(--bg-page, #F8F9F5);
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
  animation: slideIn 0.4s ease both;
}

.announcement-item:hover {
  background: white;
  border: 1px solid var(--primary, #2D8A6E);
  box-shadow: 0 4px 12px rgba(45, 138, 110, 0.1);
}

.item-time {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 52px;
  height: 52px;
  background: linear-gradient(135deg, var(--primary-bg, #EDF5F0) 0%, #E0F0EA 100%);
  border-radius: 10px;
  flex-shrink: 0;
}

.time-day {
  font-size: 19px;
  font-weight: 700;
  color: var(--primary, #2D8A6E);
  line-height: 1;
}

.time-month {
  font-size: 11px;
  color: var(--primary, #2D8A6E);
  margin-top: 2px;
}

.item-content {
  flex: 1;
  min-width: 0;
}

.item-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}

.item-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.urgent-indicator {
  width: 8px;
  height: 8px;
  background: #FF4D4F;
  border-radius: 50%;
  flex-shrink: 0;
}

.item-preview {
  font-size: 13px;
  color: var(--text-light, #888888);
  line-height: 1.5;
  margin: 0 0 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item-time-text {
  font-size: 12px;
  color: var(--text-placeholder, #AAAAAA);
}

.item-arrow {
  width: 20px;
  height: 20px;
  color: var(--text-placeholder, #AAAAAA);
  flex-shrink: 0;
  transition: all 0.3s ease;
}

.announcement-item:hover .item-arrow {
  color: var(--primary, #2D8A6E);
  transform: translateX(4px);
}

.item-arrow svg {
  width: 100%;
  height: 100%;
}

/* 空状态 */
.empty-state {
  text-align: center;
  padding: 50px 20px;
}

.empty-icon {
  font-size: 44px;
  margin-bottom: 14px;
  opacity: 0.4;
}

.empty-state h3 {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 6px;
}

.empty-state p {
  font-size: 13px;
  color: var(--text-light, #888888);
  margin: 0;
}

/* 分页 */
.pagination-wrapper {
  display: flex;
  justify-content: center;
  padding: 20px 0;
}

.pagination {
  --el-pagination-button-bg-color: white;
  --el-pagination-button-border-color: var(--border-color, #E8E2D8);
  --el-pagination-button-text-color: var(--text-regular, #555555);
  --el-pagination-button-active-bg-color: var(--primary, #2D8A6E);
  --el-pagination-button-active-text-color: white;
}

/* 弹窗 */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.55);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  animation: fadeIn 0.25s ease;
  padding: 20px;
}

.modal-content {
  width: 100%;
  max-width: 560px;
  background: white;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 25px 60px rgba(0, 0, 0, 0.2);
  animation: slideUp 0.35s cubic-bezier(0.16, 1, 0.3, 1);
  position: relative;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px;
  background: linear-gradient(135deg, #4AAB8A 0%, #3D9A7B 100%);
}

.modal-title {
  display: flex;
  align-items: center;
  gap: 12px;
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
}

.modal-icon-wrap {
  width: 36px;
  height: 36px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-icon {
  width: 18px;
  height: 18px;
  color: white;
}

.modal-title h2 {
  font-size: 18px;
  font-weight: 600;
  color: white;
  margin: 0;
}

.modal-close {
  width: 34px;
  height: 34px;
  border: none;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: rgba(255, 255, 255, 0.8);
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-close:hover {
  background: rgba(255, 255, 255, 0.25);
  color: white;
  transform: scale(1.1);
}

.modal-close svg {
  width: 16px;
  height: 16px;
}

.modal-body {
  padding: 24px;
}

.modal-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--border-color, #E8E2D8);
}

.modal-meta span {
  font-size: 13px;
  color: var(--text-light, #888888);
}

.meta-urgent {
  padding: 4px 12px;
  background: linear-gradient(135deg, #FFF1F0 0%, #FFEBE9 100%);
  color: #D93026;
  font-size: 12px;
  font-weight: 600;
  border-radius: 6px;
  border: 1px solid rgba(217, 48, 38, 0.2);
}

.modal-text {
  font-size: 15px;
  line-height: 1.9;
  color: var(--text-regular, #555555);
  white-space: pre-wrap;
  background: var(--bg-page, #F8F9F5);
  padding: 20px;
  border-radius: 12px;
  min-height: 120px;
}

.modal-footer {
  padding: 20px 24px;
  display: flex;
  justify-content: center;
}

.btn-close {
  padding: 12px 36px;
  background: linear-gradient(135deg, #4AAB8A 0%, #3D9A7B 100%);
  color: white;
  border: none;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 4px 14px rgba(74, 171, 138, 0.3);
}

.btn-close:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(74, 171, 138, 0.4);
}

.btn-close:active {
  transform: translateY(0);
}

/* 动画 */
@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateX(-12px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(16px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 响应式 */
@media (max-width: 640px) {
  .header-inner {
    flex-direction: column;
    gap: 16px;
    text-align: center;
  }

  .header-left {
    flex-direction: column;
  }

  .page-title {
    font-size: 24px;
  }

  .content-wrapper {
    padding: 0 16px;
  }

  .section-block {
    padding: 16px;
  }

  .announcement-item {
    padding: 14px;
    gap: 12px;
  }

  .item-time {
    width: 48px;
    height: 48px;
  }

  .time-day {
    font-size: 18px;
  }

  .modal-content {
    width: 95%;
    margin: 16px;
  }
}
</style>