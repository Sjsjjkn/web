<template>
  <div class="content-moderation-container">
    <header class="moderation-header slide-down">
      <div class="header-content">
        <div class="logo-area">
          <div class="logo-icon">BDU</div>
          <h1 class="system-title">内容审核与安全</h1>
        </div>
        <div class="header-actions">
          <el-button 
            type="primary" 
            round 
            class="refresh-btn"
            icon="el-icon-refresh"
            @click="loadModerationItems"
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
          v-model="filterForm.search"
          prefix-icon="el-icon-search"
          clearable
          class="animated-input"
          @keyup.enter="loadModerationItems"
        >
        </el-input>
      </div>
      
      <div class="filter-tags">
        <span 
          v-for="(status, index) in statusOptions" 
          :key="status.value"
          :class="['filter-tag', { active: filterForm.status === status.value }]"
          @click="filterForm.status = status.value; loadModerationItems()"
          :style="{ animationDelay: `${index * 0.05 + 0.2}s` }"
          class="stagger-fade-in"
        >
          {{ status.label }}
        </span>
      </div>
    </section>

    <section class="stats-section fade-in-up">
      <div class="stats-grid">
        <div 
          v-for="(stat, index) in stats" 
          :key="stat.label"
          class="stat-card stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.3}s` }"
        >
          <div class="stat-icon" :style="{ backgroundColor: stat.color }"><i :class="stat.icon"></i></div>
          <div class="stat-info">
            <div class="stat-number">{{ stat.value }}</div>
            <div class="stat-label">{{ stat.label }}</div>
          </div>
        </div>
      </div>
    </section>

    <section class="moderation-section">
      <el-empty v-if="moderationItems.length === 0" description="暂无需要审核的内容"></el-empty>
      
      <div class="moderation-grid" v-else>
        <div 
          v-for="(item, index) in moderationItems" 
          :key="item.id"
          class="moderation-card stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.4}s` }"
        >
          <el-card shadow="never" :body-style="{ padding: '0px' }">
            <div class="card-header" :class="getStatusClass(item.status)">
              <div class="work-info">
                <h3 class="work-title">{{ item.title }}</h3>
                <p class="work-author">作者：{{ item.author }}</p>
                <p class="work-category">{{ item.category }}</p>
              </div>
              <div class="status-badge">{{ getStatusText(item.status) }}</div>
              <div v-if="item.isExcellent" class="excellent-badge">
                <i class="el-icon-star-on"></i>
                <span>优秀作品</span>
              </div>
            </div>
            
            <div class="card-body">
              <div class="content-preview">
                <div v-if="item.type === 'image'" class="image-preview">
                  <img :src="getPreviewUrl(item)" :alt="item.title" class="preview-image">
                </div>
                <div v-else-if="item.type === 'model'" class="model-preview">
                  <ModelCardCover :work="item" />
                </div>
                <div v-else-if="item.type === 'video'" class="video-preview">
                  <div class="video-placeholder">
                    <i class="el-icon-video-camera"></i>
                    <span>视频内容</span>
                  </div>
                </div>
                <div v-else-if="item.type === 'document'" class="document-preview">
                  <div class="document-placeholder">
                    <i class="el-icon-document"></i>
                    <span>{{ item.fileName }}</span>
                  </div>
                </div>
                <div v-else class="text-preview">
                  <p>{{ item.content ? item.content.substring(0, 100) : item.description ? item.description.substring(0, 100) : '' }}...</p>
                </div>
              </div>
              
              <div class="moderation-actions">
                <el-button 
                  v-if="item.status === '待审核'"
                  type="primary" 
                  round 
                  size="small"
                  @click="approveItem(item)"
                  :loading="item.approving"
                >
                  通过
                </el-button>
                <el-button 
                  v-if="item.status === '待审核'"
                  type="danger" 
                  round 
                  size="small"
                  @click="openRejectDialog(item)"
                  :loading="item.rejecting"
                >
                  拒绝
                </el-button>
                <el-button 
                  type="warning" 
                  round 
                  size="small"
                  @click="toggleExcellent(item)"
                  :loading="item.excelling"
                >
                  {{ item.isExcellent ? '取消优秀' : '设为优秀' }}
                </el-button>
                <el-button 
                  type="info" 
                  round 
                  size="small"
                  @click="viewDetails(item)"
                >
                  详情
                </el-button>
              </div>
            </div>
          </el-card>
        </div>
      </div>
    </section>

    <!-- ========== 审核详情弹窗（BODA Design System）========== -->
    <el-dialog
      :visible.sync="dialogVisible"
      width="720px"
      append-to-body
      @close="currentItem = null"
      custom-class="review-detail-dialog"
    >
      <div v-if="currentItem" class="review-detail">
        <!-- 顶部：作品信息卡片 -->
        <div class="detail-hero-card">
          <div class="detail-cover">
            <img
              v-if="currentItem.type === 'image' && getPreviewUrl(currentItem)"
              :src="getPreviewUrl(currentItem)"
              :alt="currentItem.title"
              class="detail-cover-img"
            />
            <div v-else class="detail-cover-placeholder">
              <span class="cover-emoji">{{ getTypeEmoji(currentItem.type) }}</span>
              <span class="cover-label">{{ getTypeText(currentItem.type) }}</span>
            </div>
          </div>
          <div class="detail-meta">
            <h2 class="detail-title">{{ currentItem.title }}</h2>
            <div class="detail-author-row">
              <span class="author-avatar-sm">{{ (currentItem.author || '?')[0] }}</span>
              <span class="author-name">{{ currentItem.author || '未知作者' }}</span>
            </div>
            <div class="detail-tags-row">
              <span class="info-tag">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="tag-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" /></svg>
                {{ currentItem.category || '未分类' }}
              </span>
              <span class="info-tag">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="tag-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                {{ currentItem.submitTime || '未知时间' }}
              </span>
            </div>
            <!-- 风险等级条 -->
            <div class="risk-bar" :class="'risk-bar--' + (currentItem.riskLevel || 'low')">
              <div class="risk-bar-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>
              </div>
              <div class="risk-bar-body">
                <span class="risk-bar-label">风险等级</span>
                <span class="risk-bar-value">{{ getRiskText(currentItem.riskLevel) }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 内容预览 -->
        <div class="detail-section">
          <h3 class="section-heading">内容预览</h3>
          <div class="preview-card">
            <div v-if="currentItem.type === 'image'" class="preview-image-wrap">
              <img :src="getPreviewUrl(currentItem)" :alt="currentItem.title" class="preview-img" />
            </div>
            <div v-else-if="currentItem.type === 'video'" class="preview-empty-state">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="empty-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" /></svg>
              <span>视频内容 — 需下载后查看</span>
            </div>
            <div v-else-if="currentItem.type === 'document'" class="preview-empty-state">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="empty-icon"><path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
              <span>{{ currentItem.fileName || '文档文件' }}</span>
            </div>
            <div v-else class="preview-text">
              <p>{{ currentItem.content || '暂无文本内容' }}</p>
            </div>
          </div>
        </div>

        <!-- 风险详情 -->
        <div class="detail-section" v-if="currentItem.riskDetails && currentItem.riskDetails.length > 0">
          <h3 class="section-heading">风险详情</h3>
          <div class="risk-items">
            <div class="risk-item" v-for="(risk, idx) in currentItem.riskDetails" :key="idx">
              <div class="risk-item-dot"></div>
              <span class="risk-item-text">{{ risk }}</span>
            </div>
          </div>
        </div>
      </div>

      <span slot="footer" class="dialog-footer">
        <el-button @click="downloadFile(currentItem)" class="btn-download" icon="el-icon-download">下载作品</el-button>
        <div class="footer-right">
          <el-button @click="dialogVisible = false" class="btn-cancel">关闭</el-button>
          <el-button type="danger" @click="openRejectDialog(currentItem)" class="btn-reject">拒绝</el-button>
          <el-button type="primary" @click="approveItem(currentItem)" class="btn-approve">通过</el-button>
        </div>
      </span>
    </el-dialog>

    <!-- 拒绝对话框 -->
    <el-dialog
      title="拒绝作品"
      :visible.sync="rejectDialogVisible"
      width="480px"
      :close-on-click-modal="false"
    >
      <div class="reject-dialog-content">
        <p class="reject-tip">请输入拒绝理由，该理由将发送给作品提交者：</p>
        <el-input
          v-model="rejectReason"
          type="textarea"
          placeholder="请输入拒绝理由（如：内容不符合要求、存在安全风险等）"
          :rows="4"
          class="reject-textarea"
        ></el-input>
      </div>
      <div slot="footer" class="dialog-footer">
        <el-button @click="rejectDialogVisible = false">取消</el-button>
        <el-button
          type="danger"
          @click="confirmReject"
          :disabled="!rejectReason.trim()"
        >
          确认拒绝
        </el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import http from '../../utils/http'
import ModelCardCover from '../../components/ModelCardCover.vue'

export default {
  name: 'ContentModeration',
  components: {
    ModelCardCover
  },
  data() {
    return {
      loading: false,
      dialogVisible: false,
      rejectDialogVisible: false,
      rejectReason: '',
      filterForm: {
        search: '',
        status: '待审核'
      },
      moderationItems: [],
      currentItem: null,
      statusOptions: [
        { label: '待审核', value: '待审核' },
        { label: '已审核', value: '已审核' },
        { label: '优秀作品', value: '优秀作品' },
        { label: '全部', value: '' }
      ],
      stats: [
        { label: '待审核', value: 0, icon: 'el-icon-time', color: '#fffbe6' },
        { label: '已审核', value: 0, icon: 'el-icon-check', color: '#f6ffed' },
        { label: '优秀作品', value: 0, icon: 'el-icon-star-on', color: '#fff7e6' }
      ]
    }
  },
  mounted() {
    this.loadModerationItems()
  },
  methods: {
    async loadModerationItems() {
      this.loading = true
      try {
        const response = await http.get('/api/ContentModeration/items', {
          params: {
            status: this.filterForm.status,
            search: this.filterForm.search
          }
        })

        this.moderationItems = response.data
        this.loadStats()
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载审核项目失败')
        console.error('加载审核项目失败:', error)
        this.moderationItems = []
      } finally {
        this.loading = false
      }
    },

    async loadStats() {
      try {
        const response = await http.get('/api/ContentModeration/stats')
        this.stats = response.data
      } catch (error) {
        console.error('加载统计数据失败:', error)
        this.updateStats()
      }
    },

    getStatusClass(status) {
      switch (status) {
        case '待审核': return 'status-pending'
        case '已发布': return 'status-approved'
        case '已拒绝': return 'status-rejected'
        default: return ''
      }
    },

    getStatusText(status) {
      switch (status) {
        case '待审核': return '待审核'
        case '已发布': return '已通过'
        case '已拒绝': return '已拒绝'
        default: return status
      }
    },

    getTypeText(type) {
      switch (type) {
        case 'image': return '图片'
        case 'video': return '视频'
        case 'document': return '文档'
        case 'text': return '文本'
        default: return '其他'
      }
    },

    getTypeEmoji(type) {
      switch (type) {
        case 'image': return '🖼'
        case 'video': return '🎬'
        case 'document': return '📄'
        case 'text': return '📝'
        default: return '📁'
      }
    },

    async approveItem(item) {
      item.approving = true
      try {
        await http.put(`/api/ContentModeration/approve/${item.id}`, {})

        item.status = '已发布'
        this.$message.success('审核通过')
        this.loadStats()
        if (this.dialogVisible) this.dialogVisible = false
      } catch (error) {
        this.$message.error(error.response?.data?.message || '操作失败')
        console.error('审核通过失败:', error)
      } finally {
        item.approving = false
      }
    },

    openRejectDialog(item) {
      this.currentItem = item
      this.rejectReason = ''
      this.rejectDialogVisible = true
      this.dialogVisible = false
    },

    async confirmReject() {
      if (!this.rejectReason.trim()) {
        this.$message.warning('请输入拒绝理由')
        return
      }

      this.currentItem.rejecting = true
      try {
        await http.put(`/api/ContentModeration/reject/${this.currentItem.id}`, {
          reason: this.rejectReason
        })

        this.currentItem.status = '已拒绝'
        this.$message.success('已拒绝并发送通知')
        this.loadStats()
        this.rejectDialogVisible = false
      } catch (error) {
        this.$message.error(error.response?.data?.message || '操作失败')
        console.error('拒绝失败:', error)
      } finally {
        this.currentItem.rejecting = false
      }
    },

    async toggleExcellent(item) {
      item.excelling = true
      try {
        await http.put(`/api/ContentModeration/excellent/${item.id}`, {})

        item.isExcellent = !item.isExcellent
        this.$message.success(item.isExcellent ? '已设置为优秀作品' : '已取消优秀作品')
        this.loadStats()
      } catch (error) {
        this.$message.error(error.response?.data?.message || '操作失败')
        console.error('设置优秀作品失败:', error)
      } finally {
        item.excelling = false
      }
    },

    viewDetails(item) {
      if (item.riskDetails && typeof item.riskDetails === 'string') {
        item.riskDetails = item.riskDetails.split('|').filter(r => r.trim())
      } else if (!item.riskDetails) {
        item.riskDetails = []
      }
      this.currentItem = item
      this.dialogVisible = true
    },

    getRiskClass(riskLevel) {
      switch (riskLevel) {
        case 'high': return 'risk-high'
        case 'medium': return 'risk-medium'
        case 'low': return 'risk-low'
        default: return ''
      }
    },

    getRiskText(riskLevel) {
      switch (riskLevel) {
        case 'high': return '高风险'
        case 'medium': return '中风险'
        case 'low': return '低风险'
        default: return riskLevel || '未知'
      }
    },

    updateStats() {
      const pending = this.moderationItems.filter(item => item.status === '待审核').length
      const approved = this.moderationItems.filter(item => item.status === '已发布').length
      const rejected = this.moderationItems.filter(item => item.status === '已拒绝').length

      this.stats[0].value = pending
      this.stats[1].value = approved
      this.stats[2].value = rejected
    },

    getPreviewUrl(item) {
      if (!item) return ''
      const filename = item.preview || item.fileName || ''
      if (!filename) return ''

      const ext = filename.toLowerCase().split('.').pop()
      const imageExts = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp']
      if (ext && imageExts.includes(ext)) {
        return `/api/File/download?fileName=${encodeURIComponent(filename)}`
      }

      if (item.type !== 'image') return ''

      return `/api/File/download?fileName=${encodeURIComponent(filename)}`
    },

    getDownloadUrl(item) {
      if (!item) return ''
      const filename = item.preview || item.fileName || ''
      if (!filename) return ''
      return `/api/File/download?fileName=${encodeURIComponent(filename)}`
    },

    downloadFile(item) {
      if (!item) return
      const url = this.getDownloadUrl(item)
      if (!url) {
        this.$message.warning('无法获取下载地址')
        return
      }
      const a = document.createElement('a')
      a.href = url
      a.download = item.fileName || ''
      a.target = '_blank'
      a.style.display = 'none'
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
    }
  }
}
</script>

<style scoped>
/* ========================================
   CONTENT MODERATION — BODA Design System
   ======================================== */

.content-moderation-container {
  min-height: 100vh;
  background: var(--bg-page, #F8F9F5);
  font-family: var(--font-main, 'Inter', 'Noto Sans SC', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif);
}

/* ── Header ── */
.moderation-header {
  background: var(--bg-card, #FFFFFF);
  border-bottom: 1px solid var(--border-color, #E8E2D8);
  position: sticky;
  top: 0;
  z-index: 100;
  padding: 16px 0;
  animation: slideDown 0.6s var(--ease-out-expo, cubic-bezier(0.16, 1, 0.3, 1)) both;
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo-area {
  display: flex;
  align-items: center;
  gap: 12px;
}

.logo-icon {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, var(--primary, #2D8A6E), var(--primary-light, #45A884));
  border-radius: var(--radius-sm, 10px);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: 700;
  font-size: 15px;
  letter-spacing: -0.5px;
}

.system-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0;
}

.refresh-btn {
  transition: all var(--duration-normal, .3s) var(--ease-out-expo, cubic-bezier(0.16, 1, 0.3, 1));
}

.refresh-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px var(--primary-glow, rgba(45, 138, 110, .2));
}

/* ── Filters ── */
.filter-section {
  max-width: 1200px;
  margin: 32px auto 24px;
  padding: 0 24px;
}

.search-bar {
  margin-bottom: 20px;
}

.animated-input :deep(.el-input__inner) {
  border-radius: var(--radius-full, 9999px);
  transition: all var(--duration-normal, .3s);
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
  height: 44px;
}

.animated-input :deep(.el-input__inner):focus {
  border-color: var(--primary, #2D8A6E);
  box-shadow: 0 0 0 3px var(--primary-glow, rgba(45, 138, 110, .2));
}

.filter-tags {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.filter-tag {
  padding: 8px 18px;
  border-radius: var(--radius-full, 9999px);
  background: var(--bg-card, #FFFFFF);
  border: 1px solid var(--border-color, #E8E2D8);
  cursor: pointer;
  transition: all var(--duration-fast, .18s) var(--ease-out-expo);
  font-size: 13px;
  font-weight: 500;
  color: var(--text-secondary, #555555);
}

.filter-tag:hover {
  border-color: var(--primary, #2D8A6E);
  color: var(--primary, #2D8A6E);
  transform: translateY(-2px);
}

.filter-tag.active {
  background: var(--primary, #2D8A6E);
  color: #fff;
  border-color: var(--primary, #2D8A6E);
}

/* ── Stats ── */
.stats-section {
  max-width: 1200px;
  margin: 0 auto 32px;
  padding: 0 24px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-card {
  background: var(--bg-card, #FFFFFF);
  border-radius: var(--radius-lg, 18px);
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 14px;
  border: 1px solid var(--border-light, #F2EDE6);
  transition: all var(--duration-normal, .3s) var(--ease-out-expo);
  box-shadow: var(--shadow-xs, 0 1px 2px rgba(0,0,0,.03));
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-md, 0 6px 20px rgba(0,0,0,.07));
}

.stat-icon {
  width: 52px;
  height: 52px;
  border-radius: var(--radius-md, 14px);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
  color: var(--primary, #2D8A6E);
  flex-shrink: 0;
}

.stat-number {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  line-height: 1.1;
}

.stat-label {
  font-size: 12px;
  color: var(--text-light, #888888);
  margin-top: 2px;
}

/* ── Moderation Grid ── */
.moderation-section {
  max-width: 1200px;
  margin: 0 auto 80px;
  padding: 0 24px;
}

.moderation-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: 20px;
}

.moderation-card {
  transition: all var(--duration-normal, .3s) var(--ease-out-expo);
}

.moderation-card:hover {
  transform: translateY(-6px);
}

.moderation-card :deep(.el-card) {
  border-radius: var(--radius-lg, 18px);
  border: 1px solid var(--border-color, #E8E2D8);
  overflow: hidden;
  transition: box-shadow var(--duration-normal, .3s);
}

.moderation-card:hover :deep(.el-card) {
  box-shadow: var(--shadow-card-hover, 0 16px 40px rgba(0,0,0,.12));
}

/* Card Header */
.card-header {
  padding: 18px 20px;
  border-bottom: 1px solid var(--border-light, #F2EDE6);
  position: relative;
}

.status-pending {
  background: #FFFBE6;
}

.status-approved {
  background: #F6FFED;
}

.status-rejected {
  background: #FFF0F6;
}

.work-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main, #1A1A1A);
  margin: 0 0 6px 0;
  padding-right: 80px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.work-author {
  font-size: 13px;
  color: var(--text-secondary, #555555);
  margin: 0 0 2px 0;
}

.work-category {
  font-size: 11px;
  color: var(--text-light, #888888);
  margin: 0;
}

.status-badge {
  position: absolute;
  top: 18px;
  right: 20px;
  padding: 3px 10px;
  border-radius: var(--radius-full, 9999px);
  font-size: 11px;
  font-weight: 600;
}

.status-pending .status-badge {
  background: #FAAD14;
  color: #fff;
}

.status-approved .status-badge {
  background: var(--success, #34C759);
  color: #fff;
}

.status-rejected .status-badge {
  background: var(--danger, #E05555);
  color: #fff;
}

.excellent-badge {
  position: absolute;
  top: 18px;
  right: 90px;
  padding: 3px 10px;
  border-radius: var(--radius-full, 9999px);
  font-size: 11px;
  font-weight: 600;
  background: var(--accent-light, #F0E6D0);
  color: var(--accent-strong, #B8943F);
  display: flex;
  align-items: center;
  gap: 4px;
}

.excellent-badge i {
  font-size: 12px;
}

/* Card Body */
.card-body {
  padding: 18px 20px;
}

.content-preview {
  margin-bottom: 16px;
}

.image-preview {
  border-radius: var(--radius-md, 14px);
  overflow: hidden;
  margin-bottom: 12px;
}

.preview-image {
  width: 100%;
  height: 180px;
  object-fit: cover;
  transition: transform var(--duration-slow, .5s) var(--ease-out-expo);
}

.moderation-card:hover .preview-image {
  transform: scale(1.04);
}

.model-preview {
  height: 180px;
  border-radius: var(--radius-md, 14px);
  overflow: hidden;
}

.video-preview,
.document-preview,
.text-preview {
  border: 1px solid var(--border-color, #E8E2D8);
  border-radius: var(--radius-md, 14px);
  padding: 36px 20px;
  text-align: center;
  background: var(--bg-page, #F8F9F5);
  margin-bottom: 12px;
}

.video-placeholder,
.document-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

.video-placeholder i,
.document-placeholder i {
  font-size: 42px;
  color: var(--primary, #2D8A6E);
  opacity: 0.5;
}

.video-placeholder span,
.document-placeholder span {
  font-size: 13px;
  color: var(--text-light, #888888);
  font-weight: 500;
}

.text-preview p {
  color: var(--text-secondary, #555555);
  line-height: 1.6;
  margin: 0;
  font-size: 13px;
}

.moderation-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  flex-wrap: wrap;
}

.moderation-actions .el-button {
  transition: all var(--duration-fast, .18s);
}

.moderation-actions .el-button:hover {
  transform: translateY(-2px);
}

/* ========================================
   DETAIL DIALOG — BODA Design System
   ======================================== */

:deep(.review-detail-dialog) {
  border-radius: var(--radius-xl, 24px);
  overflow: hidden;
}

:deep(.review-detail-dialog .el-dialog__header) {
  padding: 0;
  border-bottom: none;
}

:deep(.review-detail-dialog .el-dialog__headerbtn) {
  top: 20px;
  right: 20px;
  z-index: 10;
  width: 36px;
  height: 36px;
  border-radius: var(--radius-full, 9999px);
  background: rgba(0,0,0,.04);
  transition: background var(--duration-fast, .18s);
}

:deep(.review-detail-dialog .el-dialog__headerbtn:hover) {
  background: rgba(0,0,0,.08);
}

:deep(.review-detail-dialog .el-dialog__body) {
  padding: 24px 28px 8px;
}

.review-detail {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Hero Card */
.detail-hero-card {
  display: flex;
  gap: 20px;
  padding: 20px;
  background: var(--bg-card, #FFFFFF);
  border: 1px solid var(--border-color, #E8E2D8);
  border-radius: var(--radius-lg, 18px);
  box-shadow: var(--shadow-card, 0 2px 12px rgba(0,0,0,.04));
}

.detail-cover {
  width: 140px;
  height: 140px;
  border-radius: var(--radius-md, 14px);
  overflow: hidden;
  flex-shrink: 0;
  background: var(--border-light, #F2EDE6);
}

.detail-cover-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.detail-cover-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  background: linear-gradient(135deg, var(--primary-bg, #EDF5F0), #D4EBE0);
}

.cover-emoji {
  font-size: 36px;
  opacity: 0.6;
}

.cover-label {
  font-size: 11px;
  font-weight: 600;
  color: var(--primary, #2D8A6E);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.detail-meta {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
}

.detail-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0;
  line-height: 1.3;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.detail-author-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.author-avatar-sm {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: var(--primary-bg, #EDF5F0);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 700;
  color: var(--primary, #2D8A6E);
  flex-shrink: 0;
}

.author-name {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-secondary, #555555);
}

.detail-tags-row {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.info-tag {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  border-radius: var(--radius-full, 9999px);
  background: var(--bg-hover, #F5F2EC);
  font-size: 11px;
  font-weight: 500;
  color: var(--text-secondary, #555555);
}

.tag-icon {
  width: 14px;
  height: 14px;
  opacity: 0.5;
}

/* Risk Bar */
.risk-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border-radius: var(--radius-sm, 10px);
  margin-top: auto;
}

.risk-bar--high {
  background: #FFF0F6;
}

.risk-bar--medium {
  background: #FFFBE6;
}

.risk-bar--low {
  background: #F6FFED;
}

.risk-bar-icon {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-xs, 6px);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.risk-bar--high .risk-bar-icon {
  background: rgba(207, 19, 34, .1);
  color: #CF1322;
}

.risk-bar--medium .risk-bar-icon {
  background: rgba(212, 136, 6, .1);
  color: #D48806;
}

.risk-bar--low .risk-bar-icon {
  background: rgba(56, 158, 13, .1);
  color: #389E0D;
}

.risk-bar-icon svg {
  width: 16px;
  height: 16px;
}

.risk-bar-body {
  display: flex;
  flex-direction: column;
}

.risk-bar-label {
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-light, #888888);
}

.risk-bar-value {
  font-size: 13px;
  font-weight: 700;
}

.risk-bar--high .risk-bar-value {
  color: #CF1322;
}

.risk-bar--medium .risk-bar-value {
  color: #D48806;
}

.risk-bar--low .risk-bar-value {
  color: #389E0D;
}

/* Detail Sections */
.detail-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.section-heading {
  font-size: 13px;
  font-weight: 700;
  color: var(--text-main, #1A1A1A);
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  padding-left: 12px;
  border-left: 3px solid var(--primary, #2D8A6E);
}

/* Preview Card */
.preview-card {
  border-radius: var(--radius-lg, 18px);
  overflow: hidden;
  border: 1px solid var(--border-color, #E8E2D8);
  background: var(--bg-card, #FFFFFF);
}

.preview-image-wrap {
  width: 100%;
  max-height: 360px;
  overflow: hidden;
}

.preview-img {
  width: 100%;
  display: block;
  object-fit: contain;
  max-height: 360px;
}

.preview-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 48px 24px;
  background: var(--bg-page, #F8F9F5);
}

.empty-icon {
  width: 48px;
  height: 48px;
  color: var(--text-placeholder, #B0B0B0);
}

.preview-empty-state span {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-light, #888888);
}

.preview-text {
  padding: 20px;
}

.preview-text p {
  font-size: 14px;
  line-height: 1.8;
  color: var(--text-regular, #333333);
  margin: 0;
  white-space: pre-wrap;
}

/* Risk Items */
.risk-items {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.risk-item {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 12px 16px;
  background: var(--bg-card, #FFFFFF);
  border: 1px solid var(--border-color, #E8E2D8);
  border-radius: var(--radius-sm, 10px);
  transition: all var(--duration-fast, .18s);
}

.risk-item:hover {
  border-color: var(--warning, #F09342);
  background: #FFFBF0;
}

.risk-item-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--warning, #F09342);
  margin-top: 6px;
  flex-shrink: 0;
}

.risk-item-text {
  font-size: 13px;
  line-height: 1.6;
  color: var(--text-regular, #333333);
}

/* Dialog Footer */
.dialog-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
  width: 100%;
}

.footer-right {
  display: flex;
  gap: 10px;
  margin-left: auto;
}

.btn-download {
  background: var(--bg-hover, #F5F2EC);
  border: 1px solid var(--border-color, #E8E2D8);
  color: var(--primary, #2D8A6E);
  border-radius: var(--radius-sm, 10px);
  font-weight: 500;
}

.btn-download:hover {
  background: var(--primary-bg, #EDF5F0);
  border-color: var(--primary, #2D8A6E);
}

.btn-cancel {
  background: var(--bg-hover, #F5F2EC);
  border: 1px solid var(--border-color, #E8E2D8);
  color: var(--text-secondary, #555555);
  border-radius: var(--radius-sm, 10px);
  font-weight: 500;
}

.btn-cancel:hover {
  background: var(--border-light, #F2EDE6);
}

.btn-reject {
  border-radius: var(--radius-sm, 10px);
  font-weight: 500;
}

.btn-approve {
  border-radius: var(--radius-sm, 10px);
  font-weight: 500;
}

/* Reject Dialog */
.reject-dialog-content {
  padding: 8px 0;
}

.reject-tip {
  font-size: 14px;
  color: var(--text-secondary, #555555);
  margin: 0 0 16px 0;
  line-height: 1.5;
}

.reject-textarea :deep(textarea) {
  border-radius: var(--radius-md, 14px);
  font-family: inherit;
  font-size: 14px;
}

/* Animations */
.fade-in-up {
  animation: fadeInUp 0.6s var(--ease-out-expo) both;
}

.stagger-fade-in {
  opacity: 0;
  animation: fadeInUp 0.6s var(--ease-out-expo) forwards;
}

@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@keyframes fadeInUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

/* Responsive */
@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    gap: 12px;
    text-align: center;
  }

  .moderation-grid {
    grid-template-columns: 1fr;
  }

  .detail-hero-card {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }

  .detail-cover {
    width: 120px;
    height: 120px;
  }

  .detail-author-row {
    justify-content: center;
  }

  .detail-tags-row {
    justify-content: center;
  }

  .filter-tags {
    justify-content: center;
  }

  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 480px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }

  .moderation-actions {
    flex-direction: column;
  }

  .moderation-actions .el-button {
    width: 100%;
  }
}
</style>