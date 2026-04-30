<template>
  <div class="gallery-container">
    <!-- 顶部导航 -->
    <header class="gallery-header slide-down">
      <div class="logo-area">
        <div class="logo-icon">BDU</div>
        <h1 class="system-title">保定学院数字作品展厅</h1>
      </div>
      <div class="user-actions">
        <el-button type="primary" round class="action-btn" @click="handleAddWork">
          <i class="el-icon-plus"></i> 上传作品
        </el-button>
      </div>
    </header>

    <!-- 页面头部 -->
    <section class="page-header fade-in-up">
      <h2>作品管理</h2>
      <p>管理您的数字作品，包括上传、编辑、删除和查看作品详情</p>
    </section>

    <!-- 统计卡片 -->
    <section class="stats-section fade-in-up">
      <el-row :gutter="24">
        <el-col :xs="12" :sm="12" :md="6" v-for="(stat, index) in stats" :key="index">
          <el-card class="stat-card stagger-fade-in" :style="{ animationDelay: `${index * 0.1 + 0.2}s` }">
            <div class="stat-icon" :class="`icon-${index + 1}`">
              <i :class="stat.icon"></i>
            </div>
            <div class="stat-value">{{ stat.value }}</div>
            <div class="stat-label">{{ stat.label }}</div>
          </el-card>
        </el-col>
      </el-row>
    </section>

    <!-- 筛选区域 -->
    <section class="filter-section fade-in-up">
      <div class="search-bar">
        <el-input
          v-model="searchQuery"
          placeholder="搜索作品标题或作者..."
          prefix-icon="el-icon-search"
          clearable
          class="animated-input"
          @keyup.enter="loadWorks"
        ></el-input>
      </div>

      <div class="category-filters">
        <span
          v-for="(cat, index) in categories"
          :key="cat.value"
          :class="['filter-tag', { active: categoryFilter === cat.value }]"
          @click="categoryFilter = cat.value; loadWorks()"
          :style="{ animationDelay: `${index * 0.05 + 0.2}s` }"
          class="stagger-fade-in"
        >
          {{ cat.label }}
        </span>
      </div>
    </section>

    <!-- 作品列表 -->
    <section class="works-grid">
      <el-empty v-if="works.length === 0 && !loading" description="暂无作品"></el-empty>

      <el-row :gutter="24">
        <el-col
          :xs="24" :sm="12" :md="8" :lg="6"
          v-for="(work, index) in filteredWorks"
          :key="work.id"
          class="stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.3}s` }"
        >
          <el-card class="work-card" shadow="never" :body-style="{ padding: '0px' }" @click.native="handleViewWork(work)">
            <div class="cover-image">
              <img v-if="getThumbnailUrl(work)" :src="getThumbnailUrl(work)" :alt="work.title" class="thumbnail-image">
              <div v-else class="thumbnail-placeholder" :style="{ backgroundColor: getRandomColor(work.id) }">
                <i class="el-icon-document"></i>
                <span>{{ getFileExtension(work) }}</span>
              </div>
              <span class="category-badge">{{ work.category }}</span>
              <span :class="['status-badge', work.status === '已发布' ? 'published' : work.status === '待审核' ? 'pending' : 'draft']">
                {{ work.status }}
              </span>
              <div class="hover-overlay">
                <el-button type="primary" circle icon="el-icon-caret-right" @click.stop="handleViewWork(work)"></el-button>
              </div>
            </div>

            <div class="work-info">
              <h3 class="work-title">{{ work.title }}</h3>
              <p class="work-author">作者: {{ work.uploadUserName || '未知作者' }}</p>
              <p class="work-upload-user" v-if="isAdmin">上传用户: {{ work.uploadUserUsername }}</p>
              <p class="work-date">{{ work.uploadDate }}</p>

              <div class="work-actions">
                <el-button type="primary" size="small" @click.stop="handleEditWork(work)">编辑</el-button>
                <el-button size="small" @click.stop="handleDeleteWork(work.id)">删除</el-button>
                <el-button
                  v-if="isAdmin && work.status === '待审核'"
                  size="small"
                  type="warning"
                  @click.stop="handleReviewWork(work, '已发布')"
                >
                  通过
                </el-button>
              </div>
            </div>
          </el-card>
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
        ></el-pagination>
      </div>
    </section>

    <!-- 上传/编辑作品对话框 -->
    <el-dialog
      :title="dialogTitle"
      :visible.sync="dialogVisible"
      width="600px"
      :close-on-click-modal="false"
      @close="handleDialogClose"
    >
      <el-form :model="workForm" :rules="rules" ref="workFormRef" label-width="100px">
        <el-form-item label="作品标题" prop="Title">
          <el-input v-model="workForm.Title" placeholder="请输入作品标题" maxlength="100" show-word-limit></el-input>
        </el-form-item>
        <el-form-item label="分类" prop="Category">
          <el-select v-model="workForm.Category" placeholder="请选择作品分类" style="width: 100%">
            <el-option label="前端开发" value="前端开发"></el-option>
            <el-option label="后端开发" value="后端开发"></el-option>
            <el-option label="人工智能" value="人工智能"></el-option>
            <el-option label="UI设计" value="UI设计"></el-option>
            <el-option label="其他" value="其他"></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="状态" prop="Status">
          <el-select v-model="workForm.Status" placeholder="请选择作品状态" style="width: 100%">
            <el-option label="草稿" value="草稿">
              <span style="color: #909399;">草稿</span>
              <span style="color: #C0C4CC; font-size: 12px; margin-left: 8px;">仅自己可见</span>
            </el-option>
            <el-option label="待审核" value="待审核">
              <span style="color: #E6A23C;">待审核</span>
              <span style="color: #C0C4CC; font-size: 12px; margin-left: 8px;">提交后等待管理员审核</span>
            </el-option>
            <el-option label="已发布" value="已发布">
              <span style="color: #67C23A;">已发布</span>
              <span style="color: #C0C4CC; font-size: 12px; margin-left: 8px;">所有人可见</span>
            </el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="作品描述" prop="Description">
          <el-input
            v-model="workForm.Description"
            type="textarea"
            rows="4"
            placeholder="请输入作品描述，介绍作品的功能、特点、技术栈等信息"
            maxlength="1000"
            show-word-limit
          ></el-input>
        </el-form-item>
        <el-form-item label="作品文件" :required="dialogTitle === '上传作品'">
          <el-upload
            class="upload-demo"
            action="#"
            :on-change="handleFileChange"
            :on-remove="handleRemove"
            :file-list="fileList"
            :auto-upload="false"
            :show-file-list="true"
            :before-upload="beforeUpload"
            :limit="1"
            drag
            style="width: 100%"
          >
            <i class="el-icon-upload"></i>
            <div class="el-upload__text">
              <em>点击上传</em> 或 <em>拖拽文件到此处</em>
            </div>
            <div slot="tip" class="el-upload__tip">
              <i class="el-icon-info"></i> 支持任意格式文件，最大支持10GB，采用分片上传技术
            </div>
          </el-upload>

          <!-- 新文件信息 -->
          <div v-if="currentFile && !isUploading" class="file-selected-info">
            <el-alert
              :title="`已选择文件: ${workForm.FileName} (${formatFileSize(workForm.FileSize)})`"
              type="info"
              :closable="false"
              show-icon
            ></el-alert>
          </div>

          <!-- 编辑时显示已有文件 -->
          <div v-if="dialogTitle === '编辑作品' && workForm.FilePath && !currentFile" class="existing-file">
            <el-alert
              :title="`当前文件: ${workForm.FileName} (${formatFileSize(workForm.FileSize)})`"
              type="success"
              :closable="false"
              show-icon
            >
              <template slot="default">
                <el-button type="text" size="small" @click="handlePreviewFileEdit">预览</el-button>
              </template>
            </el-alert>
          </div>

          <!-- 上传进度 -->
          <div v-if="isUploading" class="upload-progress">
            <el-progress
              :percentage="uploadProgress"
              :status="uploadProgress === 100 ? 'success' : 'active'"
              :stroke-width="10"
              :text-inside="true"
            ></el-progress>
            <div class="progress-info">
              <span><i class="el-icon-document"></i> {{ workForm.FileName }}</span>
              <span><i class="el-icon-s-data"></i> {{ formatFileSize(workForm.FileSize) }}</span>
              <span><i class="el-icon-loading"></i> {{ uploadProgress }}%</span>
            </div>
          </div>
        </el-form-item>

        <!-- 预览图上传 -->
        <el-form-item label="预览图片">
          <el-upload
            class="upload-demo"
            action="#"
            :on-change="handlePreviewImageChange"
            :on-remove="handlePreviewImageRemove"
            :file-list="previewImageList"
            :auto-upload="false"
            :show-file-list="true"
            :before-upload="beforePreviewImageUpload"
            :limit="1"
            drag
            style="width: 100%"
          >
            <i class="el-icon-upload"></i>
            <div class="el-upload__text">
              <em>点击上传</em> 或 <em>拖拽图片到此处</em>
            </div>
            <div slot="tip" class="el-upload__tip">
              <i class="el-icon-info"></i> 支持 JPG、PNG、GIF 格式图片，可选填，用于作品卡片预览
            </div>
          </el-upload>

          <!-- 已选择的预览图 -->
          <div v-if="workForm.PreviewImage" class="preview-image-preview">
            <el-alert
              :title="`已选择预览图`"
              type="success"
              :closable="false"
              show-icon
            ></el-alert>
            <img :src="`/api/File/download?fileName=${workForm.PreviewImage}`" class="preview-image-thumb" />
          </div>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click="handleDialogClose">取消</el-button>
        <el-button type="primary" @click="handleSubmitForm" :loading="isUploading" :disabled="isUploading">
          {{ isUploading ? '上传中...' : '确定' }}
        </el-button>
      </div>
    </el-dialog>

    <!-- 作品详情对话框 -->
    <el-dialog
      :title="''"
      :visible.sync="detailDialogVisible"
      width="900px"
      :custom-class="'work-detail-dialog'"
      :close-on-click-modal="false"
    >
      <div v-if="currentWork" class="work-detail-content">
        <div class="work-header">
          <div class="work-cover" :style="{ backgroundColor: getRandomColor(currentWork.id) }">
            <span class="category-badge">{{ currentWork.category }}</span>
          </div>
          <div class="work-info-header">
            <h2 class="work-title-detail">{{ currentWork.title }}</h2>
            <div class="work-author-info">
              <span class="author-name">{{ currentWork.uploadUserName || '未知作者' }}</span>
              <span class="upload-time">{{ currentWork.uploadDate }}</span>
              <el-tag :type="currentWork.status === '已发布' ? 'success' : currentWork.status === '待审核' ? 'warning' : 'info'" size="small">
                {{ currentWork.status }}
              </el-tag>
            </div>
            <div class="work-stats-detail">
              <span class="stat-item">
                <i class="el-icon-view"></i>
                <span>{{ currentWork.views || 0 }}</span>
                <span class="stat-label">浏览</span>
              </span>
              <span class="stat-item">
                <i class="el-icon-star-off"></i>
                <span>{{ currentWork.likes || 0 }}</span>
                <span class="stat-label">点赞</span>
              </span>
              <span class="stat-item">
                <i class="el-icon-chat-line-round"></i>
                <span>{{ currentWork.comments || 0 }}</span>
                <span class="stat-label">评论</span>
              </span>
            </div>
          </div>
        </div>
        
        <div class="work-description-detail">
          <h3 class="section-title-detail">作品描述</h3>
          <div class="description-content">
            {{ currentWork.description || '该作品暂无描述' }}
          </div>
        </div>
        
        <div class="work-file-info">
          <h3 class="section-title-detail">文件信息</h3>
          <div class="file-details">
            <span class="file-item">
              <i class="el-icon-document"></i>
              <span>{{ currentWork.fileName || '未知文件名' }}</span>
            </span>
            <span class="file-item">
              <i class="el-icon-folder"></i>
              <span>{{ currentWork.category || '未分类' }}</span>
            </span>
          </div>
        </div>
      </div>
      <div slot="footer" class="dialog-footer-detail">
        <el-button @click="detailDialogVisible = false" class="close-btn">
          <i class="el-icon-close"></i> 关闭
        </el-button>
        <el-button type="primary" @click="handleDownloadFile" class="download-btn">
          <i class="el-icon-download"></i> 下载文件
        </el-button>
      </div>
    </el-dialog>

    <!-- 文件预览弹窗（统一组件） -->
    <FilePreviewDialog
      v-model="previewDialogVisible"
      :file-path="previewTargetPath"
      :file-name="previewTargetName"
      title="文件预览"
    />
  </div>
</template>

<script>
import SparkMD5 from 'spark-md5'
import ModelPreview from '@/components/ModelPreview'
import FilePreviewDialog from '@/components/FilePreviewDialog.vue'

export default {
  name: 'WorkManagement',
  components: {
    ModelPreview,
    FilePreviewDialog
  },
  data() {
    return {
      searchQuery: '',
      categoryFilter: '',

      categories: [
        { label: '全部', value: '' },
        { label: '前端开发', value: '前端开发' },
        { label: '后端开发', value: '后端开发' },
        { label: '人工智能', value: '人工智能' },
        { label: 'UI设计', value: 'UI设计' },
        { label: '其他', value: '其他' }
      ],

      currentPage: 1,
      pageSize: 12,
      total: 0,

      dialogVisible: false,
      detailDialogVisible: false,
      previewDialogVisible: false,
      dialogTitle: '上传作品',

      workForm: {
        Id: 0,
        Title: '',
        Category: '',
        Description: '',
        UploadDate: '',
        Status: '草稿',
        FilePath: '',
        FileName: '',
        FileSize: 0,
        FileMD5: '',
        PreviewImage: ''
      },

      // 预览目标（避免“编辑预览/详情预览”互相覆盖）
      previewTargetPath: '',
      previewTargetName: '',

      fileList: [],
      currentFile: null,
      uploadProgress: 0,
      
      // 预览图相关
      previewImageList: [],
      currentPreviewImage: null,
      isUploadingPreview: false,
      isUploading: false,
      chunkSize: 5 * 1024 * 1024,
      maxRetries: 3,

      rules: {
        Title: [
          { required: true, message: '请输入作品标题', trigger: 'blur' },
          { min: 2, max: 100, message: '标题长度在 2 到 100 个字符', trigger: 'blur' }
        ],
        Category: [
          { required: true, message: '请选择作品分类', trigger: 'change' }
        ],
        Description: [
          { required: true, message: '请输入作品描述', trigger: 'blur' }
        ]
      },

      currentWork: null,
      works: [],
      isAdmin: false,
      loading: false
    }
  },
  mounted() {
    this.loadWorks()
  },
  computed: {
    stats() {
      return [
        { value: this.works.length, label: '总作品数', icon: 'el-icon-document' },
        { value: this.works.filter(w => w.status === '已发布').length, label: '已发布', icon: 'el-icon-circle-check' },
        { value: this.works.filter(w => w.status === '待审核').length, label: '待审核', icon: 'el-icon-time' },
        { value: this.works.filter(w => w.status === '草稿').length, label: '草稿', icon: 'el-icon-edit-outline' }
      ]
    },
    filteredWorks() {
      return this.works
    },
    canPreview() {
      if (!this.currentWork || !this.currentWork.filePath) {
        return false
      }

      const previewableExtensions = {
        image: ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'],
        video: ['.mp4', '.avi', '.mov', '.wmv', '.flv', '.mkv'],
        model: ['.json', '.obj', '.gltf', '.glb', '.fbx', '.dae']
      }

      const fileName = this.currentWork.filePath || ''
      const extension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'))

      return previewableExtensions.image.includes(extension) ||
             previewableExtensions.video.includes(extension) ||
             previewableExtensions.model.includes(extension)
    }
  },
  methods: {
    formatFileSize(bytes) {
      if (bytes === 0) return '0 B'
      const k = 1024
      const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
      const i = Math.floor(Math.log(bytes) / Math.log(k))
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
    },

    beforeUpload(file) {
      const maxSize = 10 * 1024 * 1024 * 1024
      if (file.size > maxSize) {
        this.$message.error('文件大小不能超过10GB')
        return false
      }
      return true
    },

    handleFileChange(file, fileList) {
      this.fileList = fileList.slice(-1)
      if (file && file.raw) {
        this.currentFile = file.raw
        this.workForm.FileName = file.name
        this.workForm.FileSize = file.size
        this.workForm.FilePath = ''
        this.calculateFileMD5(file.raw)
      }
    },

    handleRemove(file, fileList) {
      this.fileList = []
      this.currentFile = null
    },

    // 预览图上传处理
    handlePreviewImageChange(file, fileList) {
      this.previewImageList = fileList.slice(-1)
      if (file && file.raw) {
        this.currentPreviewImage = file.raw
      }
    },

    handlePreviewImageRemove(file, fileList) {
      this.previewImageList = []
      this.currentPreviewImage = null
      this.workForm.PreviewImage = ''
    },

    beforePreviewImageUpload(file) {
      const isImage = file.type.indexOf('image') > -1
      if (!isImage) {
        this.$message.error('请上传图片格式文件')
        return false
      }
      const isLt5M = file.size / 1024 / 1024 < 5
      if (!isLt5M) {
        this.$message.error('预览图大小不能超过5MB')
        return false
      }
      return true
    },

    calculateFileMD5(file) {
      const chunkSize = this.chunkSize
      const chunks = Math.ceil(file.size / chunkSize)
      let currentChunk = 0
      const spark = new SparkMD5.ArrayBuffer()
      const fileReader = new FileReader()

      const loadNext = () => {
        const start = currentChunk * chunkSize
        const end = Math.min(start + chunkSize, file.size)
        const chunk = file.slice(start, end)

        fileReader.readAsArrayBuffer(chunk)
      }

      fileReader.onload = (e) => {
        spark.append(e.target.result)
        currentChunk++

        if (currentChunk < chunks) {
          loadNext()
        } else {
          const md5 = spark.end()
          this.workForm.FileMD5 = md5
          console.log('文件MD5计算完成:', md5)
        }
      }

      fileReader.onerror = () => {
        this.$message.error('文件读取失败，无法计算MD5')
      }

      loadNext()
    },

    async uploadFileChunk(file, chunkIndex, totalChunks, md5) {
      const chunkSize = this.chunkSize
      const start = chunkIndex * chunkSize
      const end = Math.min(start + chunkSize, file.size)
      const chunk = file.slice(start, end)

      const formData = new FormData()
      formData.append('file', chunk)
      formData.append('chunkIndex', chunkIndex)
      formData.append('totalChunks', totalChunks)
      formData.append('md5', md5)
      formData.append('fileName', file.name)

      try {
        // 分片上传：统一走 /api 相对路径；token 与 multipart headers 由统一 API 层处理
        const response = await this.$axios.post('/api/File/upload-chunk', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
          timeout: 30000
        })
        return response.data
      } catch (error) {
        console.error(`分片${chunkIndex}上传失败:`, error)
        throw error
      }
    },

    async uploadFile() {
      if (!this.currentFile) {
        this.$message.error('请先选择文件')
        return null
      }

      if (!this.workForm.FileMD5) {
        this.$message.error('文件MD5计算中，请稍后')
        return null
      }

      this.isUploading = true
      this.uploadProgress = 0

      const file = this.currentFile
      const chunkSize = this.chunkSize
      const totalChunks = Math.ceil(file.size / chunkSize)

      try {
        for (let chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++) {
          let retries = 0
          let success = false

          while (retries < this.maxRetries && !success) {
            try {
              await this.uploadFileChunk(file, chunkIndex, totalChunks, this.workForm.FileMD5)
              success = true
              this.uploadProgress = Math.round(((chunkIndex + 1) / totalChunks) * 100)
            } catch (error) {
              retries++
              if (retries >= this.maxRetries) {
                throw new Error(`分片${chunkIndex}上传失败，已达到最大重试次数`)
              }
              console.log(`分片${chunkIndex}上传失败，第${retries}次重试...`)
              await new Promise(resolve => setTimeout(resolve, 1000 * retries))
            }
          }
        }

        // 合并分片：统一走 /api 相对路径，由开发代理转发
        const mergeResponse = await this.$axios.post('/api/File/merge-chunks', {
          md5: this.workForm.FileMD5,
          fileName: file.name,
          totalChunks: totalChunks
        })

        this.uploadProgress = 100
        this.$message.success('文件上传成功')

        return mergeResponse.data.filePath

      } catch (error) {
        console.error('文件上传失败:', error)
        this.$message.error('文件上传失败: ' + (error.response?.data?.message || error.message))
        return null
      } finally {
        this.isUploading = false
      }
    },

    // 上传预览图（简单上传，不分片）
    async uploadPreviewImage() {
      if (!this.currentPreviewImage) {
        this.$message.error('请先选择预览图')
        return null
      }

      this.isUploadingPreview = true

      const file = this.currentPreviewImage

      try {
        const formData = new FormData()
        formData.append('file', file)

        const response = await this.$axios.post('/api/File/upload-preview', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
          timeout: 30000
        })

        this.$message.success('预览图上传成功')
        return response.data.fileName

      } catch (error) {
        console.error('预览图上传失败:', error)
        this.$message.error('预览图上传失败: ' + (error.response?.data?.message || error.message))
        return null
      } finally {
        this.isUploadingPreview = false
      }
    },

    async loadWorks() {
      this.loading = true
      try {
        // 作品列表接口需要登录；token 会由统一 http 实例自动注入
        const response = await this.$axios.get('/api/Work', {
          params: {
            search: this.searchQuery,
            category: this.categoryFilter,
            page: this.currentPage,
            pageSize: this.pageSize
          }
        })
        this.works = response.data.items || []
        this.total = response.data.total || this.works.length
        this.isAdmin = response.data.isAdmin || false
      } catch (error) {
        console.error('加载作品失败', error)
        this.$message.error(error.response?.data?.message || '加载作品失败，请稍后重试')
      } finally {
        this.loading = false
      }
    },

    handleAddWork() {
      this.dialogTitle = '上传作品'
      this.resetForm()
      this.dialogVisible = true
    },

    // 对话框关闭处理
    handleDialogClose() {
      this.dialogVisible = false
      // 如果正在上传，给出提示
      if (this.isUploading) {
        this.$confirm('文件正在上传中，确定要取消吗？', '提示', {
          confirmButtonText: '确定',
          cancelButtonText: '继续上传',
          type: 'warning'
        }).then(() => {
          this.resetForm()
        }).catch(() => {
          this.dialogVisible = true
        })
      } else {
        this.resetForm()
      }
    },

    // 重置表单
    resetForm() {
      this.workForm = {
        Id: 0,
        Title: '',
        Category: '其他',
        Description: '',
        UploadDate: new Date().toISOString().split('T')[0],
        Status: '草稿',
        FilePath: '',
        FileName: '',
        FileSize: 0,
        FileMD5: ''
      }
      this.fileList = []
      this.currentFile = null
      this.uploadProgress = 0
      // 重置表单验证
      if (this.$refs.workFormRef) {
        this.$refs.workFormRef.resetFields()
      }
    },

    handleEditWork(row) {
      this.dialogTitle = '编辑作品'
      this.workForm = {
        Id: row.id || 0,
        Title: row.title || '',
        Category: row.category || '其他',
        Description: row.description || '',
        UploadDate: row.uploadDate ? new Date(row.uploadDate).toISOString().split('T')[0] : new Date().toISOString().split('T')[0],
        Status: row.status || '草稿',
        FilePath: row.filePath || '',
        FileName: row.fileName || '',
        FileSize: row.fileSize || 0,
        FileMD5: ''
      }
      this.fileList = []
      this.currentFile = null
      this.dialogVisible = true
    },

    handleViewWork(row) {
      this.currentWork = row
      this.detailDialogVisible = true
    },

    async handleSubmitForm() {
      try {
        await this.$refs.workFormRef.validate()
      } catch (error) {
        return
      }

      // 显示上传中提示
      const loading = this.$loading({
        lock: true,
        text: '正在上传作品...',
        spinner: 'el-icon-loading',
        background: 'rgba(0, 0, 0, 0.7)'
      })

      try {
        // 如果有预览图，先上传预览图
        if (this.currentPreviewImage) {
          const previewImagePath = await this.uploadPreviewImage()
          if (!previewImagePath) {
            loading.close()
            return
          }
          this.workForm.PreviewImage = previewImagePath
        }

        // 如果有新文件，先上传文件
        if (this.currentFile) {
          const filePath = await this.uploadFile()
          if (!filePath) {
            loading.close()
            return
          }
          this.workForm.FilePath = filePath
        }

        // 如果没有文件路径（新上传且没有文件），提示错误
        if (!this.workForm.FilePath && this.dialogTitle === '上传作品') {
          this.$message.error('请上传作品文件')
          loading.close()
          return
        }

        // 构建请求数据
        const requestData = {
          title: this.workForm.Title,
          category: this.workForm.Category,
          description: this.workForm.Description,
          status: this.workForm.Status,
          filePath: this.workForm.FilePath,
          fileName: this.workForm.FileName,
          fileSize: this.workForm.FileSize,
          fileMD5: this.workForm.FileMD5,
          previewImage: this.workForm.PreviewImage,
          uploadDate: new Date().toISOString()
        }

        if (this.dialogTitle === '上传作品') {
          // 创建作品：token 由统一 http 实例自动注入
          await this.$axios.post('/api/Work', requestData, {
            headers: { 'Content-Type': 'application/json' }
          })
          this.$message.success('作品上传成功')
        } else {
          // 更新作品：token 由统一 http 实例自动注入
          await this.$axios.put(`/api/Work/${this.workForm.Id}`, requestData, {
            headers: { 'Content-Type': 'application/json' }
          })
          this.$message.success('作品更新成功')
        }

        this.dialogVisible = false
        // 重置表单
        this.resetForm()
        // 刷新作品列表
        await this.loadWorks()
      } catch (error) {
        console.error('提交失败:', error)
        this.$message.error('提交失败: ' + (error.response?.data?.message || error.message))
      } finally {
        loading.close()
      }
    },

    async handleDeleteWork(id) {
      try {
        await this.$confirm('确定要删除这个作品吗？', '提示', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
      } catch {
        return
      }

      try {
        // 删除作品：token 由统一 http 实例自动注入
        await this.$axios.delete(`/api/Work/${id}`)
        this.$message.success('删除成功')
        this.loadWorks()
      } catch (error) {
        console.error('删除失败:', error)
        this.$message.error('删除失败: ' + (error.response?.data?.message || error.message))
      }
    },

    async handleReviewWork(work, status) {
      try {
        // 审核作品：token 由统一 http 实例自动注入
        await this.$axios.put(`/api/Work/${work.id}/review`, {
          status: status
        })
        this.$message.success(`作品已${status === '已发布' ? '通过审核' : '拒绝'}`)
        this.loadWorks()
      } catch (error) {
        console.error('审核失败:', error)
        this.$message.error('审核失败')
      }
    },

    handleDownloadFile() {
      if (!this.currentWork || !this.currentWork.filePath) {
        this.$message.error('文件路径不存在，无法下载')
        return
      }

      try {
        const downloadUrl = `/api/File/download?fileName=${encodeURIComponent(this.currentWork.filePath)}`
        const link = document.createElement('a')
        link.href = downloadUrl
        link.download = this.currentWork.fileName || this.currentWork.filePath
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        this.$message.success('文件下载已开始')
      } catch (error) {
        console.error('下载文件失败:', error)
        this.$message.error('文件下载失败，请稍后重试')
      }
    },

    handlePreviewFileEdit() {
      if (!this.workForm.FilePath) {
        this.$message.error('没有可预览的文件')
        return
      }

      // 使用统一预览弹窗：只需要设置预览目标并打开
      this.previewTargetPath = this.workForm.FilePath
      this.previewTargetName = this.workForm.FileName || this.workForm.FilePath
      this.previewDialogVisible = true
    },

    handlePreviewFile() {
      if (!this.currentWork || !this.currentWork.filePath) {
        this.$message.error('没有可预览的文件')
        return
      }

      this.previewTargetPath = this.currentWork.filePath
      this.previewTargetName = this.currentWork.fileName || this.currentWork.filePath
      this.previewDialogVisible = true
    },

    getThumbnailUrl(work) {
      if (!work.filePath) return null

      const fileName = work.filePath
      const extension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'))
      const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp']

      if (imageExtensions.includes(extension)) {
        // 缩略图直接走下载接口即可（图片类浏览器可直接展示）
        return `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
      }
      return null
    },

    getFileExtension(work) {
      if (!work.filePath) return ''
      const fileName = work.filePath
      return fileName.substring(fileName.lastIndexOf('.') + 1).toUpperCase()
    },

    getRandomColor(id) {
      const colors = [
        '#e6f7ff', '#f6ffed', '#fff7e6', '#fff0f6', '#f0f5ff',
        '#e8f5e8', '#fff3e0', '#f3e5f5', '#e3f2fd', '#fffde7'
      ]
      return colors[id % colors.length]
    },

    handleSizeChange(val) {
      this.pageSize = val
      this.currentPage = 1
      this.loadWorks()
    },

    handleCurrentChange(val) {
      this.currentPage = val
      this.loadWorks()
    }
  }
}
</script>

<style scoped>
.gallery-container {
  min-height: 100vh;
  background-color: #f8fafc;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  overflow-x: hidden;
}

.gallery-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 40px;
  height: 64px;
  background-color: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(0,0,0,0.05);
  position: sticky;
  top: 0;
  z-index: 100;
}

.slide-down {
  animation: slideDown 0.5s ease-out;
}

.logo-area { display: flex; align-items: center; gap: 12px; }
.logo-icon { background: #0052D9; color: white; font-weight: bold; padding: 4px 8px; border-radius: 6px; }
.system-title { font-size: 20px; font-weight: 600; color: #1a1a1a; margin: 0; }

.action-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 82, 217, 0.3);
}

.page-header {
  text-align: center;
  padding: 40px 0 20px;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e7ed 100%);
  margin-bottom: 0;
}

.page-header h2 {
  font-size: 32px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 8px;
}

.page-header p {
  font-size: 16px;
  color: #666;
  margin: 0;
}

.fade-in-up {
  animation: fadeInUp 0.6s ease-out both;
}

.stats-section {
  max-width: 1200px;
  margin: 40px auto;
  padding: 0 24px;
}

.stat-card {
  margin-bottom: 24px;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(0,0,0,0.05);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  background: white;
  padding: 24px;
  text-align: center;
}

.stat-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 16px 32px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  font-size: 24px;
  color: white;
}

.icon-1 { background: linear-gradient(135deg, #0052D9, #64B5F6); }
.icon-2 { background: linear-gradient(135deg, #34C759, #81C784); }
.icon-3 { background: linear-gradient(135deg, #FF9500, #FFB84D); }
.icon-4 { background: linear-gradient(135deg, #FF2D55, #FF6B8A); }

.stat-value {
  font-size: 32px;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #666;
}

.filter-section {
  max-width: 1200px;
  margin: 0 auto 32px;
  padding: 0 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 24px;
}

.search-bar { width: 100%; max-width: 600px; }
::v-deep .animated-input .el-input__inner {
  border-radius: 24px;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0,0,0,0.02);
}
::v-deep .animated-input .el-input__inner:focus {
  box-shadow: 0 4px 16px rgba(0, 82, 217, 0.15);
  border-color: #0052D9;
}

.category-filters { display: flex; gap: 12px; flex-wrap: wrap; justify-content: center; }
.filter-tag {
  padding: 8px 20px;
  border-radius: 20px;
  background: #ffffff;
  color: #666;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  border: 1px solid #e4e7ed;
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
}
.filter-tag:hover {
  color: #0052D9;
  border-color: #0052D9;
  transform: translateY(-2px);
}
.filter-tag.active {
  background: #0052D9;
  color: #ffffff;
  border-color: #0052D9;
}

.works-grid { max-width: 1200px; margin: 0 auto; padding: 0 24px 60px; }

.stagger-fade-in {
  opacity: 0;
  animation: fadeInUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.work-card {
  margin-bottom: 24px;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(0,0,0,0.05);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  background: white;
  cursor: pointer;
}

.work-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 16px 32px rgba(0, 0, 0, 0.1);
}

.cover-image {
  height: 180px;
  width: 100%;
  position: relative;
  display: flex;
  align-items: flex-start;
  justify-content: flex-end;
  padding: 12px;
  overflow: hidden;
}

.thumbnail-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.thumbnail-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: white;
  font-size: 24px;
  font-weight: bold;
}

.category-badge {
  background: rgba(0, 0, 0, 0.5);
  color: white;
  font-size: 12px;
  padding: 4px 10px;
  border-radius: 12px;
  backdrop-filter: blur(4px);
  z-index: 2;
}

.status-badge {
  position: absolute;
  top: 12px;
  left: 12px;
  font-size: 12px;
  padding: 4px 10px;
  border-radius: 12px;
  backdrop-filter: blur(4px);
  z-index: 2;
}
.status-badge.published { background: rgba(52, 199, 89, 0.9); color: white; }
.status-badge.pending { background: rgba(255, 149, 0, 0.9); color: white; }
.status-badge.draft { background: rgba(102, 102, 102, 0.9); color: white; }

.hover-overlay {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0, 82, 217, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: all 0.3s ease;
  backdrop-filter: blur(2px);
}

.hover-overlay .el-button {
  transform: scale(0.5);
  transition: transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.work-card:hover .hover-overlay { opacity: 1; }
.work-card:hover .hover-overlay .el-button { transform: scale(1); }

.work-info { padding: 16px; }
.work-title {
  font-size: 16px;
  font-weight: 600;
  margin: 0 0 8px 0;
  color: #333;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  transition: color 0.3s;
}
.work-card:hover .work-title { color: #0052D9; }
.work-author, .work-upload-user, .work-date { font-size: 13px; color: #888; margin: 0 0 8px 0; }

.work-actions {
  display: flex;
  gap: 8px;
  margin-top: 12px;
  flex-wrap: wrap;
}

.pagination {
  display: flex;
  justify-content: center;
  margin-top: 32px;
}

.work-detail { padding: 20px 0; }
.work-detail h3 { margin-bottom: 24px; color: #303133; font-size: 20px; font-weight: 600; }
.work-meta { display: flex; flex-wrap: wrap; margin-bottom: 24px; padding-bottom: 16px; border-bottom: 1px solid #e4e7ed; gap: 16px; }
.work-meta span { color: #606266; font-size: 14px; }
.work-description h4 { margin-bottom: 12px; color: #303133; font-size: 16px; font-weight: 500; }
.work-description p { line-height: 1.6; color: #606266; font-size: 14px; }
.work-files h4 { margin-bottom: 12px; color: #303133; font-size: 16px; font-weight: 500; }

.file-preview { height: 60vh; }
.image-preview { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; }
.preview-image { max-width: 100%; max-height: 100%; object-fit: contain; }
.video-preview { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; }
.preview-video { max-width: 100%; max-height: 100%; }

.existing-file {
  margin-top: 16px;
}

.file-selected-info {
  margin-top: 16px;
}
.file-info {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.upload-progress {
  margin-top: 16px;
}
.progress-info {
  display: flex;
  justify-content: space-between;
  margin-top: 8px;
  font-size: 13px;
  color: #666;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@keyframes fadeInUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@media (max-width: 768px) {
  .gallery-header {
    padding: 0 20px;
  }

  .system-title {
    font-size: 16px;
  }

  .page-header {
    padding: 30px 0 15px;
  }

  .page-header h2 {
    font-size: 24px;
  }

  .filter-section,
  .works-grid {
    padding: 0 16px;
  }

  .work-meta {
    flex-direction: column;
    gap: 8px;
  }
}

/* 作品详情对话框样式 */
.work-detail-dialog {
  border-radius: 16px;
  overflow: hidden;
}

.work-detail-content {
  padding: 0;
}

.work-header {
  display: flex;
  padding: 32px;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e7ed 100%);
  border-bottom: 1px solid #e4e7ed;
}

.work-cover {
  width: 180px;
  height: 180px;
  border-radius: 12px;
  display: flex;
  align-items: flex-start;
  justify-content: flex-end;
  padding: 12px;
  margin-right: 24px;
  flex-shrink: 0;
}

.work-info-header {
  flex: 1;
}

.work-title-detail {
  font-size: 24px;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 16px 0;
  line-height: 1.3;
}

.work-author-info {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.author-name {
  font-size: 16px;
  color: #333;
  font-weight: 500;
}

.upload-time {
  font-size: 14px;
  color: #888;
}

.work-stats-detail {
  display: flex;
  gap: 32px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.stat-item i {
  font-size: 20px;
  color: #0052D9;
}

.stat-item span:first-of-type {
  font-size: 18px;
  font-weight: 600;
  color: #333;
}

.stat-label {
  font-size: 12px;
  color: #888;
}

.work-description-detail,
.work-file-info {
  padding: 32px;
  border-bottom: 1px solid #f0f0f0;
}

.work-description-detail:last-child {
  border-bottom: none;
}

.section-title-detail {
  font-size: 18px;
  font-weight: 600;
  color: #333;
  margin: 0 0 16px 0;
  position: relative;
  display: inline-block;
}

.section-title-detail::after {
  content: '';
  position: absolute;
  bottom: -6px;
  left: 0;
  width: 40px;
  height: 3px;
  background: #0052D9;
  border-radius: 2px;
}

.description-content {
  font-size: 15px;
  line-height: 1.6;
  color: #666;
  white-space: pre-wrap;
}

.file-details {
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
}

.file-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #666;
}

.file-item i {
  color: #0052D9;
}

.dialog-footer-detail {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 24px 32px;
  background: #f8fafc;
  border-top: 1px solid #f0f0f0;
}

.close-btn,
.download-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.close-btn:hover {
  color: #0052D9;
  border-color: #0052D9;
}

.download-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 82, 217, 0.3);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .work-header {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }
  
  .work-cover {
    margin-right: 0;
    margin-bottom: 20px;
  }
  
  .work-author-info {
    justify-content: center;
  }
  
  .work-stats-detail {
    justify-content: center;
  }
  
  .work-description-detail,
  .work-file-info {
    padding: 20px;
  }
  
  .dialog-footer-detail {
    padding: 16px 20px;
  }
}
</style>