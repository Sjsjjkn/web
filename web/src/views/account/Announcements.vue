<template>
  <div class="announcement-panel">
    <!-- 顶栏操作区 -->
    <div class="panel-header">
      <div class="header-left">
        <svg class="header-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
        </svg>
        <span class="header-title">教务与系统公告</span>
      </div>
      <div class="header-right">
        <button class="mark-all-btn" @click="markAllRead">全部标为已读</button>
      </div>
    </div>

    <!-- 主体区域 -->
    <div class="panel-body">
      <!-- 左侧列表区 -->
      <div class="list-panel">
        <!-- 搜索框 -->
        <div class="search-box">
          <svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <circle cx="11" cy="11" r="8" />
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-4.35-4.35" />
          </svg>
          <input 
            type="text" 
            v-model="searchQuery" 
            placeholder="搜索公告..." 
            class="search-input"
          />
        </div>

        <!-- 公告列表 -->
        <div class="notice-list">
          <button
            v-for="notice in filteredNotices"
            :key="notice.id"
            @click="selectNotice(notice)"
            :class="['notice-item', { active: selectedNotice?.id === notice.id }]"
          >
            <!-- 未读提示 -->
            <div v-if="notice.unread" class="unread-dot"></div>
            
            <div class="notice-content">
              <div class="notice-header">
                <h4 class="notice-title">{{ notice.title }}</h4>
                <span class="notice-date">{{ notice.date }}</span>
              </div>
              <p class="notice-excerpt">{{ notice.excerpt }}</p>
              <div class="notice-footer">
                <span :class="['category-tag', getCategoryClass(notice.type)]">
                  {{ notice.category }}
                </span>
                <svg v-if="notice.attachments?.length" class="attachment-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
              </div>
            </div>
          </button>
        </div>
      </div>

      <!-- 右侧阅读区 -->
      <div class="detail-panel">
        <div v-if="selectedNotice" class="detail-content">
          <!-- 阅读区头部 -->
          <div class="detail-header">
            <div class="detail-meta">
              <span :class="['category-tag', getCategoryClass(selectedNotice.type)]">
                {{ selectedNotice.category }}
              </span>
              <span class="detail-date">{{ selectedNotice.date }}</span>
            </div>
            <h1 class="detail-title">{{ selectedNotice.title }}</h1>
            <div class="detail-sender">
              <div class="sender-avatar">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
              </div>
              <div class="sender-info">
                <div class="sender-name">{{ selectedNotice.sender }}</div>
                <div class="sender-target">发给：全院导师、全体学生</div>
              </div>
            </div>
          </div>

          <!-- 阅读区正文 -->
          <div class="detail-body">
            <div v-html="selectedNotice.content"></div>
          </div>

          <!-- 附件区 -->
          <div v-if="selectedNotice.attachments?.length" class="detail-attachments">
            <h4 class="attachments-title">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              附件 ({{ selectedNotice.attachments.length }})
            </h4>
            <div class="attachments-list">
              <button 
                v-for="(file, idx) in selectedNotice.attachments" 
                :key="idx" 
                class="attachment-item"
              >
                <div class="file-icon">PDF</div>
                <div class="file-info">
                  <div class="file-name">{{ file.name }}</div>
                  <div class="file-size">{{ file.size }}</div>
                </div>
                <svg class="download-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                </svg>
              </button>
            </div>
          </div>
        </div>

        <!-- 空状态 -->
        <div v-else class="empty-state">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
          </svg>
          <p>选择左侧通知查看详情</p>
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
      notices: [],
      selectedNotice: null,
      searchQuery: ''
    }
  },
  computed: {
    filteredNotices() {
      if (!this.searchQuery) return this.notices
      const query = this.searchQuery.toLowerCase()
      return this.notices.filter(n => 
        n.title.toLowerCase().includes(query) ||
        n.excerpt.toLowerCase().includes(query) ||
        n.category.toLowerCase().includes(query)
      )
    }
  },
  mounted() {
    this.loadNotices()
  },
  methods: {
    async loadNotices() {
      try {
        const res = await announcementApi.getAnnouncements({ page: 1, limit: 50 })
        const items = res.data.items || []
        
        this.notices = items.map(item => ({
          id: item.id,
          title: item.title,
          excerpt: item.content ? (item.content.length > 100 ? item.content.substring(0, 100) + '...' : item.content) : '',
          sender: item.publisher?.name || '系统通知',
          date: this.formatDate(item.createdAt),
          category: this.getCategory(item.type),
          type: this.getType(item.type),
          unread: item.isPinned || false,
          content: item.content,
          attachments: item.attachments ? JSON.parse(item.attachments) : []
        }))

        // 默认选中第一条
        if (this.notices.length > 0) {
          this.selectedNotice = this.notices[0]
        }
      } catch (error) {
        console.error('加载公告失败:', error)
        this.$message.error('加载公告失败')
      }
    },

    selectNotice(notice) {
      this.selectedNotice = notice
      notice.unread = false
    },

    markAllRead() {
      this.notices.forEach(n => n.unread = false)
      this.$message.success('已全部标为已读')
    },

    formatDate(dateString) {
      if (!dateString) return ''
      const date = new Date(dateString)
      const now = new Date()
      const diff = now - date
      const days = Math.floor(diff / (1000 * 60 * 60 * 24))
      
      if (days === 0) {
        const hours = Math.floor(diff / (1000 * 60 * 60))
        if (hours === 0) {
          const minutes = Math.floor(diff / (1000 * 60))
          return `${minutes}分钟前`
        }
        return `${hours}小时前`
      } else if (days === 1) {
        return '昨天'
      } else if (days < 7) {
        return `${days}天前`
      } else {
        return date.toLocaleDateString('zh-CN', { month: '2-digit', day: '2-digit' })
      }
    },

    getCategory(type) {
      const categories = {
        event: '展会通知',
        system: '系统升级',
        academic: '教务安排',
        important: '重要通报'
      }
      return categories[type] || '系统通知'
    },

    getType(type) {
      const types = {
        event: 'event',
        system: 'system',
        academic: 'academic',
        important: 'academic'
      }
      return types[type] || 'system'
    },

    getCategoryClass(type) {
      const classes = {
        event: 'category-purple',
        system: 'category-blue',
        academic: 'category-green'
      }
      return classes[type] || 'category-gray'
    }
  }
}
</script>

<style scoped>
.announcement-panel {
  display: flex;
  flex-direction: column;
  width: 100%;
  height: calc(100vh - 64px);
  max-width: 1400px;
  margin: 0 auto;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

/* 顶栏 */
.panel-header {
  height: 56px;
  border-bottom: 1px solid #e5e7eb;
  background: #f9fafb;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  flex-shrink: 0;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 10px;
}

.header-icon {
  width: 20px;
  height: 20px;
  color: #6b7280;
}

.header-title {
  font-size: 15px;
  font-weight: 600;
  color: #111827;
}

.mark-all-btn {
  font-size: 13px;
  font-weight: 500;
  color: #6b7280;
  background: transparent;
  border: none;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}

.mark-all-btn:hover {
  color: #111827;
  background: #f3f4f6;
}

/* 主体区域 */
.panel-body {
  flex: 1;
  display: flex;
  overflow: hidden;
}

/* 左侧列表区 */
.list-panel {
  width: 380px;
  border-right: 1px solid #e5e7eb;
  background: #f9fafb;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
}

.search-box {
  padding: 12px;
  background: #fff;
  border-bottom: 1px solid #e5e7eb;
  position: relative;
}

.search-icon {
  position: absolute;
  left: 20px;
  top: 50%;
  transform: translateY(-50%);
  width: 16px;
  height: 16px;
  color: #9ca3af;
}

.search-input {
  width: 100%;
  padding: 10px 12px 10px 40px;
  font-size: 13px;
  background: #f3f4f6;
  border: none;
  border-radius: 8px;
  outline: none;
  transition: all 0.2s;
}

.search-input:focus {
  background: #fff;
  box-shadow: 0 0 0 2px #3b82f6, 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.notice-list {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.notice-item {
  width: 100%;
  text-align: left;
  padding: 14px;
  margin-bottom: 6px;
  background: #fff;
  border: 1px solid transparent;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
}

.notice-item:hover {
  background: #f9fafb;
  border-color: #d1d5db;
}

.notice-item.active {
  background: #eff6ff;
  border-color: #93c5fd;
}

.unread-dot {
  position: absolute;
  top: 18px;
  left: 14px;
  width: 6px;
  height: 6px;
  background: #3b82f6;
  border-radius: 50%;
  box-shadow: 0 0 4px rgba(59, 130, 246, 0.5);
}

.notice-content {
  padding-left: 14px;
}

.notice-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 8px;
  margin-bottom: 6px;
}

.notice-title {
  font-size: 14px;
  font-weight: 600;
  color: #111827;
  margin: 0;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.notice-item.active .notice-title {
  color: #1d4ed8;
  font-weight: 700;
}

.notice-date {
  font-size: 11px;
  color: #9ca3af;
  white-space: nowrap;
  flex-shrink: 0;
}

.notice-excerpt {
  font-size: 12px;
  color: #6b7280;
  line-height: 1.5;
  margin: 0 0 10px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.notice-item.active .notice-excerpt {
  color: #374151;
}

.notice-footer {
  display: flex;
  align-items: center;
  gap: 8px;
}

.category-tag {
  font-size: 10px;
  font-weight: 600;
  padding: 3px 8px;
  border-radius: 4px;
  border: 1px solid;
}

.category-purple {
  background: #f5f3ff;
  color: #7c3aed;
  border-color: #ddd6fe;
}

.category-blue {
  background: #eff6ff;
  color: #2563eb;
  border-color: #bfdbfe;
}

.category-green {
  background: #ecfdf5;
  color: #059669;
  border-color: #a7f3d0;
}

.category-gray {
  background: #f3f4f6;
  color: #6b7280;
  border-color: #d1d5db;
}

.attachment-icon {
  width: 14px;
  height: 14px;
  color: #9ca3af;
}

/* 右侧阅读区 */
.detail-panel {
  flex: 1;
  background: #fff;
  overflow-y: auto;
}

.detail-content {
  max-width: 700px;
  margin: 0 auto;
  padding: 48px 40px;
}

.detail-header {
  padding-bottom: 32px;
  border-bottom: 1px solid #f3f4f6;
  margin-bottom: 32px;
}

.detail-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.detail-date {
  font-size: 13px;
  color: #9ca3af;
}

.detail-title {
  font-size: 28px;
  font-weight: 800;
  color: #111827;
  line-height: 1.3;
  margin: 0 0 20px;
}

.detail-sender {
  display: flex;
  align-items: center;
  gap: 12px;
}

.sender-avatar {
  width: 40px;
  height: 40px;
  background: #f3f4f6;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6b7280;
}

.sender-avatar svg {
  width: 18px;
  height: 18px;
}

.sender-info {
  display: flex;
  flex-direction: column;
}

.sender-name {
  font-size: 14px;
  font-weight: 600;
  color: #111827;
}

.sender-target {
  font-size: 12px;
  color: #9ca3af;
}

.detail-body {
  font-size: 15px;
  line-height: 1.8;
  color: #374151;
}

.detail-body h3 {
  font-size: 18px;
  font-weight: 700;
  color: #111827;
  margin: 24px 0 16px;
}

.detail-body p {
  margin: 12px 0;
}

.detail-body ul {
  margin: 12px 0;
  padding-left: 24px;
}

.detail-body li {
  margin: 8px 0;
}

.detail-body strong {
  color: #111827;
  font-weight: 600;
}

/* 附件区 */
.detail-attachments {
  margin-top: 40px;
  padding-top: 32px;
  border-top: 1px solid #f3f4f6;
}

.attachments-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 600;
  color: #111827;
  margin: 0 0 16px;
}

.attachments-title svg {
  width: 16px;
  height: 16px;
  color: #6b7280;
}

.attachments-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.attachment-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.attachment-item:hover {
  border-color: #d1d5db;
  background: #f9fafb;
}

.file-icon {
  width: 40px;
  height: 40px;
  background: #fef2f2;
  border: 1px solid #fee2e2;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 700;
  color: #dc2626;
}

.attachment-item:hover .file-icon {
  background: #fee2e2;
}

.file-info {
  display: flex;
  flex-direction: column;
  padding-right: 12px;
}

.file-name {
  font-size: 13px;
  font-weight: 500;
  color: #111827;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 200px;
}

.file-size {
  font-size: 11px;
  color: #9ca3af;
}

.download-icon {
  width: 18px;
  height: 18px;
  color: #9ca3af;
  transition: color 0.2s;
}

.attachment-item:hover .download-icon {
  color: #3b82f6;
}

/* 空状态 */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #9ca3af;
}

.empty-state svg {
  width: 64px;
  height: 64px;
  margin-bottom: 16px;
  opacity: 0.3;
}

.empty-state p {
  font-size: 14px;
}

/* 滚动条样式 */
.notice-list::-webkit-scrollbar {
  width: 6px;
}

.notice-list::-webkit-scrollbar-track {
  background: transparent;
}

.notice-list::-webkit-scrollbar-thumb {
  background: #d1d5db;
  border-radius: 3px;
}

.notice-list::-webkit-scrollbar-thumb:hover {
  background: #9ca3af;
}

.detail-panel::-webkit-scrollbar {
  width: 8px;
}

.detail-panel::-webkit-scrollbar-track {
  background: transparent;
}

.detail-panel::-webkit-scrollbar-thumb {
  background: #e5e7eb;
  border-radius: 4px;
}

.detail-panel::-webkit-scrollbar-thumb:hover {
  background: #d1d5db;
}
</style>