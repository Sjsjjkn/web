<template>
  <div class="public-profile-page" v-loading="loading">
    <!-- 顶部用户信息卡片 -->
    <div class="profile-header" v-if="user">
      <div class="header-bg"></div>
      <div class="profile-card">
        <div class="avatar-section">
          <div class="avatar-wrapper">
            <div class="avatar-content">
              <img 
                v-show="user.avatar && !avatarError" 
                :src="user.avatar" 
                :alt="user.name" 
                class="profile-avatar"
                @error="handleAvatarError"
              />
              <div v-show="!user.avatar || avatarError" class="avatar-placeholder">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
            </div>
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
              v-if="getWorkPreviewUrl(work)" 
              :src="getWorkPreviewUrl(work)" 
              :alt="work.title" 
              class="work-image"
              @load="(e) => e.target.parentElement.querySelector('.work-placeholder')?.remove()"
              @error="(e) => e.target.remove()"
            />
            <div class="work-placeholder">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
          </div>
          <div class="work-info">
            <h4 class="work-title">{{ work.title }}</h4>
            <p class="work-desc">{{ work.description || '暂无描述' }}</p>
            <div class="work-meta">
              <span class="meta-item">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                </svg>
                {{ work.favorites || work.favoriteCount || 0 }}
              </span>
              <span class="meta-item">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                {{ work.views || 0 }}
              </span>
              <span class="meta-item">{{ formatDate(work.createdAt) }}</span>
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
      
      <div class="pagination-wrapper" v-if="total > pageSize">
        <el-pagination
          @current-change="handlePageChange"
          :current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          layout="prev, pager, next"
          class="pagination"
        />
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
      totalFavorites: 0,
      avatarError: false,
      currentPage: 1,
      pageSize: 6,
      total: 0
    }
  },
  async mounted() {
    const userId = this.$route.params.userId
    if (userId) {
      await this.loadProfile(userId)
      await this.loadWorks()
    }
  },
  methods: {
    async loadProfile(userId) {
      try {
        const res = await http.get(`/api/Auth/public-profile/${userId}`)
        if (res.data) {
          this.user = res.data
        }
      } catch (error) {
        console.error('加载用户信息失败:', error)
        if (error.response?.status === 403) {
          this.$message.error('该用户的个人主页未公开')
        } else if (error.response?.status === 404) {
          this.$message.error('用户不存在')
        } else {
          this.$message.error('加载失败')
        }
      }
    },
    async loadWorks() {
      this.loading = true
      try {
        const userId = this.$route.params.userId
        console.log('尝试获取用户', userId, '的作品')
        const worksRes = await http.get('/api/Work', {
          params: {
            user_id: userId,
            page: this.currentPage,
            pageSize: this.pageSize
          }
        })
        
        console.log('后端返回:', worksRes.data)
        if (worksRes.data && worksRes.data.items) {
          this.works = worksRes.data.items
          this.total = worksRes.data.total || 0
          this.calculateStats()
        }
      } catch (error) {
        console.error('加载作品失败:', error)
        this.$message.error('加载作品失败')
      } finally {
        this.loading = false
      }
    },
    handlePageChange(page) {
      this.currentPage = page
      this.loadWorks()
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
    handleAvatarError() {
      this.avatarError = true
    },
    getWorkPreviewUrl(work) {
      if (!work) return null
      
      if (work.previewImage) {
        return `/api/File/download?fileName=${encodeURIComponent(work.previewImage)}`
      }
      
      if (!work.filePath) return null
      
      const ext = work.filePath.toLowerCase().substring(work.filePath.lastIndexOf('.'))
      if (['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'].includes(ext)) {
        return `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
      }
      
      return null
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
  background: #F7F7F8;
  padding-bottom: 40px;
}

.profile-header {
  position: relative;
  padding: 40px 24px;
}

.header-bg {
  position: absolute;
  inset: 0;
  background: linear-gradient(145deg, #2D8A6E 0%, #1E6F5C 100%);
}

.profile-card {
  position: relative;
  max-width: 900px;
  margin: 0 auto;
  background: linear-gradient(145deg, #ffffff, #fafafa);
  border-radius: 24px;
  padding: 40px;
  display: flex;
  gap: 32px;
  box-shadow: 0 16px 48px rgba(0, 0, 0, 0.12);
  border: 1px solid rgba(255, 255, 255, 0.8);
}

.avatar-section {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.avatar-wrapper {
  width: 150px;
  height: 150px;
  border-radius: 50%;
  overflow: hidden;
  border: 5px solid #fff;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(145deg, #2D8A6E 0%, #1E6F5C 100%);
}

.profile-card:hover .avatar-wrapper {
  transform: scale(1.05);
}

.profile-avatar {
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center center;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  background: linear-gradient(145deg, #2D8A6E 0%, #1E6F5C 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 60px;
}

.profile-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.profile-name {
  margin: 0 0 10px;
  font-size: 32px;
  font-weight: 700;
  color: #1a1a1a;
  letter-spacing: -0.5px;
}

.profile-role {
  display: inline-flex;
  align-items: center;
  padding: 6px 16px;
  background: linear-gradient(145deg, #2D8A6E 0%, #1E6F5C 100%);
  color: #fff;
  border-radius: 24px;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 14px;
  box-shadow: 0 4px 12px rgba(45, 138, 110, 0.3);
}

.profile-bio {
  margin: 0 0 14px;
  color: #555;
  font-size: 15px;
  line-height: 1.7;
  max-width: 500px;
}

.profile-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 14px;
  color: #777;
}

.profile-meta span {
  display: flex;
  align-items: center;
  gap: 6px;
}

.stats-row {
  max-width: 900px;
  margin: -24px auto 36px;
  position: relative;
  background: linear-gradient(145deg, #ffffff, #fafafa);
  border-radius: 18px;
  padding: 24px 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
  border: 1px solid #E8E2D8;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.stats-row:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.1);
}

.stat-item {
  text-align: center;
  flex: 1;
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.stats-row:hover .stat-item {
  transform: translateY(-3px);
}

.stat-value {
  display: block;
  font-size: 28px;
  font-weight: 700;
  color: #1a1a1a;
  background: linear-gradient(145deg, #2D8A6E 0%, #1E6F5C 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.stat-label {
  font-size: 14px;
  color: #666;
  margin-top: 4px;
}

.stat-divider {
  width: 1px;
  height: 48px;
  background: linear-gradient(180deg, transparent, #DDD, transparent);
}

.works-section {
  max-width: 900px;
  margin: 0 auto;
  padding: 0 24px;
}

.section-title {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 0 0 28px;
  font-size: 22px;
  font-weight: 700;
  color: #1a1a1a;
  letter-spacing: -0.3px;
}

.section-title svg {
  width: 22px;
  height: 22px;
  color: #2D8A6E;
}

.work-count {
  font-size: 14px;
  font-weight: 400;
  color: #888;
  margin-left: 4px;
}

.works-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
}

.work-card {
  background: linear-gradient(135deg, #fffef9 0%, #f8faf7 100%);
  border-radius: 24px;
  border: 2px solid #E8F0EC;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.work-card:hover {
  transform: translateY(-10px) scale(1.03);
  box-shadow: 0 25px 50px rgba(45, 138, 110, 0.15);
  border-color: #2D8A6E;
}

.work-cover {
  position: relative;
  width: 100%;
  height: 190px;
  overflow: hidden;
  background: linear-gradient(135deg, #E8F0EC 0%, #D4E5DE 100%);
}

.work-cover img,
.work-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.work-card:hover .work-cover img,
.work-card:hover .work-image {
  transform: scale(1.08);
}

.work-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #999;
  transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.work-card:hover .work-placeholder {
  transform: scale(1.08);
}

.work-placeholder svg {
  width: 56px;
  height: 56px;
  opacity: 0.5;
}

.work-info {
  padding: 20px;
  background: linear-gradient(180deg, transparent 0%, #f8faf7 100%);
}

.work-title {
  margin: 0 0 12px;
  font-size: 17px;
  font-weight: 600;
  color: #1a1a1a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  transition: color 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.work-card:hover .work-title {
  color: #2D8A6E;
}

.work-desc {
  margin: 0 0 14px;
  font-size: 13px;
  color: #666;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-height: 1.5;
}

.work-meta {
  display: flex;
  gap: 12px;
  align-items: center;
  font-size: 12px;
  color: #888;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.meta-item svg {
  width: 14px;
  height: 14px;
}

.meta-item:first-child {
  color: #E74C3C;
}

.meta-item:nth-child(2) {
  color: #3498DB;
}

.empty-state {
  text-align: center;
  padding: 80px 24px;
  color: #888;
  background: linear-gradient(145deg, #ffffff, #fafafa);
  border-radius: 20px;
  border: 1px dashed #DDD;
}

.empty-state svg {
  width: 96px;
  height: 96px;
  margin-bottom: 20px;
  opacity: 0.4;
}

.empty-state p {
  margin: 0;
  font-size: 15px;
  color: #888;
}

.pagination-wrapper {
  display: flex;
  justify-content: center;
  margin-top: 32px;
  padding-bottom: 24px;
}
</style>