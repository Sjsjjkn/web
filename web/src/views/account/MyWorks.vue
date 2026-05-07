<template>
  <div class="my-works-page">
    <!-- 环境光晕 -->
    <div class="ambient-glow glow-top"></div>
    <div class="ambient-glow glow-bottom"></div>

    <div class="my-works-container">
      <!-- Hero 区域 -->
      <section class="my-works-hero">
        <div class="hero-pattern"></div>
        <div class="hero-content fade-in-up">
          <div class="hero-badge">
            <span class="badge-dot"></span>
            BODA · 个人中心
          </div>
          <h1 class="hero-title">我的<span class="hero-accent">作品集</span></h1>
          <p class="hero-desc">管理你的数字创作，展现独特才华</p>
          <div class="hero-stats">
            <div class="hstat">
              <div class="hstat-icon-ring">
                <i class="el-icon-folder-opened"></i>
              </div>
              <span class="hstat-num">{{ total || 0 }}</span>
              <span class="hstat-label">作品总数</span>
            </div>
            <div class="hstat-divider"></div>
            <el-button class="hero-action upload-action" round @click="handleAddWork">
              <i class="el-icon-plus"></i>
              上传作品
            </el-button>
          </div>
        </div>
      </section>

      <!-- 筛选栏 -->
      <section class="filter-section fade-in-up" style="animation-delay: 0.15s">
        <div class="filter-bar glass-card">
          <div class="search-bar">
            <el-input
              v-model="filterForm.search"
              placeholder="搜索作品标题..."
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
              :class="['filter-chip', { active: activeCategory === cat.value }]"
              @click="activeCategory = cat.value; loadWorks()"
            >
              {{ cat.label }}
            </span>
          </div>
          <div class="filter-divider"></div>
          <div class="status-filters">
            <span
              v-for="(status, index) in statuses"
              :key="status.value"
              :class="['filter-chip status-chip', { active: activeStatus === status.value }]"
              @click="activeStatus = status.value; loadWorks()"
            >
              {{ status.label }}
            </span>
          </div>
        </div>
      </section>

      <!-- 作品区域 -->
      <section class="works-section">
        <el-empty v-if="works.length === 0 && !loading" description="暂无上传作品" class="empty-state">
          <el-button type="primary" @click="handleAddWork" round>上传第一个作品</el-button>
        </el-empty>

        <el-row :gutter="24">
          <el-col
            :xs="24" :sm="12" :md="8" :lg="6"
            v-for="(work, index) in works"
            :key="work.id"
            class="stagger-fade-in"
            :style="{ animationDelay: `${index * 0.08 + 0.2}s` }"
          >
            <div class="work-card glass-card" @click="handleViewWork(work)">
              <div class="cover-image">
                <img v-if="getThumbnailUrl(work)" :src="getThumbnailUrl(work)" :alt="work.title" class="thumbnail-image">
                <ModelThumbnail
                  v-else-if="isModelFile(work)"
                  :model-url="`/api/File/download?fileName=${encodeURIComponent(work.filePath)}`"
                  :background-color="getCategoryColor(work.category)"
                  class="model-preview"
                />
                <div v-else class="thumbnail-placeholder" :style="{ backgroundColor: getCategoryColor(work.category) }">
                  <div class="placeholder-circle">
                    <i class="el-icon-document"></i>
                  </div>
                  <span>{{ getFileExtension(work) }}</span>
                </div>
                <span class="category-badge">{{ work.category }}</span>
                <span class="status-badge" :class="{ published: work.status === '已发布', draft: work.status !== '已发布' }">
                  {{ work.status || '草稿' }}
                </span>
                <div class="hover-overlay">
                  <div class="overlay-bg"></div>
                  <div class="overlay-actions">
                    <button class="overlay-btn view-btn" @click.stop="handleViewWork(work)" title="查看详情">
                      <i class="el-icon-view"></i>
                    </button>
                    <button class="overlay-btn edit-btn" @click.stop="handleEditWork(work)" title="编辑作品">
                      <i class="el-icon-edit"></i>
                    </button>
                    <button class="overlay-btn download-btn" @click.stop="handleDownloadFile(work)" title="下载文件">
                      <i class="el-icon-download"></i>
                    </button>
                  </div>
                </div>
              </div>

              <div class="work-info">
                <h3 class="work-title" :title="work.title">{{ work.title }}</h3>
                <p class="work-author">{{ work.uploadUserName || '未知作者' }}</p>
                <div class="work-meta">
                  <span class="meta-item"><i class="el-icon-date"></i> {{ formatDate(work.uploadDate) }}</span>
                  <span class="meta-item"><i class="el-icon-folder-opened"></i> {{ work.category }}</span>
                </div>
                <div class="work-actions">
                  <button class="card-action-btn detail-action" @click.stop="handleViewWork(work)">
                    <span>详情</span>
                    <i class="el-icon-arrow-right"></i>
                  </button>
                </div>
              </div>
            </div>
          </el-col>
        </el-row>

        <div v-if="loading" class="loading-container">
          <div class="loading-spinner">
            <div class="spinner-ring"></div>
          </div>
          <p>加载中...</p>
        </div>
      </section>

      <!-- 分页 -->
      <div class="pagination-section">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
          layout="total, sizes, prev, pager, next, jumper"
          background
        />
      </div>

      <!-- 详情对话框 -->
      <el-dialog
        title="作品详情"
        :visible.sync="detailDialogVisible"
        width="800px"
        class="gallery-detail-dialog"
        :close-on-click-modal="true"
        append-to-body
      >
        <div v-if="currentWork" class="detail-content">
          <div class="detail-preview">
            <img v-if="getThumbnailUrl(currentWork)" :src="getThumbnailUrl(currentWork)" :alt="currentWork.title" class="detail-image">
            <div v-else class="detail-placeholder" :style="{ backgroundColor: getCategoryColor(currentWork.category) }">
              <i class="el-icon-document"></i>
              <span>{{ getFileExtension(currentWork) }}</span>
            </div>
          </div>
          <div class="detail-info">
            <h2 class="detail-title">{{ currentWork.title }}</h2>
            <div class="detail-author">
              <i class="el-icon-user-solid"></i>
              {{ currentWork.uploadUserName || '未知作者' }}
              <el-tag :type="currentWork.status === '已发布' ? 'success' : 'info'" size="small" style="margin-left: 8px;">
                {{ currentWork.status || '未知' }}
              </el-tag>
            </div>
            <p class="detail-desc" v-if="currentWork.description">{{ currentWork.description }}</p>
            <div class="detail-meta-row">
              <span class="detail-meta"><i class="el-icon-date"></i> {{ formatDate(currentWork.uploadDate) }}</span>
              <span class="detail-meta"><i class="el-icon-view"></i> {{ currentWork.views || 0 }} 浏览</span>
              <span class="detail-meta"><i class="el-icon-star-off"></i> {{ currentWork.favorites || 0 }} 收藏</span>
            </div>
            <div class="detail-file-info">
              <span class="detail-meta"><i class="el-icon-document"></i> {{ currentWork.fileName || '未知文件名' }}</span>
              <span v-if="currentWork.fileSize" class="detail-meta"><i class="el-icon-data-line"></i> {{ formatFileSize(currentWork.fileSize) }}</span>
            </div>
            <div class="detail-actions">
              <el-button type="primary" round @click="handleDownloadFile(currentWork)">
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
import http from '../../utils/http'
import ModelThumbnail from '../../components/ModelThumbnail.vue'

export default {
  components: { ModelThumbnail },
  name: 'MyWorks',
  data() {
    return {
      works: [],
      loading: false,
      currentPage: 1,
      pageSize: 12,
      total: 0,
      filterForm: { search: '', category: '', status: '' },
      activeCategory: '',
      activeStatus: '',
      currentWork: null,
      detailDialogVisible: false,
      categories: [
        { label: '全部', value: '' },
        { label: '前端开发', value: '前端开发' },
        { label: '后端开发', value: '后端开发' },
        { label: '人工智能', value: '人工智能' },
        { label: '设计', value: '设计' },
        { label: '其他', value: '其他' }
      ],
      statuses: [
        { label: '全部', value: '' },
        { label: '草稿', value: '草稿' },
        { label: '已发布', value: '已发布' }
      ]
    }
  },
  beforeRouteLeave(to, from, next) {
    this.detailDialogVisible = false
    next()
  },
  mounted() { this.loadWorks() },
  methods: {
    async loadWorks() {
      this.loading = true
      try {
        const response = await http.get('/api/Work/my', {
          params: {
            search: this.filterForm.search,
            category: this.activeCategory,
            status: this.activeStatus,
            page: this.currentPage,
            pageSize: this.pageSize
          }
        })
        this.works = response.data.items || []
        this.total = response.data.total || 0
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载作品失败')
        console.error('加载作品失败:', error)
      } finally { this.loading = false }
    },
    handleViewWork(work) {
      this.currentWork = work
      this.detailDialogVisible = true
    },
    handleAddWork() { this.$router.push('/works/manage') },
    handleEditWork(work) { this.$router.push(`/works/manage?edit=${work.id}`) },
    handleDownloadFile(work) {
      if (!work || !work.filePath) { this.$message.error('文件路径不存在，无法下载'); return }
      http.get('/api/File/download', { params: { fileName: work.filePath }, responseType: 'blob' })
        .then(res => {
          const blob = res.data
          const url = window.URL.createObjectURL(blob)
          const link = document.createElement('a')
          link.href = url
          link.download = work.fileName || work.filePath
          document.body.appendChild(link)
          link.click()
          document.body.removeChild(link)
          window.URL.revokeObjectURL(url)
          this.$message.success('文件下载已开始')
        })
        .catch(err => {
          this.$message.error(err.response?.data?.message || '文件下载失败')
          console.error('下载文件失败:', err)
        })
    },
    handleSizeChange(size) { this.pageSize = size; this.loadWorks() },
    handleCurrentChange(current) { this.currentPage = current; this.loadWorks() },
    getCategoryColor(category) {
      const colorMap = { '前端开发': '#e6f7ff', '后端开发': '#f6ffed', '人工智能': '#fff0f6', '设计': '#f0f5ff', '其他': '#fff7e6' }
      return colorMap[category] || '#e6f7ff'
    },
    formatDate(dateStr) { return new Date(dateStr).toLocaleDateString('zh-CN') },
    formatFileSize(bytes) {
      if (!bytes) return '0 B'
      const k = 1024
      const sizes = ['B', 'KB', 'MB', 'GB']
      const i = Math.floor(Math.log(bytes) / Math.log(k))
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
    },
    getThumbnailUrl(work) {
      if (work.previewImage) return `/api/File/download?fileName=${encodeURIComponent(work.previewImage)}`
      if (!work.filePath) return null
      const ext = work.filePath.toLowerCase().substring(work.filePath.lastIndexOf('.'))
      if (['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'].includes(ext)) return `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
      return null
    },
    isModelFile(work) {
      if (!work.filePath) return false
      const ext = work.filePath.toLowerCase().substring(work.filePath.lastIndexOf('.'))
      return ['.json', '.obj', '.gltf', '.glb', '.fbx', '.dae'].includes(ext)
    },
    getFileExtension(work) {
      if (!work.filePath) return '文件'
      return work.filePath.toLowerCase().substring(work.filePath.lastIndexOf('.') + 1).toUpperCase()
    }
  }
}
</script>

<style scoped>
/* ===== 全局容器 ===== */
.my-works-page {
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

.my-works-container {
  position: relative;
  z-index: 1;
  max-width: 1200px;
  margin: 0 auto;
  padding: 28px 24px 80px;
}

/* ===== Hero 区域 ===== */
.my-works-hero {
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

.hero-content { position: relative; z-index: 1; }

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

.hero-accent { color: var(--primary); }

.hero-desc {
  font-size: 15px;
  color: var(--text-secondary);
  margin: 0 0 28px;
  line-height: 1.6;
}

.hero-stats {
  display: flex;
  align-items: center;
  gap: 24px;
  padding-top: 20px;
  border-top: 1px solid rgba(0, 0, 0, 0.05);
}

.hstat { display: flex; align-items: center; gap: 12px; }

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

.hstat-num { font-size: 18px; font-weight: 700; color: var(--text-main); }
.hstat-label { font-size: 13px; color: var(--text-light); }

.hstat-divider {
  width: 1px;
  height: 28px;
  background: rgba(0, 0, 0, 0.06);
  flex-shrink: 0;
}

.upload-action {
  background: linear-gradient(135deg, var(--primary), var(--primary-light));
  color: white;
  border: none;
  font-weight: 600;
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.upload-action:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px var(--primary-glow);
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

.search-bar { width: 240px; flex-shrink: 0; }

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

.category-filters, .status-filters { display: flex; gap: 8px; flex-wrap: wrap; align-items: center; }

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

.status-chip.active {
  background: var(--primary-bg);
  color: var(--primary);
  border-color: var(--primary);
  box-shadow: none;
  font-weight: 600;
}

/* ===== 作品区域 ===== */
.works-section { min-height: 400px; }
.empty-state { padding: 80px 0; }

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

.work-card:hover .thumbnail-image { transform: scale(1.06); }

.model-preview { width: 100%; height: 100%; }

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

.work-card:hover .thumbnail-placeholder { transform: scale(1.06); }

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

.status-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  font-size: 11px;
  font-weight: 600;
  padding: 4px 10px;
  border-radius: var(--radius-full);
  z-index: 2;
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(8px);
  color: var(--text-regular);
}

.status-badge.published {
  color: #3A8C3D;
  background: rgba(234, 247, 234, 0.9);
}

.status-badge.draft {
  color: var(--text-light);
  background: rgba(255, 255, 255, 0.7);
}

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

.work-card:hover .hover-overlay { opacity: 1; }

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

.work-card:hover .overlay-actions { transform: translateY(0); }

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
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(8px);
}

.view-btn:hover { background: var(--primary); transform: scale(1.1); }
.edit-btn:hover { background: var(--accent-strong); transform: scale(1.1); }
.download-btn:hover { background: #5B9BD5; transform: scale(1.1); }

.work-info { padding: 16px 18px 18px; }

.work-title {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.4;
}

.work-author { font-size: 13px; color: var(--text-light); margin: 0 0 11px; }

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

.meta-item i { font-size: 13px; }

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

/* ===== 加载中 ===== */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 60px 0;
  gap: 16px;
  color: var(--text-light);
  font-size: 14px;
}

.loading-spinner { width: 44px; height: 44px; }

.spinner-ring {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  border: 3px solid var(--border-light);
  border-top-color: var(--primary);
  animation: spin 0.8s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }

/* ===== 分页 ===== */
.pagination-section { display: flex; justify-content: center; margin-top: 20px; }

/* ===== 详情对话框 ===== */
::v-deep .gallery-detail-dialog {
  border-radius: var(--radius-xl) !important;
  overflow: hidden;
}

::v-deep .gallery-detail-dialog .el-dialog__header {
  padding: 20px 24px;
  border-bottom: 1px solid var(--border-light);
}

::v-deep .gallery-detail-dialog .el-dialog__body { padding: 0; }

.detail-content { display: grid; grid-template-columns: 1fr 1fr; min-height: 300px; }

.detail-preview {
  background: linear-gradient(135deg, #F5F2EC, #EDE8DD);
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
}

.detail-image { width: 100%; height: 100%; object-fit: cover; }

.detail-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  color: rgba(255, 255, 255, 0.9);
  font-size: 16px;
  font-weight: 600;
}

.detail-placeholder i { font-size: 40px; }

.detail-info { padding: 28px 24px; display: flex; flex-direction: column; }

.detail-title { font-size: 20px; font-weight: 700; color: var(--text-main); margin: 0 0 10px; }

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

.detail-file-info {
  display: flex;
  gap: 16px;
  padding-top: 8px;
}

.detail-actions { display: flex; gap: 10px; margin-top: 14px; }

/* ===== 动画 ===== */
.fade-in-up { animation: fadeInUp 0.6s var(--ease-out-expo) both; }

.stagger-fade-in { animation: fadeInUp 0.6s var(--ease-out-expo) both; }

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(24px); }
  to { opacity: 1; transform: translateY(0); }
}

/* ===== 响应式 ===== */
@media (max-width: 900px) {
  .my-works-hero { padding: 32px 24px 28px; }
  .hero-title { font-size: 28px; }
  .hero-stats { flex-wrap: wrap; gap: 14px; }
  .upload-action { margin-left: 0; }
  .hstat-divider { display: none; }
  .filter-bar { flex-direction: column; align-items: stretch; }
  .search-bar { width: 100%; }
  .filter-divider { display: none; }
  .detail-content { grid-template-columns: 1fr; }
  .detail-preview { min-height: 200px; }
}

@media (max-width: 480px) {
  .my-works-container { padding: 16px 12px 60px; }
  .my-works-hero { padding: 24px 16px 20px; border-radius: var(--radius-lg); }
  .hero-title { font-size: 24px; }
  .work-card { margin-bottom: 16px; }
}
</style>