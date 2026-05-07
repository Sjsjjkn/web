<template>
  <div class="announcement-list">
    <div class="section-header">
      <h3 class="section-title">
        <svg class="title-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/>
        </svg>
        最新公告
      </h3>
      <a href="#" v-if="announcements.length > 3" class="view-more">查看更多</a>
    </div>

    <div class="announcement-items">
      <div 
        v-for="item in displayAnnouncements" 
        :key="item.id" 
        class="announcement-item"
        :class="{ pinned: item.isPinned }"
        @click="handleClick(item)"
      >
        <div class="item-header">
          <span v-if="item.isPinned" class="pinned-badge">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
            置顶
          </span>
          <span class="publish-time">{{ formatTime(item.createdAt) }}</span>
        </div>
        <h4 class="item-title">{{ item.title }}</h4>
        <p class="item-content">{{ truncateContent(item.content) }}</p>
        <div class="item-footer">
          <span class="publisher">{{ item.publisher?.name }}</span>
        </div>
      </div>

      <div class="empty-state" v-if="announcements.length === 0">
        <svg class="empty-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
        </svg>
        <p>暂无公告</p>
      </div>
    </div>
  </div>
</template>

<script>
import { announcementApi } from '@/services/api'

export default {
  name: 'AnnouncementList',
  data() {
    return {
      announcements: [],
      showCount: 3
    }
  },
  computed: {
    displayAnnouncements() {
      return this.announcements.slice(0, this.showCount)
    }
  },
  mounted() {
    this.loadAnnouncements()
  },
  methods: {
    async loadAnnouncements() {
      try {
        const res = await announcementApi.getAnnouncements({
          page: 1,
          limit: 10
        })
        this.announcements = res.data.items || []
      } catch (error) {
        console.error('加载公告失败:', error)
      }
    },

    handleClick(item) {
      this.$emit('click', item)
    },

    truncateContent(content) {
      if (content.length <= 80) return content
      return content.substring(0, 80) + '...'
    },

    formatTime(dateString) {
      const date = new Date(dateString)
      const now = new Date()
      const diff = now.getTime() - date.getTime()
      const days = Math.floor(diff / 86400000)

      if (days === 0) return '今天'
      if (days === 1) return '昨天'
      if (days < 7) return `${days}天前`
      return date.toLocaleDateString('zh-CN', { month: 'short', day: 'numeric' })
    }
  }
}
</script>

<style scoped>
.announcement-list {
  background: #fff;
  border-radius: 16px;
  padding: 20px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f0f0f0;
}

.section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #1d2129;
}

.title-icon {
  width: 20px;
  height: 20px;
  color: #165dff;
}

.view-more {
  font-size: 13px;
  color: #165dff;
  text-decoration: none;
  transition: color 0.2s;
}

.view-more:hover {
  color: #0d47a1;
}

.announcement-items {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.announcement-item {
  padding: 16px;
  border-radius: 12px;
  background: #fafafa;
  cursor: pointer;
  transition: all 0.2s;
}

.announcement-item:hover {
  background: #f0f5ff;
  transform: translateX(4px);
}

.announcement-item.pinned {
  border-left: 4px solid #f5a623;
  background: linear-gradient(90deg, #fffbf0 0%, #fafafa 100%);
}

.item-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 8px;
}

.pinned-badge {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 2px 8px;
  background: #fff7e6;
  border-radius: 4px;
  font-size: 12px;
  color: #d46b08;
}

.pinned-badge svg {
  width: 12px;
  height: 12px;
}

.publish-time {
  font-size: 12px;
  color: #8f959e;
}

.item-title {
  margin: 0 0 8px 0;
  font-size: 15px;
  font-weight: 500;
  color: #1d2129;
  line-height: 1.4;
}

.item-content {
  margin: 0 0 12px 0;
  font-size: 13px;
  color: #646a73;
  line-height: 1.6;
}

.item-footer {
  display: flex;
  justify-content: flex-end;
}

.publisher {
  font-size: 12px;
  color: #8f959e;
}

.empty-state {
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
</style>