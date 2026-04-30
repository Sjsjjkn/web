<template>
  <div class="model-test">
    <el-card class="page-header">
      <h2>3D模型预览测试</h2>
      <p>使用Three.js预览3D模型文件</p>
    </el-card>
    
    <el-card class="model-section">
      <div class="model-input">
        <el-input
          v-model="modelUrl"
          placeholder="输入模型文件URL"
          style="width: 400px"
        ></el-input>
        <el-button type="primary" @click="loadModel" style="margin-left: 10px">加载模型</el-button>
      </div>
      
      <div class="model-preview-container">
        <ModelPreview v-if="modelUrl" :modelUrl="modelUrl" />
        <div v-else class="empty-preview">
          <el-empty description="请输入模型文件URL并点击加载模型"></el-empty>
        </div>
      </div>
    </el-card>
    
    <el-card class="test-models">
      <h3>测试模型</h3>
      <div class="model-list">
        <el-button @click="loadTestModel(1)">立方体模型</el-button>
        <el-button @click="loadTestModel(2)">球体模型</el-button>
        <el-button @click="loadTestModel(3)">圆柱体模型</el-button>
      </div>
    </el-card>
  </div>
</template>

<script>
import ModelPreview from '@/components/ModelPreview'

export default {
  name: 'ModelTest',
  components: {
    ModelPreview
  },
  data() {
    return {
      modelUrl: ''
    }
  },
  methods: {
    // 加载模型
    loadModel() {
      if (!this.modelUrl) {
        this.$message.error('请输入模型文件URL')
        return
      }
    },
    
    // 加载测试模型
    loadTestModel(modelType) {
      // 这里使用内置的测试模型
      // 实际项目中，你可以上传模型文件到服务器，然后使用服务器返回的URL
      
      // 生成一个简单的3D模型JSON（符合Three.js ObjectLoader格式）
      let modelData
      
      switch (modelType) {
        case 1: // 立方体
          modelData = {
            "metadata": {
              "type": "Object",
              "version": 4.5,
              "generator": "ModelGenerator"
            },
            "geometries": [
              {
                "type": "BoxGeometry",
                "width": 2,
                "height": 2,
                "depth": 2,
                "uuid": "geometry-1"
              }
            ],
            "materials": [
              {
                "type": "MeshPhongMaterial",
                "color": 16777215,
                "emissive": 0,
                "specular": 1118481,
                "shininess": 30,
                "opacity": 1,
                "transparent": false,
                "uuid": "material-1"
              }
            ],
            "object": {
              "type": "Mesh",
              "geometry": "geometry-1",
              "material": "material-1",
              "position": [0, 0, 0],
              "rotation": [0, 0, 0],
              "scale": [1, 1, 1],
              "uuid": "mesh-1"
            }
          }
          break
          
        case 2: // 球体
          modelData = {
            "metadata": {
              "type": "Object",
              "version": 4.5,
              "generator": "ModelGenerator"
            },
            "geometries": [
              {
                "type": "SphereGeometry",
                "radius": 1,
                "widthSegments": 32,
                "heightSegments": 32,
                "uuid": "geometry-2"
              }
            ],
            "materials": [
              {
                "type": "MeshPhongMaterial",
                "color": 16711680,
                "emissive": 0,
                "specular": 1118481,
                "shininess": 30,
                "opacity": 1,
                "transparent": false,
                "uuid": "material-2"
              }
            ],
            "object": {
              "type": "Mesh",
              "geometry": "geometry-2",
              "material": "material-2",
              "position": [0, 0, 0],
              "rotation": [0, 0, 0],
              "scale": [1, 1, 1],
              "uuid": "mesh-2"
            }
          }
          break
          
        case 3: // 圆柱体
          modelData = {
            "metadata": {
              "type": "Object",
              "version": 4.5,
              "generator": "ModelGenerator"
            },
            "geometries": [
              {
                "type": "CylinderGeometry",
                "radiusTop": 1,
                "radiusBottom": 1,
                "height": 2,
                "radialSegments": 32,
                "uuid": "geometry-3"
              }
            ],
            "materials": [
              {
                "type": "MeshPhongMaterial",
                "color": 65280,
                "emissive": 0,
                "specular": 1118481,
                "shininess": 30,
                "opacity": 1,
                "transparent": false,
                "uuid": "material-3"
              }
            ],
            "object": {
              "type": "Mesh",
              "geometry": "geometry-3",
              "material": "material-3",
              "position": [0, 0, 0],
              "rotation": [0, 0, 0],
              "scale": [1, 1, 1],
              "uuid": "mesh-3"
            }
          }
          break
      }
      
      // 将模型数据转换为Data URL
      const modelJson = JSON.stringify(modelData)
      this.modelUrl = `data:application/json;charset=utf-8,${encodeURIComponent(modelJson)}`
    }
  }
}
</script>

<style scoped>
.model-test {
  padding: 20px;
}

.page-header {
  margin-bottom: 20px;
}

.model-section {
  margin-bottom: 20px;
}

.model-input {
  margin-bottom: 20px;
  display: flex;
  align-items: center;
}

.model-preview-container {
  width: 100%;
  height: 600px;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  overflow: hidden;
}

.empty-preview {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f5f7fa;
}

.test-models {
  margin-bottom: 20px;
}

.model-list {
  margin-top: 10px;
}

.model-list button {
  margin-right: 10px;
  margin-bottom: 10px;
}
</style>