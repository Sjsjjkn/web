<template>
  <div class="gallery-container">
    <header class="gallery-header slide-down">
      <div class="logo-area">
        <div class="logo-icon">BDU</div>
        <h1 class="system-title">保定学院数字作品展厅</h1>
      </div>
      <div class="user-actions">
        <el-button type="text" class="hover-link">作品上传</el-button>
        <el-button 
          type="primary" 
          round 
          class="login-btn"
          :loading="isLoggingIn"
          @click="handleLogin"
        >
          {{ isLoggingIn ? '登录中...' : '登录 / 注册' }}
        </el-button>
      </div>
    </header>

    <section class="filter-section fade-in-up">
      <div class="search-bar">
        <el-input 
          placeholder="搜索作品名称或作者..." 
          v-model="searchQuery"
          prefix-icon="el-icon-search"
          clearable
          class="animated-input">
        </el-input>
      </div>
      
      <div class="category-filters">
        <span 
          v-for="(cat, index) in categories" 
          :key="cat"
          :class="['filter-tag', { active: activeCategory === cat }]"
          @click="activeCategory = cat"
          :style="{ animationDelay: `${index * 0.05 + 0.2}s` }"
          class="stagger-fade-in"
        >
          {{ cat }}
        </span>
      </div>
    </section>

    <section class="works-grid">
      <el-empty v-if="filteredWorks.length === 0" description="未找到相关作品"></el-empty>
      
      <el-row :gutter="24">
        <el-col 
          :xs="24" :sm="12" :md="8" :lg="6" 
          v-for="(work, index) in filteredWorks" 
          :key="work.id"
          class="stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.3}s` }"
        >
          <el-card class="work-card" shadow="never" :body-style="{ padding: '0px' }">
            <div class="cover-image" :style="{ backgroundColor: work.color }">
              <span class="category-badge">{{ work.category }}</span>
              <div class="hover-overlay">
                <el-button type="primary" circle icon="el-icon-caret-right"></el-button>
              </div>
            </div>
            
            <div class="work-info">
              <h3 class="work-title">{{ work.title }}</h3>
              <p class="work-author">作者: {{ work.author }}</p>
              
              <div class="work-stats">
                <span><i class="el-icon-view"></i> {{ work.views }}</span>
                <span><i class="el-icon-star-off"></i> {{ work.likes }}</span>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </section>
  </div>
</template>

<script>
export default {
  name: 'DigitalGallery',
  data() {
    return {
      searchQuery: '',
      activeCategory: '全部',
      isLoggingIn: false, // 控制登录加载状态
      categories: ['全部', '视频动画', '软件源码', '视觉设计', '文档论述'],
      worksList: [
        { id: 1, title: '保定学院航拍微电影', author: '张同学', category: '视频动画', views: 1205, likes: 342, color: '#e6f7ff' },
        { id: 2, title: '校园二手交易平台', author: '李同学', category: '软件源码', views: 890, likes: 125, color: '#f6ffed' },
        { id: 3, title: '2026届毕业展海报设计', author: '王同学', category: '视觉设计', views: 2341, likes: 560, color: '#fff0f6' },
        { id: 4, title: '基于大数据的学情分析', author: '赵同学', category: '文档论述', views: 450, likes: 88, color: '#f0f5ff' },
        { id: 5, title: '迎新晚会混音剪辑', author: '刘同学', category: '视频动画', views: 3200, likes: 890, color: '#e6f7ff' },
        { id: 6, title: '图书馆座位预约小程序', author: '陈同学', category: '软件源码', views: 1560, likes: 210, color: '#f6ffed' },
        { id: 7, title: '人工智能学院Logo重塑', author: '杨同学', category: '视觉设计', views: 980, likes: 145, color: '#fff0f6' },
        { id: 8, title: '数据结构交互式演示', author: '周同学', category: '软件源码', views: 760, likes: 198, color: '#f6ffed' }
      ]
    };
  },
  computed: {
    filteredWorks() {
      return this.worksList.filter(work => {
        const matchCategory = this.activeCategory === '全部' || work.category === this.activeCategory;
        const matchSearch = work.title.includes(this.searchQuery) || work.author.includes(this.searchQuery);
        return matchCategory && matchSearch;
      });
    }
  },
  methods: {
    handleLogin() {
      this.isLoggingIn = true;
      // 模拟网络请求延迟
      setTimeout(() => {
        this.isLoggingIn = false;
        this.$message.success('登录界面跳转中...');
      }, 1500);
    }
  }
}
</script>

<style scoped>
/* 基础设置 */
.gallery-container {
  min-height: 100vh;
  background-color: #f8fafc; /* 换用更柔和的背景色 */
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  overflow-x: hidden;
}

/* 1. 顶部导航 (滑入动画) */
.gallery-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 40px;
  height: 64px;
  background-color: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px); /* 导航栏毛玻璃效果 */
  border-bottom: 1px solid rgba(0,0,0,0.05);
  position: sticky;
  top: 0;
  z-index: 100;
}

.slide-down {
  animation: slideDown 0.5s ease-out;
}

.logo-area { display: flex; align-items: center; gap: 12px; }
.logo-icon { background: #0052D9; color: white; font-weight: bold; padding: 4px 8px; border-radius: 6px; }
.system-title { font-size: 20px; font-weight: 600; color: #1a1a1a; margin: 0; }

/* 2. 按钮悬停动画 */
.hover-link {
  color: #666;
  transition: color 0.3s, transform 0.3s;
}
.hover-link:hover {
  color: #0052D9;
  transform: translateY(-2px);
}
.login-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.login-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 82, 217, 0.3);
}

/* 3. 搜索与筛选 (上滑淡入) */
.filter-section {
  max-width: 1200px;
  margin: 40px auto 32px;
  padding: 0 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 24px;
}

.fade-in-up {
  animation: fadeInUp 0.6s ease-out both;
}

/* 4. 输入框聚焦动画 */
.search-bar { width: 100%; max-width: 600px; }
::v-deep .animated-input .el-input__inner {
  border-radius: 24px;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0,0,0,0.02);
}
::v-deep .animated-input .el-input__inner:focus {
  box-shadow: 0 4px 16px rgba(0, 82, 217, 0.15);
  border-color: #0052D9;
  transform: scale(1.02); /* 聚焦时微微放大 */
}

/* 5. 分类标签 (交错加载 & 悬停) */
.category-filters { display: flex; gap: 12px; flex-wrap: wrap; justify-content: center; }
.filter-tag {
  padding: 8px 20px;
  border-radius: 20px;
  background: #ffffff;
  color: #666;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  border: 1px solid #e4e7ed;
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
}
.filter-tag:hover {
  color: #0052D9;
  border-color: #0052D9;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.05);
}
.filter-tag.active {
  background: #0052D9;
  color: #ffffff;
  border-color: #0052D9;
  transform: scale(1.05); /* 激活时放大 */
}

/* 6. 作品卡片 (交错上滑淡入 & 高级悬停) */
.works-grid { max-width: 1200px; margin: 0 auto; padding: 0 24px 60px; }

.stagger-fade-in {
  opacity: 0;
  animation: fadeInUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.work-card {
  margin-bottom: 24px;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(0,0,0,0.05);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  background: white;
}

/* 卡片整体悬停效果 */
.work-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 16px 32px rgba(0, 0, 0, 0.1);
}

.cover-image {
  height: 180px;
  width: 100%;
  position: relative;
  display: flex;
  align-items: flex-start; /* 标签移到顶部 */
  justify-content: flex-end;
  padding: 12px;
  overflow: hidden;
}

.category-badge {
  background: rgba(0, 0, 0, 0.5);
  color: white;
  font-size: 12px;
  padding: 4px 10px;
  border-radius: 12px;
  backdrop-filter: blur(4px);
  z-index: 2;
}

/* 卡片内部遮罩悬停动画 */
.hover-overlay {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0, 82, 217, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: all 0.3s ease;
  backdrop-filter: blur(2px);
}

.hover-overlay .el-button {
  transform: scale(0.5);
  transition: transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.work-card:hover .hover-overlay { opacity: 1; }
.work-card:hover .hover-overlay .el-button { transform: scale(1); }

/* 图片放大动画效果 (假设背景图或img标签) */
.work-card:hover .cover-image {
  /* 如果是用 background-image，可以通过调整 background-size 实现放大 */
  /* 如果是 img 标签，可以加上 transform: scale(1.05); */
}

.work-info { padding: 16px; }
.work-title { font-size: 16px; font-weight: 600; margin: 0 0 8px 0; color: #333; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; transition: color 0.3s; }
.work-card:hover .work-title { color: #0052D9; } /* 标题悬停变色 */
.work-author { font-size: 13px; color: #888; margin: 0 0 16px 0; }
.work-stats { display: flex; gap: 16px; font-size: 13px; color: #999; }
.work-stats span { display: flex; align-items: center; gap: 4px; }

/* 关键帧定义 */
@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@keyframes fadeInUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}
</style>