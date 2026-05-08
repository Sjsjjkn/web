<template>
  <div class="favorites-page">
    <div class="favorites-container">
      <!-- Hero 区域 -->
      <section class="gallery-hero">
        <h1 class="hero-title">我的收藏</h1>
        <p class="hero-desc">你收藏的每一件作品，都是灵感的火花</p>
        <div class="hero-stats">
          <div class="stat-item">
            <span class="stat-num">{{ total || 0 }}</span>
            <span class="stat-label">收藏总数</span>
          </div>
          <el-button type="primary" icon="el-icon-refresh" round @click="loadCollections">刷新列表</el-button>
        </div>
      </section>

      <!-- 筛选栏 -->
      <section class="filter-section">
        <div class="filter-bar">
          <div class="search-box">
            <el-input
              v-model="searchQuery"
              placeholder="搜索作品标题或作者..."
              clearable
              @keyup.enter="loadCollections"
            ></el-input>
            <el-button type="primary" icon="el-icon-search" @click="loadCollections">搜索</el-button>
          </div>
          <div class="filter-group">
            <span
              v-for="cat in categories"
              :key="cat.value"
              :class="['filter-btn', { active: activeCategory === cat.value }]"
              @click="activeCategory = cat.value; loadCollections()"
            >
              {{ cat.label }}
            </span>
          </div>
        </div>
      </section>

      <!-- 作品网格 -->
      <section class="works-section">
        <el-empty v-if="collections.length === 0 && !loading" description="暂无收藏作品"></el-empty>

        <div v-else class="works-grid">
          <div
            v-for="item in collections"
            :key="item.id"
            class="work-card"
            @click="handleViewWork(item.work)"
          >
            <div class="card-cover">
              <img v-if="getThumbnailUrl(item.work)" :src="getThumbnailUrl(item.work)" :alt="item.work.title" class="cover-img">
              <div v-else class="cover-placeholder" :style="{ background: getRandomColor(item.work.id) }">
                <span class="file-ext">{{ getFileExtension(item.work) }}</span>
              </div>
              <span class="category-tag">{{ item.work.category || '未分类' }}</span>
              <div class="card-overlay">
                <button class="overlay-btn" @click.stop="handleViewWork(item.work)">
                  <i class="el-icon-view"></i>
                </button>
              </div>
            </div>
            <div class="card-body">
              <h3 class="card-title">{{ item.work.title }}</h3>
              <div class="card-meta">
                <span class="author">{{ item.work.uploadUserName || '未知作者' }}</span>
                <span class="views">👁 {{ item.work.views || 0 }}</span>
              </div>
              <div class="card-actions">
                <button class="action-btn" @click.stop="handleViewWork(item.work)">
                  <i class="el-icon-view"></i>
                  <span>查看</span>
                </button>
                <button class="action-btn cancel" @click.stop="handleCancelFavorite(item)">
                  <i class="el-icon-star-off"></i>
                  <span>取消收藏</span>
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
      </section>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Favorites',
  data() {
    return {
      collections: [],
      total: 0,
      currentPage: 1,
      pageSize: 12,
      loading: false,
      searchQuery: '',
      activeCategory: '',
      categories: [{ value: '', label: '全部' }]
    }
  },
  mounted() {
    this.loadCollections()
  },
  methods: {
    async loadCollections() {
      this.loading = true
      try {
        const http = (await import('../../utils/http')).default
        const params = {
          page: this.currentPage,
          pageSize: this.pageSize,
          search: this.searchQuery || undefined,
          category: this.activeCategory || undefined
        }
        const { data } = await http.get('/api/Favorite', { params })
        this.collections = data.items || data.data || data || []
        this.total = data.total || data.totalCount || this.collections.length
        if (data.categories) {
          this.categories = [{ value: '', label: '全部' }, ...data.categories.map(c => ({ value: c, label: c }))]
        }
      } catch (e) {
        this.$message.error('加载收藏失败')
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
      return colors[(id || 0) % colors.length]
    },
    handleViewWork(work) {
      this.$router.push(`/works/${work.id}`)
    },
    async handleCancelFavorite(item) {
      try {
        await this.$confirm('确定要取消收藏这个作品吗？', '提示', { type: 'warning' })
        const http = (await import('../../utils/http')).default
        await http.delete(`/api/Work/${item.work.id}/favorite`)
        this.$message.success('已取消收藏')
        this.loadCollections()
      } catch (e) {
        if (e !== 'cancel') {
          this.$message.error('操作失败')
        }
      }
    },
    handleSizeChange(size) {
      this.pageSize = size
      this.loadCollections()
    },
    handleCurrentChange(page) {
      this.currentPage = page
      this.loadCollections()
    }
  }
}
</script>

<style scoped>
.favorites-page {
  min-height: 100vh;
  background: #f5f7fa;
  padding: 24px;
}

.favorites-container {
  max-width: 1400px;
  margin: 0 auto;
}

/* Hero 区域 */
.gallery-hero {
  text-align: center;
  padding: 40px 20px;
  margin-bottom: 24px;
}

.hero-title {
  font-size: 32px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 12px;
}

.hero-desc {
  font-size: 15px;
  color: #666;
  margin: 0 0 24px;
}

.hero-stats {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 24px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.stat-num {
  font-size: 28px;
  font-weight: 700;
  color: #2D8A6E;
}

.stat-label {
  font-size: 13px;
  color: #888;
}

/* 筛选栏 */
.filter-section {
  margin-bottom: 24px;
}

.filter-bar {
  background: #fff;
  border-radius: 12px;
  padding: 16px 20px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 16px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}

.search-box {
  display: flex;
  gap: 8px;
  flex: 1;
  min-width: 280px;
}

.search-box .el-input {
  flex: 1;
}

.filter-group {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.filter-btn {
  padding: 6px 14px;
  border-radius: 6px;
  font-size: 13px;
  color: #666;
  background: #f5f5f5;
  cursor: pointer;
  transition: all 0.2s;
}

.filter-btn:hover {
  background: #e8e8e8;
}

.filter-btn.active {
  background: #2D8A6E;
  color: #fff;
}

/* 作品网格 */
.works-section {
  margin-bottom: 32px;
}

.works-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.work-card {
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  transition: transform 0.2s, box-shadow 0.2s;
  cursor: pointer;
}

.work-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0,0,0,0.1);
}

.card-cover {
  position: relative;
  height: 180px;
  overflow: hidden;
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
  align-items: center;
  justify-content: center;
}

.file-ext {
  font-size: 24px;
  font-weight: 600;
  color: rgba(255,255,255,0.9);
  text-transform: uppercase;
}

.category-tag {
  position: absolute;
  top: 12px;
  left: 12px;
  padding: 4px 10px;
  background: rgba(0,0,0,0.6);
  color: #fff;
  font-size: 12px;
  border-radius: 4px;
}

.card-overlay {
  position: absolute;
  inset: 0;
  background: rgba(45, 138, 110, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.2s;
}

.work-card:hover .card-overlay {
  opacity: 1;
}

.overlay-btn {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: #fff;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: transform 0.2s;
}

.overlay-btn:hover {
  transform: scale(1.1);
}

.overlay-btn i {
  font-size: 20px;
  color: #2D8A6E;
}

.card-body {
  padding: 16px;
}

.card-title {
  font-size: 15px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 12px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.card-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
  font-size: 12px;
  color: #888;
}

.card-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: 8px;
  border: 1px solid #e8e8e8;
  border-radius: 6px;
  background: #fff;
  font-size: 13px;
  color: #666;
  cursor: pointer;
  transition: all 0.2s;
}

.action-btn:hover {
  border-color: #2D8A6E;
  color: #2D8A6E;
}

.action-btn.cancel:hover {
  border-color: #ff4d4f;
  color: #ff4d4f;
}

/* 分页 */
.pagination-wrapper {
  display: flex;
  justify-content: center;
  margin-top: 32px;
}

@media (max-width: 768px) {
  .favorites-page {
    padding: 16px;
  }
  
  .filter-bar {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-box {
    min-width: auto;
  }
  
  .works-grid {
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    gap: 16px;
  }
}
</style>
