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
</style>
