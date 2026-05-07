<template>
  <div class="work-detail-container" v-loading="loading">
    <template v-if="!loading && work">
      <div class="back-link">
        <el-button type="text" @click="$router.go(-1)" class="go-back-btn">
          <i class="el-icon-arrow-left"></i> 返回
        </el-button>
      </div>

      <div class="detail-grid">
        <div class="detail-main">
          <div class="work-preview">
            <el-image
              :src="work.coverImage ? `/api/File/download?fileName=${encodeURIComponent(work.coverImage)}` : ''"
              fit="cover"
              class="cover-image"
              v-if="work.coverImage"
            >
              <div slot="error" class="image-slot">
                <i class="el-icon-picture-outline"></i>
              </div>
            </el-image>
            <div class="file-actions">
              <el-button type="primary" size="small" @click="downloadWork">
                <i class="el-icon-download"></i> 下载源文件
              </el-button>
            </div>
          </div>

          <div class="work-meta">
            <h1 class="work-title">{{ work.title }}</h1>
            <div class="work-tags">
              <el-tag size="small" type="info" v-if="work.category">{{ work.category }}</el-tag>
              <el-tag size="small" v-for="tag in work.tags" :key="tag" class="tag-item">{{ tag }}</el-tag>
            </div>
            <p class="work-description">{{ work.description }}</p>
          </div>
        </div>

        <div class="detail-sidebar">
          <div class="author-card">
            <el-avatar :size="48" :src="work.authorAvatar ? `/api/File/download?fileName=${encodeURIComponent(work.authorAvatar)}` : ''" class="author-avatar">
              <i class="el-icon-user-solid"></i>
            </el-avatar>
            <div class="author-info">
              <div class="author-name">{{ work.author }}</div>
              <div class="author-role">{{ translateRole(work.authorRole) }}</div>
            </div>
            <el-button 
              v-if="work.authorProfilePublic" 
              type="primary" 
              size="small" 
              @click="goToAuthorProfile"
              class="view-profile-btn"
            >
              <i class="el-icon-user"></i> 查看主页
            </el-button>
          </div>

          <div class="stat-card">
            <div class="stat-item">
              <i class="el-icon-view"></i>
              <span>{{ work.viewCount || 0 }} 次浏览</span>
            </div>
            <div class="stat-item">
              <i class="el-icon-star-off"></i>
              <span>{{ work.favoriteCount || 0 }} 人收藏</span>
            </div>
            <div class="stat-item">
              <i class="el-icon-date"></i>
              <span>{{ formatDate(work.createdAt) }}</span>
            </div>
          </div>

          <div class="action-card">
            <el-button
              :type="isFavorited ? 'warning' : 'default'"
              @click="toggleFavorite"
              :loading="favLoading"
              class="fav-btn"
            >
              <i :class="isFavorited ? 'el-icon-star-on' : 'el-icon-star-off'"></i>
              {{ isFavorited ? '已收藏' : '收藏作品' }}
            </el-button>
            <el-button type="danger" plain size="small" @click="showReportDialog = true" v-if="!isOwner">
              举报作品
            </el-button>
          </div>
        </div>
      </div>

      <el-dialog
        title="举报作品"
        :visible.sync="showReportDialog"
        width="420px"
        class="modern-dialog"
      >
        <el-form label-width="80px">
          <el-form-item label="举报原因">
            <el-input
              type="textarea"
              v-model="reportReason"
              :rows="3"
              placeholder="请描述举报原因..."
            ></el-input>
          </el-form-item>
        </el-form>
        <span slot="footer" class="dialog-footer">
          <el-button @click="showReportDialog = false">取消</el-button>
          <el-button type="primary" @click="submitReport" :loading="reportLoading">提交举报</el-button>
        </span>
      </el-dialog>
    </template>

    <template v-if="!loading && error">
      <div class="error-state">
        <i class="el-icon-warning-outline"></i>
        <p>{{ error }}</p>
        <el-button type="primary" @click="$router.push('/works/explore')">返回作品探索</el-button>
      </div>
    </template>
  </div>
</template>

<script>
export default {
  name: 'WorkDetail',
  data() {
    return {
      work: null,
      loading: true,
      error: '',
      isFavorited: false,
      favLoading: false,
      showReportDialog: false,
      reportReason: '',
      reportLoading: false
    }
  },
  computed: {
    workId() {
      return this.$route.params.id
    },
    isOwner() {
      const userInfo = JSON.parse(localStorage.getItem('userInfo') || '{}')
      return userInfo.username === this.work?.author
    }
  },
  methods: {
    async fetchWorkDetail() {
      this.loading = true
      this.error = ''
      try {
        const res = await this.$http.get(`/api/Work/${this.workId}`)
        const data = res.data
        // 映射后端返回的字段
        this.work = {
          ...data,
          author: data.uploadUserName || data.author,
          authorRole: data.uploadUserRole || data.authorRole,
          authorProfilePublic: data.uploadUserProfilePublic || data.authorProfilePublic,
          authorAvatar: data.uploadUserAvatar || data.authorAvatar,
          userId: data.UserId || data.userId,
          coverImage: data.PreviewImage || data.coverImage,
          createdAt: data.UploadDate || data.createdAt,
          fileName: data.FileName || data.fileName
        }

        // 增加浏览量
        await this.$http.post(`/api/Work/${this.workId}/view`).catch(() => {})

        // 检查收藏状态
        this.checkFavoriteStatus()
      } catch (err) {
        this.error = '作品不存在或已被删除'
      } finally {
        this.loading = false
      }
    },
    goToAuthorProfile() {
      this.$router.push(`/profile/${this.work.userId}`)
    },
    async checkFavoriteStatus() {
      try {
        const res = await this.$http.get(`/api/Collection/check/${this.workId}`)
        this.isFavorited = res.data?.isFavorited || false
      } catch {
        // ignore
      }
    },
    async toggleFavorite() {
      this.favLoading = true
      try {
        const res = await this.$http.post(`/api/Collection/toggle/${this.workId}`)
        this.isFavorited = res.data?.isFavorited || !this.isFavorited
        this.$message.success(this.isFavorited ? '已收藏' : '已取消收藏')
      } catch {
        this.$message.error('操作失败，请重试')
      } finally {
        this.favLoading = false
      }
    },
    downloadWork() {
      if (this.work.fileName) {
        window.open(`/api/File/download?fileName=${encodeURIComponent(this.work.fileName)}`, '_blank')
      } else {
        this.$message.warning('该作品暂无源文件')
      }
    },
    async submitReport() {
      if (!this.reportReason.trim()) {
        this.$message.warning('请填写举报原因')
        return
      }
      this.reportLoading = true
      try {
        await this.$http.post('/api/ContentModeration/report', {
          workId: this.workId,
          reason: this.reportReason
        })
        this.$message.success('举报已提交，我们会尽快处理')
        this.showReportDialog = false
        this.reportReason = ''
      } catch {
        this.$message.error('举报提交失败，请重试')
      } finally {
        this.reportLoading = false
      }
    },
    translateRole(role) {
      const map = { Admin: '管理员', Teacher: '教师', Student: '学生' }
      return map[role] || role
    },
    formatDate(dateStr) {
      if (!dateStr) return ''
      const d = new Date(dateStr)
      return d.toLocaleDateString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' })
    }
  },
  mounted() {
    this.fetchWorkDetail()
  },
  watch: {
    '$route.params.id'() {
      this.fetchWorkDetail()
    }
  }
}
</script>

<style scoped>
.work-detail-container {
  max-width: 1100px;
  margin: 0 auto;
  padding: 0 16px;
}

.back-link {
  margin-bottom: 20px;
}

.go-back-btn {
  font-size: 14px;
  color: #4e5969;
}

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 24px;
}

.detail-main {
  background: #ffffff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  border: 1px solid #e5e6eb;
}

.cover-image {
  width: 100%;
  height: 400px;
  display: block;
}

.image-slot {
  width: 100%;
  height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f7fa;
  font-size: 48px;
  color: #c0c4cc;
}

.file-actions {
  padding: 16px 24px;
  border-bottom: 1px solid #f0f0f0;
}

.work-meta {
  padding: 24px;
}

.work-title {
  font-size: 24px;
  font-weight: 600;
  color: #1d2129;
  margin: 0 0 16px 0;
}

.work-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 20px;
}

.tag-item {
  margin-left: 0;
}

.work-description {
  font-size: 14px;
  color: #4e5969;
  line-height: 1.8;
  white-space: pre-wrap;
  margin: 0;
}

.detail-sidebar {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.author-card,
.stat-card,
.action-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  border: 1px solid #e5e6eb;
}

.author-card {
  display: flex;
  align-items: center;
  gap: 14px;
}

.author-avatar {
  background-color: #165dff;
}

.author-info {
  flex: 1;
}

.author-name {
  font-size: 15px;
  font-weight: 600;
  color: #1d2129;
}

.author-role {
  font-size: 12px;
  color: #86909c;
  margin-top: 4px;
}

.view-profile-btn {
  margin-left: auto;
}

.stat-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #4e5969;
}

.stat-item i {
  color: #165dff;
  font-size: 16px;
}

.action-card {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.fav-btn {
  width: 100%;
}

.error-state {
  text-align: center;
  padding: 80px 24px;
  color: #86909c;
  font-size: 16px;
}

.error-state i {
  font-size: 56px;
  display: block;
  margin-bottom: 16px;
  color: #c0c4cc;
}
</style>
