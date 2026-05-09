<template>
  <div class="work-detail-page" v-loading="loading">
    <!-- 加载状态 -->
    <div v-if="loading" class="loading-state">
      <div class="loading-spinner"></div>
      <p class="loading-text">Loading Asset...</p>
    </div>

    <template v-else-if="work">
      <!-- 顶部简易导航区 -->
      <div class="top-nav">
        <div class="nav-content">
          <button class="back-btn" @click="$router.go(-1)">
            <svg class="nav-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
            </svg>
            <span>返回画廊</span>
          </button>
          <span class="nav-divider">/</span>
          <span class="nav-item" @click="$router.push('/works/explore')">所有作品</span>
          <span class="nav-divider">/</span>
          <span class="nav-title">{{ work.title }}</span>
        </div>
      </div>

      <!-- 核心双栏布局 -->
      <main class="main-content">
        <!-- 左侧：作品视觉预览区 -->
        <div class="preview-section">
          <div class="preview-container">
            <img 
              v-if="work.coverImage"
              :src="`/api/File/download?fileName=${encodeURIComponent(work.coverImage)}`"
              :alt="work.title"
              class="preview-image"
            />
            <div v-else class="preview-placeholder">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span>NO PREVIEW</span>
            </div>
            
            <!-- 渐变遮罩与底部文件信息悬浮条 -->
            <div class="preview-overlay"></div>
            <div class="preview-info-bar">
              <div class="file-info">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4m0 5c0 2.21-3.582 4-8 4s-8-1.79-8-4" />
                </svg>
                <span>{{ fileSize }}</span>
              </div>
              <div class="file-divider"></div>
              <span class="file-name">{{ work.fileName }}</span>
            </div>
          </div>
        </div>

        <!-- 右侧：作品信息与操作区 -->
        <div class="info-section">
          <!-- 状态徽标 -->
          <div class="status-badges">
            <span class="badge badge-published">已发布</span>
            <span class="badge badge-category">{{ work.category }}</span>
          </div>

          <!-- 标题 -->
          <h1 class="work-title">{{ work.title }}</h1>

          <!-- 作者名片 -->
          <div class="author-card">
            <div class="author-avatar-wrap">
              <img 
                v-if="work.authorAvatar"
                :src="`/api/File/download?fileName=${encodeURIComponent(work.authorAvatar)}`"
                :alt="work.author"
                class="author-avatar"
              />
              <div v-else class="author-avatar-placeholder">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
            </div>
            <div class="author-info">
              <h3 class="author-name">{{ work.author || work.uploadUserName || '匿名用户' }}</h3>
              <p class="author-role">{{ translateRole(work.authorRole || work.role || work.uploadUserRole) }}</p>
            </div>
            <button 
              class="view-profile-btn" 
              :disabled="!work.uploadUserProfilePublic"
              @click="handleViewProfile"
            >
              {{ work.uploadUserProfilePublic ? '查看主页' : '主页未公开' }}
            </button>
          </div>

          <!-- 数据指标 -->
          <div class="stats-grid">
            <div class="stat-item">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
              <div class="stat-value">{{ work.views || work.Views || work.viewCount || work.view_count || 0 }}</div>
              <div class="stat-label">浏览量</div>
            </div>
            <div class="stat-item">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
              </svg>
              <div class="stat-value">{{ work.favorites || work.Favorites || work.favoriteCount || work.favorite_count || 0 }}</div>
              <div class="stat-label">收藏数</div>
            </div>
            <div class="stat-item">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <div class="stat-value">{{ formatDateShort(work.createdAt) }}</div>
              <div class="stat-label">{{ formatYear(work.createdAt) }}</div>
            </div>
          </div>

          <!-- 核心操作按钮 -->
          <div class="action-buttons">
            <button class="download-btn" @click="handleDownload">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
              </svg>
              <span>下载源文件</span>
            </button>
            <div class="btn-group">
              <button 
                :class="['favorite-btn', { favorited: isFavorited }]" 
                @click="handleFavorite"
              >
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                </svg>
                <span>{{ isFavorited ? '已收藏' : '收藏作品' }}</span>
              </button>
              <button class="share-btn">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
                </svg>
              </button>
            </div>
          </div>

          <!-- 分割线 -->
          <div class="divider"></div>

          <!-- 作品描述 -->
          <div class="description-section">
            <h3 class="section-title">作品介绍</h3>
            <div v-if="work.description" class="description-content">
              {{ work.description }}
            </div>
            <div v-else class="description-placeholder">
              作者很懒，没有留下描述。
            </div>
          </div>

          <!-- 作品标签 -->
          <div class="tags-section">
            <h3 class="section-title">标签</h3>
            <div class="tags-list">
              <span 
                v-for="(tag, index) in work.tags" 
                :key="index" 
                class="tag-item"
              >
                #{{ tag }}
              </span>
            </div>
          </div>

          <!-- 举报按钮 -->
          <button class="report-btn">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            <span>举报该作品</span>
          </button>
        </div>
      </main>
    </template>
  </div>
</template>

<script>
import { workApi } from '../../services/api'

export default {
  name: 'WorkDetail',
  data() {
    return {
      loading: true,
      work: null,
      isFavorited: false,
      fileSize: ''
    }
  },
  mounted() {
    this.loadWork().then(() => {
      this.incrementViews()
    })
  },
  methods: {
    async loadWork() {
      const id = this.$route.params.id
      try {
        const res = await workApi.getWorkById(id)
        this.work = res.data
        
        // 处理文件大小
        if (this.work.fileSize) {
          const size = parseInt(this.work.fileSize)
          if (size < 1024) {
            this.fileSize = size + ' B'
          } else if (size < 1024 * 1024) {
            this.fileSize = (size / 1024).toFixed(1) + ' KB'
          } else {
            this.fileSize = (size / (1024 * 1024)).toFixed(1) + ' MB'
          }
        } else {
          this.fileSize = '未知大小'
        }

        // 处理标签
        if (this.work.tags) {
          try {
            this.work.tags = JSON.parse(this.work.tags)
          } catch {
            this.work.tags = []
          }
        } else {
          this.work.tags = []
        }

        this.loading = false
      } catch (error) {
        console.error('加载作品失败:', error)
        this.$message.error('加载作品失败')
        this.loading = false
      }
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
        Guest: '访客',
        '系统管理员': '系统管理员',
        '超级管理员': '超级管理员',
        '普通用户': '普通用户',
        user: '用户'
      }
      return roles[role] || role || '未知身份'
    },

    formatDateShort(dateString) {
      if (!dateString) return '--'
      const date = new Date(dateString)
      return `${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
    },

    formatYear(dateString) {
      if (!dateString) return '--'
      return new Date(dateString).getFullYear()
    },

    async incrementViews() {
      const id = this.$route.params.id
      try {
        await workApi.incrementViews(id)
        if (this.work) {
          this.work.views = (this.work.views || 0) + 1
        }
      } catch (error) {
        console.error('增加浏览量失败:', error)
      }
    },

    handleDownload() {
      if (this.work.filePath) {
        window.open(`/api/File/download?fileName=${encodeURIComponent(this.work.filePath)}`, '_blank')
      } else {
        this.$message.warning('暂无下载文件')
      }
    },

    handleViewProfile() {
      console.log('handleViewProfile called', {
        work: this.work,
        allKeys: this.work ? Object.keys(this.work) : [],
        userId: this.work?.UserId,
        userId_lower: this.work?.userId,
        uploadUserId: this.work?.uploadUserId,
        profilePublic: this.work?.uploadUserProfilePublic
      })
      const userId = this.work?.UserId || this.work?.userId || this.work?.uploadUserId || this.work?.user_id || this.work?.id
      if (this.work && userId) {
        this.$router.push(`/profile/${userId}`)
      } else {
        this.$message.warning('无法查看主页')
      }
    },

    async handleFavorite() {
      try {
        await workApi.toggleFavorite(this.work.id)
        this.isFavorited = !this.isFavorited
        if (this.isFavorited) {
          this.work.favoriteCount = (this.work.favoriteCount || 0) + 1
          this.$message.success('收藏成功')
        } else {
          this.work.favoriteCount = Math.max(0, (this.work.favoriteCount || 0) - 1)
          this.$message.success('取消收藏')
        }
      } catch (error) {
        console.error('收藏失败:', error)
        this.$message.error('操作失败')
      }
    }
  }
}
</script>

<style scoped>
.work-detail-page {
  min-height: 100vh;
  background: #F7F7F8;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

/* 加载状态 */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  gap: 16px;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #e5e7eb;
  border-top-color: #111;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.loading-text {
  font-size: 12px;
  font-weight: 500;
  color: #9ca3af;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}

/* 顶部导航 */
.top-nav {
  padding: 24px 24px 0;
  max-width: 1600px;
  margin: 0 auto;
}

.nav-content {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #6b7280;
  flex-wrap: wrap;
}

.back-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 9999px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.back-btn:hover {
  border-color: #d1d5db;
  color: #111;
  box-shadow: 0 2px 8px rgba(0,0,0,0.08);
}

.nav-icon {
  width: 16px;
  height: 16px;
  color: #9ca3af;
}

.back-btn:hover .nav-icon {
  color: #111;
}

.nav-divider {
  opacity: 0.4;
  padding: 0 4px;
}

.nav-item {
  cursor: pointer;
  transition: color 0.2s;
}

.nav-item:hover {
  color: #111;
}

.nav-title {
  color: #111;
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 主内容区 */
.main-content {
  max-width: 1600px;
  margin: 0 auto;
  padding: 24px;
  display: grid;
  grid-template-columns: 1fr;
  gap: 40px;
}

@media (min-width: 1024px) {
  .main-content {
    grid-template-columns: 7fr 5fr;
    gap: 64px;
  }
}

/* 左侧预览区 */
.preview-section {
  position: sticky;
  top: 32px;
  align-self: start;
}

.preview-container {
  position: relative;
  width: 100%;
  aspect-ratio: auto;
  min-height: 500px;
  background: #f3f4f6;
  border: 1px solid rgba(229, 231, 235, 0.6);
  border-radius: 2rem;
  box-shadow: 0 8px 30px rgba(0,0,0,0.04);
  overflow: hidden;
}

.preview-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.7s ease-out;
}

.preview-container:hover .preview-image {
  transform: scale(1.02);
}

.preview-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: #9ca3af;
  width: 100%;
  height: 100%;
}

.preview-placeholder svg {
  width: 48px;
  height: 48px;
}

.preview-placeholder span {
  font-size: 14px;
  font-weight: 500;
  letter-spacing: 0.1em;
}

.preview-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, rgba(0,0,0,0.4), transparent);
  opacity: 0;
  transition: opacity 0.3s ease;
  pointer-events: none;
}

.preview-container:hover .preview-overlay {
  opacity: 1;
}

.preview-info-bar {
  position: absolute;
  bottom: 24px;
  left: 24px;
  right: 24px;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 20px;
  background: rgba(255,255,255,0.2);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255,255,255,0.3);
  border-radius: 1rem;
  color: #fff;
  opacity: 0;
  transform: translateY(8px);
  transition: all 0.3s ease;
}

.preview-container:hover .preview-info-bar {
  opacity: 1;
  transform: translateY(0);
}

.file-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.file-info svg {
  width: 16px;
  height: 16px;
  opacity: 0.8;
}

.file-info span {
  font-size: 14px;
  font-weight: 700;
  letter-spacing: 0.05em;
}

.file-divider {
  width: 1px;
  height: 16px;
  background: rgba(255,255,255,0.3);
}

.file-name {
  font-size: 14px;
  font-weight: 500;
  opacity: 0.9;
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 右侧信息区 */
.info-section {
  display: flex;
  flex-direction: column;
  gap: 24px;
  padding-top: 8px;
}

/* 状态徽标 */
.status-badges {
  display: flex;
  gap: 12px;
}

.badge {
  padding: 6px 12px;
  border-radius: 9999px;
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}

.badge-published {
  background: #dcfce7;
  color: #166534;
  border: 1px solid rgba(34, 197, 94, 0.3);
}

.badge-category {
  background: rgba(229, 231, 235, 0.6);
  color: #374151;
}

/* 标题 */
.work-title {
  font-size: 2.25rem;
  font-weight: 800;
  line-height: 1.1;
  color: #111;
  margin: 0;
}

@media (min-width: 640px) {
  .work-title {
    font-size: 3rem;
  }
}

/* 作者名片 */
.author-card {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 24px 8px 8px;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 1rem;
  box-shadow: 0 1px 2px rgba(0,0,0,0.04);
  transition: box-shadow 0.2s;
}

.author-card:hover {
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
}

.author-avatar-wrap {
  padding: 8px;
}

.author-avatar {
  width: 56px;
  height: 56px;
  border-radius: 0.75rem;
  object-fit: cover;
  transition: transform 0.2s;
}

.author-card:hover .author-avatar {
  transform: scale(1.05);
}

.author-avatar-placeholder {
  width: 56px;
  height: 56px;
  border-radius: 0.75rem;
  background: #f3f4f6;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #9ca3af;
}

.author-avatar-placeholder svg {
  width: 24px;
  height: 24px;
}

.author-info {
  flex: 1;
}

.author-name {
  font-size: 16px;
  font-weight: 700;
  color: #111;
  margin: 0;
}

.author-role {
  font-size: 12px;
  font-weight: 500;
  color: #6b7280;
  margin: 2px 0 0;
}

.view-profile-btn {
  padding: 8px 16px;
  background: #f3f4f6;
  border: none;
  border-radius: 0.75rem;
  font-size: 12px;
  font-weight: 700;
  color: #374151;
  cursor: pointer;
  transition: all 0.2s;
}

.view-profile-btn:hover {
  background: #e5e7eb;
}

/* 数据指标 */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 16px;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 1rem;
  box-shadow: 0 1px 2px rgba(0,0,0,0.04);
}

.stat-item svg {
  width: 20px;
  height: 20px;
  color: #9ca3af;
  margin-bottom: 8px;
}

.stat-item:nth-child(2) svg {
  color: #f43f5e;
}

.stat-value {
  font-size: 1.25rem;
  font-weight: 800;
  color: #111;
}

.stat-label {
  font-size: 10px;
  font-weight: 700;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-top: 4px;
}

/* 操作按钮 */
.action-buttons {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.download-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  height: 56px;
  background: #111;
  color: #fff;
  border: none;
  border-radius: 1rem;
  font-size: 16px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s;
}

.download-btn:hover {
  background: #374151;
  box-shadow: 0 8px 24px rgba(0,0,0,0.2);
  transform: translateY(-2px);
}

.download-btn svg {
  width: 20px;
  height: 20px;
}

.btn-group {
  display: flex;
  gap: 12px;
}

.favorite-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  height: 48px;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 1rem;
  font-size: 14px;
  font-weight: 700;
  color: #374151;
  cursor: pointer;
  transition: all 0.2s;
}

.favorite-btn:hover {
  background: #f9fafb;
}

.favorite-btn.favorited {
  background: #fef2f2;
  border-color: #fecaca;
  color: #dc2626;
}

.favorite-btn svg {
  width: 18px;
  height: 18px;
}

.favorite-btn.favorited svg {
  fill: currentColor;
}

.share-btn {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 1rem;
  color: #6b7280;
  cursor: pointer;
  transition: all 0.2s;
}

.share-btn:hover {
  color: #111;
  background: #f9fafb;
}

.share-btn svg {
  width: 18px;
  height: 18px;
}

/* 分割线 */
.divider {
  height: 1px;
  background: rgba(229, 231, 235, 0.8);
}

/* 作品描述 */
.description-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.section-title {
  font-size: 14px;
  font-weight: 700;
  color: #111;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0;
}

.description-content {
  font-size: 15px;
  line-height: 1.8;
  color: #4b5563;
  white-space: pre-wrap;
  font-weight: 500;
}

.description-placeholder {
  padding: 24px;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 1rem;
  text-align: center;
  color: #9ca3af;
  font-size: 14px;
}

/* 标签 */
.tags-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.tags-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.tag-item {
  padding: 8px 16px;
  background: #f3f4f6;
  border-radius: 0.75rem;
  font-size: 12px;
  font-weight: 700;
  color: #374151;
  cursor: pointer;
  transition: all 0.2s;
}

.tag-item:hover {
  background: #e5e7eb;
}

/* 举报按钮 */
.report-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  align-self: flex-start;
  padding: 0;
  background: none;
  border: none;
  font-size: 12px;
  font-weight: 700;
  color: #9ca3af;
  cursor: pointer;
  transition: color 0.2s;
}

.report-btn:hover {
  color: #dc2626;
}

.report-btn svg {
  width: 14px;
  height: 14px;
}
</style>