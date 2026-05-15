<template>
  <div class="gallery-page">
    <div class="gallery-container">
      <!-- Hero 区域 -->
      <section class="gallery-hero">
        <h1 class="hero-title">作品展示</h1>
        <p class="hero-desc">浏览和欣赏保定学院师生的优秀数字作品</p>
        <div class="hero-stats">
          <div class="stat-item">
            <span class="stat-num">{{ total || 0 }}</span>
            <span class="stat-label">作品总数</span>
          </div>
          <div class="stat-item">
            <span class="stat-num">{{ categoryStats || '多' }}</span>
            <span class="stat-label">分类</span>
          </div>
        </div>
      </section>

      <!-- 筛选和搜索 -->
      <section class="filter-section">
        <div class="filter-bar">
          <div class="search-box">
            <el-input
              v-model="searchQuery"
              placeholder="搜索作品标题或作者..."
              clearable
              @keyup.enter="loadWorks"
            ></el-input>
            <el-button type="primary" icon="el-icon-search" @click="loadWorks">搜索</el-button>
          </div>
          <div class="filter-group">
            <span
              v-for="cat in categories"
              :key="cat.value"
              :class="['filter-btn', { active: categoryFilter === cat.value }]"
              @click="categoryFilter = cat.value; loadWorks()"
            >
              {{ cat.label }}
            </span>
          </div>
          <div class="sort-group">
            <span
              v-for="sort in sortOptions"
              :key="sort.value"
              :class="['sort-btn', { active: sortBy === sort.value }]"
              @click="sortBy = sort.value; loadWorks()"
            >
              {{ sort.label }}
            </span>
          </div>
        </div>
      </section>

      <!-- 作品网格展示 -->
      <section class="works-section">
        <el-empty v-if="works.length === 0 && !loading" description="未找到相关作品"></el-empty>

        <div v-else class="works-grid">
          <div
            v-for="work in works"
            :key="work.id"
            class="work-card"
            @click="handleViewWork(work.id)"
          >
            <div class="card-cover">
              <img v-if="getThumbnailUrl(work)" :src="getThumbnailUrl(work)" :alt="work.title" class="cover-img">
              <div v-else class="cover-placeholder" :style="{ background: getGradient(work.id) }">
                <span class="file-icon">{{ getFileEmoji(work) }}</span>
                <span class="file-ext">{{ getFileExtension(work) }}</span>
              </div>
              <span class="category-tag">{{ work.category || '未分类' }}</span>
            </div>
            <div class="card-body">
              <h3 class="card-title">{{ work.title }}</h3>
              <div class="card-meta">
                <span class="author">{{ work.uploadUserName || '未知作者' }}</span>
                <span class="views">👁 {{ work.views || 0 }}</span>
                <span class="favorites">⭐ {{ work.favorites || 0 }}</span>
              </div>
              <div class="card-actions">
                <button
                  class="action-btn"
                  :class="{ active: work.isFavorited }"
                  @click.stop="handleToggleFavorite(work)"
                  title="收藏"
                >
                  <i :class="work.isFavorited ? 'el-icon-star-on' : 'el-icon-star-off'"></i>
                  <span>{{ work.isFavorited ? '已收藏' : '收藏' }}</span>
                </button>
                <button class="action-btn" @click.stop="handleDownloadFile(work)" title="下载">
                  <i class="el-icon-download"></i>
                  <span>下载</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- 分页 -->
        <div class="pagination-wrapper" v-if="total > 0">
          <el-pagination
            @size-change="handleSizeChange"
            @current-change="handleCurrentChange"
            :current-page="currentPage"
            :page-sizes="[12, 24, 36]"
            :page-size="pageSize"
            layout="total, sizes, prev, pager, next"
            :total="total"
            background
          ></el-pagination>
        </div>

        <!-- 加载中 -->
        <div v-if="loading" class="loading-state">
          <i class="el-icon-loading"></i>
          <span>加载中...</span>
        </div>
      </section>

      <!-- 作品详情对话框 -->
      <el-dialog
        :visible.sync="detailVisible"
        width="700px"
        :close-on-click-modal="true"
        append-to-body
        :modal-append-to-body="false"
        :modal="false"
      >
        <div v-if="selectedWork" class="detail-content">
          <div class="detail-cover">
            <img v-if="getThumbnailUrl(selectedWork)" :src="getThumbnailUrl(selectedWork)" :alt="selectedWork.title">
            <div v-else class="detail-placeholder" :style="{ background: getRandomColor(selectedWork.id) }">
              <span>{{ getFileExtension(selectedWork) }}</span>
            </div>
          </div>
          <div class="detail-info">
            <h2>{{ selectedWork.title }}</h2>
            <p class="detail-desc">{{ selectedWork.description }}</p>
            <div class="detail-meta">
              <span>作者：{{ selectedWork.uploadUserName || '未知作者' }}</span>
              <span>浏览：{{ selectedWork.viewCount || 0 }}</span>
              <span>收藏：{{ selectedWork.favoriteCount || 0 }}</span>
            </div>
            <div class="detail-actions">
              <el-button type="primary" round @click="handleViewWork(selectedWork.id); detailVisible = false">
                查看详情
              </el-button>
              <el-button round @click="handleDownloadFile(selectedWork)">
                下载文件
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
        { value: 'latest', label: '最新' },
        { value: 'popular', label: '热门' },
        { value: 'favorites', label: '收藏' },
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
  beforeRouteLeave(to, from, next) {
    this.detailVisible = false
    next()
  },
  mounted() {
    this.loadWorks()
  },
  methods: {
    async loadWorks() {
      this.loading = true
      try {
        const http = (await import('../../utils/http')).default
        const params = {
          page: this.currentPage,
          pageSize: this.pageSize,
          search: this.searchQuery || undefined,
          category: this.categoryFilter || undefined,
          sortBy: this.sortBy,
        }
        const { data } = await http.get('/api/Work', { params })
        const worksData = data.items || data.data || data || []
        
        for (const work of worksData) {
          try {
            const favResponse = await http.get(`/api/Work/${work.id}/is-favorite`)
            work.isFavorited = favResponse.data.isFavorite || false
          } catch (e) {
            work.isFavorited = false
          }
        }
        
        this.works = worksData
        this.total = data.total || data.totalCount || this.works.length
        if (data.categories) {
          this.categories = [{ value: '', label: '全部' }, ...data.categories.map(c => ({ value: c, label: c }))]
        } else {
          const catSet = new Set()
          this.works.forEach(w => { if (w.category) catSet.add(w.category) })
          this.categories = [{ value: '', label: '全部' }, ...Array.from(catSet).map(c => ({ value: c, label: c }))]
        }
        this.categoryStats = this.categories.length - 1 || '多'
      } catch (e) {
        this.$message.error('加载作品失败')
      } finally {
        this.loading = false
      }
    },
    getThumbnailUrl(work) {
      if (work.coverImage) {
        return `/api/File/download?fileName=${encodeURIComponent(work.coverImage)}`
      }
      if (work.thumbnailPath) {
        return `/api/File/download?fileName=${encodeURIComponent(work.thumbnailPath)}`
      }
      if (work.filePath) {
        const ext = (work.filePath || '').toLowerCase().split('.').pop()
        const imageExts = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp']
        if (imageExts.includes(ext)) {
          return `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
        }
      }
      if (work.previewImage) {
        return `/api/File/download?fileName=${encodeURIComponent(work.previewImage)}`
      }
      if (work.fileType === 'image' && work.filePath) {
        return `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
      }
      return null
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
    getGradient(id) {
      const gradients = [
        'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
        'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
        'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
        'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
        'linear-gradient(135deg, #a18cd1 0%, #fbc2eb 100%)',
        'linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)',
        'linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%)'
      ]
      const index = (typeof id === 'number' ? id : (id || '').toString().charCodeAt(0) || 0) % gradients.length
      return gradients[index]
    },
    getFileEmoji(work) {
      const ext = (work.filePath || work.fileName || '').toLowerCase().split('.').pop()
      const emojiMap = {
        'pdf': '📕',
        'doc': '📘',
        'docx': '📘',
        'ppt': '📗',
        'pptx': '📗',
        'xls': '📙',
        'xlsx': '📙',
        'zip': '📦',
        'rar': '📦',
        'jpg': '🖼️',
        'jpeg': '🖼️',
        'png': '🖼️',
        'gif': '🖼️',
        'mp4': '🎬',
        'mp3': '🎵',
        'txt': '📝',
        'md': '📝',
        'html': '🌐',
        'css': '🎨',
        'js': '💻',
        'json': '📄',
        'xml': '📄'
      }
      return emojiMap[ext] || '📄'
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
        if (!localStorage.getItem('token')) {
          this.$message.error('请先登录')
          return
        }
        const http = (await import('../../utils/http')).default
        if (work.isFavorited) {
          const response = await http.delete(`/api/Work/${work.id}/favorite`)
          work.favorites = response.data.favorites
          work.isFavorited = false
          this.$message.success('已取消收藏')
        } else {
          const response = await http.post(`/api/Work/${work.id}/favorite`, {})
          work.favorites = response.data.favorites
          work.isFavorited = true
          this.$message.success('收藏成功')
        }
      } catch (error) {
        if (error.response && error.response.data && error.response.data.message) {
          this.$message.error(error.response.data.message)
        } else {
          this.$message.error('收藏操作失败，请稍后重试')
        }
      }
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
.gallery-page {
  min-height: 100vh;
  background: #F8F9F5;
}

.gallery-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

.gallery-hero {
  text-align: center;
  padding: 48px 24px;
  margin-bottom: 32px;
}

.hero-title {
  font-size: 36px;
  font-weight: 700;
  color: #1A1A1A;
  margin: 0 0 12px;
}

.hero-desc {
  font-size: 16px;
  color: #888888;
  margin: 0 0 32px;
}

.hero-stats {
  display: flex;
  justify-content: center;
  gap: 48px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.stat-num {
  font-size: 24px;
  font-weight: 700;
  color: #2D8A6E;
}

.stat-label {
  font-size: 14px;
  color: #888888;
}

.filter-section {
  margin-bottom: 24px;
}

.filter-bar {
  background: white;
  border-radius: 12px;
  padding: 16px 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.search-box {
  flex: 1;
  min-width: 200px;
  display: flex;
  gap: 8px;
}

.search-box ::v-deep .el-input {
  flex: 1;
}

.search-box ::v-deep .el-input__inner {
  border-radius: 8px;
  border: 1px solid #E8E2D8;
}

.filter-group,
.sort-group {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.filter-btn,
.sort-btn {
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 14px;
  color: #555555;
  background: #F8F9F5;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.2s;
}

.filter-btn:hover,
.sort-btn:hover {
  background: #EDF5F0;
  color: #2D8A6E;
}

.filter-btn.active,
.sort-btn.active {
  background: #2D8A6E;
  color: white;
}

.works-section {
  min-height: 400px;
}

.works-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  margin-bottom: 32px;
}

.work-card {
  background: linear-gradient(145deg, #ffffff, #f8f9f5);
  border-radius: 18px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.35s cubic-bezier(0.16, 1, 0.3, 1);
  border: 1px solid #E8E2D8;
}

.work-card:hover {
  transform: translateY(-8px) scale(1.02);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.12);
  border-color: #D4CEC6;
}

.card-cover {
  position: relative;
  height: 220px;
  overflow: hidden;
  background: #F8F9F5;
}

.cover-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.cover-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: white;
  gap: 8px;
}

.cover-placeholder .file-icon {
  font-size: 48px;
  opacity: 0.9;
}

.cover-placeholder .file-ext {
  font-size: 14px;
  font-weight: 600;
  opacity: 0.9;
}

.category-tag {
  position: absolute;
  top: 12px;
  left: 12px;
  padding: 6px 14px;
  background: rgba(255, 255, 255, 0.95);
  border-radius: 9999px;
  font-size: 12px;
  font-weight: 600;
  color: #667eea;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.card-body {
  padding: 20px;
}

.card-title {
  font-size: 16px;
  font-weight: 700;
  color: #1A1A1A;
  margin: 0 0 12px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.card-meta {
  display: flex;
  align-items: center;
  gap: 16px;
  font-size: 13px;
  color: #666666;
  margin-bottom: 16px;
}

.card-meta .author {
  display: flex;
  align-items: center;
  gap: 6px;
}

.card-meta .author::before {
  content: '👤';
  font-size: 14px;
}

.card-meta .views {
  display: flex;
  align-items: center;
  gap: 4px;
}

.card-meta .favorites {
  display: flex;
  align-items: center;
  gap: 4px;
}

.card-actions {
  display: flex;
  gap: 10px;
}

.action-btn {
  flex: 1;
  padding: 10px 14px;
  border: 1px solid #E8E2D8;
  border-radius: 10px;
  background: white;
  cursor: pointer;
  transition: all 0.25s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  color: #888888;
  font-size: 13px;
}

.action-btn:hover {
  border-color: #667eea;
  color: #667eea;
  background: #F5F7FF;
  transform: translateY(-1px);
}

.action-btn.active {
  background: linear-gradient(135deg, #FFF8E1 0%, #FFF0C6 100%);
  border-color: #F5A623;
  color: #B8860B;
  box-shadow: 0 2px 8px rgba(245, 166, 35, 0.2);
}

.pagination-wrapper {
  display: flex;
  justify-content: center;
  padding: 24px 0;
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
  color: #888888;
  font-size: 14px;
}

.loading-state i {
  font-size: 32px;
  margin-bottom: 12px;
}

.detail-content {
  display: flex;
  gap: 24px;
}

.detail-cover {
  width: 200px;
  height: 200px;
  border-radius: 12px;
  overflow: hidden;
  flex-shrink: 0;
  background: #F2EDE6;
}

.detail-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.detail-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 24px;
  font-weight: 600;
}

.detail-info {
  flex: 1;
}

.detail-info h2 {
  font-size: 20px;
  font-weight: 700;
  color: #1A1A1A;
  margin: 0 0 12px;
}

.detail-desc {
  font-size: 14px;
  color: #555555;
  line-height: 1.6;
  margin: 0 0 16px;
}

.detail-meta {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: #888888;
  margin-bottom: 20px;
}

.detail-actions {
  display: flex;
  gap: 12px;
}

@media (max-width: 768px) {
  .gallery-hero {
    padding: 32px 16px;
  }

  .hero-title {
    font-size: 28px;
  }

  .hero-stats {
    gap: 24px;
  }

  .filter-bar {
    padding: 12px 16px;
  }

  .works-grid {
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    gap: 16px;
  }

  .detail-content {
    flex-direction: column;
  }

  .detail-cover {
    width: 100%;
    height: 200px;
  }
}
</style>
