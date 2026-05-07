<template>
  <div class="gallery-page">
    <!-- 环境光晕 -->
    <div class="ambient-glow glow-top"></div>
    <div class="ambient-glow glow-bottom"></div>

    <div class="gallery-container">
      <!-- Hero 区域 -->
      <section class="gallery-hero">
        <div class="hero-pattern"></div>
        <div class="hero-content fade-in-up">
          <div class="hero-badge">
            <span class="badge-dot"></span>
            BODA · 数字作品
          </div>
          <h1 class="hero-title">作品<span class="hero-accent">展示</span></h1>
          <p class="hero-desc">浏览和欣赏保定学院师生的优秀数字作品</p>
          <div class="hero-stats">
            <div class="hstat">
              <div class="hstat-icon-ring">
                <i class="el-icon-collection"></i>
              </div>
              <span class="hstat-num">{{ total || 0 }}</span>
              <span class="hstat-label">作品总数</span>
            </div>
            <div class="hstat-divider"></div>
            <div class="hstat">
              <div class="hstat-icon-ring">
                <i class="el-icon-s-grid"></i>
              </div>
              <span class="hstat-num">{{ categoryStats || '多' }}</span>
              <span class="hstat-label">分类</span>
            </div>
            <div class="hstat-divider"></div>
            <div class="hstat">
              <div class="hstat-icon-ring">
                <i class="el-icon-star-on"></i>
              </div>
              <span class="hstat-num">热门</span>
              <span class="hstat-label">精选推荐</span>
            </div>
          </div>
        </div>
      </section>

      <!-- 筛选和搜索 -->
      <section class="filter-section fade-in-up" style="animation-delay: 0.15s">
        <div class="filter-bar glass-card">
          <div class="search-bar">
            <el-input
              v-model="searchQuery"
              placeholder="搜索作品标题或作者..."
              prefix-icon="el-icon-search"
              clearable
              class="modern-search"
              @keyup.enter="loadWorks"
            ></el-input>
          </div>

          <div class="filter-divider"></div>

          <div class="category-filters">
            <span
              v-for="(cat, index) in categories"
              :key="cat.value"
              :class="['filter-chip', { active: categoryFilter === cat.value }]"
              @click="categoryFilter = cat.value; loadWorks()"
            >
              <i :class="getCategoryIcon(cat.value)"></i>
              {{ cat.label }}
            </span>
          </div>

          <div class="sort-options">
            <span
              v-for="sort in sortOptions"
              :key="sort.value"
              :class="['filter-chip sort-chip', { active: sortBy === sort.value }]"
              @click="sortBy = sort.value; loadWorks()"
            >
              {{ sort.label }}
            </span>
          </div>
        </div>
      </section>

      <!-- 作品网格展示 -->
      <section class="works-grid">
        <el-empty v-if="works.length === 0 && !loading" description="未找到相关作品" class="empty-state"></el-empty>

        <el-row :gutter="24">
          <el-col
            :xs="24" :sm="12" :md="8" :lg="6"
            v-for="(work, index) in works"
            :key="work.id"
            class="stagger-fade-in"
            :style="{ animationDelay: `${index * 0.08 + 0.2}s` }"
          >
            <div class="work-card glass-card" @click="handleViewWork(work.id)">
              <div class="cover-image">
                <img v-if="getThumbnailUrl(work)" :src="getThumbnailUrl(work)" :alt="work.title" class="thumbnail-image">
                <ModelThumbnail
                  v-else-if="isModelFile(work)"
                  :model-url="`/api/File/download?fileName=${encodeURIComponent(work.filePath)}`"
                  :background-color="getRandomColor(work.id)"
                  class="model-preview"
                />
                <div v-else class="thumbnail-placeholder" :style="{ backgroundColor: getRandomColor(work.id) }">
                  <div class="placeholder-circle">
                    <i class="el-icon-document"></i>
                  </div>
                  <span>{{ getFileExtension(work) }}</span>
                </div>
                <span class="category-badge">{{ work.category }}</span>
                <div class="hover-overlay">
                  <div class="overlay-bg"></div>
                  <div class="overlay-actions">
                    <button class="overlay-btn view-btn" @click.stop="handleViewWork(work.id)" title="查看详情">
                      <i class="el-icon-view"></i>
                    </button>
                    <button class="overlay-btn download-btn" @click.stop="handleDownloadFile(work)" title="下载文件">
                      <i class="el-icon-download"></i>
                    </button>
                  </div>
                </div>
              </div>

              <div class="work-info">
                <h3 class="work-title" :title="work.title">{{ work.title }}</h3>
                <div class="author-row">
                  <div class="author-avatar-sm">
                    <i class="el-icon-user-solid"></i>
                  </div>
                  <span class="work-author">{{ work.uploadUserName || '未知作者' }}</span>
                  <el-button
                    v-if="work.uploadUserProfilePublic"
                    type="text"
                    size="mini"
                    class="author-profile-btn"
                    @click.stop="goToAuthorProfile(work.UserId)"
                  >
                    <i class="el-icon-link"></i>
                  </el-button>
                </div>
                <div class="work-meta">
                  <span class="meta-item"><i class="el-icon-date"></i> {{ work.uploadDate }}</span>
                  <span class="meta-item"><i class="el-icon-view"></i> {{ work.viewCount || 0 }}</span>
                </div>

                <div class="work-actions">
                  <button
                    class="card-action-btn fav-action"
                    :class="{ 'is-fav': work.isFavorite }"
                    @click.stop="handleToggleFavorite(work)"
                  >
                    <i :class="work.isFavorite ? 'el-icon-star-on' : 'el-icon-star-off'"></i>
                    <span>{{ work.isFavorite ? '已收藏' : '收藏' }}</span>
                  </button>
                  <button class="card-action-btn detail-action" @click.stop="handleViewWork(work.id)">
                    <span>详情</span>
                    <i class="el-icon-arrow-right"></i>
                  </button>
                </div>
              </div>
            </div>
          </el-col>
        </el-row>

        <!-- 分页 -->
        <div class="pagination" v-if="total > 0">
          <el-pagination
            @size-change="handleSizeChange"
            @current-change="handleCurrentChange"
            :current-page="currentPage"
            :page-sizes="[12, 24, 36, 48]"
            :page-size="pageSize"
            layout="total, sizes, prev, pager, next, jumper"
            :total="total"
            background
          ></el-pagination>
        </div>

        <!-- 加载中 -->
        <div v-if="loading" class="loading-container fade-in-up">
          <div class="loading-spinner">
            <div class="spinner-ring"></div>
          </div>
          <p>加载中...</p>
        </div>
      </section>

      <!-- 作品详情对话框 -->
      <el-dialog
        :visible.sync="detailVisible"
        width="800px"
        class="gallery-detail-dialog"
        :close-on-click-modal="true"
      >
        <div v-if="selectedWork" class="detail-content">
          <div class="detail-preview">
            <img v-if="getThumbnailUrl(selectedWork)" :src="getThumbnailUrl(selectedWork)" :alt="selectedWork.title" class="detail-image">
            <div v-else class="detail-placeholder" :style="{ backgroundColor: getRandomColor(selectedWork.id) }">
              <i class="el-icon-document"></i>
              <span>{{ getFileExtension(selectedWork) }}</span>
            </div>
          </div>
          <div class="detail-info">
            <h2 class="detail-title">{{ selectedWork.title }}</h2>
            <div class="detail-author">
              <i class="el-icon-user-solid"></i>
              {{ selectedWork.uploadUserName || '未知作者' }}
            </div>
            <p class="detail-desc" v-if="selectedWork.description">{{ selectedWork.description }}</p>
            <div class="detail-meta-row">
              <span class="detail-meta"><i class="el-icon-date"></i> {{ selectedWork.uploadDate }}</span>
              <span class="detail-meta"><i class="el-icon-view"></i> {{ selectedWork.viewCount || 0 }} 浏览</span>
              <span class="detail-meta"><i class="el-icon-star-off"></i> {{ selectedWork.favoriteCount || 0 }} 收藏</span>
            </div>
            <div class="detail-actions">
              <el-button type="primary" round @click="handleViewWork(selectedWork.id); detailVisible = false">
                <i class="el-icon-view"></i> 查看完整详情
              </el-button>
              <el-button round @click="handleDownloadFile(selectedWork)">
                <i class="el-icon-download"></i> 下载文件
              </el-button>
            </div>
          </div>
        </div>
      </el-dialog>
    </div>
  </div>
</template>

<script>
export default {
  name: 'WorkDisplay',
  data() {
    return {
      works: [],
      categories: [],
      categoryFilter: '',
      searchQuery: '',
      sortBy: 'latest',
      sortOptions: [
        { value: 'latest', label: '最新发布' },
        { value: 'popular', label: '最多浏览' },
        { value: 'favorites', label: '最多收藏' },
      ],
      currentPage: 1,
      pageSize: 12,
      total: 0,
      loading: false,
      detailVisible: false,
      selectedWork: null,
      categoryStats: '多',
    }
  },
  mounted() {
    this.loadWorks()
  },
  methods: {
    async loadWorks() {
      this.loading = true
      try {
        // 动态导入 http 模块以避免循环依赖
        const http = (await import('../../utils/http')).default
        const params = {
          page: this.currentPage,
          pageSize: this.pageSize,
          search: this.searchQuery || undefined,
          category: this.categoryFilter || undefined,
          sortBy: this.sortBy,
        }
        const { data } = await http.get('/api/Work', { params })
        this.works = data.items || data.data || data || []
        this.total = data.total || data.totalCount || this.works.length
        if (data.categories) {
          this.categories = [{ value: '', label: '全部' }, ...data.categories.map(c => ({ value: c, label: c }))]
        } else {
          // 从作品数据中提取分类
          const catSet = new Set()
          this.works.forEach(w => { if (w.category) catSet.add(w.category) })
          this.categories = [{ value: '', label: '全部' }, ...Array.from(catSet).map(c => ({ value: c, label: c }))]
        }
        this.categoryStats = this.categories.length - 1 || '多'
        this.total = this.total
      } catch (e) {
        this.$message.error('加载作品失败')
      } finally {
        this.loading = false
      }
    },
    getCategoryIcon(cat) {
      const map = {
        '': 'el-icon-menu',
        '平面设计': 'el-icon-picture-outline',
        'UI/UX设计': 'el-icon-mobile-phone',
        '三维建模': 'el-icon-s-data',
        '视频动画': 'el-icon-video-camera',
        '交互媒体': 'el-icon-thumb',
        '其他': 'el-icon-more-outline',
      }
      return map[cat] || 'el-icon-folder-opened'
    },
    getThumbnailUrl(work) {
      if (work.coverImage) {
        return `/api/File/download?fileName=${encodeURIComponent(work.coverImage)}`
      }
      if (work.thumbnailPath) {
        return `/api/File/download?fileName=${encodeURIComponent(work.thumbnailPath)}`
      }
      if (work.fileType === 'image' && work.filePath) {
        return `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
      }
      return null
    },
    isModelFile(work) {
      const ext = (work.filePath || work.fileName || '').toLowerCase().split('.').pop()
      return ['glb', 'gltf', 'obj', 'fbx', 'stl'].includes(ext)
    },
    getFileExtension(work) {
      const ext = (work.filePath || work.fileName || '').toUpperCase().split('.').pop()
      return ext || 'FILE'
    },
    getRandomColor(id) {
      const colors = ['#2D8A6E', '#45A884', '#5DB89E', '#C8AA6E', '#B8943F', '#5B9BD5', '#F09342']
      const index = (typeof id === 'number' ? id : (id || '').toString().charCodeAt(0) || 0) % colors.length
      return colors[index]
    },
    handleViewWork(id) {
      this.$router.push(`/works/${id}`)
    },
    handleDownloadFile(work) {
      if (!work.filePath) {
        this.$message.error('无文件可下载')
        return
      }
      import('../../utils/http').then(({ default: http }) => {
        http.get('/api/File/download', { params: { fileName: work.filePath }, responseType: 'blob' })
          .then(res => {
            const url = URL.createObjectURL(res.data)
            const a = document.createElement('a')
            a.href = url
            a.download = work.fileName || 'work'
            a.click()
            URL.revokeObjectURL(url)
          })
          .catch(() => {
            this.$message.error('下载失败')
          })
      })
    },
    async handleToggleFavorite(work) {
      try {
        const http = (await import('../../utils/http')).default
        if (work.isFavorite) {
          await http.delete(`/api/Work/${work.id}/favorite`)
          work.isFavorite = false
          this.$message.success('已取消收藏')
        } else {
          await http.post(`/api/Work/${work.id}/favorite`)
          work.isFavorite = true
          this.$message.success('已收藏')
        }
      } catch (e) {
        this.$message.error('操作失败')
      }
    },
    goToAuthorProfile(userId) {
      this.$router.push(`/profile/${userId}`)
    },
    handleSizeChange(val) {
      this.pageSize = val
      this.currentPage = 1
      this.loadWorks()
    },
    handleCurrentChange(val) {
      this.currentPage = val
      this.loadWorks()
      window.scrollTo({ top: 0, behavior: 'smooth' })
    },
  },
}
</script>

<style scoped>
/* ===== 全局容器 ===== */
.gallery-page {
  min-height: 100vh;
  background: var(--bg-page);
  position: relative;
  overflow: hidden;
}

.ambient-glow {
  position: fixed;
  border-radius: 50%;
  filter: blur(140px);
  pointer-events: none;
  z-index: 0;
  opacity: 0.5;
}

.glow-top {
  width: 500px;
  height: 500px;
  background: radial-gradient(circle, rgba(45, 138, 110, 0.1) 0%, transparent 70%);
  top: -180px;
  right: -120px;
  animation: floatGlow 8s ease-in-out infinite alternate;
}

.glow-bottom {
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, rgba(200, 170, 110, 0.08) 0%, transparent 70%);
  bottom: -120px;
  left: -80px;
  animation: floatGlow 10s ease-in-out infinite alternate-reverse;
}

@keyframes floatGlow {
  0% { transform: translate(0, 0) scale(1); }
  100% { transform: translate(30px, -20px) scale(1.1); }
}

.gallery-container {
  position: relative;
  z-index: 1;
  max-width: 1200px;
  margin: 0 auto;
  padding: 28px 24px 80px;
}

/* ===== Hero 区域 ===== */
.gallery-hero {
  position: relative;
  background: linear-gradient(135deg, var(--primary-bg) 0%, #F7F4F0 40%, #FCFAF5 100%);
  border-radius: var(--radius-xl);
  padding: 48px 48px 40px;
  margin-bottom: 28px;
  overflow: hidden;
  border: 1px solid var(--border-light);
  box-shadow: var(--shadow-sm);
}

.hero-pattern {
  position: absolute;
  top: -80px;
  right: -80px;
  width: 400px;
  height: 400px;
  background:
    radial-gradient(circle at 50% 40%, rgba(45, 138, 110, 0.05) 0%, transparent 60%),
    radial-gradient(circle at 40% 60%, rgba(200, 170, 110, 0.04) 0%, transparent 60%);
  border-radius: 50%;
  pointer-events: none;
  animation: pulseGlow 6s ease-in-out infinite alternate;
}

@keyframes pulseGlow {
  0% { transform: scale(1); opacity: 0.6; }
  100% { transform: scale(1.15); opacity: 1; }
}

.hero-content {
  position: relative;
  z-index: 1;
}

.hero-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 600;
  color: var(--primary);
  background: rgba(45, 138, 110, 0.08);
  padding: 6px 16px;
  border-radius: var(--radius-full);
  margin-bottom: 16px;
  letter-spacing: 0.5px;
}

.badge-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--primary);
  animation: dotPulse 2s ease-in-out infinite;
}

@keyframes dotPulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}

.hero-title {
  font-size: 40px;
  font-weight: 800;
  color: var(--text-main);
  margin: 0 0 12px;
  letter-spacing: -0.5px;
}

.hero-accent {
  color: var(--primary);
}

.hero-desc {
  font-size: 15px;
  color: var(--text-secondary);
  margin: 0 0 28px;
  line-height: 1.6;
}

.hero-stats {
  display: flex;
  align-items: center;
  gap: 28px;
  padding-top: 20px;
  border-top: 1px solid rgba(0, 0, 0, 0.05);
}

.hstat {
  display: flex;
  align-items: center;
  gap: 12px;
}

.hstat-icon-ring {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: rgba(45, 138, 110, 0.08);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary);
  font-size: 18px;
  flex-shrink: 0;
}

.hstat-num {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main);
}

.hstat-label {
  font-size: 13px;
  color: var(--text-light);
}

.hstat-divider {
  width: 1px;
  height: 28px;
  background: rgba(0, 0, 0, 0.06);
  flex-shrink: 0;
}

/* ===== 筛选栏 ===== */
.filter-bar {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 20px;
  margin-bottom: 28px;
  flex-wrap: wrap;
}

.glass-card {
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px) saturate(180%);
  -webkit-backdrop-filter: blur(16px) saturate(180%);
  border-radius: var(--radius-xl);
  border: 1px solid rgba(232, 226, 216, 0.6);
  box-shadow: var(--shadow-sm);
}

.search-bar {
  width: 240px;
  flex-shrink: 0;
}

.modern-search ::v-deep .el-input__inner {
  border-radius: var(--radius-full);
  border-color: transparent;
  background: var(--bg-page);
  font-size: 13px;
  height: 38px;
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.modern-search ::v-deep .el-input__inner:focus {
  background: white;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px var(--primary-glow);
}

.filter-divider {
  width: 1px;
  height: 24px;
  background: var(--border-light);
  flex-shrink: 0;
}

.category-filters,
.sort-options {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  align-items: center;
}

.filter-chip {
  font-size: 13px;
  font-weight: 500;
  padding: 7px 16px;
  border-radius: var(--radius-full);
  background: var(--bg-page);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all var(--duration-normal) var(--ease-out-expo);
  white-space: nowrap;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  border: 1px solid transparent;
}

.filter-chip:hover {
  color: var(--primary);
  background: var(--primary-bg);
  border-color: rgba(45, 138, 110, 0.15);
}

.filter-chip.active {
  background: linear-gradient(135deg, var(--primary), var(--primary-light));
  color: white;
  border-color: transparent;
  box-shadow: 0 4px 12px rgba(45, 138, 110, 0.3);
}

.sort-chip.active {
  background: var(--primary-bg);
  color: var(--primary);
  border-color: var(--primary);
  box-shadow: none;
  font-weight: 600;
}

/* ===== 作品网格 ===== */
.works-grid {
  min-height: 400px;
}

.empty-state {
  padding: 80px 0;
}

/* ── 作品卡片 ── */
.work-card {
  border-radius: var(--radius-xl);
  overflow: hidden;
  cursor: pointer;
  transition: all 0.4s var(--ease-out-expo);
  padding: 0;
  margin-bottom: 24px;
  background: white;
}

.work-card:hover {
  transform: translateY(-6px);
  box-shadow: var(--shadow-card-hover);
  border-color: rgba(45, 138, 110, 0.2);
}

/* ── 封面图 ── */
.cover-image {
  position: relative;
  width: 100%;
  height: 200px;
  overflow: hidden;
  background: linear-gradient(135deg, #F5F2EC, #EDE8DD);
}

.thumbnail-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s var(--ease-out-expo);
}

.work-card:hover .thumbnail-image {
  transform: scale(1.06);
}

.model-preview {
  width: 100%;
  height: 100%;
}

.thumbnail-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: rgba(255, 255, 255, 0.9);
  font-size: 14px;
  font-weight: 600;
  transition: transform 0.5s var(--ease-out-expo);
}

.work-card:hover .thumbnail-placeholder {
  transform: scale(1.06);
}

.placeholder-circle {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.25);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 26px;
}

.category-badge {
  position: absolute;
  top: 12px;
  left: 12px;
  background: rgba(0, 0, 0, 0.55);
  backdrop-filter: blur(8px);
  color: white;
  font-size: 11px;
  font-weight: 600;
  padding: 4px 12px;
  border-radius: var(--radius-full);
  letter-spacing: 0.5px;
  z-index: 2;
}

/* ── 悬浮遮罩 ── */
.hover-overlay {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.35s var(--ease-out-expo);
  z-index: 3;
}

.work-card:hover .hover-overlay {
  opacity: 1;
}

.overlay-bg {
  position: absolute;
  inset: 0;
  background: linear-gradient(180deg, rgba(45, 138, 110, 0.15) 0%, rgba(0, 0, 0, 0.55) 100%);
}

.overlay-actions {
  position: relative;
  z-index: 1;
  display: flex;
  gap: 14px;
  transform: translateY(12px);
  transition: transform 0.35s var(--ease-out-expo);
}

.work-card:hover .overlay-actions {
  transform: translateY(0);
}

.overlay-btn {
  width: 46px;
  height: 46px;
  border-radius: 50%;
  border: none;
  cursor: pointer;
  font-size: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.25s var(--ease-out-expo);
  color: white;
}

.view-btn {
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(8px);
}

.view-btn:hover {
  background: var(--primary);
  transform: scale(1.1);
}

.download-btn {
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(8px);
}

.download-btn:hover {
  background: var(--accent-strong);
  transform: scale(1.1);
}

/* ── 卡片信息 ── */
.work-info {
  padding: 16px 18px 18px;
}

.work-title {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 10px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.4;
}

.author-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.author-avatar-sm {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: var(--primary-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary);
  font-size: 11px;
  flex-shrink: 0;
}

.work-author {
  font-size: 13px;
  color: var(--text-secondary);
  flex: 1;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.author-profile-btn {
  flex-shrink: 0;
  padding: 2px;
  color: var(--primary);
}

.work-meta {
  display: flex;
  gap: 14px;
  margin-bottom: 14px;
}

.meta-item {
  font-size: 12px;
  color: var(--text-light);
  display: flex;
  align-items: center;
  gap: 4px;
}

.meta-item i {
  font-size: 13px;
}

/* ── 卡片操作按钮 ── */
.work-actions {
  display: flex;
  gap: 8px;
  padding-top: 12px;
  border-top: 1px solid var(--border-light);
}

.card-action-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
  padding: 8px 0;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-color);
  background: var(--surface);
  cursor: pointer;
  font-size: 13px;
  font-weight: 500;
  transition: all 0.25s var(--ease-out-expo);
  color: var(--text-secondary);
  font-family: var(--font-main);
}

.card-action-btn:hover {
  border-color: var(--primary);
  color: var(--primary);
  background: var(--primary-bg);
}

.fav-action.is-fav {
  background: linear-gradient(135deg, #F09342, #FFB74D);
  border-color: transparent;
  color: white;
}

.fav-action.is-fav:hover {
  background: linear-gradient(135deg, #E08532, #F0A835);
  color: white;
}

.detail-action {
  background: linear-gradient(135deg, var(--primary), var(--primary-light));
  border-color: transparent;
  color: white;
  font-weight: 600;
}

.detail-action:hover {
  box-shadow: 0 4px 12px rgba(45, 138, 110, 0.35);
  color: white;
  transform: translateY(-1px);
}

/* ===== 分页 ===== */
.pagination {
  display: flex;
  justify-content: center;
  margin-top: 40px;
}

/* ===== 加载中 ===== */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 0;
  gap: 16px;
  color: var(--text-light);
  font-size: 14px;
}

.loading-spinner {
  position: relative;
  width: 44px;
  height: 44px;
}

.spinner-ring {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  border: 3px solid var(--border-light);
  border-top-color: var(--primary);
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* ===== 详情对话框 ===== */
::v-deep .gallery-detail-dialog {
  border-radius: var(--radius-xl) !important;
  overflow: hidden;
}

::v-deep .gallery-detail-dialog .el-dialog__header {
  padding: 0;
}

::v-deep .gallery-detail-dialog .el-dialog__headerbtn {
  top: 16px;
  right: 16px;
  z-index: 5;
}

::v-deep .gallery-detail-dialog .el-dialog__headerbtn .el-dialog__close {
  color: white;
  font-size: 20px;
  background: rgba(0, 0, 0, 0.4);
  border-radius: 50%;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

::v-deep .gallery-detail-dialog .el-dialog__body {
  padding: 0;
}

.detail-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  min-height: 320px;
}

.detail-preview {
  background: linear-gradient(135deg, #F5F2EC, #EDE8DD);
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 320px;
}

.detail-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.detail-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  color: rgba(255, 255, 255, 0.9);
  font-size: 16px;
  font-weight: 600;
}

.detail-placeholder i {
  font-size: 40px;
}

.detail-info {
  padding: 28px 24px;
  display: flex;
  flex-direction: column;
}

.detail-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 10px;
}

.detail-author {
  font-size: 14px;
  color: var(--text-secondary);
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 14px;
}

.detail-desc {
  font-size: 14px;
  color: var(--text-regular);
  line-height: 1.7;
  margin: 0 0 auto;
  flex: 1;
  overflow-y: auto;
}

.detail-meta-row {
  display: flex;
  gap: 16px;
  padding: 14px 0;
  border-top: 1px solid var(--border-light);
  margin-top: 14px;
}

.detail-meta {
  font-size: 12px;
  color: var(--text-light);
  display: flex;
  align-items: center;
  gap: 4px;
}

.detail-actions {
  display: flex;
  gap: 10px;
  margin-top: 14px;
}

/* ===== 动画 ===== */
.fade-in-up {
  animation: fadeInUp 0.6s var(--ease-out-expo) both;
}

.stagger-fade-in {
  animation: fadeInUp 0.6s var(--ease-out-expo) both;
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(24px); }
  to { opacity: 1; transform: translateY(0); }
}

/* ===== 响应式 ===== */
@media (max-width: 900px) {
  .gallery-hero {
    padding: 32px 24px 28px;
  }

  .hero-title {
    font-size: 28px;
  }

  .hero-stats {
    flex-wrap: wrap;
    gap: 16px;
  }

  .hstat-divider {
    display: none;
  }

  .filter-bar {
    flex-direction: column;
    align-items: stretch;
  }

  .search-bar {
    width: 100%;
  }

  .filter-divider {
    display: none;
  }

  .detail-content {
    grid-template-columns: 1fr;
  }

  .detail-preview {
    min-height: 200px;
  }
}

@media (max-width: 480px) {
  .gallery-container {
    padding: 16px 12px 60px;
  }

  .gallery-hero {
    padding: 24px 16px 20px;
    border-radius: var(--radius-lg);
  }

  .hero-title {
    font-size: 24px;
  }

  .work-card {
    margin-bottom: 16px;
  }
}
</style>