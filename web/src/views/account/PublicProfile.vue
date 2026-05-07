<template>
  <div class="public-profile-container" v-if="!loading">
    <div v-if="error" class="error-page">
      <el-empty description="个人主页不可访问" :image="emptyImage">
        <p class="error-message">{{ error }}</p>
      </el-empty>
    </div>

    <div v-else class="profile-content">
      <el-card class="profile-header-card" shadow="never">
        <div class="profile-banner">
          <div class="banner-decoration"></div>
        </div>
        <div class="profile-info-wrap">
          <div class="avatar-wrapper">
            <el-avatar :size="96" class="user-avatar" :src="userInfo.avatar" />
          </div>
          <div class="user-main-info">
            <h2 class="user-name">
              {{ userInfo.name }}
              <el-tag size="small" effect="plain" class="role-tag">
                {{ getRoleText(userInfo.role) }}
              </el-tag>
            </h2>
            <div class="user-meta">
              <span><i class="el-icon-user"></i> {{ getWorkIdLabel(userInfo.role) }}：{{ userInfo.workId }}</span>
              <el-divider direction="vertical" />
              <span><i class="el-icon-office-building"></i> {{ getDepartmentLabel(userInfo.role) }}：{{ userInfo.department }}</span>
            </div>
          </div>
        </div>
        <div v-if="userInfo.bio" class="profile-bio">
          <p>{{ userInfo.bio }}</p>
        </div>
      </el-card>

      <el-card class="profile-works-card" shadow="never">
        <div class="content-header">
          <h3 class="section-title">作品展示</h3>
          <span class="work-count">{{ userInfo.workCount }} 件作品</span>
        </div>
        <div v-if="userInfo.works && userInfo.works.length > 0" class="works-grid">
          <div
            v-for="work in userInfo.works"
            :key="work.id"
            class="work-card"
            @click="goToWorkDetail(work.id)"
          >
            <div class="work-thumbnail">
              <img v-if="work.thumbnailUrl" :src="work.thumbnailUrl" :alt="work.title" />
              <div v-else class="no-thumbnail">
                <i class="el-icon-file-text"></i>
                <span>{{ work.fileType }}</span>
              </div>
            </div>
            <div class="work-info">
              <h4 class="work-title">{{ work.title }}</h4>
              <p class="work-desc">{{ work.description }}</p>
              <span class="work-date">{{ formatDate(work.createdAt) }}</span>
            </div>
          </div>
        </div>
        <div v-else class="no-works">
          <el-empty description="暂无作品" />
        </div>
      </el-card>
    </div>
  </div>
  <div v-else class="loading-container">
    <i class="el-icon-loading loading-icon"></i>
    <span>加载中...</span>
  </div>
</template>

<script>
export default {
  name: 'PublicProfile',
  data() {
    return {
      loading: true,
      error: '',
      userInfo: {},
      emptyImage: 'https://cube.elemecdn.com/e/fd/0fc7d20532fdaf769a25683617711png.png'
    }
  },
  mounted() {
    this.loadProfile()
  },
  methods: {
    async loadProfile() {
      try {
        const userId = this.$route.params.userId
        console.log('加载用户资料，userId:', userId)
        const response = await this.$axios.get(`/api/Auth/public-profile/${userId}`)
        this.userInfo = response.data
        this.error = ''
      } catch (error) {
        console.error('加载公开资料失败:', error)
        if (error.response?.status === 403) {
          this.error = '该用户的个人主页未公开'
        } else if (error.response?.status === 404) {
          this.error = '用户不存在'
        } else {
          this.error = '加载个人主页失败'
        }
        this.userInfo = {}
      } finally {
        this.loading = false
      }
    },
    getRoleText(role) {
      const roles = {
        Admin: '管理员',
        Teacher: '教师',
        Student: '学生'
      }
      return roles[role] || role
    },
    getWorkIdLabel(role) {
      if (role === 'Teacher') return '教师编号'
      if (role === 'Student') return '学号'
      return '编号'
    },
    getDepartmentLabel(role) {
      if (role === 'Teacher') return '所属院系'
      if (role === 'Student') return '班级'
      return '部门'
    },
    formatDate(dateStr) {
      if (!dateStr) return ''
      const date = new Date(dateStr)
      return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
    },
    goToWorkDetail(workId) {
      this.$router.push(`/works/${workId}`)
    }
  }
}
</script>

<style scoped>
.public-profile-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  padding: 20px;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
}

.loading-icon {
  font-size: 48px;
  color: #165dff;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.error-page {
  padding: 60px 20px;
}

.error-message {
  color: #f53f3f;
  margin-top: 10px;
}

.profile-content {
  max-width: 900px;
  margin: 0 auto;
}

.profile-header-card {
  margin-bottom: 20px;
  border-radius: 12px;
  overflow: hidden;
}

.profile-banner {
  height: 180px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  position: relative;
}

.banner-decoration {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 30px;
  background: linear-gradient(45deg, transparent 33.33%, rgba(255,255,255,0.1) 33.33%, rgba(255,255,255,0.1) 66.66%, transparent 66.66%);
  background-size: 20px 20px;
}

.profile-info-wrap {
  display: flex;
  align-items: flex-end;
  padding: 20px;
  margin-top: -60px;
  position: relative;
}

.avatar-wrapper {
  margin-right: 24px;
}

.user-avatar {
  border: 4px solid #fff;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.user-main-info {
  flex: 1;
}

.user-name {
  font-size: 24px;
  font-weight: 600;
  margin: 0 0 8px 0;
  display: flex;
  align-items: center;
  gap: 12px;
}

.role-tag {
  font-size: 12px;
}

.user-meta {
  display: flex;
  align-items: center;
  gap: 16px;
  color: #646a73;
  font-size: 14px;
}

.profile-bio {
  padding: 0 20px 20px;
  margin-top: -10px;
}

.profile-bio p {
  margin: 0;
  color: #8f959e;
  font-size: 14px;
  line-height: 1.6;
}

.profile-works-card {
  border-radius: 12px;
}

.content-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.section-title {
  font-size: 18px;
  font-weight: 600;
  margin: 0;
}

.work-count {
  color: #8f959e;
  font-size: 14px;
}

.works-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.work-card {
  cursor: pointer;
  border-radius: 8px;
  overflow: hidden;
  background: #fff;
  box-shadow: 0 2px 8px rgba(0,0,0,0.08);
  transition: all 0.3s ease;
}

.work-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0,0,0,0.12);
}

.work-thumbnail {
  height: 160px;
  overflow: hidden;
  background: #f5f7fa;
}

.work-thumbnail img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.no-thumbnail {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #8f959e;
}

.no-thumbnail i {
  font-size: 32px;
  margin-bottom: 8px;
}

.no-thumbnail span {
  font-size: 12px;
}

.work-info {
  padding: 16px;
}

.work-title {
  margin: 0 0 8px 0;
  font-size: 15px;
  font-weight: 500;
  color: #1f2329;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.work-desc {
  margin: 0 0 10px 0;
  font-size: 13px;
  color: #646a73;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  line-height: 1.5;
}

.work-date {
  font-size: 12px;
  color: #8f959e;
}

.no-works {
  padding: 40px 0;
}
</style>
