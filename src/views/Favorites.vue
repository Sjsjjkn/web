<template>
  <div class="favorites-container">
    <header class="favorites-header slide-down">
      <div class="header-content">
        <div class="logo-area">
          <div class="logo-icon">BDU</div>
          <h1 class="system-title">我的收藏</h1>
        </div>
        <div class="header-actions">
          <el-button 
            type="primary" 
            round 
            class="refresh-btn"
            icon="el-icon-refresh"
            @click="loadCollections"
          >
            刷新列表
          </el-button>
        </div>
      </div>
    </header>

    <section class="filter-section fade-in-up">
      <div class="search-bar">
        <el-input 
          placeholder="搜索作品标题或作者..." 
          v-model="searchQuery"
          prefix-icon="el-icon-search"
          clearable
          class="animated-input"
          @keyup.enter="loadCollections"
        >
        </el-input>
      </div>
      
      <div class="category-filters">
        <span 
          v-for="(cat, index) in categories" 
          :key="cat.value"
          :class="['filter-tag', { active: activeCategory === cat.value }]"
          @click="activeCategory = cat.value; loadCollections()"
          :style="{ animationDelay: `${index * 0.05 + 0.2}s` }"
          class="stagger-fade-in"
        >
          {{ cat.label }}
        </span>
      </div>
    </section>

    <section class="works-section">
      <el-empty v-if="collections.length === 0 && !loading" description="暂无收藏作品"></el-empty>
      
      <el-row :gutter="24">
        <el-col 
          :xs="24" :sm="12" :md="8" :lg="6" 
          v-for="(collection, index) in collections" 
          :key="collection.id"
          class="stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.3}s` }"
        >
          <el-card class="work-card" shadow="never" :body-style="{ padding: '0px' }">
            <div class="cover-image" :style="{ backgroundColor: getCategoryColor(collection.work.category) }">
              <span class="category-badge">{{ collection.work.category }}</span>
              <div class="hover-overlay">
                <el-button 
                  type="primary" 
                  circle 
                  icon="el-icon-view"
                  @click="handleViewWork(collection.work)"
                ></el-button>
              </div>
            </div>
            
            <div class="work-info">
              <h3 class="work-title">{{ collection.work.title }}</h3>
              <p class="work-author">作者: {{ collection.work.uploadUserName || '未知作者' }}</p>
              
              <div class="work-stats">
                <span><i class="el-icon-time"></i> {{ formatDate(collection.collectionDate) }}</span>
                <span><i class="el-icon-folder-opened"></i> {{ collection.work.category }}</span>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <div v-if="loading" class="loading-container">
        <el-spinner type="el-icon-loading" size="50px"></el-spinner>
        <p>加载中...</p>
      </div>
    </section>

    <div class="pagination-section">
      <el-pagination
        :current-page="currentPage"
        :page-size="pageSize"
        :total="total"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
        layout="total, sizes, prev, pager, next, jumper"
        background
      />
    </div>

    <el-dialog
      title="作品详情"
      :visible.sync="detailDialogVisible"
      width="800px"
      class="modern-dialog"
    >
      <div v-if="currentWork" class="work-detail">
        <h3>{{ currentWork.title }}</h3>
        <div class="work-meta">
          <span>作者：{{ currentWork.uploadUserName || '未知作者' }}</span>
          <span>分类：{{ currentWork.category }}</span>
          <span>上传时间：{{ formatDate(currentWork.uploadDate) }}</span>
          <span>状态：
            <el-tag :type="currentWork.status === '已发布' ? 'success' : 'info'">
              {{ currentWork.status }}
            </el-tag>
          </span>
        </div>
        <div class="work-description">
          <h4>作品描述</h4>
          <p>{{ currentWork.description }}</p>
        </div>
        <div class="work-files">
          <h4>作品文件</h4>
          <el-button type="primary" size="small" @click="handleDownloadFile(currentWork)">下载文件</el-button>
        </div>
      </div>
      <div slot="footer" class="dialog-footer">
        <el-button @click="detailDialogVisible = false" round>关闭</el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import http from '../api/http'
import { downloadFile } from '../api/file'

export default {
  name: 'Favorites',
  data() {
    return {
      searchQuery: '',
      activeCategory: '',
      collections: [],
      total: 0,
      currentPage: 1,
      pageSize: 12,
      loading: false,
      detailDialogVisible: false,
      currentWork: null,
      categories: [
        { label: '全部', value: '' },
        { label: '前端开发', value: '前端开发' },
        { label: '后端开发', value: '后端开发' },
        { label: '人工智能', value: '人工智能' },
        { label: 'UI设计', value: 'UI设计' },
        { label: '其他', value: '其他' }
      ]
    }
  },
  mounted() {
    this.loadCollections()
  },
  methods: {
    async loadCollections() {
      this.loading = true
      try {
        // 收藏列表接口需要登录；token 会由统一的 http 实例自动注入
        const response = await http.get('/api/Collection', {
          params: {
            search: this.searchQuery,
            category: this.activeCategory,
            page: this.currentPage,
            pageSize: this.pageSize
          }
        })
        this.collections = response.data.items || response.data
        this.total = response.data.total || this.collections.length
      } catch (error) {
        // 统一提示：避免把技术细节暴露给用户
        this.$message.error(error.response?.data?.message || '加载收藏失败')
        console.error('加载收藏失败:', error)
      } finally {
        this.loading = false
      }
    },
    handleViewWork(work) {
      this.currentWork = work
      this.detailDialogVisible = true
    },
    handleDownloadFile(work) {
      if (!work || !work.filePath) {
        this.$message.error('文件路径不存在，无法下载')
        return
      }
      try {
        // 通过后端下载接口获取文件流（相对路径 /api，由开发代理转发）
        downloadFile({ fileName: work.filePath })
          .then(res => {
            const blob = new Blob([res.data])
            const url = window.URL.createObjectURL(blob)
            const link = document.createElement('a')
            link.href = url
            link.download = work.fileName || work.filePath
            document.body.appendChild(link)
            link.click()
            document.body.removeChild(link)
            window.URL.revokeObjectURL(url)
            this.$message.success('文件下载已开始')
          })
          .catch(err => {
            this.$message.error(err.response?.data?.message || '文件下载失败')
            console.error('下载文件失败:', err)
          })
      } catch (error) {
        this.$message.error('文件下载失败')
        console.error('下载文件失败:', error)
      }
    },
    handleSizeChange(size) {
      this.pageSize = size
      this.loadCollections()
    },
    handleCurrentChange(current) {
      this.currentPage = current
      this.loadCollections()
    },
    getCategoryColor(category) {
      const colorMap = {
        '前端开发': '#e6f7ff',
        '后端开发': '#f6ffed',
        '人工智能': '#fff0f6',
        'UI设计': '#f0f5ff',
        '其他': '#fff7e6'
      }
      return colorMap[category] || '#e6f7ff'
    },
    formatDate(dateStr) {
      const date = new Date(dateStr)
      return date.toLocaleDateString('zh-CN')
    }
  }
}
</script>

<style scoped>
/* 基础设置 */
.favorites-container {
  min-height: 100vh;
  background-color: #f8fafc;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  overflow-x: hidden;
}

/* 1. 顶部导航 (滑入动画) */
.favorites-header {
  display: flex;
  justify-content: center;
  padding: 0 40px;
  height: 64px;
  background-color: rgba(255,255,255,0.9);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(0,0,0,0.05);
  position: sticky;
  top: 0;
  z-index: 100;
}

.slide-down {
  animation: slideDown 0.5s ease-out;
}

.header-content {
  width: 100%;
  max-width: 1200px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo-area { display: flex; align-items: center; gap: 12px; }
.logo-icon { background: #0052D9; color: white; font-weight: bold; padding: 4px 8px; border-radius: 6px; }
.system-title { font-size: 20px; font-weight: 600; color: #1a1a1a; margin: 0; }

/* 2. 按钮悬停动画 */
.refresh-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.refresh-btn:hover {
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
  transform: scale(1.02);
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
  transform: scale(1.05);
}

/* 6. 作品卡片 (交错上滑淡入 & 高级悬停) */
.works-section { max-width: 1200px; margin: 0 auto; padding: 0 24px 60px; }

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
  cursor: pointer;
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
  align-items: flex-start;
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

.work-info { padding: 16px; }
.work-title { font-size: 16px; font-weight: 600; margin: 0 0 8px 0; color: #333; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; transition: color 0.3s; }
.work-card:hover .work-title { color: #0052D9; }
.work-author { font-size: 13px; color: #888; margin: 0 0 16px 0; }
.work-stats { display: flex; gap: 16px; font-size: 13px; color: #999; }
.work-stats span { display: flex; align-items: center; gap: 4px; }

/* 加载状态 */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 40px 0;
  gap: 16px;
}

/* 分页区域 */
.pagination-section {
  max-width: 1200px;
  margin: 0 auto 40px;
  padding: 0 24px;
  display: flex;
  justify-content: center;
}

/* 对话框样式 */
::v-deep .modern-dialog .el-dialog {
  border-radius: 12px;
  overflow: hidden;
}

::v-deep .modern-dialog .el-dialog__header {
  background: #f8fafc;
  padding: 20px 24px;
  border-bottom: 1px solid #e4e7ed;
}

::v-deep .modern-dialog .el-dialog__body {
  padding: 24px;
}

.work-detail h3 {
  margin: 0 0 16px 0;
  color: #333;
  font-size: 20px;
}

.work-meta {
  display: flex;
  gap: 16px;
  margin-bottom: 20px;
  font-size: 14px;
  color: #666;
}

.work-description {
  margin-bottom: 20px;
}

.work-description h4 {
  margin: 0 0 12px 0;
  color: #333;
  font-size: 16px;
}

.work-description p {
  color: #666;
  line-height: 1.6;
}

.work-files h4 {
  margin: 0 0 12px 0;
  color: #333;
  font-size: 16px;
}

.dialog-footer {
  text-align: right;
}

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