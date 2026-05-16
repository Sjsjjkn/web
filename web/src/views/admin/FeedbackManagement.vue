<template>
  <div class="feedback-management-container">
    <div class="page-header">
      <h2 class="page-title">反馈管理</h2>
      <p class="page-subtitle">查看和处理用户反馈</p>
    </div>

    <!-- 筛选栏 -->
    <div class="filter-bar">
      <div class="filter-item">
        <el-select v-model="filterStatus" placeholder="全部状态" class="filter-select">
          <el-option label="全部" value=""></el-option>
          <el-option label="待处理" value="pending"></el-option>
          <el-option label="处理中" value="processing"></el-option>
          <el-option label="已解决" value="resolved"></el-option>
          <el-option label="已驳回" value="rejected"></el-option>
        </el-select>
      </div>
      <div class="filter-item">
        <el-select v-model="filterType" placeholder="全部类型" class="filter-select">
          <el-option label="全部" value=""></el-option>
          <el-option label="功能建议" value="suggestion"></el-option>
          <el-option label="问题投诉" value="complaint"></el-option>
          <el-option label="问题咨询" value="question"></el-option>
          <el-option label="其他" value="other"></el-option>
        </el-select>
      </div>
      <button class="btn btn-primary" @click="loadFeedbacks">
        刷新
      </button>
    </div>

    <!-- 统计卡片 -->
    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-value">{{ stats.total }}</div>
        <div class="stat-label">总反馈</div>
      </div>
      <div class="stat-card pending">
        <div class="stat-value">{{ stats.pending }}</div>
        <div class="stat-label">待处理</div>
      </div>
      <div class="stat-card processing">
        <div class="stat-value">{{ stats.processing }}</div>
        <div class="stat-label">处理中</div>
      </div>
      <div class="stat-card resolved">
        <div class="stat-value">{{ stats.resolved }}</div>
        <div class="stat-label">已解决</div>
      </div>
    </div>

    <!-- 反馈列表 -->
    <div class="feedback-list-card">
      <div class="card-header">
        <h3>反馈列表</h3>
      </div>
      <div class="card-body">
        <div v-if="feedbacks.length === 0" class="empty-state">
          <div class="empty-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
            </svg>
          </div>
          <p>暂无反馈</p>
        </div>

        <div v-else class="feedback-table">
          <table>
            <thead>
              <tr>
                <th>标题</th>
                <th>类型</th>
                <th>用户</th>
                <th>联系方式</th>
                <th>状态</th>
                <th>提交时间</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="feedback in feedbacks" :key="feedback.id">
                <td class="title-cell" @click="showFeedbackDetail(feedback)">{{ feedback.title }}</td>
                <td><span :class="['type-tag', feedback.type]">{{ getTypeLabel(feedback.type) }}</span></td>
                <td>{{ feedback.userName }}</td>
                <td>{{ feedback.contact || '-' }}</td>
                <td><span :class="['status-tag', feedback.status]">{{ getStatusLabel(feedback.status) }}</span></td>
                <td>{{ formatTime(feedback.createdAt) }}</td>
                <td>
                  <button class="btn btn-sm btn-primary" @click="showFeedbackDetail(feedback)">查看</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- 反馈详情弹窗 -->
    <div class="modal-overlay" v-if="showDetailModal" @click.self="showDetailModal = false">
      <div class="modal-content">
        <div class="modal-header">
          <div class="header-info">
            <div class="header-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"/>
              </svg>
            </div>
            <h3>{{ selectedFeedback?.title }}</h3>
          </div>
          <button class="close-btn" @click="showDetailModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <!-- 基本信息卡片 -->
          <div class="info-card">
            <div class="card-title">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              <span>基本信息</span>
            </div>
            <div class="info-grid">
              <div class="info-item">
                <span class="info-label">类型</span>
                <span :class="['info-tag', 'type', selectedFeedback?.type]">{{ getTypeLabel(selectedFeedback?.type) }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">状态</span>
                <span :class="['info-tag', 'status', selectedFeedback?.status]">{{ getStatusLabel(selectedFeedback?.status) }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">用户</span>
                <span class="info-text">{{ selectedFeedback?.userName }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">联系</span>
                <span class="info-text">{{ selectedFeedback?.contact || '未提供' }}</span>
              </div>
              <div class="info-item full">
                <span class="info-label">时间</span>
                <span class="info-text">{{ formatTime(selectedFeedback?.createdAt) }}</span>
              </div>
            </div>
          </div>

          <!-- 反馈内容卡片 -->
          <div class="content-card">
            <div class="card-title">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
              </svg>
              <span>反馈内容</span>
            </div>
            <div class="content-body">
              {{ selectedFeedback?.content }}
            </div>
          </div>

          <!-- 回复内容卡片 -->
          <div v-if="selectedFeedback?.reply" class="reply-card">
            <div class="card-title">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"/>
              </svg>
              <span>管理员回复</span>
            </div>
            <div class="reply-body">
              <div class="reply-badge">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                </svg>
                <span>管理员</span>
              </div>
              <p>{{ selectedFeedback?.reply }}</p>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <div class="status-actions" v-if="selectedFeedback?.status !== 'resolved' && selectedFeedback?.status !== 'rejected'">
            <button class="btn btn-secondary" @click="updateStatus('processing')">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
              </svg>
              处理中
            </button>
            <button class="btn btn-warning" @click="updateStatus('rejected')">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              驳回
            </button>
          </div>
          <div v-if="selectedFeedback?.status !== 'resolved'">
            <button class="btn btn-primary" @click="showReplyForm = true">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"/>
              </svg>
              回复
            </button>
          </div>
          <button class="btn btn-secondary" @click="showDetailModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
            关闭
          </button>
        </div>

        <!-- 回复表单 -->
        <div v-if="showReplyForm" class="reply-section">
          <div class="reply-header">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
            </svg>
            <h4>回复反馈</h4>
          </div>
          <textarea
            v-model="replyContent"
            class="reply-textarea"
            placeholder="请输入回复内容..."
          ></textarea>
          <div class="reply-actions">
            <button class="btn btn-secondary" @click="showReplyForm = false">取消</button>
            <button class="btn btn-primary" @click="submitReply">提交回复</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { feedbackApi } from '../../services/api'
import eventBus from '../../utils/eventBus'

export default {
  name: 'FeedbackManagement',
  data() {
    return {
      feedbacks: [],
      filterStatus: '',
      filterType: '',
      showDetailModal: false,
      selectedFeedback: null,
      showReplyForm: false,
      replyContent: '',
      stats: {
        total: 0,
        pending: 0,
        processing: 0,
        resolved: 0
      }
    }
  },
  mounted() {
    this.loadFeedbacks()
  },
  methods: {
    async loadFeedbacks() {
      try {
        const params = {}
        if (this.filterStatus) params.status = this.filterStatus
        if (this.filterType) params.type = this.filterType
        
        const response = await feedbackApi.getFeedbacks(params)
        this.feedbacks = response.data.items || []
        this.calculateStats()
      } catch (error) {
        console.error('加载反馈失败:', error)
        this.$message.error('加载反馈失败')
      }
    },

    calculateStats() {
      this.stats.total = this.feedbacks.length
      this.stats.pending = this.feedbacks.filter(f => f.status === 'pending').length
      this.stats.processing = this.feedbacks.filter(f => f.status === 'processing').length
      this.stats.resolved = this.feedbacks.filter(f => f.status === 'resolved').length
    },

    showFeedbackDetail(feedback) {
      this.selectedFeedback = feedback
      this.showDetailModal = true
      this.showReplyForm = false
      this.replyContent = ''
    },

    async updateStatus(status) {
      if (!this.selectedFeedback) return
      
      try {
        await feedbackApi.updateFeedbackStatus(this.selectedFeedback.id, { status })
        this.selectedFeedback.status = status
        this.$message.success('状态更新成功')
        this.loadFeedbacks()
      } catch (error) {
        this.$message.error('更新失败')
      }
    },

    async submitReply() {
      if (!this.selectedFeedback || !this.replyContent.trim()) {
        this.$message.warning('请输入回复内容')
        return
      }

      try {
        await feedbackApi.replyFeedback(this.selectedFeedback.id, { reply: this.replyContent })
        this.selectedFeedback.reply = this.replyContent
        this.selectedFeedback.status = 'resolved'
        this.showReplyForm = false
        this.replyContent = ''
        this.$message.success('回复成功')
        this.loadFeedbacks()
        // 通知其他组件刷新通知
        eventBus.$emit('notification-updated')
      } catch (error) {
        this.$message.error('回复失败')
      }
    },

    getTypeLabel(type) {
      const types = {
        suggestion: '功能建议',
        complaint: '问题投诉',
        question: '问题咨询',
        other: '其他'
      }
      return types[type] || type
    },

    getStatusLabel(status) {
      const statuses = {
        pending: '待处理',
        processing: '处理中',
        resolved: '已解决',
        rejected: '已驳回'
      }
      return statuses[status] || status
    },

    formatTime(dateStr) {
      if (!dateStr) return ''
      const date = new Date(dateStr)
      return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      })
    }
  }
}
</script>

<style scoped>
.feedback-management-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

.page-header {
  margin-bottom: 24px;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: #1a1a2e;
  margin: 0 0 8px 0;
}

.page-subtitle {
  color: #666;
  margin: 0;
}

.filter-bar {
  display: flex;
  gap: 16px;
  align-items: center;
  margin-bottom: 20px;
}

.filter-select {
  width: 150px;
}

.btn {
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 14px;
}

.btn-primary {
  background: #409eff;
  color: #fff;
}

.btn-primary:hover {
  background: #66b1ff;
}

.btn-secondary {
  background: #f5f5f5;
  color: #666;
}

.btn-secondary:hover {
  background: #eee;
}

.btn-sm {
  padding: 4px 12px;
  font-size: 12px;
}

.btn-warning {
  background: #f5a623;
  color: #fff;
}

.stats-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  margin-bottom: 20px;
}

.stat-card {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  text-align: center;
}

.stat-card .stat-value {
  font-size: 28px;
  font-weight: 600;
  color: #333;
  margin-bottom: 4px;
}

.stat-card .stat-label {
  font-size: 14px;
  color: #999;
}

.stat-card.pending .stat-value { color: #d48806; }
.stat-card.processing .stat-value { color: #1890ff; }
.stat-card.resolved .stat-value { color: #52c41a; }

.feedback-list-card {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.08);
}

.card-header {
  padding: 16px 20px;
  border-bottom: 1px solid #eee;
}

.card-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #333;
}

.card-body {
  padding: 20px;
}

.empty-state {
  text-align: center;
  padding: 40px 0;
  color: #999;
}

.empty-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 16px;
  color: #ccc;
}

.feedback-table {
  overflow-x: auto;
}

.feedback-table table {
  width: 100%;
  border-collapse: collapse;
}

.feedback-table th,
.feedback-table td {
  padding: 12px 16px;
  text-align: left;
  border-bottom: 1px solid #eee;
}

.feedback-table th {
  background: #f9f9f9;
  font-weight: 600;
  color: #666;
  font-size: 13px;
}

.feedback-table .title-cell {
  cursor: pointer;
  color: #409eff;
}

.feedback-table .title-cell:hover {
  text-decoration: underline;
}

.type-tag,
.status-tag {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 4px;
}

.type-tag.suggestion { background: #e6f7ff; color: #1890ff; }
.type-tag.complaint { background: #fff2f0; color: #ff4d4f; }
.type-tag.question { background: #f6ffed; color: #52c41a; }
.type-tag.other { background: #f5f5f5; color: #666; }

.status-tag.pending { background: #fff7e6; color: #d48806; }
.status-tag.processing { background: #e6f7ff; color: #1890ff; }
.status-tag.resolved { background: #f6ffed; color: #52c41a; }
.status-tag.rejected { background: #fff2f0; color: #ff4d4f; }

/* 弹窗样式 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(26, 26, 26, 0.5);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  animation: fadeIn 0.25s var(--ease-out-expo);
  padding: 20px;
}

.modal-content {
  background: var(--bg-card);
  border-radius: var(--radius-xl);
  width: 100%;
  max-width: 620px;
  max-height: 90vh;
  overflow: hidden;
  box-shadow: var(--shadow-popup);
  animation: slideUp 0.3s var(--ease-out-back);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px 28px;
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  position: relative;
  overflow: hidden;
}

.modal-header::before {
  content: '';
  position: absolute;
  top: -50%;
  right: -20%;
  width: 140px;
  height: 140px;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 50%;
}

.header-info {
  display: flex;
  align-items: center;
  gap: 12px;
  position: relative;
  z-index: 1;
}

.header-icon {
  width: 40px;
  height: 40px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
}

.header-icon svg {
  width: 20px;
  height: 20px;
  color: #fff;
}

.modal-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #fff;
  position: relative;
  z-index: 1;
}

.close-btn {
  background: rgba(255, 255, 255, 0.2);
  border: none;
  cursor: pointer;
  color: #fff;
  padding: 8px;
  border-radius: var(--radius-md);
  transition: all var(--duration-fast);
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  z-index: 1;
}

.close-btn:hover {
  background: rgba(255, 255, 255, 0.35);
  transform: rotate(90deg);
}

.close-btn svg {
  width: 20px;
  height: 20px;
}

.modal-body {
  padding: 24px;
  max-height: 500px;
  overflow-y: auto;
}

/* 卡片通用样式 */
.info-card,
.content-card,
.reply-card {
  background: var(--bg-card);
  border-radius: var(--radius-lg);
  padding: 20px;
  margin-bottom: 20px;
  border: 1px solid var(--border-light);
  box-shadow: var(--shadow-xs);
}

.card-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 15px;
  font-weight: 600;
  color: var(--text-main);
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--border-light);
}

.card-title svg {
  width: 18px;
  height: 18px;
  color: var(--primary);
}

/* 信息卡片网格 */
.info-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 14px;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.info-item.full {
  grid-column: span 2;
}

.info-label {
  font-size: 12px;
  font-weight: 500;
  color: var(--text-light);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.info-text {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-main);
}

/* 标签样式 */
.info-tag {
  display: inline-flex;
  align-items: center;
  padding: 6px 14px;
  border-radius: var(--radius-full);
  font-size: 12px;
  font-weight: 600;
  width: fit-content;
}

.info-tag.type.suggestion { 
  background: linear-gradient(135deg, #E6F7FF 0%, #B3DFFF 100%); 
  color: var(--info); 
}
.info-tag.type.complaint { 
  background: linear-gradient(135deg, #FFF2F0 0%, #FFCCC7 100%); 
  color: var(--danger); 
}
.info-tag.type.question { 
  background: linear-gradient(135deg, #F6FFED 0%, #D9F7BE 100%); 
  color: var(--success); 
}
.info-tag.type.other { 
  background: linear-gradient(135deg, #F5F5F5 0%, #E8E8E8 100%); 
  color: var(--text-secondary); 
}

.info-tag.status.pending { 
  background: linear-gradient(135deg, #FFF7E6 0%, #FFE0B2 100%); 
  color: #D48806; 
}
.info-tag.status.processing { 
  background: linear-gradient(135deg, #E6F7FF 0%, #91D5FF 100%); 
  color: var(--info); 
}
.info-tag.status.resolved { 
  background: linear-gradient(135deg, #F6FFED 0%, #B7EB8F 100%); 
  color: var(--success); 
}
.info-tag.status.rejected { 
  background: linear-gradient(135deg, #FFF2F0 0%, #FFCCC7 100%); 
  color: var(--danger); 
}

/* 内容卡片 */
.content-body {
  font-size: 14px;
  line-height: 1.8;
  color: var(--text-regular);
  padding: 16px;
  background: var(--bg-hover);
  border-radius: var(--radius-md);
  border-left: 3px solid var(--primary);
}

/* 回复卡片 */
.reply-body {
  padding: 16px;
  background: linear-gradient(135deg, var(--primary-bg) 0%, #E0F0E8 100%);
  border-radius: var(--radius-md);
  border: 1px solid var(--primary-bg);
  position: relative;
}

.reply-body::before {
  content: '';
  position: absolute;
  left: 20px;
  top: -8px;
  width: 0;
  height: 0;
  border-left: 8px solid transparent;
  border-right: 8px solid transparent;
  border-bottom: 8px solid var(--primary-bg);
}

.reply-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: rgba(45, 138, 110, 0.15);
  padding: 4px 10px;
  border-radius: var(--radius-full);
  font-size: 11px;
  font-weight: 600;
  color: var(--primary);
  margin-bottom: 10px;
}

.reply-badge svg {
  width: 12px;
  height: 12px;
}

.reply-body p {
  margin: 0;
  font-size: 14px;
  line-height: 1.7;
  color: var(--text-regular);
}

.modal-footer {
  padding: 20px 28px;
  border-top: 1px solid var(--border-color);
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  flex-wrap: wrap;
  background: var(--bg-page);
}

.status-actions {
  display: flex;
  gap: 10px;
}

.status-actions .btn svg {
  width: 14px;
  height: 14px;
}

.modal-footer .btn svg {
  width: 14px;
  height: 14px;
}

.reply-section {
  padding: 0 28px 24px;
  border-top: 1px dashed var(--border-color);
  margin-top: 20px;
}

.reply-header {
  padding: 20px 0 12px;
  display: flex;
  align-items: center;
  gap: 10px;
}

.reply-header svg {
  width: 18px;
  height: 18px;
  color: var(--primary);
}

.reply-header h4 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main);
}

.reply-textarea {
  width: 100%;
  min-height: 120px;
  padding: 16px;
  border: 2px solid var(--border-color);
  border-radius: var(--radius-lg);
  resize: vertical;
  font-size: 14px;
  line-height: 1.6;
  box-sizing: border-box;
  color: var(--text-main);
  background: var(--input-bg);
  transition: all var(--duration-fast);
  font-family: var(--font-main);
}

.reply-textarea:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 4px var(--primary-glow);
  background: var(--bg-card);
}

.reply-actions {
  display: flex;
  justify-content: flex-end;
  gap: 14px;
  margin-top: 16px;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUp {
  from { 
    opacity: 0; 
    transform: translateY(30px) scale(0.98); 
  }
  to { 
    opacity: 1; 
    transform: translateY(0) scale(1); 
  }
}

/* 按钮样式优化 */
.btn {
  padding: 10px 20px;
  border-radius: var(--radius-md);
  border: none;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all var(--duration-fast);
  font-family: var(--font-main);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.btn-primary {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: #fff;
  box-shadow: 0 4px 14px var(--primary-glow);
}

.btn-primary:hover {
  background: linear-gradient(135deg, var(--primary-hover) 0%, var(--primary) 100%);
  transform: translateY(-1px);
  box-shadow: 0 6px 20px var(--primary-glow);
}

.btn-secondary {
  background: var(--bg-hover);
  color: var(--text-secondary);
  border: 1px solid var(--border-color);
}

.btn-secondary:hover {
  background: var(--border-light);
  color: var(--text-main);
}

.btn-sm {
  padding: 6px 14px;
  font-size: 12px;
}

.btn-warning {
  background: linear-gradient(135deg, var(--warning) 0%, #E88535 100%);
  color: #fff;
  box-shadow: 0 4px 14px rgba(240, 147, 66, 0.3);
}

.btn-warning:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(240, 147, 66, 0.4);
}
</style>
