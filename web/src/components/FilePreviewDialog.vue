<template>
  <el-dialog
    :title="dialogTitle"
    :visible.sync="innerVisible"
    width="92%"
    :fullscreen="true"
    :close-on-click-modal="false"
    class="file-preview-dialog"
    @close="handleClose"
  >
    <div class="preview-shell">
      <div class="preview-toolbar">
        <div class="meta">
          <div class="name">{{ safeFileName }}</div>
          <div class="hint">{{ typeHint }}</div>
        </div>
        <div class="actions">
          <el-button size="small" @click="download">
            <i class="el-icon-download"></i> 下载
          </el-button>
          <el-button size="small" type="primary" @click="innerVisible = false">
            <i class="el-icon-close"></i> 关闭
          </el-button>
        </div>
      </div>

      <div v-if="loading" class="loading">
        <i class="el-icon-loading"></i>
        <span>正在加载预览...</span>
      </div>

      <div v-else class="preview-stage">
        <!-- 图片预览 -->
        <div v-if="fileType === 'image'" class="image">
          <img :src="fileUrl" alt="预览图片" class="img" />
        </div>

        <!-- 视频预览 -->
        <div v-else-if="fileType === 'video'" class="video">
          <video :src="fileUrl" controls class="player">您的浏览器不支持视频播放</video>
        </div>

        <!-- PDF 预览 -->
        <div v-else-if="fileType === 'pdf'" class="pdf">
          <iframe :src="fileUrl" class="pdf-frame" title="PDF预览"></iframe>
        </div>

        <!-- 文本预览 -->
        <div v-else-if="fileType === 'text'" class="text">
          <pre class="text-pre">{{ textContent || '（空文件）' }}</pre>
        </div>

        <!-- 3D 模型预览（当前仅对 three ObjectLoader 的 json 最友好） -->
        <div v-else-if="fileType === 'model'" class="model">
          <ModelPreview :modelUrl="fileUrl" />
        </div>

        <!-- 其他类型 -->
        <div v-else class="other">
          <el-empty description="该文件类型暂不支持在线预览，请下载后查看"></el-empty>
        </div>
      </div>
    </div>
  </el-dialog>
</template>

<script>
import ModelPreview from './ModelPreview'
import http from '../utils/http'

export default {
  name: 'FilePreviewDialog',
  components: { ModelPreview },
  props: {
    // v-model: visible
    value: { type: Boolean, required: true },
    // 后端保存的 filePath（如 Uploads 下的文件名）
    filePath: { type: String, default: '' },
    // 原始文件名（用于展示/下载名）
    fileName: { type: String, default: '' },
    title: { type: String, default: '文件预览' }
  },
  data() {
    return {
      innerVisible: false,
      loading: false,
      textContent: ''
    }
  },
  computed: {
    safeFileName() {
      return this.fileName || this.filePath || '未命名文件'
    },
    dialogTitle() {
      return this.title
    },
    fileUrl() {
      // 说明：这里使用下载接口作为预览源，统一走 /api 相对路径，由开发代理转发
      if (!this.filePath) return ''
      return `/api/File/download?fileName=${encodeURIComponent(this.filePath)}`
    },
    extension() {
      const name = (this.filePath || this.fileName || '').toLowerCase()
      const idx = name.lastIndexOf('.')
      return idx >= 0 ? name.slice(idx) : ''
    },
    fileType() {
      const ext = this.extension
      const image = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp']
      const video = ['.mp4', '.avi', '.mov', '.wmv', '.flv', '.mkv']
      const model = ['.json', '.obj', '.gltf', '.glb', '.fbx', '.dae']
      const text = ['.txt', '.md', '.log', '.json', '.csv']
      if (image.includes(ext)) return 'image'
      if (video.includes(ext)) return 'video'
      if (ext === '.pdf') return 'pdf'
      if (model.includes(ext)) return 'model'
      if (text.includes(ext)) return 'text'
      return 'other'
    },
    typeHint() {
      const map = {
        image: '图片预览（可右键另存）',
        video: '视频预览（支持倍速/全屏）',
        pdf: 'PDF 预览（可滚动/搜索）',
        text: '文本预览（大文件可能截断）',
        model: '3D 预览（支持拖拽旋转缩放）',
        other: '暂不支持在线预览'
      }
      return map[this.fileType] || ''
    }
  },
  watch: {
    value: {
      immediate: true,
      handler(v) {
        this.innerVisible = v
        if (v) this.preparePreview()
      }
    },
    innerVisible(v) {
      this.$emit('input', v)
    },
    filePath() {
      // 文件变化时，如果弹窗开着，重新准备预览
      if (this.innerVisible) this.preparePreview()
    }
  },
  methods: {
    async preparePreview() {
      this.textContent = ''
      if (!this.filePath) return

      // 文本类：拉取 blob 后转成字符串显示（避免 iframe 直接打开导致下载）
      if (this.fileType !== 'text') return

      this.loading = true
      try {
        const res = await http.get('/api/File/download', { params: { fileName: this.filePath }, responseType: 'blob' })
        const text = await res.data.text()
        // 简单截断，避免超大文本卡死页面
        const maxLen = 200000
        this.textContent = text.length > maxLen ? text.slice(0, maxLen) + '\n\n（内容过长，已截断，请下载查看完整文件）' : text
      } catch (e) {
        this.textContent = '加载失败，请下载后查看'
        console.error('加载文本预览失败:', e)
      } finally {
        this.loading = false
      }
    },
    async download() {
      if (!this.filePath) return
      try {
        const res = await http.get('/api/File/download', { params: { fileName: this.filePath }, responseType: 'blob' })
        const blob = res.data
        const url = window.URL.createObjectURL(blob)
        const link = document.createElement('a')
        link.href = url
        link.download = this.safeFileName
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        window.URL.revokeObjectURL(url)
      } catch (e) {
        this.$message.error('下载失败，请稍后重试')
        console.error('下载失败:', e)
      }
    },
    handleClose() {
      // 关闭时清理状态
      this.loading = false
      this.textContent = ''
    }
  }
}
</script>

<style scoped>
.preview-shell {
  height: 78vh;
  display: flex;
  flex-direction: column;
}

.preview-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 4px 16px;
  border-bottom: 1px solid rgba(0, 0, 0, 0.06);
}

.meta .name {
  font-weight: 600;
  color: #1d2129;
}

.meta .hint {
  margin-top: 4px;
  font-size: 12px;
  color: #86909c;
}

.loading {
  padding: 28px 0;
  display: flex;
  align-items: center;
  gap: 10px;
  color: #4e5969;
}

.preview-stage {
  flex: 1;
  padding: 16px 0 0;
  overflow: hidden;
}

.image, .video, .pdf, .text, .model, .other {
  height: 100%;
}

.img {
  max-width: 100%;
  max-height: 100%;
  display: block;
  margin: 0 auto;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08);
}

.player {
  width: 100%;
  height: 100%;
  border-radius: 12px;
  background: #000;
}

.pdf-frame {
  width: 100%;
  height: 100%;
  border: none;
  border-radius: 12px;
  background: #fff;
}

.text-pre {
  height: 100%;
  overflow: auto;
  margin: 0;
  padding: 14px 16px;
  border-radius: 12px;
  background: #0b1220;
  color: #e6edf3;
  line-height: 1.6;
  font-size: 12px;
}
</style>

