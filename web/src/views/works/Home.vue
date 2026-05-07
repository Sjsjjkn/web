<template>
  <div class="gallery-container">
    <!-- 顶部导航 -->
    <header class="gallery-header slide-down">
      <div class="logo-area">
        <div class="logo-icon">BDU</div>
        <h1 class="system-title">保定学院数字作品展厅</h1>
      </div>
      <div class="user-actions">
        <el-button type="primary" round class="action-btn" @click="navigateToUpload">
          <i class="el-icon-upload"></i> 上传作品
        </el-button>
        <el-button round class="action-btn secondary" @click="navigateToWorks">
          <i class="el-icon-view"></i> 浏览作品
        </el-button>
      </div>
    </header>

    <!-- 页面头部 -->
    <section class="page-header fade-in-up">
      <h2>欢迎来到保定学院数字作品展厅</h2>
      <p>探索和欣赏保定学院师生的优秀数字作品</p>
    </section>
    
    <!-- 数据概览 -->
    <section class="stats-section fade-in-up">
      <el-row :gutter="24">
        <el-col :xs="12" :sm="12" :md="6" v-for="(stat, index) in stats" :key="index">
          <el-card class="stat-card stagger-fade-in" :style="{ animationDelay: `${index * 0.1 + 0.2}s` }">
            <div class="stat-icon" :class="`icon-${index + 1}`">
              <i :class="stat.icon"></i>
            </div>
            <div class="stat-value">{{ stat.value }}</div>
            <div class="stat-label">{{ stat.label }}</div>
            <div class="stat-trend" :class="stat.trend > 0 ? 'positive' : 'negative'">
              <i :class="stat.trend > 0 ? 'el-icon-top' : 'el-icon-bottom'"></i>
              {{ Math.abs(stat.trend) }}%
            </div>
          </el-card>
        </el-col>
      </el-row>
    </section>
    
    <!-- 公告列表 -->
    <section class="announcement-section fade-in-up">
      <AnnouncementList @click="handleAnnouncementClick" />
    </section>
    
    <!-- 最近上传作品 -->
    <section class="works-section fade-in-up">
      <div class="section-header">
        <h2 class="section-title">最近上传作品</h2>
        <el-button type="text" class="view-all-btn" @click="navigateToWorks">查看全部 <i class="el-icon-arrow-right"></i></el-button>
      </div>
      
      <el-row :gutter="24">
        <el-col 
          :xs="24" :sm="12" :md="8" :lg="6" 
          v-for="(work, index) in recentWorks" 
          :key="work.id"
          class="stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.3}s` }"
        >
          <div @click="handleViewWork(work)" style="cursor: pointer;">
            <el-card class="work-card" shadow="never" :body-style="{ padding: '0px' }">
            <div class="cover-image" :style="{ backgroundColor: getRandomColor(work.id) }">
              <img v-if="getThumbnailUrl(work)" :src="getThumbnailUrl(work)" :alt="work.title" class="thumbnail-image">
              <ModelThumbnail 
                v-else-if="isModelFile(work)" 
                :model-url="`/api/File/download?fileName=${encodeURIComponent(work.filePath)}`"
                :background-color="getRandomColor(work.id)"
                class="model-preview"
              />
              <div v-else class="thumbnail-placeholder" :style="{ backgroundColor: getRandomColor(work.id) }">
                <i class="el-icon-document"></i>
                <span>{{ getFileExtension(work) }}</span>
              </div>
              <span class="category-badge">{{ work.category }}</span>
              <div class="hover-overlay">
                <el-button type="primary" circle icon="el-icon-caret-right"></el-button>
              </div>
            </div>
            
            <div class="work-info">
              <h3 class="work-title">{{ work.title }}</h3>
              <div class="author-row">
                <span class="work-author">作者: {{ work.uploadUserName || '未知作者' }}</span>
                <el-button 
                  v-if="work.uploadUserProfilePublic" 
                  type="text" 
                  size="small" 
                  class="author-profile-btn"
                  @click.stop="goToAuthorProfile(work.userId || work.UserId)"
                >
                  <i class="el-icon-user"></i> 主页
                </el-button>
              </div>
              <p class="work-date">{{ work.fileUploadTime ? formatDate(work.fileUploadTime) : work.uploadDate }}</p>
            </div>
            </el-card>
          </div>
        </el-col>
      </el-row>
    </section>
    
    <!-- 优秀作品推荐 -->
    <section class="featured-section fade-in-up">
      <div class="section-header">
        <h2 class="section-title">优秀作品推荐</h2>
        <el-button type="text" class="view-all-btn" @click="navigateToWorks">查看全部 <i class="el-icon-arrow-right"></i></el-button>
      </div>
      
      <el-row :gutter="24">
        <el-col 
          :xs="24" :sm="12" :md="8" :lg="8"
          v-for="(work, index) in featuredWorks" 
          :key="work.id"
          class="stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.3}s` }"
        >
          <div @click="handleViewWork(work)" style="cursor: pointer;">
            <el-card class="featured-card" shadow="never" :body-style="{ padding: '0px' }">
            <div class="featured-badge">
              <i class="el-icon-star-on"></i> 推荐
            </div>
            <div class="cover-image" :style="{ backgroundColor: getRandomColor(work.id + 100) }">
              <img v-if="getThumbnailUrl(work)" :src="getThumbnailUrl(work)" :alt="work.title" class="thumbnail-image">
              <ModelThumbnail 
                v-else-if="isModelFile(work)" 
                :model-url="`/api/File/download?fileName=${encodeURIComponent(work.filePath)}`"
                :background-color="getRandomColor(work.id + 100)"
                class="model-preview"
              />
              <div v-else class="thumbnail-placeholder" :style="{ backgroundColor: getRandomColor(work.id + 100) }">
                <i class="el-icon-document"></i>
                <span>{{ getFileExtension(work) }}</span>
              </div>
              <span class="category-badge">{{ work.category }}</span>
              <div class="hover-overlay">
                <el-button type="primary" circle icon="el-icon-caret-right"></el-button>
              </div>
            </div>
            
            <div class="work-info">
              <h3 class="work-title">{{ work.title }}</h3>
              <div class="author-row">
                <span class="work-author">作者: {{ work.uploadUserName || '未知作者' }}</span>
                <el-button 
                  v-if="work.uploadUserProfilePublic" 
                  type="text" 
                  size="small" 
                  class="author-profile-btn"
                  @click.stop="goToAuthorProfile(work.userId || work.UserId)"
                >
                  <i class="el-icon-user"></i> 主页
                </el-button>
              </div>
              <p class="work-description">{{ work.description }}</p>
              
              <div class="work-stats">
                <span><i class="el-icon-view"></i> {{ work.views || 0 }}</span>
                <span><i class="el-icon-star-off"></i> {{ work.favorites || 0 }}</span>
                <span><i class="el-icon-chat-line-round"></i> {{ work.comments || 0 }}</span>
              </div>
            </div>
            </el-card>
          </div>
        </el-col>
      </el-row>
    </section>
    
    <!-- 3D模型预览测试入口 -->
    <section class="test-section fade-in-up">
      <el-card class="test-card" shadow="never">
        <div class="test-content">
          <div class="test-icon">
            <svg width="64" height="64" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M32 6L6 14V50L32 58L58 50V14L32 6Z" fill="url(#test_linear)"/>
              <path d="M32 14L14 20V44L32 50L50 44V20L32 14Z" fill="url(#test_linear2)"/>
              <defs>
                <linearGradient id="test_linear" x1="6" y1="14" x2="58" y2="50" gradientUnits="userSpaceOnUse">
                  <stop stop-color="#0052D9"/>
                  <stop offset="1" stop-color="#34C759"/>
                </linearGradient>
                <linearGradient id="test_linear2" x1="14" y1="20" x2="50" y2="44" gradientUnits="userSpaceOnUse">
                  <stop stop-color="#64B5F6"/>
                  <stop offset="1" stop-color="#81C784"/>
                </linearGradient>
              </defs>
            </svg>
          </div>
          <div class="test-text">
            <h3>3D模型预览测试</h3>
            <p>体验Three.js的3D渲染效果</p>
          </div>
          <el-button type="primary" size="large" @click="navigateToModelTest" class="test-btn">
            进入测试 <i class="el-icon-arrow-right"></i>
          </el-button>
        </div>
      </el-card>
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
            <img v-if="getThumbnailUrl(currentWork)" :src="getThumbnailUrl(currentWork)" :alt="currentWork.title" class="detail-cover-image">
            <ModelThumbnail 
                v-else-if="isModelFile(currentWork)" 
                :model-url="`/api/File/download?fileName=${encodeURIComponent(currentWork.filePath)}`"
                :background-color="getRandomColor(currentWork.id)"
                class="detail-model-preview"
              />
            <div v-else class="detail-cover-placeholder" :style="{ backgroundColor: getRandomColor(currentWork.id) }">
              <i class="el-icon-document"></i>
              <span>{{ getFileExtension(currentWork) }}</span>
            </div>
            <span class="category-badge">{{ currentWork.category }}</span>
          </div>
          <div class="work-info-header">
            <h2 class="work-title-detail">{{ currentWork.title }}</h2>
            <div class="work-author-info">
              <span class="author-name">{{ currentWork.uploadUserName || '未知作者' }}</span>
              <el-button 
                v-if="currentWork.uploadUserProfilePublic" 
                type="text" 
                size="small" 
                class="author-profile-btn"
                @click="goToAuthorProfile(currentWork.userId || currentWork.UserId)"
              >
                <i class="el-icon-user"></i> 查看主页
              </el-button>
              <span class="upload-time">{{ currentWork.fileUploadTime ? formatDate(currentWork.fileUploadTime) : currentWork.uploadDate }}</span>
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
import { getUser } from '../../utils/auth'
import ModelThumbnail from '../../components/ModelThumbnail.vue'
import AnnouncementList from '../../components/AnnouncementList.vue'

export default {
  components: {
    ModelThumbnail,
    AnnouncementList
  },
  name: 'Home',
  data() {
    return {
      // 数据概览
      stats: [],
      // 最近上传作品数据
      recentWorks: [],
      // 优秀作品推荐数据
      featuredWorks: [],
      // 加载状态
      loading: {
        overview: false,
        works: false
      },
      // 错误信息
      error: null,
      // 详情对话框
      detailDialogVisible: false,
      // 当前查看的作品
      currentWork: null,
      // 自动刷新定时器
      refreshTimer: null
    }
  },
  mounted() {
    // 组件加载时获取数据
    this.fetchOverviewData();
    this.fetchWorksData();

    // 每30秒自动刷新数据
    this.refreshTimer = setInterval(() => {
      this.fetchWorksData();
    }, 30000);
  },
  beforeDestroy() {
    // 组件销毁时清除定时器
    if (this.refreshTimer) {
      clearInterval(this.refreshTimer);
    }
  },
  methods: {
    // 导航到模型预览测试页面
    navigateToModelTest() {
      this.$router.push('/lab/model-test')
    },
    // 获取文件类型
    getFileType(filePath) {
      if (!filePath) return 'other'
      const extension = filePath.toLowerCase().substring(filePath.lastIndexOf('.'))
      const imageExts = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp']
      const videoExts = ['.mp4', '.avi', '.mov', '.wmv', '.flv', '.mkv']
      const modelExts = ['.json', '.obj', '.gltf', '.glb', '.fbx', '.dae']
      
      if (imageExts.includes(extension)) return 'image'
      if (videoExts.includes(extension)) return 'video'
      if (modelExts.includes(extension)) return 'model'
      return 'other'
    },
    // 获取文件URL
    getFileUrl(filePath) {
      if (!filePath) return ''
      return `http://localhost:5200/api/File/download?filePath=${encodeURIComponent(filePath)}`
    },
    // 获取缩略图URL
    getThumbnailUrl(work) {
      // 优先使用用户上传的预览图
      if (work.previewImage) {
        return `/api/File/download?fileName=${encodeURIComponent(work.previewImage)}`
      }
      
      // 如果没有预览图，尝试使用作品文件本身（仅图片格式）
      if (!work.filePath) return null
      
      const fileName = work.filePath
      const extension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'))
      const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp']
      
      if (imageExtensions.includes(extension)) {
        return `/api/File/download?fileName=${encodeURIComponent(fileName)}`
      }
      
      return null
    },
    // 判断是否是模型文件
    isModelFile(work) {
      if (!work.filePath) return false
      const extension = work.filePath.toLowerCase().substring(work.filePath.lastIndexOf('.'))
      const modelExts = ['.json', '.obj', '.gltf', '.glb', '.fbx', '.dae', '.3ds']
      return modelExts.includes(extension)
    },
    // 获取文件扩展名
    getFileExtension(work) {
      if (!work.filePath) return '文件'
      
      const fileName = work.filePath
      const extension = fileName.toLowerCase().substring(fileName.lastIndexOf('.') + 1)
      return extension.toUpperCase()
    },
    // 获取概览数据
    async fetchOverviewData() {
      this.loading.overview = true;
      this.error = null;
      try {
        const response = await this.$axios.get('/api/Data/overview');
        const data = response.data;
        // 转换数据格式以匹配前端需要的结构
        this.stats = [
          {
            value: data.totalWorks.toString(),
            label: '总作品数',
            icon: 'el-icon-document',
            trend: 12 // 暂时使用固定值，实际应该从API获取
          },
          {
            value: data.totalUsers.toString(),
            label: '活跃用户',
            icon: 'el-icon-user',
            trend: 8 // 暂时使用固定值，实际应该从API获取
          },
          {
            value: data.totalViews.toString(),
            label: '收藏数',
            icon: 'el-icon-star-on',
            trend: 15 // 暂时使用固定值，实际应该从API获取
          },
          {
            value: data.todayWorks.toString(),
            label: '今日上传',
            icon: 'el-icon-chat-line-round',
            trend: 20 // 暂时使用固定值，实际应该从API获取
          }
        ];
      } catch (error) {
        console.error('获取概览数据失败:', error);
        this.error = '获取概览数据失败，请稍后重试';
        // 清空数据，不使用默认数据
        this.stats = [];
      } finally {
        this.loading.overview = false;
      }
    },
    // 获取作品数据
    async fetchWorksData() {
      this.loading.works = true;
      this.error = null;
      try {
        // 获取所有已发布作品（不需要认证）
        console.log('开始获取作品列表');
        const response = await this.$axios.get('/api/Work/public?limit=20');
        console.log('获取作品列表成功:', response.data);
        const allWorks = response.data.items || [];
        console.log('作品总数:', allWorks.length);
        console.log('原始作品数据示例:', JSON.stringify(allWorks[0], null, 2));
        console.log('所有字段名:', allWorks[0] ? Object.keys(allWorks[0]) : []);
        // 检查是否包含用户ID字段
        if (allWorks[0]) {
          console.log('userId字段值:', allWorks[0].userId);
          console.log('UserId字段值:', allWorks[0].UserId);
          console.log('userid字段值:', allWorks[0].userid);
        }

        // 最近上传作品 - 按上传时间倒序取前4个
        const sortedWorks = [...allWorks].sort((a, b) => {
          const dateA = new Date(a.fileUploadTime || a.uploadDate || 0);
          const dateB = new Date(b.fileUploadTime || b.uploadDate || 0);
          return dateB - dateA;
        });

        this.recentWorks = sortedWorks
          .slice(0, 4)
          .map(work => ({
            id: work.id,
            title: work.title || '未命名作品',
            uploadUserName: work.uploadUserName || '未知作者',
            category: work.category || '未分类',
            uploadDate: work.uploadDate,
            fileUploadTime: work.fileUploadTime,
            filePath: work.filePath,
            previewImage: work.previewImage,
            description: work.description,
            fileName: work.fileName,
            views: work.views || 0,
            favorites: work.favorites || 0,
            isExcellent: !!work.isExcellent,
            userId: work.userId || work.UserId || work.userid,
            uploadUserProfilePublic: work.uploadUserProfilePublic
          }));
        
        console.log('最近上传作品数据:', this.recentWorks);

        this.featuredWorks = sortedWorks
          .filter(work => !!work.isExcellent)
          .sort((a, b) => {
            const scoreA = (a.favorites || 0) * 2 + (a.views || 0)
            const scoreB = (b.favorites || 0) * 2 + (b.views || 0)
            return scoreB - scoreA
          })
          .slice(0, 3)
          .map(work => ({
            id: work.id,
            title: work.title || '未命名作品',
            uploadUserName: work.uploadUserName || '未知作者',
            category: work.category || '未分类',
            uploadDate: work.uploadDate,
            fileUploadTime: work.fileUploadTime,
            filePath: work.filePath,
            previewImage: work.previewImage,
            description: work.description,
            fileName: work.fileName,
            views: work.views || 0,
            favorites: work.favorites || 0,
            isExcellent: true,
            userId: work.userId || work.UserId || work.userid,
            uploadUserProfilePublic: work.uploadUserProfilePublic
          }));
      } catch (error) {
        console.error('获取作品数据失败:', error);
        this.error = '获取作品数据失败，请稍后重试';
        this.recentWorks = [];
        this.featuredWorks = [];
      } finally {
        this.loading.works = false;
      }
    },
    // 导航到上传作品页面
    navigateToUpload() {
      const user = getUser()
      const target = user && ['Admin', 'Teacher'].includes(user.role)
        ? '/works/manage'
        : '/account/works'

      if (this.$route.path !== target) {
        this.$router.push(target)
      }
    },
    // 导航到作品展示页面
    navigateToWorks() {
      this.$router.push('/works')
    },
    // 查看作品详情
    async handleViewWork(work) {
      console.log('点击作品卡片:', work);
      this.currentWork = work
      this.detailDialogVisible = true
      console.log('对话框状态:', this.detailDialogVisible);
      
      // 增加浏览量
      try {
        await this.$axios.get(`/api/Work/${work.id}/view`);
        // 更新本地数据
        work.views = (work.views || 0) + 1;
      } catch (error) {
        console.error('增加浏览量失败:', error);
      }
    },
    // 跳转到作者主页
    goToAuthorProfile(userId) {
      this.$router.push(`/profile/${userId}`)
    },
    // 处理公告点击
    handleAnnouncementClick(announcement) {
      console.log('点击公告:', announcement);
      // 显示公告详情弹窗
      this.$alert(announcement.content, announcement.title, {
        confirmButtonText: '知道了',
        dangerouslyUseHTMLString: true
      });
    },
    // 收藏作品
    async handleFavoriteWork(work) {
      try {
        // 收藏接口需要登录；token 由统一 http 实例自动注入
        if (!localStorage.getItem('token')) {
          this.$message.error('请先登录');
          return;
        }
        const response = await this.$axios.post(`/api/Work/${work.id}/favorite`, {});
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
        if (!localStorage.getItem('token')) {
          this.$message.error('请先登录');
          return;
        }
        const response = await this.$axios.delete(`/api/Work/${work.id}/favorite`);
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
        if (!localStorage.getItem('token')) {
          return false;
        }
        const response = await this.$axios.get(`/api/Work/${workId}/is-favorite`);
        return response.data.isFavorite;
      } catch (error) {
        console.error('检查收藏状态失败:', error);
        return false;
      }
    },
    // 下载文件
    handleDownloadFile(work) {
      if (!work || !work.filePath) {
        this.$message.error('文件路径不存在，无法下载')
        return
      }
      try {
        const downloadUrl = `/api/File/download?fileName=${encodeURIComponent(work.filePath)}`
        const link = document.createElement('a')
        link.href = downloadUrl
        link.download = work.fileName || work.filePath
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        this.$message.success('文件下载已开始')
      } catch (error) {
        console.error('下载文件失败:', error)
        this.$message.error('文件下载失败，请稍后重试')
      }
    },
    // 格式化日期
    formatDate(date) {
      if (!date) return ''
      const d = new Date(date)
      return d.toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      })
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
  background-color: #f8fafc;
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
  backdrop-filter: blur(10px);
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

.action-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 82, 217, 0.3);
}
.action-btn.secondary {
  background-color: white;
  border: 1px solid #e4e7ed;
  color: #666;
}
.action-btn.secondary:hover {
  border-color: #0052D9;
  color: #0052D9;
}

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

/* 3. 数据概览 */
.stats-section {
  max-width: 1200px;
  margin: 40px auto;
  padding: 0 24px;
}

.stat-card {
  margin-bottom: 24px;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(0,0,0,0.05);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  background: white;
  padding: 24px;
  text-align: center;
}

.stat-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 16px 32px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  font-size: 24px;
  color: white;
}

.icon-1 { background: linear-gradient(135deg, #0052D9, #64B5F6); }
.icon-2 { background: linear-gradient(135deg, #34C759, #81C784); }
.icon-3 { background: linear-gradient(135deg, #FF9500, #FFB84D); }
.icon-4 { background: linear-gradient(135deg, #FF2D55, #FF6B8A); }

.stat-value {
  font-size: 32px;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #666;
  margin-bottom: 8px;
}

.stat-trend {
  font-size: 12px;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 2px 8px;
  border-radius: 12px;
}
.stat-trend.positive { background: rgba(52, 199, 89, 0.1); color: #34C759; }
.stat-trend.negative { background: rgba(255, 45, 85, 0.1); color: #FF2D55; }

/* 4. 区块样式 */
.works-section,
.featured-section,
.test-section {
  max-width: 1200px;
  margin: 0 auto 40px;
  padding: 0 24px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.section-title {
  font-size: 24px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
  position: relative;
  display: inline-block;
}

.section-title::after {
  content: '';
  position: absolute;
  bottom: -8px;
  left: 0;
  width: 60px;
  height: 4px;
  background: linear-gradient(90deg, #0052D9, #34C759);
  border-radius: 2px;
}

.view-all-btn {
  color: #0052D9;
  font-size: 14px;
  transition: all 0.3s;
}
.view-all-btn:hover {
  color: #003d8f;
  transform: translateX(4px);
}

/* 5. 作品卡片 */
.stagger-fade-in {
  opacity: 0;
  animation: fadeInUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.work-card,
.featured-card {
  margin-bottom: 24px;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(0,0,0,0.05);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  background: white;
  cursor: pointer;
}

.work-card:hover,
.featured-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 16px 32px rgba(0, 0, 0, 0.1);
}

.featured-badge {
  position: absolute;
  top: 12px;
  left: 12px;
  background: linear-gradient(135deg, #FF9500, #FF2D55);
  color: white;
  font-size: 12px;
  padding: 4px 12px;
  border-radius: 12px;
  z-index: 2;
  display: flex;
  align-items: center;
  gap: 4px;
}

.cover-image {
  height: 180px;
  width: 100%;
  position: relative;
  display: flex;
  align-items: flex-start;
  justify-content: flex-start;
  padding: 12px;
  overflow: hidden;
}

.thumbnail-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: contain;
  background-color: #f5f5f5;
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

.model-preview {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  transition: transform 0.5s ease;
}

.work-card:hover .model-preview {
  transform: scale(1.05);
}

.featured-card:hover .model-preview {
  transform: scale(1.05);
}

.detail-model-preview {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
}

.featured-card:hover .thumbnail-image {
  transform: scale(1.05);
}

.featured-card:hover .thumbnail-placeholder {
  transform: scale(1.05);
}

.category-badge {
  position: absolute;
  top: 12px;
  left: 12px;
  background: rgba(0, 0, 0, 0.5);
  color: white;
  font-size: 12px;
  padding: 4px 10px;
  border-radius: 12px;
  backdrop-filter: blur(4px);
  z-index: 2;
}

.featured-card .category-badge {
  left: auto;
  right: 60px;
}

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

.work-card:hover .hover-overlay,
.featured-card:hover .hover-overlay { opacity: 1; }
.work-card:hover .hover-overlay .el-button,
.featured-card:hover .hover-overlay .el-button { transform: scale(1); }

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
.work-card:hover .work-title,
.featured-card:hover .work-title { color: #0052D9; }
.work-author, .work-date {
  font-size: 13px; 
  color: #888; 
  margin: 0 0 8px 0; 
}

.author-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.author-profile-btn {
  padding: 0;
  font-size: 12px;
  color: #165dff;
}
.work-description {
  font-size: 13px;
  color: #666;
  margin: 0 0 12px 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.work-stats {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: #999;
  border-top: 1px solid #f0f0f0;
  padding-top: 12px;
  margin-top: 12px;
}
.work-stats span { display: flex; align-items: center; gap: 4px; }

/* 6. 测试卡片 */
.test-card {
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid rgba(0,0,0,0.05);
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e7ed 100%);
}

.test-content {
  display: flex;
  align-items: center;
  padding: 32px;
  gap: 24px;
}

.test-icon {
  flex-shrink: 0;
}

.test-text {
  flex: 1;
}
.test-text h3 {
  font-size: 20px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 4px;
}
.test-text p {
  font-size: 14px;
  color: #666;
  margin: 0;
}

.test-btn {
  flex-shrink: 0;
}

/* 7. 作品详情 */
.work-detail {
  padding: 20px 0;
}

.work-detail h3 {
  margin-bottom: 24px;
  color: #303133;
  font-size: 20px;
  font-weight: 600;
}

.work-meta {
  display: flex;
  flex-wrap: wrap;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e4e7ed;
  gap: 16px;
}

.work-meta span {
  color: #606266;
  font-size: 14px;
}

.work-description h4 {
  margin-bottom: 12px;
  color: #303133;
  font-size: 16px;
  font-weight: 500;
}

.work-description p {
  line-height: 1.6;
  color: #606266;
  font-size: 14px;
}

/* 8. 关键帧定义 */
@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@keyframes fadeInUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

/* 9. 响应式设计 */
@media (max-width: 768px) {
  .gallery-header {
    padding: 0 20px;
  }
  
  .system-title {
    font-size: 16px;
  }
  
  .action-btn span {
    display: none;
  }
  
  .page-header {
    padding: 30px 0 15px;
  }
  
  .page-header h2 {
    font-size: 24px;
  }
  
  .stats-section,
  .works-section,
  .featured-section,
  .test-section {
    padding: 0 16px;
  }
  
  .test-content {
    flex-direction: column;
    text-align: center;
    padding: 24px;
  }
  
  .work-meta {
    flex-direction: column;
    gap: 8px;
  }
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
  position: relative;
  overflow: hidden;
}

.detail-cover-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.detail-cover-placeholder {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: white;
}

.detail-cover-placeholder i {
  font-size: 48px;
  margin-bottom: 10px;
}

.detail-cover-placeholder span {
  font-size: 14px;
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
</style>
