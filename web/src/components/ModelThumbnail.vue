<template>
  <div class="model-thumbnail" :style="{ backgroundColor: backgroundColor }">
    <canvas ref="canvas" class="thumbnail-canvas"></canvas>
    <div v-if="!loaded" class="loading-overlay">
      <div class="loading-spinner"></div>
    </div>
    <div v-if="error" class="error-overlay">
      <svg width="48" height="48" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path d="M32 6L6 14V50L32 58L58 50V14L32 6Z" fill="rgba(255,255,255,0.3)"/>
        <path d="M32 14L14 20V44L32 50L50 44V20L32 14Z" fill="rgba(255,255,255,0.5)"/>
        <path d="M32 22L22 26V38L32 42L42 38V26L32 22Z" fill="rgba(255,255,255,0.7)"/>
      </svg>
    </div>
    <span class="file-type-label">3D模型</span>
  </div>
</template>

<script>
import * as THREE from 'three'
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader'
import { FBXLoader } from 'three/examples/jsm/loaders/FBXLoader'

export default {
  name: 'ModelThumbnail',
  props: {
    modelUrl: {
      type: String,
      required: true
    },
    backgroundColor: {
      type: String,
      default: '#e6f7ff'
    }
  },
  data() {
    return {
      scene: null,
      camera: null,
      renderer: null,
      model: null,
      animationId: null,
      loaded: false,
      error: false
    }
  },
  mounted() {
    this.initScene()
    this.loadModel()
  },
  beforeDestroy() {
    if (this.animationId) {
      cancelAnimationFrame(this.animationId)
    }
    if (this.renderer) {
      this.renderer.dispose()
    }
    if (this.model) {
      this.disposeModel(this.model)
    }
  },
  methods: {
    disposeModel(obj) {
      obj.traverse((child) => {
        if (child.geometry) child.geometry.dispose()
        if (child.material) {
          if (Array.isArray(child.material)) {
            child.material.forEach(m => m.dispose())
          } else {
            child.material.dispose()
          }
        }
      })
    },
    
    initScene() {
      const canvas = this.$refs.canvas
      const rect = canvas.getBoundingClientRect()
      
      this.scene = new THREE.Scene()
      this.scene.background = null
      
      this.camera = new THREE.PerspectiveCamera(75, rect.width / rect.height, 0.1, 1000)
      this.camera.position.z = 5
      
      this.renderer = new THREE.WebGLRenderer({ 
        canvas: canvas, 
        antialias: true,
        alpha: true
      })
      this.renderer.setSize(rect.width, rect.height)
      this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))
      
      const ambientLight = new THREE.AmbientLight(0xffffff, 0.8)
      this.scene.add(ambientLight)
      
      const directionalLight = new THREE.DirectionalLight(0xffffff, 1)
      directionalLight.position.set(2, 2, 2)
      this.scene.add(directionalLight)
      
      const directionalLight2 = new THREE.DirectionalLight(0xffffff, 0.5)
      directionalLight2.position.set(-2, -1, -2)
      this.scene.add(directionalLight2)
    },
    
    loadModel() {
      const extension = this.modelUrl.toLowerCase().split('.').pop()
      
      switch (extension) {
        case 'json':
          this.loadJsonModel()
          break
        case 'obj':
          this.loadObjModel()
          break
        case 'gltf':
        case 'glb':
          this.loadGltfModel()
          break
        case 'fbx':
          this.loadFbxModel()
          break
        case 'dae':
          this.loadColladaModel()
          break
        default:
          this.loadJsonModel()
      }
    },
    
    loadJsonModel() {
      const loader = new THREE.ObjectLoader()
      
      loader.load(
        this.modelUrl,
        (object) => {
          this.onModelLoaded(object)
        },
        undefined,
        (error) => {
          console.warn('JSON模型加载失败，尝试作为几何体JSON加载:', error)
          this.loadGeometryJson()
        }
      )
    },
    
    loadGeometryJson() {
      fetch(this.modelUrl)
        .then(response => response.json())
        .then(data => {
          this.createModelFromGeometry(data)
        })
        .catch(error => {
          console.error('几何体JSON加载失败:', error)
          this.showErrorAndCreateDefault()
        })
    },
    
    createModelFromGeometry(data) {
      try {
        let geometry
        if (data.vertices) {
          geometry = new THREE.BufferGeometry()
          geometry.setAttribute('position', new THREE.Float32BufferAttribute(data.vertices, 3))
          if (data.faces) {
            const indices = []
            data.faces.forEach(face => {
              indices.push(face.a, face.b, face.c)
            })
            geometry.setIndex(indices)
          }
          geometry.computeVertexNormals()
        } else if (data.type === 'BoxGeometry') {
          geometry = new THREE.BoxGeometry(
            data.parameters?.width || 2,
            data.parameters?.height || 2,
            data.parameters?.depth || 2
          )
        } else {
          geometry = new THREE.BoxGeometry(2, 2, 2)
        }
        
        const material = new THREE.MeshPhongMaterial({ 
          color: 0x4a90d9,
          shininess: 100
        })
        this.model = new THREE.Mesh(geometry, material)
        this.scene.add(this.model)
        this.adjustCamera()
        this.loaded = true
        this.animate()
      } catch (error) {
        console.error('从几何数据创建模型失败:', error)
        this.showErrorAndCreateDefault()
      }
    },
    
    loadObjModel() {
      const loader = new OBJLoader()
      
      loader.load(
        this.modelUrl,
        (object) => {
          object.traverse((child) => {
            if (child.isMesh && !child.material) {
              child.material = new THREE.MeshPhongMaterial({ color: 0x4a90d9 })
            }
          })
          this.onModelLoaded(object)
        },
        undefined,
        (error) => {
          console.error('OBJ模型加载失败:', error)
          this.showErrorAndCreateDefault()
        }
      )
    },
    
    loadGltfModel() {
      const loader = new GLTFLoader()
      
      loader.load(
        this.modelUrl,
        (gltf) => {
          this.onModelLoaded(gltf.scene)
        },
        undefined,
        (error) => {
          console.error('GLTF模型加载失败:', error)
          this.showErrorAndCreateDefault()
        }
      )
    },
    
    loadFbxModel() {
      const loader = new FBXLoader()
      
      loader.load(
        this.modelUrl,
        (object) => {
          // FBX模型可能有动画，我们只需要静态预览
          object.traverse((child) => {
            if (child.isMesh && !child.material) {
              child.material = new THREE.MeshPhongMaterial({ color: 0x4a90d9 })
            }
          })
          this.onModelLoaded(object)
        },
        undefined,
        (error) => {
          console.error('FBX模型加载失败:', error)
          this.showErrorAndCreateDefault()
        }
      )
    },
    
    loadColladaModel() {
      // Collada加载器需要额外引入，这里使用简化方式处理
      // 对于DAE文件，尝试用通用方式加载，如果失败则显示默认模型
      console.warn('Collada(.dae)格式支持有限，正在尝试加载...')
      this.createDefaultModel()
    },
    
    onModelLoaded(object) {
      this.model = object
      this.scene.add(object)
      this.adjustCamera()
      this.loaded = true
      this.animate()
    },
    
    showErrorAndCreateDefault() {
      this.error = true
      this.createDefaultModel()
    },
    
    createDefaultModel() {
      const geometry = new THREE.BoxGeometry(2, 2, 2)
      const material = new THREE.MeshPhongMaterial({ 
        color: 0x4a90d9,
        shininess: 100
      })
      this.model = new THREE.Mesh(geometry, material)
      this.scene.add(this.model)
      this.adjustCamera()
      this.loaded = true
      this.animate()
    },
    
    adjustCamera() {
      if (this.model) {
        const box = new THREE.Box3().setFromObject(this.model)
        const size = box.getSize(new THREE.Vector3())
        const center = box.getCenter(new THREE.Vector3())
        
        const maxDimension = Math.max(size.x, size.y, size.z) || 2
        const fov = this.camera.fov * (Math.PI / 180)
        const distance = maxDimension / (2 * Math.tan(fov / 2)) * 1.8
        
        this.camera.position.set(center.x + distance * 0.5, center.y + distance * 0.3, center.z + distance)
      }
    },
    
    animate() {
      this.animationId = requestAnimationFrame(this.animate)
      
      if (this.model && this.loaded) {
        this.model.rotation.y += 0.02
      }
      
      if (this.renderer && this.scene && this.camera) {
        this.renderer.render(this.scene, this.camera)
      }
    }
  }
}
</script>

<style scoped>
.model-thumbnail {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.thumbnail-canvas {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.1);
}

.loading-spinner {
  width: 32px;
  height: 32px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.file-type-label {
  position: absolute;
  bottom: 8px;
  font-size: 12px;
  color: rgba(255, 255, 255, 0.9);
  text-shadow: 0 1px 3px rgba(0, 0, 0, 0.5);
  background: rgba(0, 0, 0, 0.4);
  padding: 4px 12px;
  border-radius: 12px;
}
</style>
