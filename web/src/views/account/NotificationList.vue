<template>
  <div class="notification-container">
    <div class="page-header">
      <h2 class="page-title">我的通知</h2>
      <p class="page-subtitle">查看和管理您的所有通知</p>
    </div>

    <!-- 操作栏 -->
    <div class="action-bar">
      <el-button type="primary" @click="markAllRead">
        全部标为已读
      </el-button>
      <el-button @click="loadNotifications">
        刷新
      </el-button>
      <div class="filter-group">
        <el-select v-model="filterType" placeholder="全部类型" class="filter-select">
          <el-option label="全部" value=""></el-option>
          <el-option label="作品审核" value="work_approval"></el-option>
          <el-option label="意见反馈" value="feedback"></el-option>
          <el-option label="系统通知" value="system"></el-option>
          <el-option label="消息通知" value="message"></el-option>
        </el-select>
        <el-select v-model="filterRead" placeholder="全部状态" class="filter-select">
          <el-option label="全部" value=""></el-option>
          <el-option label="未读" value="unread"></el-option>
          <el-option label="已读" value="read"></el-option>
        </el-select>
      </div>
    </div>

    <!-- 统计卡片 -->
    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-value">{{ stats.total }}</div>
        <div class="stat-label">总通知</div>
      </div>
      <div class="stat-card unread">
        <div class="stat-value">{{ stats.unread }}</div>
        <div class="stat-label">未读</div>
      </div>
      <div class="stat-card read">
        <div class="stat-value">{{ stats.read }}</div>
        <div class="stat-label">已读</div>
      </div>
    </div>

    <!-- 通知列表 -->
    <div class="notification-card">
      <div class="card-body">
        <div v-if="notifications.length === 0" class="empty-state">
          <div class="empty-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
            </svg>
          </div>
          <p>暂无通知</p>
        </div>

        <el-timeline v-else>
          <el-timeline-item 
            v-for="notification in notifications" 
            :key="notification.id"
            :color="getTimelineColor(notification.type)"
          >
            <div 
              class="timeline-content" 
              :class="{ read: notification.isRead }"
              @click="showNotificationDetail(notification)"
            >
              <div class="content-header">
                <span class="notification-title">{{ notification.title }}</span>
                <span v-if="!notification.isRead" class="unread-tag">未读</span>
              </div>
              <p class="notification-body">{{ notification.content }}</p>
              <div class="content-footer">
                <span class="notification-time">{{ formatTime(notification.createdAt) }}</span>
                <span class="notification-type">{{ getTypeLabel(notification.type) }}</span>
                <div class="content-actions">
                  <el-button 
                    v-if="!notification.isRead" 
                    size="mini" 
                    @click.stop="markAsRead(notification)"
                  >
                    标为已读
                  </el-button>
                  <el-button 
                    v-if="notification.type === 'work_approval' && notification.workId" 
                    size="mini" 
                    type="primary" 
                    @click.stop="goToWorkDetail(notification.workId)"
                  >
                    查看作品
                  </el-button>
                  <el-button 
                    v-if="notification.type === 'feedback'" 
                    size="mini" 
                    type="primary" 
                    @click.stop="goToFeedback"
                  >
                    处理反馈
                  </el-button>
                </div>
              </div>
            </div>
          </el-timeline-item>
        </el-timeline>
      </div>

      <!-- 分页 -->
      <div v-if="total > pageSize" class="card-footer">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          @current-change="handlePageChange"
          layout="prev, pager, next, ->, total"
        ></el-pagination>
      </div>
    </div>

    <!-- 通知详情弹窗 -->
    <el-dialog 
      title="通知详情" 
      :visible.sync="showDetailModal" 
      width="500px"
    >
      <div class="detail-content">
        <div class="detail-icon" :class="selectedNotification?.type">
          <svg v-if="selectedNotification?.type === 'work_approval'" viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <svg v-else-if="selectedNotification?.type === 'feedback'" viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"/>
          </svg>
          <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
          </svg>
        </div>
        <h3 class="detail-title">{{ selectedNotification?.title }}</h3>
        <p class="detail-body">{{ selectedNotification?.content }}</p>
        <div class="detail-meta">
          <span>{{ formatTime(selectedNotification?.createdAt) }}</span>
          <span>{{ getTypeLabel(selectedNotification?.type) }}</span>
        </div>
      </div>
      <div slot="footer" class="dialog-footer">
        <el-button @click="showDetailModal = false">关闭</el-button>
        <el-button 
          v-if="selectedNotification?.workId && selectedNotification?.type === 'work_approval'" 
          type="primary" 
          @click="goToWorkDetail(selectedNotification.workId)"
        >
          查看作品详情
        </el-button>
        <el-button 
          v-if="selectedNotification?.type === 'feedback' && isAdmin" 
          type="primary" 
          @click="goToFeedback"
        >
          处理反馈
        </el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import { notificationApi } from '../../services/api'

export default {
  name: 'NotificationList',
  data() {
    return {
      notifications: [],
      currentPage: 1,
      pageSize: 10,
      total: 0,
      filterType: '',
      filterRead: '',
      showDetailModal: false,
      selectedNotification: null,
      stats: {
        total: 0,
        unread: 0,
        read: 0
      }
    }
  },
  computed: {
    isAdmin() {
      const user = JSON.parse(localStorage.getItem('userInfo') || '{}')
      return user.role === 'Admin'
    }
  },
  mounted() {
    this.loadNotifications()
  },
  methods: {
    async loadNotifications() {
      try {
        const params = {
          page: this.currentPage,
          limit: this.pageSize
        }
        if (this.filterType) params.type = this.filterType
        if (this.filterRead === 'unread') params.isRead = false
        if (this.filterRead === 'read') params.isRead = true

        const res = await notificationApi.getNotifications(params)
        this.notifications = res.data.items || []
        this.total = res.data.total || 0
        this.calculateStats()
      } catch (error) {
        console.error('加载通知失败:', error)
        this.$message.error('加载通知失败')
      }
    },

    calculateStats() {
      this.stats.total = this.notifications.length
      this.stats.unread = this.notifications.filter(n => !n.isRead).length
      this.stats.read = this.notifications.filter(n => n.isRead).length
    },

    async markAllRead() {
      try {
        await notificationApi.markAllAsRead()
        this.notifications.forEach(n => n.isRead = true)
        this.calculateStats()
        this.$message.success('已全部标为已读')
      } catch (error) {
        this.$message.error('操作失败')
      }
    },

    async markAsRead(notification) {
      try {
        await notificationApi.markAsRead(notification.id)
        notification.isRead = true
        this.calculateStats()
      } catch (error) {
        this.$message.error('操作失败')
      }
    },

    showNotificationDetail(notification) {
      this.selectedNotification = notification
      this.showDetailModal = true
      if (!notification.isRead) {
        this.markAsRead(notification)
      }
    },

    goToWorkDetail(workId) {
      this.showDetailModal = false
      const user = JSON.parse(localStorage.getItem('userInfo') || '{}')
      if (user.role === 'Admin' || user.role === 'Teacher') {
        this.$router.push('/admin/moderation')
      } else {
        this.$router.push(`/works/${workId}`)
      }
    },

    goToFeedback() {
      this.showDetailModal = false
      const user = JSON.parse(localStorage.getItem('userInfo') || '{}')
      if (user.role === 'Admin') {
        this.$router.push('/admin/feedbacks')
      } else {
        this.$router.push('/account/feedback')
      }
    },

    handlePageChange(page) {
      this.currentPage = page
      this.loadNotifications()
    },

    getTypeLabel(type) {
      const typeMap = {
        work_approval: '作品审核',
        feedback: '意见反馈',
        system: '系统通知',
        message: '消息通知'
      }
      return typeMap[type] || type
    },

    getTimelineColor(type) {
      const colorMap = {
        work_approval: '#d48806',
        feedback: '#1890ff',
        system: '#52c41a',
        message: '#722ed1'
      }
      return colorMap[type] || '#999'
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
      return date.toLocaleString('zh-CN')
    }
  },
  watch: {
    filterType() {
      this.currentPage = 1
      this.loadNotifications()
    },
    filterRead() {
      this.currentPage = 1
      this.loadNotifications()
    }
  }
}
</script>

<style scoped>
.notification-container {
  max-width: 900px;
  margin: 0 auto;
  padding: 24px;
}

.page-header {
  margin-bottom: 24px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: #1a1a2e;
  margin: 0 0 8px 0;
}

.page-subtitle {
  color: #666;
  margin: 0;
}

.action-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
}

.filter-group {
  margin-left: auto;
  display: flex;
  gap: 12px;
}

.filter-select {
  width: 120px;
}

.stats-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin-bottom: 20px;
}

.stat-card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  text-align: center;
}

.stat-card .stat-value {
  font-size: 28px;
  font-weight: 600;
  color: #333;
  margin-bottom: 4px;
}

.stat-card .stat-label {
  font-size: 14px;
  color: #999;
}

.stat-card.unread .stat-value { color: #d48806; }
.stat-card.read .stat-value { color: #52c41a; }

.notification-card {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.08);
}

.card-body {
  padding: 20px;
}

.empty-state {
  text-align: center;
  padding: 60px 0;
  color: #999;
}

.empty-icon {
  width: 80px;
  height: 80px;
  margin: 0 auto 16px;
  color: #ccc;
}

.timeline-content {
  padding: 16px;
  background: #fafafa;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.timeline-content:hover {
  background: #f0f5ff;
}

.timeline-content.read {
  opacity: 0.7;
}

.content-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}

.notification-title {
  font-size: 15px;
  font-weight: 500;
  color: #333;
}

.unread-tag {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 4px;
  background: #ff4d4f;
  color: #fff;
}

.notification-body {
  font-size: 14px;
  color: #666;
  margin: 0 0 8px 0;
  line-height: 1.5;
}

.content-footer {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.notification-time {
  font-size: 12px;
  color: #999;
}

.notification-type {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 4px;
  background: #e8f4fd;
  color: #1890ff;
}

.content-actions {
  margin-left: auto;
  display: flex;
  gap: 8px;
}

.card-footer {
  padding: 16px 20px;
  border-top: 1px solid #eee;
}

.detail-content {
  padding: 16px 0;
}

.detail-icon {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  margin-bottom: 16px;
  color: #409eff;
  background: #f0f5ff;
}

.detail-icon svg {
  width: 24px;
  height: 24px;
}

.detail-icon.work_approval { color: #d48806; background: #fff7e6; }
.detail-icon.feedback { color: #1890ff; background: #e6f7ff; }
.detail-icon.system { color: #52c41a; background: #f6ffed; }
.detail-icon.message { color: #722ed1; background: #f9f0ff; }

.detail-title {
  font-size: 18px;
  font-weight: 600;
  margin: 0 0 12px 0;
}

.detail-body {
  font-size: 14px;
  line-height: 1.6;
  color: #666;
  margin: 0 0 12px 0;
}

.detail-meta {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: #999;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
