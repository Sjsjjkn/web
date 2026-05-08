<template>
  <div class="my-works-page">
    <div class="my-works-container">
      <!-- Hero 区域 -->
      <section class="gallery-hero">
        <h1 class="hero-title">我的作品集</h1>
        <p class="hero-desc">管理你的数字创作，展现独特才华</p>
        <div class="hero-stats">
          <div class="stat-item">
            <span class="stat-num">{{ total || 0 }}</span>
            <span class="stat-label">作品总数</span>
          </div>
          <el-button type="primary" icon="el-icon-plus" round @click="handleAddWork">上传作品</el-button>
        </div>
      </section>

      <!-- 筛选栏 -->
      <section class="filter-section">
        <div class="filter-bar">
          <div class="search-box">
            <el-input
              v-model="filterForm.search"
              placeholder="搜索作品标题..."
              clearable
              @keyup.enter="loadWorks"
            ></el-input>
            <el-button type="primary" icon="el-icon-search" @click="loadWorks">搜索</el-button>
          </div>
          <div class="filter-group">
            <span
              v-for="cat in categories"
              :key="cat.value"
              :class="['filter-btn', { active: activeCategory === cat.value }]"
              @click="activeCategory = cat.value; loadWorks()"
            >
              {{ cat.label }}
            </span>
          </div>
          <div class="filter-group">
            <span
              v-for="status in statuses"
              :key="status.value"
              :class="['filter-btn', { active: activeStatus === status.value }]"
              @click="activeStatus = status.value; loadWorks()"
            >
              {{ status.label }}
            </span>
          </div>
        </div>
      </section>

      <!-- 作品网格 -->
      <section class="works-section">
        <el-empty v-if="works.length === 0 && !loading" description="暂无上传作品">
          <el-button type="primary" @click="handleAddWork" round>上传第一个作品</el-button>
        </el-empty>

        <div v-else class="works-grid">
          <div
            v-for="work in works"
            :key="work.id"
            class="work-card"
            @click="handleViewWork(work)"
          >
            <div class="card-cover">
              <img v-if="getThumbnailUrl(work)" :src="getThumbnailUrl(work)" :alt="work.title" class="cover-img">
              <div v-else class="cover-placeholder" :style="{ background: getRandomColor(work.id) }">
                <span class="file-ext">{{ getFileExtension(work) }}</span>
              </div>
              <span class="category-tag">{{ work.category || '未分类' }}</span>
              <div class="card-overlay">
                <button class="overlay-btn" @click.stop="handleViewWork(work)">
                  <i class="el-icon-view"></i>
                </button>
              </div>
            </div>
            <div class="card-body">
              <h3 class="card-title">{{ work.title }}</h3>
              <div class="card-meta">
                <span class="status" :class="work.status">{{ getStatusText(work.status) }}</span>
                <span class="date">{{ formatDate(work.createdAt) }}</span>
              </div>
              <div class="card-actions">
                <button class="action-btn" @click.stop="handleEditWork(work)" title="编辑">
                  <i class="el-icon-edit"></i>
                  <span>编辑</span>
                </button>
                <button class="action-btn delete" @click.stop="handleDeleteWork(work)" title="删除">
                  <i class="el-icon-delete"></i>
                  <span>删除</span>
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

      <!-- 作品详情对话框 -->
      <el-dialog
        :visible.sync="detailDialogVisible"
        width="700px"
        :close-on-click-modal="true"
        append-to-body
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
              <span>分类：{{ selectedWork.category || '未分类' }}</span>
              <span>状态：{{ getStatusText(selectedWork.status) }}</span>
              <span>浏览：{{ selectedWork.views || 0 }}</span>
            </div>
            <div class="detail-actions">
              <el-button type="primary" round @click="handleEditWork(selectedWork); detailDialogVisible = false">
                编辑作品
              </el-button>
              <el-button round @click="handleDownloadFile(selectedWork)">下载文件</el-button>
            </div>
          </div>
        </div>
      </el-dialog>
    </div>
  </div>
</template>

<script>
export default {
  name: 'MyWorks',
  data() {
    return {
      works: [],
      total: 0,
      currentPage: 1,
      pageSize: 12,
      loading: false,
      filterForm: { search: '' },
      activeCategory: '',
      activeStatus: '',
      categories: [{ value: '', label: '全部' }],
      statuses: [
        { value: '', label: '全部状态' },
        { value: 'published', label: '已发布' },
        { value: 'draft', label: '草稿' }
      ],
      detailDialogVisible: false,
      selectedWork: null
    }
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
          search: this.filterForm.search || undefined,
          category: this.activeCategory || undefined,
          status: this.activeStatus || undefined
        }
        const { data } = await http.get('/api/Work/my', { params })
        this.works = data.items || data.data || data || []
        this.total = data.total || data.totalCount || this.works.length
        if (data.categories) {
          this.categories = [{ value: '', label: '全部' }, ...data.categories.map(c => ({ value: c, label: c }))]
        }
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
    getStatusText(status) {
      const map = { published: '已发布', draft: '草稿', pending: '审核中' }
      return map[status] || status || '未知'
    },
    formatDate(date) {
      if (!date) return '-'
      return new Date(date).toLocaleDateString('zh-CN')
    },
    handleViewWork(work) {
      this.selectedWork = work
      this.detailDialogVisible = true
    },
    handleEditWork(work) {
      this.$router.push(`/works/edit/${work.id}`)
    },
    async handleDeleteWork(work) {
      try {
        await this.$confirm('确定要删除这个作品吗？', '提示', { type: 'warning' })
        const http = (await import('../../utils/http')).default
        await http.delete(`/api/Work/${work.id}`)
        this.$message.success('删除成功')
        this.loadWorks()
      } catch (e) {
        if (e !== 'cancel') {
          this.$message.error('删除失败')
        }
      }
    },
    handleAddWork() {
      this.$router.push('/works/upload')
    },
    handleDownloadFile(work) {
      if (!work.filePath) {
        this.$message.error('无文件可下载')
        return
      }
      const url = `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
      const link = document.createElement('a')
      link.href = url
      link.download = work.fileName || 'download'
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
    },
    handleSizeChange(size) {
      this.pageSize = size
      this.loadWorks()
    },
    handleCurrentChange(page) {
      this.currentPage = page
      this.loadWorks()
    }
  }
}
</script>

<style scoped>
.my-works-page {
  min-height: 100vh;
  background: #f5f7fa;
  padding: 24px;
}

.my-works-container {
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

.status {
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 11px;
}

.status.published {
  background: #e6f7e6;
  color: #52c41a;
}

.status.draft {
  background: #fff7e6;
  color: #faad14;
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

.action-btn.delete:hover {
  border-color: #ff4d4f;
  color: #ff4d4f;
}

/* 分页 */
.pagination-wrapper {
  display: flex;
  justify-content: center;
  margin-top: 32px;
}

/* 详情弹窗 */
.detail-content {
  display: flex;
  gap: 24px;
}

.detail-cover {
  flex: 0 0 280px;
  height: 200px;
  border-radius: 8px;
  overflow: hidden;
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
}

.detail-placeholder span {
  font-size: 32px;
  font-weight: 600;
  color: rgba(255,255,255,0.9);
}

.detail-info {
  flex: 1;
}

.detail-info h2 {
  font-size: 20px;
  font-weight: 600;
  margin: 0 0 12px;
}

.detail-desc {
  font-size: 14px;
  color: #666;
  line-height: 1.6;
  margin-bottom: 16px;
}

.detail-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 20px;
  font-size: 13px;
  color: #888;
}

.detail-actions {
  display: flex;
  gap: 12px;
}

@media (max-width: 768px) {
  .my-works-page {
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
  
  .detail-content {
    flex-direction: column;
  }
  
  .detail-cover {
    flex: none;
    width: 100%;
  }
}
</style>
