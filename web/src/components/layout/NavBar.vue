<template>
  <header class="modern-header">
    <div class="header-content">
      <div class="brand-area" @click="$router.push('/works/explore')">
        <div class="logo-mark">
          <i class="el-icon-monitor"></i>
        </div>
        <h1 class="logo-text">数字作品管理系统</h1>
        <span class="school-tag">保定学院</span>
      </div>

      <nav class="center-nav">
        <router-link to="/works/explore" class="nav-item">
          <i class="el-icon-s-home"></i>首页
        </router-link>

        <router-link to="/account/announcements" class="nav-item">
          <i class="el-icon-bell"></i>系统公告
        </router-link>

        <router-link to="/works" class="nav-item">
          <i class="el-icon-picture-outline"></i>作品展厅
        </router-link>

        <div class="nav-item has-dropdown">
          <i class="el-icon-user"></i>用户中心
          <i class="el-icon-arrow-down nav-arrow"></i>
          <div class="nav-dropdown-panel">
            <router-link to="/account/profile" class="dropdown-link">个人信息</router-link>
            <router-link to="/account/favorites" class="dropdown-link">我的收藏</router-link>
            <router-link to="/account/works" class="dropdown-link">个人作品集</router-link>
            <router-link to="/account/announcements" class="dropdown-link">系统公告</router-link>
            <div class="dropdown-divider" v-if="hasPermission(['Admin', 'Teacher'])"></div>
            <router-link v-if="hasPermission(['Admin', 'Teacher'])" to="/works/manage" class="dropdown-link">作品管理</router-link>
            <router-link to="/account/feedback" class="dropdown-link">意见反馈</router-link>
            <router-link to="/account/settings" class="dropdown-link">偏好设置</router-link>
          </div>
        </div>

        <div class="nav-item has-dropdown" v-if="hasPermission(['Admin', 'Teacher'])">
          <i class="el-icon-data-line"></i>数据分析
          <i class="el-icon-arrow-down nav-arrow"></i>
          <div class="nav-dropdown-panel">
            <router-link to="/admin/analytics" class="dropdown-link">全局数据统计</router-link>
          </div>
        </div>

        <div class="nav-item has-dropdown" v-if="hasPermission(['Admin', 'Teacher'])">
          <i class="el-icon-connection"></i>教学协同
          <i class="el-icon-arrow-down nav-arrow"></i>
          <div class="nav-dropdown-panel">
            <router-link to="/teaching/overview" class="dropdown-link">教学协同总览</router-link>
            <router-link to="/teaching/students" class="dropdown-link">我的学生</router-link>
            <router-link to="/teaching/works" class="dropdown-link">学生作品管理</router-link>
          </div>
        </div>

        <div class="nav-item has-dropdown" v-if="hasPermission(['Admin'])">
          <i class="el-icon-setting"></i>系统管理
          <i class="el-icon-arrow-down nav-arrow"></i>
          <div class="nav-dropdown-panel">
            <router-link to="/admin/users" class="dropdown-link">用户与权限</router-link>
            <router-link to="/admin/moderation" class="dropdown-link">内容审核与安全</router-link>
            <router-link to="/admin/announcements" class="dropdown-link">公告管理</router-link>
            <router-link to="/admin/feedbacks" class="dropdown-link">反馈管理</router-link>
          </div>
        </div>

        <router-link to="/lab/model-test" class="nav-item">
          <i class="el-icon-cpu"></i>模型测试
        </router-link>
      </nav>

      <div class="user-actions">
        <!-- 角色切换按钮 -->
        <el-dropdown trigger="click" @command="handleRoleSwitch" class="role-switch-wrap">
          <div class="role-switch-btn">
            <i class="el-icon-switch-button"></i>
            <span class="role-text">切换身份</span>
            <i class="el-icon-arrow-down el-icon--right"></i>
          </div>
          <el-dropdown-menu slot="dropdown" class="role-switch-menu">
            <template>
              <div class="dropdown-group">
                <div class="dropdown-group-title">管理员</div>
                <el-dropdown-item command="admin" icon="el-icon-user">admin (系统管理员)</el-dropdown-item>
              </div>
              <div class="dropdown-group">
                <div class="dropdown-group-title">教师</div>
                <el-dropdown-item command="teacher_zhang" icon="el-icon-user-solid">teacher_zhang (张老师)</el-dropdown-item>
                <el-dropdown-item command="teacher_li" icon="el-icon-user-solid">teacher_li (李老师)</el-dropdown-item>
              </div>
              <div class="dropdown-group">
                <div class="dropdown-group-title">学生</div>
                <el-dropdown-item command="student_wang" icon="el-icon-user">student_wang (王晨)</el-dropdown-item>
                <el-dropdown-item command="student_liu" icon="el-icon-user">student_liu (刘悦)</el-dropdown-item>
                <el-dropdown-item command="student_chen" icon="el-icon-user">student_chen (陈思)</el-dropdown-item>
                <el-dropdown-item command="student_zhao" icon="el-icon-user">student_zhao (赵宁)</el-dropdown-item>
              </div>
            </template>
          </el-dropdown-menu>
        </el-dropdown>

        <!-- 通知按钮 -->
        <div class="notification-btn" @click="toggleNotificationPanel">
          <i class="el-icon-bell"></i>
          <span class="notification-badge" v-if="unreadCount > 0">{{ unreadCount }}</span>
        </div>

        <el-dropdown trigger="click" @command="handleCommand" class="user-dropdown-wrap">
          <div class="user-profile-trigger">
            <el-avatar :size="32" :src="getAvatarUrl()" icon="el-icon-user-solid" class="custom-avatar"></el-avatar>
            <div class="user-info-text">
              <span class="user-name">{{ currentUser.username }}</span>
              <span class="user-role">{{ translateRole(currentUser.role) }}</span>
            </div>
            <i class="el-icon-arrow-down el-icon--right"></i>
          </div>
          <el-dropdown-menu slot="dropdown" class="modern-dropdown-menu">
            <el-dropdown-item command="profile" icon="el-icon-user">个人中心</el-dropdown-item>
            <el-dropdown-item command="settings" icon="el-icon-setting">偏好设置</el-dropdown-item>
            <el-dropdown-item divided command="logout" icon="el-icon-switch-button" class="danger-text">退出登录</el-dropdown-item>
          </el-dropdown-menu>
        </el-dropdown>
      </div>
    </div>

    <!-- 通知面板 -->
    <div class="notification-panel" v-if="showNotificationPanel" @click.self="showNotificationPanel = false">
      <NotificationCenter @update-count="handleNotificationCountUpdate" />
    </div>
  </header>
</template>

<script>
import eventBus from '../../utils/eventBus'
import NotificationCenter from '../NotificationCenter.vue'
import { notificationApi } from '../../services/api'
import http from '../../utils/http'
import { setToken, setUser, clearAuth } from '../../utils/auth'

export default {
  name: 'NavBar',
  components: {
    NotificationCenter
  },
  data() {
    return {
      currentUser: {
        username: '',
        role: '',
        avatar: ''
      },
      showNotificationPanel: false,
      unreadCount: 0
    }
  },
  computed: {
    isLoggedIn() {
      return !!localStorage.getItem('token')
    }
  },
  mounted() {
    this.loadUserInfo()
    this.loadUnreadCount()
    // 监听用户信息更新事件，实现头像实时更新
    eventBus.$on('user-info-updated', this.handleUserInfoUpdated)
    eventBus.$on('avatar-updated', this.handleAvatarUpdated)
  },
  beforeDestroy() {
    eventBus.$off('user-info-updated', this.handleUserInfoUpdated)
    eventBus.$off('avatar-updated', this.handleAvatarUpdated)
  },
  methods: {
    loadUserInfo() {
      try {
        const stored = localStorage.getItem('userInfo')
        if (stored) {
          const parsed = JSON.parse(stored)
          this.currentUser.username = parsed.username || ''
          this.currentUser.role = parsed.role || ''
          this.currentUser.avatar = parsed.avatar || ''
        }
      } catch (e) {
        console.warn('Failed to parse userInfo from localStorage', e)
      }
    },
    handleUserInfoUpdated(userInfo) {
      console.log('NavBar: user-info-updated', userInfo)
      if (userInfo) {
        this.currentUser.username = userInfo.username || ''
        this.currentUser.role = userInfo.role || ''
        this.currentUser.avatar = userInfo.avatar || ''
        // 同步到 localStorage
        const stored = localStorage.getItem('userInfo')
        const current = stored ? JSON.parse(stored) : {}
        Object.assign(current, userInfo)
        localStorage.setItem('userInfo', JSON.stringify(current))
      }
    },
    handleAvatarUpdated(fileName) {
      console.log('NavBar: avatar-updated', fileName)
      if (fileName) {
        this.currentUser.avatar = fileName
        // 同步到 localStorage
        const stored = localStorage.getItem('userInfo')
        const current = stored ? JSON.parse(stored) : {}
        current.avatar = fileName
        localStorage.setItem('userInfo', JSON.stringify(current))
      }
    },
    hasPermission(roles) {
      const userRole = this.currentUser.role
      return userRole === 'Admin' || roles.includes(userRole)
    },
    translateRole(role) {
      const roleMap = {
        Admin: '超级管理员',
        Teacher: '指导教师',
        Student: '学生',
        Guest: '访客'
      }
      return roleMap[role] || role
    },
    getAvatarUrl() {
      if (this.currentUser.avatar) {
        return `/api/File/download?fileName=${encodeURIComponent(this.currentUser.avatar)}`
      }
      return null
    },
    handleCommand(command) {
      if (command === 'profile') {
        this.$router.push('/account/profile')
        return
      }
      if (command === 'settings') {
        this.$router.push('/account/settings')
        return
      }
      if (command === 'logout') {
        this.handleLogout()
      }
    },
    async handleLogout() {
      try {
        await this.$confirm('确定要退出当前账号吗？', '退出确认', {
          confirmButtonText: '确定退出',
          cancelButtonText: '取消',
          type: 'warning'
        })
        localStorage.removeItem('token')
        localStorage.removeItem('userInfo')
        this.$message.success('已安全退出系统')
        this.$router.push('/auth/login')
      } catch (error) {
        // ignore cancel
      }
    },
    async loadUnreadCount() {
      try {
        const res = await notificationApi.getUnreadCount()
        this.unreadCount = res.data?.count || 0
      } catch (error) {
        console.error('加载未读通知数量失败:', error)
      }
    },
    toggleNotificationPanel() {
      this.showNotificationPanel = !this.showNotificationPanel
      if (this.showNotificationPanel) {
        // 打开面板后重新加载通知列表
        eventBus.$emit('notification-panel-opened')
      }
    },
    handleNotificationCountUpdate(count) {
      if (count !== undefined) {
        this.unreadCount = count
      } else {
        // 如果没有传具体数值，重新获取
        this.loadUnreadCount()
      }
    },
    async handleRoleSwitch(username) {
      const password = '123456'
      this.$loading({
        lock: true,
        text: '正在切换身份...',
        spinner: 'el-icon-loading',
        background: 'rgba(0, 0, 0, 0.7)'
      })

      try {
        const response = await http.post('/api/Auth/login', {
          Username: username,
          Password: password,
          Remember: false
        })

        const { data } = response
        if (!data?.user) {
          throw new Error('登录返回数据不完整')
        }

        if (data.token) {
          setToken(data.token)
        } else {
          clearAuth()
        }
        setUser(data.user)

        // 更新当前用户信息
        this.currentUser.username = data.user.username || ''
        this.currentUser.role = data.user.role || ''
        this.currentUser.avatar = data.user.avatar || ''

        // 刷新通知数量
        this.loadUnreadCount()

        // 触发用户信息更新事件
        eventBus.$emit('user-info-updated', data.user)

        this.$message.success(`已切换为 ${data.user.name || data.user.username} 身份`)

        // 刷新当前页面以应用新身份的权限
        window.location.reload()
      } catch (error) {
        const errorMessage = error.response?.data?.message || error.message || '切换身份失败'
        this.$message.error(errorMessage)
      } finally {
        this.$loading().close()
      }
    }
  }
}
</script>

<style scoped>
.modern-header {
  height: 60px;
  background-color: #ffffff;
  border-bottom: 1px solid #e5e6eb;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.04);
  display: flex;
  justify-content: center;
  position: relative;
  z-index: 101;
  flex-shrink: 0;
}

.header-content {
  width: 100%;
  max-width: 1440px;
  padding: 0 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.brand-area {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.logo-mark {
  width: 32px;
  height: 32px;
  background-color: #165dff;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 18px;
}

.logo-text {
  font-size: 18px;
  font-weight: 600;
  margin: 0;
  color: #1d2129;
  letter-spacing: 0.5px;
}

.school-tag {
  font-size: 12px;
  padding: 2px 6px;
  background-color: #f5f7fa;
  border-radius: 4px;
  color: #4e5969;
  border: 1px solid #e5e6eb;
}

.center-nav {
  display: flex;
  align-items: center;
  height: 100%;
  gap: 4px;
}

.nav-item {
  height: 40px;
  display: flex;
  align-items: center;
  padding: 0 16px;
  color: #4e5969;
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
  border-radius: 6px;
  transition: all 0.2s ease;
  cursor: pointer;
  position: relative;
}

.nav-item i {
  margin-right: 6px;
  font-size: 16px;
}

.nav-arrow {
  margin-left: 4px;
  margin-right: 0 !important;
  font-size: 12px !important;
  transition: transform 0.3s ease;
}

.nav-item:hover {
  background-color: #e8f3ff;
  color: #165dff;
}

.nav-dropdown-panel {
  position: absolute;
  top: 48px;
  left: 50%;
  transform: translateX(-50%) translateY(10px);
  background: white;
  min-width: 170px;
  border-radius: 8px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.08);
  border: 1px solid #e5e6eb;
  padding: 8px 0;
  opacity: 0;
  visibility: hidden;
  transition: all 0.2s cubic-bezier(0.2, 0.8, 0.2, 1);
  z-index: 510;
}

.has-dropdown:hover .nav-dropdown-panel {
  opacity: 1;
  visibility: visible;
  transform: translateX(-50%) translateY(0);
}

.has-dropdown:hover .nav-arrow {
  transform: rotate(180deg);
}

.dropdown-link {
  display: block;
  padding: 10px 20px;
  color: #1d2129;
  text-decoration: none;
  font-size: 14px;
  transition: background 0.2s;
}

.dropdown-link:hover {
  background-color: #f5f7fa;
  color: #165dff;
}

.dropdown-divider {
  height: 1px;
  background-color: #e5e6eb;
  margin: 8px 0;
}

.user-dropdown-wrap {
  cursor: pointer;
}

.user-profile-trigger {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 12px;
  border-radius: 24px;
  transition: background 0.2s;
}

.user-profile-trigger:hover {
  background-color: #f5f7fa;
}

.custom-avatar {
  background-color: #165dff;
  color: white;
}

.user-info-text {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.user-name {
  font-size: 14px;
  font-weight: 500;
  color: #1d2129;
  line-height: 1.2;
}

.user-role {
  font-size: 12px;
  color: #86909c;
  line-height: 1.2;
  margin-top: 2px;
}

.modern-dropdown-menu {
  border-radius: 8px;
  padding: 8px 0;
  border: 1px solid #e5e6eb;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.08);
  z-index: 110;
}

.modern-dropdown-menu .el-dropdown-menu__item {
  padding: 8px 24px;
  font-size: 14px;
}

.danger-text {
  color: #f53f3f !important;
}

.danger-text:hover {
  background-color: #ffece8 !important;
  color: #f53f3f !important;
}

.user-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.notification-btn {
  position: relative;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  color: #4e5969;
  font-size: 18px;
}

.notification-btn:hover {
  background-color: #f5f7fa;
  color: #165dff;
}

.notification-badge {
  position: absolute;
  top: 6px;
  right: 6px;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  background-color: #f53f3f;
  color: white;
  font-size: 12px;
  font-weight: 500;
  border-radius: 9px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.notification-panel {
  position: fixed;
  top: 70px;
  right: 20px;
  z-index: 1000;
}

/* 角色切换按钮样式 */
.role-switch-wrap {
  cursor: pointer;
}

.role-switch-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 24px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  transition: all 0.2s ease;
  font-size: 13px;
  font-weight: 500;
}

.role-switch-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.role-switch-btn i {
  font-size: 14px;
}

.role-text {
  padding-right: 4px;
}

.role-switch-menu {
  min-width: 200px;
  border-radius: 8px;
  padding: 8px 0;
  border: 1px solid #e5e6eb;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.08);
}

.role-switch-menu .dropdown-group {
  padding: 0;
}

.role-switch-menu .dropdown-group-title {
  padding: 6px 20px;
  font-size: 12px;
  color: #86909c;
  font-weight: 500;
  background-color: #f5f7fa;
}

.role-switch-menu .el-dropdown-menu__item {
  padding: 10px 20px;
  font-size: 14px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.role-switch-menu .el-dropdown-menu__item:hover {
  background-color: #e8f3ff;
  color: #165dff;
}

.role-switch-menu .el-dropdown-menu__item i {
  font-size: 16px;
}
</style>
