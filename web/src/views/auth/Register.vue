<template>
  <div class="register-container">
    <div class="register-card">
      <div class="register-header">
        <div class="apple-logo">
          <svg width="32" height="32" viewBox="0 0 1024 1024">
            <path d="M512 0C229.25 0 0 229.25 0 512s229.25 512 512 512 512-229.25 512-512S794.75 0 512 0zm119.438 710.25c-12.25 23.812-35.312 46.062-65.624 57.438-34.25 12.938-74.562 12.938-108.812 0-30.312-11.376-53.374-33.626-65.624-57.438-10.562-20.124-16.312-42.938-16.312-66.312 0-20.812 4.938-40.438 14.5-56.5 11.5-19 30.5-32.688 51.812-37.438 15.75-3.812 49.562-3.812 65.312 0 21.312 4.75 40.312 18.438 51.812 37.438 9.562 16.062 14.5 35.688 14.5 56.5 0 23.374-5.75 46.188-16.312 66.312z" fill="#1d1d1f" />
            <path d="M712.5 354c-3.5 8.5-15 15.5-26 15.5-10 0-17.5-6.5-20.5-15-3-8.5-7.5-24.5-11.5-46.5-5-27.5-12.5-65.5-33-65.5-19 0-29 30.5-33 58-3.5 24.5-8 42.5-10.5 49.5-3 6.5-11 11-19.5 11-8.5 0-15.5-4.5-18.5-11-2.5-7-7-25-10.5-49.5-4-27.5-14-58-33-58-20.5 0-28 38-33 65.5-4 22-8.5 38-11.5 46.5-3 8.5-10.5 15-20.5 15-11 0-22.5-7-26-15.5-3.5-8.5-4.5-24-4.5-44 0-38.5 6.5-89.5 36.5-126.5 23-27.5 57.5-45 98.5-45 45 0 80.5 21 101 51.5 20.5 30.5 29 76 29 117.5 0 20-1 35.5-4.5 44z" fill="#1d1d1f" />
          </svg>
        </div>
        <h1 class="register-title">保定学院数字作品管理系统</h1>
        <p class="register-subtitle">创建新账号</p>
      </div>

      <div class="register-form">
        <el-form ref="registerForm" :model="registerForm" :rules="registerRules" label-width="80px">
          <el-form-item label="用户名" prop="username">
            <el-input
              v-model.trim="registerForm.username"
              placeholder="请输入用户名"
              prefix-icon="el-icon-user"
              class="apple-input"
            />
          </el-form-item>

          <el-form-item label="密码" prop="password">
            <el-input
              v-model="registerForm.password"
              type="password"
              placeholder="请输入密码"
              prefix-icon="el-icon-lock"
              show-password
              class="apple-input"
            />
          </el-form-item>

          <el-form-item label="确认密码" prop="confirmPassword">
            <el-input
              v-model="registerForm.confirmPassword"
              type="password"
              placeholder="请再次输入密码"
              prefix-icon="el-icon-lock"
              show-password
              class="apple-input"
            />
          </el-form-item>

          <el-form-item>
            <el-button type="primary" class="register-button" :loading="loading" @click="handleRegister">
              {{ loading ? '注册中...' : '注册' }}
            </el-button>
          </el-form-item>

          <div class="login-link">
            <span>已有账号？</span>
            <el-button type="text" class="apple-link" @click="handleLogin">立即登录</el-button>
          </div>
        </el-form>
      </div>
    </div>

    <div class="background-decoration">
      <div class="decoration-circle decoration-circle-1" />
      <div class="decoration-circle decoration-circle-2" />
      <div class="decoration-circle decoration-circle-3" />
      <div class="decoration-circle decoration-circle-4" />
      <div class="decoration-circle decoration-circle-5" />
      <div class="decoration-circle decoration-circle-6" />
      <div class="decoration-circle decoration-circle-7" />
      <div class="decoration-circle decoration-circle-8" />
    </div>
  </div>
</template>

<script>
import http from '../../utils/http'

export default {
  name: 'Register',
  data() {
    return {
      registerForm: {
        username: '',
        password: '',
        confirmPassword: ''
      },
      registerRules: {
        username: [
          { required: true, message: '请输入用户名', trigger: 'blur' },
          { min: 4, max: 20, message: '用户名长度需为 4 到 20 位', trigger: 'blur' }
        ],
        password: [
          { required: true, message: '请输入密码', trigger: 'blur' },
          { min: 6, max: 20, message: '密码长度需为 6 到 20 位', trigger: 'blur' }
        ],
        confirmPassword: [
          { required: true, message: '请再次输入密码', trigger: 'blur' },
          {
            validator: (rule, value, callback) => {
              if (value !== this.registerForm.password) {
                callback(new Error('两次输入的密码不一致'))
                return
              }
              callback()
            },
            trigger: 'blur'
          }
        ]
      },
      loading: false
    }
  },
  methods: {
    handleRegister() {
      this.$refs.registerForm.validate(valid => {
        if (!valid) {
          return false
        }

        this.loading = true
        http.post('/api/Auth/register', {
          Username: this.registerForm.username,
          Password: this.registerForm.password
        })
          .then(response => {
            this.$message.success(response.data?.message || '注册成功')
            this.$router.push('/auth/login')
          })
          .catch(error => {
            const errorMessage = error.response?.data?.message || '注册失败，请重试'
            this.$message.error(errorMessage)
          })
          .finally(() => {
            this.loading = false
          })
      })
    },
    handleLogin() {
      this.$router.push('/auth/login')
    }
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=SF+Pro+Display:wght@400;600;700&family=SF+Pro+Text:wght@300;400;600&display=swap');

.register-container {
  position: relative;
  width: 100vw;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #e6f0fa;
  overflow: hidden;
  font-family: 'SF Pro Text', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.background-decoration {
  position: absolute;
  inset: 0;
  z-index: 1;
}

.decoration-circle {
  position: absolute;
  border-radius: 50%;
  animation: float 8s ease-in-out infinite;
}

.decoration-circle-1 {
  width: 300px;
  height: 300px;
  top: -100px;
  right: -100px;
  background: linear-gradient(135deg, rgba(0, 113, 227, 0.15), rgba(0, 113, 227, 0.08));
}

.decoration-circle-2 {
  width: 200px;
  height: 200px;
  bottom: -50px;
  left: -50px;
  background: linear-gradient(135deg, rgba(52, 199, 89, 0.12), rgba(52, 199, 89, 0.06));
  animation-delay: 2s;
}

.decoration-circle-3 {
  width: 150px;
  height: 150px;
  top: 50%;
  left: 10%;
  transform: translateY(-50%);
  background: linear-gradient(135deg, rgba(255, 149, 0, 0.12), rgba(255, 149, 0, 0.06));
  animation-delay: 4s;
}

.decoration-circle-4 {
  width: 120px;
  height: 120px;
  top: 20%;
  left: 20%;
  background: linear-gradient(135deg, rgba(236, 72, 153, 0.1), rgba(236, 72, 153, 0.05));
  animation-delay: 1s;
}

.decoration-circle-5 {
  width: 80px;
  height: 80px;
  right: 15%;
  bottom: 30%;
  background: linear-gradient(135deg, rgba(16, 185, 129, 0.1), rgba(16, 185, 129, 0.05));
  animation-delay: 3s;
}

.decoration-circle-6 {
  width: 250px;
  height: 250px;
  right: 20%;
  bottom: -100px;
  background: linear-gradient(135deg, rgba(99, 102, 241, 0.12), rgba(99, 102, 241, 0.06));
  animation-delay: 5s;
}

.decoration-circle-7 {
  width: 100px;
  height: 100px;
  top: 70%;
  left: 30%;
  background: linear-gradient(135deg, rgba(244, 63, 94, 0.1), rgba(244, 63, 94, 0.05));
  animation-delay: 6s;
}

.decoration-circle-8 {
  width: 180px;
  height: 180px;
  top: 10%;
  left: 30%;
  background: linear-gradient(135deg, rgba(59, 130, 246, 0.12), rgba(59, 130, 246, 0.06));
  animation-delay: 7s;
}

.register-card {
  position: relative;
  z-index: 10;
  width: 400px;
  padding: 48px;
  background-color: rgba(255, 255, 255, 0.78);
  border: 1px solid rgba(255, 255, 255, 0.35);
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(20px);
  animation: slideIn 0.8s ease-out;
}

.register-header {
  margin-bottom: 32px;
  text-align: center;
}

.apple-logo {
  display: flex;
  justify-content: center;
  margin-bottom: 24px;
}

.register-title {
  margin-bottom: 8px;
  color: #1d1d1f;
  font-family: 'SF Pro Display', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  font-size: 28px;
  font-weight: 600;
}

.register-subtitle {
  margin: 0;
  color: #86868b;
  font-size: 16px;
}

.apple-input {
  font-family: 'SF Pro Text', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.register-button {
  width: 100%;
  height: 44px;
  border: 1px solid transparent;
  border-radius: 8px;
  background-color: #0071e3;
  color: #ffffff;
  font-size: 16px;
  transition: all 0.3s ease;
}

.register-button:hover {
  background-color: #0077ed;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 113, 227, 0.3);
}

.login-link {
  margin-top: 24px;
  color: #86868b;
  font-size: 14px;
  text-align: center;
}

.apple-link {
  color: #0071e3;
}

@media (max-width: 768px) {
  .register-card {
    width: 90%;
    padding: 32px;
  }

  .register-title {
    font-size: 24px;
  }
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(50px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes float {
  0%, 100% {
    transform: translateY(0);
  }

  50% {
    transform: translateY(-20px);
  }
}
</style>
