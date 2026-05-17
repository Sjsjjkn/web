<template>
  <div class="model-card-cover" ref="container">
    <!-- 有预览图：直接显示图片 -->
    <img
      v-if="previewUrl"
      :src="previewUrl"
      :alt="work.title || '作品预览'"
      class="cover-img"
      @error="onImageError"
    />

    <!-- 3D 模型文件：显示实时渲染预览 -->
    <div v-else-if="isModel" ref="canvasWrap" class="cover-canvas-wrap">
      <div class="model-loading" v-if="loading">
        <i class="el-icon-loading"></i>
      </div>
    </div>

    <!-- 其他文件：渐变色占位符 -->
    <div v-else class="cover-placeholder" :style="{ background: gradient || defaultCoverGradient }">
      <span class="file-icon">{{ fileIcon || defaultFileIcon }}</span>
      <span class="file-ext">{{ fileExt || work?.category || '' }}</span>
    </div>

    <!-- 角标 -->
    <slot name="badge"></slot>

    <!-- 悬浮操作层 -->
    <div class="cover-overlay">
      <slot name="overlay"></slot>
    </div>
  </div>
</template>

<script>
import * as THREE from 'three'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js'
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader.js'
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js'
import { FBXLoader } from 'three/examples/jsm/loaders/FBXLoader.js'
import { getPreviewImageUrl, getFileDownloadUrl, isModelFile, MODEL_EXTENSIONS } from '../utils/fileUtils'

// 全局活跃的渲染器计数
let activeRendererCount = 0
const MAX_ACTIVE_RENDERERS = 3

export default {
  name: 'ModelCardCover',

  props: {
    work: {
      type: Object,
      required: true
    },
    // 文件图标文本（非模型时）
    fileIcon: {
      type: String,
      default: '📄'
    },
    // 文件扩展名（非模型时）
    fileExt: {
      type: String,
      default: ''
    },
    // 渐变色（非模型时）
    gradient: {
      type: String,
      default: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
    }
  },

  data() {
    return {
      previewUrl: null,
      isModel: false,
      loading: false,
      scene: null,
      camera: null,
      renderer: null,
      animationId: null,
      modelGroup: null,
      observer: null,
      hasObserver: false,
      destroyed: false
    }
  },

  computed: {
    defaultCoverGradient() {
      const categoryGradients = {
        '前端开发': 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        '后端开发': 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
        '人工智能': 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
        '设计': 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
        '视频动画': 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
        '其他': 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
      }
      const category = this.work?.category || ''
      return categoryGradients[category] || categoryGradients['其他']
    },
    defaultFileIcon() {
      const categoryIcons = {
        '前端开发': '🌐',
        '后端开发': '⚙️',
        '人工智能': '🤖',
        '设计': '🎨',
        '视频动画': '🎬',
        '其他': '📄'
      }
      const category = this.work?.category || ''
      return categoryIcons[category] || categoryIcons['其他']
    }
  },

  watch: {
    work: {
      immediate: true,
      handler() {
        this.init()
      }
    }
  },

  mounted() {
    this.setupObserver()
  },

  beforeDestroy() {
    this.destroyed = true
    this.stopRenderer()
    if (this.observer) {
      this.observer.disconnect()
    }
  },

  methods: {
    init() {
      // 优先使用预览图
      const previewFile = this.work.previewImage || this.work.preview
      if (previewFile) {
        this.previewUrl = `/api/File/download?fileName=${encodeURIComponent(previewFile)}`
        this.isModel = false
        return
      }

      // 获取文件路径（兼容多个可能的字段）
      const filePath = this.work.filePath || this.work.fileName || this.work.preview || this.work.url || ''
      if (!filePath) {
        this.isModel = false
        this.previewUrl = null
        return
      }

      // 检查是否为图片文件
      const ext = filePath.toLowerCase().substring(filePath.lastIndexOf('.'))
      const imageExts = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg']
      if (imageExts.includes(ext)) {
        this.isModel = false
        this.previewUrl = `/api/File/download?fileName=${encodeURIComponent(filePath)}`
        return
      }

      // 判断是否为 3D 模型
      if (isModelFile(filePath)) {
        this.isModel = true
        this.previewUrl = null
        return
      }

      // 其他文件显示占位符
      this.isModel = false
      this.previewUrl = null
    },

    onImageError() {
      // 预览图加载失败，回退到模型渲染或占位符
      this.previewUrl = null
      const fileName = this.work.fileName || this.work.filePath || ''
      if (isModelFile(fileName)) {
        this.isModel = true
      }
    },

    setupObserver() {
      if (typeof IntersectionObserver === 'undefined') {
        // 不支持 IntersectionObserver，直接开始渲染
        if (this.isModel) {
          this.startRenderer()
        }
        return
      }

      this.observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            if (this.isModel && !this.renderer && !this.destroyed) {
              this.startRenderer()
            }
          } else {
            if (this.renderer) {
              this.stopRenderer()
            }
          }
        })
      }, { rootMargin: '200px' })

      this.observer.observe(this.$refs.container)
      this.hasObserver = true
    },

    async startRenderer() {
      if (this.renderer || this.destroyed) return
      if (activeRendererCount >= MAX_ACTIVE_RENDERERS) return

      this.loading = true
      activeRendererCount++

      try {
        const canvasWrap = this.$refs.canvasWrap
        if (!canvasWrap || this.destroyed) {
          activeRendererCount--
          return
        }

        const width = canvasWrap.clientWidth || 200
        const height = canvasWrap.clientHeight || 200

        // 创建场景
        this.scene = new THREE.Scene()
        this.scene.background = new THREE.Color(0xf5f5f5)

        // 相机
        this.camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 100)
        this.camera.position.set(3, 2, 5)
        this.camera.lookAt(0, 0, 0)

        // 渲染器
        this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false })
        this.renderer.setSize(width, height)
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 1.5))
        this.renderer.outputColorSpace = THREE.SRGBColorSpace
        canvasWrap.appendChild(this.renderer.domElement)

        // 光照
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.6)
        this.scene.add(ambientLight)

        const directionalLight = new THREE.DirectionalLight(0xffffff, 1.0)
        directionalLight.position.set(5, 10, 7)
        this.scene.add(directionalLight)

        const fillLight = new THREE.DirectionalLight(0xffffff, 0.3)
        fillLight.position.set(-5, 0, -3)
        this.scene.add(fillLight)

        // 辅助地面
        const gridHelper = new THREE.GridHelper(4, 20, 0x444444, 0x222222)
        gridHelper.position.y = -1
        this.scene.add(gridHelper)

        // 加载模型
        const downloadUrl = getFileDownloadUrl(this.work)
        if (!downloadUrl) {
          this.loading = false
          return
        }

        await this.loadModel(downloadUrl, this.work.fileName || '')

        this.loading = false

        if (this.isModel) {
          this.animate()
        }
      } catch (err) {
        console.error('[ModelCardCover] 渲染失败:', err)
        this.loading = false
        activeRendererCount--
        this.cleanup()
      }
    },

    async loadModel(url, fileName) {
      const ext = fileName.substring(fileName.lastIndexOf('.')).toLowerCase()

      try {
        let object
        if (ext === '.glb' || ext === '.gltf') {
          object = await this.loadGLTF(url)
        } else if (ext === '.obj') {
          object = await this.loadOBJ(url)
        } else if (ext === '.stl') {
          object = await this.loadSTL(url)
        } else if (ext === '.fbx') {
          object = await this.loadFBX(url)
        } else if (ext === '.dae') {
          object = await this.loadGLTF(url)
        } else {
          // 不支持的格式，清理资源并显示占位符
          this.isModel = false
          this.disposeScene()
          this.cleanup()
          return
        }

        if (!object || this.destroyed) {
          this.isModel = false
          this.disposeScene()
          this.cleanup()
          return
        }

        // 计算包围盒，适配视图
        const box = new THREE.Box3().setFromObject(object)
        const center = box.getCenter(new THREE.Vector3())
        const size = box.getSize(new THREE.Vector3())
        const maxDim = Math.max(size.x, size.y, size.z)

        // 缩放模型使其适合视图
        const scale = 3 / maxDim
        object.scale.set(scale, scale, scale)

        // 居中
        object.position.set(-center.x * scale, -center.y * scale, -center.z * scale)

        this.modelGroup = object
        this.scene.add(object)
      } catch (err) {
        console.error('[ModelCardCover] 模型加载失败:', err)
        this.isModel = false
        this.disposeScene()
        this.cleanup()
      }
    },

    loadGLTF(url) {
      return new Promise((resolve, reject) => {
        const loader = new GLTFLoader()
        loader.load(
          url,
          (gltf) => resolve(gltf.scene),
          undefined,
          reject
        )
      })
    },

    loadOBJ(url) {
      return new Promise((resolve, reject) => {
        const loader = new OBJLoader()
        loader.load(
          url,
          resolve,
          undefined,
          reject
        )
      })
    },

    loadSTL(url) {
      return new Promise((resolve, reject) => {
        const loader = new STLLoader()
        loader.load(
          url,
          (geometry) => {
            const material = new THREE.MeshPhongMaterial({
              color: 0x00aaff,
              specular: 0x111111,
              shininess: 30
            })
            const mesh = new THREE.Mesh(geometry, material)
            resolve(mesh)
          },
          undefined,
          reject
        )
      })
    },

    loadFBX(url) {
      return new Promise((resolve, reject) => {
        const loader = new FBXLoader()
        loader.load(
          url,
          resolve,
          undefined,
          reject
        )
      })
    },

    animate() {
      if (this.destroyed || !this.isModel || !this.renderer || !this.scene || !this.camera) return

      this.animationId = requestAnimationFrame(this.animate)

      // 自动旋转
      if (this.modelGroup) {
        this.modelGroup.rotation.y += 0.005
      }

      this.renderer.render(this.scene, this.camera)
    },

    stopRenderer() {
      if (this.animationId) {
        cancelAnimationFrame(this.animationId)
        this.animationId = null
      }
      if (this.renderer) {
        activeRendererCount = Math.max(0, activeRendererCount - 1)
        this.renderer.dispose()
        this.renderer = null
      }
      if (this.scene) {
        this.disposeScene()
        this.scene = null
      }
      this.camera = null
      this.modelGroup = null
    },

    cleanup() {
      if (this.renderer) {
        activeRendererCount = Math.max(0, activeRendererCount - 1)
        this.renderer.dispose()
        this.renderer = null
      }
    },

    disposeScene() {
      if (!this.scene) return
      this.scene.traverse((child) => {
        if (child.geometry) {
          child.geometry.dispose()
        }
        if (child.material) {
          if (Array.isArray(child.material)) {
            child.material.forEach((m) => m.dispose())
          } else {
            child.material.dispose()
          }
        }
      })
    }
  }
}
</script>

<style scoped>
.model-card-cover {
  width: 100%;
  height: 100%;
  overflow: hidden;
  position: relative;
}

.cover-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.cover-canvas-wrap {
  width: 100%;
  height: 100%;
  position: relative;
}

.cover-canvas-wrap canvas {
  display: block;
}

.model-loading {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: #fff;
  font-size: 24px;
  z-index: 1;
}

.cover-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #fff;
  text-shadow: 0 1px 3px rgba(0, 0, 0, 0.5);
  gap: 4px;
}

.file-icon {
  font-size: 28px;
}

.file-ext {
  font-size: 12px;
  opacity: 0.8;
}
</style>