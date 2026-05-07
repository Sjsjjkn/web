<template>
  <div class="gallery-container">
    <!-- 环境光晕 -->
    <div class="ambient-glow glow-top"></div>
    <div class="ambient-glow glow-bottom"></div>

    <!-- 顶部导航栏 -->
    <header class="gallery-header slide-down">
      <div class="logo-area">
        <div class="logo-icon">BODA</div>
        <h1 class="system-title">作品管理中心</h1>
      </div>
      <div class="header-actions">
        <el-button class="action-btn secondary-btn" round size="small" @click="$router.push('/works')">
          <i class="el-icon-view"></i> 浏览作品
        </el-button>
        <el-button class="action-btn upload-btn" type="primary" round size="small" @click="handleCreateWork">
          <i class="el-icon-plus"></i> 上传作品
        </el-button>
      </div>
    </header>

    <!-- Hero 区域 -->
    <section class="page-hero">
      <div class="hero-pattern"></div>
      <div class="hero-content fade-in-up">
        <div class="hero-badge">
          <span class="badge-dot"></span>
          BODA · 创作空间
        </div>
        <h2 class="hero-title">管理你的<span class="hero-accent">数字作品</span></h2>
        <p class="hero-desc">上传、编辑、发布你的创意作品，让世界看到你的才华</p>
      </div>
    </section>

    <!-- 统计卡片 -->
    <section class="stats-section fade-in-up" style="animation-delay: 0.1s">
      <el-row :gutter="20">
        <el-col :xs="12" :sm="6" v-for="(stat, index) in statsCards" :key="index">
          <div class="stat-card glass-card">
            <div class="stat-icon-wrap" :class="stat.colorClass">
              <i :class="stat.icon"></i>
            </div>
            <div class="stat-value">{{ stat.value }}</div>
            <div class="stat-label">{{ stat.label }}</div>
          </div>
        </el-col>
      </el-row>
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
            v-for="cat in categories"
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
            v-for="status in statuses"
            :key="status.value"
            :class="['filter-chip status-chip', { active: activeStatus === status.value }]"
            @click="activeStatus = status.value; loadWorks()"
          >
            {{ status.label }}
          </span>
        </div>
      </div>
    </section>

    <!-- 作品网格 -->
    <section class="works-grid">
      <el-empty v-if="works.length === 0 && !loading" description="暂无上传作品" class="empty-state">
        <el-button type="primary" @click="handleCreateWork" round>上传第一个作品</el-button>
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
            <!-- 封面 -->
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
              <!-- 角标 -->
              <span class="category-badge">{{ work.category }}</span>
              <span :class="['status-badge', getStatusClass(work.status)]">
                {{ getStatusLabel(work.status) }}
              </span>
              <!-- 悬浮操作层 -->
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
            <!-- 作品信息 -->
            <div class="work-info">
              <h3 class="work-title" :title="work.title">{{ work.title }}</h3>
              <p class="work-author">{{ work.uploadUserName || '未知作者' }}</p>
              <div class="work-meta">
                <span class="meta-item"><i class="el-icon-date"></i> {{ formatDate(work.uploadDate) }}</span>
                <span class="meta-item"><i class="el-icon-view"></i> {{ work.views || 0 }}</span>
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

    <!-- 上传/编辑对话框 -->
    <el-dialog
      :title="isEdit ? '编辑作品' : '上传新作品'"
      :visible.sync="uploadDialogVisible"
      width="680px"
      class="upload-dialog"
      :close-on-click-modal="false"
      @close="resetForm"
    >
      <el-form ref="workForm" :model="form" :rules="rules" label-width="90px" label-position="right">
        <el-form-item label="作品标题" prop="title">
          <el-input v-model="form.title" placeholder="请输入作品标题" maxlength="100" show-word-limit class="form-input"></el-input>
        </el-form-item>

        <el-form-item label="作品分类" prop="category">
          <el-select v-model="form.category" placeholder="请选择分类" class="form-input">
            <el-option v-for="cat in categories.filter(c => c.value)" :key="cat.value" :label="cat.label" :value="cat.value"></el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="发布状态" prop="status">
          <el-radio-group v-model="form.status">
            <el-radio-button label="已发布">发布</el-radio-button>
            <el-radio-button label="草稿">暂存草稿</el-radio-button>
          </el-radio-group>
        </el-form-item>

        <el-form-item label="作品描述">
          <el-input v-model="form.description" type="textarea" :rows="4" placeholder="描述你的作品...（可选）" class="form-input"></el-input>
        </el-form-item>

        <el-form-item label="上传文件">
          <div class="upload-area" v-if="!selectedFile && !existingFilePath">
            <el-upload
              class="file-upload"
              drag
              action="#"
              :auto-upload="false"
              :on-change="handleFileChange"
              :file-list="fileList"
              :limit="1"
              accept="*"
            >
              <div class="upload-placeholder">
                <div class="upload-icon-ring">
                  <i class="el-icon-upload"></i>
                </div>
                <div class="upload-text">
                  <span class="upload-primary">拖拽文件到此处或点击上传</span>
                  <span class="upload-secondary">支持图片、视频、音频、3D模型、文档等</span>
                </div>
              </div>
            </el-upload>
          </div>
          <div class="file-selected-info" v-else>
            <div class="file-card">
              <div class="file-card-icon">
                <i class="el-icon-document"></i>
              </div>
              <div class="file-card-info">
                <span class="file-name">{{ selectedFile ? selectedFile.name : existingFileName }}</span>
                <span class="file-size" v-if="selectedFile">{{ formatFileSize(selectedFile.size) }}</span>
                <span class="file-size" v-else>{{ existingFileSize ? formatFileSize(existingFileSize) : '' }}</span>
              </div>
              <el-button type="text" class="file-remove" @click="removeFile">
                <i class="el-icon-close"></i>
              </el-button>
            </div>
          </div>
          <div class="upload-progress" v-if="uploading">
            <el-progress :percentage="uploadProgress" :color="'var(--primary)'"></el-progress>
            <div class="progress-info">
              <span>{{ uploadProgress < 100 ? '上传中...' : '上传完成' }}</span>
              <span>{{ uploadProgress }}%</span>
            </div>
          </div>
        </el-form-item>
      </el-form>

      <div slot="footer" class="dialog-footer">
        <el-button @click="uploadDialogVisible = false" round class="cancel-btn">取消</el-button>
        <el-button type="primary" @click="submitWork" :loading="uploading" round class="submit-btn">
          {{ isEdit ? '保存修改' : '上传作品' }}
        </el-button>
      </div>
    </el-dialog>

    <!-- 详情对话框 -->
    <el-dialog
      :visible.sync="detailDialogVisible"
      width="800px"
      class="gallery-detail-dialog"
      :close-on-click-modal="true"
    >
      <div slot="title" class="detail-dialog-title">
        <span>作品详情</span>
      </div>
      <div v-if="currentWork" class="detail-content">
        <div class="detail-preview">
          <img v-if="getThumbnailUrl(currentWork)" :src="getThumbnailUrl(currentWork)" :alt="currentWork.title" class="detail-image">
          <div v-else class="detail-placeholder" :style="{ backgroundColor: getCategoryColor(currentWork.category) }">
            <div class="detail-placeholder-icon">
              <i class="el-icon-document"></i>
            </div>
            <span>{{ getFileExtension(currentWork) }}</span>
          </div>
        </div>
        <div class="detail-info">
          <h2 class="detail-title">{{ currentWork.title }}</h2>
          <div class="detail-author">
            <div class="author-avatar">
              <i class="el-icon-user-solid"></i>
            </div>
            <div class="author-meta">
              <span class="author-name">{{ currentWork.uploadUserName || '未知作者' }}</span>
              <span class="upload-time">{{ formatDate(currentWork.uploadDate) }}</span>
            </div>
            <el-tag :type="currentWork.status === '已发布' ? 'success' : 'info'" size="small" effect="plain">
              {{ currentWork.status || '未知' }}
            </el-tag>
          </div>
          <p class="detail-desc" v-if="currentWork.description">{{ currentWork.description }}</p>
          <p class="detail-desc detail-desc-empty" v-else>暂无描述</p>
          <div class="detail-meta-section">
            <div class="detail-meta-row">
              <div class="detail-meta-item">
                <i class="el-icon-view"></i>
                <span class="meta-num">{{ currentWork.views || 0 }}</span>
                <span class="meta-label">浏览</span>
              </div>
              <div class="detail-meta-item">
                <i class="el-icon-star-off"></i>
                <span class="meta-num">{{ currentWork.favorites || 0 }}</span>
                <span class="meta-label">收藏</span>
              </div>
              <div class="detail-meta-item">
                <i class="el-icon-download"></i>
                <span class="meta-num">{{ currentWork.downloads || 0 }}</span>
                <span class="meta-label">下载</span>
              </div>
            </div>
            <div class="detail-file-row">
              <span class="file-tag">
                <i class="el-icon-document"></i> {{ currentWork.fileName || '未知文件' }}
              </span>
              <span class="file-tag" v-if="currentWork.fileSize">
                <i class="el-icon-data-line"></i> {{ formatFileSize(currentWork.fileSize) }}
              </span>
            </div>
          </div>
          <div class="detail-actions">
            <el-button class="detail-download-btn" @click="handleDownloadFile(currentWork)" round>
              <i class="el-icon-download"></i> 下载文件
            </el-button>
          </div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import http from '../../utils/http'
import ModelThumbnail from '../../components/ModelThumbnail.vue'

export default {
  components: { ModelThumbnail },
  name: 'WorkManagement',
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
      uploadDialogVisible: false,
      isEdit: false,
      uploading: false,
      uploadProgress: 0,
      form: { title: '', category: '', description: '', status: '已发布' },
      rules: {
        title: [{ required: true, message: '请输入作品标题', trigger: 'blur' }],
        category: [{ required: true, message: '请选择作品分类', trigger: 'change' }],
        status: [{ required: true, message: '请选择发布状态', trigger: 'change' }]
      },
      selectedFile: null,
      fileList: [],
      existingFilePath: '',
      existingFileName: '',
      existingFileSize: 0,
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
  computed: {
    statsCards() {
      const totalWorks = this.total || 0
      const published = this.works.filter(w => w.status === '已发布').length
      const drafts = this.works.filter(w => w.status && w.status !== '已发布').length
      const totalViews = this.works.reduce((sum, w) => sum + (w.views || 0), 0)
      const totalFavorites = this.works.reduce((sum, w) => sum + (w.favorites || 0), 0)
      return [
        { icon: 'el-icon-folder-opened', value: totalWorks, label: '作品总数', colorClass: 'stat-color-1' },
        { icon: 'el-icon-s-promotion', value: published, label: '已发布', colorClass: 'stat-color-2' },
        { icon: 'el-icon-edit-outline', value: drafts, label: '草稿', colorClass: 'stat-color-3' },
        { icon: 'el-icon-view', value: totalViews, label: '总浏览', colorClass: 'stat-color-4' }
      ]
    }
  },
  mounted() {
    this.loadWorks()
  },
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
      } finally {
        this.loading = false
      }
    },

    handleViewWork(work) {
      this.currentWork = work
      this.detailDialogVisible = true
    },

    handleCreateWork() {
      this.isEdit = false
      this.resetForm()
      this.uploadDialogVisible = true
    },

    handleEditWork(work) {
      this.isEdit = true
      this.form = {
        id: work.id,
        title: work.title || '',
        category: work.category || '',
        description: work.description || '',
        status: work.status || '草稿'
      }
      this.existingFilePath = work.filePath || ''
      this.existingFileName = work.fileName || ''
      this.existingFileSize = work.fileSize || 0
      this.selectedFile = null
      this.fileList = []
      this.uploadDialogVisible = true
    },

    handleFileChange(file, fileList) {
      this.selectedFile = file.raw
      this.fileList = fileList.slice(-1)
      this.existingFilePath = ''
      this.existingFileName = ''
      this.existingFileSize = 0
    },

    removeFile() {
      this.selectedFile = null
      this.fileList = []
      this.existingFilePath = ''
      this.existingFileName = ''
      this.existingFileSize = 0
    },

    async submitWork() {
      try {
        await this.$refs.workForm.validate()
      } catch {
        return
      }

      if (!this.isEdit && !this.selectedFile) {
        this.$message.warning('请选择要上传的文件')
        return
      }

      this.uploading = true
      this.uploadProgress = 0

      const formData = new FormData()
      formData.append('title', this.form.title)
      formData.append('category', this.form.category)
      formData.append('description', this.form.description || '')
      formData.append('status', this.form.status)
      if (this.isEdit && this.form.id) {
        formData.append('id', this.form.id)
      }
      if (this.selectedFile) {
        formData.append('file', this.selectedFile)
      }

      try {
        const url = this.isEdit ? '/api/Work/update' : '/api/Work/upload'
        await http.post(url, formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
          onUploadProgress: (progressEvent) => {
            if (progressEvent.total) {
              this.uploadProgress = Math.round((progressEvent.loaded * 100) / progressEvent.total)
            }
          }
        })
        this.$message.success(this.isEdit ? '作品修改成功' : '作品上传成功')
        this.uploadDialogVisible = false
        this.resetForm()
        this.loadWorks()
      } catch (error) {
        this.$message.error(error.response?.data?.message || '操作失败')
        console.error('操作失败:', error)
      } finally {
        this.uploading = false
        this.uploadProgress = 0
      }
    },

    handleDownloadFile(work) {
      if (!work || !work.filePath) {
        this.$message.error('文件路径不存在，无法下载')
        return
      }
      http.get('/api/File/download', {
        params: { fileName: work.filePath },
        responseType: 'blob'
      })
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

    resetForm() {
      this.form = { title: '', category: '', description: '', status: '已发布' }
      this.selectedFile = null
      this.fileList = []
      this.existingFilePath = ''
      this.existingFileName = ''
      this.existingFileSize = 0
      this.uploadProgress = 0
      if (this.$refs.workForm) {
        this.$refs.workForm.clearValidate()
      }
    },

    handleSizeChange(size) {
      this.pageSize = size
      this.loadWorks()
    },
    handleCurrentChange(current) {
      this.currentPage = current
      this.loadWorks()
    },

    getCategoryColor(category) {
      const colorMap = {
        '前端开发': '#EDF4FF',
        '后端开发': '#E8F5E9',
        '人工智能': '#FFF0F5',
        '设计': '#F3E5F5',
        '其他': '#FFF8E1'
      }
      return colorMap[category] || '#EDF4FF'
    },

    getStatusClass(status) {
      if (status === '已发布') return 'published'
      if (status === '待审核') return 'pending'
      return 'draft'
    },

    getStatusLabel(status) {
      if (status === '已发布') return '已发布'
      if (status === '待审核') return '审核中'
      return status || '草稿'
    },

    formatDate(dateStr) {
      if (!dateStr) return ''
      return new Date(dateStr).toLocaleDateString('zh-CN')
    },

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
      if (['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'].includes(ext)) {
        return `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
      }
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
.gallery-container {
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

/* ===== 顶部导航栏 ===== */
.gallery-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 40px;
  height: 64px;
  background: rgba(255, 255, 255, 0.78);
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  border-bottom: 1px solid var(--border-light);
  position: sticky;
  top: 0;
  z-index: 100;
}

.slide-down {
  animation: slideDown 0.5s var(--ease-out-expo);
}

@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.logo-area {
  display: flex;
  align-items: center;
  gap: 12px;
}

.logo-icon {
  background: linear-gradient(135deg, var(--primary), var(--primary-light));
  color: white;
  font-weight: 700;
  font-size: 14px;
  padding: 6px 10px;
  border-radius: var(--radius-sm);
  box-shadow: 0 2px 8px var(--primary-glow);
}

.system-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 10px;
}

.action-btn {
  font-weight: 600;
  font-size: 13px;
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.action-btn:hover {
  transform: translateY(-2px);
}

.secondary-btn {
  background: var(--surface);
  border: 1px solid var(--border-color);
  color: var(--text-regular);
}

.secondary-btn:hover {
  border-color: var(--primary);
  color: var(--primary);
}

.upload-btn {
  background: linear-gradient(135deg, var(--primary), var(--primary-light));
  border: none;
  color: white;
}

.upload-btn:hover {
  box-shadow: 0 6px 20px var(--primary-glow);
}

/* ===== Hero 区域 ===== */
.page-hero {
  position: relative;
  background: linear-gradient(135deg, var(--primary-bg) 0%, #F7F4F0 40%, #FCFAF5 100%);
  padding: 52px 40px 44px;
  text-align: center;
  overflow: hidden;
  border-bottom: 1px solid var(--border-light);
}

.hero-pattern {
  position: absolute;
  top: -60px;
  right: -60px;
  width: 350px;
  height: 350px;
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
  font-size: 38px;
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
  margin: 0;
  line-height: 1.6;
}

/* ===== 统计卡片 ===== */
.stats-section {
  max-width: 1200px;
  margin: -20px auto 0;
  padding: 0 24px;
  position: relative;
  z-index: 2;
}

.stat-card {
  padding: 22px 18px;
  text-align: center;
  margin-bottom: 20px;
  border-radius: var(--radius-xl);
  transition: all 0.4s var(--ease-out-expo);
  cursor: default;
  background: white;
}

.stat-card:hover {
  transform: translateY(-6px);
  box-shadow: var(--shadow-card-hover);
}

.stat-icon-wrap {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 14px;
  font-size: 22px;
  color: white;
}

.stat-color-1 { background: linear-gradient(135deg, var(--primary), #5DB89E); }
.stat-color-2 { background: linear-gradient(135deg, #43A047, #66BB6A); }
.stat-color-3 { background: linear-gradient(135deg, #FB8C00, #FFA726); }
.stat-color-4 { background: linear-gradient(135deg, #5C6BC0, #7986CB); }

.stat-value {
  font-size: 28px;
  font-weight: 800;
  color: var(--text-main);
  margin-bottom: 4px;
  letter-spacing: -0.5px;
}

.stat-label {
  font-size: 12px;
  color: var(--text-light);
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/* ===== 公用玻璃卡片 ===== */
.glass-card {
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px) saturate(180%);
  -webkit-backdrop-filter: blur(16px) saturate(180%);
  border: 1px solid rgba(232, 226, 216, 0.6);
  box-shadow: var(--shadow-sm);
}

/* ===== 筛选栏 ===== */
.filter-section {
  max-width: 1200px;
  margin: 0 auto 28px;
  padding: 0 24px;
}

.filter-bar {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 20px;
  border-radius: var(--radius-xl);
  flex-wrap: wrap;
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
.status-filters {
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

/* ===== 作品网格 ===== */
.works-grid {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px 60px;
  min-height: 400px;
}

.empty-state {
  padding: 80px 0;
}

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

/* 封面 */
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
  color: rgba(255, 255, 255, 0.85);
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

/* 角标 */
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
  backdrop-filter: blur(8px);
}

.status-badge.published {
  color: #2E7D32;
  background: rgba(200, 230, 201, 0.9);
}

.status-badge.pending {
  color: #E65100;
  background: rgba(255, 224, 178, 0.9);
}

.status-badge.draft {
  color: #616161;
  background: rgba(245, 245, 245, 0.85);
}

/* 悬浮操作层 */
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
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(8px);
}

.view-btn:hover { background: var(--primary); transform: scale(1.1); }
.edit-btn:hover { background: var(--accent-strong); transform: scale(1.1); }
.download-btn:hover { background: #5B9BD5; transform: scale(1.1); }

/* 作品信息 */
.work-info {
  padding: 16px 18px 18px;
}

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

.work-author {
  font-size: 13px;
  color: var(--text-light);
  margin: 0 0 11px;
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

.loading-spinner {
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

@keyframes spin { to { transform: rotate(360deg); } }

/* ===== 分页 ===== */
.pagination-section {
  display: flex;
  justify-content: center;
  margin-top: 0;
  padding-bottom: 40px;
}

/* ===== 上传对话框 ===== */
::v-deep .upload-dialog {
  border-radius: var(--radius-xl) !important;
  overflow: hidden;
}

::v-deep .upload-dialog .el-dialog__header {
  padding: 20px 24px;
  border-bottom: 1px solid var(--border-light);
  background: linear-gradient(135deg, var(--primary-bg), #FCFAF5);
}

::v-deep .upload-dialog .el-dialog__title {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main);
}

::v-deep .upload-dialog .el-dialog__body {
  padding: 24px;
}

.form-input {
  width: 100%;
}

.form-input ::v-deep .el-input__inner,
.form-input ::v-deep .el-textarea__inner {
  border-radius: var(--radius-md);
  border-color: var(--border-color);
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.form-input ::v-deep .el-input__inner:focus,
.form-input ::v-deep .el-textarea__inner:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 3px var(--primary-glow);
}

.upload-area {
  width: 100%;
}

.file-upload ::v-deep .el-upload-dragger {
  border: 2px dashed var(--border-color);
  border-radius: var(--radius-lg);
  background: var(--bg-page);
  transition: all var(--duration-normal) var(--ease-out-expo);
  padding: 40px 20px;
}

.file-upload ::v-deep .el-upload-dragger:hover {
  border-color: var(--primary);
  background: var(--primary-bg);
}

.upload-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14px;
}

.upload-icon-ring {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: rgba(45, 138, 110, 0.08);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  color: var(--primary);
}

.upload-text {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.upload-primary {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
}

.upload-secondary {
  font-size: 12px;
  color: var(--text-light);
}

.file-selected-info {
  width: 100%;
}

.file-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 16px;
  border-radius: var(--radius-md);
  background: var(--primary-bg);
  border: 1px solid rgba(45, 138, 110, 0.15);
}

.file-card-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-sm);
  background: var(--primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  color: white;
  flex-shrink: 0;
}

.file-card-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
  overflow: hidden;
}

.file-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.file-size {
  font-size: 12px;
  color: var(--text-light);
}

.file-remove {
  color: var(--text-light);
  font-size: 18px;
  padding: 4px;
}

.file-remove:hover {
  color: #E53935;
}

.upload-progress {
  margin-top: 16px;
}

.progress-info {
  display: flex;
  justify-content: space-between;
  margin-top: 8px;
  font-size: 13px;
  color: var(--text-light);
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.cancel-btn {
  border-radius: var(--radius-full);
  color: var(--text-regular);
}

.submit-btn {
  border-radius: var(--radius-full);
  background: linear-gradient(135deg, var(--primary), var(--primary-light));
  border: none;
  font-weight: 600;
}

.submit-btn:hover {
  box-shadow: 0 6px 20px var(--primary-glow);
  transform: translateY(-1px);
}

/* ===== 详情对话框 ===== */
::v-deep .gallery-detail-dialog {
  border-radius: var(--radius-xl) !important;
  overflow: hidden;
}

::v-deep .gallery-detail-dialog .el-dialog__header {
  padding: 20px 24px;
  border-bottom: 1px solid var(--border-light);
  background: linear-gradient(135deg, var(--primary-bg), #FCFAF5);
}

.detail-dialog-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main);
}

::v-deep .gallery-detail-dialog .el-dialog__body {
  padding: 0;
}

.detail-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  min-height: 340px;
}

.detail-preview {
  background: linear-gradient(135deg, #F5F2EC, #EDE8DD);
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 340px;
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
  gap: 10px;
  color: rgba(255, 255, 255, 0.85);
  font-size: 16px;
  font-weight: 600;
}

.detail-placeholder-icon {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.25);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 30px;
}

.detail-info {
  padding: 28px 24px;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.detail-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 16px;
  line-height: 1.3;
}

.detail-author {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 18px;
}

.author-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary);
  font-size: 16px;
  flex-shrink: 0;
}

.author-meta {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
}

.author-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main);
}

.upload-time {
  font-size: 12px;
  color: var(--text-light);
}

.detail-desc {
  font-size: 14px;
  color: var(--text-regular);
  line-height: 1.7;
  margin: 0 0 18px;
  flex: 1;
}

.detail-desc-empty {
  color: var(--text-light);
  font-style: italic;
}

.detail-meta-section {
  border-top: 1px solid var(--border-light);
  padding-top: 18px;
}

.detail-meta-row {
  display: flex;
  gap: 24px;
  margin-bottom: 14px;
}

.detail-meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.detail-meta-item i {
  font-size: 15px;
  color: var(--primary);
}

.meta-num {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-main);
}

.meta-label {
  font-size: 12px;
  color: var(--text-light);
}

.detail-file-row {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.file-tag {
  font-size: 12px;
  color: var(--text-secondary);
  background: var(--bg-page);
  padding: 6px 12px;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  gap: 5px;
}

.file-tag i {
  color: var(--primary);
  font-size: 13px;
}

.detail-actions {
  margin-top: 20px;
  display: flex;
  gap: 10px;
}

.detail-download-btn {
  border-radius: var(--radius-full);
  background: linear-gradient(135deg, var(--primary), var(--primary-light));
  border: none;
  color: white;
  font-weight: 600;
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.detail-download-btn:hover {
  box-shadow: 0 6px 20px var(--primary-glow);
  transform: translateY(-1px);
  color: white;
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
  .gallery-header {
    padding: 0 20px;
  }

  .system-title {
    font-size: 16px;
  }

  .page-hero {
    padding: 36px 20px 32px;
  }

  .hero-title {
    font-size: 28px;
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
    min-height: 220px;
  }
}

@media (max-width: 480px) {
  .gallery-header {
    padding: 0 12px;
  }

  .header-actions {
    gap: 6px;
  }

  .action-btn {
    font-size: 12px;
    padding: 7px 12px;
  }

  .page-hero {
    padding: 28px 16px 24px;
  }

  .hero-title {
    font-size: 24px;
  }

  .stats-section,
  .filter-section,
  .works-grid {
    padding: 0 12px;
  }

  .work-card {
    margin-bottom: 16px;
  }
}
</style>
</final_file_content>