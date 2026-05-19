<template>
  <div class="announcement-page">
    <!-- 背景装饰 -->
    <div class="page-bg">
      <div class="bg-shape bg-shape-1"></div>
      <div class="bg-shape bg-shape-2"></div>
    </div>

    <div class="page-content" v-loading="loading">
      <!-- 头部 -->
      <section class="page-hero">
        <div class="hero-content">
          <h1 class="hero-title">
            <i class="el-icon-bell"></i> 系统公告
          </h1>
          <p class="hero-subtitle">了解平台最新动态与通知</p>
        </div>
        <div class="hero-search">
          <el-input
            v-model="searchKeyword"
            placeholder="搜索公告标题..."
            prefix-icon="el-icon-search"
            size="medium"
            clearable
            class="search-input"
            @clear="loadAnnouncements"
            @keyup.enter.native="loadAnnouncements"
          >
            <el-button
              slot="append"
              icon="el-icon-search"
              @click="loadAnnouncements"
              class="search-btn"
            ></el-button>
          </el-input>
        </div>
      </section>

      <!-- 公告列表 -->
      <section class="announcement-section">
        <div class="announcement-grid" v-if="announcements.length > 0">
          <div
            v-for="item in announcements"
            :key="item.id"
            class="announcement-card"
            :class="{ pinned: item.isPinned, expanded: expandedId === item.id }"
            @click="toggleExpand(item)"
          >
            <div class="card-header">
              <div class="card-badges">
                <span v-if="item.isPinned" class="badge-pinned">
                  <i class="el-icon-top"></i> 置顶
                </span>
              </div>
              <span class="card-time">{{ formatDate(item.createdAt) }}</span>
            </div>
            <h3 class="card-title">{{ item.title }}</h3>
            <div class="card-body" v-if="expandedId === item.id">
              <div class="card-content" v-html="formatContent(item.content)"></div>
            </div>
            <div class="card-body-preview" v-else>
              <p class="card-preview">{{ truncateContent(item.content) }}</p>
              <span class="expand-hint">点击展开查看详情 →</span>
            </div>
            <div class="card-footer" v-if="item.publisher">
              <span class="publisher-name">
                {{ item.publisher.name || item.publisher.username }}
              </span>
            </div>
          </div>
        </div>

        <div class="empty-state" v-else>
          <div class="empty-icon">📭</div>
          <h3>暂无公告</h3>
          <p>{{ searchKeyword ? '未找到匹配的公告，换个关键词试试？' : '管理员还没有发布任何公告' }}</p>
        </div>
      </section>

      <!-- 分页 -->
      <div class="pagination-section" v-if="pagination.total > 0">
        <el-pagination
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
          :current-page="pagination.page"
          :page-sizes="[10, 20, 30]"
          :page-size="pagination.limit"
          layout="total, sizes, prev, pager, next"
          :total="pagination.total"
          background
        ></el-pagination>
      </div>
    </div>
  </div>
</template>

<script>
import { announcementApi } from '@/services/api'

export default {
  name: 'AnnouncementPage',
  data() {
    return {
      loading: false,
      searchKeyword: '',
      announcements: [],
      expandedId: null,
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
      this.loading = true
      try {
        const params = {
          page: this.pagination.page,
          limit: this.pagination.limit
        }
        if (this.searchKeyword.trim()) {
          params.search = this.searchKeyword.trim()
        }
        const res = await announcementApi.getAnnouncements(params)
        this.announcements = res.data.items || []
        this.pagination.total = res.data.total || 0
      } catch (error) {
        console.error('加载公告失败:', error)
        this.$message.error('加载公告失败')
      } finally {
        this.loading = false
      }
    },
    toggleExpand(item) {
      this.expandedId = this.expandedId === item.id ? null : item.id
    },
    truncateContent(content) {
      if (!content) return ''
      const text = content.replace(/<[^>]*>/g, '')
      if (text.length <= 120) return text
      return text.substring(0, 120) + '...'
    },
    formatContent(content) {
      if (!content) return ''
      // 将换行转换为 <br>，保留基本格式
      return content.replace(/\n/g, '<br>')
    },
    handleSizeChange(val) {
      this.pagination.limit = val
      this.pagination.page = 1
      this.loadAnnouncements()
    },
    handleCurrentChange(val) {
      this.pagination.page = val
      this.loadAnnouncements()
    },
    formatDate(dateStr) {
      if (!dateStr) return ''
      const date = new Date(dateStr)
      if (isNaN(date.getTime())) return dateStr
      const now = new Date()
      const diff = now.getTime() - date.getTime()
      const days = Math.floor(diff / 86400000)
      if (days === 0) {
        const hours = Math.floor(diff / 3600000)
        if (hours === 0) {
          const mins = Math.floor(diff / 60000)
          return mins <= 1 ? '刚刚' : `${mins}分钟前`
        }
        return `${hours}小时前`
      }
      if (days === 1) return '昨天'
      if (days < 7) return `${days}天前`
      return date.toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      })
    }
  }
}
</script>

<style scoped>
.announcement-page {
  position: relative;
  min-height: 100vh;
  background: var(--bg-page, #F8F9F5);
  padding: 24px;
  font-family: var(--font-main);
}

/* 背景装饰 */
.page-bg {
  position: absolute;
  inset: 0;
  overflow: hidden;
  pointer-events: none;
}

.bg-shape {
  position: absolute;
  border-radius: 50%;
  opacity: 0.05;
}

.bg-shape-1 {
  width: 360px;
  height: 360px;
  background: var(--primary, #2D8A6E);
  top: -80px;
  right: -80px;
}

.bg-shape-2 {
  width: 240px;
  height: 240px;
  background: var(--accent, #C8AA6E);
  bottom: -60px;
  left: -60px;
}

.page-content {
  position: relative;
  max-width: 880px;
  margin: 0 auto;
}

/* 头部 */
.page-hero {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 24px;
  margin-bottom: 28px;
  flex-wrap: wrap;
}

.hero-content {
  flex: 1;
}

.hero-title {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 6px;
  display: flex;
  align-items: center;
  gap: 10px;
  letter-spacing: -0.3px;
}

.hero-title i {
  color: var(--primary, #2D8A6E);
  font-size: 26px;
}

.hero-subtitle {
  font-size: 14px;
  color: var(--text-light, #888);
  margin: 0;
}

.hero-search {
  flex-shrink: 0;
}

.search-input {
  width: 260px;
}

/* 公告卡片列表 */
.announcement-section {
  margin-bottom: 24px;
}

.announcement-grid {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.announcement-card {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  padding: 20px 24px;
  cursor: pointer;
  transition: all var(--duration-fast, 0.2s) var(--ease-out-expo);
}

.announcement-card:hover {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.06);
  transform: translateY(-1px);
  border-color: var(--primary, #2D8A6E);
}

.announcement-card.pinned {
  border-left: 4px solid #D48F2C;
  background: linear-gradient(90deg, #FFFBF2 0%, #FFFFFF 100%);
}

.announcement-card.expanded {
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
  border-color: var(--primary, #2D8A6E);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
}

.card-badges {
  display: flex;
  gap: 8px;
}

.badge-pinned {
  display: flex;
  align-items: center;
  gap: 3px;
  padding: 2px 10px;
  background: #FFF7E6;
  border-radius: var(--radius-full, 9999px);
  font-size: 11px;
  font-weight: 600;
  color: #D46B08;
}

.badge-pinned i {
  font-size: 12px;
}

.card-time {
  font-size: 12px;
  color: var(--text-light, #888);
}

.card-title {
  font-size: 17px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 10px;
  line-height: 1.4;
}

.card-body {
  margin-top: 10px;
}

.card-content {
  font-size: 14px;
  color: var(--text-secondary, #555);
  line-height: 1.8;
  padding: 14px 16px;
  background: var(--bg-hover, #F5F2EC);
  border-radius: var(--radius-sm, 10px);
}

.card-body-preview {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 16px;
}

.card-preview {
  font-size: 13px;
  color: var(--text-light, #888);
  margin: 0;
  line-height: 1.5;
  flex: 1;
}

.expand-hint {
  font-size: 12px;
  color: var(--primary, #2D8A6E);
  white-space: nowrap;
  font-weight: 500;
  flex-shrink: 0;
}

.card-footer {
  margin-top: 12px;
  display: flex;
  justify-content: flex-end;
}

.publisher-name {
  font-size: 12px;
  color: var(--text-light, #888);
}

/* 空状态 */
.empty-state {
  text-align: center;
  padding: 80px 20px;
  color: var(--text-light, #888);
}

.empty-icon {
  font-size: 56px;
  margin-bottom: 16px;
}

.empty-state h3 {
  font-size: 18px;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 8px;
}

.empty-state p {
  font-size: 14px;
  margin: 0;
}

/* 分页 */
.pagination-section {
  display: flex;
  justify-content: center;
  padding: 10px 0 30px;
}
</style>