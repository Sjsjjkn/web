<template>
  <div class="settings-container">
    <div class="settings-header">
      <h2 class="page-title">偏好设置</h2>
      <p class="page-desc">管理您的账户偏好和系统设置</p>
    </div>

    <!-- 通知设置 -->
    <div class="settings-card">
      <h3 class="section-title">通知设置</h3>
      <el-form label-width="160px" class="settings-form">
        <el-form-item label="消息通知">
          <el-switch v-model="prefs.notificationEnabled" active-color="var(--primary)" @change="autoSavePrefs"></el-switch>
          <span class="form-item-tip">关闭后将不再接收系统内部消息通知</span>
        </el-form-item>
        <el-form-item label="邮件通知">
          <el-switch v-model="prefs.emailNotification" active-color="var(--primary)" @change="autoSavePrefs"></el-switch>
          <span class="form-item-tip">接收作品审核结果、系统公告等邮件通知</span>
        </el-form-item>
      </el-form>
    </div>

    <!-- 隐私设置 -->
    <div class="settings-card">
      <h3 class="section-title">隐私设置</h3>
      <el-form label-width="160px" class="settings-form">
        <el-form-item label="个人主页公开">
          <el-switch v-model="prefs.profilePublic" active-color="var(--primary)" @change="autoSavePrefs"></el-switch>
          <span class="form-item-tip">关闭后其他用户将无法查看您的个人主页</span>
        </el-form-item>
        <el-form-item label="展示联系方式">
          <el-switch v-model="prefs.showContactInfo" active-color="var(--primary)" @change="autoSavePrefs"></el-switch>
          <span class="form-item-tip">开启后其他用户可在您的主页看到联系方式</span>
        </el-form-item>
        <el-form-item label="收藏可见性">
          <el-select v-model="prefs.favoritesVisibility" placeholder="选择可见范围" style="width: 180px" @change="autoSavePrefs">
            <el-option label="所有人可见" value="public"></el-option>
            <el-option label="仅关注者可见" value="followers"></el-option>
            <el-option label="仅自己可见" value="private"></el-option>
          </el-select>
        </el-form-item>
      </el-form>
    </div>

    <!-- 界面设置 -->
    <div class="settings-card">
      <h3 class="section-title">界面设置</h3>
      <el-form label-width="160px" class="settings-form">
        <el-form-item label="界面主题">
          <el-radio-group v-model="prefs.theme" @change="handleThemeChange">
            <el-radio label="light">浅色模式</el-radio>
            <el-radio label="dark">深色模式</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="界面语言">
          <el-radio-group v-model="prefs.language" @change="autoSavePrefs">
            <el-radio label="zh-CN">中文</el-radio>
            <el-radio label="en">English</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="默认首页">
          <el-select v-model="localPrefs.defaultPage" placeholder="请选择" style="width: 180px" @change="saveLocalPrefs">
            <el-option label="作品探索" value="explore"></el-option>
            <el-option label="作品展厅" value="gallery"></el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="每页显示数量">
          <el-radio-group v-model="localPrefs.pageSize" @change="saveLocalPrefs">
            <el-radio :label="12">12</el-radio>
            <el-radio :label="24">24</el-radio>
            <el-radio :label="48">48</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
    </div>

    <!-- 安全设置 -->
    <div class="settings-card">
      <h3 class="section-title">安全设置</h3>
      <el-form label-width="160px" class="settings-form">
        <el-form-item label="修改密码">
          <el-button type="primary" size="small" @click="showPasswordDialog = true">修改密码</el-button>
        </el-form-item>
      </el-form>
    </div>

    <div class="settings-actions">
      <el-button type="primary" @click="saveAllSettings" :loading="saving">
        {{ saving ? '保存中...' : '保存全部设置' }}
      </el-button>
      <el-button @click="resetSettings">恢复默认</el-button>
    </div>

    <!-- 修改密码对话框 -->
    <el-dialog
      title="修改密码"
      :visible.sync="showPasswordDialog"
      width="420px"
      class="modern-dialog"
      @closed="resetPasswordForm"
    >
      <el-form :model="passwordForm" :rules="passwordRules" ref="passwordFormRef" label-width="100px">
        <el-form-item label="当前密码" prop="currentPassword">
          <el-input v-model="passwordForm.currentPassword" type="password" placeholder="请输入当前密码"></el-input>
        </el-form-item>
        <el-form-item label="新密码" prop="newPassword">
          <el-input v-model="passwordForm.newPassword" type="password" placeholder="请输入新密码（6-20位）"></el-input>
        </el-form-item>
        <el-form-item label="确认密码" prop="confirmPassword">
          <el-input v-model="passwordForm.confirmPassword" type="password" placeholder="请确认新密码"></el-input>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button @click="showPasswordDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmChangePassword" :loading="changingPassword">确认修改</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import { authApi } from '@/services/api'
import { eventBus } from '@/utils/eventBus'

export default {
  name: 'Settings',
  data() {
    return {
      // 后端存储的偏好设置
      prefs: {
        theme: 'light',
        language: 'zh-CN',
        notificationEnabled: true,
        emailNotification: true,
        profilePublic: true,
        showContactInfo: false,
        favoritesVisibility: 'public'
      },
      // 前端本地偏好（仅存 localStorage）
      localPrefs: {
        defaultPage: 'explore',
        pageSize: 12
      },
      showPasswordDialog: false,
      saving: false,
      changingPassword: false,
      passwordForm: {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      },
      passwordRules: {
        currentPassword: [
          { required: true, message: '请输入当前密码', trigger: 'blur' }
        ],
        newPassword: [
          { required: true, message: '请输入新密码', trigger: 'blur' },
          { min: 6, max: 20, message: '密码长度在 6 到 20 位之间', trigger: 'blur' }
        ],
        confirmPassword: [
          { required: true, message: '请确认新密码', trigger: 'blur' },
          { validator: this.validateConfirmPassword, trigger: 'blur' }
        ]
      },
      // 标记是否已从后端加载过
      backendLoaded: false
    }
  },
  methods: {
    // ───── 加载设置 ─────
    async loadSettings() {
      try {
        // 加载本地偏好
        this.loadLocalPrefs()

        // 从后端加载偏好设置
        const res = await authApi.getPreferences()
        if (res && res.data) {
          const data = res.data
          // 可单独返回 preferences 字段或直接返回
          const p = data.preferences || data
          if (p.theme !== undefined) this.prefs.theme = p.theme
          if (p.language !== undefined) this.prefs.language = p.language
          this.prefs.notificationEnabled = p.notificationEnabled ?? true
          this.prefs.emailNotification = p.emailNotification ?? true
          this.prefs.profilePublic = p.profilePublic ?? true
          this.prefs.showContactInfo = p.showContactInfo ?? false
          if (p.favoritesVisibility !== undefined) this.prefs.favoritesVisibility = p.favoritesVisibility
          this.backendLoaded = true
        }
      } catch (error) {
        console.warn('加载偏好设置失败，使用默认值:', error.message || error)
        // 不弹错误，静默使用默认值
      }
    },

    loadLocalPrefs() {
      try {
        const raw = localStorage.getItem('userLocalSettings')
        if (raw) {
          const parsed = JSON.parse(raw)
          if (parsed.defaultPage) this.localPrefs.defaultPage = parsed.defaultPage
          if (parsed.pageSize) this.localPrefs.pageSize = parsed.pageSize
        }
      } catch { /* ignore */ }
    },

    saveLocalPrefs() {
      localStorage.setItem('userLocalSettings', JSON.stringify(this.localPrefs))
    },

    // ───── 保存设置 ─────
    async autoSavePrefs() {
      // 后端偏好自动保存（切换开关时即保存）
      if (!this.backendLoaded) return
      try {
        await authApi.updatePreferences(this.prefs)
        this.$message.success('偏好已自动保存')
      } catch (error) {
        this.$message.error(error.response?.data?.message || '保存失败')
      }
    },

    async saveAllSettings() {
      this.saving = true
      try {
        await authApi.updatePreferences(this.prefs)
        this.saveLocalPrefs()
        this.$message.success('所有设置已保存')
      } catch (error) {
        this.$message.error(error.response?.data?.message || '保存失败')
      } finally {
        this.saving = false
      }
    },

    handleThemeChange(value) {
      // 触发主题变更事件，App.vue 或全局可监听
      document.documentElement.setAttribute('data-theme', value)
      localStorage.setItem('theme', value)
      eventBus.$emit('theme-changed', value)
      this.autoSavePrefs()
    },

    resetSettings() {
      this.prefs = {
        theme: 'light',
        language: 'zh-CN',
        notificationEnabled: true,
        emailNotification: true,
        profilePublic: true,
        showContactInfo: false,
        favoritesVisibility: 'public'
      }
      this.localPrefs = {
        defaultPage: 'explore',
        pageSize: 12
      }
      document.documentElement.setAttribute('data-theme', 'light')
      localStorage.setItem('theme', 'light')
      this.saveLocalPrefs()
      // 同步重置后端
      authApi.updatePreferences(this.prefs).catch(() => {})
      this.$message.success('已恢复默认设置')
    },

    // ───── 密码修改 ─────
    validateConfirmPassword(rule, value, callback) {
      if (value !== this.passwordForm.newPassword) {
        callback(new Error('两次输入的密码不一致'))
      } else {
        callback()
      }
    },

    resetPasswordForm() {
      this.$refs.passwordFormRef && this.$refs.passwordFormRef.resetFields()
    },

    async confirmChangePassword() {
      const valid = await this.$refs.passwordFormRef.validate().catch(() => null)
      if (!valid) return

      this.changingPassword = true
      try {
        await authApi.updateProfile({
          currentPassword: this.passwordForm.currentPassword,
          newPassword: this.passwordForm.newPassword
        })
        this.$message.success('密码修改成功，请重新登录')
        this.showPasswordDialog = false
        // 清除登录状态，跳转登录页
        localStorage.removeItem('token')
        localStorage.removeItem('userInfo')
        this.$router.push('/auth/login')
      } catch (error) {
        this.$message.error(error.response?.data?.message || '密码修改失败')
      } finally {
        this.changingPassword = false
      }
    }
  },
  mounted() {
    this.loadSettings()
    // 应用当前主题
    document.documentElement.setAttribute('data-theme', this.prefs.theme)
  }
}
</script>

<style scoped>
.settings-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 16px 40px;
}

.settings-header {
  margin-bottom: 32px;
}

.page-title {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 8px 0;
  letter-spacing: -0.3px;
}

.page-desc {
  font-size: 14px;
  color: var(--text-light);
  margin: 0;
}

.settings-card {
  background: var(--card-bg);
  border-radius: var(--radius-lg);
  padding: 28px 32px;
  margin-bottom: 20px;
  box-shadow: var(--shadow-sm);
  border: 1px solid var(--border-color);
  transition: box-shadow var(--duration-normal) var(--ease-out-expo);
}

.settings-card:hover {
  box-shadow: var(--shadow-card);
}

.section-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 20px 0;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--border-light);
  position: relative;
}

.section-title::after {
  content: '';
  position: absolute;
  bottom: -1px;
  left: 0;
  width: 40px;
  height: 3px;
  background: linear-gradient(90deg, var(--primary), var(--accent));
  border-radius: var(--radius-full);
}

.settings-form {
  margin-top: 8px;
}

.form-item-tip {
  display: inline-block;
  margin-left: 12px;
  font-size: 12px;
  color: var(--text-light);
}

.settings-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 32px;
}

::v-deep .modern-dialog .el-dialog {
  border-radius: var(--radius-lg);
}
</style>
