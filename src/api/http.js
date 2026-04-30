import axios from 'axios'
import { getToken, clearAuth } from '../utils/auth'

// 统一的 axios 实例：默认使用相对路径，通过 vue.config.js 的代理转发到后端
const http = axios.create({
  timeout: 30000
})

http.defaults.headers.post['Content-Type'] = 'application/json'

http.interceptors.request.use(
  config => {
    const token = getToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  error => Promise.reject(error)
)

http.interceptors.response.use(
  response => response,
  error => {
    // 401：常见于 token 失效或未登录。这里先清空本地鉴权，页面层根据需要跳转登录。
    if (error?.response?.status === 401) {
      clearAuth()
    }
    return Promise.reject(error)
  }
)

export default http

