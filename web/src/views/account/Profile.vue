<template>
  <div class="profile-container fade-in-up">
    <el-card class="profile-header-card" shadow="never">
      <div class="profile-banner">
        <div class="banner-decoration"></div>
      </div>
      <div class="profile-info-wrap">
        <div class="avatar-wrapper" @click="showAvatarDialog = true">
          <el-avatar :size="96" class="user-avatar" :src="getAvatarUrl()" />
          <div class="avatar-upload-btn">
            <i class="el-icon-camera"></i>
          </div>
          <input type="file" ref="avatarInput" accept="image/jpeg,image/png,image/gif" class="avatar-upload-input" @change="handleAvatarUpload">
        </div>

        <div class="user-main-info">
          <h2 class="user-name">
            {{ userInfo.name || '未设置姓名' }}
            <el-tag size="small" effect="plain" class="role-tag gradient-tag">
              {{ getRoleText(userInfo.role) }}
            </el-tag>
          </h2>
          <div class="user-meta">
            <span><i class="el-icon-user"></i> {{ getWorkIdLabel(userInfo.role) }}：{{ userInfo.workId || '未设置' }}</span>
            <el-divider direction="vertical" />
            <span><i class="el-icon-office-building"></i> {{ getDepartmentLabel(userInfo.role) }}：{{ userInfo.department || '未设置' }}</span>
          </div>
        </div>
      </div>
    </el-card>

    <el-card class="profile-content-card" shadow="never">
      <el-tabs v-model="activeTab" class="custom-tabs">
        <el-tab-pane label="基本信息" name="basic">
          <div class="tab-content">
            <div class="content-header">
              <h3 class="section-title">个人资料</h3>
              <el-button
                v-if="!isEditing"
                type="primary"
                @click="toggleEdit"
                class="hover-lift primary-btn-glow"
              >
                <i class="el-icon-edit"></i> 编辑资料
              </el-button>
            </div>

            <transition name="fade-slide" mode="out-in">
              <div v-if="!isEditing" key="view" class="view-mode">
                <el-descriptions :column="2" border class="custom-descriptions">
                  <el-descriptions-item label="姓名">
                    <span class="desc-text">{{ userInfo.name || '未填写' }}</span>
                  </el-descriptions-item>
                  <el-descriptions-item :label="getWorkIdLabel(userInfo.role)">
                    <span class="desc-text">{{ userInfo.workId || '未填写' }}</span>
                  </el-descriptions-item>
                  <el-descriptions-item :label="getDepartmentLabel(userInfo.role)">
                    <span class="desc-text">{{ userInfo.department || '未填写' }}</span>
                  </el-descriptions-item>
                  <el-descriptions-item label="联系电话">
                    <span class="desc-text">{{ userInfo.phone || '未填写' }}</span>
                  </el-descriptions-item>
                  <el-descriptions-item label="校园邮箱">
                    <span class="desc-text">{{ userInfo.email || '未填写' }}</span>
                  </el-descriptions-item>
                  <el-descriptions-item label="注册时间">
                    <span class="desc-text">{{ formatDate(userInfo.createdAt) }}</span>
                  </el-descriptions-item>
                  <!-- 教师特有字段 -->
                  <el-descriptions-item v-if="userInfo.role === 'Teacher'" label="职称">
                    <span class="desc-text">{{ userInfo.title || '未填写' }}</span>
                  </el-descriptions-item>
                  <el-descriptions-item v-if="userInfo.role === 'Teacher'" label="研究方向" :span="2">
                    <span class="desc-text">{{ userInfo.researchArea || '未填写' }}</span>
                  </el-descriptions-item>
                  <!-- 管理员特有字段 -->
                  <el-descriptions-item v-if="userInfo.role === 'Admin'" label="职位">
                    <span class="desc-text">{{ userInfo.position || '未填写' }}</span>
                  </el-descriptions-item>
                  <el-descriptions-item label="个人简介" :span="2">
                    <div class="bio-text">{{ userInfo.bio || '这个人很懒，什么都没写~' }}</div>
                  </el-descriptions-item>
                </el-descriptions>
              </div>

              <div v-else key="edit" class="edit-mode">
                <el-form
                  ref="formRef"
                  :model="editForm"
                  :rules="rules"
                  label-width="100px"
                  class="modern-form"
                >
                  <el-row :gutter="24">
                    <el-col :span="12">
                      <el-form-item label="姓名" prop="name">
                        <el-input v-model="editForm.name" placeholder="请输入姓名" class="animated-input" />
                      </el-form-item>
                    </el-col>
                    <el-col :span="12">
                      <el-form-item :label="getWorkIdLabel(userInfo.role)" prop="workId">
                        <el-input v-model="editForm.workId" :placeholder="getWorkIdPlaceholder(userInfo.role)" class="animated-input" />
                      </el-form-item>
                    </el-col>
                  </el-row>

                  <el-row :gutter="24">
                    <el-col :span="12">
                      <el-form-item :label="getDepartmentLabel(userInfo.role)" prop="department">
                        <el-input v-model="editForm.department" :placeholder="getDepartmentPlaceholder(userInfo.role)" class="animated-input" />
                      </el-form-item>
                    </el-col>
                    <el-col :span="12">
                      <el-form-item label="联系电话" prop="phone">
                        <el-input v-model="editForm.phone" placeholder="请输入手机号" class="animated-input" />
                      </el-form-item>
                    </el-col>
                  </el-row>

                  <el-row :gutter="24">
                    <el-col :span="12">
                      <el-form-item label="校园邮箱" prop="email">
                        <el-input v-model="editForm.email" placeholder="请输入邮箱" class="animated-input" />
                      </el-form-item>
                    </el-col>
                    <el-col v-if="userInfo.role === 'Teacher'" :span="12">
                      <el-form-item label="职称" prop="title">
                        <el-input v-model="editForm.title" placeholder="请输入职称" class="animated-input" />
                      </el-form-item>
                    </el-col>
                    <el-col v-if="userInfo.role === 'Admin'" :span="12">
                      <el-form-item label="职位" prop="position">
                        <el-input v-model="editForm.position" placeholder="请输入职位" class="animated-input" />
                      </el-form-item>
                    </el-col>
                  </el-row>

                  <!-- 教师特有字段 -->
                  <el-form-item v-if="userInfo.role === 'Teacher'" label="研究方向" prop="researchArea">
                    <el-input v-model="editForm.researchArea" placeholder="请输入研究方向" class="animated-input" />
                  </el-form-item>

                  <el-form-item label="个人简介" prop="bio">
                    <el-input
                      v-model="editForm.bio"
                      type="textarea"
                      :rows="4"
                      placeholder="介绍一下你自己、擅长的技术或创作理念..."
                      maxlength="200"
                      show-word-limit
                      class="animated-input"
                    />
                  </el-form-item>

                  <el-form-item class="form-actions">
                    <el-button @click="cancelEdit" class="hover-lift">取消</el-button>
                    <el-button
                      type="primary"
                      @click="saveProfile"
                      :loading="isSaving"
                      class="hover-lift primary-btn-glow"
                    >
                      {{ isSaving ? '保存中...' : '保存更改' }}
                    </el-button>
                  </el-form-item>
                </el-form>
              </div>
            </transition>
          </div>
        </el-tab-pane>

        <el-tab-pane label="安全设置" name="security">
          <div class="tab-content">
            <h3 class="section-title">账号安全</h3>
            <div class="security-list">
              <div class="security-item hover-card-lift item-1">
                <div class="sec-info">
                  <span class="sec-title">账户密码</span>
                  <span class="sec-desc">定期修改密码有助于保护账号安全</span>
                </div>
                <el-button type="primary" plain class="action-btn" @click="showPasswordDialog = true">修改密码</el-button>
              </div>
              <div class="security-item hover-card-lift item-2">
                <div class="sec-info">
                  <span class="sec-title">绑定手机</span>
                  <span class="sec-desc">已绑定：{{ maskPhone(userInfo.phone) }}</span>
                </div>
                <el-button plain class="action-btn">更改手机</el-button>
              </div>
              <div class="security-item hover-card-lift danger-zone item-3">
                <div class="sec-info">
                  <span class="sec-title text-danger">账号注销</span>
                  <span class="sec-desc">注销后您的所有作品和数据将被永久清除，请谨慎操作</span>
                </div>
                <el-button type="danger" plain class="action-btn">申请注销</el-button>
              </div>
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <!-- 修改密码对话框 -->
    <el-dialog
      title="修改密码"
      :visible.sync="showPasswordDialog"
      width="400px"
      :close-on-click-modal="false"
      :z-index="2000"
      :modal-append-to-body="true"
      :append-to-body="true"
    >
      <el-form
        ref="passwordFormRef"
        :model="passwordForm"
        :rules="passwordRules"
        label-width="100px"
      >
        <el-form-item label="当前密码" prop="currentPassword">
          <el-input
            v-model="passwordForm.currentPassword"
            type="password"
            placeholder="请输入当前密码"
            show-password
          />
        </el-form-item>
        <el-form-item label="新密码" prop="newPassword">
          <el-input
            v-model="passwordForm.newPassword"
            type="password"
            placeholder="请输入新密码（6-20位）"
            show-password
          />
        </el-form-item>
        <el-form-item label="确认密码" prop="confirmPassword">
          <el-input
            v-model="passwordForm.confirmPassword"
            type="password"
            placeholder="请再次输入新密码"
            show-password
          />
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click="cancelPasswordChange">取消</el-button>
        <el-button type="primary" @click="changePassword" :loading="changingPassword">
          {{ changingPassword ? '修改中...' : '确认修改' }}
        </el-button>
      </div>
    </el-dialog>

    <!-- 头像上传对话框 -->
    <el-dialog
      title="更换头像"
      :visible.sync="showAvatarDialog"
      width="450px"
      :close-on-click-modal="false"
      :z-index="2000"
      :modal-append-to-body="true"
      :append-to-body="true"
      class="avatar-upload-dialog"
    >
      <div class="avatar-dialog-content">
        <!-- 预览区域 -->
        <div class="preview-section">
          <h4 class="preview-title">头像预览</h4>
          <div class="preview-container">
            <div class="main-preview">
              <div class="preview-circle-large" :style="{ backgroundImage: `url(${previewImage})` }"></div>
              <span class="preview-size">120×120</span>
            </div>
            <div class="preview-sizes">
              <div class="preview-item">
                <div class="preview-circle-medium" :style="{ backgroundImage: `url(${previewImage})` }"></div>
                <span>96×96</span>
              </div>
              <div class="preview-item">
                <div class="preview-circle-small" :style="{ backgroundImage: `url(${previewImage})` }"></div>
                <span>48×48</span>
              </div>
              <div class="preview-item">
                <div class="preview-circle-tiny" :style="{ backgroundImage: `url(${previewImage})` }"></div>
                <span>32×32</span>
              </div>
            </div>
          </div>
        </div>
        
        <!-- 上传区域 -->
        <div class="upload-section">
          <div class="upload-area" @click="triggerFileInput" @dragover.prevent @drop="handleDrop">
            <i class="el-icon-upload upload-icon-small"></i>
            <span>点击选择图片</span>
          </div>
          <span class="upload-hint">支持 JPG、PNG，最大 5MB</span>
        </div>
        
        <input type="file" ref="dialogFileInput" accept="image/jpeg,image/png" class="hidden-file-input" @change="handleAvatarUpload">
      </div>
      
      <div slot="footer" class="dialog-footer">
        <el-button @click="showAvatarDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmAvatarUpload" :loading="isUploading">
          {{ isUploading ? '上传中...' : '确认上传' }}
        </el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import http from '../../utils/http'
import { authApi } from '@/services/api'
import { eventBus } from '@/utils/eventBus'

export default {
  name: 'Profile',
  data() {
    const validateConfirmPassword = (rule, value, callback) => {
      if (value === '') {
        callback(new Error('请再次输入新密码'))
      } else if (value !== this.passwordForm.newPassword) {
        callback(new Error('两次输入的密码不一致'))
      } else {
        callback()
      }
    }

    return {
      activeTab: 'basic',
      isEditing: false,
      isSaving: false,
      isUploading: false,
      selectedFile: null,
      previewImageUrl: '',
      formRef: null,
      showPasswordDialog: false,
      showAvatarDialog: false,
      changingPassword: false,
      passwordFormRef: null,
      userInfo: {
        name: '',
        role: 'Student',
        workId: '',
        department: '',
        phone: '',
        email: '',
        bio: '',
        title: '',
        researchArea: '',
        position: '',
        createdAt: ''
      },
      editForm: {
        name: '',
        role: 'Student',
        workId: '',
        department: '',
        phone: '',
        email: '',
        bio: '',
        title: '',
        researchArea: '',
        position: ''
      },
      passwordForm: {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      },
      rules: {
        phone: [
          { required: true, message: '请输入手机号', trigger: 'blur' },
          { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号格式', trigger: 'blur' }
        ],
        email: [
          { required: true, message: '请输入邮箱地址', trigger: 'blur' },
          { type: 'email', message: '请输入正确的邮箱地址', trigger: 'blur' }
        ]
      },
      passwordRules: {
        currentPassword: [
          { required: true, message: '请输入当前密码', trigger: 'blur' }
        ],
        newPassword: [
          { required: true, message: '请输入新密码', trigger: 'blur' },
          { min: 6, max: 20, message: '密码长度在6-20位之间', trigger: 'blur' }
        ],
        confirmPassword: [
          { required: true, validator: validateConfirmPassword, trigger: 'blur' }
        ]
      }
    }
  },
  computed: {
    previewImage() {
      // 如果有选中的图片，显示选中的图片预览；否则显示当前头像
      return this.previewImageUrl || this.getAvatarUrl()
    }
  },
  mounted() {
    this.fetchUserProfile()
  },
  methods: {
    getRoleText(role) {
      const roleMap = {
        'Admin': '管理员',
        'Teacher': '教师',
        'Student': '学生'
      }
      return roleMap[role] || role
    },
    getWorkIdLabel(role) {
      return role === 'Student' ? '学号' : '工号'
    },
    getWorkIdPlaceholder(role) {
      return role === 'Student' ? '请输入学号' : '请输入工号'
    },
    getDepartmentLabel(role) {
      const deptMap = {
        'Admin': '处室',
        'Teacher': '学院',
        'Student': '学院'
      }
      return deptMap[role] || '部门'
    },
    getDepartmentPlaceholder(role) {
      return role === 'Admin' ? '请输入处室' : '请输入学院'
    },
    formatDate(dateString) {
      if (!dateString) return '未知'
      const date = new Date(dateString)
      return date.toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      })
    },
    getAvatarUrl() {
      if (this.userInfo.avatar) {
        return `/api/File/download?fileName=${encodeURIComponent(this.userInfo.avatar)}`
      }
      return 'https://cube.elemecdn.com/0/88/03b0d39583f48206768a7534e55bcpng.png'
    },
    triggerFileInput() {
      this.$refs.dialogFileInput.click()
    },
    handleAvatarUpload(event) {
      const file = event.target.files[0]
      if (file) {
        this.selectedFile = file
        // 生成预览URL
        const reader = new FileReader()
        reader.onload = (e) => {
          this.previewImageUrl = e.target.result
        }
        reader.readAsDataURL(file)
      }
    },
    handleDrop(event) {
      const file = event.dataTransfer.files[0]
      if (file && file.type.startsWith('image/')) {
        this.selectedFile = file
        // 生成预览URL
        const reader = new FileReader()
        reader.onload = (e) => {
          this.previewImageUrl = e.target.result
        }
        reader.readAsDataURL(file)
      }
    },
    async confirmAvatarUpload() {
      if (!this.selectedFile) {
        this.$message.error('请先选择图片')
        return
      }
      
      // 验证文件类型
      const validTypes = ['image/jpeg', 'image/png']
      if (!validTypes.includes(this.selectedFile.type)) {
        this.$message.error('请上传有效的图片文件（JPG、PNG）')
        return
      }
      
      // 验证文件大小（最大5MB）
      if (this.selectedFile.size > 5 * 1024 * 1024) {
        this.$message.error('图片大小不能超过5MB')
        return
      }
      
      this.isUploading = true
      
      try {
        // 调用后端头像上传接口 PUT /api/Auth/avatar
        const response = await authApi.uploadAvatar(this.selectedFile)
        
        if (response.data) {
          const avatarFileName = response.data.avatar || response.data.avatarFileName || response.data.fileName || ''
          this.userInfo.avatar = avatarFileName
          this.$message.success('头像上传成功')
          
          // 刷新完整用户信息以获取最新 avatar
          const profileRes = await authApi.getProfile()
          if (profileRes.data) {
            this.userInfo.avatar = profileRes.data.avatar || avatarFileName
          }
          
          // 更新 localStorage 中的用户信息，使导航栏头像同步更新
          const rawUserInfo = localStorage.getItem('userInfo')
          if (rawUserInfo) {
            const storedInfo = JSON.parse(rawUserInfo)
            storedInfo.avatar = this.userInfo.avatar
            localStorage.setItem('userInfo', JSON.stringify(storedInfo))
          }
          
          // 触发全局事件，通知 NavBar 等组件头像已更新
          eventBus.$emit('avatar-updated', this.userInfo.avatar)
          
          this.showAvatarDialog = false
          this.selectedFile = null
          this.previewImageUrl = ''
        } else {
          this.$message.error('头像上传失败: 服务器返回数据异常')
        }
      } catch (error) {
        console.error('头像上传失败:', error)
        this.$message.error('头像上传失败: ' + (error.response?.data?.message || error.message))
      } finally {
        this.isUploading = false
        // 清空文件选择器
        if (this.$refs.dialogFileInput) {
          this.$refs.dialogFileInput.value = ''
        }
      }
    },
    async fetchUserProfile() {
      try {
        // 获取用户资料：统一走 /api 相对路径，由开发代理转发
        const response = await http.get('/api/Auth/profile')
        if (response.data) {
          this.userInfo = {
            name: response.data.name || '',
            role: response.data.role || 'Student',
            workId: response.data.workId || '',
            department: response.data.department || '',
            phone: response.data.phone || '',
            email: response.data.email || '',
            bio: response.data.bio || '',
            title: response.data.title || '',
            researchArea: response.data.researchArea || '',
            position: response.data.position || '',
            avatar: response.data.avatar || '',
            createdAt: response.data.createdAt || ''
          }
          this.editForm = { ...this.userInfo }
        }
      } catch (error) {
        console.error('获取用户信息失败:', error)
        this.$message.error('获取用户信息失败，请刷新页面重试')
      }
    },
    toggleEdit() {
      Object.assign(this.editForm, this.userInfo)
      this.isEditing = true
    },
    cancelEdit() {
      this.isEditing = false
      if (this.formRef) this.formRef.clearValidate()
    },
    async saveProfile() {
      if (!this.formRef) return

      this.formRef.validate(async (valid) => {
        if (valid) {
          this.isSaving = true
          try {
            // 更新用户资料：统一走 /api 相对路径，由开发代理转发
            const response = await http.put('/api/Auth/profile', this.editForm)
            if (response.data) {
              this.userInfo = {
                name: response.data.user.name || '',
                role: response.data.user.role || 'Student',
                workId: response.data.user.workId || '',
                department: response.data.user.department || '',
                phone: response.data.user.phone || '',
                email: response.data.user.email || '',
                bio: response.data.user.bio || '',
                title: response.data.user.title || '',
                researchArea: response.data.user.researchArea || '',
                position: response.data.user.position || '',
                createdAt: response.data.user.createdAt || ''
              }
              this.isSaving = false
              this.isEditing = false
              this.$message.success(response.data.message || '个人资料已成功更新！')
            }
          } catch (error) {
            console.error('更新用户信息失败:', error)
            this.$message.error('更新用户信息失败，请重试')
            this.isSaving = false
          }
        }
      })
    },
    maskPhone(phone) {
      if (!phone) return '未绑定'
      return phone.replace(/(\d{3})\d{4}(\d{4})/, '$1****$2')
    },
    cancelPasswordChange() {
      this.showPasswordDialog = false
      this.resetPasswordForm()
    },
    resetPasswordForm() {
      this.passwordForm = {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      }
      if (this.passwordFormRef) {
        this.passwordFormRef.clearValidate()
      }
    },
    async changePassword() {
      if (!this.passwordFormRef) return

      this.passwordFormRef.validate(async (valid) => {
        if (valid) {
          this.changingPassword = true
          try {
            // 修改密码：统一走 /api 相对路径，由开发代理转发
            const response = await http.put('/api/Auth/change-password', {
              currentPassword: this.passwordForm.currentPassword,
              newPassword: this.passwordForm.newPassword,
              confirmPassword: this.passwordForm.confirmPassword
            })
            if (response.data) {
              this.changingPassword = false
              this.showPasswordDialog = false
              this.resetPasswordForm()
              this.$message.success(response.data.message || '密码修改成功')
            }
          } catch (error) {
            console.error('修改密码失败:', error)
            this.$message.error(error.response?.data?.message || '修改密码失败，请重试')
            this.changingPassword = false
          }
        }
      })
    }
  }
}
</script>

<style scoped>
/* ================= 全局容器动画 ================= */
.profile-container {
  max-width: 1000px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 24px;
  padding-bottom: 40px;
}

.fade-in-up {
  animation: fadeInUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}

/* ================= 顶部卡片 & 背景 ================= */
.profile-header-card, .profile-content-card {
  border-radius: var(--radius-xl);
  border: 1px solid var(--border-color);
  background: var(--card-bg);
  box-shadow: var(--shadow-card);
  transition: box-shadow var(--duration-normal) var(--ease-out-expo);
  overflow: visible;
}

.profile-header-card:hover, .profile-content-card:hover {
  box-shadow: var(--shadow-card-hover);
}

.profile-banner {
  height: 140px;
  background: 
    radial-gradient(circle at top right, var(--accent-light), transparent 24%),
    linear-gradient(135deg, var(--primary-bg) 0%, #eef6ff 52%, var(--card-bg) 100%);
  position: relative;
  overflow: hidden;
}

/* 背景光晕装饰 */
.banner-decoration {
  position: absolute;
  top: -50px;
  right: -50px;
  width: 250px;
  height: 250px;
  background: radial-gradient(circle, rgba(255,255,255,0.2) 0%, rgba(255,255,255,0) 70%);
  border-radius: 50%;
  animation: pulseGlow 6s infinite alternate;
}

@keyframes pulseGlow {
  from { transform: scale(1); opacity: 0.6; }
  to { transform: scale(1.2); opacity: 1; }
}

/* ================= 头像与基础信息 ================= */
.profile-info-wrap {
  padding: 0 40px 32px;
  display: flex;
  align-items: center;
  gap: 24px;
  margin-top: -55px;
  position: relative;
  z-index: 10;
  justify-content: center;
}

/* 头像样式 */
.avatar-wrapper {
  position: relative;
  width: 100px;
  height: 100px;
  border-radius: 50%;
  cursor: pointer;
  flex-shrink: 0;
  transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
}

.avatar-wrapper:hover {
  transform: scale(1.05);
}

.avatar-wrapper:hover .user-avatar {
  box-shadow: 0 8px 20px rgba(45, 138, 110, 0.2);
}

.avatar-wrapper:hover .avatar-upload-btn {
  transform: scale(1.15);
  box-shadow: 0 6px 16px rgba(45, 138, 110, 0.4);
}

.user-avatar { 
  display: block; 
  border-radius: 50%;
  width: 100%;
  height: 100%;
  object-fit: cover;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.avatar-upload-btn {
  position: absolute;
  bottom: -4px;
  right: -4px;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--primary) 0%, var(--accent) 100%);
  border: 3px solid var(--card-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 14px;
  cursor: pointer;
  transition: all var(--duration-normal) ease;
  box-shadow: 0 4px 10px rgba(22, 93, 255, 0.3);
}

.user-main-info { flex: 1; padding-bottom: 8px; }

.user-name {
  margin: 0 0 12px 0;
  font-size: 26px;
  font-weight: 700;
  color: var(--text-main);
  display: flex;
  align-items: center;
  gap: 12px;
}

.gradient-tag {
  background: linear-gradient(90deg, var(--primary), var(--accent));
  border: none;
  font-weight: 500;
}

.user-meta { display: flex; align-items: center; gap: 16px; color: var(--text-light); font-size: 14px; }
.user-meta span { display: flex; align-items: center; gap: 6px; }

/* ================= 标签页与内容区 ================= */
::v-deep .custom-tabs .el-tabs__item {
  font-size: 16px;
  padding: 0 32px;
  height: 50px;
  line-height: 50px;
  transition: all 0.3s;
}
::v-deep .custom-tabs .el-tabs__item.is-active {
  font-weight: 600;
  font-size: 17px;
}
::v-deep .custom-tabs .el-tabs__nav-wrap::after { height: 1px; background-color: var(--border-light); }

.tab-content { padding: 32px; }

.content-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.section-title {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main);
  position: relative;
  padding-left: 12px;
}

.section-title::before {
  content: '';
  position: absolute;
  left: 0; top: 10%; height: 80%; width: 4px;
  background: var(--primary);
  border-radius: 2px;
}

/* ================= 通用交互组件 (Buttons & Inputs) ================= */
.hover-lift {
  transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
}
.hover-lift:hover {
  transform: translateY(-2px);
}

.primary-btn-glow {
  border: none;
  background: var(--primary);
  box-shadow: 0 4px 12px rgba(22, 93, 255, 0.2);
}
.primary-btn-glow:hover {
  background: var(--primary-hover);
  box-shadow: 0 8px 20px rgba(22, 93, 255, 0.35);
}

::v-deep .animated-input .el-input__wrapper,
::v-deep .animated-input .el-textarea__inner {
  border-radius: var(--radius-md);
  background-color: var(--input-bg);
  box-shadow: none !important;
  border: 1px solid transparent;
  transition: all var(--duration-normal) ease;
}

::v-deep .animated-input .el-input__wrapper:focus-within,
::v-deep .animated-input .el-textarea__inner:focus {
  background-color: var(--card-bg);
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(22, 93, 255, 0.1) !important;
  transform: scale(1.01);
}

/* ================= 查看模式 (Descriptions) ================= */
::v-deep .custom-descriptions {
  border-radius: 8px;
  overflow: hidden;
}
::v-deep .custom-descriptions .el-descriptions__label {
  width: 140px;
  background-color: var(--primary-bg);
  color: var(--text-regular);
  font-weight: 500;
}
.desc-text, .bio-text { color: var(--text-main); font-size: 14px; line-height: 1.6; }

/* ================= 安全列表 (交错动画) ================= */
.security-list { display: flex; flex-direction: column; gap: 16px; margin-top: 24px; }

.security-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px;
  border-radius: var(--radius-lg);
  background: var(--card-bg);
  border: 1px solid var(--border-light);
  transition: all var(--duration-normal) ease;

  /* 初始不可见，等待动画执行 */
  opacity: 0;
  animation: slideInRight 0.6s cubic-bezier(0.2, 0.8, 0.2, 1) forwards;
}

/* 通过 nth-child 设置不同的延迟，形成瀑布流入场感 */
.item-1 { animation-delay: 0.1s; }
.item-2 { animation-delay: 0.2s; }
.item-3 { animation-delay: 0.3s; }

@keyframes slideInRight {
  from { opacity: 0; transform: translateX(30px); }
  to { opacity: 1; transform: translateX(0); }
}

.hover-card-lift:hover {
  background: var(--card-bg);
  border-color: var(--border-color);
  transform: translateY(-4px);
  box-shadow: var(--shadow-card);
}

.sec-info { display: flex; flex-direction: column; gap: 8px; }
.sec-title { font-size: 16px; font-weight: 500; color: var(--text-main); }
.sec-desc { font-size: 13px; color: var(--text-light); }
.text-danger { color: var(--el-color-danger); }

.action-btn { transition: all 0.3s; border-radius: 6px; }
.hover-card-lift:hover .action-btn { background-color: var(--el-color-primary-light-9); }
.danger-zone:hover .action-btn { background-color: var(--el-color-danger-light-9); }

/* ================= 模式切换动画 ================= */
.fade-slide-enter-active, .fade-slide-leave-active { transition: all 0.4s cubic-bezier(0.2, 0.8, 0.2, 1); }
.fade-slide-enter-from { opacity: 0; transform: translateY(15px); }
.fade-slide-leave-to { opacity: 0; transform: translateY(-15px); }

/* ================= 头像上传对话框样式 ================= */
::v-deep .avatar-upload-dialog .el-dialog__header {
  background: linear-gradient(135deg, var(--primary-bg) 0%, #eef6ff 52%, var(--card-bg) 100%);
  border-bottom: none;
  padding: 20px 24px;
}

::v-deep .avatar-upload-dialog .el-dialog__title {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-main);
}

::v-deep .avatar-upload-dialog .el-dialog__body {
  padding: 24px;
}

.avatar-dialog-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.preview-section {
  text-align: center;
}

.preview-title {
  margin: 0 0 16px 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-regular);
}

.preview-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}

.main-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.preview-circle-large {
  width: 160px;
  height: 160px;
  border-radius: 50%;
  background-size: cover;
  background-position: center;
  border: 3px solid var(--border-color);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
}

.preview-size {
  font-size: 12px;
  color: var(--text-light);
}

.preview-sizes {
  display: flex;
  gap: 20px;
  justify-content: center;
}

.preview-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.preview-circle-medium {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background-size: cover;
  background-position: center;
  border: 2px solid var(--border-color);
}

.preview-circle-small {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background-size: cover;
  background-position: center;
  border: 2px solid var(--border-color);
}

.preview-circle-tiny {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background-size: cover;
  background-position: center;
  border: 2px solid var(--border-color);
}

.preview-item span {
  font-size: 11px;
  color: var(--text-light);
}

/* 上传区域 */
.upload-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.upload-area {
  width: 100%;
  padding: 16px;
  border: 2px dashed var(--border-color);
  border-radius: var(--radius-md);
  background: var(--card-bg);
  cursor: pointer;
  transition: all var(--duration-normal) ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.upload-area:hover {
  border-color: var(--primary);
  background: var(--primary-bg);
}

.upload-icon-small {
  font-size: 20px;
  color: var(--primary);
}

.upload-area span {
  font-size: 14px;
  color: var(--text-regular);
}

.upload-hint {
  font-size: 12px;
  color: var(--text-light);
}

.hidden-file-input {
  display: none;
}

.avatar-upload-input {
  display: none;
}
</style>