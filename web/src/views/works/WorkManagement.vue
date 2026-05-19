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
              <ModelCardCover :work="work" :gradient="getGradient(work.id, 0)">
                <template #badge>
                  <span class="category-tag">{{ work.category || '未分类' }}</span>
                  <span :class="['status-tag', work.status]">{{ getStatusLabel(work.status) }}</span>
                </template>
              </ModelCardCover>
            </div>
            <div class="card-body">
              <div class="card-title">{{ work.title }}</div>
              <div class="card-author">
                <span class="author-avatar">{{ (work.uploadUserName || '未')[0] }}</span>
                {{ work.uploadUserName || '未知作者' }}
                <span class="card-date" v-if="work.fileUploadTime || work.uploadDate">
                  · {{ formatDate(work.fileUploadTime || work.uploadDate) }}
                </span>
              </div>
              <div class="card-stats">
                <span class="stat-item">👁 {{ work.views || 0 }}</span>
                <span class="stat-item">⭐ {{ work.favorites || 0 }}</span>
              </div>
              <div class="card-actions">
                <button class="action-btn" @click.stop="handleOpenReview(work)">
                  <i class="el-icon-s-management"></i>
                  <span>管理</span>
                </button>
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
      custom-class="work-form-dialog"
      append-to-body
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
            <el-radio-button label="待审核">发布</el-radio-button>
            <el-radio-button label="草稿">暂存草稿</el-radio-button>
            <el-radio-button v-if="isEdit && form.originalStatus === '已发布'" label="已发布">保持发布</el-radio-button>
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

        <el-form-item label="预览图">
          <div class="preview-image-upload">
            <div v-if="previewImageFile || existingPreviewImage" class="preview-selected">
              <img v-if="previewImageUrl" :src="previewImageUrl" class="preview-thumb" />
              <img v-else-if="existingPreviewImage" :src="'/api/File/download?fileName=' + encodeURIComponent(existingPreviewImage)" class="preview-thumb" />
              <div class="preview-info">
                <span>{{ previewImageFile ? previewImageFile.name : existingPreviewImage }}</span>
                <el-button type="text" size="small" @click="removePreviewImage">移除</el-button>
              </div>
            </div>
            <el-upload
              v-else
              class="preview-upload"
              action="#"
              :auto-upload="false"
              :on-change="handlePreviewFileChange"
              :file-list="previewFileList"
              :limit="1"
              accept="image/*"
            >
              <div class="preview-placeholder">
                <i class="el-icon-picture-outline"></i>
                <span>上传预览图（可选）</span>
                <span class="preview-hint">3D模型等无法直接预览的文件建议上传预览图</span>
              </div>
            </el-upload>
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
      :title="currentWork ? currentWork.title : '作品详情'"
      width="800px"
      :close-on-click-modal="true"
      custom-class="detail-dialog"
      destroy-on-close
      append-to-body
    >
      <div v-if="currentWork" class="detail-content">
        <!-- 顶部预览区 -->
        <div class="detail-header">
          <div class="detail-preview">
            <!-- 模型预览 -->
            <div v-if="isModelFile(currentWork.filePath || currentWork.fileName)" class="preview-model-wrap">
              <ModelCardCover :work="currentWork" />
            </div>
            <!-- 图片预览 -->
            <img
              v-else-if="getThumbnailUrl(currentWork)"
              :src="getThumbnailUrl(currentWork)"
              class="detail-preview-img"
            />
            <!-- 占位符 -->
            <div
              v-else
              class="detail-preview-placeholder"
              :style="{ background: getGradient(currentWork.id, 0) }"
            >
              <span class="file-emoji">{{ getFileEmoji(currentWork) }}</span>
              <span class="file-ext">{{ getFileExtension(currentWork) }}</span>
            </div>
          </div>
        </div>

        <!-- 作品信息 -->
        <div class="detail-info">
          <div class="detail-author-section">
            <div class="detail-author">
              <span class="author-avatar-lg">{{ (currentWork.uploadUserName || '?')[0] }}</span>
              <div class="author-info">
                <span class="detail-author-name">{{ currentWork.uploadUserName || '未知作者' }}</span>
                <span class="detail-date" v-if="currentWork.fileUploadTime || currentWork.uploadDate">
                  上传于 {{ formatDate(currentWork.fileUploadTime || currentWork.uploadDate) }}
                </span>
              </div>
            </div>
          </div>

          <div class="detail-stats-row">
            <div class="stat-item">
              <i class="el-icon-view"></i>
              <span>{{ currentWork.views || 0 }}</span>
              <span class="stat-label">浏览</span>
            </div>
            <div class="stat-item">
              <i class="el-icon-star"></i>
              <span>{{ currentWork.favorites || 0 }}</span>
              <span class="stat-label">收藏</span>
            </div>
            <div class="stat-item">
              <i class="el-icon-folder"></i>
              <span>{{ currentWork.category || '未分类' }}</span>
              <span class="stat-label">分类</span>
            </div>
            <div class="stat-item">
              <i class="el-icon-tag"></i>
              <span :class="['status-badge', currentWork.status]">{{ getStatusLabel(currentWork.status) }}</span>
              <span class="stat-label">状态</span>
            </div>
          </div>

          <!-- 描述 -->
          <div class="detail-description" v-if="currentWork.description">
            <h4><i class="el-icon-file-text"></i> 作品描述</h4>
            <p>{{ currentWork.description }}</p>
          </div>

          <!-- 文件信息 -->
          <div class="detail-file">
            <h4><i class="el-icon-document"></i> 文件信息</h4>
            <div class="file-info-grid">
              <div class="file-info-item">
                <span class="info-label">文件名</span>
                <span class="info-value">{{ currentWork.fileName || currentWork.filePath || '未知' }}</span>
              </div>
              <div class="file-info-item">
                <span class="info-label">文件类型</span>
                <span class="info-value">{{ formatFileType(currentWork.filePath) }}</span>
              </div>
              <div class="file-info-item">
                <span class="info-label">文件大小</span>
                <span class="info-value">{{ formatFileSize(currentWork.fileSize) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <span slot="footer" class="dialog-footer">
        <el-button @click="detailDialogVisible = false" round>关闭</el-button>
        <el-button
          v-if="currentWork && currentWork.filePath"
          type="primary"
          @click="handleDownloadFile(currentWork)"
          round
        >
          <i class="el-icon-download"></i> 下载文件
        </el-button>
      </span>
    </el-dialog>

    <!-- 作品管理/反馈对话框 -->
    <el-dialog
      title="作品管理"
      :visible.sync="reviewDialogVisible"
      width="720px"
      :close-on-click-modal="false"
      custom-class="review-dialog"
      append-to-body
      destroy-on-close
    >
      <div v-if="reviewWork" class="review-dialog-content">
        <!-- 作品预览区 -->
        <div class="review-preview">
          <div class="review-cover">
            <ModelCardCover :work="reviewWork" />
          </div>
          <div class="review-info">
            <h3 class="review-title">{{ reviewWork.title }}</h3>
            <div class="review-meta-row">
              <span class="meta-item">
                <i class="el-icon-folder"></i>
                {{ reviewWork.category || '未分类' }}
              </span>
              <span :class="['status-badge', reviewWork.status]">
                {{ getStatusLabel(reviewWork.status) }}
              </span>
            </div>
            <div class="review-meta-row">
              <span class="meta-item">
                <i class="el-icon-calendar"></i>
                {{ formatDate(reviewWork.fileUploadTime || reviewWork.uploadDate) }}
              </span>
            </div>
          </div>
        </div>

        <!-- 评语历史 -->
        <div class="review-section">
          <h4 class="section-title">
            <i class="el-icon-chat-dot-round"></i>
            评语 / 反馈历史
          </h4>
          <div v-if="reviewHistory.length > 0" class="review-history-list">
            <div
              v-for="item in reviewHistory"
              :key="item.id"
              class="review-card"
            >
              <div class="review-card-header">
                <div class="reviewer-info">
                  <span class="reviewer-avatar">{{ (item.reviewerName || '?')[0] }}</span>
                  <div class="reviewer-details">
                    <span class="reviewer-name">{{ item.reviewerName }}</span>
                    <span class="reviewer-role">
                      {{ item.reviewerRole === 'Admin' ? '管理员' : item.reviewerRole === 'Teacher' ? '教师' : '学生' }}
                    </span>
                  </div>
                </div>
                <div class="review-meta-right">
                  <el-tag
                    size="small"
                    :type="item.type === 'resubmit' ? 'warning' : 'primary'"
                  >
                    {{ item.type === 'review' ? '教师评语' : '修改说明' }}
                  </el-tag>
                  <span class="review-time">{{ formatDateTime(item.createdAt) }}</span>
                </div>
              </div>
              <div class="review-card-body">
                <p>{{ item.comment }}</p>
              </div>
            </div>
          </div>
          <div v-else class="review-empty">
            <i class="el-icon-chat-dot-round"></i>
            <p>暂无评语记录</p>
            <span>等待教师审核后给出反馈</span>
          </div>
        </div>

        <!-- 重新提交 -->
        <div class="review-section resubmit-section">
          <h4 class="section-title">
            <i class="el-icon-upload2"></i>
            修改后重新提交
          </h4>
          <p class="resubmit-hint">如需根据教师建议修改作品，可在此说明修改内容并重新提交审核</p>
          <el-input
            v-model="resubmitComment"
            type="textarea"
            :rows="4"
            placeholder="请说明你做了哪些修改..."
            show-word-limit
            maxlength="500"
          />
          <div class="resubmit-actions">
            <el-button @click="reviewDialogVisible = false" round>关闭</el-button>
            <el-button type="primary" :loading="resubmitting" @click="handleResubmit" round>
              <i class="el-icon-upload2"></i>
              重新提交审核
            </el-button>
          </div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import http from '../../utils/http'
import ModelCardCover from '../../components/ModelCardCover.vue'
import { isModelFile } from '../../utils/fileUtils'

export default {
  name: 'WorkManagement',
  components: {
    ModelCardCover
  },
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
      form: { title: '', category: '', description: '', status: '待审核' },
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
      previewImageFile: null,
      previewFileList: [],
      existingPreviewImage: '',
      previewImageUrl: '',
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
        { label: '审核中', value: '待审核' },
        { label: '已发布', value: '已发布' }
      ],
      // 作品管理/反馈
      reviewDialogVisible: false,
      reviewWork: null,
      reviewHistory: [],
      resubmitComment: '',
      resubmitting: false
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
    if (this.$route.query.action === 'upload') {
      this.$nextTick(() => {
        this.handleCreateWork()
      })
    }
  },
  methods: {
    isModelFile,
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
        const worksData = response.data.items || []
        
        for (const work of worksData) {
          try {
            const favResponse = await http.get(`/api/Work/${work.id}/is-favorite`)
            work.isFavorited = favResponse.data.isFavorite || false
          } catch (e) {
            work.isFavorited = false
          }
        }
        
        this.works = worksData
        this.total = response.data.total || 0
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载作品失败')
      } finally {
        this.loading = false
      }
    },

    async handleViewWork(work) {
      this.currentWork = work
      this.detailDialogVisible = true
      try {
        await http.get(`/api/Work/${work.id}/view`)
        work.views = (work.views || 0) + 1
      } catch (error) {
        console.error('增加浏览量失败:', error)
      }
    },

    async handleFavoriteWork(work) {
      try {
        if (work.isFavorited) {
          await http.delete(`/api/Work/${work.id}/favorite`)
        } else {
          await http.post(`/api/Work/${work.id}/favorite`)
        }
        work.isFavorited = !work.isFavorited
        work.favorites = work.isFavorited ? (work.favorites || 0) + 1 : Math.max(0, (work.favorites || 0) - 1)
        this.$message.success(work.isFavorited ? '收藏成功' : '取消收藏成功')
      } catch (error) {
        this.$message.error(error.response?.data?.message || '操作失败')
      }
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
        status: work.status || '草稿',
        originalStatus: work.status || ''
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

    handlePreviewFileChange(file, fileList) {
      this.previewImageFile = file.raw
      this.previewFileList = fileList.slice(-1)
      this.existingPreviewImage = ''
      // 生成本地预览URL
      if (file.raw) {
        this.previewImageUrl = URL.createObjectURL(file.raw)
      }
    },

    removePreviewImage() {
      this.previewImageFile = null
      this.previewFileList = []
      this.existingPreviewImage = ''
      if (this.previewImageUrl && this.previewImageUrl.startsWith('blob:')) {
        URL.revokeObjectURL(this.previewImageUrl)
      }
      this.previewImageUrl = ''
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

      try {
        let filePath = ''
        
        // 如果有新文件，使用分片上传
        if (this.selectedFile) {
          filePath = await this.uploadFileWithChunks(this.selectedFile)
        } else if (this.isEdit) {
          // 编辑时没有选择新文件，保持原文件路径
          filePath = this.existingFilePath
        }

        // 上传预览图（如果有新选择的预览图）
        let previewImage = ''
        if (this.previewImageFile) {
          const previewFormData = new FormData()
          previewFormData.append('file', this.previewImageFile)
          const previewResponse = await http.post('/api/File/upload-preview', previewFormData, {
            headers: { 'Content-Type': 'multipart/form-data' }
          })
          previewImage = previewResponse.data.fileName
        } else if (this.isEdit && !this.previewImageFile && this.previewFileList.length === 0) {
          // 编辑时未修改预览图，保持原有预览图
          previewImage = this.existingPreviewImage
        }

        // 保存作品信息
        const workData = {
          title: this.form.title,
          category: this.form.category,
          description: this.form.description || '',
          status: this.form.status,
          filePath: filePath,
          fileName: this.selectedFile ? this.selectedFile.name : this.existingFileName,
          fileSize: this.selectedFile ? this.selectedFile.size : this.existingFileSize,
          previewImage: previewImage
        }

        if (this.isEdit && this.form.id) {
          workData.id = this.form.id
          await http.put('/api/Work/' + this.form.id, workData)
          this.$message.success('作品修改成功')
        } else {
          await http.post('/api/Work', workData)
          this.$message.success('作品上传成功')
        }

        this.uploadDialogVisible = false
        this.resetForm()
        this.loadWorks()
      } catch (error) {
        this.$message.error(error.response?.data?.message || error.message || '操作失败')
      } finally {
        this.uploading = false
        this.uploadProgress = 0
      }
    },

    async uploadFileWithChunks(file) {
      const chunkSize = 2 * 1024 * 1024 // 2MB 分片大小
      const totalChunks = Math.ceil(file.size / chunkSize)
      const fileMD5 = await this.calculateMD5(file)
      
      // 检查上传状态（断点续传）
      const statusResponse = await http.get('/api/File/upload-status', {
        params: { md5: fileMD5, totalChunks: totalChunks }
      })
      const uploadedChunks = statusResponse.data.uploadedChunks || []

      // 上传所有分片
      for (let i = 0; i < totalChunks; i++) {
        // 跳过已上传的分片
        if (uploadedChunks.includes(i)) {
          this.updateProgress(i, totalChunks)
          continue
        }

        const start = i * chunkSize
        const end = Math.min(start + chunkSize, file.size)
        const chunk = file.slice(start, end)

        const formData = new FormData()
        formData.append('chunk', chunk)
        formData.append('chunkIndex', i)
        formData.append('totalChunks', totalChunks)
        formData.append('md5', fileMD5)
        formData.append('fileName', file.name)

        await http.post('/api/File/upload-chunk', formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        })

        this.updateProgress(i, totalChunks)
      }

      // 合并分片
      const mergeResponse = await http.post('/api/File/merge-chunks', {
        md5: fileMD5,
        fileName: file.name,
        totalChunks: totalChunks
      })

      return mergeResponse.data.filePath
    },

    updateProgress(currentChunk, totalChunks) {
      this.uploadProgress = Math.round(((currentChunk + 1) / totalChunks) * 100)
    },

    calculateMD5(file) {
      return new Promise((resolve, reject) => {
        const SparkMD5 = require('spark-md5')
        const chunkSize = 2 * 1024 * 1024
        const fileReader = new FileReader()
        const md5 = new SparkMD5.ArrayBuffer()
        let offset = 0

        const readNextChunk = () => {
          const slice = file.slice(offset, offset + chunkSize)
          fileReader.readAsArrayBuffer(slice)
        }

        fileReader.onload = (e) => {
          md5.append(e.target.result)
          offset += chunkSize

          if (offset < file.size) {
            readNextChunk()
          } else {
            resolve(md5.end())
          }
        }

        fileReader.onerror = reject
        readNextChunk()
      })
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
      this.form = { title: '', category: '', description: '', status: '待审核', originalStatus: '' }
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
      if (!work.filePath && !work.fileName) return null
      const filePath = work.filePath || work.fileName || ''
      const ext = filePath.toLowerCase().substring(filePath.lastIndexOf('.'))
      if (['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'].includes(ext)) {
        return `/api/File/download?fileName=${encodeURIComponent(filePath)}`
      }
      return null
    },

    getFileExtension(work) {
      if (!work.filePath) return 'FILE'
      return work.filePath.toLowerCase().substring(work.filePath.lastIndexOf('.') + 1).toUpperCase()
    },

    getGradient(seed, offset = 0) {
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
      return gradients[(seed + offset) % gradients.length]
    },

    getFileEmoji(work) {
      if (!work.filePath) return '📄'
      const ext = work.filePath.toLowerCase().substring(work.filePath.lastIndexOf('.'))
      const emojiMap = {
        '.jpg': '🖼️',
        '.jpeg': '🖼️',
        '.png': '🖼️',
        '.gif': '🖼️',
        '.webp': '🖼️',
        '.mp4': '🎬',
        '.avi': '🎬',
        '.mov': '🎬',
        '.pdf': '📕',
        '.doc': '📘',
        '.docx': '📘',
        '.txt': '📝',
        '.zip': '📦',
        '.rar': '📦',
        '.exe': '⚙️',
        '.js': '💻',
        '.html': '🌐',
        '.css': '🎨',
        '.json': '📋',
        '.xml': '📄',
        '.md': '📝'
      }
      return emojiMap[ext] || '📄'
    },

    formatFileType(filePath) {
      if (!filePath) return '未知类型'
      const ext = filePath.toLowerCase().substring(filePath.lastIndexOf('.') + 1)
      const typeMap = {
        'jpg': 'JPEG 图片',
        'jpeg': 'JPEG 图片',
        'png': 'PNG 图片',
        'gif': 'GIF 图片',
        'webp': 'WebP 图片',
        'mp4': 'MP4 视频',
        'avi': 'AVI 视频',
        'mov': 'MOV 视频',
        'pdf': 'PDF 文档',
        'doc': 'Word 文档',
        'docx': 'Word 文档',
        'txt': '文本文件',
        'zip': 'ZIP 压缩包',
        'rar': 'RAR 压缩包',
        'js': 'JavaScript 文件',
        'html': 'HTML 文件',
        'css': 'CSS 文件',
        'json': 'JSON 文件',
        'xml': 'XML 文件',
        'md': 'Markdown 文件'
      }
      return typeMap[ext] || `${ext.toUpperCase()} 文件`
    },

    // ========== 作品管理/反馈（学生查看评语 & 重新提交） ==========
    async handleOpenReview(work) {
      this.reviewWork = work
      this.resubmitComment = ''
      this.reviewHistory = []
      this.reviewDialogVisible = true
      await this.loadReviewHistory(work.id)
    },

    async loadReviewHistory(workId) {
      try {
        const res = await http.get(`/api/TeachingCollaboration/works/${workId}/reviews`)
        this.reviewHistory = res.data?.reviews || []
      } catch (error) {
        console.error('加载评语历史失败:', error)
        if (error.response?.status === 403) {
          this.reviewHistory = []
        } else {
          this.$message.error('加载评语历史失败')
        }
      }
    },

    async handleResubmit() {
      if (!this.reviewWork) return
      this.resubmitting = true
      try {
        const { data } = await http.post(`/api/Work/${this.reviewWork.id}/resubmit`, {
          comment: this.resubmitComment
        })
        this.$message.success(data.message || '作品已重新提交审核')
        this.reviewDialogVisible = false
        await this.loadWorks()
      } catch (error) {
        this.$message.error(error.response?.data?.message || '重新提交失败')
      } finally {
        this.resubmitting = false
      }
    },

    reviewStatusTag(status) {
      switch (status) {
        case '已发布': return 'success'
        case '待审核': return 'warning'
        case '已归档': return 'info'
        default: return ''
      }
    },

    formatDateTime(value) {
      if (!value) return '暂无'
      return new Date(value).toLocaleString('zh-CN')
    }
  }
}
</script>

<style scoped>
.work-management-page {
  min-height: 100vh;
  background: var(--bg-page, #F8F9F5);
  padding: 24px;
  font-family: var(--font-main);
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
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-xl, 24px);
  border: 1px solid var(--border-color, #E8E2D8);
  margin-bottom: 24px;
  box-shadow: var(--shadow-card);
}

.hero-content {
  color: var(--text-main, #1A1A1A);
}

.hero-title {
  font-size: 28px;
  font-weight: 700;
  margin: 0 0 8px;
}

.hero-desc {
  font-size: 14px;
  color: var(--text-light, #888888);
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
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  padding: 20px;
  text-align: center;
  box-shadow: var(--shadow-card);
}

.stat-icon-wrap {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md, 14px);
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 12px;
  font-size: 20px;
  color: #fff;
}

.stat-color-1 { background: linear-gradient(135deg, var(--primary), var(--primary-light)); }
.stat-color-2 { background: linear-gradient(135deg, #43A047, #66BB6A); }
.stat-color-3 { background: linear-gradient(135deg, #FB8C00, #FFA726); }
.stat-color-4 { background: linear-gradient(135deg, #5C6BC0, #7986CB); }

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin-bottom: 4px;
}

.stat-label {
  font-size: 12px;
  color: var(--text-light, #888888);
}

/* 筛选栏 */
.filter-section {
  margin-bottom: 24px;
}

.filter-bar {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  padding: 16px 20px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 16px;
  box-shadow: var(--shadow-card);
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
  border-radius: var(--radius-full, 9999px);
  font-size: 13px;
  color: var(--text-light, #888888);
  background: var(--bg-hover, #F5F2EC);
  cursor: pointer;
  transition: all var(--duration-fast, .18s);
}

.filter-btn:hover {
  background: var(--primary-bg, #EDF5F0);
}

.filter-btn.active {
  background: var(--primary, #2D8A6E);
  color: #fff;
}

/* 作品网格 */
.works-section {
  margin-bottom: 32px;
}

.works-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
}

.work-card {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  overflow: hidden;
  transition: all var(--duration-normal, .3s) var(--ease-out-expo);
  cursor: pointer;
}

.work-card:hover {
  box-shadow: var(--shadow-card-hover);
  transform: translateY(-4px);
}

.card-cover {
  height: 180px;
  position: relative;
  overflow: hidden;
  cursor: pointer;
  background: var(--border-light, #F2EDE6);
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
  gap: 6px;
  font-size: 13px;
  color: var(--text-light, #888888);
}

.cover-placeholder .file-icon {
  font-size: 48px;
  opacity: 0.8;
}

.category-tag {
  position: absolute;
  top: 12px;
  left: 12px;
  padding: 4px 10px;
  border-radius: var(--radius-full, 9999px);
  background: rgba(255, 255, 255, .85);
  backdrop-filter: blur(8px);
  font-size: 11px;
  font-weight: 600;
  color: var(--primary, #2D8A6E);
}

.status-tag {
  position: absolute;
  top: 12px;
  right: 12px;
  padding: 4px 10px;
  font-size: 11px;
  font-weight: 600;
  border-radius: var(--radius-full, 9999px);
}

.status-tag.已发布 {
  background: rgba(76, 175, 80, 0.9);
  color: #fff;
}

.status-tag.草稿 {
  background: rgba(100, 100, 100, 0.9);
  color: #fff;
}

.status-tag.待审核 {
  background: rgba(251, 140, 0, 0.9);
  color: #fff;
}

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

.card-stats {
  display: flex;
  gap: 12px;
  font-size: 12px;
  color: var(--text-light, #888888);
  margin-bottom: 12px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 4px;
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
  border: 1px solid var(--border-color, #E8E2D8);
  border-radius: var(--radius-sm, 10px);
  background: var(--bg-card, #FFFFFF);
  font-size: 13px;
  color: var(--text-secondary, #555555);
  cursor: pointer;
  transition: all var(--duration-fast, .18s);
}

.action-btn:hover {
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
}

.action-btn.primary {
  background: var(--primary, #2D8A6E);
  border-color: var(--primary, #2D8A6E);
  color: #fff;
}

.action-btn.primary:hover {
  background: var(--primary-hover, #25755C);
  border-color: var(--primary-hover, #25755C);
}

.action-btn.favorite-btn.active {
  border-color: #FFB300;
  color: #FFB300;
  background: #FFF8E1;
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
  border: 2px dashed var(--border-color, #E8E2D8);
  border-radius: var(--radius-md, 14px);
  padding: 40px;
  text-align: center;
  transition: border-color var(--duration-fast, .18s);
}

.file-upload .el-upload-dragger:hover {
  border-color: var(--primary, #2D8A6E);
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
  background: var(--bg-hover, #F5F2EC);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  color: var(--primary, #2D8A6E);
}

.upload-text {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.upload-text span:first-child {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-main, #1A1A1A);
}

.upload-text span:last-child {
  font-size: 12px;
  color: var(--text-light, #888888);
}

.file-selected-info {
  margin-top: 8px;
}

.file-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: var(--bg-page, #F8F9F5);
  border-radius: var(--radius-md, 14px);
}

.file-card-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-sm, 10px);
  background: var(--primary, #2D8A6E);
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
  color: var(--text-main, #1A1A1A);
}

.file-size {
  font-size: 12px;
  color: var(--text-light, #888888);
}

.upload-progress {
  margin-top: 12px;
}

.progress-info {
  display: flex;
  justify-content: space-between;
  margin-top: 8px;
  font-size: 12px;
  color: var(--text-light, #888888);
}

/* 对话框样式 */
::v-deep .work-form-dialog,
::v-deep .detail-dialog {
  border-radius: var(--radius-xl, 24px);
  overflow: hidden;
  box-shadow: var(--shadow-popup);
}

::v-deep .work-form-dialog .el-dialog__header,
::v-deep .detail-dialog .el-dialog__header {
  padding: 24px 28px;
  font-size: 20px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  background: var(--bg-card, #FFFFFF);
  border-bottom: 1px solid var(--border-color, #E8E2D8);
  margin: 0;
}

::v-deep .work-form-dialog .el-dialog__body,
::v-deep .detail-dialog .el-dialog__body {
  padding: 28px;
  background: var(--bg-page, #F8F9F5);
}

::v-deep .work-form-dialog .el-dialog__footer,
::v-deep .detail-dialog .el-dialog__footer {
  padding: 20px 28px;
  background: var(--bg-card, #FFFFFF);
  border-top: 1px solid var(--border-color, #E8E2D8);
}

/* 表单样式优化 */
::v-deep .work-form-dialog .el-form {
  margin: 0;
}

::v-deep .work-form-dialog .el-form-item {
  margin-bottom: 24px;
}

::v-deep .work-form-dialog .el-form-item__label {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  line-height: 1.5;
}

::v-deep .work-form-dialog .el-input__inner {
  border-radius: var(--radius-md, 14px);
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  color: var(--text-main, #1A1A1A);
  font-size: 14px;
  transition: all var(--duration-fast, .18s);
  padding: 0 16px;
  height: 42px;
  line-height: 42px;
}

::v-deep .work-form-dialog .el-input__inner:hover {
  border-color: var(--primary, #2D8A6E);
}

::v-deep .work-form-dialog .el-input__inner:focus {
  border-color: var(--primary, #2D8A6E);
  box-shadow: 0 0 0 3px var(--primary-glow, rgba(45, 138, 110, 0.2));
}

::v-deep .work-form-dialog .el-input__inner::placeholder {
  color: var(--text-placeholder, #B0B0B0);
}

::v-deep .work-form-dialog .el-textarea__inner {
  border-radius: var(--radius-md, 14px);
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  color: var(--text-main, #1A1A1A);
  font-size: 14px;
  transition: all var(--duration-fast, .18s);
  padding: 12px 16px;
  line-height: 1.6;
}

::v-deep .work-form-dialog .el-textarea__inner:hover {
  border-color: var(--primary, #2D8A6E);
}

::v-deep .work-form-dialog .el-textarea__inner:focus {
  border-color: var(--primary, #2D8A6E);
  box-shadow: 0 0 0 3px var(--primary-glow, rgba(45, 138, 110, 0.2));
}

::v-deep .work-form-dialog .el-select .el-input__inner {
  cursor: pointer;
}

::v-deep .work-form-dialog .el-select-dropdown {
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  box-shadow: var(--shadow-popup);
  padding: 8px;
}

::v-deep .work-form-dialog .el-select-dropdown__item {
  border-radius: var(--radius-sm, 10px);
  margin: 2px 0;
  color: var(--text-regular, #333333);
  font-size: 14px;
  transition: all var(--duration-fast, .18s);
}

::v-deep .work-form-dialog .el-select-dropdown__item:hover {
  background: var(--primary-bg, #EDF5F0);
  color: var(--primary, #2D8A6E);
}

::v-deep .work-form-dialog .el-select-dropdown__item.selected {
  background: var(--primary, #2D8A6E);
  color: #fff;
  font-weight: 600;
}

::v-deep .work-form-dialog .el-radio-group {
  display: flex;
  gap: 12px;
}

::v-deep .work-form-dialog .el-radio-button__inner {
  border-radius: var(--radius-md, 14px);
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  color: var(--text-secondary, #555555);
  font-weight: 500;
  font-size: 14px;
  padding: 10px 20px;
  transition: all var(--duration-fast, .18s);
}

::v-deep .work-form-dialog .el-radio-button__inner:hover {
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
}

::v-deep .work-form-dialog .el-radio-button__orig-radio:checked + .el-radio-button__inner {
  background: var(--primary, #2D8A6E);
  border-color: var(--primary, #2D8A6E);
  color: #fff;
  font-weight: 600;
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45, 138, 110, 0.3));
}

/* 按钮样式优化 */
::v-deep .work-form-dialog .el-button {
  border-radius: var(--radius-lg, 18px);
  font-weight: 600;
  font-size: 14px;
  padding: 10px 24px;
  transition: all var(--duration-fast, .18s);
}

::v-deep .work-form-dialog .el-button--default {
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  color: var(--text-secondary, #555555);
}

::v-deep .work-form-dialog .el-button--default:hover {
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
  background: var(--primary-bg, #EDF5F0);
}

::v-deep .work-form-dialog .el-button--primary {
  background: var(--primary, #2D8A6E);
  border-color: var(--primary, #2D8A6E);
  color: #fff;
}

::v-deep .work-form-dialog .el-button--primary:hover {
  background: var(--primary-hover, #25755C);
  border-color: var(--primary-hover, #25755C);
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45, 138, 110, 0.3));
  transform: translateY(-1px);
}

::v-deep .work-form-dialog .el-button--primary:active {
  transform: translateY(0);
}

::v-deep .work-form-dialog .el-button.is-loading {
  opacity: 0.7;
}

::v-deep .work-form-dialog .el-button.is-loading:before {
  background-color: var(--primary, #2D8A6E);
}

/* 上传区域优化 */
::v-deep .work-form-dialog .el-upload-dragger {
  border-radius: var(--radius-lg, 18px);
  border: 2px dashed var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  transition: all var(--duration-fast, .18s);
}

::v-deep .work-form-dialog .el-upload-dragger:hover {
  border-color: var(--primary, #2D8A6E);
  background: var(--primary-bg, #EDF5F0);
}

::v-deep .work-form-dialog .el-upload-dragger .el-icon-upload {
  font-size: 48px;
  color: var(--primary, #2D8A6E);
  margin-bottom: 16px;
}

::v-deep .work-form-dialog .el-upload-dragger .el-upload__text {
  color: var(--text-secondary, #555555);
  font-size: 14px;
}

::v-deep .work-form-dialog .el-upload-dragger .el-upload__text em {
  color: var(--primary, #2D8A6E);
  font-style: normal;
  font-weight: 600;
}

/* 进度条样式 */
::v-deep .work-form-dialog .el-progress-bar__outer {
  background: var(--border-light, #F2EDE6);
  border-radius: var(--radius-full, 9999px);
}

::v-deep .work-form-dialog .el-progress-bar__inner {
  background: linear-gradient(90deg, var(--primary, #2D8A6E), var(--primary-light, #45A884));
  border-radius: var(--radius-full, 9999px);
  transition: all var(--duration-normal, .3s);
}

::v-deep .work-form-dialog .el-progress__text {
  color: var(--primary, #2D8A6E);
  font-weight: 600;
  font-size: 14px;
}

/* 详情对话框内容 */
.detail-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.detail-header {
  width: 100%;
}

.detail-preview {
  width: 100%;
  height: 320px;
  border-radius: var(--radius-lg, 18px);
  overflow: hidden;
  background: var(--bg-card, #FFFFFF);
  border: 1px solid var(--border-color, #E8E2D8);
  box-shadow: var(--shadow-card);
}

.preview-model-wrap {
  width: 100%;
  height: 100%;
}

.preview-model-wrap :deep(.model-card-cover) {
  width: 100%;
  height: 100%;
}

.detail-preview-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.detail-preview-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  background: var(--primary-bg, #EDF5F0);
}

.file-emoji {
  font-size: 64px;
  opacity: 0.7;
}

.file-ext {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-secondary, #555555);
}

.detail-info {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.detail-author-section {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  padding: 16px;
  box-shadow: var(--shadow-card);
}

.detail-author {
  display: flex;
  align-items: center;
  gap: 12px;
}

.author-avatar-lg {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md, 14px);
  background: linear-gradient(135deg, var(--primary, #2D8A6E), var(--primary-light, #45A884));
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  font-weight: 600;
  color: #fff;
}

.author-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.detail-author-name {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
}

.detail-date {
  font-size: 12px;
  color: var(--text-light, #888888);
}

.detail-stats-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 16px;
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  box-shadow: var(--shadow-card);
  transition: all var(--duration-fast, .18s);
}

.stat-item:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-card-hover);
}

.stat-item i {
  font-size: 20px;
  color: var(--primary, #2D8A6E);
}

.stat-item span:first-of-type {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
}

.stat-label {
  font-size: 12px;
  color: var(--text-light, #888888);
}

.status-badge {
  padding: 4px 12px;
  border-radius: var(--radius-full, 9999px);
  font-size: 12px;
  font-weight: 600;
}

.status-badge.已发布 {
  background: rgba(76, 175, 80, 0.15);
  color: #4CAF50;
}

.status-badge.待审核 {
  background: rgba(251, 140, 0, 0.15);
  color: #FB8C00;
}

.status-badge.草稿 {
  background: rgba(100, 100, 100, 0.15);
  color: #666666;
}

.detail-description,
.detail-file {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  padding: 20px;
  box-shadow: var(--shadow-card);
}

.detail-description h4,
.detail-file h4 {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 15px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--border-color, #E8E2D8);
}

.detail-description h4 i,
.detail-file h4 i {
  color: var(--primary, #2D8A6E);
}

.detail-description p {
  font-size: 14px;
  line-height: 1.7;
  color: var(--text-secondary, #555555);
  white-space: pre-wrap;
  margin: 0;
}

.file-info-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}

.file-info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 12px;
  background: var(--bg-page, #F8F9F5);
  border-radius: var(--radius-md, 14px);
}

.info-label {
  font-size: 12px;
  color: var(--text-light, #888888);
}

.info-value {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-main, #1A1A1A);
  word-break: break-all;
}

/* 对话框按钮样式 */
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.dialog-footer .el-button {
  border-radius: var(--radius-lg, 18px);
  font-weight: 600;
  font-size: 14px;
  padding: 10px 24px;
  transition: all var(--duration-fast, .18s);
}

.dialog-footer .el-button--default {
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  color: var(--text-secondary, #555555);
}

.dialog-footer .el-button--default:hover {
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
  background: var(--primary-bg, #EDF5F0);
}

.dialog-footer .el-button--primary {
  background: var(--primary, #2D8A6E);
  border-color: var(--primary, #2D8A6E);
  color: #fff;
}

.dialog-footer .el-button--primary:hover {
  background: var(--primary-hover, #25755C);
  border-color: var(--primary-hover, #25755C);
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45, 138, 110, 0.3));
}

.file-tag:hover {
  background: var(--primary, #2D8A6E);
  color: #fff;
  border-color: var(--primary, #2D8A6E);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45, 138, 110, 0.3));
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

/* 详情对话框按钮优化 */
::v-deep .detail-dialog .el-button {
  border-radius: var(--radius-lg, 18px);
  font-weight: 600;
  font-size: 14px;
  padding: 10px 24px;
  transition: all var(--duration-fast, .18s);
}

::v-deep .detail-dialog .el-button--default {
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  color: var(--text-secondary, #555555);
}

::v-deep .detail-dialog .el-button--default:hover {
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
  background: var(--primary-bg, #EDF5F0);
}

::v-deep .detail-dialog .el-button--primary {
  background: var(--primary, #2D8A6E);
  border-color: var(--primary, #2D8A6E);
  color: #fff;
}

::v-deep .detail-dialog .el-button--primary:hover {
  background: var(--primary-hover, #25755C);
  border-color: var(--primary-hover, #25755C);
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45, 138, 110, 0.3));
  transform: translateY(-1px);
}

::v-deep .detail-dialog .el-button--primary:active {
  transform: translateY(0);
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
}

/* ========== 管理/反馈对话框样式 ========== */
::v-deep .review-dialog {
  border-radius: var(--radius-xl, 24px);
  overflow: hidden;
  box-shadow: var(--shadow-popup);
}

::v-deep .review-dialog .el-dialog__header {
  padding: 24px 28px;
  font-size: 20px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  background: var(--bg-card, #FFFFFF);
  border-bottom: 1px solid var(--border-color, #E8E2D8);
  margin: 0;
}

::v-deep .review-dialog .el-dialog__body {
  padding: 0;
  background: var(--bg-page, #F8F9F5);
}

::v-deep .review-dialog .el-dialog__footer {
  padding: 20px 28px;
  background: var(--bg-card, #FFFFFF);
  border-top: 1px solid var(--border-color, #E8E2D8);
}

.review-dialog-content {
  max-height: 70vh;
  overflow-y: auto;
  padding: 24px;
}

.review-preview {
  display: flex;
  gap: 20px;
  margin-bottom: 24px;
  padding-bottom: 24px;
  border-bottom: 1px solid var(--border-color, #E8E2D8);
}

.review-cover {
  width: 160px;
  height: 120px;
  border-radius: var(--radius-lg, 18px);
  overflow: hidden;
  flex-shrink: 0;
  background: var(--bg-card, #FFFFFF);
  border: 1px solid var(--border-color, #E8E2D8);
  box-shadow: var(--shadow-card);
}

.review-cover :deep(.model-card-cover) {
  width: 100%;
  height: 100%;
}

.review-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 12px;
}

.review-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0;
}

.review-meta-row {
  display: flex;
  align-items: center;
  gap: 16px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--text-secondary, #555555);
}

.meta-item i {
  color: var(--primary, #2D8A6E);
}

.status-badge {
  padding: 4px 12px;
  border-radius: var(--radius-full, 9999px);
  font-size: 12px;
  font-weight: 600;
}

.status-badge.已发布 {
  background: rgba(76, 175, 80, 0.15);
  color: #4CAF50;
}

.status-badge.待审核 {
  background: rgba(251, 140, 0, 0.15);
  color: #FB8C00;
}

.status-badge.草稿 {
  background: rgba(100, 100, 100, 0.15);
  color: #666666;
}

.review-section {
  margin-bottom: 24px;
  padding-bottom: 24px;
  border-bottom: 1px solid var(--border-color, #E8E2D8);
}

.review-section:last-child {
  border-bottom: none;
  margin-bottom: 0;
  padding-bottom: 0;
}

.section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 16px;
}

.section-title i {
  color: var(--primary, #2D8A6E);
}

.review-history-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-height: 300px;
  overflow-y: auto;
}

.review-card {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  box-shadow: var(--shadow-card);
  overflow: hidden;
  transition: all var(--duration-fast, .18s);
}

.review-card:hover {
  box-shadow: var(--shadow-card-hover);
  transform: translateY(-2px);
}

.review-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px;
  background: var(--bg-page, #F8F9F5);
  border-bottom: 1px solid var(--border-color, #E8E2D8);
}

.reviewer-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.reviewer-avatar {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md, 14px);
  background: linear-gradient(135deg, var(--primary, #2D8A6E), var(--primary-light, #45A884));
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: 600;
  color: #fff;
}

.reviewer-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.reviewer-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
}

.reviewer-role {
  font-size: 12px;
  color: var(--text-light, #888888);
}

.review-meta-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.review-time {
  font-size: 12px;
  color: var(--text-light, #888888);
}

.review-card-body {
  padding: 16px;
}

.review-card-body p {
  font-size: 14px;
  line-height: 1.7;
  color: var(--text-secondary, #555555);
  white-space: pre-wrap;
  margin: 0;
}

.review-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
}

.review-empty i {
  font-size: 48px;
  color: var(--border-color, #E8E2D8);
  margin-bottom: 16px;
}

.review-empty p {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 4px;
}

.review-empty span {
  font-size: 13px;
  color: var(--text-light, #888888);
}

.resubmit-section {
  margin-bottom: 0;
}

.resubmit-hint {
  margin: 0 0 16px;
  color: var(--text-secondary, #555555);
  font-size: 13px;
  line-height: 1.6;
}

.resubmit-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 16px;
}

.resubmit-actions .el-button {
  border-radius: var(--radius-lg, 18px);
  font-weight: 600;
  font-size: 14px;
  padding: 10px 24px;
  transition: all var(--duration-fast, .18s);
}

.resubmit-actions .el-button--default {
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  color: var(--text-secondary, #555555);
}

.resubmit-actions .el-button--default:hover {
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
  background: var(--primary-bg, #EDF5F0);
}

.resubmit-actions .el-button--primary {
  background: var(--primary, #2D8A6E);
  border-color: var(--primary, #2D8A6E);
  color: #fff;
}

.resubmit-actions .el-button--primary:hover {
  background: var(--primary-hover, #25755C);
  border-color: var(--primary-hover, #25755C);
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45, 138, 110, 0.3));
}

.resubmit-actions .el-button--primary i {
  margin-right: 6px;
}
</style>
