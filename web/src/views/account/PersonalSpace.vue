<template>
  <div class="personal-space-container">
    <!-- 背景装饰 -->
    <div class="space-bg">
      <div class="bg-shape bg-shape-1"></div>
      <div class="bg-shape bg-shape-2"></div>
    </div>

    <div class="space-content" v-loading="pageLoading">
      <!-- 用户信息卡片 -->
      <div class="profile-card glass-card">
        <div class="profile-header">
          <div class="avatar-section">
            <el-avatar :size="80" :src="getAvatarUrl()" icon="el-icon-user-solid" class="profile-avatar"></el-avatar>
            <div class="avatar-overlay" @click="showAvatarUpload = true">
              <i class="el-icon-camera"></i>
            </div>
          </div>
          <div class="profile-info">
            <h2 class="profile-name">{{ userInfo.name || userInfo.username }}</h2>
            <p class="profile-role">
              <el-tag :type="roleTagType" size="small">{{ translateRole(userInfo.role) }}</el-tag>
            </p>
            <p class="profile-email" v-if="userInfo.email">
              <i class="el-icon-message"></i> {{ userInfo.email }}
            </p>
            <p class="profile-phone" v-if="userInfo.phone">
              <i class="el-icon-phone"></i> {{ maskPhone(userInfo.phone) }}
            </p>
          </div>
        </div>
        <div class="profile-stats">
          <div class="stat-item" @click="$router.push('/account/space?tab=works')">
            <span class="stat-value">{{ stats.workCount }}</span>
            <span class="stat-label">作品</span>
          </div>
          <div class="stat-item" @click="$router.push('/account/space?tab=favorites')">
            <span class="stat-value">{{ stats.favoriteCount }}</span>
            <span class="stat-label">收藏</span>
          </div>
          <div class="stat-item">
            <span class="stat-value">{{ stats.viewCount }}</span>
            <span class="stat-label">浏览</span>
          </div>
        </div>
      </div>

      <!-- 功能标签页 -->
      <div class="content-card glass-card">
        <el-tabs v-model="activeTab" @tab-click="handleTabClick">
          <el-tab-pane label="我的作品" name="works">
            <div class="tab-actions" v-if="activeTab === 'works'">
              <el-input
                v-model="workSearch"
                placeholder="搜索作品..."
                prefix-icon="el-icon-search"
                size="small"
                clearable
                class="search-input"
              ></el-input>
              <el-button type="primary" size="small" icon="el-icon-upload" @click="$router.push('/upload')">上传作品</el-button>
            </div>
            <div class="work-grid" v-if="filteredWorks.length > 0">
              <div
                class="work-card"
                v-for="work in filteredWorks"
                :key="work.id"
                @click="$router.push(`/works/${work.id}`)"
              >
                <div class="work-cover">
                  <img v-if="getWorkThumb(work)" :src="getWorkThumb(work)" :alt="work.title" />
                  <div v-else class="work-placeholder">
                    <i :class="getWorkIcon(work.fileType)"></i>
                  </div>
                  <div class="work-overlay">
                    <el-button type="primary" size="mini" icon="el-icon-view" circle @click.stop="$router.push(`/works/${work.id}`)"></el-button>
                    <el-button type="danger" size="mini" icon="el-icon-delete" circle @click.stop="deleteWork(work)"></el-button>
                  </div>
                  <span class="work-type-badge">{{ getFileTypeLabel(work.fileType) }}</span>
                </div>
                <div class="work-info">
                  <h4 class="work-title">{{ work.title }}</h4>
                  <p class="work-desc">{{ work.description || '暂无描述' }}</p>
                  <div class="work-meta">
                    <span><i class="el-icon-view"></i> {{ work.viewCount || 0 }}</span>
                    <span><i class="el-icon-star-off"></i> {{ work.favoriteCount || 0 }}</span>
                    <span><i class="el-icon-time"></i> {{ formatDate(work.createdAt) }}</span>
                  </div>
                </div>
              </div>
            </div>
            <el-empty v-else description="还没有作品，快去上传吧！"></el-empty>
          </el-tab-pane>

          <el-tab-pane label="我的收藏" name="favorites">
            <div class="work-grid" v-if="favoriteWorks.length > 0">
              <div
                class="work-card"
                v-for="fw in favoriteWorks"
                :key="fw.id"
                @click="$router.push(`/works/${fw.workId}`)"
              >
                <div class="work-cover">
                  <img v-if="getWorkThumb(fw)" :src="getWorkThumb(fw)" :alt="fw.title" />
                  <div v-else class="work-placeholder">
                    <i :class="getWorkIcon(fw.fileType)"></i>
                  </div>
                  <div class="work-overlay">
                    <el-button type="warning" size="mini" icon="el-icon-star-off" circle @click.stop="unfavoriteWork(fw)"></el-button>
                  </div>
                </div>
                <div class="work-info">
                  <h4 class="work-title">{{ fw.title }}</h4>
                  <p class="work-desc">{{ fw.description || '暂无描述' }}</p>
                  <div class="work-meta">
                    <span><i class="el-icon-user"></i> {{ fw.authorName }}</span>
                    <span><i class="el-icon-time"></i> {{ formatDate(fw.favoritedAt) }}</span>
                  </div>
                </div>
              </div>
            </div>
            <el-empty v-else description="还没有收藏任何作品"></el-empty>
          </el-tab-pane>

          <el-tab-pane label="浏览历史" name="history">
            <div class="work-grid" v-if="historyWorks.length > 0">
              <div
                class="work-card"
                v-for="hw in historyWorks"
                :key="hw.id"
                @click="$router.push(`/works/${hw.workId}`)"
              >
                <div class="work-cover">
                  <img v-if="getWorkThumb(hw)" :src="getWorkThumb(hw)" :alt="hw.title" />
                  <div v-else class="work-placeholder">
                    <i :class="getWorkIcon(hw.fileType)"></i>
                  </div>
                </div>
                <div class="work-info">
                  <h4 class="work-title">{{ hw.title }}</h4>
                  <p class="work-desc">{{ hw.description || '暂无描述' }}</p>
                  <div class="work-meta">
                    <span><i class="el-icon-user"></i> {{ hw.authorName }}</span>
                    <span><i class="el-icon-time"></i> {{ formatDate(hw.viewedAt) }}</span>
                  </div>
                </div>
              </div>
            </div>
            <el-empty v-else description="还没有浏览记录"></el-empty>
          </el-tab-pane>

          <el-tab-pane label="个人设置" name="settings">
            <div class="settings-container">
              <el-card class="settings-card" header="基本信息">
                <el-form :model="editForm" label-width="120px" :disabled="saving">
                  <el-form-item label="用户名">
                    <el-input v-model="editForm.username" disabled />
                  </el-form-item>
                  <el-form-item label="姓名">
                    <el-input v-model="editForm.name" placeholder="请输入姓名" disabled />
                  </el-form-item>
                  <el-form-item label="学号/工号">
                    <el-input v-model="editForm.workId" placeholder="请输入学号或工号" disabled />
                  </el-form-item>
                  <el-form-item label="院系/部门">
                    <el-input v-model="editForm.department" placeholder="请输入院系或部门" disabled />
                  </el-form-item>
                  <el-form-item label="邮箱">
                    <el-input v-model="editForm.email" placeholder="请输入邮箱" />
                  </el-form-item>
                  <el-form-item label="手机号">
                    <el-input v-model="editForm.phone" placeholder="请输入手机号" />
                  </el-form-item>
                  <el-form-item label="个人简介">
                    <el-input v-model="editForm.bio" type="textarea" rows="3" placeholder="请输入个人简介" />
                  </el-form-item>
                  <el-form-item>
                    <el-button type="primary" @click="saveProfile" :loading="saving">保存修改</el-button>
                  </el-form-item>
                </el-form>
              </el-card>

              <el-card class="settings-card" header="修改密码">
                <el-form :model="passwordForm" label-width="120px" :disabled="savingPassword">
                  <el-form-item label="当前密码">
                    <el-input v-model="passwordForm.currentPassword" type="password" placeholder="请输入当前密码" />
                  </el-form-item>
                  <el-form-item label="新密码">
                    <el-input v-model="passwordForm.newPassword" type="password" placeholder="请输入新密码（6-20位）" />
                  </el-form-item>
                  <el-form-item label="确认密码">
                    <el-input v-model="passwordForm.confirmPassword" type="password" placeholder="请再次输入新密码" />
                  </el-form-item>
                  <el-form-item>
                    <el-button type="primary" @click="changePassword" :loading="savingPassword">修改密码</el-button>
                  </el-form-item>
                </el-form>
              </el-card>
            </div>
          </el-tab-pane>
        </el-tabs>
      </div>
    </div>

    <!-- 头像上传对话框 -->
    <el-dialog title="上传头像" :visible.sync="showAvatarUpload" width="420px" :close-on-click-modal="false" append-to-body :modal-append-to-body="true">
      <div class="avatar-upload-section">
        <div class="avatar-preview">
          <el-avatar :size="120" :src="avatarPreviewUrl" icon="el-icon-user-solid"></el-avatar>
        </div>
        <el-upload
          class="avatar-uploader"
          action=""
          :auto-upload="false"
          :show-file-list="false"
          :on-change="handleAvatarChange"
          accept="image/*"
        >
          <el-button type="primary" :loading="avatarUploading">
            {{ avatarUploading ? '上传中...' : '选择图片' }}
          </el-button>
        </el-upload>
        <p class="upload-tip">支持 JPG、PNG 格式，大小不超过 2MB</p>
      </div>
      <span slot="footer" class="dialog-footer">
        <el-button @click="showAvatarUpload = false">取消</el-button>
        <el-button type="primary" @click="uploadAvatar" :loading="avatarUploading" :disabled="!avatarFile">确认上传</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import http from '../../utils/http'
import eventBus from '../../utils/eventBus'
import { getWorkIcon, getFileTypeLabel } from '../../utils/fileUtils'

export default {
  name: 'PersonalSpace',
  data() {
    return {
      pageLoading: false,
      activeTab: 'works',
      userInfo: {
        id: '',
        username: '',
        name: '',
        role: '',
        email: '',
        phone: '',
        avatar: ''
      },
      stats: {
        workCount: 0,
        favoriteCount: 0,
        viewCount: 0
      },
      works: [],
      favoriteWorks: [],
      historyWorks: [],
      workSearch: '',
      showAvatarUpload: false,
      avatarFile: null,
      avatarPreviewUrl: '',
      avatarUploading: false,
      editForm: {
        username: '',
        name: '',
        workId: '',
        department: '',
        email: '',
        phone: '',
        bio: ''
      },
      passwordForm: {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      },
      saving: false,
      savingPassword: false
    }
  },
  computed: {
    roleTagType() {
      const map = { Admin: 'danger', Teacher: 'warning', Student: 'success', Guest: 'info' }
      return map[this.userInfo.role] || 'info'
    },
    filteredWorks() {
      if (!this.workSearch) return this.works
      const keyword = this.workSearch.toLowerCase()
      return this.works.filter(w => w.title?.toLowerCase().includes(keyword) || w.description?.toLowerCase().includes(keyword))
    }
  },
  mounted() {
    this.loadUserInfo()
    this.loadStats()
    this.loadWorks()
    const tab = this.$route.query.tab
    if (tab) this.activeTab = tab
    eventBus.$on('avatar-updated', this.handleAvatarUpdated)
  },
  beforeDestroy() {
    eventBus.$off('avatar-updated', this.handleAvatarUpdated)
  },
  methods: {
    loadUserInfo() {
      try {
        const stored = localStorage.getItem('userInfo')
        if (stored) {
          const parsed = JSON.parse(stored)
          Object.assign(this.userInfo, {
            id: parsed.id || '',
            username: parsed.username || '',
            name: parsed.name || '',
            role: parsed.role || '',
            email: parsed.email || '',
            phone: parsed.phone || '',
            avatar: parsed.avatar || ''
          })
          // 同步到编辑表单
          Object.assign(this.editForm, {
            username: parsed.username || '',
            name: parsed.name || '',
            workId: parsed.workId || '',
            department: parsed.department || '',
            email: parsed.email || '',
            phone: parsed.phone || '',
            bio: parsed.bio || ''
          })
        }
      } catch (e) {
        console.warn('Failed to parse userInfo', e)
      }
    },
    async loadStats() {
      try {
        const res = await http.get('/api/Auth/stats')
        if (res.data) {
          this.stats = {
            workCount: res.data.workCount || 0,
            favoriteCount: res.data.favoriteCount || 0,
            viewCount: res.data.viewCount || 0
          }
        }
      } catch (e) {
        console.warn('Failed to load stats', e)
      }
    },
    async loadWorks() {
      try {
        const res = await http.get('/api/Work/my')
        this.works = res.data.items || res.data || []
      } catch (e) {
        console.warn('Failed to load works', e)
      }
    },
    handleTabClick(tab) {
      this.activeTab = tab.name
      if (tab.name === 'favorites') {
        this.loadFavorites()
      } else if (tab.name === 'history') {
        this.loadHistory()
      } else if (tab.name === 'works') {
        this.loadWorks()
      } else if (tab.name === 'settings') {
        this.loadUserInfo()
      }
    },
    async saveProfile() {
      this.saving = true
      try {
        const res = await http.put('/api/Auth/profile', {
          name: this.editForm.name,
          workId: this.editForm.workId,
          department: this.editForm.department,
          email: this.editForm.email,
          phone: this.editForm.phone,
          bio: this.editForm.bio
        })
        if (res.data?.user) {
          const user = res.data.user
          this.userInfo.name = user.name || ''
          this.userInfo.email = user.email || ''
          this.userInfo.phone = user.phone || ''
          const stored = JSON.parse(localStorage.getItem('userInfo') || '{}')
          Object.assign(stored, {
            name: user.name,
            workId: user.workId,
            department: user.department,
            email: user.email,
            phone: user.phone,
            bio: user.bio
          })
          localStorage.setItem('userInfo', JSON.stringify(stored))
          eventBus.$emit('user-info-updated', { ...this.userInfo })
        }
        this.$message.success('个人信息更新成功')
      } catch (e) {
        this.$message.error(e.response?.data?.message || '更新失败')
      } finally {
        this.saving = false
      }
    },
    async changePassword() {
      if (!this.passwordForm.currentPassword) {
        this.$message.error('请输入当前密码')
        return
      }
      if (!this.passwordForm.newPassword) {
        this.$message.error('请输入新密码')
        return
      }
      if (this.passwordForm.newPassword.length < 6 || this.passwordForm.newPassword.length > 20) {
        this.$message.error('新密码长度必须在6-20位之间')
        return
      }
      if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
        this.$message.error('两次输入的密码不一致')
        return
      }
      this.savingPassword = true
      try {
        await http.put('/api/Auth/change-password', {
          currentPassword: this.passwordForm.currentPassword,
          newPassword: this.passwordForm.newPassword
        })
        this.$message.success('密码修改成功，请重新登录')
        this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' }
        setTimeout(() => {
          localStorage.removeItem('userToken')
          localStorage.removeItem('userInfo')
          this.$router.push('/login')
        }, 1500)
      } catch (e) {
        this.$message.error(e.response?.data?.message || '密码修改失败')
      } finally {
        this.savingPassword = false
      }
    },
    async loadFavorites() {
      try {
        const res = await http.get('/api/Work/favorites')
        this.favoriteWorks = res.data || []
      } catch (e) {
        console.warn('Failed to load favorites', e)
      }
    },
    async loadHistory() {
      try {
        const res = await http.get('/api/Work/history')
        this.historyWorks = res.data || []
      } catch (e) {
        console.warn('Failed to load history', e)
      }
    },
    async deleteWork(work) {
      try {
        await this.$confirm('确定要删除该作品吗？此操作不可恢复。', '确认删除', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
        await http.delete(`/api/Work/${work.id}`)
        this.$message.success('作品已删除')
        this.loadWorks()
        this.loadStats()
      } catch (e) {
        if (e !== 'cancel') {
          this.$message.error('删除失败')
        }
      }
    },
    async unfavoriteWork(fw) {
      try {
        await http.delete(`/api/Work/${fw.workId}/favorite`)
        this.$message.success('已取消收藏')
        this.loadFavorites()
        this.loadStats()
      } catch (e) {
        this.$message.error('取消收藏失败')
      }
    },
    getAvatarUrl() {
      if (this.userInfo.avatar) return `/api/File/download?fileName=${encodeURIComponent(this.userInfo.avatar)}&t=${Date.now()}`
      return 'https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png'
    },
    getWorkThumb(work) {
      const t = Date.now()
      if (work?.coverImage) return `/api/File/download?fileName=${encodeURIComponent(work.coverImage)}&t=${t}`
      if (work?.thumbnailPath) return `/api/File/download?fileName=${encodeURIComponent(work.thumbnailPath)}&t=${t}`
      if (work?.fileType === 'image' && work?.filePath) return `/api/File/download?fileName=${encodeURIComponent(work.filePath)}&t=${t}`
      return null
    },
    getWorkIcon: getWorkIcon,
    getFileTypeLabel: getFileTypeLabel,
    translateRole(role) {
      const map = { Admin: '超级管理员', Teacher: '指导教师', Student: '学生', Guest: '访客' }
      return map[role] || role
    },
    maskPhone(phone) {
      if (!phone) return ''
      return phone.replace(/(\d{3})\d{4}(\d{4})/, '$1****$2')
    },
    formatDate(dateStr) {
      if (!dateStr) return ''
      const d = new Date(dateStr)
      return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
    },
    handleAvatarUpdated(fileName) {
      console.log('PersonalSpace: avatar-updated', fileName)
      this.userInfo.avatar = fileName
      const stored = JSON.parse(localStorage.getItem('userInfo') || '{}')
      stored.avatar = fileName
      localStorage.setItem('userInfo', JSON.stringify(stored))
    },
    handleAvatarChange(file) {
      const isImage = file.raw.type.startsWith('image/')
      const isLt2M = file.raw.size / 1024 / 1024 < 2
      if (!isImage) {
        this.$message.error('只能上传图片文件！')
        return
      }
      if (!isLt2M) {
        this.$message.error('图片大小不能超过 2MB！')
        return
      }
      this.avatarFile = file.raw
      this.avatarPreviewUrl = URL.createObjectURL(file.raw)
    },
    async uploadAvatar() {
      if (!this.avatarFile) return
      this.avatarUploading = true
      try {
        const formData = new FormData()
        formData.append('file', this.avatarFile)
        const res = await http.post('/api/File/upload-avatar', formData)
        const fileName = res.data?.fileName || res.data?.url || ''
        if (fileName) {
          await http.put('/api/Auth/avatar', { avatar: fileName })
          this.userInfo.avatar = fileName
          const stored = JSON.parse(localStorage.getItem('userInfo') || '{}')
          stored.avatar = fileName
          localStorage.setItem('userInfo', JSON.stringify(stored))
          eventBus.$emit('avatar-updated', fileName)
          eventBus.$emit('user-info-updated', { ...this.userInfo })
          this.$message.success('头像更新成功')
        }
        this.showAvatarUpload = false
      } catch (e) {
        this.$message.error(e.response?.data?.message || '头像上传失败')
      } finally {
        this.avatarUploading = false
      }
    }
  }
}
</script>

<style scoped>
.personal-space-container {
  position: relative;
  min-height: 100vh;
  background: var(--bg-main);
  padding: 24px;
}

.space-bg {
  position: absolute;
  inset: 0;
  overflow: hidden;
  pointer-events: none;
}

.bg-shape {
  position: absolute;
  border-radius: 50%;
  opacity: 0.06;
}

.bg-shape-1 {
  width: 400px;
  height: 400px;
  background: var(--primary);
  top: -100px;
  right: -100px;
}

.bg-shape-2 {
  width: 300px;
  height: 300px;
  background: var(--accent);
  bottom: -80px;
  left: -80px;
}

.space-content {
  position: relative;
  max-width: 1000px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.glass-card {
  background: var(--card-bg);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: 24px;
  box-shadow: var(--shadow-sm);
}

.profile-card {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.profile-header {
  display: flex;
  align-items: center;
  gap: 24px;
}

.avatar-section {
  position: relative;
  cursor: pointer;
}

.profile-avatar {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: white;
  font-size: 32px;
}

.avatar-overlay {
  position: absolute;
  inset: 0;
  border-radius: 50%;
  background: rgba(0,0,0,0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity var(--duration-fast);
  color: white;
  font-size: 20px;
}

.avatar-section:hover .avatar-overlay {
  opacity: 1;
}

.profile-info {
  flex: 1;
}

.profile-name {
  font-size: 22px;
  font-weight: 700;
  margin: 0 0 6px;
  color: var(--text-main);
}

.profile-role {
  margin: 0 0 8px;
}

.profile-email,
.profile-phone {
  font-size: 13px;
  color: var(--text-light);
  margin: 2px 0;
}

.profile-stats {
  display: flex;
  border-top: 1px solid var(--border-light);
  padding-top: 16px;
  gap: 0;
}

.stat-item {
  flex: 1;
  text-align: center;
  cursor: pointer;
  transition: color var(--duration-fast);
}

.stat-item:hover {
  color: var(--primary);
}

.stat-value {
  display: block;
  font-size: 24px;
  font-weight: 700;
  color: var(--text-main);
}

.stat-label {
  display: block;
  font-size: 12px;
  color: var(--text-light);
  margin-top: 2px;
}

.content-card {
  flex: 1;
}

.tab-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  gap: 12px;
}

.search-input {
  max-width: 280px;
}

.work-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 16px;
}

.work-card {
  background: var(--bg-main);
  border-radius: var(--radius-md);
  overflow: hidden;
  cursor: pointer;
  transition: all var(--duration-normal);
  border: 1px solid var(--border-color);
}

.work-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-md);
}

.work-cover {
  position: relative;
  width: 100%;
  height: 160px;
  overflow: hidden;
  background: var(--bg-hover);
}

.work-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.work-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 48px;
  color: var(--text-light);
}

.work-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  opacity: 0;
  transition: opacity var(--duration-fast);
}

.work-card:hover .work-overlay {
  opacity: 1;
}

.work-type-badge {
  position: absolute;
  top: 8px;
  right: 8px;
  background: rgba(0,0,0,0.6);
  color: white;
  padding: 2px 8px;
  border-radius: var(--radius-full);
  font-size: 11px;
}

.work-info {
  padding: 12px;
}

.work-title {
  font-size: 14px;
  font-weight: 600;
  margin: 0 0 6px;
  color: var(--text-main);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.work-desc {
  font-size: 12px;
  color: var(--text-light);
  margin: 0 0 8px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.work-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  font-size: 11px;
  color: var(--text-light);
}

.avatar-upload-section {
  text-align: center;
}

.avatar-preview {
  margin-bottom: 16px;
}

.upload-tip {
  font-size: 12px;
  color: var(--text-light);
  margin-top: 8px;
}

.settings-container {
  max-width: 600px;
  margin: 0 auto;
}

.settings-card {
  margin-bottom: 20px;
  border-radius: var(--radius-md);
}

.settings-card .el-card__header {
  background: var(--bg-hover);
  border-bottom: 1px solid var(--border-color);
  font-weight: 600;
}

.settings-card .el-form-item {
  margin-bottom: 16px;
}

.settings-card .el-form-item__label {
  font-weight: 500;
  color: var(--text-main);
}

.settings-card .el-input__inner {
  border-radius: var(--radius-sm);
}
</style>