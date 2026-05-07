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
          <h3>{{ selectedFeedback?.title }}</h3>
          <button class="close-btn" @click="showDetailModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <div class="detail-row">
            <span class="detail-label">反馈类型：</span>
            <span :class="['detail-value', 'type', selectedFeedback?.type]">{{ getTypeLabel(selectedFeedback?.type) }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">提交用户：</span>
            <span class="detail-value">{{ selectedFeedback?.userName }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">联系方式：</span>
            <span class="detail-value">{{ selectedFeedback?.contact || '未提供' }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">提交时间：</span>
            <span class="detail-value">{{ formatTime(selectedFeedback?.createdAt) }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">当前状态：</span>
            <span :class="['detail-value', 'status', selectedFeedback?.status]">{{ getStatusLabel(selectedFeedback?.status) }}</span>
          </div>
          <div class="detail-row full-width">
            <span class="detail-label">反馈内容：</span>
            <div class="detail-content">{{ selectedFeedback?.content }}</div>
          </div>
          <div v-if="selectedFeedback?.reply" class="detail-row full-width">
            <span class="detail-label">管理员回复：</span>
            <div class="detail-content reply">{{ selectedFeedback?.reply }}</div>
          </div>
        </div>
        <div class="modal-footer">
          <div class="status-actions" v-if="selectedFeedback?.status !== 'resolved' && selectedFeedback?.status !== 'rejected'">
            <button class="btn btn-secondary" @click="updateStatus('processing')">标记为处理中</button>
            <button class="btn btn-warning" @click="updateStatus('rejected')">驳回</button>
          </div>
          <div v-if="selectedFeedback?.status !== 'resolved'">
            <button class="btn btn-primary" @click="showReplyForm = true">回复反馈</button>
          </div>
          <button class="btn btn-secondary" @click="showDetailModal = false">关闭</button>
        </div>

        <!-- 回复表单 -->
        <div v-if="showReplyForm" class="reply-section">
          <div class="reply-header">
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
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: #fff;
  border-radius: 12px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow: hidden;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #eee;
}

.modal-header h3 {
  margin: 0;
  font-size: 16px;
}

.close-btn {
  background: none;
  border: none;
  cursor: pointer;
  color: #999;
  padding: 4px;
}

.close-btn:hover {
  color: #666;
}

.close-btn svg {
  width: 20px;
  height: 20px;
}

.modal-body {
  padding: 20px;
  max-height: 300px;
  overflow-y: auto;
}

.detail-row {
  margin-bottom: 12px;
  display: flex;
  align-items: flex-start;
}

.detail-row.full-width {
  flex-direction: column;
}

.detail-label {
  font-size: 13px;
  color: #999;
  min-width: 80px;
  flex-shrink: 0;
}

.detail-value {
  font-size: 13px;
  color: #333;
}

.detail-value.type.suggestion { color: #1890ff; }
.detail-value.type.complaint { color: #ff4d4f; }
.detail-value.type.question { color: #52c41a; }
.detail-value.type.other { color: #666; }

.detail-value.status.pending { color: #d48806; }
.detail-value.status.processing { color: #1890ff; }
.detail-value.status.resolved { color: #52c41a; }
.detail-value.status.rejected { color: #ff4d4f; }

.detail-content {
  margin-top: 4px;
  padding: 12px;
  background: #f9f9f9;
  border-radius: 8px;
  font-size: 14px;
  line-height: 1.6;
  color: #333;
}

.detail-content.reply {
  background: #f6ffed;
  color: #52c41a;
}

.modal-footer {
  padding: 16px 20px;
  border-top: 1px solid #eee;
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  flex-wrap: wrap;
}

.status-actions {
  display: flex;
  gap: 8px;
}

.reply-section {
  padding: 0 20px 20px;
  border-top: 1px solid #eee;
  margin-top: 16px;
}

.reply-header {
  padding: 16px 0 8px;
}

.reply-header h4 {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
}

.reply-textarea {
  width: 100%;
  min-height: 100px;
  padding: 12px;
  border: 1px solid #e6e6e6;
  border-radius: 8px;
  resize: vertical;
  font-size: 14px;
  box-sizing: border-box;
}

.reply-textarea:focus {
  outline: none;
  border-color: #409eff;
}

.reply-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 12px;
}
</style>
