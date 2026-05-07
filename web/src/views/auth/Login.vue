<template>
  <div class="login-container">
    <div class="login-card">
      <div class="login-header">
        <div class="apple-logo">
          <svg width="40" height="40" viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg">
            <path d="M911.3 512c0-43.6-7.2-85.2-20.3-123.8-13.4-39.3-32.2-74.4-55.6-103.6-23.8-29.7-52.1-53.9-84-71.7-32.4-18.3-67.9-31.4-105.3-38.7-37.8-7.5-77-11.7-116.4-11.7-51.4 0-101.5 6.1-149.1 18.3-48 12.5-92.5 30.7-132 54.2C148.9 253.4 122 295.8 105.9 342.5 90.3 388.2 82.9 440.2 82.9 500c0 59.6 7.5 111.4 21.8 155.2 14.6 44.3 35.9 83.6 62.7 116.3 27.5 33.2 60 59.3 97.1 77.2 37.7 18.3 79 29.6 123.2 33.6 44.7 4.1 90.6 4.1 136.3 0 45.7-4 87.1-15.3 123.2-33.6 37.1-18 69.6-44 97.1-77.2 26.8-32.7 48.1-72 62.7-116.3 14.3-43.8 21.8-95.6 21.8-155.2zM320 745.6c-55.8-30.9-97.4-82.4-112.4-144.9-15.3-63 4.6-129.1 55.6-160.6 44.7-27.3 100.2-26.8 143.8 1.4 45 29.1 76.6 76.6 83.7 130.6 7 54-11.4 110.4-51.2 142.1-41.3 32.8-95.8 40.8-140.5 21.8zm176-485.1c-52.4 0-95 42.9-95 96 0 53 42.6 96 95 96 52 0 94.5-43 94.5-96 0-53.1-42.5-96-94.5-96zm208 264.7c-7.1-54-38.7-101.5-83.7-130.6-43.6-28.2-99.1-28.7-143.8-1.4-51 31.5-70.9 97.6-55.6 160.6 15 62.5 56.6 114 112.4 144.9 44.7 19 99.2 11 140.5-21.8 39.8-31.7 58.2-88.1 51.2-142.1z" fill="#0071e3" />
          </svg>
        </div>
        <h2 class="login-title">保定学院数字作品管理系统</h2>
        <p class="login-subtitle">使用你的账号登录系统</p>
      </div>

      <el-form ref="loginForm" :model="loginForm" :rules="loginRules" label-width="80px">
        <el-form-item label="用户名" prop="username">
          <el-input
            id="username"
            v-model.trim="loginForm.username"
            name="username"
            placeholder="请输入用户名"
            prefix-icon="el-icon-user"
            class="apple-input"
          />
        </el-form-item>

        <el-form-item label="密码" prop="password">
          <el-input
            id="password"
            v-model="loginForm.password"
            name="password"
            type="password"
            placeholder="请输入密码"
            prefix-icon="el-icon-lock"
            show-password
            class="apple-input"
            @keyup.enter.native="handleLogin"
          />
        </el-form-item>

        <el-form-item label="验证码" prop="captcha">
          <div class="captcha-container">
            <el-input
              id="captcha"
              v-model.trim="loginForm.captcha"
              name="captcha"
              placeholder="请输入验证码"
              prefix-icon="el-icon-chat-line-round"
              class="apple-input captcha-input"
              @keyup.enter.native="handleLogin"
            />
            <div class="captcha-image" @click="generateCaptcha">
              <img :src="captchaImage" alt="验证码" class="captcha-img">
            </div>
          </div>
        </el-form-item>

        <el-form-item>
          <el-checkbox v-model="loginForm.remember" class="apple-checkbox">记住我</el-checkbox>
          <el-button type="text" class="forgot-password" @click="handleForgotPassword">忘记密码？</el-button>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" class="login-button" :loading="loading" @click="handleLogin">
            {{ loading ? '登录中...' : '登录' }}
          </el-button>
        </el-form-item>

        <div class="login-footer">
          <span>还没有账号？</span>
          <el-button type="text" class="apple-link" @click="handleRegister">立即注册</el-button>
        </div>
      </el-form>
    </div>

    <el-dialog
      title="忘记密码"
      :visible.sync="forgotPasswordDialogVisible"
      width="400px"
      center
      class="forgot-password-dialog"
    >
      <el-form
        ref="forgotPasswordForm"
        :model="forgotPasswordForm"
        :rules="forgotPasswordRules"
        label-width="100px"
      >
        <el-form-item label="用户名" prop="username">
          <el-input
            v-model.trim="forgotPasswordForm.username"
            placeholder="请输入用户名"
            prefix-icon="el-icon-user"
            class="apple-input"
          />
        </el-form-item>

        <el-form-item label="验证码" prop="verificationCode">
          <el-input
            v-model.trim="forgotPasswordForm.verificationCode"
            placeholder="请输入验证码"
            prefix-icon="el-icon-chat-line-round"
            class="apple-input"
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            class="verification-code-button"
            :disabled="countdown > 0"
            @click="sendVerificationCode"
          >
            {{ countdown > 0 ? `${countdown} 秒后重发` : '发送验证码' }}
          </el-button>
        </el-form-item>

        <el-form-item label="新密码" prop="newPassword">
          <el-input
            v-model="forgotPasswordForm.newPassword"
            type="password"
            placeholder="请输入新密码"
            prefix-icon="el-icon-lock"
            show-password
            class="apple-input"
          />
        </el-form-item>

        <el-form-item label="确认密码" prop="confirmPassword">
          <el-input
            v-model="forgotPasswordForm.confirmPassword"
            type="password"
            placeholder="请再次输入新密码"
            prefix-icon="el-icon-lock"
            show-password
            class="apple-input"
          />
        </el-form-item>
      </el-form>

      <div slot="footer" class="dialog-footer">
        <el-button @click="forgotPasswordDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="resetLoading" @click="handleResetPassword">重置密码</el-button>
      </div>
    </el-dialog>

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
import { setToken, setUser, clearAuth } from '../../utils/auth'

export default {
  name: 'Login',
  data() {
    const savedUsername = localStorage.getItem('rememberedUsername') || ''
    const savedPassword = localStorage.getItem('rememberedPassword') || ''
    const savedRemember = localStorage.getItem('rememberedRemember') === 'true'

    return {
      loginForm: {
        username: savedUsername,
        password: savedPassword,
        captcha: '',
        remember: savedRemember
      },
      loginRules: {
        username: [
          { required: true, message: '请输入用户名', trigger: 'blur' }
        ],
        password: [
          { required: true, message: '请输入密码', trigger: 'blur' }
        ],
        captcha: [
          { required: true, message: '请输入验证码', trigger: 'blur' },
          {
            validator: (rule, value, callback) => {
              const input = (value || '').toUpperCase()
              if (input !== this.captchaText) {
                callback(new Error('验证码错误'))
                return
              }
              callback()
            },
            trigger: 'blur'
          }
        ]
      },
      loading: false,
      forgotPasswordDialogVisible: false,
      forgotPasswordForm: {
        username: '',
        verificationCode: '',
        newPassword: '',
        confirmPassword: ''
      },
      captchaText: '',
      captchaImage: '',
      forgotPasswordRules: {
        username: [
          { required: true, message: '请输入用户名', trigger: 'blur' }
        ],
        verificationCode: [
          { required: true, message: '请输入验证码', trigger: 'blur' }
        ],
        newPassword: [
          { required: true, message: '请输入新密码', trigger: 'blur' },
          { min: 6, message: '密码长度至少 6 位', trigger: 'blur' }
        ],
        confirmPassword: [
          { required: true, message: '请再次输入新密码', trigger: 'blur' },
          {
            validator: (rule, value, callback) => {
              if (value !== this.forgotPasswordForm.newPassword) {
                callback(new Error('两次输入的密码不一致'))
                return
              }
              callback()
            },
            trigger: 'blur'
          }
        ]
      },
      countdown: 0,
      resetLoading: false
    }
  },
  methods: {
    handleLogin() {
      this.$refs.loginForm.validate(valid => {
        if (!valid) {
          return false
        }

        this.loading = true
        http.post('/api/Auth/login', {
          Username: this.loginForm.username,
          Password: this.loginForm.password,
          Remember: this.loginForm.remember
        })
          .then(response => {
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

            if (this.loginForm.remember) {
              localStorage.setItem('rememberedUsername', this.loginForm.username)
              localStorage.setItem('rememberedPassword', this.loginForm.password)
              localStorage.setItem('rememberedRemember', 'true')
            } else {
              localStorage.removeItem('rememberedUsername')
              localStorage.removeItem('rememberedPassword')
              localStorage.removeItem('rememberedRemember')
            }

            this.$message.success(data.message || '登录成功')
            const redirect = this.$route.query.redirect
            this.$router.push(redirect || '/works/explore')
          })
          .catch(error => {
            clearAuth()
            const errorMessage = error.response?.data?.message || error.message || '登录失败，请重试'
            this.$message.error(errorMessage)
            this.generateCaptcha()
          })
          .finally(() => {
            this.loading = false
          })
      })
    },
    handleRegister() {
      this.$router.push('/auth/register')
    },
    handleForgotPassword() {
      this.forgotPasswordDialogVisible = true
    },
    sendVerificationCode() {
      this.$refs.forgotPasswordForm.validateField('username', error => {
        if (error) {
          return
        }

        const { username } = this.forgotPasswordForm
        if (!username) {
          this.$message.error('请输入用户名')
          return
        }

        const loadingInstance = this.$loading({
          lock: true,
          text: '正在发送验证码...',
          spinner: 'el-icon-loading',
          background: 'rgba(0, 0, 0, 0.7)'
        })

        const verificationCode = Math.floor(100000 + Math.random() * 900000).toString()
        setTimeout(() => {
          this.$message.success(`验证码已生成: ${verificationCode}`)
          this.countdown = 60
          const timer = setInterval(() => {
            this.countdown -= 1
            if (this.countdown <= 0) {
              clearInterval(timer)
            }
          }, 1000)
          loadingInstance.close()
        }, 1000)
      })
    },
    handleResetPassword() {
      this.$refs.forgotPasswordForm.validate(valid => {
        if (!valid) {
          return false
        }

        this.resetLoading = true
        setTimeout(() => {
          this.$message.success('密码重置成功')
          this.forgotPasswordDialogVisible = false
          this.forgotPasswordForm = {
            username: '',
            verificationCode: '',
            newPassword: '',
            confirmPassword: ''
          }
          this.countdown = 0
          this.resetLoading = false
        }, 1000)
      })
    },
    generateCaptcha() {
      const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789'
      let captcha = ''
      for (let i = 0; i < 4; i += 1) {
        captcha += chars.charAt(Math.floor(Math.random() * chars.length))
      }
      this.captchaText = captcha
      this.captchaImage = this.createCaptchaImage(captcha)
    },
    createCaptchaImage(text) {
      const canvas = document.createElement('canvas')
      const ctx = canvas.getContext('2d')
      canvas.width = 120
      canvas.height = 40

      ctx.fillStyle = '#f5f5f7'
      ctx.fillRect(0, 0, canvas.width, canvas.height)

      for (let i = 0; i < 5; i += 1) {
        ctx.beginPath()
        ctx.moveTo(Math.random() * canvas.width, Math.random() * canvas.height)
        ctx.lineTo(Math.random() * canvas.width, Math.random() * canvas.height)
        ctx.strokeStyle = `rgba(0, 113, 227, ${Math.random() * 0.5 + 0.2})`
        ctx.lineWidth = 1
        ctx.stroke()
      }

      for (let i = 0; i < 20; i += 1) {
        ctx.beginPath()
        ctx.arc(Math.random() * canvas.width, Math.random() * canvas.height, 1, 0, 2 * Math.PI)
        ctx.fillStyle = `rgba(0, 113, 227, ${Math.random() * 0.5 + 0.2})`
        ctx.fill()
      }

      ctx.font = '20px Arial'
      ctx.textAlign = 'center'
      ctx.textBaseline = 'middle'

      for (let i = 0; i < text.length; i += 1) {
        const char = text.charAt(i)
        const x = (canvas.width / text.length) * (i + 0.5)
        const y = canvas.height / 2
        const rotate = (Math.random() - 0.5) * 0.5

        ctx.save()
        ctx.translate(x, y)
        ctx.rotate(rotate)
        ctx.fillStyle = `rgb(${Math.floor(Math.random() * 50 + 100)}, ${Math.floor(Math.random() * 50 + 100)}, ${Math.floor(Math.random() * 50 + 150)})`
        ctx.fillText(char, 0, 0)
        ctx.restore()
      }

      return canvas.toDataURL('image/png')
    }
  },
  mounted() {
    this.generateCaptcha()
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=SF+Pro+Display:wght@400;600;700&family=SF+Pro+Text:wght@300;400;600&display=swap');

.login-container {
  position: relative;
  width: 100vw;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #e6f0fa;
  font-family: 'SF Pro Text', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  overflow: hidden;
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
  left: -100px;
  background: linear-gradient(135deg, rgba(0, 113, 227, 0.15), rgba(0, 113, 227, 0.08));
  animation-delay: 0s;
}

.decoration-circle-2 {
  width: 200px;
  height: 200px;
  right: -50px;
  bottom: -50px;
  background: linear-gradient(135deg, rgba(52, 199, 89, 0.12), rgba(52, 199, 89, 0.06));
  animation-delay: 2s;
}

.decoration-circle-3 {
  width: 150px;
  height: 150px;
  top: 50%;
  right: 10%;
  transform: translateY(-50%);
  background: linear-gradient(135deg, rgba(255, 149, 0, 0.12), rgba(255, 149, 0, 0.06));
  animation-delay: 4s;
}

.decoration-circle-4 {
  width: 120px;
  height: 120px;
  top: 20%;
  right: 20%;
  background: linear-gradient(135deg, rgba(236, 72, 153, 0.1), rgba(236, 72, 153, 0.05));
  animation-delay: 1s;
}

.decoration-circle-5 {
  width: 80px;
  height: 80px;
  bottom: 30%;
  left: 15%;
  background: linear-gradient(135deg, rgba(16, 185, 129, 0.1), rgba(16, 185, 129, 0.05));
  animation-delay: 3s;
}

.decoration-circle-6 {
  width: 250px;
  height: 250px;
  bottom: -100px;
  left: 20%;
  background: linear-gradient(135deg, rgba(99, 102, 241, 0.12), rgba(99, 102, 241, 0.06));
  animation-delay: 5s;
}

.decoration-circle-7 {
  width: 100px;
  height: 100px;
  top: 70%;
  right: 30%;
  background: linear-gradient(135deg, rgba(244, 63, 94, 0.1), rgba(244, 63, 94, 0.05));
  animation-delay: 6s;
}

.decoration-circle-8 {
  width: 180px;
  height: 180px;
  top: 10%;
  right: 30%;
  background: linear-gradient(135deg, rgba(59, 130, 246, 0.12), rgba(59, 130, 246, 0.06));
  animation-delay: 7s;
}

.login-card {
  position: relative;
  z-index: 10;
  width: 400px;
  padding: 48px;
  background-color: rgba(255, 255, 255, 0.78);
  border: 1px solid rgba(255, 255, 255, 0.35);
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(20px);
  transition: transform 0.3s ease, box-shadow 0.3s ease, background-color 0.3s ease;
  animation: slideIn 0.8s ease-out;
}

.login-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
  background-color: rgba(255, 255, 255, 0.84);
}

.login-header {
  margin-bottom: 32px;
  text-align: center;
}

.apple-logo {
  display: flex;
  justify-content: center;
  margin-bottom: 24px;
  animation: fadeIn 1s ease-out;
}

.login-title {
  margin-bottom: 8px;
  color: #1d1d1f;
  font-family: 'SF Pro Display', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  font-size: 28px;
  font-weight: 600;
  line-height: 1.14;
  letter-spacing: 0.196px;
  animation: fadeIn 1s ease-out 0.2s both;
}

.login-subtitle {
  margin: 0;
  color: #86868b;
  font-size: 16px;
  line-height: 1.33;
  letter-spacing: 0.08px;
  animation: fadeIn 1s ease-out 0.4s both;
}

.apple-input {
  font-family: 'SF Pro Text', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.login-button {
  position: relative;
  width: 100%;
  height: 44px;
  margin-top: 16px;
  overflow: hidden;
  border: 1px solid transparent;
  border-radius: 8px;
  background-color: #0071e3;
  color: #ffffff;
  font-size: 16px;
  transition: all 0.3s ease;
}

.login-button:hover {
  background-color: #0077ed;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 113, 227, 0.3);
}

.login-button::before,
.verification-code-button::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
  transition: left 0.5s;
}

.login-button:hover::before,
.verification-code-button:hover::before {
  left: 100%;
}

.login-footer {
  margin-top: 24px;
  color: #86868b;
  font-size: 14px;
  text-align: center;
  animation: fadeIn 1s ease-out 0.6s both;
}

.apple-link,
.forgot-password {
  color: #0071e3;
}

.apple-checkbox {
  color: #1d1d1f;
  font-size: 14px;
}

.forgot-password {
  float: right;
}

.captcha-container {
  display: flex;
  align-items: center;
  gap: 12px;
}

.captcha-input {
  flex: 1;
}

.captcha-image {
  cursor: pointer;
  overflow: hidden;
  border-radius: 8px;
  transition: transform 0.3s ease;
}

.captcha-image:hover {
  transform: scale(1.02);
}

.captcha-img {
  width: 120px;
  height: 44px;
  border-radius: 8px;
  object-fit: cover;
}

.forgot-password-dialog {
  font-family: 'SF Pro Text', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.verification-code-button {
  position: relative;
  width: 100%;
  height: 44px;
  overflow: hidden;
  border: 1px solid transparent;
  border-radius: 8px;
  background-color: #0071e3;
  color: #ffffff;
  transition: all 0.3s ease;
}

.verification-code-button:hover {
  background-color: #0077ed;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 113, 227, 0.3);
}

.verification-code-button:disabled {
  background-color: #d1d1d6;
  color: #86868b;
  box-shadow: none;
  transform: none;
}

.dialog-footer {
  display: flex;
  justify-content: center;
  gap: 12px;
}

@media (max-width: 768px) {
  .login-card {
    width: 90%;
    padding: 32px;
  }

  .login-title {
    font-size: 24px;
  }
}

@media (prefers-color-scheme: dark) {
  .login-container {
    background-color: #000000;
  }

  .login-card {
    background-color: rgba(39, 39, 41, 0.9);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  }

  .login-title,
  .apple-checkbox {
    color: #ffffff;
  }

  .login-subtitle,
  .login-footer {
    color: #b1b1b6;
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

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(30px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }

  to {
    opacity: 1;
  }
}
</style>
