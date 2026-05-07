<template>
  <div class="notification-container">
    <div class="notification-center">
      <div class="notification-header">
        <h3>通知中心</h3>
        <button class="mark-all-btn" @click="markAllRead">
          全部标为已读
        </button>
      </div>

      <div class="notification-list" v-if="notifications.length > 0">
        <div 
          v-for="notification in notifications" 
          :key="notification.id"
          class="notification-item"
          :class="{ read: notification.isRead }"
          @click="handleNotificationClick(notification)"
        >
          <div class="notification-icon">
            <svg v-if="notification.type === 'work_approval'" class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
            <svg v-else class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
            </svg>
          </div>
          <div class="notification-content">
            <div class="notification-title">{{ notification.title }}</div>
            <div class="notification-body">{{ notification.content }}</div>
            <div class="notification-time">{{ formatTime(notification.createdAt) }}</div>
          </div>
          <div class="notification-status" v-if="!notification.isRead">
            <span class="unread-dot"></span>
          </div>
        </div>
      </div>

      <div class="empty-state" v-else>
        <svg class="empty-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
        </svg>
        <p>暂无通知</p>
      </div>
    </div>

    <!-- 通知详情弹窗 -->
    <div class="notification-detail-modal" v-if="showDetailModal" @click.self="showDetailModal = false">
      <div class="modal-content">
        <div class="modal-header">
          <div class="modal-icon">
            <svg v-if="selectedNotification?.type === 'work_approval'" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
            </svg>
          </div>
          <div class="modal-title">{{ selectedNotification?.title }}</div>
          <button class="close-btn" @click="showDetailModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <p>{{ selectedNotification?.content }}</p>
          <div class="modal-meta">
            <span class="meta-item">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              {{ formatTime(selectedNotification?.createdAt) }}
            </span>
            <span class="meta-item" v-if="selectedNotification?.type">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01"/>
              </svg>
              {{ getTypeLabel(selectedNotification?.type) }}
            </span>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="showDetailModal = false">关闭</button>
          <button 
            v-if="selectedNotification?.workId" 
            class="btn btn-primary" 
            @click="goToWorkDetail"
          >
            查看作品详情
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { notificationApi } from '@/services/api'

export default {
  name: 'NotificationCenter',
  data() {
    return {
      notifications: [],
      showDetailModal: false,
      selectedNotification: null
    }
  },
  mounted() {
    this.loadNotifications()
  },
  methods: {
    async loadNotifications() {
      try {
        const res = await notificationApi.getNotifications()
        this.notifications = res.data || []
      } catch (error) {
        console.error('加载通知失败:', error)
      }
    },

    async markAllRead() {
      try {
        await notificationApi.markAllAsRead()
        this.notifications.forEach(n => n.isRead = true)
        this.$emit('update-count', 0)
      } catch (error) {
        console.error('标记已读失败:', error)
      }
    },

    async handleNotificationClick(notification) {
      if (!notification.isRead) {
        await notificationApi.markAsRead(notification.id)
        notification.isRead = true
        this.$emit('update-count')
      }

      // 显示通知详情弹窗
      this.selectedNotification = notification
      this.showDetailModal = true
    },

    goToWorkDetail() {
      if (this.selectedNotification?.workId) {
        this.showDetailModal = false
        this.$router.push(`/works/${this.selectedNotification.workId}`)
      }
    },

    getTypeLabel(type) {
      const typeMap = {
        work_approval: '作品审核',
        system: '系统通知',
        message: '消息通知'
      }
      return typeMap[type] || type
    },

    formatTime(dateString) {
      const date = new Date(dateString)
      const now = new Date()
      const diff = now.getTime() - date.getTime()
      const minutes = Math.floor(diff / 60000)
      const hours = Math.floor(diff / 3600000)
      const days = Math.floor(diff / 86400000)

      if (minutes < 1) return '刚刚'
      if (minutes < 60) return `${minutes}分钟前`
      if (hours < 24) return `${hours}小时前`
      if (days < 7) return `${days}天前`
      return date.toLocaleDateString()
    }
  }
}
</script>

<style scoped>
.notification-container {
  position: relative;
}

.notification-center {
  width: 380px;
  max-height: 500px;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
  display: flex;
  flex-direction: column;
}

.notification-header {
  padding: 16px 20px;
  border-bottom: 1px solid #f0f0f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.notification-header h3 {
  margin: 0;
  font-size: 15px;
  font-weight: 600;
  color: #1d2129;
}

.mark-all-btn {
  font-size: 13px;
  color: #165dff;
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 4px;
  transition: background 0.2s;
}

.mark-all-btn:hover {
  background: rgba(22, 93, 255, 0.08);
}

.notification-list {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.notification-item {
  display: flex;
  align-items: flex-start;
  padding: 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
}

.notification-item:hover {
  background: #f7f8fa;
}

.notification-item.read {
  opacity: 0.65;
}

.notification-icon {
  width: 36px;
  height: 36px;
  border-radius: 9px;
  background: #f0f5ff;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  margin-right: 12px;
}

.notification-icon .icon {
  width: 18px;
  height: 18px;
  color: #165dff;
}

.notification-content {
  flex: 1;
  min-width: 0;
}

.notification-title {
  font-size: 14px;
  font-weight: 500;
  color: #1d2129;
  margin-bottom: 4px;
}

.notification-body {
  font-size: 13px;
  color: #646a73;
  margin-bottom: 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.notification-time {
  font-size: 12px;
  color: #8f959e;
}

.notification-status {
  flex-shrink: 0;
}

.unread-dot {
  display: block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #f5a623;
}

.empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  color: #8f959e;
}

.empty-icon {
  width: 48px;
  height: 48px;
  margin-bottom: 12px;
}

.empty-state p {
  margin: 0;
  font-size: 14px;
}

/* 通知详情弹窗 */
.notification-detail-modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
}

.modal-content {
  width: 90%;
  max-width: 480px;
  background: #fff;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
}

.modal-header {
  display: flex;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #f0f0f0;
  background: #fafafa;
}

.modal-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  background: #f0f5ff;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 14px;
}

.modal-icon svg {
  width: 20px;
  height: 20px;
  color: #165dff;
}

.modal-title {
  flex: 1;
  font-size: 16px;
  font-weight: 600;
  color: #1d2129;
}

.close-btn {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: transparent;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
}

.close-btn:hover {
  background: #f0f0f0;
}

.close-btn svg {
  width: 18px;
  height: 18px;
  color: #8f959e;
}

.modal-body {
  padding: 24px;
}

.modal-body p {
  margin: 0 0 16px 0;
  font-size: 15px;
  color: #4e5969;
  line-height: 1.7;
}

.modal-meta {
  display: flex;
  gap: 20px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #8f959e;
}

.meta-item svg {
  width: 14px;
  height: 14px;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 16px 24px;
  border-top: 1px solid #f0f0f0;
}

.btn {
  padding: 8px 20px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.btn-secondary {
  background: #f4f5f7;
  color: #4e5969;
}

.btn-secondary:hover {
  background: #e9eaec;
}

.btn-primary {
  background: #165dff;
  color: #fff;
}

.btn-primary:hover {
  background: #0d47a1;
}
</style>
