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
  padding: 32px;
}

.page-header {
  margin-bottom: 28px;
  animation: fadeInUp 0.6s var(--ease-out-expo) both;
}

.page-title {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-main);
  margin: 0 0 6px 0;
  letter-spacing: -0.3px;
}

.page-subtitle {
  font-size: 14px;
  color: var(--text-light);
  margin: 0;
}

.feedback-form-card,
.feedback-list-card {
  background: var(--card-bg);
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  margin-bottom: 24px;
  overflow: hidden;
}

.feedback-form-card {
  animation: fadeInUp 0.6s var(--ease-out-expo) 0.1s both;
}

.feedback-list-card {
  animation: fadeInUp 0.6s var(--ease-out-expo) 0.2s both;
}

.card-header {
  padding: 18px 24px;
  border-bottom: 1px solid var(--border-color);
  background: var(--app-bg);
}

.card-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-main);
}

.card-body {
  padding: 24px;
}

.custom-textarea {
  width: 100%;
  min-height: 130px;
  padding: 14px;
  border: 2px solid var(--border-color);
  border-radius: var(--radius-md);
  resize: vertical;
  font-size: 14px;
  line-height: 1.6;
  box-sizing: border-box;
  color: var(--text-main);
  background: var(--app-bg);
  transition: border-color var(--duration-fast), box-shadow var(--duration-fast);
  font-family: inherit;
}

.custom-textarea:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 4px var(--primary-bg);
}

.form-actions {
  margin-bottom: 0;
}

.form-actions .el-button {
  border-radius: var(--radius-full);
  padding: 12px 32px;
  font-weight: 600;
}

.empty-state {
  text-align: center;
  padding: 48px 0;
  color: var(--text-light);
}

.empty-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 16px;
  color: var(--text-placeholder);
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
  padding: 18px 20px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all var(--duration-normal) var(--ease-out-expo);
  background: var(--app-bg);
}

.feedback-item:hover {
  border-color: var(--primary);
  box-shadow: var(--shadow-sm);
  transform: translateY(-1px);
  background: var(--card-bg);
}

.feedback-item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.feedback-title {
  font-weight: 600;
  color: var(--text-main);
  font-size: 15px;
}

.status-badge {
  font-size: 12px;
  padding: 3px 12px;
  border-radius: var(--radius-full);
  font-weight: 600;
}

.status-badge.pending {
  background: #FFF8EC;
  color: #B8700A;
}

.status-badge.processing {
  background: #E8F4FD;
  color: #1E7EC8;
}

.status-badge.resolved {
  background: #EAF7EA;
  color: #3A8C3D;
}

.status-badge.rejected {
  background: #FDECEC;
  color: #CC3D3D;
}

.feedback-item-body {
  display: flex;
  gap: 16px;
  color: var(--text-light);
  font-size: 13px;
}

.feedback-reply {
  margin-top: 14px;
  padding-top: 14px;
  border-top: 1px solid var(--border-color);
}

.reply-label {
  font-size: 12px;
  font-weight: 600;
  color: var(--text-light);
  margin-bottom: 6px;
}

.reply-content {
  padding: 12px 16px;
  background: #EAF7EA;
  border-radius: var(--radius-sm);
  color: var(--primary-dark);
  font-size: 13px;
  line-height: 1.6;
}

.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  animation: fadeIn 0.2s ease;
}

.modal-content {
  background: var(--card-bg);
  border-radius: var(--radius-lg);
  width: 90%;
  max-width: 520px;
  max-height: 80vh;
  overflow: hidden;
  box-shadow: var(--shadow-card-hover);
  animation: scaleIn 0.25s var(--ease-out-expo);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 18px 24px;
  border-bottom: 1px solid var(--border-color);
}

.modal-header h3 {
  margin: 0;
  font-size: 17px;
  font-weight: 700;
  color: var(--text-main);
}

.close-btn {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-light);
  padding: 6px;
  border-radius: var(--radius-sm);
  transition: all var(--duration-fast);
}

.close-btn:hover {
  color: var(--text-main);
  background: var(--bg-hover);
}

.close-btn svg {
  width: 20px;
  height: 20px;
}

.modal-body {
  padding: 24px;
  max-height: 420px;
  overflow-y: auto;
}

.detail-row {
  margin-bottom: 14px;
  display: flex;
  align-items: flex-start;
}

.detail-row.full-width {
  flex-direction: column;
}

.detail-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-light);
  min-width: 80px;
  flex-shrink: 0;
}

.detail-value {
  font-size: 14px;
  color: var(--text-main);
}

.detail-content {
  margin-top: 6px;
  padding: 14px;
  background: var(--app-bg);
  border-radius: var(--radius-md);
  font-size: 14px;
  line-height: 1.7;
  color: var(--text-regular);
}

.detail-content.reply {
  background: #EAF7EA;
  color: var(--primary-dark);
}

.modal-footer {
  padding: 16px 24px;
  border-top: 1px solid var(--border-color);
  display: flex;
  justify-content: flex-end;
}

.btn {
  padding: 8px 20px;
  border-radius: var(--radius-full);
  border: none;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all var(--duration-fast);
}

.btn-secondary {
  background: var(--bg-hover);
  color: var(--text-regular);
}

.btn-secondary:hover {
  background: var(--border-color);
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes scaleIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}
</style>
