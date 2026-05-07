<template>
  <div class="teaching-page">
    <section class="page-shell">
      <div class="page-title">
        <p>{{ pageSubtitle }}</p>
        <h1>{{ pageTitle }}</h1>
      </div>
      <div class="toolbar">
        <el-input
          v-model.trim="search"
          placeholder="搜索学生姓名、用户名或学号"
          clearable
          class="search-input"
          @keyup.enter.native="loadStudents"
        />
        <el-button type="primary" @click="loadStudents">刷新列表</el-button>
      </div>
    </section>

    <section class="table-shell">
      <el-table :data="students" stripe v-loading="loading">
        <el-table-column prop="name" label="学生" min-width="160" />
        <el-table-column prop="username" label="账号" min-width="140" />
        <el-table-column prop="workId" label="学号" min-width="120" />
        <el-table-column prop="department" label="院系" min-width="160" />
        <el-table-column prop="workCount" label="作品数" width="100" />
        <el-table-column prop="publishedCount" label="已发布" width="100" />
        <el-table-column prop="excellentCount" label="优秀" width="100" />
        <el-table-column label="最近上传" min-width="160">
          <template slot-scope="{ row }">
            {{ formatDateTime(row.latestUploadTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template slot-scope="{ row }">
            <el-button type="text" @click="goToWorks(row)">查看作品</el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-empty v-if="!loading && students.length === 0" description="当前没有可管理的学生" />
    </section>
  </div>
</template>

<script>
import http from '../../utils/http'
import { getUser } from '../../utils/auth'

export default {
  name: 'TeachingStudents',
  data() {
    return {
      students: [],
      loading: false,
      search: ''
    }
  },
  computed: {
    pageTitle() {
      const user = getUser()
      return user?.role === 'Admin' ? '所有学生' : '我的学生'
    },
    pageSubtitle() {
      const user = getUser()
      return user?.role === 'Admin' ? '管理员可查看所有学生数据' : '教师只能查看自己负责的学生'
    }
  },
  async mounted() {
    await this.loadStudents()
  },
  methods: {
    async loadStudents() {
      this.loading = true
      try {
        const { data } = await http.get('/api/TeachingCollaboration/students', { params: { search: this.search } })
        this.students = data.items || []
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载学生列表失败')
      } finally {
        this.loading = false
      }
    },
    goToWorks(student) {
      this.$router.push({
        path: '/teaching/works',
        query: {
          studentId: String(student.id),
          studentName: student.name
        }
      })
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
  border: 1px solid #dde7f0;
  border-radius: 20px;
  background: #ffffff;
  box-shadow: 0 14px 34px rgba(22, 54, 91, 0.08);
}

.page-shell {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 24px;
  background:
    radial-gradient(circle at left top, rgba(65, 137, 230, 0.12), transparent 26%),
    linear-gradient(135deg, #f7fbff 0%, #fff9f2 100%);
}

.page-title p {
  margin: 0 0 8px;
  color: #73869b;
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

.toolbar {
  display: flex;
  gap: 12px;
}

.search-input {
  width: 320px;
}

@media (max-width: 760px) {
  .page-shell {
    flex-direction: column;
    align-items: stretch;
  }

  .toolbar {
    flex-direction: column;
  }

  .search-input {
    width: 100%;
  }
}
</style>
