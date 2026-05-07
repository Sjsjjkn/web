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
                  <img :src="item.preview" :alt="item.title" class="preview-image">
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
                  <p>{{ item.content.substring(0, 100) }}...</p>
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
                  @click="rejectItem(item)"
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

    <div v-if="currentItem" class="detail-dialog">
      <el-dialog
        :title="currentItem.title"
        :visible.sync="dialogVisible"
        width="800px"
        append-to-body
        @close="currentItem = null"
      >
        <div class="dialog-content">
          <div class="detail-info">
            <p><strong>作者：</strong>{{ currentItem.author }}</p>
            <p><strong>类别：</strong>{{ currentItem.category }}</p>
            <p><strong>类型：</strong>{{ getTypeText(currentItem.type) }}</p>
            <p><strong>提交时间：</strong>{{ currentItem.submitTime }}</p>
            <p><strong>风险等级：</strong><span :class="['risk-tag', getRiskClass(currentItem.riskLevel)]">{{ getRiskText(currentItem.riskLevel) }}</span></p>
          </div>
          
          <div class="detail-content">
            <h3>内容详情</h3>
            <div v-if="currentItem.type === 'image'" class="detail-image">
              <img :src="currentItem.preview" :alt="currentItem.title" style="max-width: 100%;">
            </div>
            <div v-else-if="currentItem.type === 'video'" class="detail-video">
              <div class="video-placeholder large">
                <i class="el-icon-video-camera"></i>
                <span>视频内容</span>
              </div>
            </div>
            <div v-else-if="currentItem.type === 'document'" class="detail-document">
              <div class="document-placeholder large">
                <i class="el-icon-document"></i>
                <span>{{ currentItem.fileName }}</span>
              </div>
            </div>
            <div v-else class="detail-text">
              <p>{{ currentItem.content }}</p>
            </div>
          </div>
          
          <div v-if="currentItem.riskDetails.length > 0" class="detail-risks">
            <h3>风险详情</h3>
            <ul class="risk-list">
              <li v-for="(risk, idx) in currentItem.riskDetails" :key="idx">
                <i class="el-icon-warning"></i>
                <span>{{ risk }}</span>
              </li>
            </ul>
          </div>
        </div>
        
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">关闭</el-button>
          <el-button type="primary" @click="approveItem(currentItem)">通过</el-button>
          <el-button type="danger" @click="openRejectDialog(currentItem)">拒绝</el-button>
        </div>
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
  </div>
</template>

<script>
import http from '../../utils/http'

export default {
  name: 'ContentModeration',
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
        { label: '待审核', value: 12, icon: 'el-icon-time', color: '#fffbe6' },
        { label: '已审核', value: 156, icon: 'el-icon-check', color: '#f6ffed' },
        { label: '优秀作品', value: 8, icon: 'el-icon-star-on', color: '#fff7e6' }
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
        // 审核列表接口需要登录；token 由统一 http 实例自动注入
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
        // 保持当前统计数据
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
    
    async approveItem(item) {
      item.approving = true
      try {
        await http.put(`/api/ContentModeration/approve/${item.id}`, {})
        
        item.status = '已发布'
        this.$message.success('审核通过')
        this.loadStats()
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
    
    async rejectItem(item) {
      item.rejecting = true
      try {
        await http.put(`/api/ContentModeration/reject/${item.id}`, { reason: '未提供具体原因' })
        
        item.status = '已拒绝'
        this.$message.success('已拒绝')
        this.loadStats()
      } catch (error) {
        this.$message.error(error.response?.data?.message || '操作失败')
        console.error('拒绝失败:', error)
      } finally {
        item.rejecting = false
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
      this.currentItem = item
      this.dialogVisible = true
    },
    
    updateStats() {
      const pending = this.moderationItems.filter(item => item.status === 'pending').length
      const approved = this.moderationItems.filter(item => item.status === 'approved').length
      const rejected = this.moderationItems.filter(item => item.status === 'rejected').length
      const highRisk = this.moderationItems.filter(item => item.riskLevel === 'high').length
      
      this.stats[0].value = pending
      this.stats[1].value = approved
      this.stats[2].value = rejected
      this.stats[3].value = highRisk
    }
  }
}
</script>

<style scoped>
.content-moderation-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  font-family: 'PingFang SC', 'Helvetica Neue', Arial, sans-serif;
}

.moderation-header {
  background: white;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 100;
  padding: 20px 0;
  animation: slideDown 0.6s ease-out both;
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
  background: linear-gradient(135deg, #0052D9, #1890FF);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: bold;
  font-size: 16px;
}

.system-title {
  font-size: 20px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}

.refresh-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.refresh-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 82, 217, 0.3);
}

.filter-section {
  max-width: 1200px;
  margin: 40px auto 32px;
  padding: 0 24px;
}

.search-bar {
  margin-bottom: 24px;
}

.animated-input {
  border-radius: 24px;
  transition: all 0.3s ease;
  border: 2px solid #e4e7ed;
}

.animated-input:focus {
  border-color: #0052D9;
  box-shadow: 0 0 0 2px rgba(0, 82, 217, 0.2);
  transform: scale(1.02);
}

.filter-tags {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.filter-tag {
  padding: 8px 16px;
  border-radius: 20px;
  background: white;
  border: 1px solid #e4e7ed;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 14px;
  color: #666;
}

.filter-tag:hover {
  border-color: #0052D9;
  color: #0052D9;
  transform: translateY(-2px);
}

.filter-tag.active {
  background: #0052D9;
  color: white;
  border-color: #0052D9;
}

.stats-section {
  max-width: 1200px;
  margin: 0 auto 32px;
  padding: 0 24px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 24px;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  border: 1px solid rgba(0,0,0,0.05);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}

.stat-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 16px 32px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  width: 64px;
  height: 64px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  color: #0052D9;
}

.stat-number {
  font-size: 32px;
  font-weight: 700;
  color: #333;
  line-height: 1.2;
}

.stat-label {
  font-size: 14px;
  color: #666;
}

.moderation-section {
  max-width: 1200px;
  margin: 0 auto 60px;
  padding: 0 24px;
}

.moderation-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: 24px;
}

.moderation-card {
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.moderation-card:hover {
  transform: translateY(-8px);
}

.card-header {
  padding: 20px;
  border-bottom: 1px solid #f0f0f0;
  position: relative;
}

.status-pending {
  background: #fffbe6;
  border-color: #ffe58f;
}

.status-approved {
  background: #f6ffed;
  border-color: #b7eb8f;
}

.status-rejected {
  background: #fff0f6;
  border-color: #ffadd2;
}

.work-title {
  font-size: 18px;
  font-weight: 600;
  color: #333;
  margin: 0 0 8px 0;
}

.work-author {
  font-size: 14px;
  color: #666;
  margin: 0 0 4px 0;
}

.work-category {
  font-size: 12px;
  color: #999;
  margin: 0;
}

.status-badge {
  position: absolute;
  top: 20px;
  right: 20px;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.status-pending .status-badge {
  background: #faad14;
  color: white;
}

.status-approved .status-badge {
  background: #52c41a;
  color: white;
}

.status-rejected .status-badge {
  background: #ff4d4f;
  color: white;
}

.excellent-badge {
  position: absolute;
  top: 20px;
  right: 100px;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  background: #faad14;
  color: white;
  display: flex;
  align-items: center;
  gap: 4px;
}

.excellent-badge i {
  font-size: 14px;
}

.card-body {
  padding: 20px;
}

.content-preview {
  margin-bottom: 20px;
}

.image-preview {
  border-radius: 8px;
  overflow: hidden;
  margin-bottom: 16px;
}

.preview-image {
  width: 100%;
  height: 180px;
  object-fit: cover;
}

.video-preview, .document-preview, .text-preview {
  border: 1px solid #e4e7ed;
  border-radius: 8px;
  padding: 40px 20px;
  text-align: center;
  background: #fafafa;
  margin-bottom: 16px;
}

.video-placeholder, .document-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.video-placeholder i, .document-placeholder i {
  font-size: 48px;
  color: #0052D9;
}

.video-placeholder span, .document-placeholder span {
  font-size: 14px;
  color: #666;
}

.text-preview p {
  color: #666;
  line-height: 1.6;
  margin: 0;
}

.moderation-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  flex-wrap: wrap;
}

.moderation-actions .el-button {
  transition: all 0.3s ease;
}

.moderation-actions .el-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.dialog-content {
  max-height: 500px;
  overflow-y: auto;
}

.detail-info {
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid #f0f0f0;
}

.detail-info p {
  margin: 8px 0;
  color: #666;
}

.risk-tag {
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 600;
}

.detail-content {
  margin-bottom: 24px;
}

.detail-content h3 {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0 0 16px 0;
}

.detail-image img {
  border-radius: 8px;
}

.video-placeholder.large, .document-placeholder.large {
  padding: 80px 40px;
}

.video-placeholder.large i, .document-placeholder.large i {
  font-size: 72px;
}

.detail-text p {
  line-height: 1.8;
  color: #333;
}

.detail-risks h3 {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0 0 16px 0;
}

.dialog-footer {
  text-align: right;
}

.fade-in-up {
  animation: fadeInUp 0.6s ease-out both;
}

.stagger-fade-in {
  opacity: 0;
  animation: fadeInUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@keyframes fadeInUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    gap: 16px;
    text-align: center;
  }
  
  .moderation-grid {
    grid-template-columns: 1fr;
  }
  
  .filter-tags {
    justify-content: center;
  }
  
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

/* 拒绝对话框样式 */
.reject-dialog-content {
  padding: 16px 0;
}

.reject-tip {
  font-size: 14px;
  color: #666;
  margin: 0 0 16px 0;
}

.reject-textarea {
  width: 100%;
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