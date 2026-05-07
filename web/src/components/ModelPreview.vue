<template>
  <div class="model-preview">
    <div ref="container" class="model-container"></div>
    <div class="controls">
      <el-button type="primary" size="small" @click="resetCamera">重置视角</el-button>
      <el-button type="info" size="small" @click="toggleWireframe">线框模式</el-button>
      <el-button type="info" size="small" @click="toggleAnimation">动画</el-button>
    </div>
  </div>
</template>

<script>
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'

export default {
  name: 'ModelPreview',
  props: {
    modelUrl: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      scene: null,
      camera: null,
      renderer: null,
      controls: null,
      model: null,
      isWireframe: false,
      isAnimating: false
    }
  },
  watch: {
    // 监听modelUrl变化，重新加载模型
    modelUrl: {
      handler(newUrl) {
        if (newUrl) {
          // 移除旧模型
          if (this.model) {
            this.scene.remove(this.model)
            // 释放几何体和材质资源
            if (this.model.geometry) {
              this.model.geometry.dispose()
            }
            if (this.model.material) {
              if (Array.isArray(this.model.material)) {
                this.model.material.forEach(material => material.dispose())
              } else {
                this.model.material.dispose()
              }
            }
            this.model = null
          }
          // 加载新模型
          this.loadModel()
        }
      },
      immediate: false
    }
  },
  mounted() {
    this.initThree()
    this.loadModel()
    this.animate()
  },
  beforeDestroy() {
    if (this.animationId) {
      cancelAnimationFrame(this.animationId)
    }
    if (this.renderer) {
      this.renderer.dispose()
    }
  },
  methods: {
    // 初始化Three.js场景
    initThree() {
      // 创建场景
      this.scene = new THREE.Scene()
      this.scene.background = new THREE.Color(0xf0f0f0)
      
      // 创建相机
      this.camera = new THREE.PerspectiveCamera(
        75,
        this.$refs.container.clientWidth / this.$refs.container.clientHeight,
        0.1,
        1000
      )
      this.camera.position.z = 5
      
      // 创建渲染器
      this.renderer = new THREE.WebGLRenderer({ antialias: true })
      this.renderer.setSize(this.$refs.container.clientWidth, this.$refs.container.clientHeight)
      this.$refs.container.appendChild(this.renderer.domElement)
      
      // 添加轨道控制器
      this.controls = new OrbitControls(this.camera, this.renderer.domElement)
      this.controls.enableDamping = true
      this.controls.dampingFactor = 0.05
      
      // 添加光源
      const ambientLight = new THREE.AmbientLight(0xffffff, 0.5)
      this.scene.add(ambientLight)
      
      const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8)
      directionalLight.position.set(1, 1, 1)
      this.scene.add(directionalLight)
      
      // 添加网格辅助线
      const gridHelper = new THREE.GridHelper(10, 10)
      this.scene.add(gridHelper)
      
      // 监听窗口大小变化
      window.addEventListener('resize', this.onWindowResize)
    },
    
    // 加载模型
    loadModel() {
      // 检查是否是Data URL
      if (this.modelUrl.startsWith('data:')) {
        // 从Data URL中解析模型数据
        try {
          const jsonData = decodeURIComponent(this.modelUrl.split(',')[1])
          const modelData = JSON.parse(jsonData)
          this.createModelFromData(modelData)
        } catch (error) {
          console.error('Error parsing model data:', error)
          // 创建默认立方体作为 fallback
          this.createDefaultModel()
        }
      } else {
        // 使用ObjectLoader加载远程模型
        const loader = new THREE.ObjectLoader()
        
        loader.load(
          this.modelUrl,
          (object) => {
            this.model = object
            this.scene.add(object)
            this.adjustCamera()
          },
          (xhr) => {
            console.log((xhr.loaded / xhr.total * 100) + '% loaded')
          },
          (error) => {
            console.error('An error happened while loading the model:', error)
            this.createDefaultModel()
          }
        )
      }
    },
    
    // 从模型数据创建模型
    createModelFromData(modelData) {
      // 基于模型类型创建几何体
      let geometry
      let material
      
      switch (modelData.object.type) {
        case 'Mesh':
          // 根据几何体类型创建几何体
          const geometryData = modelData.geometries[0]
          switch (geometryData.type) {
            case 'BoxGeometry':
              geometry = new THREE.BoxGeometry(
                geometryData.width,
                geometryData.height,
                geometryData.depth
              )
              break
            case 'SphereGeometry':
              geometry = new THREE.SphereGeometry(
                geometryData.radius,
                geometryData.widthSegments,
                geometryData.heightSegments
              )
              break
            case 'CylinderGeometry':
              geometry = new THREE.CylinderGeometry(
                geometryData.radiusTop,
                geometryData.radiusBottom,
                geometryData.height,
                geometryData.radialSegments
              )
              break
            default:
              geometry = new THREE.BoxGeometry(2, 2, 2)
          }
          
          // 创建材质
          const materialData = modelData.materials[0]
          material = new THREE.MeshPhongMaterial({
            color: materialData.color,
            emissive: materialData.emissive,
            specular: materialData.specular,
            shininess: materialData.shininess,
            opacity: materialData.opacity,
            transparent: materialData.transparent
          })
          
          // 创建网格
          this.model = new THREE.Mesh(geometry, material)
          
          // 设置位置、旋转和缩放
          if (modelData.object.position) {
            this.model.position.set(
              modelData.object.position[0],
              modelData.object.position[1],
              modelData.object.position[2]
            )
          }
          
          if (modelData.object.rotation) {
            this.model.rotation.set(
              modelData.object.rotation[0],
              modelData.object.rotation[1],
              modelData.object.rotation[2]
            )
          }
          
          if (modelData.object.scale) {
            this.model.scale.set(
              modelData.object.scale[0],
              modelData.object.scale[1],
              modelData.object.scale[2]
            )
          }
          
          this.scene.add(this.model)
          this.adjustCamera()
          break
        default:
          this.createDefaultModel()
      }
    },
    
    // 创建默认模型
    createDefaultModel() {
      const geometry = new THREE.BoxGeometry(2, 2, 2)
      const material = new THREE.MeshPhongMaterial({ color: 0xffffff })
      this.model = new THREE.Mesh(geometry, material)
      this.scene.add(this.model)
      this.adjustCamera()
    },
    
    // 调整相机位置
    adjustCamera() {
      if (this.model) {
        const box = new THREE.Box3().setFromObject(this.model)
        const size = box.getSize(new THREE.Vector3())
        const center = box.getCenter(new THREE.Vector3())
        
        // 计算合适的相机距离
        const maxDimension = Math.max(size.x, size.y, size.z)
        const fov = this.camera.fov * (Math.PI / 180)
        const distance = maxDimension / (2 * Math.tan(fov / 2)) * 1.5
        
        // 设置相机位置
        this.camera.position.set(center.x, center.y, center.z + distance)
        this.controls.target.copy(center)
        this.controls.update()
      }
    },
    
    // 动画循环
    animate() {
      this.animationId = requestAnimationFrame(this.animate)
      
      if (this.controls) {
        this.controls.update()
      }
      
      if (this.isAnimating && this.model) {
        this.model.rotation.y += 0.01
      }
      
      if (this.renderer && this.scene && this.camera) {
        this.renderer.render(this.scene, this.camera)
      }
    },
    
    // 窗口大小变化处理
    onWindowResize() {
      if (this.camera && this.renderer && this.$refs.container) {
        this.camera.aspect = this.$refs.container.clientWidth / this.$refs.container.clientHeight
        this.camera.updateProjectionMatrix()
        this.renderer.setSize(this.$refs.container.clientWidth, this.$refs.container.clientHeight)
      }
    },
    
    // 重置相机视角
    resetCamera() {
      if (this.model) {
        const box = new THREE.Box3().setFromObject(this.model)
        const size = box.getSize(new THREE.Vector3())
        const center = box.getCenter(new THREE.Vector3())
        
        const maxDimension = Math.max(size.x, size.y, size.z)
        const fov = this.camera.fov * (Math.PI / 180)
        const distance = maxDimension / (2 * Math.tan(fov / 2)) * 1.5
        
        this.camera.position.set(center.x, center.y, center.z + distance)
        this.controls.target.copy(center)
        this.controls.update()
      }
    },
    
    // 切换线框模式
    toggleWireframe() {
      this.isWireframe = !this.isWireframe
      if (this.model) {
        this.model.traverse((child) => {
          if (child.isMesh) {
            child.material.wireframe = this.isWireframe
          }
        })
      }
    },
    
    // 切换动画
    toggleAnimation() {
      this.isAnimating = !this.isAnimating
    }
  }
}
</script>

<style scoped>
.model-preview {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.model-container {
  flex: 1;
  width: 100%;
  height: calc(100% - 50px);
  background-color: #f0f0f0;
}

.controls {
  height: 50px;
  display: flex;
  align-items: center;
  padding: 0 20px;
  background-color: #fff;
  border-top: 1px solid #e4e7ed;
}

.controls button {
  margin-right: 10px;
}
</style>