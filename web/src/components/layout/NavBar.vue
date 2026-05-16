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

        <router-link to="/announcements" class="nav-item">
          <i class="el-icon-bell"></i>公告
        </router-link>

        <router-link to="/works" class="nav-item">
          <i class="el-icon-picture-outline"></i>作品展厅
        </router-link>

        <div class="nav-item has-dropdown">
          <i class="el-icon-user"></i>用户中心
          <i class="el-icon-arrow-down nav-arrow"></i>
          <div class="nav-dropdown-panel">
            <router-link to="/account/space" class="dropdown-link">个人空间</router-link>
            <div class="dropdown-divider" v-if="hasPermission(['Admin', 'Teacher', 'Student'])"></div>
            <router-link v-if="hasPermission(['Admin', 'Teacher', 'Student'])" to="/works/manage" class="dropdown-link">作品管理</router-link>
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
        return `/api/File/download?fileName=${encodeURIComponent(this.currentUser.avatar)}&t=${Date.now()}`
      }
      return null
    },
    handleCommand(command) {
      if (command === 'profile') {
        this.$router.push('/account/space')
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
        eventBus.$emit('notification-panel-opened')
      }
    },
    handleNotificationCountUpdate(count) {
      if (count !== undefined) {
        this.unreadCount = count
      } else {
        this.loadUnreadCount()
      }
    },
    async handleRoleSwitch(username) {
      const password = '123456'
      const loadingInstance = this.$loading({
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

        this.currentUser.username = data.user.username || ''
        this.currentUser.role = data.user.role || ''
        this.currentUser.avatar = data.user.avatar || ''

        this.loadUnreadCount()

        eventBus.$emit('user-info-updated', data.user)

        this.$message.success(`已切换为 ${data.user.name || data.user.username} 身份`)

        window.location.reload()
      } catch (error) {
        const errorMessage = error.response?.data?.message || error.message || '切换身份失败'
        this.$message.error(errorMessage)
      } finally {
        loadingInstance.close()
      }
    }
  }
}
</script>

<style scoped>
.modern-header {
  height: 64px;
  background: var(--header-bg);
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  border-bottom: 1px solid var(--border-color);
  box-shadow: var(--shadow-xs);
  display: flex;
  justify-content: center;
  position: sticky;
  top: 0;
  z-index: 101;
  flex-shrink: 0;
}

.header-content {
  width: 100%;
  max-width: 1440px;
  padding: 0 28px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.brand-area {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
}

.logo-mark {
  width: 36px;
  height: 36px;
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 18px;
  box-shadow: 0 2px 8px var(--primary-glow);
}

.logo-text {
  font-size: 18px;
  font-weight: 700;
  margin: 0;
  color: var(--text-main);
  letter-spacing: -0.3px;
}

.school-tag {
  font-size: 11px;
  padding: 3px 10px;
  background-color: var(--primary-bg);
  border-radius: var(--radius-full);
  color: var(--primary);
  font-weight: 600;
  letter-spacing: 0.3px;
}

.center-nav {
  display: flex;
  align-items: center;
  height: 100%;
  gap: 2px;
}

.nav-item {
  height: 40px;
  display: flex;
  align-items: center;
  padding: 0 14px;
  color: var(--text-regular);
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
  border-radius: var(--radius-sm);
  transition: all var(--duration-normal) var(--ease-out-expo);
  cursor: pointer;
  position: relative;
  letter-spacing: -0.1px;
}

.nav-item i {
  margin-right: 6px;
  font-size: 16px;
}

.nav-arrow {
  margin-left: 4px;
  margin-right: 0 !important;
  font-size: 11px !important;
  transition: transform var(--duration-normal) var(--ease-out-expo);
}

.nav-item:hover {
  background-color: var(--primary-bg);
  color: var(--primary);
}

.nav-dropdown-panel {
  position: absolute;
  top: 48px;
  left: 50%;
  transform: translateX(-50%) translateY(8px);
  background: var(--card-bg);
  min-width: 180px;
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  border: 1px solid var(--border-color);
  padding: 6px;
  opacity: 0;
  visibility: hidden;
  transition: all var(--duration-normal) var(--ease-out-expo);
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
  padding: 9px 14px;
  color: var(--text-main);
  text-decoration: none;
  font-size: 13px;
  font-weight: 500;
  border-radius: var(--radius-sm);
  transition: all var(--duration-fast) var(--ease-in-out);
}

.dropdown-link:hover {
  background-color: var(--primary-bg);
  color: var(--primary);
}

.dropdown-divider {
  height: 1px;
  background-color: var(--border-color);
  margin: 6px 8px;
}

.user-dropdown-wrap {
  cursor: pointer;
}

.user-profile-trigger {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 5px 14px 5px 5px;
  border-radius: var(--radius-full);
  transition: background var(--duration-fast) var(--ease-in-out);
  border: 1px solid transparent;
}

.user-profile-trigger:hover {
  background-color: var(--bg-hover);
  border-color: var(--border-color);
}

.custom-avatar {
  background: transparent;
  color: white;
}

.user-info-text {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.user-name {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-main);
  line-height: 1.3;
}

.user-role {
  font-size: 11px;
  color: var(--text-light);
  line-height: 1.3;
  margin-top: 1px;
}

.modern-dropdown-menu {
  border-radius: var(--radius-md);
  padding: 6px;
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-lg);
  z-index: 110;
}

.modern-dropdown-menu .el-dropdown-menu__item {
  padding: 8px 16px;
  font-size: 13px;
  border-radius: var(--radius-sm);
  transition: all var(--duration-fast) var(--ease-in-out);
}

.modern-dropdown-menu .el-dropdown-menu__item:hover {
  background-color: var(--primary-bg);
  color: var(--primary);
}

.danger-text {
  color: #D44444 !important;
}

.danger-text:hover {
  background-color: #FEF2F2 !important;
  color: #D44444 !important;
}

.user-actions {
  display: flex;
  align-items: center;
  gap: 6px;
}

.notification-btn {
  position: relative;
  width: 38px;
  height: 38px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 8px;
  border-radius: var(--radius-full);
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-in-out);
  color: var(--text-regular);
  font-size: 18px;
}

.notification-btn:hover {
  background-color: var(--bg-hover);
  color: var(--primary);
}

.notification-badge {
  position: absolute;
  top: 4px;
  right: 4px;
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  background-color: #D44444;
  color: white;
  font-size: 11px;
  font-weight: 600;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 6px rgba(212, 68, 68, 0.3);
}

.notification-panel {
  position: fixed;
  top: 72px;
  right: 24px;
  z-index: 1000;
}

.role-switch-wrap {
  cursor: pointer;
}

.role-switch-btn {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 5px 12px;
  border-radius: var(--radius-full);
  background: linear-gradient(135deg, var(--accent) 0%, #C87D2A 100%);
  color: white;
  transition: all var(--duration-fast) var(--ease-out-expo);
  font-size: 12px;
  font-weight: 600;
  box-shadow: 0 2px 8px rgba(212, 145, 62, 0.25);
}

.role-switch-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 16px rgba(212, 145, 62, 0.4);
}

.role-switch-btn i {
  font-size: 14px;
}

.role-text {
  padding-right: 2px;
}

.role-switch-menu {
  min-width: 200px;
  border-radius: var(--radius-md);
  padding: 6px;
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-lg);
}

.role-switch-menu .dropdown-group {
  padding: 0;
}

.role-switch-menu .dropdown-group-title {
  padding: 6px 16px;
  font-size: 11px;
  color: var(--text-light);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.role-switch-menu .el-dropdown-menu__item {
  padding: 9px 16px;
  font-size: 13px;
  display: flex;
  align-items: center;
  gap: 8px;
  border-radius: var(--radius-sm);
  transition: all var(--duration-fast) var(--ease-in-out);
}

.role-switch-menu .el-dropdown-menu__item:hover {
  background-color: var(--primary-bg);
  color: var(--primary);
}

.role-switch-menu .el-dropdown-menu__item i {
  font-size: 15px;
}
</style>