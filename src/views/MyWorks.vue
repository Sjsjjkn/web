<template>
  <div class="my-works-container">
    <header class="my-works-header slide-down">
      <div class="header-content">
        <div class="logo-area">
          <div class="logo-icon">BDU</div>
          <h1 class="system-title">我的作品集</h1>
        </div>
        <div class="header-actions">
          <el-button 
            type="primary" 
            round 
            class="upload-btn"
            icon="el-icon-plus"
            @click="handleAddWork"
          >
            上传作品
          </el-button>
        </div>
      </div>
    </header>

    <section class="filter-section fade-in-up">
      <div class="search-bar">
        <el-input 
          placeholder="搜索作品标题..." 
          v-model="filterForm.search"
          prefix-icon="el-icon-search"
          clearable
          class="animated-input"
          @keyup.enter="loadWorks"
        >
        </el-input>
      </div>
      
      <div class="category-filters">
        <span 
          v-for="(cat, index) in categories" 
          :key="cat.value"
          :class="['filter-tag', { active: activeCategory === cat.value }]"
          @click="activeCategory = cat.value; loadWorks()"
          :style="{ animationDelay: `${index * 0.05 + 0.2}s` }"
          class="stagger-fade-in"
        >
          {{ cat.label }}
        </span>
      </div>

      <div class="status-filters">
        <span 
          v-for="(status, index) in statuses" 
          :key="status.value"
          :class="['filter-tag', { active: activeStatus === status.value }]"
          @click="activeStatus = status.value; loadWorks()"
          :style="{ animationDelay: `${index * 0.05 + 0.3}s` }"
          class="stagger-fade-in"
        >
          {{ status.label }}
        </span>
      </div>
    </section>

    <section class="works-section">
      <el-empty v-if="works.length === 0 && !loading" description="暂无上传作品">
        <el-button type="primary" @click="handleAddWork" round>
          上传第一个作品
        </el-button>
      </el-empty>
      
      <el-row :gutter="24">
        <el-col 
          :xs="24" :sm="12" :md="8" :lg="6" 
          v-for="(work, index) in works" 
          :key="work.id"
          class="stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.4}s` }"
        >
          <el-card class="work-card" shadow="never" :body-style="{ padding: '0px' }">
            <div class="cover-image" :style="{ backgroundColor: getCategoryColor(work.category) }">
              <span class="category-badge">{{ work.category }}</span>
              <span class="status-badge" :class="work.status === '已发布' ? 'published' : 'draft'">
                {{ work.status }}
              </span>
              <div class="hover-overlay">
                <el-button 
                  type="primary" 
                  circle 
                  icon="el-icon-view"
                  @click="handleViewWork(work)"
                ></el-button>
              </div>
            </div>
            
            <div class="work-info">
              <h3 class="work-title">{{ work.title }}</h3>
              <p class="work-author">作者: {{ work.uploadUserName || '未知作者' }}</p>
              
              <div class="work-stats">
                <span><i class="el-icon-time"></i> {{ formatDate(work.uploadDate) }}</span>
                <span><i class="el-icon-folder-opened"></i> {{ work.category }}</span>
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
      :title="''"
      :visible.sync="detailDialogVisible"
      width="900px"
      :custom-class="'work-detail-dialog'"
      :close-on-click-modal="false"
    >
      <div v-if="currentWork" class="work-detail-content">
        <div class="work-header">
          <div class="work-cover" :style="{ backgroundColor: getRandomColor(currentWork.id) }">
            <span class="category-badge">{{ currentWork.category }}</span>
          </div>
          <div class="work-info-header">
            <h2 class="work-title-detail">{{ currentWork.title }}</h2>
            <div class="work-author-info">
              <span class="author-name">{{ currentWork.uploadUserName || '未知作者' }}</span>
              <span class="upload-time">{{ formatDate(currentWork.uploadDate) }}</span>
              <el-tag :type="currentWork.status === '已发布' ? 'success' : 'info'" size="small">
                {{ currentWork.status || '未知' }}
              </el-tag>
            </div>
            <div class="work-stats-detail">
              <span class="stat-item">
                <i class="el-icon-view"></i>
                <span>{{ currentWork.views || 0 }}</span>
                <span class="stat-label">浏览</span>
              </span>
              <span class="stat-item">
                <i class="el-icon-star-off"></i>
                <span>{{ currentWork.favorites || 0 }}</span>
                <span class="stat-label">收藏</span>
              </span>
              <span class="stat-item">
                <i class="el-icon-chat-line-round"></i>
                <span>{{ currentWork.comments || 0 }}</span>
                <span class="stat-label">评论</span>
              </span>
            </div>
          </div>
        </div>
        
        <div class="work-description-detail">
          <h3 class="section-title-detail">作品描述</h3>
          <div class="description-content">
            {{ currentWork.description || '该作品暂无描述' }}
          </div>
        </div>
        
        <div class="work-file-info">
          <h3 class="section-title-detail">文件信息</h3>
          <div class="file-details">
            <span class="file-item">
              <i class="el-icon-document"></i>
              <span>{{ currentWork.fileName || '未知文件名' }}</span>
            </span>
            <span class="file-item">
              <i class="el-icon-folder"></i>
              <span>{{ currentWork.category || '未分类' }}</span>
            </span>
            <span v-if="currentWork.fileSize" class="file-item">
              <i class="el-icon-data-line"></i>
              <span>{{ formatFileSize(currentWork.fileSize) }}</span>
            </span>
          </div>
        </div>
      </div>
      <div slot="footer" class="dialog-footer-detail">
        <el-button @click="detailDialogVisible = false" class="close-btn">
          <i class="el-icon-close"></i> 关闭
        </el-button>
        <el-button type="info" @click="handleFavoriteWork(currentWork)" class="favorite-btn">
          <i class="el-icon-star-off"></i> 收藏
        </el-button>
        <el-button type="primary" @click="handleDownloadFile(currentWork)" class="download-btn">
          <i class="el-icon-download"></i> 下载文件
        </el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import { getWorks, addView, favoriteWork, unfavoriteWork, isFavorite } from '../api/work'
import { downloadFile } from '../api/file'

export default {
  name: 'MyWorks',
  data() {
    return {
      works: [],
      loading: false,
      currentPage: 1,
      pageSize: 12,
      total: 0,
      filterForm: {
        search: '',
        category: '',
        status: ''
      },
      activeCategory: '',
      activeStatus: '',
      currentWork: null,
      detailDialogVisible: false,
      categories: [
        { label: '全部', value: '' },
        { label: '前端开发', value: '前端开发' },
        { label: '后端开发', value: '后端开发' },
        { label: '人工智能', value: '人工智能' },
        { label: '设计', value: '设计' },
        { label: '其他', value: '其他' }
      ],
      statuses: [
        { label: '全部', value: '' },
        { label: '草稿', value: '草稿' },
        { label: '已发布', value: '已发布' }
      ]
    }
  },
  mounted() {
    this.loadWorks()
  },
  methods: {
    async loadWorks() {
      this.loading = true
      try {
        // 作品列表接口需要登录；token 会由统一 http 实例自动注入
        const response = await getWorks({
          search: this.filterForm.search,
          category: this.activeCategory,
          status: this.activeStatus,
          page: this.currentPage,
          pageSize: this.pageSize
        })
        
        this.works = response.data.items || []
        this.total = response.data.total || 0
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载作品失败')
        console.error('加载作品失败:', error)
      } finally {
        this.loading = false
      }
    },
    handleViewWork(work) {
      this.currentWork = work
      this.detailDialogVisible = true
    },
    handleAddWork() {
      this.$router.push('/works/manage')
    },
    handleDownloadFile(work) {
      if (!work || !work.filePath) {
        this.$message.error('文件路径不存在，无法下载')
        return
      }
      // 文件下载：统一走 /api 相对路径，由开发代理转发
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
    },
    handleSizeChange(size) {
      this.pageSize = size
      this.loadWorks()
    },
    handleCurrentChange(current) {
      this.currentPage = current
      this.loadWorks()
    },
    getCategoryColor(category) {
      const colorMap = {
        '前端开发': '#e6f7ff',
        '后端开发': '#f6ffed',
        '人工智能': '#fff0f6',
        '设计': '#f0f5ff',
        '其他': '#fff7e6'
      }
      return colorMap[category] || '#e6f7ff'
    },
    formatDate(dateStr) {
      const date = new Date(dateStr)
      return date.toLocaleDateString('zh-CN')
    },
    formatFileSize(bytes) {
      if (!bytes) return '0 B'
      const k = 1024
      const sizes = ['B', 'KB', 'MB', 'GB']
      const i = Math.floor(Math.log(bytes) / Math.log(k))
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
    },
    // 生成随机颜色
    getRandomColor(id) {
      const colors = [
        '#e6f7ff', '#f6ffed', '#fff7e6', '#fff0f6', '#f0f5ff',
        '#e8f5e8', '#fff3e0', '#f3e5f5', '#e3f2fd', '#fffde7'
      ]
      return colors[id % colors.length]
    },
    // 查看作品详情
    async handleViewWork(work) {
      this.currentWork = work
      this.detailDialogVisible = true
      
      // 增加浏览量
      try {
        await addView(work.id)
        // 更新本地数据
        work.views = (work.views || 0) + 1
      } catch (error) {
        console.error('增加浏览量失败:', error)
      }
    },
    // 收藏作品
    async handleFavoriteWork(work) {
      try {
        const response = await favoriteWork(work.id)
        this.$message.success('收藏成功')
        // 更新本地数据
        work.favorites = response.data.favorites
      } catch (error) {
        if (error.response && error.response.data && error.response.data.message) {
          this.$message.error(error.response.data.message)
        } else {
          this.$message.error('收藏失败，请稍后重试')
        }
        console.error('收藏作品失败:', error)
      }
    },
    // 取消收藏
    async handleUnfavoriteWork(work) {
      try {
        const response = await unfavoriteWork(work.id)
        this.$message.success('取消收藏成功')
        // 更新本地数据
        work.favorites = response.data.favorites
      } catch (error) {
        if (error.response && error.response.data && error.response.data.message) {
          this.$message.error(error.response.data.message)
        } else {
          this.$message.error('取消收藏失败，请稍后重试')
        }
        console.error('取消收藏失败:', error)
      }
    },
    // 检查收藏状态
    async checkFavoriteStatus(workId) {
      try {
        const response = await isFavorite(workId)
        return !!response.data.isFavorite
      } catch (error) {
        console.error('检查收藏状态失败:', error)
        return false
      }
    }
  }
}
</script>

<style scoped>
/* 基础设置 */
.my-works-container {
  min-height: 100vh;
  background-color: #f8fafc;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  overflow-x: hidden;
}

/* 1. 顶部导航 (滑入动画) */
.my-works-header {
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
.upload-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.upload-btn:hover {
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
.category-filters, .status-filters { 
  display: flex; 
  gap: 12px; 
  flex-wrap: wrap; 
  justify-content: center; 
}

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
  justify-content: space-between;
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

.status-badge {
  background: rgba(0, 0, 0, 0.5);
  color: white;
  font-size: 12px;
  padding: 4px 10px;
  border-radius: 12px;
  backdrop-filter: blur(4px);
  z-index: 2;
}

.status-badge.published {
  background: rgba(82, 196, 26, 0.8);
}

.status-badge.draft {
  background: rgba(144, 147, 153, 0.8);
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

.work-files p {
  color: #666;
  margin: 4px 0;
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

/* 作品详情对话框样式 */
.work-detail-dialog {
  border-radius: 16px;
  overflow: hidden;
}

.work-detail-content {
  padding: 0;
}

.work-header {
  display: flex;
  padding: 32px;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e7ed 100%);
  border-bottom: 1px solid #e4e7ed;
}

.work-cover {
  width: 180px;
  height: 180px;
  border-radius: 12px;
  display: flex;
  align-items: flex-start;
  justify-content: flex-end;
  padding: 12px;
  margin-right: 24px;
  flex-shrink: 0;
}

.work-info-header {
  flex: 1;
}

.work-title-detail {
  font-size: 24px;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 16px 0;
  line-height: 1.3;
}

.work-author-info {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.author-name {
  font-size: 16px;
  color: #333;
  font-weight: 500;
}

.upload-time {
  font-size: 14px;
  color: #888;
}

.work-stats-detail {
  display: flex;
  gap: 32px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.stat-item i {
  font-size: 20px;
  color: #0052D9;
}

.stat-item span:first-of-type {
  font-size: 18px;
  font-weight: 600;
  color: #333;
}

.stat-label {
  font-size: 12px;
  color: #888;
}

.work-description-detail,
.work-file-info {
  padding: 32px;
  border-bottom: 1px solid #f0f0f0;
}

.work-description-detail:last-child {
  border-bottom: none;
}

.section-title-detail {
  font-size: 18px;
  font-weight: 600;
  color: #333;
  margin: 0 0 16px 0;
  position: relative;
  display: inline-block;
}

.section-title-detail::after {
  content: '';
  position: absolute;
  bottom: -6px;
  left: 0;
  width: 40px;
  height: 3px;
  background: #0052D9;
  border-radius: 2px;
}

.description-content {
  font-size: 15px;
  line-height: 1.6;
  color: #666;
  white-space: pre-wrap;
}

.file-details {
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
}

.file-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #666;
}

.file-item i {
  color: #0052D9;
}

.dialog-footer-detail {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 24px 32px;
  background: #f8fafc;
  border-top: 1px solid #f0f0f0;
}

.close-btn,
.favorite-btn,
.download-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.close-btn:hover {
  color: #0052D9;
  border-color: #0052D9;
}

.favorite-btn:hover {
  color: #ff9800;
  border-color: #ff9800;
}

.download-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 82, 217, 0.3);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .work-header {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }
  
  .work-cover {
    margin-right: 0;
    margin-bottom: 20px;
  }
  
  .work-author-info {
    justify-content: center;
  }
  
  .work-stats-detail {
    justify-content: center;
  }
  
  .work-description-detail,
  .work-file-info {
    padding: 20px;
  }
  
  .dialog-footer-detail {
    padding: 16px 20px;
  }
}
</style>