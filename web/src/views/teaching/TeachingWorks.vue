<template>
  <div class="teaching-page">
    <section class="page-shell">
      <div class="page-title">
        <p>{{ pageSubtitle }}</p>
        <h1>{{ pageTitle }}</h1>
        <span v-if="selectedStudentName" class="student-hint">当前筛选学生：{{ selectedStudentName }}</span>
      </div>

      <div class="filter-grid">
        <el-input
          v-model.trim="filters.search"
          placeholder="搜索作品标题或描述"
          clearable
          @keyup.enter.native="loadWorks"
        />
        <el-select v-model="filters.studentId" clearable placeholder="选择学生" @change="loadWorks">
          <el-option
            v-for="student in students"
            :key="student.id"
            :label="student.name"
            :value="student.id"
          />
        </el-select>
        <el-select v-model="filters.status" clearable placeholder="作品状态" @change="loadWorks">
          <el-option v-for="item in statusOptions" :key="item" :label="item" :value="item" />
        </el-select>
        <el-input
          v-model.trim="filters.category"
          placeholder="分类筛选"
          clearable
          @keyup.enter.native="loadWorks"
        />
        <el-button type="primary" @click="loadWorks">搜索</el-button>
      </div>
    </section>

    <section class="table-shell">
      <el-table :data="works" stripe v-loading="loading">
        <el-table-column prop="title" label="作品标题" min-width="220" />
        <el-table-column prop="studentName" label="学生" min-width="140" />
        <el-table-column prop="studentWorkId" label="学号" width="110" />
        <el-table-column prop="category" label="分类" width="120" />
        <el-table-column label="当前状态" width="120">
          <template slot-scope="{ row }">
            <el-tag :type="statusTagType(row.status)">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="优秀作品" width="100">
          <template slot-scope="{ row }">
            <el-tag :type="row.isExcellent ? 'warning' : 'info'">
              {{ row.isExcellent ? '已标记' : '未标记' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="上传时间" min-width="170">
          <template slot-scope="{ row }">
            {{ formatDateTime(row.fileUploadTime || row.uploadDate) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template slot-scope="{ row }">
            <el-button type="text" @click="openReview(row)">管理</el-button>
            <el-button type="text" @click="download(row)">下载</el-button>
            <el-button type="text" @click="viewWork(row)">查看</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          :current-page="pagination.page"
          :page-size="pagination.pageSize"
          :total="pagination.total"
          layout="total, prev, pager, next"
          @current-change="handlePageChange"
        />
      </div>

      <el-empty v-if="!loading && works.length === 0" description="当前筛选条件下没有作品" />
    </section>

    <!-- ========== 管理学生作品弹窗 ========== -->
    <el-dialog
      :visible.sync="dialogVisible"
      width="820px"
      :close-on-click-modal="false"
      :append-to-body="true"
      :modal-append-to-body="true"
      custom-class="teaching-work-dialog"
      @closed="currentWork = null"
    >
      <div v-if="currentWork" class="manage-work-dialog">
        <!-- 左侧：作品预览 -->
        <div class="work-preview-section">
          <div class="preview-header">
            <h3 class="preview-title">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="section-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
              作品预览
            </h3>
          </div>
          <div class="preview-content">
            <!-- 3D模型预览 -->
            <div v-if="getFileType(currentWork.filePath || currentWork.fileName) === 'model'" class="preview-model-wrap">
              <ModelCardCover :work="currentWork" />
            </div>
            <!-- 图片预览 -->
            <div v-else-if="getFileType(currentWork.filePath || currentWork.fileName) === 'image'" class="preview-image-wrap">
              <img :src="getPreviewUrl(currentWork)" :alt="currentWork.title" class="preview-img" />
            </div>
            <!-- 视频预览 -->
            <div v-else-if="getFileType(currentWork.filePath || currentWork.fileName) === 'video'" class="preview-empty-state">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="empty-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" /></svg>
              <span>视频内容 — 需下载后查看</span>
            </div>
            <!-- 文档预览 -->
            <div v-else-if="getFileType(currentWork.filePath || currentWork.fileName) === 'document'" class="preview-empty-state">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="empty-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
              <span>{{ currentWork.fileName || '文档文件' }}</span>
            </div>
            <!-- 其他类型 -->
            <div v-else class="preview-empty-state">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="empty-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" /></svg>
              <span>{{ currentWork.fileName || '文件' }}</span>
            </div>
          </div>
        </div>

        <!-- 右侧：作品信息与评语 -->
        <div class="work-info-section">
          <!-- 作品基本信息卡片 -->
          <div class="info-hero-card">
            <h2 class="work-title">{{ currentWork.title }}</h2>
            <div class="work-author-row">
              <span class="author-avatar">{{ (currentWork.studentName || '?')[0] }}</span>
              <span class="author-name">{{ currentWork.studentName || '未知学生' }}</span>
            </div>
            <div class="work-tags-row">
              <span class="info-tag">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="tag-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" /></svg>
                {{ currentWork.category || '未分类' }}
              </span>
              <span class="info-tag">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="tag-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                {{ formatDateTime(currentWork.fileUploadTime || currentWork.uploadDate) }}
              </span>
              <el-tag :type="currentWork.isExcellent ? 'warning' : 'info'" size="small">
                {{ currentWork.isExcellent ? '优秀作品' : '普通作品' }}
              </el-tag>
            </div>
          </div>

          <!-- 评语历史 -->
          <div class="review-history-section">
            <h3 class="section-heading">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="section-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-3.582 8-8 8a8.013 8.013 0 01-5.73-2.28L3 16l-.35-1.05A6.014 6.014 0 012 12c0-4.418 3.582-8 8-8s8 3.582 8 8z" /></svg>
              评语 / 反馈历史
            </h3>
            <div v-if="reviewHistory.length > 0" class="review-history-list">
              <div v-for="item in reviewHistory" :key="item.id" class="review-card">
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
                  <div class="review-meta">
                    <span v-if="item.type" class="review-type-tag">{{ item.type === 'review' ? '评语' : '反馈' }}</span>
                    <span class="review-time">{{ formatDateTime(item.createdAt) }}</span>
                  </div>
                </div>
                <div class="review-card-body">
                  <p class="review-text">{{ item.comment }}</p>
                </div>
              </div>
            </div>
            <div v-else class="review-empty">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="empty-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-3.582 8-8 8a8.013 8.013 0 01-5.73-2.28L3 16l-.35-1.05A6.014 6.014 0 012 12c0-4.418 3.582-8 8-8s8 3.582 8 8z" /></svg>
              <span>暂无评语记录</span>
            </div>
          </div>

          <!-- 添加评语表单 -->
          <div class="add-review-section">
            <h3 class="section-heading">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="section-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4" /></svg>
              添加评语
            </h3>
            <el-form label-width="90px" class="review-form">
              <el-form-item label="作品状态">
                <el-select v-model="reviewForm.status" placeholder="请选择状态" style="width: 100%">
                  <el-option v-for="item in statusOptions" :key="item" :label="item" :value="item" />
                </el-select>
              </el-form-item>
              <el-form-item label="优秀推荐">
                <el-switch v-model="reviewForm.isExcellent" active-text="是" inactive-text="否" />
              </el-form-item>
              <el-form-item label="评语/建议">
                <el-input
                  v-model="reviewForm.comment"
                  type="textarea"
                  :rows="3"
                  placeholder="请为学生写评语或修改建议，学生将收到通知"
                  show-word-limit
                  maxlength="1000"
                />
              </el-form-item>
            </el-form>
            <div class="form-actions">
              <el-button @click="dialogVisible = false" class="btn-cancel">取消</el-button>
              <el-button type="primary" :loading="saving" @click="saveReview" class="btn-submit">
                保存评语
              </el-button>
            </div>
          </div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import http from '../../utils/http'
  import { getUser } from '../../utils/auth'
  import ModelCardCover from '../../components/ModelCardCover.vue'

  export default {
  name: 'TeachingWorks',
  components: {
    ModelCardCover
  },
  data() {
    return {
      students: [],
      works: [],
      loading: false,
      saving: false,
      dialogVisible: false,
      currentWork: null,
      reviewHistory: [],
      loadingHistory: false,
      filters: {
        search: '',
        studentId: '',
        status: '',
        category: ''
      },
      pagination: {
        page: 1,
        pageSize: 10,
        total: 0
      },
      reviewForm: {
        status: '已发布',
        isExcellent: false,
        comment: ''
      },
      statusOptions: ['草稿', '待审核', '已发布', '已归档']
    }
  },
  computed: {
    pageTitle() {
      const user = getUser()
      return user?.role === 'Admin' ? '学生作品管理' : '我的学生作品'
    },
    pageSubtitle() {
      const user = getUser()
      return user?.role === 'Admin' ? '管理员可管理所有学生作品' : '教师只能管理自己负责学生的作品'
    },
    selectedStudentName() {
      const student = this.students.find(item => item.id === this.filters.studentId)
      return student?.name || this.$route.query.studentName || ''
    }
  },
  async mounted() {
    if (this.$route.query.studentId) {
      this.filters.studentId = Number(this.$route.query.studentId)
    }
    await this.loadStudents()
    await this.loadWorks()
  },
  methods: {
    async loadStudents() {
      try {
        const { data } = await http.get('/api/TeachingCollaboration/students')
        this.students = data.items || []
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载学生筛选项失败')
      }
    },
    async loadWorks() {
      this.loading = true
      try {
        const { data } = await http.get('/api/TeachingCollaboration/works', {
          params: {
            ...this.filters,
            page: this.pagination.page,
            pageSize: this.pagination.pageSize
          }
        })
        this.works = data.items || []
        this.pagination.total = data.total || 0
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载学生作品失败')
      } finally {
        this.loading = false
      }
    },
    handlePageChange(page) {
      this.pagination.page = page
      this.loadWorks()
    },
    async openReview(work) {
      this.currentWork = work
      this.reviewForm = {
        status: work.status,
        isExcellent: !!work.isExcellent,
        comment: ''
      }
      this.dialogVisible = true
      // 加载评语历史
      await this.loadReviewHistory(work.id)
    },
    async loadReviewHistory(workId) {
      this.loadingHistory = true
      try {
        const { data } = await http.get(`/api/TeachingCollaboration/works/${workId}/reviews`)
        this.reviewHistory = data.reviews || []
      } catch (error) {
        console.error('加载评语历史失败:', error)
        this.reviewHistory = []
      } finally {
        this.loadingHistory = false
      }
    },
    async saveReview() {
      if (!this.currentWork) return

      this.saving = true
      try {
        const { data } = await http.put(
          `/api/TeachingCollaboration/works/${this.currentWork.id}/review`,
          this.reviewForm
        )
        this.$message.success(data.message || '作品更新成功')
        this.dialogVisible = false
        await this.loadWorks()
      } catch (error) {
        this.$message.error(error.response?.data?.message || '更新作品失败')
      } finally {
        this.saving = false
      }
    },
    viewWork(work) {
      this.$router.push(`/works/${work.id}`)
    },
    async download(work) {
      if (!work?.filePath) {
        this.$message.error('当前作品没有可下载文件')
        return
      }

      try {
        const res = await http.get('/api/File/download', {
          params: { fileName: work.filePath },
          responseType: 'blob'
        })
        const blob = res.data
        const url = window.URL.createObjectURL(blob)
        const link = document.createElement('a')
        link.href = url
        link.download = work.fileName || work.filePath
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        window.URL.revokeObjectURL(url)
      } catch (error) {
        this.$message.error(error.response?.data?.message || '下载文件失败')
      }
    },
    statusTagType(status) {
      switch (status) {
        case '已发布':
          return 'success'
        case '待审核':
          return 'warning'
        case '已归档':
          return 'info'
        default:
          return ''
      }
    },
    formatDateTime(value) {
      if (!value) return '暂无'
      return new Date(value).toLocaleString('zh-CN')
    },
    getPreviewUrl(item) {
      if (!item) return ''
      const filename = item.preview || item.filePath || item.fileName || ''
      if (!filename) return ''

      const ext = filename.toLowerCase().split('.').pop()
      const imageExts = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp']
      if (ext && imageExts.includes(ext)) {
        return `/api/File/download?fileName=${encodeURIComponent(filename)}`
      }

      if (item.type !== 'image') return ''

      return `/api/File/download?fileName=${encodeURIComponent(filename)}`
    },
    getFileType(filename) {
      if (!filename) return 'other'
      const ext = (filename.split('.').pop() || '').toLowerCase()
      const imageExts = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp']
      const videoExts = ['mp4', 'avi', 'mov', 'wmv', 'flv']
      const docExts = ['pdf', 'doc', 'docx', 'txt', 'zip', 'rar']
      const modelExts = ['glb', 'gltf', 'obj', 'stl', 'fbx', 'dae', '3ds', 'ply', 'wrl', 'iges', 'igs', 'step', 'stp']

      if (imageExts.includes(ext)) return 'image'
      if (videoExts.includes(ext)) return 'video'
      if (docExts.includes(ext)) return 'document'
      if (modelExts.includes(ext)) return 'model'
      return 'other'
    }
  }
}
</script>

<style scoped>
.teaching-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.page-shell,
.table-shell {
  padding: 24px;
  border: 1px solid #dce6ef;
  border-radius: 20px;
  background: #ffffff;
  box-shadow: 0 14px 34px rgba(22, 54, 91, 0.08);
}

.page-shell {
  background:
    radial-gradient(circle at top left, rgba(255, 183, 77, 0.18), transparent 22%),
    linear-gradient(135deg, #fffaf1 0%, #f4f8ff 100%);
}

.page-title p {
  margin: 0 0 8px;
  color: #7f6a45;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.16em;
  text-transform: uppercase;
}

.page-title h1 {
  margin: 0;
  color: #182a3a;
  font-size: 30px;
}

.student-hint {
  display: inline-block;
  margin-top: 10px;
  color: #5f7185;
  font-size: 13px;
}

.filter-grid {
  display: grid;
  grid-template-columns: 1.3fr 1fr 1fr 1fr auto;
  gap: 12px;
  margin-top: 20px;
}

/* ========== 管理弹窗新样式 ========== */
.manage-work-dialog {
  display: flex;
  gap: 24px;
  padding: 0;
}

.manage-work-dialog :deep(.el-dialog__body) {
  padding: 0;
}

.work-preview-section {
  width: 360px;
  flex-shrink: 0;
  background: var(--bg-page, #F8F9F5);
  border-radius: var(--radius-lg, 18px);
  overflow: hidden;
}

.preview-header {
  padding: 16px 20px;
  background: var(--bg-card, #FFFFFF);
  border-bottom: 1px solid var(--border-color, #E8E2D8);
}

.preview-title {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  display: flex;
  align-items: center;
}

.section-icon {
  width: 18px;
  height: 18px;
  margin-right: 8px;
  vertical-align: middle;
  color: var(--primary, #2D8A6E);
}

.preview-content {
  height: 300px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.preview-model-wrap {
  width: 100%;
  height: 100%;
  background: #f5f5f5;
  border-radius: var(--radius-md, 12px);
  overflow: hidden;
}

.preview-model-wrap :deep(.model-card-cover) {
  width: 100%;
  height: 100%;
}

.preview-image-wrap {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  border-radius: var(--radius-md, 12px);
}

.preview-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: var(--text-secondary, #555555);
  padding: 20px;
  text-align: center;
}

.empty-icon {
  width: 48px;
  height: 48px;
  opacity: 0.5;
}

.work-info-section {
  flex: 1;
  min-width: 0;
  padding: 20px 0;
  display: flex;
  flex-direction: column;
  gap: 20px;
  max-height: 70vh;
  overflow-y: auto;
}

/* 作品信息卡片 */
.info-hero-card {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  padding: 20px;
  border: 1px solid var(--border-color, #E8E2D8);
}

.work-title {
  margin: 0 0 12px 0;
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
}

.work-author-row {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 12px;
}

.author-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary, #2D8A6E);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 15px;
}

.author-name {
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
}

.work-tags-row {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.info-tag {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 10px;
  background: var(--bg-page, #F8F9F5);
  border-radius: var(--radius-full, 9999px);
  font-size: 12px;
  color: var(--text-secondary, #555555);
}

.tag-icon {
  width: 14px;
  height: 14px;
}

/* 评语历史 */
.review-history-section {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  padding: 20px;
  border: 1px solid var(--border-color, #E8E2D8);
}

.section-heading {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 16px 0;
  display: flex;
  align-items: center;
}

.review-history-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: 200px;
  overflow-y: auto;
}

.review-card {
  background: var(--bg-page, #F8F9F5);
  border-radius: var(--radius-md, 12px);
  padding: 14px;
  border: 1px solid var(--border-color, #E8E2D8);
  transition: all var(--duration-fast, .18s);
}

.review-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-card, 0 2px 12px rgba(0, 0, 0, .08));
}

.review-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.reviewer-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.reviewer-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: var(--primary, #2D8A6E);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 12px;
}

.reviewer-details {
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.reviewer-name {
  font-weight: 600;
  font-size: 12px;
  color: var(--text-main, #1A1A1A);
}

.reviewer-role {
  font-size: 10px;
  color: var(--text-secondary, #555555);
}

.review-meta {
  display: flex;
  align-items: center;
  gap: 8px;
}

.review-type-tag {
  padding: 2px 8px;
  border-radius: var(--radius-full, 9999px);
  font-size: 10px;
  font-weight: 500;
  background: var(--primary-light, rgba(45, 138, 110, .1));
  color: var(--primary, #2D8A6E);
}

.review-time {
  font-size: 11px;
  color: var(--text-secondary, #555555);
}

.review-card-body {
  padding-left: 36px;
}

.review-text {
  margin: 0;
  font-size: 13px;
  color: var(--text-main, #1A1A1A);
  line-height: 1.5;
  white-space: pre-wrap;
}

.review-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 24px;
  color: var(--text-secondary, #555555);
  font-size: 13px;
}

.review-empty .empty-icon {
  width: 32px;
  height: 32px;
  opacity: 0.4;
}

/* 添加评语表单 */
.add-review-section {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  padding: 20px;
  border: 1px solid var(--border-color, #E8E2D8);
}

.review-form :deep(.el-form-item) {
  margin-bottom: 14px;
}

.review-form :deep(.el-form-item__label) {
  font-size: 13px;
  color: var(--text-main, #1A1A1A);
}

.review-form :deep(.el-select) {
  width: 100%;
}

.review-form :deep(.el-textarea__inner) {
  border-radius: var(--radius-md, 12px);
  padding: 10px 14px;
  font-size: 13px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--border-color, #E8E2D8);
}

.btn-cancel {
  border-radius: var(--radius-full, 9999px);
  padding: 10px 24px;
  font-weight: 500;
}

.btn-submit {
  background: var(--primary, #2D8A6E) !important;
  border-color: var(--primary, #2D8A6E) !important;
  border-radius: var(--radius-full, 9999px);
  padding: 10px 24px;
  font-weight: 500;
  transition: all var(--duration-fast, .18s);
}

.btn-submit:hover {
  background: var(--primary-dark, #257a5e) !important;
  border-color: var(--primary-dark, #257a5e) !important;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45, 138, 110, 0.3));
}

@media (max-width: 768px) {
  .manage-work-dialog {
    flex-direction: column;
  }

  .work-preview-section {
    width: 100%;
  }
}

/* ========== 旧样式保留（兼容其他部分）========== */
.pagination-wrap {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.dialog-footer {
  text-align: right;
}

@media (max-width: 980px) {
  .filter-grid {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 640px) {
  .filter-grid {
    grid-template-columns: 1fr;
  }
}
</style>