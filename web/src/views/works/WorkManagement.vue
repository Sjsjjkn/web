<template>
  <div class="work-management-page">
    <div class="management-container">
      <!-- Hero 区域 -->
      <section class="gallery-hero">
        <div class="hero-content">
          <h1 class="hero-title">作品管理中心</h1>
          <p class="hero-desc">上传、编辑、发布你的创意作品，让世界看到你的才华</p>
        </div>
        <div class="hero-actions">
          <el-button type="primary" icon="el-icon-plus" round @click="handleCreateWork">上传作品</el-button>
          <el-button icon="el-icon-view" round @click="$router.push('/works')">浏览作品</el-button>
        </div>
      </section>

      <!-- 统计卡片 -->
      <section class="stats-section">
        <div class="stats-grid">
          <div class="stat-card" v-for="(stat, index) in statsCards" :key="index">
            <div class="stat-icon-wrap" :class="stat.colorClass">
              <i :class="stat.icon"></i>
            </div>
            <div class="stat-value">{{ stat.value }}</div>
            <div class="stat-label">{{ stat.label }}</div>
          </div>
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
          <el-button type="primary" @click="handleCreateWork" round>上传第一个作品</el-button>
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
              <div v-else class="cover-placeholder" :style="{ background: getCategoryColor(work.category) }">
                <span class="file-ext">{{ getFileExtension(work) }}</span>
              </div>
              <span class="category-tag">{{ work.category || '未分类' }}</span>
              <span :class="['status-tag', work.status]">{{ getStatusLabel(work.status) }}</span>
              <div class="card-overlay">
                <button class="overlay-btn view-btn" @click.stop="handleViewWork(work)">
                  <i class="el-icon-view"></i>
                </button>
                <button class="overlay-btn edit-btn" @click.stop="handleEditWork(work)">
                  <i class="el-icon-edit"></i>
                </button>
                <button class="overlay-btn download-btn" @click.stop="handleDownloadFile(work)">
                  <i class="el-icon-download"></i>
                </button>
              </div>
            </div>
            <div class="card-body">
              <h3 class="card-title">{{ work.title }}</h3>
              <div class="card-meta">
                <span class="author">{{ work.uploadUserName || '未知作者' }}</span>
                <span class="views">👁 {{ work.views || 0 }}</span>
              </div>
              <div class="card-actions">
                <button class="action-btn" @click.stop="handleEditWork(work)">
                  <i class="el-icon-edit"></i>
                  <span>编辑</span>
                </button>
                <button class="action-btn primary" @click.stop="handleViewWork(work)">
                  <i class="el-icon-view"></i>
                  <span>详情</span>
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

    <!-- 上传/编辑对话框 -->
    <el-dialog
      :title="isEdit ? '编辑作品' : '上传新作品'"
      :visible.sync="uploadDialogVisible"
      width="680px"
      :close-on-click-modal="false"
      @close="resetForm"
      append-to-body
      :modal-append-to-body="false"
    >
      <el-form ref="workForm" :model="form" :rules="rules" label-width="90px" label-position="right">
        <el-form-item label="作品标题" prop="title">
          <el-input v-model="form.title" placeholder="请输入作品标题" maxlength="100" show-word-limit></el-input>
        </el-form-item>

        <el-form-item label="作品分类" prop="category">
          <el-select v-model="form.category" placeholder="请选择分类">
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
          <el-input v-model="form.description" type="textarea" :rows="4" placeholder="描述你的作品...（可选）"></el-input>
        </el-form-item>

        <el-form-item label="上传文件">
          <div v-if="!selectedFile && !existingFilePath" class="upload-area">
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
                <div class="upload-icon">
                  <i class="el-icon-upload"></i>
                </div>
                <div class="upload-text">
                  <span>拖拽文件到此处或点击上传</span>
                  <span>支持图片、视频、音频、3D模型、文档等</span>
                </div>
              </div>
            </el-upload>
          </div>
          <div v-else class="file-selected-info">
            <div class="file-card">
              <div class="file-card-icon">
                <i class="el-icon-document"></i>
              </div>
              <div class="file-card-info">
                <span class="file-name">{{ selectedFile ? selectedFile.name : existingFileName }}</span>
                <span class="file-size">{{ selectedFile ? formatFileSize(selectedFile.size) : (existingFileSize ? formatFileSize(existingFileSize) : '') }}</span>
              </div>
              <el-button type="text" @click="removeFile">
                <i class="el-icon-close"></i>
              </el-button>
            </div>
          </div>
          <div v-if="uploading" class="upload-progress">
            <el-progress :percentage="uploadProgress"></el-progress>
            <div class="progress-info">
              <span>{{ uploadProgress < 100 ? '上传中...' : '上传完成' }}</span>
              <span>{{ uploadProgress }}%</span>
            </div>
          </div>
        </el-form-item>
      </el-form>

      <div slot="footer">
        <el-button @click="uploadDialogVisible = false" round>取消</el-button>
        <el-button type="primary" @click="submitWork" :loading="uploading" round>
          {{ isEdit ? '保存修改' : '上传作品' }}
        </el-button>
      </div>
    </el-dialog>

    <!-- 详情对话框 -->
    <el-dialog
      :visible.sync="detailDialogVisible"
      width="800px"
    >
      <div slot="title">作品详情</div>
      <div v-if="currentWork" class="detail-content">
        <div class="detail-preview">
          <img v-if="getThumbnailUrl(currentWork)" :src="getThumbnailUrl(currentWork)" :alt="currentWork.title" class="detail-image">
          <div v-else class="detail-placeholder" :style="{ backgroundColor: getCategoryColor(currentWork.category) }">
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
            <el-tag :type="currentWork.status === '已发布' ? 'success' : 'info'" size="small">{{ currentWork.status || '未知' }}</el-tag>
          </div>
          <p class="detail-desc" v-if="currentWork.description">{{ currentWork.description }}</p>
          <p class="detail-desc" v-else>暂无描述</p>
          <div class="detail-meta-section">
            <div class="detail-meta-row">
              <div class="detail-meta-item">
                <i class="el-icon-view"></i>
                <span>{{ currentWork.views || 0 }}</span>
                <span>浏览</span>
              </div>
              <div class="detail-meta-item">
                <i class="el-icon-star-off"></i>
                <span>{{ currentWork.favorites || 0 }}</span>
                <span>收藏</span>
              </div>
              <div class="detail-meta-item">
                <i class="el-icon-download"></i>
                <span>{{ currentWork.downloads || 0 }}</span>
                <span>下载</span>
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
            <el-button @click="handleDownloadFile(currentWork)" round>
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

export default {
  name: 'WorkManagement',
  data() {
    return {
      works: [],
      loading: false,
      currentPage: 1,
      pageSize: 12,
      total: 0,
      filterForm: { search: '' },
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
        '前端开发': '#5B9BD5',
        '后端开发': '#43A047',
        '人工智能': '#F09342',
        '设计': '#C8AA6E',
        '其他': '#90A4AE'
      }
      return colorMap[category] || '#5B9BD5'
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

    getFileExtension(work) {
      if (!work.filePath) return 'FILE'
      return work.filePath.toLowerCase().substring(work.filePath.lastIndexOf('.') + 1).toUpperCase()
    }
  }
}
</script>

<style scoped>
.work-management-page {
  min-height: 100vh;
  background: #f5f7fa;
  padding: 24px;
}

.management-container {
  max-width: 1400px;
  margin: 0 auto;
}

/* Hero 区域 */
.gallery-hero {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 32px 24px;
  background: linear-gradient(135deg, #2D8A6E 0%, #45A884 100%);
  border-radius: 16px;
  margin-bottom: 24px;
}

.hero-content {
  color: #fff;
}

.hero-title {
  font-size: 28px;
  font-weight: 700;
  margin: 0 0 8px;
}

.hero-desc {
  font-size: 14px;
  opacity: 0.9;
  margin: 0;
}

.hero-actions {
  display: flex;
  gap: 12px;
}

.hero-actions .el-button {
  padding: 8px 20px;
  font-size: 14px;
}

/* 统计卡片 */
.stats-section {
  margin-bottom: 24px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-card {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  text-align: center;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

.stat-icon-wrap {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 12px;
  font-size: 20px;
  color: #fff;
}

.stat-color-1 { background: linear-gradient(135deg, #2D8A6E, #5DB89E); }
.stat-color-2 { background: linear-gradient(135deg, #43A047, #66BB6A); }
.stat-color-3 { background: linear-gradient(135deg, #FB8C00, #FFA726); }
.stat-color-4 { background: linear-gradient(135deg, #5C6BC0, #7986CB); }

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 12px;
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

.status-tag {
  position: absolute;
  top: 12px;
  right: 12px;
  padding: 4px 10px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 4px;
}

.status-tag.已发布 {
  background: rgba(76, 175, 80, 0.9);
  color: #fff;
}

.status-tag.草稿, .status-tag {
  background: rgba(100, 100, 100, 0.9);
  color: #fff;
}

.card-overlay {
  position: absolute;
  inset: 0;
  background: rgba(45, 138, 110, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  opacity: 0;
  transition: opacity 0.2s;
}

.work-card:hover .card-overlay {
  opacity: 1;
}

.overlay-btn {
  width: 44px;
  height: 44px;
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
  font-size: 18px;
  color: #2D8A6E;
}

.card-body {
  padding: 16px;
}

.card-title {
  font-size: 15px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 10px;
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

.action-btn.primary {
  background: #2D8A6E;
  border-color: #2D8A6E;
  color: #fff;
}

.action-btn.primary:hover {
  background: #1D7A5E;
  border-color: #1D7A5E;
}

/* 分页 */
.pagination-wrapper {
  display: flex;
  justify-content: center;
  margin-top: 32px;
}

/* 上传区域 */
.upload-area {
  margin-top: 8px;
}

.file-upload .el-upload-dragger {
  border: 2px dashed #ddd;
  border-radius: 10px;
  padding: 40px;
  text-align: center;
  transition: border-color 0.2s;
}

.file-upload .el-upload-dragger:hover {
  border-color: #2D8A6E;
}

.upload-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.upload-icon {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: #f0f0f0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  color: #2D8A6E;
}

.upload-text {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.upload-text span:first-child {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.upload-text span:last-child {
  font-size: 12px;
  color: #999;
}

.file-selected-info {
  margin-top: 8px;
}

.file-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: #f5f7fa;
  border-radius: 8px;
}

.file-card-icon {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  background: #2D8A6E;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  color: #fff;
}

.file-card-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.file-name {
  font-size: 13px;
  font-weight: 500;
  color: #333;
}

.file-size {
  font-size: 12px;
  color: #999;
}

.upload-progress {
  margin-top: 12px;
}

.progress-info {
  display: flex;
  justify-content: space-between;
  margin-top: 8px;
  font-size: 12px;
  color: #999;
}

/* 详情对话框 */
.detail-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  min-height: 300px;
}

.detail-preview {
  background: #f5f7fa;
  display: flex;
  align-items: center;
  justify-content: center;
}

.detail-image {
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
  color: rgba(255,255,255,0.9);
  font-size: 18px;
  font-weight: 600;
}

.detail-info {
  padding: 20px;
}

.detail-title {
  font-size: 20px;
  font-weight: 700;
  color: #333;
  margin: 0 0 16px;
}

.detail-author {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.author-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: #e8e8e8;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  color: #666;
}

.author-meta {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.author-name {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.upload-time {
  font-size: 12px;
  color: #999;
}

.detail-desc {
  font-size: 14px;
  color: #666;
  line-height: 1.6;
  margin-bottom: 16px;
}

.detail-meta-section {
  margin-bottom: 16px;
}

.detail-meta-row {
  display: flex;
  gap: 24px;
  margin-bottom: 12px;
}

.detail-meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #666;
}

.detail-meta-item span:nth-child(2) {
  font-weight: 600;
  color: #333;
}

.detail-file-row {
  display: flex;
  gap: 12px;
}

.file-tag {
  padding: 4px 10px;
  background: #f0f0f0;
  border-radius: 4px;
  font-size: 12px;
  color: #666;
}

.detail-actions {
  display: flex;
  justify-content: flex-end;
}

@media (max-width: 768px) {
  .work-management-page {
    padding: 16px;
  }
  
  .gallery-hero {
    flex-direction: column;
    gap: 16px;
    text-align: center;
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
    grid-template-columns: 1fr;
  }
}
</style>
