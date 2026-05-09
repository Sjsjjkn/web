<template>
  <div class="public-profile-page" v-loading="loading">
    <!-- 顶部用户信息卡片 -->
    <div class="profile-header" v-if="user">
      <div class="header-bg"></div>
      <div class="profile-card">
        <div class="avatar-section">
          <img 
            v-if="user.avatar" 
            :src="user.avatar" 
            :alt="user.name" 
            class="profile-avatar"
          />
          <div v-else class="avatar-placeholder">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
        <div class="profile-info">
          <h1 class="profile-name">{{ user.name }}</h1>
          <span class="profile-role">{{ translateRole(user.role) }}</span>
          <p class="profile-bio" v-if="user.bio">{{ user.bio }}</p>
          <div class="profile-meta" v-if="user.workId || user.department">
            <span v-if="user.workId">{{ user.workId }}</span>
            <span v-if="user.workId && user.department">|</span>
            <span v-if="user.department">{{ user.department }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 统计数据 -->
    <div class="stats-row" v-if="user">
      <div class="stat-item">
        <span class="stat-value">{{ user.workCount }}</span>
        <span class="stat-label">作品</span>
      </div>
      <div class="stat-divider"></div>
      <div class="stat-item">
        <span class="stat-value">{{ totalViews }}</span>
        <span class="stat-label">总浏览</span>
      </div>
      <div class="stat-divider"></div>
      <div class="stat-item">
        <span class="stat-value">{{ totalFavorites }}</span>
        <span class="stat-label">总收藏</span>
      </div>
    </div>

    <!-- 作品列表 -->
    <div class="works-section" v-if="user">
      <h2 class="section-title">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
        <span>作品集</span>
        <span class="work-count">({{ user.workCount }})</span>
      </h2>
      
      <div class="works-grid" v-if="works.length > 0">
        <div 
          class="work-card"
          v-for="work in works"
          :key="work.id"
          @click="$router.push(`/works/${work.id}`)"
        >
          <div class="work-cover">
            <img 
              v-if="work.thumbnailUrl" 
              :src="work.thumbnailUrl" 
              :alt="work.title" 
            />
            <div v-else class="work-placeholder">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
          </div>
          <div class="work-info">
            <h4 class="work-title">{{ work.title }}</h4>
            <p class="work-desc">{{ work.description || '暂无描述' }}</p>
            <div class="work-meta">
              <span>{{ work.fileType }}</span>
              <span>{{ formatDate(work.createdAt) }}</span>
            </div>
          </div>
        </div>
      </div>
      
      <div class="empty-state" v-else>
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1">
          <path stroke-linecap="round" stroke-linejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
        <p>暂无作品</p>
      </div>
    </div>
  </div>
</template>

<script>
import http from '../../utils/http'

export default {
  name: 'PublicProfile',
  data() {
    return {
      loading: true,
      user: null,
      works: [],
      totalViews: 0,
      totalFavorites: 0
    }
  },
  async mounted() {
    const userId = this.$route.params.userId
    if (userId) {
      await this.loadProfile(userId)
    }
  },
  methods: {
    async loadProfile(userId) {
      try {
        const res = await http.get(`/api/Auth/public-profile/${userId}`)
        if (res.data) {
          this.user = res.data
          this.works = res.data.works || []
          this.calculateStats()
        }
      } catch (error) {
        console.error('加载用户主页失败:', error)
        if (error.response?.status === 403) {
          this.$message.error('该用户的个人主页未公开')
        } else if (error.response?.status === 404) {
          this.$message.error('用户不存在')
        } else {
          this.$message.error('加载失败')
        }
      } finally {
        this.loading = false
      }
    },
    calculateStats() {
      this.totalViews = this.works.reduce((sum, w) => sum + (w.views || 0), 0)
      this.totalFavorites = this.works.reduce((sum, w) => sum + (w.favorites || 0), 0)
    },
    translateRole(role) {
      const roles = {
        admin: '管理员',
        teacher: '导师',
        student: '学生',
        guest: '访客',
        Admin: '管理员',
        Teacher: '导师',
        Student: '学生',
        Guest: '访客'
      }
      return roles[role] || role || '未知身份'
    },
    formatDate(dateString) {
      if (!dateString) return '--'
      const date = new Date(dateString)
      return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
    }
  }
}
</script>

<style scoped>
.public-profile-page {
  min-height: 100vh;
  background: #f5f7fa;
  padding-bottom: 40px;
}

.profile-header {
  position: relative;
  padding: 40px 24px;
}

.header-bg {
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.profile-card {
  position: relative;
  max-width: 800px;
  margin: 0 auto;
  background: #fff;
  border-radius: 20px;
  padding: 32px;
  display: flex;
  gap: 24px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
}

.avatar-section {
  flex-shrink: 0;
}

.profile-avatar {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  object-fit: cover;
  border: 4px solid #fff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.avatar-placeholder {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 48px;
  border: 4px solid #fff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.profile-info {
  flex: 1;
}

.profile-name {
  margin: 0 0 8px;
  font-size: 28px;
  font-weight: 700;
  color: #1a1a1a;
}

.profile-role {
  display: inline-block;
  padding: 4px 12px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
  margin-bottom: 12px;
}

.profile-bio {
  margin: 0 0 12px;
  color: #666;
  font-size: 14px;
  line-height: 1.6;
}

.profile-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #888;
}

.stats-row {
  max-width: 800px;
  margin: -20px auto 32px;
  position: relative;
  background: #fff;
  border-radius: 12px;
  padding: 20px 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.stat-item {
  text-align: center;
  flex: 1;
}

.stat-value {
  display: block;
  font-size: 24px;
  font-weight: 700;
  color: #1a1a1a;
}

.stat-label {
  font-size: 13px;
  color: #888;
}

.stat-divider {
  width: 1px;
  height: 40px;
  background: #eee;
}

.works-section {
  max-width: 900px;
  margin: 0 auto;
  padding: 0 24px;
}

.section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0 0 24px;
  font-size: 20px;
  font-weight: 600;
  color: #1a1a1a;
}

.section-title svg {
  width: 20px;
  height: 20px;
  color: #667eea;
}

.work-count {
  font-size: 14px;
  font-weight: 400;
  color: #888;
}

.works-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 20px;
}

.work-card {
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.work-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

.work-cover {
  position: relative;
  width: 100%;
  height: 160px;
  overflow: hidden;
  background: #f5f7fa;
}

.work-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.work-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ccc;
  font-size: 48px;
}

.work-info {
  padding: 16px;
}

.work-title {
  margin: 0 0 8px;
  font-size: 15px;
  font-weight: 600;
  color: #1a1a1a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.work-desc {
  margin: 0 0 12px;
  font-size: 13px;
  color: #666;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.work-meta {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
  color: #999;
}

.empty-state {
  text-align: center;
  padding: 60px 24px;
  color: #999;
}

.empty-state svg {
  width: 80px;
  height: 80px;
  margin-bottom: 16px;
}

.empty-state p {
  margin: 0;
  font-size: 14px;
}
</style>