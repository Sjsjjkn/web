<template>
  <div class="work-detail-page" v-loading="loading">
    <div class="work-detail-container" v-if="work && !loading">
      <!-- 面包屑导航 -->
      <div class="breadcrumb">
        <div class="back-btn" @click="$router.go(-1)">
          <i class="el-icon-arrow-left"></i>
          <span>返回</span>
        </div>
        <el-breadcrumb separator-class="el-icon-arrow-right">
          <el-breadcrumb-item :to="{ path: '/works/explore' }">作品展示</el-breadcrumb-item>
          <el-breadcrumb-item>{{ work.title }}</el-breadcrumb-item>
        </el-breadcrumb>
      </div>

      <!-- Hero 区域 -->
      <div class="hero-section">
        <div class="hero-content">
          <div class="hero-badge">
            <el-tag size="small" effect="dark" class="status-tag">已发布</el-tag>
            <span class="hero-category" v-if="work.category">{{ work.category }}</span>
          </div>
          <h1 class="hero-title">{{ work.title }}</h1>
          <div class="hero-author">
            <el-avatar :size="48" :src="work.authorAvatar ? `/api/File/download?fileName=${encodeURIComponent(work.authorAvatar)}` : ''" class="hero-avatar">
              <i class="el-icon-user-solid"></i>
            </el-avatar>
            <div class="hero-author-info">
              <span class="hero-author-name">{{ work.author || '匿名用户' }}</span>
              <span class="hero-author-role">{{ translateRole(work.authorRole) }}</span>
            </div>
          </div>
          <div class="hero-stats">
            <div class="hero-stat-item">
              <div class="stat-icon-ring">
                <i class="el-icon-view"></i>
              </div>
              <div class="stat-info">
                <span class="stat-number">{{ work.views || 0 }}</span>
                <span class="stat-unit">次浏览</span>
              </div>
            </div>
            <div class="hero-stat-divider"></div>
            <div class="hero-stat-item">
              <div class="stat-icon-ring">
                <i class="el-icon-star-off"></i>
              </div>
              <div class="stat-info">
                <span class="stat-number">{{ work.favoriteCount || 0 }}</span>
                <span class="stat-unit">人收藏</span>
              </div>
            </div>
            <div class="hero-stat-divider"></div>
            <div class="hero-stat-item">
              <div class="stat-icon-ring">
                <i class="el-icon-date"></i>
              </div>
              <div class="stat-info">
                <span class="stat-number">{{ formatDate(work.createdAt) }}</span>
                <span class="stat-unit">发布</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 主内容区：两栏布局 -->
      <div class="detail-body">
        <!-- 左栏：预览 + 描述 -->
        <div class="detail-main">
          <!-- 作品预览 -->
          <div class="preview-card glass-card">
            <div class="card-header">
              <div class="card-header-icon">
                <i class="el-icon-picture-outline"></i>
              </div>
              <span>作品预览</span>
            </div>
            <div class="preview-container">
              <el-image
                :src="work.coverImage ? `/api/File/download?fileName=${encodeURIComponent(work.coverImage)}` : ''"
                fit="cover"
                class="preview-image"
                v-if="work.coverImage"
              >
                <div slot="error" class="preview-placeholder">
                  <div class="placeholder-icon">
                    <i class="el-icon-picture-outline"></i>
                  </div>
                  <span>暂无预览图</span>
                </div>
              </el-image>
              <div v-else class="preview-placeholder">
                <div class="placeholder-icon">
                  <i class="el-icon-picture-outline"></i>
                </div>
                <span>暂无预览图</span>
              </div>
            </div>
            <div class="file-action-bar">
              <div class="file-meta-info">
                <span class="file-meta-item" v-if="work.fileName">
                  <i class="el-icon-document"></i>
                  {{ work.fileName }}
                </span>
                <span class="file-meta-item" v-if="work.fileSize">
                  <i class="el-icon-s-data"></i>
                  {{ formatFileSize(work.fileSize) }}
                </span>
              </div>
              <el-button
                type="primary"
                size="small"
                round
                icon="el-icon-download"
                @click="downloadWork"
                class="download-btn"
              >
                下载源文件
              </el-button>
            </div>
          </div>

          <!-- 操作按钮 -->
          <div class="action-bar">
            <el-button
              :type="isFavorited ? 'warning' : 'default'"
              :icon="isFavorited ? 'el-icon-star-on' : 'el-icon-star-off'"
              @click="toggleFavorite"
              :loading="favLoading"
              round
              :class="['fav-btn', { 'is-fav': isFavorited }]"
            >
              {{ isFavorited ? '已收藏' : '收藏作品' }}
            </el-button>
            <el-button
              v-if="!isOwner"
              type="danger"
              plain
              round
              icon="el-icon-warning-outline"
              @click="showReportDialog = true"
              class="report-btn"
            >
              举报作品
            </el-button>
            <el-button
              v-if="work.authorProfilePublic"
              type="primary"
              plain
              round
              icon="el-icon-user"
              @click="goToAuthorProfile"
              class="profile-btn"
            >
              查看作者主页
            </el-button>
          </div>

          <!-- 作品描述 -->
          <div class="description-card glass-card">
            <div class="card-header">
              <div class="card-header-icon">
                <i class="el-icon-document-copy"></i>
              </div>
              <span>作品描述</span>
            </div>
            <div class="description-content">
              <p v-if="work.description">{{ work.description }}</p>
              <div v-else class="description-empty">
                <i class="el-icon-edit-outline"></i>
                <span>作者还没有添加作品描述</span>
              </div>
            </div>
          </div>

          <!-- 标签 -->
          <div class="tags-card glass-card" v-if="work.tags && work.tags.length > 0">
            <div class="card-header">
              <div class="card-header-icon">
                <i class="el-icon-collection-tag"></i>
              </div>
              <span>作品标签</span>
            </div>
            <div class="tags-list">
              <el-tag
                v-for="tag in work.tags"
                :key="tag"
                size="small"
                class="work-tag"
                effect="plain"
              >{{ tag }}</el-tag>
            </div>
          </div>
        </div>

        <!-- 右栏：作者信息 -->
        <div class="detail-sidebar">
          <div class="author-card glass-card">
            <div class="author-card-bg"></div>
            <el-avatar :size="72" :src="work.authorAvatar ? `/api/File/download?fileName=${encodeURIComponent(work.authorAvatar)}` : ''" class="author-avatar-large">
              <i class="el-icon-user-solid"></i>
            </el-avatar>
            <h3 class="author-card-name">{{ work.author || '匿名用户' }}</h3>
            <span class="author-card-role">{{ translateRole(work.authorRole) }}</span>
            <div class="author-card-divider"></div>
            <div class="author-card-stats">
              <div class="author-stat">
                <span class="author-stat-num">{{ work.views || 0 }}</span>
                <span class="author-stat-label">作品浏览</span>
              </div>
              <div class="author-stat">
                <span class="author-stat-num">{{ work.favoriteCount || 0 }}</span>
                <span class="author-stat-label">被收藏</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 举报对话框 -->
      <el-dialog
        title="举报作品"
        :visible.sync="showReportDialog"
        width="440px"
        class="report-dialog"
        :close-on-click-modal="false"
      >
        <div class="report-form">
          <div class="report-icon">
            <i class="el-icon-warning-outline"></i>
          </div>
          <p class="report-desc">请描述您举报该作品的原因，我们会尽快审核处理。</p>
          <el-form label-width="0">
            <el-form-item>
              <el-input
                v-model="reportReason"
                type="textarea"
                :rows="4"
                placeholder="请详细描述举报原因..."
                class="report-textarea"
              ></el-input>
            </el-form-item>
          </el-form>
        </div>
        <span slot="footer" class="dialog-footer">
          <el-button @click="showReportDialog = false" round>取消</el-button>
          <el-button type="primary" @click="submitReport" round>提交举报</el-button>
        </span>
      </el-dialog>
    </div>

    <el-empty v-else-if="!loading" description="作品不存在" class="empty-state"></el-empty>
  </div>
</template>

<script>
import http from '../../utils/http'
import { getUser } from '../../utils/auth'

export default {
  name: 'WorkDetail',
  data() {
    return {
      work: null,
      loading: false,
      isFavorited: false,
      favLoading: false,
      showReportDialog: false,
      reportReason: '',
    }
  },
  computed: {
    workId() {
      return this.$route.params.id
    },
    isOwner() {
      const user = getUser()
      return user && this.work && user.id === this.work.userId
    }
  },
  mounted() {
    this.loadDetail()
  },
  methods: {
    async loadDetail() {
      this.loading = true
      try {
        const { data } = await http.get(`/api/Work/${this.workId}`)
        this.work = data
        await this.checkFavorite()
      } catch (e) {
        this.$message.error('加载失败')
      } finally {
        this.loading = false
      }
    },
    async checkFavorite() {
      try {
        const { data } = await http.get(`/api/Work/${this.workId}/is-favorite`)
        this.isFavorited = data.isFavorite
      } catch (e) { /* silent */ }
    },
    async toggleFavorite() {
      this.favLoading = true
      try {
        if (this.isFavorited) {
          await http.delete(`/api/Work/${this.workId}/favorite`)
          this.isFavorited = false
          this.work.favoriteCount = Math.max(0, (this.work.favoriteCount || 0) - 1)
          this.$message.success('已取消收藏')
        } else {
          await http.post(`/api/Work/${this.workId}/favorite`)
          this.isFavorited = true
          this.work.favoriteCount = (this.work.favoriteCount || 0) + 1
          this.$message.success('已收藏')
        }
      } catch (e) {
        this.$message.error('操作失败')
      } finally {
        this.favLoading = false
      }
    },
    downloadWork() {
      if (!this.work.filePath) {
        this.$message.error('无文件可下载')
        return
      }
      http.get('/api/File/download', { params: { fileName: this.work.filePath }, responseType: 'blob' })
        .then(res => {
          const url = URL.createObjectURL(res.data)
          const a = document.createElement('a')
          a.href = url
          a.download = this.work.fileName || 'work'
          a.click()
          URL.revokeObjectURL(url)
        })
        .catch(() => {
          this.$message.error('下载失败')
        })
    },
    goToAuthorProfile() {
      this.$router.push(`/profile/${this.work.userId}`)
    },
    formatDate(date) {
      if (!date) return ''
      return new Date(date).toLocaleDateString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' })
    },
    formatFileSize(bytes) {
      if (!bytes) return ''
      const units = ['B', 'KB', 'MB', 'GB']
      let i = 0
      let size = bytes
      while (size >= 1024 && i < units.length - 1) { size /= 1024; i++ }
      return size.toFixed(i === 0 ? 0 : 1) + ' ' + units[i]
    },
    translateRole(role) {
      const map = { Student: '学生', Teacher: '教师', Admin: '管理员' }
      return map[role] || role || ''
    },
    async submitReport() {
      if (!this.reportReason.trim()) {
        this.$message.warning('请输入举报原因')
        return
      }
      try {
        await http.post(`/api/Work/${this.workId}/report`, { reason: this.reportReason })
        this.$message.success('举报已提交')
        this.showReportDialog = false
        this.reportReason = ''
      } catch (e) {
        this.$message.error('举报失败')
      }
    }
  }
}
</script>

<style scoped>
/* ================= 全局容器 ================= */
.work-detail-page {
  min-height: 100vh;
  background: var(--bg-page);
  position: relative;
  overflow: hidden;
}

/* ── 环境光晕 ── */


/* ================= 面包屑 ================= */
.work-detail-container {
  position: relative;
  z-index: 1;
  max-width: 1200px;
  margin: 0 auto;
  padding: 28px 24px 80px;
}

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
}

.back-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  color: var(--text-light);
  cursor: pointer;
  padding: 6px 14px;
  border-radius: var(--radius-full);
  background: var(--bg-card);
  border: 1px solid var(--border-light);
  transition: all var(--duration-normal) var(--ease-out-expo);
  font-weight: 500;
}

.back-btn:hover {
  color: var(--primary);
  border-color: var(--primary);
  background: var(--primary-bg);
  transform: translateX(-2px);
}

/* ================= Hero 区域 ================= */
.hero-section {
  background: var(--bg-card);
  border-radius: var(--radius-xl);
  padding: 32px;
  margin-bottom: 24px;
  border: 1px solid var(--border-light);
}

.hero-content {
  position: relative;
  z-index: 1;
}

.hero-badge {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
}

.status-tag {
  background: var(--primary) !important;
  border-color: var(--primary) !important;
  font-weight: 600;
  letter-spacing: 0.5px;
  border-radius: var(--radius-xs);
}

.hero-category {
  font-size: 13px;
  color: var(--primary);
  background: rgba(45, 138, 110, 0.08);
  padding: 4px 14px;
  border-radius: var(--radius-full);
  font-weight: 600;
}

.hero-title {
  font-size: 36px;
  font-weight: 800;
  color: var(--text-main);
  margin: 0 0 28px;
  line-height: 1.25;
  letter-spacing: -0.5px;
}

.hero-author {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 28px;
}

.hero-avatar {
  border: 3px solid rgba(45, 138, 110, 0.2);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
}

.hero-author-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.hero-author-name {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-main);
}

.hero-author-role {
  font-size: 13px;
  color: var(--text-light);
}

.hero-stats {
  display: flex;
  align-items: center;
  gap: 24px;
  padding: 20px 0 0;
  border-top: 1px solid rgba(0, 0, 0, 0.05);
}

.hero-stat-item {
  display: flex;
  align-items: center;
  gap: 12px;
}

.stat-icon-ring {
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

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-number {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main);
  line-height: 1.2;
}

.stat-unit {
  font-size: 12px;
  color: var(--text-light);
}

.hero-stat-divider {
  width: 1px;
  height: 32px;
  background: rgba(0, 0, 0, 0.06);
  flex-shrink: 0;
}

/* ================= 主内容两栏 ================= */
.detail-body {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 28px;
  align-items: start;
}

.detail-main {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* ── 玻璃卡片 ── */
.glass-card {
  background: var(--bg-glass);
  backdrop-filter: blur(16px) saturate(180%);
  -webkit-backdrop-filter: blur(16px) saturate(180%);
  border-radius: var(--radius-xl);
  border: 1px solid rgba(232, 226, 216, 0.6);
  box-shadow: var(--shadow-sm);
  padding: 24px;
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.glass-card:hover {
  box-shadow: var(--shadow-card);
  border-color: rgba(45, 138, 110, 0.15);
}

.card-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 20px;
  font-size: 15px;
  font-weight: 700;
  color: var(--text-main);
}

.card-header-icon {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-sm);
  background: linear-gradient(135deg, var(--primary-bg), rgba(45, 138, 110, 0.1));
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary);
  font-size: 14px;
}

/* ── 预览卡片 ── */
.preview-card {
  padding: 0;
  overflow: hidden;
}

.preview-card .card-header {
  padding: 20px 24px 0;
}

.preview-container {
  padding: 0 24px;
}

.preview-image {
  width: 100%;
  height: 360px;
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--bg-hover);
}

.preview-image ::v-deep .el-image__inner {
  object-fit: cover;
}

.preview-placeholder {
  width: 100%;
  height: 360px;
  background: linear-gradient(135deg, #F5F2EC 0%, #EDE8DD 100%);
  border-radius: var(--radius-md);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: var(--text-placeholder);
}

.placeholder-icon {
  width: 72px;
  height: 72px;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.04);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
}

.file-action-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px 20px;
  border-top: 1px solid var(--border-light);
  margin-top: 16px;
}

.file-meta-info {
  display: flex;
  align-items: center;
  gap: 16px;
  flex: 1;
  min-width: 0;
}

.file-meta-item {
  font-size: 13px;
  color: var(--text-secondary);
  display: flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.file-meta-item i {
  color: var(--primary);
  font-size: 14px;
}

.download-btn {
  flex-shrink: 0;
  font-weight: 600;
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  border: none;
  box-shadow: 0 4px 14px rgba(45, 138, 110, 0.3);
  padding: 10px 24px;
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.download-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(45, 138, 110, 0.4);
}

.download-btn ::v-deep span {
  display: flex;
  align-items: center;
  gap: 6px;
}

/* ── 操作栏 ── */
.action-bar {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.fav-btn {
  font-weight: 600;
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.fav-btn.is-fav {
  background: linear-gradient(135deg, #F09342, #FFB74D);
  border-color: transparent;
  color: white;
}

.fav-btn.is-fav:hover {
  background: linear-gradient(135deg, #E08532, #F0A835);
}

.report-btn {
  font-weight: 500;
}

.profile-btn {
  font-weight: 500;
}

/* ── 描述卡片 ── */
.description-content {
  font-size: 15px;
  line-height: 1.8;
  color: var(--text-regular);
  white-space: pre-wrap;
}

.description-content p {
  margin: 0;
}

.description-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 32px 0;
  color: var(--text-placeholder);
  font-size: 14px;
}

.description-empty i {
  font-size: 32px;
  opacity: 0.5;
}

/* ── 标签卡片 ── */
.tags-list {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.work-tag {
  background: var(--primary-bg) !important;
  border-color: rgba(45, 138, 110, 0.15) !important;
  color: var(--primary) !important;
  font-weight: 500;
  border-radius: var(--radius-full);
  padding: 6px 16px;
}

/* ================= 侧边栏 ================= */
.detail-sidebar {
  position: sticky;
  top: 24px;
}

.author-card {
  position: relative;
  overflow: hidden;
  text-align: center;
  padding: 32px 24px 24px;
}

.author-card-bg {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 100px;
  background: linear-gradient(135deg, var(--primary-bg) 0%, rgba(200, 170, 110, 0.1) 100%);
  pointer-events: none;
}

.author-avatar-large {
  position: relative;
  z-index: 1;
  border: 4px solid white;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
  margin-bottom: 12px;
}

.author-card-name {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 4px;
}

.author-card-role {
  font-size: 13px;
  color: var(--text-light);
  background: var(--primary-bg);
  padding: 3px 14px;
  border-radius: var(--radius-full);
  display: inline-block;
  font-weight: 500;
}

.author-card-divider {
  height: 1px;
  background: var(--border-light);
  margin: 20px 0;
}

.author-card-stats {
  display: flex;
  justify-content: center;
  gap: 32px;
}

.author-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.author-stat-num {
  font-size: 22px;
  font-weight: 700;
  color: var(--primary);
}

.author-stat-label {
  font-size: 12px;
  color: var(--text-light);
}

/* ================= 举报对话框 ================= */
::v-deep .report-dialog {
  border-radius: var(--radius-xl) !important;
  overflow: hidden;
}

::v-deep .report-dialog .el-dialog__header {
  padding: 28px 32px 0;
  font-weight: 700;
  font-size: 18px;
  color: var(--text-main);
}

::v-deep .report-dialog .el-dialog__body {
  padding: 16px 32px;
}

::v-deep .report-dialog .el-dialog__footer {
  padding: 16px 32px 28px;
}

.report-form {
  text-align: center;
}

.report-icon {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: linear-gradient(135deg, #FEF0F0, #FDE8E8);
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  font-size: 26px;
  color: var(--danger);
}

.report-desc {
  font-size: 14px;
  color: var(--text-secondary);
  line-height: 1.6;
  margin: 0 0 20px;
}

.report-textarea ::v-deep .el-textarea__inner {
  border-radius: var(--radius-md);
  background: var(--bg-page);
  border-color: var(--border-color);
  font-size: 14px;
  resize: none;
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.report-textarea ::v-deep .el-textarea__inner:focus {
  border-color: var(--danger);
  box-shadow: 0 0 0 3px rgba(224, 85, 85, 0.1);
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.dialog-footer .el-button {
  font-weight: 500;
}

/* ================= 空状态 ================= */
.empty-state {
  padding: 120px 0;
  position: relative;
  z-index: 1;
}

/* ================= 动画 ================= */
.fade-in-up {
  animation: fadeInUp 0.6s var(--ease-out-expo) both;
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(24px); }
  to { opacity: 1; transform: translateY(0); }
}

/* ================= 响应式 ================= */
@media (max-width: 900px) {
  .detail-body {
    grid-template-columns: 1fr;
  }

  .detail-sidebar {
    position: static;
  }

  .hero-section {
    padding: 32px 24px 28px;
  }

  .hero-title {
    font-size: 26px;
  }

  .hero-stats {
    flex-wrap: wrap;
    gap: 16px;
  }

  .hero-stat-divider {
    display: none;
  }

  .preview-image,
  .preview-placeholder {
    height: 240px;
  }

  .file-action-bar {
    flex-direction: column;
    gap: 12px;
  }

  .file-meta-info {
    flex-wrap: wrap;
  }
}

@media (max-width: 480px) {
  .work-detail-container {
    padding: 16px 12px 60px;
  }

  .hero-section {
    padding: 24px 16px 20px;
    border-radius: var(--radius-lg);
  }

  .hero-title {
    font-size: 22px;
  }

  .hero-author {
    gap: 12px;
  }

  .action-bar {
    flex-direction: column;
  }

  .action-bar .el-button {
    width: 100%;
  }
}
</style>