<template>
  <div class="teaching-page">
    <section class="page-shell">
      <div class="page-title">
        <p>{{ pageSubtitle }}</p>
        <h1>{{ pageTitle }}</h1>
        <span v-if="selectedStudentName" class="student-hint">当前筛选学生：{{ selectedStudentName }}</span>
      </div>

      <div class="filter-grid">
        <el-input
          v-model.trim="filters.search"
          placeholder="搜索作品标题或描述"
          clearable
          @keyup.enter.native="loadWorks"
        />
        <el-select v-model="filters.studentId" clearable placeholder="选择学生" @change="loadWorks">
          <el-option
            v-for="student in students"
            :key="student.id"
            :label="student.name"
            :value="student.id"
          />
        </el-select>
        <el-select v-model="filters.status" clearable placeholder="作品状态" @change="loadWorks">
          <el-option v-for="item in statusOptions" :key="item" :label="item" :value="item" />
        </el-select>
        <el-input
          v-model.trim="filters.category"
          placeholder="分类筛选"
          clearable
          @keyup.enter.native="loadWorks"
        />
        <el-button type="primary" @click="loadWorks">搜索</el-button>
      </div>
    </section>

    <section class="table-shell">
      <el-table :data="works" stripe v-loading="loading">
        <el-table-column prop="title" label="作品标题" min-width="220" />
        <el-table-column prop="studentName" label="学生" min-width="140" />
        <el-table-column prop="studentWorkId" label="学号" width="110" />
        <el-table-column prop="category" label="分类" width="120" />
        <el-table-column label="当前状态" width="120">
          <template slot-scope="{ row }">
            <el-tag :type="statusTagType(row.status)">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="优秀作品" width="100">
          <template slot-scope="{ row }">
            <el-tag :type="row.isExcellent ? 'warning' : 'info'">
              {{ row.isExcellent ? '已标记' : '未标记' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="上传时间" min-width="170">
          <template slot-scope="{ row }">
            {{ formatDateTime(row.fileUploadTime || row.uploadDate) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template slot-scope="{ row }">
            <el-button type="text" @click="openReview(row)">管理</el-button>
            <el-button type="text" @click="download(row)">下载</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrap">
        <el-pagination
          :current-page="pagination.page"
          :page-size="pagination.pageSize"
          :total="pagination.total"
          layout="total, prev, pager, next"
          @current-change="handlePageChange"
        />
      </div>

      <el-empty v-if="!loading && works.length === 0" description="当前筛选条件下没有作品" />
    </section>

    <el-dialog title="管理学生作品" :visible.sync="dialogVisible" width="420px">
      <el-form label-width="100px">
        <el-form-item label="作品标题">
          <span>{{ currentWork?.title || '-' }}</span>
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="reviewForm.status" placeholder="请选择状态" style="width: 100%">
            <el-option v-for="item in statusOptions" :key="item" :label="item" :value="item" />
          </el-select>
        </el-form-item>
        <el-form-item label="优秀推荐">
          <el-switch v-model="reviewForm.isExcellent" active-text="是" inactive-text="否" />
        </el-form-item>
      </el-form>

      <div slot="footer">
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="saveReview">保存</el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import http from '../../utils/http'
import { getUser } from '../../utils/auth'

export default {
  name: 'TeachingWorks',
  data() {
    return {
      students: [],
      works: [],
      loading: false,
      saving: false,
      dialogVisible: false,
      currentWork: null,
      filters: {
        search: '',
        studentId: '',
        status: '',
        category: ''
      },
      pagination: {
        page: 1,
        pageSize: 10,
        total: 0
      },
      reviewForm: {
        status: '已发布',
        isExcellent: false
      },
      statusOptions: ['草稿', '待审核', '已发布', '已归档']
    }
  },
  computed: {
    pageTitle() {
      const user = getUser()
      return user?.role === 'Admin' ? '学生作品管理' : '我的学生作品'
    },
    pageSubtitle() {
      const user = getUser()
      return user?.role === 'Admin' ? '管理员可管理所有学生作品' : '教师只能管理自己负责学生的作品'
    },
    selectedStudentName() {
      const student = this.students.find(item => item.id === this.filters.studentId)
      return student?.name || this.$route.query.studentName || ''
    }
  },
  async mounted() {
    if (this.$route.query.studentId) {
      this.filters.studentId = Number(this.$route.query.studentId)
    }
    await this.loadStudents()
    await this.loadWorks()
  },
  methods: {
    async loadStudents() {
      try {
        const { data } = await http.get('/api/TeachingCollaboration/students')
        this.students = data.items || []
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载学生筛选项失败')
      }
    },
    async loadWorks() {
      this.loading = true
      try {
        const { data } = await http.get('/api/TeachingCollaboration/works', {
          params: {
            ...this.filters,
            page: this.pagination.page,
            pageSize: this.pagination.pageSize
          }
        })
        this.works = data.items || []
        this.pagination.total = data.total || 0
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载学生作品失败')
      } finally {
        this.loading = false
      }
    },
    handlePageChange(page) {
      this.pagination.page = page
      this.loadWorks()
    },
    openReview(work) {
      this.currentWork = work
      this.reviewForm = {
        status: work.status,
        isExcellent: !!work.isExcellent
      }
      this.dialogVisible = true
    },
    async saveReview() {
      if (!this.currentWork) return

      this.saving = true
      try {
        const { data } = await http.put(`/api/TeachingCollaboration/works/${this.currentWork.id}/review`, this.reviewForm)
        this.$message.success(data.message || '作品更新成功')
        this.dialogVisible = false
        await this.loadWorks()
      } catch (error) {
        this.$message.error(error.response?.data?.message || '更新作品失败')
      } finally {
        this.saving = false
      }
    },
    async download(work) {
      if (!work?.filePath) {
        this.$message.error('当前作品没有可下载文件')
        return
      }

      try {
        const res = await http.get('/api/File/download', { params: { fileName: work.filePath }, responseType: 'blob' })
        const blob = res.data
        const url = window.URL.createObjectURL(blob)
        const link = document.createElement('a')
        link.href = url
        link.download = work.fileName || work.filePath
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        window.URL.revokeObjectURL(url)
      } catch (error) {
        this.$message.error(error.response?.data?.message || '下载文件失败')
      }
    },
    statusTagType(status) {
      switch (status) {
        case '已发布':
          return 'success'
        case '待审核':
          return 'warning'
        case '已归档':
          return 'info'
        default:
          return ''
      }
    },
    formatDateTime(value) {
      if (!value) return '暂无'
      return new Date(value).toLocaleString('zh-CN')
    }
  }
}
</script>

<style scoped>
.teaching-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.page-shell,
.table-shell {
  padding: 24px;
  border: 1px solid #dce6ef;
  border-radius: 20px;
  background: #ffffff;
  box-shadow: 0 14px 34px rgba(22, 54, 91, 0.08);
}

.page-shell {
  background:
    radial-gradient(circle at top left, rgba(255, 183, 77, 0.18), transparent 22%),
    linear-gradient(135deg, #fffaf1 0%, #f4f8ff 100%);
}

.page-title p {
  margin: 0 0 8px;
  color: #7f6a45;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.16em;
  text-transform: uppercase;
}

.page-title h1 {
  margin: 0;
  color: #182a3a;
  font-size: 30px;
}

.student-hint {
  display: inline-block;
  margin-top: 10px;
  color: #5f7185;
  font-size: 13px;
}

.filter-grid {
  display: grid;
  grid-template-columns: 1.3fr 1fr 1fr 1fr auto;
  gap: 12px;
  margin-top: 20px;
}

.pagination-wrap {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

@media (max-width: 980px) {
  .filter-grid {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 640px) {
  .filter-grid {
    grid-template-columns: 1fr;
  }
}
</style>
