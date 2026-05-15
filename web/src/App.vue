<template>
  <div id="app">
    <template v-if="!isAuthPage">
      <NavBar />
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
import NavBar from './components/layout/NavBar.vue'

export default {
  name: 'App',
  components: { NavBar },
  computed: {
    isAuthPage() {
      return this.$route.path === '/' || this.$route.path === '/auth/login' || this.$route.path === '/auth/register'
    }
  }
}
</script>

<style>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&family=Noto+Sans+SC:wght@400;500;600;700&display=swap');

/* ===== 数字作品管理系统 — 暖绿 · 现代有机设计系统 ===== */

:root {
  /* ── 品牌色 · 森林绿系 ── */
  --primary: #2D8A6E;
  --primary-hover: #25755C;
  --primary-light: #45A884;
  --primary-dark: #1E6B52;
  --primary-bg: #EDF5F0;
  --primary-glow: rgba(45, 138, 110, 0.20);

  /* ── 强调色 · 暖金 ── */
  --accent: #C8AA6E;
  --accent-light: #F0E6D0;
  --accent-strong: #B8943F;

  /* ── 功能色 ── */
  --success: #34C759;
  --warning: #F09342;
  --danger: #E05555;
  --info: #5B9BD5;

  /* ── 中性色 · 暖米色系统 ── */
  --app-bg: #F8F9F5;
  --bg: #F8F9F5;
  --surface: #FFFFFF;
  --card-bg: #FFFFFF;
  --bg-hover: #F5F2EC;
  --header-bg: rgba(255, 255, 255, 0.85);
  --bg-glass: rgba(255, 255, 255, 0.82);
  --text-main: #1A1A1A;
  --text-regular: #555555;
  --text-light: #888888;
  --text-placeholder: #B0B0B0;
  --border-color: #E8E2D8;
  --border-light: #F2EDE6;
  --input-bg: #F8F9F5;

  /* ── 圆角 ── */
  --radius-xs: 6px;
  --radius-sm: 10px;
  --radius-md: 14px;
  --radius-lg: 18px;
  --radius-xl: 24px;
  --radius-full: 9999px;

  /* ── 阴影 ── */
  --shadow-xs: 0 1px 2px rgba(0, 0, 0, .03);
  --shadow-sm: 0 2px 8px rgba(0, 0, 0, .05);
  --shadow-card: 0 2px 12px rgba(0, 0, 0, .04), 0 1px 3px rgba(0, 0, 0, .03);
  --shadow-card-hover: 0 16px 40px rgba(0, 0, 0, .12);
  --shadow-md: 0 6px 20px rgba(0, 0, 0, .07);
  --shadow-lg: 0 12px 36px rgba(0, 0, 0, .10);
  --shadow-xl: 0 20px 48px rgba(0, 0, 0, .14);
  --shadow-popup: 0 20px 48px rgba(0, 0, 0, .14);

  /* ── 动画时长 ── */
  --duration-fast: .18s;
  --duration-normal: .3s;
  --duration-slow: .5s;

  /* ── 缓动曲线 ── */
  --ease-out-expo: cubic-bezier(0.16, 1, 0.3, 1);
  --ease-out-back: cubic-bezier(0.34, 1.56, 0.64, 1);
  --ease-in-out: cubic-bezier(0.45, 0, 0.55, 1);

  /* ── 字体 ── */
  --font-sans: 'Inter', 'Noto Sans SC', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'PingFang SC',
    'Microsoft YaHei', 'Hiragino Sans GB', sans-serif;
  --font-mono: 'SF Mono', 'Cascadia Code', 'Consolas', monospace;

  /* ── Element UI 覆盖 ── */
  --el-color-primary: #2D8A6E;
}

/* ── 全局重置 ── */
*,
*::before,
*::after {
  box-sizing: border-box;
}

body, html, #app {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100vh;
  background-color: var(--app-bg);
  font-family: var(--font-sans);
  color: var(--text-main);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* ── 环境光晕 ── */
body::before {
  content: '';
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background:
    radial-gradient(ellipse 80% 60% at 50% -10%, rgba(45, 138, 110, .06), transparent),
    radial-gradient(ellipse 60% 50% at 85% 80%, rgba(200, 170, 110, .04), transparent),
    radial-gradient(ellipse 50% 50% at 15% 70%, rgba(91, 155, 213, .03), transparent);
  pointer-events: none;
  z-index: 0;
}

.main-layout {
  position: relative;
  z-index: 1;
  flex: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.content-wrapper {
  flex: 1;
  overflow-y: auto;
  padding: 28px 32px;
  scroll-behavior: smooth;
}

/* ── Element UI 全局覆盖 ── */

/* 按钮 */
.el-button--primary {
  background-color: var(--primary) !important;
  border-color: var(--primary) !important;
  border-radius: var(--radius-sm) !important;
  font-weight: 600 !important;
  transition: all var(--duration-normal) var(--ease-out-expo) !important;
}
.el-button--primary:hover {
  background-color: var(--primary-dark) !important;
  border-color: var(--primary-dark) !important;
  box-shadow: 0 4px 16px var(--primary-glow) !important;
  transform: translateY(-1px) !important;
}
.el-button--primary:active {
  transform: translateY(0) !important;
}

.el-button--default {
  border-radius: var(--radius-sm) !important;
  font-weight: 500 !important;
  border-color: var(--border-color) !important;
  transition: all var(--duration-normal) var(--ease-out-expo) !important;
}
.el-button--default:hover {
  border-color: var(--primary) !important;
  color: var(--primary) !important;
  background: var(--primary-bg) !important;
}

/* 输入框 */
.el-input__inner {
  border-radius: var(--radius-sm) !important;
  border-color: var(--border-color) !important;
  background: var(--app-bg) !important;
  transition: all var(--duration-normal) var(--ease-out-expo) !important;
  font-family: var(--font-sans) !important;
}
.el-input__inner:focus {
  border-color: var(--primary) !important;
  box-shadow: 0 0 0 3px var(--primary-glow) !important;
}

.el-textarea__inner {
  border-radius: var(--radius-sm) !important;
  border-color: var(--border-color) !important;
  background: var(--app-bg) !important;
  font-family: var(--font-sans) !important;
}
.el-textarea__inner:focus {
  border-color: var(--primary) !important;
  box-shadow: 0 0 0 3px var(--primary-glow) !important;
}

/* 标签 */
.el-tag {
  border-radius: var(--radius-full) !important;
  font-weight: 600 !important;
  border: none !important;
}

.el-tag--primary {
  background-color: var(--primary-bg) !important;
  color: var(--primary) !important;
}

/* 分页 */
.el-pagination .active {
  color: var(--primary) !important;
}

.el-pager li.active {
  background-color: var(--primary) !important;
  border-radius: var(--radius-sm) !important;
}

/* 对话框 */
.el-dialog {
  border-radius: var(--radius-lg) !important;
  box-shadow: var(--shadow-popup) !important;
  overflow: hidden !important;
}

.el-dialog__header {
  padding: 20px 24px 0 !important;
  border-bottom: none !important;
}

.el-dialog__title {
  font-size: 18px !important;
  font-weight: 700 !important;
  color: var(--text-main) !important;
}

.el-dialog__headerbtn {
  top: 16px !important;
  right: 20px !important;
}

.el-dialog__headerbtn .el-dialog__close {
  color: var(--text-light) !important;
  font-size: 20px !important;
}
.el-dialog__headerbtn .el-dialog__close:hover {
  color: var(--danger) !important;
}

.el-dialog__body {
  padding: 16px 24px 24px !important;
}

.el-dialog__footer {
  padding: 0 24px 20px !important;
}

/* 表格 */
.el-table {
  border-radius: var(--radius-lg) !important;
  overflow: hidden !important;
  border: 1px solid var(--border-color) !important;
}

.el-table th.el-table__cell {
  background-color: var(--app-bg) !important;
  color: var(--text-regular) !important;
  font-weight: 600 !important;
  font-size: 13px !important;
  border-bottom: 2px solid var(--border-color) !important;
}

.el-table td.el-table__cell {
  border-bottom: 1px solid var(--border-light) !important;
}

.el-table tr:hover > td {
  background-color: #FDFCF9 !important;
}

/* 菜单 */
.el-menu {
  border-radius: var(--radius-md) !important;
}

/* 加载 */
.el-loading-mask {
  border-radius: var(--radius-lg) !important;
}

/* 解决 touchmove 事件警告 */
.el-dialog__wrapper,
.el-dialog,
.el-loading-mask,
.el-popover,
.el-dropdown-menu {
  touch-action: pan-y;
  -ms-touch-action: pan-y;
}

/* 确保对话框遮罩层和内容都显示在最顶层 */
/* 模态遮罩必须在对话框下方 */
.v-modal {
  z-index: 2000 !important;
}

.el-dialog {
  z-index: 2001 !important;
}

.el-dialog__header,
.el-dialog__body,
.el-dialog__footer {
  position: relative;
  z-index: 2002 !important;
}

/* ── 过渡动画 ── */
.fade-transform-enter-active,
.fade-transform-leave-active {
  transition: all 0.35s var(--ease-out-expo);
}

.fade-transform-enter {
  opacity: 0;
  transform: translateY(12px);
}

.fade-transform-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

/* ── 滚动条 ── */
.content-wrapper::-webkit-scrollbar {
  width: 6px;
}
.content-wrapper::-webkit-scrollbar-track {
  background: transparent;
}
.content-wrapper::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: var(--radius-full);
}
.content-wrapper::-webkit-scrollbar-thumb:hover {
  background: var(--text-placeholder);
}

/* ── 页面通用卡片 ── */
.page-card {
  background: var(--card-bg);
  border-radius: var(--radius-lg);
  padding: 28px;
  box-shadow: var(--shadow-card);
  border: 1px solid var(--border-color);
  transition: all var(--duration-normal) var(--ease-out-expo);
}
.page-card:hover {
  box-shadow: var(--shadow-md);
}

.page-header {
  margin-bottom: 28px;
}
.page-header h2 {
  font-size: 24px;
  font-weight: 700;
  margin: 0 0 8px 0;
  color: var(--text-main);
  letter-spacing: -0.3px;
}
.page-header p {
  font-size: 14px;
  color: var(--text-light);
  margin: 0;
}

/* ── Toast 消息 ── */
.el-message {
  border-radius: var(--radius-md) !important;
  box-shadow: var(--shadow-lg) !important;
  border: 1px solid var(--border-color) !important;
}

/* ── 确认框 ── */
.el-message-box {
  border-radius: var(--radius-lg) !important;
  box-shadow: var(--shadow-popup) !important;
  padding-bottom: 20px !important;
}

.el-message-box__title {
  font-weight: 700 !important;
  font-size: 17px !important;
}

.el-message-box__btns .el-button {
  border-radius: var(--radius-sm) !important;
  font-weight: 600 !important;
}
</style>