<template>
  <div id="app">
    <template v-if="!isAuthPage">
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
                <div class="dropdown-divider" v-if="hasPermission(['Admin', 'Teacher'])"></div>
                <router-link v-if="hasPermission(['Admin', 'Teacher'])" to="/works/manage" class="dropdown-link">作品管理</router-link>
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
              </div>
            </div>
          </nav>

          <div class="user-actions">
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
      </header>

      <div class="main-layout">
        <main class="content-wrapper">
          <transition name="fade-transform" mode="out-in">
            <router-view />
          </transition>
        </main>
      </div>
    </template>

    <template v-else>
      <router-view />
    </template>
  </div>
</template>

<script>
export default {
  name: 'App',
  computed: {
    isAuthPage() {
      return this.$route.path === '/' || this.$route.path === '/auth/login' || this.$route.path === '/auth/register'
    },
    currentUser() {
      const userInfo = localStorage.getItem('userInfo')
      return userInfo ? JSON.parse(userInfo) : { username: '未登录', role: 'Guest' }
    }
  },
  methods: {
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
        this.$message.info('偏好设置开发中')
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
    }
  }
}
</script>

<style>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap');

:root {
  --app-bg: #f5f7fa;
  --header-bg: #ffffff;
  --text-main: #1d2129;
  --text-regular: #4e5969;
  --text-light: #86909c;
  --primary: #165dff;
  --primary-hover: #e8f3ff;
  --border-color: #e5e6eb;
  --shadow-sm: 0 2px 5px rgba(0, 0, 0, 0.04);
  --shadow-md: 0 4px 10px rgba(0, 0, 0, 0.08);
}

body, html, #app {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100vh;
  background-color: var(--app-bg);
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  color: var(--text-main);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.modern-header {
  height: 60px;
  background-color: var(--header-bg);
  border-bottom: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
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
  background-color: var(--primary);
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
  color: var(--text-main);
  letter-spacing: 0.5px;
}

.school-tag {
  font-size: 12px;
  padding: 2px 6px;
  background-color: var(--app-bg);
  border-radius: 4px;
  color: var(--text-regular);
  border: 1px solid var(--border-color);
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
  color: var(--text-regular);
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
  background-color: var(--primary-hover);
  color: var(--primary);
}

.nav-dropdown-panel {
  position: absolute;
  top: 48px;
  left: 50%;
  transform: translateX(-50%) translateY(10px);
  background: white;
  min-width: 170px;
  border-radius: 8px;
  box-shadow: var(--shadow-md);
  border: 1px solid var(--border-color);
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
  color: var(--text-main);
  text-decoration: none;
  font-size: 14px;
  transition: background 0.2s;
}

.dropdown-link:hover {
  background-color: var(--app-bg);
  color: var(--primary);
}

.dropdown-divider {
  height: 1px;
  background-color: var(--border-color);
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
  background-color: var(--app-bg);
}

.custom-avatar {
  background-color: var(--primary);
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
  color: var(--text-main);
  line-height: 1.2;
}

.user-role {
  font-size: 12px;
  color: var(--text-light);
  line-height: 1.2;
  margin-top: 2px;
}

.main-layout {
  flex: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.content-wrapper {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  scroll-behavior: smooth;
}

.fade-transform-enter-active,
.fade-transform-leave-active {
  transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
}

.fade-transform-enter {
  opacity: 0;
  transform: translateX(-20px);
}

.fade-transform-leave-to {
  opacity: 0;
  transform: translateX(20px);
}

.modern-dropdown-menu {
  border-radius: 8px;
  padding: 8px 0;
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-md);
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
</style>
