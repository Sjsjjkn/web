import Vue from 'vue'
import App from './App.vue'
import router from './router'
import ElementUI from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'
import http from './api/http'
import './styles/design-tokens.css'

// 将 http 实例挂载到 Vue，页面中仍可通过 this.$axios 使用
Vue.prototype.$axios = http

Vue.use(router)
Vue.use(ElementUI)
Vue.config.productionTip = false

new Vue({
  router,
  render: h => h(App)
}).$mount('#app')