<template>
  <div class="personal-page">
    <!-- 背景装饰 -->
    <div class="page-bg">
      <div class="bg-shape bg-shape-1"></div>
      <div class="bg-shape bg-shape-2"></div>
      <div class="bg-shape bg-shape-3"></div>
    </div>

    <div class="page-content" v-loading="pageLoading">
      <!-- ── Hero 头部区 ── -->
      <section class="hero-section">
        <div class="hero-card">
          <div class="hero-avatar-wrap">
            <div class="hero-avatar-ring">
              <el-avatar :size="96" :src="getAvatarUrl()" icon="el-icon-user-solid" class="hero-avatar"></el-avatar>
            </div>
            <div class="avatar-camera-overlay" @click="showAvatarUpload = true">
              <i class="el-icon-camera"></i>
            </div>
          </div>
          <div class="hero-info">
            <h1 class="hero-name">{{ userInfo.name || userInfo.username }}</h1>
            <div class="hero-tags">
              <span class="role-tag" :class="'role-' + (userInfo.role || 'student')">{{ translateRole(userInfo.role) }}</span>
              <span class="dept-tag" v-if="userInfo.department">{{ userInfo.department }}</span>
            </div>
            <p class="hero-bio" v-if="userInfo.bio">{{ userInfo.bio }}</p>
            <p class="hero-email" v-if="userInfo.email">
              <i class="el-icon-message"></i> {{ userInfo.email }}
            </p>
            <div class="hero-actions">
              <button class="btn btn-outline" @click="activeTab = 'settings'">
                <span>✏️</span> 编辑资料
              </button>
            </div>
          </div>
          <div class="hero-stats">
            <div class="h-stat" @click="activeTab = 'works'">
              <span class="h-stat-num">{{ stats.workCount }}</span>
              <span class="h-stat-label">🖼 作品</span>
            </div>
            <div class="h-stat" @click="activeTab = 'favorites'">
              <span class="h-stat-num">{{ stats.favoriteCount }}</span>
              <span class="h-stat-label">⭐ 收藏</span>
            </div>
            <div class="h-stat">
              <span class="h-stat-num">{{ stats.viewCount }}</span>
              <span class="h-stat-label">👁 浏览</span>
            </div>
          </div>
        </div>
      </section>

      <!-- ── Tab 切换区（自定义 Pill） ── -->
      <section class="tab-nav-section">
        <div class="pill-group">
          <button
            v-for="tab in tabItems"
            :key="tab.key"
            :class="['pill-btn', { active: activeTab === tab.key }]"
            @click="activeTab = tab.key; handleTabSwitch(tab.key)"
          >
            <span class="pill-emoji">{{ tab.emoji }}</span>
            <span>{{ tab.label }}</span>
          </button>
        </div>
      </section>

      <!-- ── 我的作品 ── -->
      <section v-if="activeTab === 'works'" class="works-section">
        <div class="section-toolbar">
          <el-input
            v-model="workSearch"
            placeholder="搜索作品..."
            prefix-icon="el-icon-search"
            size="medium"
            clearable
            class="toolbar-search"
          ></el-input>
          <button class="btn btn-primary" @click="$router.push('/works/manage?action=upload')">
            <span>📤</span> 上传作品
          </button>
        </div>

        <div class="works-grid" v-if="filteredWorks.length > 0">
          <div
            v-for="work in filteredWorks"
            :key="work.id"
            class="work-card"
            @click="$router.push(`/works/${work.id}`)"
          >
            <div class="card-cover">
              <img v-if="getWorkThumb(work)" :src="getWorkThumb(work)" :alt="work.title" class="cover-img" />
              <div v-else class="cover-placeholder" :style="{ background: getGradient(work.id, 0) }">
                <span class="cover-icon">{{ getWorkIconText(work.fileType) }}</span>
                <span class="cover-ext">{{ getFileTypeLabel(work.fileType) }}</span>
              </div>
              <span class="category-badge">{{ (work.category || '未分类') }}</span>
              <div class="cover-overlay">
                <button class="cover-btn" @click.stop="$router.push(`/works/${work.id}`)" title="查看">
                  👁
                </button>
                <button class="cover-btn cover-btn-danger" @click.stop="deleteWork(work)" title="删除">
                  🗑
                </button>
              </div>
            </div>
            <div class="card-body">
              <h3 class="card-title">{{ work.title }}</h3>
              <p class="card-desc">{{ work.description || '暂无描述' }}</p>
              <div class="card-meta">
                <span>👁 {{ work.views || 0 }}</span>
                <span>⭐ {{ work.favorites || 0 }}</span>
                <span>🕐 {{ formatDate(work.createdAt) }}</span>
              </div>
            </div>
          </div>
        </div>
        <div v-else class="empty-state">
          <div class="empty-emoji">📂</div>
          <h3>还没有作品</h3>
          <p>快去上传你的第一个作品吧！</p>
          <button class="btn btn-primary" @click="$router.push('/works/manage?action=upload')">📤 上传作品</button>
        </div>
      </section>

      <!-- ── 我的收藏 ── -->
      <section v-if="activeTab === 'favorites'" class="works-section">
        <div class="works-grid" v-if="favoriteWorks.length > 0">
          <div
            v-for="fw in favoriteWorks"
            :key="fw.id"
            class="work-card"
            @click="$router.push(`/works/${fw.workId}`)"
          >
            <div class="card-cover">
              <img v-if="getWorkThumb(fw)" :src="getWorkThumb(fw)" :alt="fw.title" class="cover-img" />
              <div v-else class="cover-placeholder" :style="{ background: getGradient(fw.workId || fw.id, 0) }">
                <span class="cover-icon">{{ getWorkIconText(fw.fileType) }}</span>
                <span class="cover-ext">{{ getFileTypeLabel(fw.fileType) }}</span>
              </div>
              <span class="category-badge favorite-badge">⭐ 已收藏</span>
              <div class="cover-overlay">
                <button class="cover-btn cover-btn-warning" @click.stop="unfavoriteWork(fw)" title="取消收藏">
                  ⭐
                </button>
              </div>
            </div>
            <div class="card-body">
              <h3 class="card-title">{{ fw.title }}</h3>
              <div class="card-meta">
                <span>👁 {{ fw.views || 0 }}</span>
                <span>⭐ {{ fw.favorites || 0 }}</span>
                <span>👤 {{ fw.authorName }}</span>
                <span>🕐 {{ formatDate(fw.collectionDate) }}</span>
              </div>
            </div>
          </div>
        </div>
        <div v-else class="empty-state">
          <div class="empty-emoji">💛</div>
          <h3>还没有收藏</h3>
          <p>浏览作品展厅，发现喜欢的作品吧！</p>
          <button class="btn btn-primary" @click="$router.push('/display')">🔍 浏览作品</button>
        </div>
      </section>

      <!-- ── 浏览历史 ── -->
      <section v-if="activeTab === 'history'" class="works-section">
        <div class="works-grid" v-if="historyWorks.length > 0">
          <div
            v-for="hw in historyWorks"
            :key="hw.id"
            class="work-card"
            @click="$router.push(`/works/${hw.workId}`)"
          >
            <div class="card-cover">
              <img v-if="getWorkThumb(hw)" :src="getWorkThumb(hw)" :alt="hw.title" class="cover-img" />
              <div v-else class="cover-placeholder" :style="{ background: getGradient(hw.workId || hw.id, 0) }">
                <span class="cover-icon">{{ getWorkIconText(hw.fileType) }}</span>
                <span class="cover-ext">{{ getFileTypeLabel(hw.fileType) }}</span>
              </div>
              <span class="category-badge history-badge">🕐 已浏览</span>
            </div>
            <div class="card-body">
              <h3 class="card-title">{{ hw.title }}</h3>
              <div class="card-meta">
                <span>👁 {{ hw.views || 0 }}</span>
                <span>⭐ {{ hw.favorites || 0 }}</span>
                <span>👤 {{ hw.authorName }}</span>
                <span>🕐 {{ formatDate(hw.viewedAt) }}</span>
              </div>
            </div>
          </div>
        </div>
        <div v-else class="empty-state">
          <div class="empty-emoji">👣</div>
          <h3>还没有浏览记录</h3>
          <p>去作品展厅逛逛吧！</p>
        </div>
      </section>

      <!-- ── 个人设置 ── -->
      <section v-if="activeTab === 'settings'" class="settings-section">
        <div class="settings-grid">
          <!-- 基本信息卡片 -->
          <div class="settings-card">
            <div class="settings-card-header">
              <h3>📋 基本信息</h3>
              <button class="btn btn-primary btn-sm" @click="saveProfile" :disabled="saving">
                <span v-if="saving">⏳ 保存中...</span>
                <span v-else>💾 保存修改</span>
              </button>
            </div>
            <div class="settings-form">
              <div class="form-row">
                <label class="form-label">用户名</label>
                <el-input v-model="editForm.username" disabled size="medium" />
              </div>
              <div class="form-row">
                <label class="form-label">姓名</label>
                <el-input v-model="editForm.name" placeholder="请输入姓名" disabled size="medium" />
              </div>
              <div class="form-row">
                <label class="form-label">学号/工号</label>
                <el-input v-model="editForm.workId" placeholder="请输入学号或工号" disabled size="medium" />
              </div>
              <div class="form-row">
                <label class="form-label">院系/部门</label>
                <el-input v-model="editForm.department" placeholder="请输入院系或部门" disabled size="medium" />
              </div>
              <div class="form-row">
                <label class="form-label">邮箱</label>
                <el-input v-model="editForm.email" placeholder="请输入邮箱" size="medium" />
              </div>
              <div class="form-row">
                <label class="form-label">手机号</label>
                <el-input v-model="editForm.phone" placeholder="请输入手机号" size="medium" />
              </div>
              <div class="form-row">
                <label class="form-label">个人简介</label>
                <el-input v-model="editForm.bio" type="textarea" :rows="3" placeholder="介绍一下自己..." />
              </div>
            </div>
          </div>

          <!-- 修改密码卡片 -->
          <div class="settings-card">
            <div class="settings-card-header">
              <h3>🔒 修改密码</h3>
              <button class="btn btn-outline btn-sm" @click="changePassword" :disabled="savingPassword">
                <span v-if="savingPassword">⏳ 修改中...</span>
                <span v-else>🔑 修改密码</span>
              </button>
            </div>
            <div class="settings-form">
              <div class="form-row">
                <label class="form-label">当前密码</label>
                <el-input v-model="passwordForm.currentPassword" type="password" placeholder="请输入当前密码" size="medium" show-password />
              </div>
              <div class="form-row">
                <label class="form-label">新密码</label>
                <el-input v-model="passwordForm.newPassword" type="password" placeholder="6-20位新密码" size="medium" show-password />
              </div>
              <div class="form-row">
                <label class="form-label">确认密码</label>
                <el-input v-model="passwordForm.confirmPassword" type="password" placeholder="再次输入新密码" size="medium" show-password />
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>

    <!-- 头像上传对话框（修复遮罩层 bug） -->
    <el-dialog
      title="上传头像"
      :visible.sync="showAvatarUpload"
      width="420px"
      :close-on-click-modal="false"
      :modal="false"
      append-to-body
    >
      <div class="avatar-upload-section">
        <div class="avatar-preview">
          <el-avatar :size="120" :src="avatarPreviewUrl" icon="el-icon-user-solid"></el-avatar>
        </div>
        <div class="avatar-upload-action">
          <el-upload
            class="avatar-uploader"
            action=""
            :auto-upload="false"
            :show-file-list="false"
            :on-change="handleAvatarChange"
            accept="image/*"
          >
            <el-button type="primary" :loading="avatarUploading">
              <i class="el-icon-upload2"></i> 选择图片
            </el-button>
          </el-upload>
          <el-button @click="uploadAvatar" type="success" :disabled="!avatarFile" :loading="avatarUploading">
            <i class="el-icon-check"></i> 确认上传
          </el-button>
        </div>
        <p class="avatar-hint">支持 JPG、PNG 格式，大小不超过 2MB</p>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import { getToken, getUser } from '../../utils/auth'
import { getAvatarUrl } from '../../utils/fileUtils'
import eventBus from '../../utils/eventBus'

export default {
  name: 'PersonalSpace',
  data() {
    return {
      pageLoading: false,
      activeTab: 'works',
      workSearch: '',
      userInfo: {},
      stats: {
        workCount: 0,
        favoriteCount: 0,
        viewCount: 0
      },
      works: [],
      favoriteWorks: [],
      historyWorks: [],
      // 头像上传
      showAvatarUpload: false,
      avatarFile: null,
      avatarPreviewUrl: '',
      avatarUploading: false,
      // 编辑资料
      saving: false,
      editForm: {
        username: '',
        name: '',
        workId: '',
        department: '',
        email: '',
        phone: '',
        bio: ''
      },
      // 修改密码
      savingPassword: false,
      passwordForm: {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      },
      tabItems: [
        { key: 'works', label: '我的作品', emoji: '🖼' },
        { key: 'favorites', label: '我的收藏', emoji: '⭐' },
        { key: 'history', label: '浏览历史', emoji: '🕐' },
        { key: 'settings', label: '个人设置', emoji: '⚙' }
      ]
    }
  },
  computed: {
    roleTagType() {
      const map = { teacher: 'warning', student: 'success', admin: 'danger' }
      return map[this.userInfo.role] || 'info'
    },
    filteredWorks() {
      if (!this.workSearch.trim()) return this.works
      const q = this.workSearch.trim().toLowerCase()
      return this.works.filter(w => (w.title || '').toLowerCase().includes(q))
    }
  },
  mounted() {
    const savedTab = this.$route.query.tab
    if (savedTab && this.tabItems.some(t => t.key === savedTab)) {
      this.activeTab = savedTab
    }
    this.loadUserInfo()
    this.loadStats()
    this.loadWorks()
  },
  watch: {
    activeTab(newTab) {
      if (newTab === 'favorites' && this.favoriteWorks.length === 0) this.loadFavorites()
      if (newTab === 'history' && this.historyWorks.length === 0) this.loadHistory()
    }
  },
  methods: {
    getAvatarUrl() {
      return getAvatarUrl(this.userInfo.avatar)
    },
    handleTabSwitch(key) {
      // 已经由 watch 处理
    },
    translateRole(role) {
      const map = { teacher: '教师', student: '学生', admin: '管理员' }
      return map[role] || '用户'
    },
    maskPhone(phone) {
      if (!phone || phone.length < 7) return phone
      return phone.slice(0, 3) + '****' + phone.slice(-4)
    },
    formatDate(dateStr) {
      if (!dateStr) return ''
      const d = new Date(dateStr)
      if (isNaN(d.getTime())) return dateStr
      return d.toLocaleDateString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' })
    },
    getFileTypeLabel(fileType) {
      const map = { image: '图片', video: '视频', model: '3D模型', music: '音乐', document: '文档', other: '其他' }
      return map[fileType] || '其他'
    },
    getWorkIcon(fileType) {
      const map = { image: 'el-icon-picture', video: 'el-icon-video-camera', model: 'el-icon-cpu', music: 'el-icon-headset', document: 'el-icon-document', other: 'el-icon-files' }
      return map[fileType] || 'el-icon-files'
    },
    getWorkIconText(fileType) {
      const map = { image: '🖼', video: '🎬', model: '🧊', music: '🎵', document: '📄', other: '📁' }
      return map[fileType] || '📁'
    },
    getWorkThumb(work) {
      if (!work) return ''
      if (work.previewImage) return `/api/File/download?fileName=${encodeURIComponent(work.previewImage)}`
      if (!work.filePath && !work.fileName) return ''
      const fp = work.filePath || work.fileName || ''
      const ext = fp.toLowerCase().substring(fp.lastIndexOf('.'))
      const imageExts = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp']
      if (imageExts.includes(ext)) {
        return `/api/File/download?fileName=${encodeURIComponent(fp)}`
      }
      return ''
    },
    getGradient(id, idx) {
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
      return gradients[(id + idx) % gradients.length]
    },
    // ── API 调用 ──
    async loadUserInfo() {
      try {
        const currentUser = getUser()
        if (currentUser && currentUser.id) {
          const res = await this.$axios.get('/api/Auth/profile')
          this.userInfo = res.data || {}
          this.editForm = {
            username: this.userInfo.username || '',
            name: this.userInfo.name || '',
            workId: this.userInfo.workId || '',
            department: this.userInfo.department || '',
            email: this.userInfo.email || '',
            phone: this.userInfo.phone || '',
            bio: this.userInfo.bio || ''
          }
        }
      } catch (e) {
        console.error('加载用户信息失败', e)
      }
    },
    async loadStats() {
      try {
        const currentUser = getUser()
        if (!currentUser || !currentUser.id) return
        const res = await this.$axios.get('/api/Data/userStats')
        this.stats = res.data || { workCount: 0, favoriteCount: 0, viewCount: 0 }
      } catch (e) {
        console.error('加载统计数据失败', e)
      }
    },
    async loadWorks() {
      try {
        const currentUser = getUser()
        if (!currentUser || !currentUser.id) return
        // 后端参数名是 user_id 不是 userId，必须严格匹配
        const res = await this.$axios.get('/api/Work', { params: { user_id: currentUser.id } })
        this.works = Array.isArray(res.data) ? res.data : (res.data.items || [])
      } catch (e) {
        console.error('加载作品失败', e)
      }
    },
    async loadFavorites() {
      try {
        const currentUser = getUser()
        if (!currentUser || !currentUser.id) return
        const res = await this.$axios.get('/api/Collection')
        const items = Array.isArray(res.data) ? res.data : (res.data.items || [])
        // 后端使用显式 camelCase 字段名，直接展平 work 子对象
        this.favoriteWorks = items.map(item => ({
          ...(item.work || {}),
          id: item.work?.id,
          title: item.work?.title,
          views: item.work?.views ?? 0,
          favorites: item.work?.favorites ?? 0,
          authorName: item.work?.uploadUserName,
          favoriteId: item.id,
          workId: item.workId,
          collectionDate: item.collectionDate
        }))
      } catch (e) {
        console.error('加载收藏失败', e)
      }
    },
    async loadHistory() {
      try {
        const currentUser = getUser()
        if (!currentUser || !currentUser.id) return
        // 正确调用浏览历史接口（后端从JWT获取用户ID，无需传参）
        const res = await this.$axios.get('/api/Work/history')
        this.historyWorks = Array.isArray(res.data) ? res.data : (res.data.items || [])
      } catch (e) {
        console.error('加载浏览历史失败', e)
      }
    },
    async deleteWork(work) {
      try {
        await this.$confirm('确定删除该作品吗？此操作不可恢复。', '确认删除', {
          confirmButtonText: '删除',
          cancelButtonText: '取消',
          type: 'warning'
        })
        await this.$axios.delete(`/api/Work/${work.id}`)
        this.$message.success('作品已删除')
        this.loadWorks()
        this.loadStats()
      } catch (e) {
        if (e !== 'cancel') {
          this.$message.error(e.response?.data?.message || '删除失败')
        }
      }
    },
    async unfavoriteWork(fw) {
      try {
        // 后端取消收藏接口：DELETE /api/Collection/{workId}
        await this.$axios.delete(`/api/Collection/${fw.workId || fw.id}`)
        this.$message.success('已取消收藏')
        this.favoriteWorks = this.favoriteWorks.filter(w => (w.favoriteId || w.id) !== (fw.favoriteId || fw.id))
        this.loadStats()
      } catch (e) {
        this.$message.error('取消收藏失败')
      }
    },
    async saveProfile() {
      this.saving = true
      try {
        const currentUser = getUser()
        await this.$axios.put('/api/Auth/profile', this.editForm)
        this.$message.success('资料已更新')
        this.loadUserInfo()
      } catch (e) {
        this.$message.error(e.response?.data?.message || '保存失败')
      } finally {
        this.saving = false
      }
    },
    async changePassword() {
      if (!this.passwordForm.currentPassword) {
        this.$message.warning('请输入当前密码')
        return
      }
      if (!this.passwordForm.newPassword || this.passwordForm.newPassword.length < 6) {
        this.$message.warning('新密码至少6位')
        return
      }
      if (this.passwordForm.newPassword !== this.passwordForm.confirmPassword) {
        this.$message.warning('两次输入的密码不一致')
        return
      }
      this.savingPassword = true
      try {
        const currentUser = getUser()
        await this.$axios.put('/api/Auth/change-password', {
          currentPassword: this.passwordForm.currentPassword,
          newPassword: this.passwordForm.newPassword
        })
        this.$message.success('密码已修改，请重新登录')
        this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' }
      } catch (e) {
        this.$message.error(e.response?.data?.message || '密码修改失败')
      } finally {
        this.savingPassword = false
      }
    },
    // ── 头像上传 ──
    handleAvatarChange(file) {
      const isImage = file.raw.type.startsWith('image/')
      const isLt2M = file.raw.size / 1024 / 1024 < 2
      if (!isImage) {
        this.$message.error('仅支持图片格式')
        return
      }
      if (!isLt2M) {
        this.$message.error('图片大小不能超过 2MB')
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
        const res = await this.$axios.post('/api/File/upload-avatar', formData, {
          headers: { 'Content-Type': undefined }
        })
        
        const newFileName = res.data.fileName
        // 更新 localStorage
        const stored = localStorage.getItem('userInfo')
        if (stored) {
          const current = JSON.parse(stored)
          current.avatar = newFileName
          localStorage.setItem('userInfo', JSON.stringify(current))
        }
        // 通知 NavBar 刷新头像
        eventBus.$emit('avatar-updated', newFileName)
        
        this.$message.success('头像上传成功')
        this.showAvatarUpload = false
        this.avatarFile = null
        this.loadUserInfo()
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
/* ── 页面容器 ── */
.personal-page {
  position: relative;
  min-height: 100vh;
  background: var(--bg-page, #F8F9F5);
  padding: 24px;
  font-family: var(--font-main);
}

/* ── 背景装饰 ── */
.page-bg {
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
  background: var(--primary, #2D8A6E);
  top: -100px;
  right: -100px;
}

.bg-shape-2 {
  width: 300px;
  height: 300px;
  background: var(--accent, #C8AA6E);
  bottom: -80px;
  left: -80px;
}

.bg-shape-3 {
  width: 200px;
  height: 200px;
  background: var(--primary-light, #45A884);
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%) translateX(300px) translateY(-200px);
}

.page-content {
  position: relative;
  max-width: 1100px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* ── Hero 头部 ── */
.hero-section {
  animation: fadeInUp .6s var(--ease-out-expo, cubic-bezier(0.16, 1, 0.3, 1));
}

.hero-card {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-xl, 24px);
  border: 1px solid var(--border-color, #E8E2D8);
  padding: 36px 40px;
  display: flex;
  align-items: center;
  gap: 32px;
  flex-wrap: wrap;
  box-shadow: var(--shadow-card, 0 2px 12px rgba(0,0,0,.04));
  transition: box-shadow var(--duration-normal, .3s) var(--ease-out-expo);
}

.hero-card:hover {
  box-shadow: var(--shadow-card-hover, 0 16px 40px rgba(0,0,0,.12));
}

/* 头像 */
.hero-avatar-wrap {
  position: relative;
  flex-shrink: 0;
}

.hero-avatar-ring {
  padding: 4px;
  border-radius: 50%;
  background: transparent;
}

.hero-avatar {
  background: transparent;
  color: white;
  font-size: 36px;
  font-weight: 700;
}

.avatar-camera-overlay {
  position: absolute;
  inset: 4px;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity var(--duration-fast, .18s);
  color: white;
  font-size: 22px;
  cursor: pointer;
}

.hero-avatar-wrap:hover .avatar-camera-overlay {
  opacity: 1;
}

/* 信息 */
.hero-info {
  flex: 1;
  min-width: 200px;
}

.hero-name {
  font-size: 26px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 12px;
  letter-spacing: -0.3px;
}

.hero-tags {
  display: flex;
  gap: 8px;
  margin-bottom: 10px;
  flex-wrap: wrap;
}

.role-tag {
  display: inline-block;
  padding: 4px 14px;
  border-radius: var(--radius-full, 9999px);
  font-size: 12px;
  font-weight: 600;
}

.role-teacher {
  background: #FFF3E0;
  color: #E67E22;
}

.role-student {
  background: var(--primary-bg, #EDF5F0);
  color: var(--primary, #2D8A6E);
}

.role-admin {
  background: #FDEBEB;
  color: #C0392B;
}

.dept-tag {
  display: inline-block;
  padding: 4px 14px;
  border-radius: var(--radius-full, 9999px);
  background: var(--bg-hover, #F5F2EC);
  color: var(--text-light, #888888);
  font-size: 12px;
  font-weight: 500;
}

.hero-bio {
  font-size: 14px;
  color: var(--text-secondary, #555555);
  margin: 0 0 8px;
  line-height: 1.5;
  max-width: 480px;
}

.hero-email {
  font-size: 13px;
  color: var(--text-light, #888888);
  margin: 0 0 14px;
}

.hero-actions {
  display: flex;
  gap: 10px;
}

/* 统计数据 */
.hero-stats {
  display: flex;
  gap: 0;
  border-left: 1px solid var(--border-light, #F2EDE6);
  padding-left: 32px;
  flex-shrink: 0;
}

.h-stat {
  text-align: center;
  padding: 0 20px;
  cursor: pointer;
  transition: color var(--duration-fast, .18s);
}

.h-stat:hover {
  color: var(--primary, #2D8A6E);
}

.h-stat + .h-stat {
  border-left: 1px solid var(--border-light, #F2EDE6);
}

.h-stat-num {
  display: block;
  font-size: 28px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
}

.h-stat-label {
  display: block;
  font-size: 13px;
  color: var(--text-light, #888888);
  margin-top: 4px;
}

/* ── Pill Tab 导航 ── */
.tab-nav-section {
  animation: fadeInUp .6s var(--ease-out-expo) .05s both;
}

.pill-group {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.pill-btn {
  padding: 10px 22px;
  border-radius: var(--radius-full, 9999px);
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  color: var(--text-light, #888888);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all var(--duration-fast, .18s) var(--ease-out-expo);
  display: flex;
  align-items: center;
  gap: 6px;
  font-family: var(--font-main);
}

.pill-btn:hover {
  background: var(--primary-bg, #EDF5F0);
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
}

.pill-btn.active {
  background: var(--primary, #2D8A6E);
  border-color: var(--primary, #2D8A6E);
  color: #FFFFFF;
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45,138,110,.2));
}

.pill-emoji {
  font-size: 16px;
}

/* ── 作品区域 ── */
.works-section {
  animation: fadeInUp .5s var(--ease-out-expo) .1s both;
}

.section-toolbar {
  display: flex;
  gap: 12px;
  margin-bottom: 20px;
  align-items: center;
}

.toolbar-search {
  max-width: 320px;
}

/* ── 作品卡片网格 ── */
.works-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
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
  box-shadow: var(--shadow-card-hover, 0 16px 40px rgba(0,0,0,.12));
  transform: translateY(-4px);
}

/* 封面 */
.card-cover {
  height: 180px;
  position: relative;
  overflow: hidden;
  background: var(--border-light, #F2EDE6);
}

.cover-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform var(--duration-slow, .5s) var(--ease-out-expo);
}

.work-card:hover .cover-img {
  transform: scale(1.06);
}

.cover-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: transform var(--duration-slow, .5s) var(--ease-out-expo);
  color: var(--primary, #2D8A6E);
}

.work-card:hover .cover-placeholder {
  transform: scale(1.06);
}

.cover-icon {
  font-size: 40px;
  opacity: 0.5;
}

.cover-ext {
  font-size: 13px;
  font-weight: 600;
}

.category-badge {
  position: absolute;
  top: 12px;
  left: 12px;
  padding: 4px 12px;
  border-radius: var(--radius-full, 9999px);
  background: rgba(255, 255, 255, .85);
  backdrop-filter: blur(8px);
  font-size: 11px;
  font-weight: 600;
  color: var(--primary, #2D8A6E);
  z-index: 2;
}

.favorite-badge {
  color: #B8860B;
  background: rgba(255, 248, 225, .9);
}

.history-badge {
  color: var(--text-light, #888888);
  background: rgba(245, 242, 236, .9);
}

/* 封面悬浮操作 */
.cover-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  opacity: 0;
  transition: opacity var(--duration-fast, .18s);
}

.work-card:hover .cover-overlay {
  opacity: 1;
}

.cover-btn {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  border: none;
  background: rgba(255, 255, 255, .9);
  font-size: 18px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--duration-fast, .18s);
}

.cover-btn:hover {
  background: white;
  transform: scale(1.1);
}

.cover-btn-danger:hover {
  background: #FDEBEB;
}

.cover-btn-warning:hover {
  background: #FFF8E1;
}

/* 卡片内容 */
.card-body {
  padding: 16px;
}

.card-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.work-card:hover .card-title {
  color: var(--primary, #2D8A6E);
}

.card-desc {
  font-size: 12px;
  color: var(--text-light, #888888);
  margin: 0 0 10px;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-meta {
  display: flex;
  gap: 14px;
  font-size: 12px;
  color: var(--text-light, #888888);
  flex-wrap: wrap;
}

/* ── 空状态 ── */
.empty-state {
  text-align: center;
  padding: 64px 40px;
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
}

.empty-emoji {
  font-size: 56px;
  margin-bottom: 16px;
  opacity: .4;
}

.empty-state h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 8px;
}

.empty-state p {
  font-size: 14px;
  color: var(--text-light, #888888);
  margin: 0 0 20px;
}

/* ── 按钮系统 ── */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 10px 22px;
  border-radius: var(--radius-sm, 10px);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition: all var(--duration-fast, .18s);
  font-family: var(--font-main);
  text-decoration: none;
}

.btn-primary {
  background: var(--primary, #2D8A6E);
  color: #fff;
}

.btn-primary:hover {
  background: var(--primary-hover, #25755C);
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45,138,110,.2));
}

.btn-outline {
  background: transparent;
  border: 1px solid var(--border-color, #E8E2D8);
  color: var(--text-main, #1A1A1A);
}

.btn-outline:hover {
  background: var(--bg-hover, #F5F2EC);
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
}

.btn-sm {
  padding: 6px 16px;
  font-size: 13px;
}

/* ── 个人设置区域 ── */
.settings-section {
  animation: fadeInUp .5s var(--ease-out-expo) .1s both;
}

.settings-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.settings-card {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  padding: 24px;
}

.settings-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--border-light, #F2EDE6);
}

.settings-card-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
}

.settings-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-row {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-label {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-secondary, #555555);
}

/* ── 头像上传弹窗 ── */
.avatar-upload-section {
  text-align: center;
}

.avatar-preview {
  margin-bottom: 20px;
}

.avatar-upload-action {
  display: flex;
  gap: 12px;
  justify-content: center;
}

.avatar-hint {
  font-size: 12px;
  color: var(--text-placeholder, #B0B0B0);
  margin-top: 12px;
}

/* ── 动画 ── */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* ── 响应式 ── */
@media (max-width: 768px) {
  .personal-page {
    padding: 16px;
  }

  .hero-card {
    padding: 24px;
    flex-direction: column;
    text-align: center;
  }

  .hero-stats {
    border-left: none;
    border-top: 1px solid var(--border-light, #F2EDE6);
    padding-left: 0;
    padding-top: 20px;
    width: 100%;
    justify-content: center;
  }

  .hero-info {
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .hero-bio {
    max-width: 100%;
  }

  .settings-grid {
    grid-template-columns: 1fr;
  }

  .works-grid {
    grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  }
}
</style>