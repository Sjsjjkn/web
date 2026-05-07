<template>
  <div class="not-found-page">
    <!-- 环境光晕 -->
    <div class="ambient-glow glow-top"></div>
    <div class="ambient-glow glow-bottom"></div>

    <!-- 漂浮装饰 -->
    <div class="floating-shapes">
      <div class="shape shape-1"></div>
      <div class="shape shape-2"></div>
      <div class="shape shape-3"></div>
    </div>

    <div class="not-found-container">
      <div class="not-found-card glass-card fade-in-up">
        <div class="error-visual">
          <div class="error-number">
            <span class="digit digit-4">4</span>
            <span class="digit digit-0">0</span>
            <span class="digit digit-4-last">4</span>
          </div>
          <div class="error-ring">
            <div class="ring-inner">
              <i class="el-icon-search"></i>
            </div>
          </div>
        </div>

        <h1 class="error-title">页面未找到</h1>
        <p class="error-message">
          您访问的页面不存在或已被移除
        </p>
        <p class="error-hint">
          请检查网址是否正确，或返回首页继续浏览
        </p>

        <div class="action-buttons">
          <el-button type="primary" round size="medium" @click="$router.push('/works/explore')" class="primary-btn">
            <i class="el-icon-s-home"></i>
            返回首页
          </el-button>
          <el-button round size="medium" @click="$router.go(-1)" class="secondary-btn">
            <i class="el-icon-back"></i>
            返回上一页
          </el-button>
        </div>
      </div>
    </div>
  </div>
</template>
REPLACE

<script>
export default {
  name: 'NotFound'
}
</script>

<style scoped>
/* ===== 全局页面 ===== */
.not-found-page {
  min-height: calc(100vh - 60px);
  background: var(--bg-page);
  position: relative;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* ── 环境光晕 ── */
.ambient-glow {
  position: fixed;
  border-radius: 50%;
  filter: blur(140px);
  pointer-events: none;
  z-index: 0;
  opacity: 0.5;
}

.glow-top {
  width: 500px;
  height: 500px;
  background: radial-gradient(circle, rgba(45, 138, 110, 0.1) 0%, transparent 70%);
  top: -200px;
  right: -150px;
  animation: floatGlow 8s ease-in-out infinite alternate;
}

.glow-bottom {
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, rgba(200, 170, 110, 0.08) 0%, transparent 70%);
  bottom: -150px;
  left: -100px;
  animation: floatGlow 10s ease-in-out infinite alternate-reverse;
}

@keyframes floatGlow {
  0% { transform: translate(0, 0) scale(1); }
  100% { transform: translate(30px, -20px) scale(1.1); }
}

/* ── 漂浮装饰形状 ── */
.floating-shapes {
  position: fixed;
  inset: 0;
  pointer-events: none;
  z-index: 0;
}

.shape {
  position: absolute;
  border-radius: 50%;
  opacity: 0.04;
  background: var(--primary);
  animation: floatShape 12s ease-in-out infinite;
}

.shape-1 {
  width: 120px;
  height: 120px;
  top: 15%;
  left: 10%;
  animation-delay: 0s;
}

.shape-2 {
  width: 80px;
  height: 80px;
  top: 60%;
  right: 15%;
  animation-delay: -4s;
  background: var(--accent);
}

.shape-3 {
  width: 60px;
  height: 60px;
  bottom: 20%;
  left: 40%;
  animation-delay: -8s;
}

@keyframes floatShape {
  0%, 100% { transform: translateY(0) rotate(0deg); }
  33% { transform: translateY(-30px) rotate(10deg); }
  66% { transform: translateY(15px) rotate(-5deg); }
}

/* ── 内容容器 ── */
.not-found-container {
  position: relative;
  z-index: 1;
  padding: 40px 24px;
}

/* ── 玻璃卡片 ── */
.glass-card {
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px) saturate(180%);
  -webkit-backdrop-filter: blur(16px) saturate(180%);
  border-radius: var(--radius-xl);
  border: 1px solid rgba(232, 226, 216, 0.6);
  box-shadow: var(--shadow-card);
}

.not-found-card {
  text-align: center;
  max-width: 520px;
  width: 100%;
  margin: 0 auto;
  padding: 56px 48px 48px;
}

/* ── 视觉区域 ── */
.error-visual {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 32px;
}

.error-number {
  display: flex;
  align-items: center;
  gap: 4px;
}

.digit {
  font-size: 120px;
  font-weight: 800;
  line-height: 1;
  letter-spacing: -4px;
  background: linear-gradient(180deg, var(--primary) 0%, var(--primary-light) 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  filter: drop-shadow(0 4px 12px rgba(45, 138, 110, 0.2));
  animation: digitFloat 3s ease-in-out infinite;
}

.digit-0 {
  animation-delay: 0.1s;
}

.digit-4-last {
  animation-delay: 0.2s;
}

@keyframes digitFloat {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-8px); }
}

.error-ring {
  position: absolute;
  right: -20px;
  bottom: 10px;
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: linear-gradient(135deg, rgba(240, 147, 66, 0.15), rgba(247, 186, 30, 0.15));
  display: flex;
  align-items: center;
  justify-content: center;
  animation: ringPulse 2s ease-in-out infinite;
}

.ring-inner {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: linear-gradient(135deg, #FFF8F0, #FFF3E0);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
  color: var(--warning);
}

@keyframes ringPulse {
  0%, 100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(240, 147, 66, 0.2); }
  50% { transform: scale(1.05); box-shadow: 0 0 0 12px rgba(240, 147, 66, 0); }
}

/* ── 标题与文案 ── */
.error-title {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 12px;
}

.error-message {
  font-size: 15px;
  color: var(--text-regular);
  margin: 0 0 4px;
  line-height: 1.6;
}

.error-hint {
  font-size: 13px;
  color: var(--text-light);
  margin: 0 0 36px;
  line-height: 1.5;
}

/* ── 操作按钮 ── */
.action-buttons {
  display: flex;
  gap: 14px;
  justify-content: center;
  flex-wrap: wrap;
}

.primary-btn {
  font-weight: 600;
  padding: 12px 28px;
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  border: none;
  box-shadow: 0 4px 16px rgba(45, 138, 110, 0.3);
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.primary-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(45, 138, 110, 0.4);
}

.primary-btn ::v-deep span {
  display: flex;
  align-items: center;
  gap: 6px;
}

.secondary-btn {
  font-weight: 500;
  padding: 12px 28px;
  border: 1px solid var(--border-color);
  background: var(--surface);
  color: var(--text-regular);
  transition: all var(--duration-normal) var(--ease-out-expo);
}

.secondary-btn:hover {
  border-color: var(--primary);
  color: var(--primary);
  background: var(--primary-bg);
  transform: translateY(-2px);
}

.secondary-btn ::v-deep span {
  display: flex;
  align-items: center;
  gap: 6px;
}

/* ── 入场动画 ── */
.fade-in-up {
  animation: fadeInUp 0.7s var(--ease-out-expo) both;
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}

/* ── 响应式 ── */
@media (max-width: 600px) {
  .not-found-card {
    padding: 40px 24px 36px;
  }

  .digit {
    font-size: 80px;
  }

  .error-ring {
    right: -10px;
    bottom: 5px;
    width: 44px;
    height: 44px;
  }

  .ring-inner {
    width: 32px;
    height: 32px;
    font-size: 18px;
  }

  .error-title {
    font-size: 20px;
  }

  .action-buttons {
    flex-direction: column;
    align-items: center;
  }

  .action-buttons .el-button {
    width: 200px;
  }
}
</style>
