<template>
  <div class="home-page">
    <!-- ========== Hero ========== -->
    <section class="hero">
      <div class="hero-content">
        <h1 class="hero-title">作品展示</h1>
        <p class="hero-subtitle">浏览和欣赏保定学院师生的优秀数字作品</p>
      </div>
    </section>

    <!-- ========== Stats Banner ========== -->
    <div class="stats-banner" v-loading="loading.overview">
      <div class="stat-card" v-for="(s, i) in stats" :key="i">
        <div class="stat-icon" :class="['icon-' + i]">{{ s.iconEmoji }}</div>
        <div class="stat-info">
          <h3>{{ s.value }}</h3>
          <p>{{ s.label }}</p>
        </div>
      </div>
    </div>

    <!-- ========== 最新上传 ========== -->
    <section class="works-section">
      <div class="section-header">
        <h2 class="section-title">最新上传</h2>
        <router-link to="/works/explore" class="view-all">查看全部 →</router-link>
      </div>

      <div v-if="loading.works" class="skeleton-grid">
        <div class="skeleton-card" v-for="n in 4" :key="n">
          <div class="skeleton-cover"></div>
          <div class="skeleton-body">
            <div class="skeleton-line w-80"></div>
            <div class="skeleton-line w-50"></div>
            <div class="skeleton-line w-60"></div>
          </div>
        </div>
      </div>

      <div v-else-if="recentWorks.length === 0" class="empty-state">
        <div class="empty-icon">📭</div>
        <h3>暂无作品</h3>
        <p>还没有任何作品上传，快来分享你的第一个作品吧</p>
      </div>

      <div v-else class="works-grid">
        <div
          class="work-card"
          v-for="(work, idx) in recentWorks"
          :key="work.id"
          :style="{ animationDelay: idx * 0.08 + 's' }"
        >
          <div class="card-cover" @click="handleViewWork(work)">
            <!-- 真实缩略图 -->
            <img
              v-if="getThumbnailUrl(work)"
              :src="getThumbnailUrl(work)"
              class="thumbnail-image"
              :alt="work.title"
            />
            <!-- 无缩略图时：带渐变的占位符 -->
            <div
              v-else
              class="cover-placeholder"
              :style="{ background: getGradient(work.id, idx) }"
            >
              <span class="file-icon">{{ getFileEmoji(work) }}</span>
              <span>{{ getFileExtension(work) }}</span>
            </div>
            <!-- 分类标签 -->
            <span class="category-badge">{{ work.category || '未分类' }}</span>
          </div>
          <div class="card-body">
            <div class="card-title" @click="handleViewWork(work)">{{ work.title }}</div>
            <div class="card-author">
              <span class="author-avatar">{{ (work.uploadUserName || '未')[0] }}</span>
              {{ work.uploadUserName || '未知作者' }}
              <span class="card-date" v-if="work.fileUploadTime || work.uploadDate">
                · {{ formatDate(work.fileUploadTime || work.uploadDate) }}
              </span>
            </div>
            <div class="card-actions">
              <button class="btn btn-primary" @click="handleViewWork(work)">查看详情</button>
              <button
                class="btn btn-favorite"
                @click="handleFavoriteWork(work)"
                :class="{ active: work.isFavorited }"
                :title="work.isFavorited ? '取消收藏' : '收藏'"
              >{{ work.isFavorited ? '⭐' : '☆' }}</button>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ========== 优秀作品推荐 =========茗 -->
    <section class="featured-section" v-if="featuredWorks.length > 0">
      <div class="section-header">
        <h2 class="section-title">优秀作品推荐</h2>
      </div>
      <div class="works-grid">
        <div
          class="work-card featured"
          v-for="(work, idx) in featuredWorks"
          :key="'f-' + work.id"
          :style="{ animationDelay: idx * 0.08 + 's' }"
        >
          <div class="card-cover" @click="handleViewWork(work)">
            <img
              v-if="getThumbnailUrl(work)"
              :src="getThumbnailUrl(work)"
              class="thumbnail-image"
              :alt="work.title"
            />
            <div
              v-else
              class="cover-placeholder"
              :style="{ background: getGradient(work.id, idx + 10) }"
            >
              <span class="file-icon">{{ getFileEmoji(work) }}</span>
              <span>{{ getFileExtension(work) }}</span>
            </div>
            <span class="category-badge">{{ work.category || '未分类' }}</span>
            <span class="featured-badge">⭐ 优秀</span>
          </div>
          <div class="card-body">
            <div class="card-title" @click="handleViewWork(work)">{{ work.title }}</div>
            <div class="card-author">
              <span class="author-avatar">{{ (work.uploadUserName || '优')[0] }}</span>
              {{ work.uploadUserName || '未知作者' }}
              <span class="card-date" v-if="work.fileUploadTime || work.uploadDate">
                · {{ formatDate(work.fileUploadTime || work.uploadDate) }}
              </span>
            </div>
            <div class="card-actions">
              <button class="btn btn-primary" @click="handleViewWork(work)">查看详情</button>
              <button
                class="btn btn-favorite"
                @click="handleFavoriteWork(work)"
                :class="{ active: work.isFavorited }"
                :title="work.isFavorited ? '取消收藏' : '收藏'"
              >{{ work.isFavorited ? '⭐' : '☆' }}</button>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ========== 详情对话框 ========== -->
    <el-dialog
      :visible.sync="detailDialogVisible"
      :title="currentWork ? currentWork.title : '作品详情'"
      width="680px"
      :close-on-click-modal="true"
      custom-class="detail-dialog"
      destroy-on-close
      append-to-body
      :modal-append-to-body="false"
      :modal="false"
      :lock-scroll="false"
    >
      <div v-if="currentWork" class="detail-content">
        <!-- 顶部封面区 -->
        <div class="detail-header">
          <div class="detail-cover">
            <img
              v-if="getThumbnailUrl(currentWork)"
              :src="getThumbnailUrl(currentWork)"
              class="detail-cover-img"
            />
            <div
              v-else
              class="detail-cover-placeholder"
              :style="{ background: getGradient(currentWork.id, 0) }"
            >
              <span style="font-size: 48px; opacity: 0.5;">{{ getFileEmoji(currentWork) }}</span>
              <span>{{ getFileExtension(currentWork) }}</span>
            </div>
          </div>
          <div class="detail-meta">
            <div class="detail-author">
              <span class="author-avatar-lg">{{ (currentWork.uploadUserName || '?')[0] }}</span>
              <span class="detail-author-name">{{ currentWork.uploadUserName || '未知作者' }}</span>
            </div>
            <div class="detail-stats">
              <span>👁 {{ currentWork.views || 0 }}</span>
              <span>⭐ {{ currentWork.favorites || 0 }}</span>
              <span>📁 {{ currentWork.category || '未分类' }}</span>
            </div>
            <div class="detail-date" v-if="currentWork.fileUploadTime || currentWork.uploadDate">
              上传于 {{ formatDate(currentWork.fileUploadTime || currentWork.uploadDate) }}
            </div>
          </div>
        </div>

        <!-- 描述 -->
        <div class="detail-description" v-if="currentWork.description">
          <h4>作品描述</h4>
          <p>{{ currentWork.description }}</p>
        </div>

        <!-- 文件信息 -->
        <div class="detail-file">
          <h4>文件信息</h4>
          <div class="file-tags">
            <span class="file-tag">📄 {{ currentWork.fileName || currentWork.filePath || '未知' }}</span>
            <span class="file-tag">🏷 {{ formatFileType(currentWork.filePath) }}</span>
          </div>
        </div>
      </div>

      <span slot="footer" class="dialog-footer">
        <el-button @click="detailDialogVisible = false" class="btn-cancel">关闭</el-button>
        <el-button
          v-if="currentWork && currentWork.filePath"
          type="primary"
          @click="handleDownloadFile(currentWork)"
          class="btn-download"
        >⬇ 下载文件</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import { getUser } from '../../utils/auth'

export default {
  name: 'Home',
  data() {
    return {
      stats: [],
      recentWorks: [],
      featuredWorks: [],
      loading: {
        overview: false,
        works: false
      },
      error: null,
      detailDialogVisible: false,
      currentWork: null,
      refreshTimer: null
    }
  },
  beforeRouteLeave(to, from, next) {
    this.detailDialogVisible = false
    next()
  },
  mounted() {
    this.fetchOverviewData()
    this.fetchWorksData()
    this.refreshTimer = setInterval(() => {
      this.fetchWorksData()
    }, 30000)
  },
  beforeDestroy() {
    if (this.refreshTimer) {
      clearInterval(this.refreshTimer)
    }
  },
  methods: {
    getFileType(filePath) {
      if (!filePath) return 'other'
      const extension = filePath.toLowerCase().substring(filePath.lastIndexOf('.'))
      const imageExts = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp']
      const videoExts = ['.mp4', '.avi', '.mov', '.wmv', '.flv', '.mkv']
      const modelExts = ['.json', '.obj', '.gltf', '.glb', '.fbx', '.dae']
      if (imageExts.includes(extension)) return 'image'
      if (videoExts.includes(extension)) return 'video'
      if (modelExts.includes(extension)) return 'model'
      return 'other'
    },

    getFileUrl(filePath) {
      if (!filePath) return ''
      return `http://localhost:5200/api/File/download?filePath=${encodeURIComponent(filePath)}`
    },

    getThumbnailUrl(work) {
      if (work.previewImage) {
        return `/api/File/download?fileName=${encodeURIComponent(work.previewImage)}`
      }
      if (!work.filePath) return null
      const fileName = work.filePath
      const extension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'))
      const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp']
      if (imageExtensions.includes(extension)) {
        return `/api/File/download?fileName=${encodeURIComponent(fileName)}`
      }
      return null
    },

    isModelFile(work) {
      if (!work.filePath) return false
      const extension = work.filePath.toLowerCase().substring(work.filePath.lastIndexOf('.'))
      const modelExts = ['.json', '.obj', '.gltf', '.glb', '.fbx', '.dae', '.3ds']
      return modelExts.includes(extension)
    },

    getFileExtension(work) {
      if (!work.filePath) return '文件'
      const fileName = work.filePath
      const extension = fileName.toLowerCase().substring(fileName.lastIndexOf('.') + 1)
      return extension.toUpperCase()
    },

    getFileEmoji(work) {
      const type = this.getFileType(work.filePath)
      const map = {
        image: '🖼',
        video: '🎬',
        model: '🧊',
        other: '📄'
      }
      return map[type] || '📄'
    },

    formatFileType(filePath) {
      if (!filePath) return '未知'
      const ext = filePath.toLowerCase().substring(filePath.lastIndexOf('.') + 1)
      return ext.toUpperCase() + ' 格式'
    },

    getGradient(id, idx) {
      const gradients = [
        'linear-gradient(135deg, #EDF5F0 0%, #D4EBE0 100%)',
        'linear-gradient(135deg, #E5F0F5 0%, #CCE0EB 100%)',
        'linear-gradient(135deg, #FDF2E4 0%, #F8E4C8 100%)',
        'linear-gradient(135deg, #FDF0EB 0%, #F8D8CB 100%)',
        'linear-gradient(135deg, #F3E5F5 0%, #E1CCE8 100%)',
        'linear-gradient(135deg, #E8F5E9 0%, #C8E0CA 100%)',
        'linear-gradient(135deg, #FFF9C4 0%, #FFECB3 100%)',
        'linear-gradient(135deg, #E3F2FD 0%, #BBDEFB 100%)',
        'linear-gradient(135deg, #FCE4EC 0%, #F8BBD0 100%)',
        'linear-gradient(135deg, #E0F2F1 0%, #B2DFDB 100%)',
        'linear-gradient(135deg, #FFF3E0 0%, #FFE0B2 100%)',
        'linear-gradient(135deg, #F1F8E9 0%, #DCEDC8 100%)'
      ]
      return gradients[(id + idx) % gradients.length]
    },

    async fetchOverviewData() {
      this.loading.overview = true
      this.error = null
      try {
        const response = await this.$axios.get('/api/Data/overview')
        const data = response.data
        this.stats = [
          {
            value: data.totalWorks.toString(),
            label: '作品总数',
            iconEmoji: '🖼'
          },
          {
            value: data.totalUsers.toString(),
            label: '活跃用户',
            iconEmoji: '👥'
          },
          {
            value: data.excellentWorks.toString(),
            label: '优秀作品',
            iconEmoji: '⭐'
          },
          {
            value: data.todayWorks.toString(),
            label: '今日上传',
            iconEmoji: '📤'
          }
        ]
      } catch (error) {
        console.error('获取概览数据失败:', error)
        this.error = '获取概览数据失败，请稍后重试'
        this.stats = []
      } finally {
        this.loading.overview = false
      }
    },

    async fetchWorksData() {
      this.loading.works = true
      this.error = null
      try {
        const response = await this.$axios.get('/api/Work/public?limit=20')
        const allWorks = response.data.items || []

        const sortedWorks = [...allWorks].sort((a, b) => {
          const dateA = new Date(a.fileUploadTime || a.uploadDate || 0)
          const dateB = new Date(b.fileUploadTime || b.uploadDate || 0)
          return dateB - dateA
        })

        const recentWorksData = sortedWorks.slice(0, 4).map(work => ({
          id: work.id,
          title: work.title || '未命名作品',
          uploadUserName: work.uploadUserName || '未知作者',
          category: work.category || '未分类',
          uploadDate: work.uploadDate,
          fileUploadTime: work.fileUploadTime,
          filePath: work.filePath,
          previewImage: work.previewImage,
          description: work.description,
          fileName: work.fileName,
          views: work.views || 0,
          favorites: work.favorites || 0,
          isFavorited: false,
          isExcellent: !!work.isExcellent,
          userId: work.userId || work.UserId || work.userid,
          uploadUserProfilePublic: work.uploadUserProfilePublic
        }))

        for (const work of recentWorksData) {
          try {
            const favResponse = await this.$axios.get(`/api/Work/${work.id}/is-favorite`)
            work.isFavorited = favResponse.data.isFavorite || false
          } catch (e) {
            work.isFavorited = false
          }
        }

        this.recentWorks = recentWorksData

        const featuredWorksData = sortedWorks
          .filter(work => !!work.isExcellent)
          .sort((a, b) => {
            const scoreA = (a.favorites || 0) * 2 + (a.views || 0)
            const scoreB = (b.favorites || 0) * 2 + (a.views || 0)
            return scoreB - scoreA
          })
          .slice(0, 4)
          .map(work => ({
            id: work.id,
            title: work.title || '未命名作品',
            uploadUserName: work.uploadUserName || '未知作者',
            category: work.category || '未分类',
            uploadDate: work.uploadDate,
            fileUploadTime: work.fileUploadTime,
            filePath: work.filePath,
            previewImage: work.previewImage,
            description: work.description,
            fileName: work.fileName,
            views: work.views || 0,
            favorites: work.favorites || 0,
            isFavorited: false,
            isExcellent: true,
            userId: work.userId || work.UserId || work.userid,
            uploadUserProfilePublic: work.uploadUserProfilePublic
          }))

        for (const work of featuredWorksData) {
          try {
            const favResponse = await this.$axios.get(`/api/Work/${work.id}/is-favorite`)
            work.isFavorited = favResponse.data.isFavorite || false
          } catch (e) {
            work.isFavorited = false
          }
        }

        this.featuredWorks = featuredWorksData
      } catch (error) {
        console.error('获取作品数据失败:', error)
        this.error = '获取作品数据失败，请稍后重试'
        this.recentWorks = []
        this.featuredWorks = []
      } finally {
        this.loading.works = false
      }
    },

    navigateToUpload() {
      const user = getUser()
      const target = user && ['Admin', 'Teacher'].includes(user.role)
        ? '/works/manage'
        : '/account/space'
      if (this.$route.path !== target) {
        this.$router.push(target)
      }
    },

    navigateToWorks() {
      this.$router.push('/works')
    },

    async handleViewWork(work) {
      this.currentWork = work
      this.detailDialogVisible = true
      try {
        await this.$axios.get(`/api/Work/${work.id}/view`)
        work.views = (work.views || 0) + 1
      } catch (error) {
        console.error('增加浏览量失败:', error)
      }
    },

    goToAuthorProfile(userId) {
      this.$router.push(`/profile/${userId}`)
    },

    async handleFavoriteWork(work) {
      try {
        if (!localStorage.getItem('token')) {
          this.$message.error('请先登录')
          return
        }
        if (work.isFavorited) {
          const response = await this.$axios.delete(`/api/Work/${work.id}/favorite`)
          this.$message.success('已取消收藏')
          work.favorites = response.data.favorites
          work.isFavorited = false
        } else {
          const response = await this.$axios.post(`/api/Work/${work.id}/favorite`, {})
          this.$message.success('收藏成功')
          work.favorites = response.data.favorites
          work.isFavorited = true
        }
      } catch (error) {
        if (error.response && error.response.data && error.response.data.message) {
          this.$message.error(error.response.data.message)
        } else {
          this.$message.error('收藏操作失败，请稍后重试')
        }
        console.error('收藏作品失败:', error)
      }
    },

    handleDownloadFile(work) {
      if (!work || !work.filePath) {
        this.$message.error('文件路径不存在，无法下载')
        return
      }
      try {
        const downloadUrl = `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
        const link = document.createElement('a')
        link.href = downloadUrl
        link.download = work.fileName || work.filePath
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        this.$message.success('文件下载已开始')
      } catch (error) {
        console.error('下载文件失败:', error)
        this.$message.error('文件下载失败，请稍后重试')
      }
    },

    formatDate(date) {
      if (!date) return ''
      const d = new Date(date)
      return d.toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      })
    }
  }
}
</script>

<style scoped>
/* ========================================
   HOME PAGE — BODA Design System
   ======================================== */
.home-page {
  min-height: 100vh;
  background: var(--bg-page, #F8F9F5);
  font-family: var(--font-main, 'Inter', 'Noto Sans SC', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif);
  padding-bottom: 60px;
}

/* ── Hero ── */
.hero {
  text-align: center;
  padding: 48px 24px 36px;
}

.hero-title {
  font-size: 32px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 10px;
  animation: fadeInUp .6s var(--ease-out-expo, cubic-bezier(0.16, 1, 0.3, 1)) both;
}

.hero-subtitle {
  font-size: 16px;
  color: var(--text-light, #888888);
  margin: 0;
  animation: fadeInUp .6s var(--ease-out-expo, cubic-bezier(0.16, 1, 0.3, 1)) .08s both;
}

/* ── Stats Banner ── */
.stats-banner {
  max-width: 1200px;
  margin: 0 auto 40px;
  padding: 0 24px;
  display: flex;
  gap: 16px;
  animation: fadeInUp .6s var(--ease-out-expo) .12s both;
}

.stat-card {
  flex: 1;
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 14px;
  transition: all var(--duration-normal, .3s);
}

.stat-card:hover {
  box-shadow: var(--shadow-md, 0 6px 20px rgba(0,0,0,.07));
  transform: translateY(-2px);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md, 14px);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
  flex-shrink: 0;
}

.stat-icon.icon-0 { background: var(--primary-bg, #EDF5F0); color: var(--primary, #2D8A6E); }
.stat-icon.icon-1 { background: #E5F0F5; color: var(--info, #5B9BD5); }
.stat-icon.icon-2 { background: #FFF8E1; color: #FFB300; }
.stat-icon.icon-3 { background: #FDF0EB; color: var(--accent, #C8AA6E); }

.stat-info h3 {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0;
  line-height: 1;
}

.stat-info p {
  font-size: 13px;
  color: var(--text-light, #888888);
  margin: 4px 0 0;
}

/* ── Section ── */
.works-section,
.featured-section {
  max-width: 1200px;
  margin: 0 auto 48px;
  padding: 0 24px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  margin-bottom: 20px;
}

.section-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0;
  position: relative;
  padding-left: 14px;
}

.section-title::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 4px;
  height: 22px;
  background: var(--primary, #2D8A6E);
  border-radius: 2px;
}

.view-all {
  font-size: 14px;
  color: var(--primary, #2D8A6E);
  text-decoration: none;
  font-weight: 500;
  transition: all var(--duration-fast, .18s);
}

.view-all:hover {
  color: var(--primary-hover, #25755C);
  transform: translateX(4px);
}

/* ── Works Grid ── */
.works-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
}

/* ── Work Card ── */
.work-card {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  overflow: hidden;
  transition: all var(--duration-normal, .3s) var(--ease-out-expo, cubic-bezier(0.16, 1, 0.3, 1));
  animation: fadeInUp .5s var(--ease-out-expo) both;
  cursor: default;
}

.work-card:hover {
  box-shadow: var(--shadow-card-hover, 0 16px 40px rgba(0,0,0,.12));
  transform: translateY(-4px);
}

.featured {
  border-color: var(--accent-light, #F0E6D0);
}

/* Card Cover */
.card-cover {
  height: 180px;
  position: relative;
  overflow: hidden;
  cursor: pointer;
  background: var(--border-light, #F2EDE6);
}

.thumbnail-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform var(--duration-slow, .5s) var(--ease-out-expo);
}

.work-card:hover .thumbnail-image {
  transform: scale(1.06);
}

.cover-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 500;
  color: var(--primary, #2D8A6E);
  transition: transform var(--duration-slow, .5s) var(--ease-out-expo);
}

.work-card:hover .cover-placeholder {
  transform: scale(1.06);
}

.cover-placeholder .file-icon {
  font-size: 48px;
  opacity: .5;
}

/* Category Badge */
.category-badge {
  position: absolute;
  top: 12px;
  left: 12px;
  padding: 4px 12px;
  border-radius: var(--radius-full, 9999px);
  background: rgba(255, 255, 255, .85);
  backdrop-filter: blur(8px);
  font-size: 11px;
  font-weight: 600;
  color: var(--primary, #2D8A6E);
  z-index: 2;
}

/* Featured Badge */
.featured-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  padding: 4px 12px;
  border-radius: var(--radius-full, 9999px);
  background: var(--accent-light, #F0E6D0);
  font-size: 11px;
  font-weight: 600;
  color: var(--accent-strong, #B8943F);
  z-index: 2;
}

/* Card Body */
.card-body {
  padding: 16px;
}

.card-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin-bottom: 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  cursor: pointer;
  transition: color var(--duration-fast, .18s);
}

.work-card:hover .card-title {
  color: var(--primary, #2D8A6E);
}

.card-author {
  font-size: 12px;
  color: var(--text-light, #888888);
  margin-bottom: 12px;
  display: flex;
  align-items: center;
  gap: 6px;
}

.author-avatar {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: var(--primary-bg, #EDF5F0);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 600;
  color: var(--primary, #2D8A6E);
}

.card-date {
  color: var(--text-placeholder, #B0B0B0);
}

/* Card Actions */
.card-actions {
  display: flex;
  gap: 8px;
}

.btn {
  padding: 7px 14px;
  border-radius: var(--radius-sm, 10px);
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition: all var(--duration-fast, .18s);
  font-family: inherit;
  display: flex;
  align-items: center;
  gap: 6px;
}

.btn-primary {
  background: var(--primary, #2D8A6E);
  color: #fff;
  flex: 1;
  justify-content: center;
}

.btn-primary:hover {
  background: var(--primary-hover, #25755C);
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45,138,110,.2));
}

/* ── Favorite Button ── */
.btn-favorite {
  background: transparent;
  border: 1px solid var(--border-color, #E8E2D8);
  color: var(--text-light, #888888);
  padding: 7px 10px;
  min-width: 36px;
  justify-content: center;
  font-size: 15px;
}

.btn-favorite:hover {
  background: #FDF0EB;
  border-color: var(--accent, #C8AA6E);
  color: var(--accent, #C8AA6E);
}

/* 已收藏状态：金色实心星 */
.btn-favorite.active {
  background: #FFF8E1;
  border-color: #F5A623;
  color: #F5A623;
}

/* ── Skeleton ── */
.skeleton-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
}

.skeleton-card {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  overflow: hidden;
}

.skeleton-cover {
  height: 180px;
  background: linear-gradient(90deg, var(--border-light, #F2EDE6) 25%, var(--bg-hover, #F5F2EC) 50%, var(--border-light, #F2EDE6) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}

.skeleton-body {
  padding: 16px;
}

.skeleton-line {
  height: 12px;
  border-radius: 6px;
  background: var(--border-light, #F2EDE6);
  margin-bottom: 10px;
}

.skeleton-line.w-80 { width: 80%; }
.skeleton-line.w-60 { width: 60%; }
.skeleton-line.w-50 { width: 50%; }

/* ── Empty State ── */
.empty-state {
  text-align: center;
  padding: 64px 40px;
  grid-column: 1 / -1;
}

.empty-icon {
  font-size: 56px;
  margin-bottom: 16px;
  opacity: .3;
}

.empty-state h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 8px;
}

.empty-state p {
  font-size: 14px;
  color: var(--text-light, #888888);
  max-width: 400px;
  margin: 0 auto;
}

/* ── Detail Dialog ── */
::v-deep .detail-dialog {
  border-radius: var(--radius-lg, 18px);
  overflow: hidden;
}

::v-deep .detail-dialog .el-dialog__header {
  padding: 20px 24px 0;
  font-size: 20px;
  font-weight: 700;
}

::v-deep .detail-dialog .el-dialog__body {
  padding: 20px 24px;
}

.detail-header {
  display: flex;
  gap: 20px;
  padding-bottom: 20px;
  border-bottom: 1px solid var(--border-color, #E8E2D8);
}

.detail-cover {
  width: 160px;
  height: 160px;
  border-radius: var(--radius-md, 14px);
  overflow: hidden;
  flex-shrink: 0;
  background: var(--border-light, #F2EDE6);
}

.detail-cover-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.detail-cover-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  font-size: 13px;
  color: var(--text-light, #888888);
}

.detail-meta {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.detail-author {
  display: flex;
  align-items: center;
  gap: 10px;
}

.author-avatar-lg {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--primary-bg, #EDF5F0);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 600;
  color: var(--primary, #2D8A6E);
}

.detail-author-name {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
}

.detail-stats {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: var(--text-secondary, #555555);
}

.detail-date {
  font-size: 12px;
  color: var(--text-light, #888888);
}

.detail-description {
  padding: 20px 0;
  border-bottom: 1px solid var(--border-color, #E8E2D8);
}

.detail-description h4,
.detail-file h4 {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 12px;
}

.detail-description p {
  font-size: 14px;
  line-height: 1.7;
  color: var(--text-secondary, #555555);
  white-space: pre-wrap;
}

.detail-file {
  padding-top: 20px;
}

.file-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.file-tag {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 6px 12px;
  border-radius: var(--radius-full, 9999px);
  background: var(--primary-bg, #EDF5F0);
  font-size: 12px;
  color: var(--primary, #2D8A6E);
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.btn-cancel {
  border-color: var(--border-color, #E8E2D8);
  color: var(--text-secondary, #555555);
}

.btn-cancel:hover {
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
}

.btn-download {
  background: var(--primary, #2D8A6E);
  border-color: var(--primary, #2D8A6E);
}

.btn-download:hover {
  background: var(--primary-hover, #25755C);
}

/* ── Animations ── */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes shimmer {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}

/* ── Responsive ── */
@media (max-width: 768px) {
  .hero { padding: 36px 16px 24px; }
  .hero-title { font-size: 24px; }

  .stats-banner {
    flex-direction: column;
    padding: 0 16px;
    gap: 10px;
  }

  .works-section,
  .featured-section {
    padding: 0 16px;
  }

  .works-grid,
  .skeleton-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 14px;
  }

  .detail-header {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }

  .detail-stats {
    justify-content: center;
  }
}
</style>