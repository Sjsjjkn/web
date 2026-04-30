<template>
  <div class="gallery-container">
    <!-- 顶部导航 -->
    <header class="gallery-header slide-down">
      <div class="logo-area">
        <div class="logo-icon">BDU</div>
        <h1 class="system-title">保定学院数字作品展厅</h1>
      </div>
    </header>

    <!-- 页面头部 -->
    <section class="page-header fade-in-up">
      <h2>作品展示</h2>
      <p>浏览和欣赏保定学院师生的优秀数字作品</p>
    </section>
    
    <!-- 筛选和搜索 -->
    <section class="filter-section fade-in-up">
      <div class="search-bar">
        <el-input 
          v-model="searchQuery"
          placeholder="搜索作品标题或作者..." 
          prefix-icon="el-icon-search"
          clearable
          class="animated-input"
          @keyup.enter="loadWorks"
        ></el-input>
      </div>
      
      <div class="category-filters">
        <span 
          v-for="(cat, index) in categories" 
          :key="cat.value"
          :class="['filter-tag', { active: categoryFilter === cat.value }]"
          @click="categoryFilter = cat.value; loadWorks()"
          :style="{ animationDelay: `${index * 0.05 + 0.2}s` }"
          class="stagger-fade-in"
        >
          {{ cat.label }}
        </span>
      </div>
      
      <div class="sort-options">
        <span 
          v-for="(sort, index) in sortOptions" 
          :key="sort.value"
          :class="['filter-tag', { active: sortBy === sort.value }]"
          @click="sortBy = sort.value; loadWorks()"
          :style="{ animationDelay: `${(index + 5) * 0.05 + 0.2}s` }"
          class="stagger-fade-in"
        >
          {{ sort.label }}
        </span>
      </div>
    </section>
    
    <!-- 作品网格展示 -->
    <section class="works-grid">
      <el-empty v-if="works.length === 0 && !loading" description="未找到相关作品"></el-empty>
      
      <el-row :gutter="24">
        <el-col 
          :xs="24" :sm="12" :md="8" :lg="6" 
          v-for="(work, index) in works" 
          :key="work.id"
          class="stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.3}s` }"
        >
          <el-card class="work-card" shadow="never" :body-style="{ padding: '0px' }" @click="handleViewWork(work)">
            <div class="cover-image">
              <img v-if="getThumbnailUrl(work)" :src="getThumbnailUrl(work)" :alt="work.title" class="thumbnail-image">
              <div v-else class="thumbnail-placeholder" :style="{ backgroundColor: getRandomColor(work.id) }">
                <i class="el-icon-document"></i>
                <span>{{ getFileExtension(work) }}</span>
              </div>
              <span class="category-badge">{{ work.category }}</span>
              <div class="hover-overlay">
                <el-button type="primary" circle icon="el-icon-caret-right" @click.stop="handleViewWork(work)"></el-button>
              </div>
            </div>
            
            <div class="work-info">
              <h3 class="work-title">{{ work.title }}</h3>
              <p class="work-author">作者: {{ work.uploadUserName || '未知作者' }}</p>
              <p class="work-upload-user">上传用户: {{ work.uploadUserUsername }}</p>
              <p class="work-date">{{ work.uploadDate }}</p>
              
              <div class="work-actions">
                <el-button type="primary" size="small" @click.stop="handleViewWork(work)">查看详情</el-button>
                <el-button size="small" @click.stop="handleDownloadFile(work)">下载</el-button>
                <el-button 
                  size="small" 
                  @click.stop="handleToggleFavorite(work)"
                  :type="work.isFavorite ? 'warning' : 'default'"
                  :icon="work.isFavorite ? 'el-icon-star-on' : 'el-icon-star-off'"
                >
                  {{ work.isFavorite ? '已收藏' : '收藏' }}
                </el-button>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
      
      <!-- 分页 -->
      <div class="pagination" v-if="total > 0">
        <el-pagination
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
          :current-page="currentPage"
          :page-sizes="[12, 24, 36, 48]"
          :page-size="pageSize"
          layout="total, sizes, prev, pager, next, jumper"
          :total="total"
        ></el-pagination>
      </div>
      
      <!-- 加载中 -->
      <div v-if="loading" class="loading-container">
        <el-spinner type="el-icon-loading" size="50px"></el-spinner>
        <p>加载中...</p>
      </div>
    </section>
    
    <!-- 作品详情对话框 -->
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
              <span class="upload-time">{{ currentWork.uploadDate }}</span>
              <el-tag :type="currentWork.status === '已发布' ? 'success' : 'info'" size="small">
                {{ currentWork.status }}
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
    
    <!-- 文件预览弹窗（统一组件） -->
    <FilePreviewDialog
      v-model="previewDialogVisible"
      :file-path="currentWork?.filePath || ''"
      :file-name="currentWork?.fileName || ''"
      title="文件预览"
    />
  </div>
</template>

<script>
import { getWorks, addView, favoriteWork, unfavoriteWork, isFavorite } from '../api/work'
import { downloadFile } from '../api/file'
import { getUser, getToken, clearAuth } from '../utils/auth'
import FilePreviewDialog from '../components/FilePreviewDialog.vue'

export default {
  name: 'WorkDisplay',
  components: {
    FilePreviewDialog
  },
  data() {
    return {
      // 搜索和筛选
      searchQuery: '',
      categoryFilter: '',
      sortBy: 'uploadDate',
      
      // 分类和排序选项
      categories: [
        { label: '全部', value: '' },
        { label: '前端开发', value: '前端开发' },
        { label: '后端开发', value: '后端开发' },
        { label: '人工智能', value: '人工智能' },
        { label: 'UI设计', value: 'UI设计' },
        { label: '其他', value: '其他' }
      ],
      sortOptions: [
        { label: '最新上传', value: 'uploadDate' },
        { label: '热门程度', value: 'popularity' }
      ],
      
      // 分页
      currentPage: 1,
      pageSize: 12,
      total: 0,
      
      // 对话框
      detailDialogVisible: false,
      
      // 预览相关
      previewDialogVisible: false,
      
      // 当前查看的作品
      currentWork: null,
      
      // 作品数据
      works: [],
      
      // 加载状态
      loading: false,
      

    }
  },
  mounted() {
    this.loadWorks()
  },
  computed: {
    // 是否具备预览条件：有文件路径即可（具体类型由预览组件判断）
    canPreview() {
      return !!(this.currentWork && this.currentWork.filePath)
    }
  },
  methods: {
    // 加载作品列表
    async loadWorks() {
      this.loading = true
      try {
        // 作品展示页：优先走“需要登录的列表”，由后端按权限返回（管理员/普通用户）
        const response = await getWorks({
          search: this.searchQuery,
          category: this.categoryFilter,
          sortBy: this.sortBy,
          page: this.currentPage,
          pageSize: this.pageSize
        })
        this.works = response.data.items || response.data
        this.total = response.data.total || this.works.length
        
        // 为每个作品添加收藏状态
        await this.updateFavoriteStatuses()
      } catch (error) {
        console.error('加载作品失败', error)
        this.$message.error(error.response?.data?.message || '加载作品失败，请稍后重试')
      } finally {
        this.loading = false
      }
    },
    
    // 更新所有作品的收藏状态
    async updateFavoriteStatuses() {
      const token = getToken()
      if (!token) {
        // 用户未登录，所有作品都未收藏
        this.works.forEach(work => {
          work.isFavorite = false
        })
        return
      }
      
      // 并行检查所有作品的收藏状态
      await Promise.all(this.works.map(async (work) => {
        try {
          work.isFavorite = await this.checkFavoriteStatus(work.id)
        } catch (error) {
          console.error(`检查作品 ${work.id} 收藏状态失败:`, error)
          work.isFavorite = false
        }
      }))
    },
    
    // 处理查看作品
    async handleViewWork(row) {
      this.currentWork = row
      this.detailDialogVisible = true
      
      // 增加浏览量
      try {
        await addView(row.id)
        // 更新本地数据
        row.views = (row.views || 0) + 1;
      } catch (error) {
        console.error('增加浏览量失败:', error);
      }
    },
    
    // 收藏作品
    async handleFavoriteWork(work) {
      try {
        const response = await favoriteWork(work.id)
        this.$message.success('收藏成功');
        // 更新本地数据
        work.favorites = response.data.favorites;
      } catch (error) {
        if (error.response && error.response.data && error.response.data.message) {
          this.$message.error(error.response.data.message);
        } else {
          this.$message.error('收藏失败，请稍后重试');
        }
        console.error('收藏作品失败:', error);
      }
    },
    // 取消收藏
    async handleUnfavoriteWork(work) {
      try {
        const response = await unfavoriteWork(work.id)
        this.$message.success('取消收藏成功');
        // 更新本地数据
        work.favorites = response.data.favorites;
      } catch (error) {
        if (error.response && error.response.data && error.response.data.message) {
          this.$message.error(error.response.data.message);
        } else {
          this.$message.error('取消收藏失败，请稍后重试');
        }
        console.error('取消收藏失败:', error);
      }
    },
    // 检查收藏状态
    async checkFavoriteStatus(workId) {
      try {
        if (!getToken()) return false
        const response = await isFavorite(workId)
        return !!response.data.isFavorite
      } catch (error) {
        console.error('检查收藏状态失败:', error);
        return false;
      }
    },
    
    // 处理文件下载
    handleDownloadFile(work) {
      if (!work || !work.filePath) {
        this.$message.error('文件路径不存在，无法下载')
        return
      }

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
          this.$message.error(err.response?.data?.message || '文件下载失败，请稍后重试')
          console.error('下载文件失败:', err)
        })
    },
    
    // 处理文件预览
    handlePreviewFile() {
      if (!this.currentWork || !this.currentWork.filePath) {
        this.$message.error('文件路径不存在，无法预览')
        return
      }
      // 直接打开预览弹窗：具体渲染逻辑由 FilePreviewDialog 统一处理
      this.previewDialogVisible = true
    },
    
    // 处理分页大小变化
    handleSizeChange(size) {
      this.pageSize = size
      this.loadWorks()
    },
    
    // 处理页码变化
    handleCurrentChange(current) {
      this.currentPage = current
      this.loadWorks()
    },
    
    // 获取缩略图URL
    getThumbnailUrl(work) {
      if (!work.filePath) return null
      
      const fileName = work.filePath
      const extension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'))
      const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp']
      
      if (imageExtensions.includes(extension)) {
        return `/api/File/download?fileName=${encodeURIComponent(fileName)}`
      }
      
      return null
    },
    
    // 获取文件扩展名
    getFileExtension(work) {
      if (!work.filePath) return '文件'
      
      const fileName = work.filePath
      const extension = fileName.toLowerCase().substring(fileName.lastIndexOf('.') + 1)
      return extension.toUpperCase()
    },
    
    // 获取当前用户ID
    getCurrentUserId() {
      return getUser()?.id || null
    },
    
    // 切换收藏状态
    async handleToggleFavorite(work) {
      try {
        const isFav = await this.checkFavoriteStatus(work.id)
        if (!getToken()) {
          this.$message.error('请先登录')
          return
        }

        if (isFav) {
          // 取消收藏
          const response = await unfavoriteWork(work.id)
          this.$message.success('已取消收藏')
          work.favorites = response.data.favorites
          work.isFavorite = false
        } else {
          // 添加收藏
          const response = await favoriteWork(work.id)
          this.$message.success('收藏成功')
          work.favorites = response.data.favorites
          work.isFavorite = true
        }
      } catch (error) {
        console.error('操作收藏失败', error)
        if (error.response && error.response.status === 401) {
          // 401：清空本地鉴权信息，提示重新登录
          clearAuth()
          this.$message.error('登录已过期，请重新登录')
        } else if (error.response && error.response.data && error.response.data.message) {
          this.$message.error(error.response.data.message)
        } else {
          this.$message.error('操作失败，请稍后重试')
        }
      }
    },
    
    // 判断作品是否已收藏
    async isFavorite(workId) {
      return await this.checkFavoriteStatus(workId)
    },
    
    // 生成随机颜色
    getRandomColor(id) {
      const colors = [
        '#e6f7ff', '#f6ffed', '#fff7e6', '#fff0f6', '#f0f5ff',
        '#e8f5e8', '#fff3e0', '#f3e5f5', '#e3f2fd', '#fffde7'
      ]
      return colors[id % colors.length]
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

/* 2. 页面头部 */
.page-header {
  text-align: center;
  padding: 40px 0 20px;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e7ed 100%);
  margin-bottom: 0;
}

.page-header h2 {
  font-size: 32px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 8px;
}

.page-header p {
  font-size: 16px;
  color: #666;
  margin: 0;
}

.fade-in-up {
  animation: fadeInUp 0.6s ease-out both;
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
.category-filters, .sort-options { display: flex; gap: 12px; flex-wrap: wrap; justify-content: center; }
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
.works-grid { 
  max-width: 1200px; 
  margin: 0 auto; 
  padding: 0 24px 60px; 
  background: transparent;
  border: none;
  box-shadow: none;
}

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
  align-items: flex-start; /* 标签移到顶部 */
  justify-content: flex-end;
  padding: 12px;
  overflow: hidden;
}

.thumbnail-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s ease;
}

.work-card:hover .thumbnail-image {
  transform: scale(1.05);
}

.thumbnail-placeholder {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: rgba(0, 0, 0, 0.6);
  transition: transform 0.5s ease;
}

.work-card:hover .thumbnail-placeholder {
  transform: scale(1.05);
}

.thumbnail-placeholder i {
  font-size: 48px;
  margin-bottom: 10px;
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
.work-title { 
  font-size: 16px; 
  font-weight: 600; 
  margin: 0 0 8px 0; 
  color: #333; 
  white-space: nowrap; 
  overflow: hidden; 
  text-overflow: ellipsis; 
  transition: color 0.3s; 
}
.work-card:hover .work-title { color: #0052D9; } /* 标题悬停变色 */
.work-author, .work-upload-user, .work-date {
  font-size: 13px; 
  color: #888; 
  margin: 0 0 8px 0; 
}

.work-actions {
  display: flex;
  justify-content: space-between;
  margin-top: 12px;
  gap: 8px;
}

.work-actions .el-button {
  flex: 1;
  font-size: 12px;
  padding: 6px 8px;
}

.pagination {
  margin-top: 32px;
  display: flex;
  justify-content: center;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
}

.loading-container p {
  margin-top: 16px;
  color: #606266;
  font-size: 14px;
}

/* 7. 作品详情对话框样式 */
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
.download-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.close-btn:hover {
  color: #0052D9;
  border-color: #0052D9;
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

/* 8. 文件预览样式 */
.file-preview {
  width: 100%;
  height: 70vh;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-preview {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-image {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}

.video-preview {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-video {
  max-width: 100%;
  max-height: 100%;
}

.other-preview {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.model-preview-container {
  width: 100%;
  height: 100%;
}

/* 9. 关键帧定义 */
@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@keyframes fadeInUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

/* 10. 响应式设计 */
@media (max-width: 768px) {
  .gallery-header {
    padding: 0 20px;
  }
  
  .system-title {
    font-size: 16px;
  }
  
  .page-header {
    padding: 30px 0 15px;
  }
  
  .page-header h2 {
    font-size: 24px;
  }
  
  .filter-section {
    margin: 30px auto 24px;
    padding: 0 16px;
  }
  
  .works-grid {
    padding: 0 16px 40px;
  }
  
  .work-actions {
    flex-direction: column;
  }
  
  .work-actions .el-button {
    width: 100%;
    margin-bottom: 8px;
  }
  
  .work-actions .el-button:last-child {
    margin-bottom: 0;
  }
  
  .work-meta {
    flex-direction: column;
    gap: 8px;
  }
  
  .work-meta span {
    margin-right: 0;
    margin-bottom: 0;
  }
}
</style>
