<template>
  <div class="not-found-container">
    <div class="not-found-card">
      <div class="error-illustration">
        <div class="error-code">404</div>
        <div class="error-icon">
          <i class="el-icon-warning-outline"></i>
        </div>
      </div>
      <h2 class="error-title">页面未找到</h2>
      <p class="error-description">
        您访问的页面不存在或已被移除。<br />
        请检查网址是否正确，或返回首页继续浏览。
      </p>
      <div class="action-buttons">
        <el-button type="primary" round @click="$router.push('/works/explore')">
          返回首页
        </el-button>
        <el-button round @click="$router.go(-1)">
          返回上一页
        </el-button>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'NotFound'
}
</script>

<style scoped>
.not-found-container {
  min-height: 60vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px 24px;
}

.not-found-card {
  text-align: center;
  max-width: 480px;
  width: 100%;
  padding: 48px 40px;
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
  border: 1px solid #e5e6eb;
}

.error-illustration {
  margin-bottom: 24px;
}

.error-code {
  font-size: 72px;
  font-weight: 700;
  color: #165dff;
  line-height: 1;
  letter-spacing: -2px;
  margin-bottom: 8px;
}

.error-icon {
  font-size: 48px;
  color: #f7ba1e;
}

.error-title {
  font-size: 22px;
  font-weight: 600;
  color: #1d2129;
  margin: 0 0 12px 0;
}

.error-description {
  font-size: 14px;
  color: #86909c;
  line-height: 1.8;
  margin: 0 0 32px 0;
}

.action-buttons {
  display: flex;
  gap: 12px;
  justify-content: center;
}
</style>
