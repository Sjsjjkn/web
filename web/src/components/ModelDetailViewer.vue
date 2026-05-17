<template>
  <div class="model-detail-viewer" ref="container">
    <!-- 预览图模式 -->
    <div v-if="showPreviewImage" class="preview-image-view">
      <img :src="previewUrl" :alt="work.title" class="detail-preview-img" />
      <!-- 如果是模型文件，提供切换到 3D 的按钮 -->
      <button
        v-if="isModelFile"
        class="switch-btn switch-to-3d"
        @click="showPreviewImage = false"
      >
        <i class="el-icon-cpu"></i> 切换至 3D 预览
      </button>
    </div>

    <!-- 3D 模型渲染视图 -->
    <div v-else-if="isModelFile" ref="canvasWrap" class="canvas-view">
      <div class="viewer-loading" v-if="loading">
        <div class="loading-spinner">
          <i class="el-icon-loading"></i>
          <span>正在加载 3D 模型...</span>
        </div>
      </div>
      <div class="viewer-controls" v-if="!loading">
        <button class="ctrl-btn" @click="resetCamera" title="重置视角">
          <i class="el-icon-refresh"></i>
        </button>
        <button class="ctrl-btn" @click="toggleAutoRotate" title="自动旋转">
          <i :class="autoRotate ? 'el-icon-video-pause' : 'el-icon-video-play'"></i>
        </button>
        <button class="ctrl-btn" @click="toggleWireframe" title="线框模式">
          <i class="el-icon-menu"></i>
        </button>
        <button
          v-if="work.previewImage"
          class="ctrl-btn"
          @click="showPreviewImage = true"
          title="查看预览图"
        >
          <i class="el-icon-picture"></i>
        </button>
      </div>
      <div class="viewer-hint" v-if="!loading">
        <span>🖱 鼠标左键旋转 | 滚轮缩放 | 右键平移</span>
      </div>
    </div>

    <!-- 未知类型 -->
    <div v-else class="unsupported-view">
      <span class="unsupported-icon">{{ fileIcon }}</span>
      <span class="unsupported-text">该文件类型暂无预览</span>
      <span class="unsupported-ext">{{ fileExt }}</span>
    </div>
  </div>
</template>

<script>
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js'
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader.js'
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js'
import { FBXLoader } from 'three/examples/jsm/loaders/FBXLoader.js'
import { getPreviewImageUrl, getFileDownloadUrl, isModelFile } from '../utils/fileUtils'

export default {
  name: 'ModelDetailViewer',

  props: {
    work: {
      type: Object,
      required: true
    },
    fileIcon: {
      type: String,
      default: '📄'
    },
    fileExt: {
      type: String,
      default: ''
    }
  },

  data() {
    return {
      previewUrl: null,
      isModelFile: false,
      showPreviewImage: false,
      loading: false,
      scene: null,
      camera: null,
      renderer: null,
      controls: null,
      animationId: null,
      modelGroup: null,
      autoRotate: true,
      wireframe: false,
      destroyed: false
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
    this.init()
  },

  beforeDestroy() {
    this.destroyed = true
    this.stopRenderer()
  },

  methods: {
    init() {
      this.stopRenderer()

      // 获取预览图
      this.previewUrl = getPreviewImageUrl(this.work)

      // 判断是否为 3D 模型
      const fileName = this.work.fileName || this.work.filePath || ''
      this.isModelFile = isModelFile(fileName)

      // 有预览图则默认显示预览图
      this.showPreviewImage = !!this.work.previewImage

      // 如果是 3D 模型但没有预览图，直接启动 3D 渲染
      if (this.isModelFile && !this.work.previewImage) {
        this.showPreviewImage = false
        this.$nextTick(() => {
          this.startRenderer()
        })
      }
    },

    async startRenderer() {
      if (this.renderer || this.destroyed || !this.isModelFile) return

      this.loading = true

      try {
        const canvasWrap = this.$refs.canvasWrap
        if (!canvasWrap || this.destroyed) return

        const width = canvasWrap.clientWidth || 800
        const height = canvasWrap.clientHeight || 500

        // 创建场景
        this.scene = new THREE.Scene()
        this.scene.background = new THREE.Color(0x1a1a2e)

        // 相机
        this.camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 100)
        this.camera.position.set(5, 3, 7)
        this.camera.lookAt(0, 0, 0)

        // 渲染器
        this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false })
        this.renderer.setSize(width, height)
        this.renderer.setPixelRatio(window.devicePixelRatio)
        this.renderer.outputColorSpace = THREE.SRGBColorSpace
        this.renderer.shadowMap.enabled = true
        canvasWrap.appendChild(this.renderer.domElement)

        // OrbitControls
        this.controls = new OrbitControls(this.camera, this.renderer.domElement)
        this.controls.enableDamping = true
        this.controls.dampingFactor = 0.08
        this.controls.autoRotate = this.autoRotate
        this.controls.autoRotateSpeed = 1.0
        this.controls.minDistance = 1
        this.controls.maxDistance = 20

        // 光照系统
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.4)
        this.scene.add(ambientLight)

        const mainLight = new THREE.DirectionalLight(0xffffff, 1.0)
        mainLight.position.set(5, 10, 7)
        mainLight.castShadow = true
        mainLight.shadow.mapSize.width = 1024
        mainLight.shadow.mapSize.height = 1024
        this.scene.add(mainLight)

        const fillLight = new THREE.DirectionalLight(0xaaccff, 0.5)
        fillLight.position.set(-3, 2, -4)
        this.scene.add(fillLight)

        const rimLight = new THREE.DirectionalLight(0xffccaa, 0.3)
        rimLight.position.set(0, 1, 5)
        this.scene.add(rimLight)

        // 地面
        const groundGeo = new THREE.PlaneGeometry(10, 10)
        const groundMat = new THREE.ShadowMaterial({ opacity: 0.3 })
        const ground = new THREE.Mesh(groundGeo, groundMat)
        ground.rotation.x = -Math.PI / 2
        ground.position.y = -1.5
        ground.receiveShadow = true
        this.scene.add(ground)

        // 网格辅助线
        const gridHelper = new THREE.GridHelper(8, 40, 0x444444, 0x2a2a2a)
        gridHelper.position.y = -1.5
        this.scene.add(gridHelper)

        // 加载模型
        const downloadUrl = getFileDownloadUrl(this.work)
        if (downloadUrl) {
          await this.loadModel(downloadUrl, this.work.fileName || '')
        }

        this.loading = false

        // 开始动画循环
        this.animate()
      } catch (err) {
        console.error('[ModelDetailViewer] 渲染失败:', err)
        this.loading = false
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
          return
        }

        if (!object || this.destroyed) return

        // 计算包围盒
        const box = new THREE.Box3().setFromObject(object)
        const center = box.getCenter(new THREE.Vector3())
        const size = box.getSize(new THREE.Vector3())
        const maxDim = Math.max(size.x, size.y, size.z)

        // 缩放以适应视图
        const scale = 4 / maxDim
        object.scale.set(scale, scale, scale)
        object.position.set(-center.x * scale, -center.y * scale, -center.z * scale)

        // 应用阴影
        object.traverse((child) => {
          if (child.isMesh) {
            child.castShadow = true
            child.receiveShadow = true
          }
        })

        this.modelGroup = object
        this.scene.add(object)

        // 调整相机距离
        if (this.controls) {
          this.controls.target.copy(new THREE.Vector3(0, 0, 0))
          this.camera.position.set(maxDim * 0.8, maxDim * 0.5, maxDim * 1.2)
          this.controls.update()
        }
      } catch (err) {
        console.error('[ModelDetailViewer] 模型加载失败:', err)
      }
    },

    loadGLTF(url) {
      return new Promise((resolve, reject) => {
        const loader = new GLTFLoader()
        loader.load(url, (gltf) => resolve(gltf.scene), undefined, reject)
      })
    },

    loadOBJ(url) {
      return new Promise((resolve, reject) => {
        const loader = new OBJLoader()
        loader.load(url, resolve, undefined, reject)
      })
    },

    loadSTL(url) {
      return new Promise((resolve, reject) => {
        const loader = new STLLoader()
        loader.load(
          url,
          (geometry) => {
            const material = new THREE.MeshStandardMaterial({
              color: 0x00aaff,
              roughness: 0.4,
              metalness: 0.1
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
        loader.load(url, resolve, undefined, reject)
      })
    },

    animate() {
      if (this.destroyed || !this.renderer) return

      this.animationId = requestAnimationFrame(this.animate)

      if (this.controls) {
        this.controls.update()
      }

      this.renderer.render(this.scene, this.camera)
    },

    resetCamera() {
      if (!this.controls || !this.camera) return
      this.controls.target.set(0, 0, 0)
      this.camera.position.set(5, 3, 7)
      this.controls.update()
    },

    toggleAutoRotate() {
      this.autoRotate = !this.autoRotate
      if (this.controls) {
        this.controls.autoRotate = this.autoRotate
      }
    },

    toggleWireframe() {
      this.wireframe = !this.wireframe
      if (this.modelGroup) {
        this.modelGroup.traverse((child) => {
          if (child.isMesh && child.material) {
            if (Array.isArray(child.material)) {
              child.material.forEach((m) => {
                m.wireframe = this.wireframe
              })
            } else {
              child.material.wireframe = this.wireframe
            }
          }
        })
      }
    },

    stopRenderer() {
      if (this.animationId) {
        cancelAnimationFrame(this.animationId)
        this.animationId = null
      }
      if (this.renderer) {
        this.renderer.dispose()
        this.renderer = null
      }
      if (this.scene) {
        this.disposeScene()
        this.scene = null
      }
      if (this.controls) {
        this.controls.dispose()
        this.controls = null
      }
      this.camera = null
      this.modelGroup = null
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
.model-detail-viewer {
  width: 100%;
  min-height: 400px;
  border-radius: 8px;
  overflow: hidden;
  background: #1a1a2e;
  position: relative;
}

.preview-image-view {
  width: 100%;
  min-height: 400px;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #0f0f1e;
}

.detail-preview-img {
  max-width: 100%;
  max-height: 500px;
  object-fit: contain;
  display: block;
}

.switch-btn {
  position: absolute;
  bottom: 16px;
  right: 16px;
  padding: 8px 16px;
  background: rgba(0, 0, 0, 0.7);
  color: #fff;
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 6px;
  cursor: pointer;
  font-size: 13px;
  transition: all 0.2s;
  backdrop-filter: blur(4px);
}

.switch-btn:hover {
  background: rgba(0, 0, 0, 0.9);
  border-color: rgba(255, 255, 255, 0.4);
}

.canvas-view {
  width: 100%;
  min-height: 400px;
  position: relative;
}

.canvas-view canvas {
  display: block;
}

.viewer-loading {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  z-index: 10;
}

.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  color: #fff;
  font-size: 14px;
}

.loading-spinner i {
  font-size: 32px;
}

.viewer-controls {
  position: absolute;
  top: 12px;
  right: 12px;
  display: flex;
  gap: 6px;
  z-index: 10;
}

.ctrl-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.6);
  color: #fff;
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s;
  backdrop-filter: blur(4px);
}

.ctrl-btn:hover {
  background: rgba(0, 0, 0, 0.85);
  border-color: rgba(255, 255, 255, 0.3);
}

.viewer-hint {
  position: absolute;
  bottom: 12px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 10;
  background: rgba(0, 0, 0, 0.6);
  color: rgba(255, 255, 255, 0.6);
  padding: 4px 12px;
  border-radius: 4px;
  font-size: 12px;
  backdrop-filter: blur(4px);
  pointer-events: none;
}

.unsupported-view {
  width: 100%;
  min-height: 300px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: rgba(255, 255, 255, 0.5);
}

.unsupported-icon {
  font-size: 48px;
}

.unsupported-text {
  font-size: 14px;
}

.unsupported-ext {
  font-size: 12px;
  opacity: 0.6;
}
</style>