<template>
  <div class="feedback-container">
    <div class="page-header">
      <h2 class="page-title">意见反馈</h2>
      <p class="page-subtitle">您的意见对我们很重要，感谢您的反馈</p>
    </div>

    <!-- 反馈表单 -->
    <div class="feedback-form-card">
      <div class="card-header">
        <h3>提交反馈</h3>
      </div>
      <div class="card-body">
        <el-form ref="feedbackForm" :model="feedbackForm" :rules="formRules" label-width="80px">
          <el-form-item label="反馈类型" prop="type">
            <el-select v-model="feedbackForm.type" placeholder="请选择反馈类型">
              <el-option label="功能建议" value="suggestion"></el-option>
              <el-option label="问题投诉" value="complaint"></el-option>
              <el-option label="问题咨询" value="question"></el-option>
              <el-option label="其他" value="other"></el-option>
            </el-select>
          </el-form-item>

          <el-form-item label="反馈标题" prop="title">
            <el-input v-model="feedbackForm.title" placeholder="请输入反馈标题"></el-input>
          </el-form-item>

          <el-form-item label="详细内容" prop="content">
            <textarea
              v-model="feedbackForm.content"
              class="custom-textarea"
              placeholder="请详细描述您的反馈内容..."
            ></textarea>
          </el-form-item>

          <el-form-item label="联系方式">
            <el-input v-model="feedbackForm.contact" placeholder="选填，方便我们联系您"></el-input>
          </el-form-item>

          <el-form-item class="form-actions">
            <el-button type="primary" @click="submitFeedback" :loading="submitting">
              提交反馈
            </el-button>
          </el-form-item>
        </el-form>
      </div>
    </div>

    <!-- 我的反馈列表 -->
    <div class="feedback-list-card">
      <div class="card-header">
        <h3>我的反馈记录</h3>
      </div>
      <div class="card-body">
        <div v-if="feedbackList.length === 0" class="empty-state">
          <div class="empty-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"></path>
            </svg>
          </div>
          <p>暂无反馈记录</p>
        </div>

        <div v-else class="feedback-items">
          <div
            v-for="item in feedbackList"
            :key="item.id"
            class="feedback-item"
            @click="showFeedbackDetail(item)"
          >
            <div class="feedback-item-header">
              <span class="feedback-title">{{ item.title }}</span>
              <span :class="['status-badge', item.status]">{{ getStatusLabel(item.status) }}</span>
            </div>
            <div class="feedback-item-body">
              <span class="feedback-type">{{ getTypeLabel(item.type) }}</span>
              <span class="feedback-time">{{ formatTime(item.createdAt) }}</span>
            </div>
            <div v-if="item.reply" class="feedback-reply">
              <div class="reply-label">管理员回复：</div>
              <div class="reply-content">{{ item.reply }}</div>
            </div>
          </div>
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
            <span class="detail-value">{{ getTypeLabel(selectedFeedback?.type) }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">提交时间：</span>
            <span class="detail-value">{{ formatTime(selectedFeedback?.createdAt) }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">当前状态：</span>
            <span :class="['detail-value', 'status', selectedFeedback?.status]">
              {{ getStatusLabel(selectedFeedback?.status) }}
            </span>
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
          <button class="btn btn-secondary" @click="showDetailModal = false">关闭</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { feedbackApi } from '../../services/api'

export default {
  name: 'Feedback',
  data() {
    return {
      feedbackForm: {
        type: 'suggestion',
        title: '',
        content: '',
        contact: ''
      },
      formRules: {
        type: [{ required: true, message: '请选择反馈类型', trigger: 'change' }],
        title: [{ required: true, message: '请输入反馈标题', trigger: 'blur' }],
        content: [{ required: true, message: '请输入反馈内容', trigger: 'blur' }]
      },
      submitting: false,
      feedbackList: [],
      showDetailModal: false,
      selectedFeedback: null
    }
  },
  mounted() {
    this.loadFeedbacks()
  },
  methods: {
    async submitFeedback() {
      this.$refs.feedbackForm.validate(async (valid) => {
        if (valid) {
          this.submitting = true
          try {
            const response = await feedbackApi.createFeedback(this.feedbackForm)
            this.$message.success(response.data.message)
            this.feedbackForm = {
              type: 'suggestion',
              title: '',
              content: '',
              contact: ''
            }
            this.loadFeedbacks()
          } catch (error) {
            this.$message.error('提交失败，请稍后重试')
          } finally {
            this.submitting = false
          }
        }
      })
    },

    async loadFeedbacks() {
      try {
        const response = await feedbackApi.getFeedbacks()
        this.feedbackList = response.data.items || []
      } catch (error) {
        console.error('加载反馈列表失败:', error)
      }
    },

    showFeedbackDetail(item) {
      this.selectedFeedback = item
      this.showDetailModal = true
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
.feedback-container {
  max-width: 900px;
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

.feedback-form-card,
.feedback-list-card {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  margin-bottom: 24px;
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

.custom-textarea {
  width: 100%;
  min-height: 120px;
  padding: 12px;
  border: 1px solid #e6e6e6;
  border-radius: 8px;
  resize: vertical;
  font-size: 14px;
  line-height: 1.5;
  box-sizing: border-box;
}

.custom-textarea:focus {
  outline: none;
  border-color: #409eff;
}

.form-actions {
  margin-bottom: 0;
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

.empty-icon svg {
  width: 100%;
  height: 100%;
}

.feedback-items {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.feedback-item {
  padding: 16px;
  border: 1px solid #eee;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.feedback-item:hover {
  border-color: #409eff;
  box-shadow: 0 2px 8px rgba(64, 158, 255, 0.15);
}

.feedback-item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.feedback-title {
  font-weight: 500;
  color: #333;
}

.status-badge {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 4px;
}

.status-badge.pending {
  background: #fff7e6;
  color: #d48806;
}

.status-badge.processing {
  background: #e6f7ff;
  color: #1890ff;
}

.status-badge.resolved {
  background: #f6ffed;
  color: #52c41a;
}

.status-badge.rejected {
  background: #fff2f0;
  color: #ff4d4f;
}

.feedback-item-body {
  display: flex;
  gap: 16px;
  color: #999;
  font-size: 13px;
}

.feedback-reply {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px dashed #eee;
}

.reply-label {
  font-size: 12px;
  color: #999;
  margin-bottom: 4px;
}

.reply-content {
  padding: 8px 12px;
  background: #f6ffed;
  border-radius: 4px;
  color: #52c41a;
  font-size: 13px;
}

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
  max-width: 500px;
  max-height: 80vh;
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
  max-height: 400px;
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

.detail-value.status.pending {
  color: #d48806;
}

.detail-value.status.processing {
  color: #1890ff;
}

.detail-value.status.resolved {
  color: #52c41a;
}

.detail-value.status.rejected {
  color: #ff4d4f;
}

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
  justify-content: flex-end;
}

.btn {
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 14px;
}

.btn-secondary {
  background: #f5f5f5;
  color: #666;
}

.btn-secondary:hover {
  background: #eee;
}
</style>
