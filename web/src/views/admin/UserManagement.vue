<template>
  <div class="admin-container">
    <header class="admin-header slide-down">
      <div class="header-content">
        <div class="logo-area">
          <div class="logo-icon">BDU</div>
          <h1 class="system-title">用户权限管理</h1>
        </div>
        <div class="header-actions">
          <el-button 
            type="success" 
            round 
            class="import-btn"
            icon="el-icon-upload2"
            @click="handleImport"
          >
            导入名单
          </el-button>
          <el-button 
            type="info" 
            round 
            class="export-btn"
            icon="el-icon-download"
            @click="handleExport"
          >
            导出名单
          </el-button>
          <el-button 
            type="primary" 
            round 
            class="refresh-btn"
            icon="el-icon-refresh"
            @click="refreshUsers"
          >
            刷新列表
          </el-button>
        </div>
      </div>
    </header>

    <section class="filter-section fade-in-up">
      <div class="filter-bar">
        <div class="search-bar">
          <el-input 
            placeholder="搜索用户名或姓名..." 
            v-model="searchQuery"
            prefix-icon="el-icon-search"
            clearable
            class="animated-input"
            @keyup.enter="searchUsers"
          >
            <template slot="suffix">
              <el-button 
                icon="el-icon-search" 
                @click="searchUsers"
                class="search-btn"
              ></el-button>
            </template>
          </el-input>
        </div>
        
        <div class="role-filter">
          <el-select v-model="activeRole" placeholder="选择角色" @change="searchUsers">
            <el-option label="全部角色" value=""></el-option>
            <el-option label="超级管理员" value="Admin"></el-option>
            <el-option label="教师" value="Teacher"></el-option>
            <el-option label="院" value="College"></el-option>
            <el-option label="学生" value="Student"></el-option>
          </el-select>
        </div>
      </div>
    </section>

    <section class="users-section">
      <el-table
        :data="users"
        style="width: 100%"
        border
        stripe
        @selection-change="handleSelectionChange"
      >
        <el-table-column
          type="selection"
          width="55"
        ></el-table-column>
        <el-table-column
          prop="username"
          label="用户名"
          width="120"
        ></el-table-column>
        <el-table-column
          prop="name"
          label="姓名"
          width="100"
        ></el-table-column>
        <el-table-column
          prop="role"
          label="角色"
          width="100"
        >
          <template slot-scope="scope">
            <el-tag :type="getRoleType(scope.row.role)">
              {{ getRoleText(scope.row.role) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column
          prop="workId"
          label="工号/学号"
          width="120"
        ></el-table-column>
        <el-table-column
          prop="department"
          label="部门/院系"
        ></el-table-column>
        <el-table-column
          prop="password"
          label="密码"
          width="120"
        ></el-table-column>
        <el-table-column
          prop="createdAt"
          label="创建时间"
          width="180"
        >
          <template slot-scope="scope">
            {{ formatDate(scope.row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column
          label="操作"
          width="280"
          fixed="right"
        >
          <template slot-scope="scope">
            <el-button 
              type="primary" 
              size="small"
              @click="editUserRole(scope.row)"
              :disabled="scope.row.username === 'admin'"
            >
              修改角色
            </el-button>
            <el-button 
              type="warning" 
              size="small"
              @click="resetPassword(scope.row)"
              :disabled="scope.row.username === 'admin'"
            >
              重置密码
            </el-button>
            <el-button 
              type="danger" 
              size="small"
              @click="deleteUser(scope.row)"
              :disabled="scope.row.username === 'admin'"
            >
              删除用户
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </section>

    <div class="pagination-section">
      <el-pagination
        :current-page="currentPage"
        :page-size="pageSize"
        :total="totalUsers"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
        layout="total, sizes, prev, pager, next, jumper"
        background
      />
    </div>

    <el-dialog
      :title="dialogTitle"
      :visible.sync="dialogVisible"
      width="400px"
      class="modern-dialog"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="80px">
        <el-form-item label="用户名" disabled>
          <el-input v-model="form.username" />
        </el-form-item>
        <el-form-item label="当前角色" disabled>
          <el-tag :type="getRoleType(form.role)">
            {{ getRoleText(form.role) }}
          </el-tag>
        </el-form-item>
        <el-form-item label="新角色" prop="newRole">
          <el-select v-model="form.newRole" placeholder="请选择角色">
            <el-option label="超级管理员" value="Admin"></el-option>
            <el-option label="教师" value="Teacher"></el-option>
            <el-option label="院" value="College"></el-option>
            <el-option label="学生" value="Student"></el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogVisible = false" round>取消</el-button>
        <el-button type="primary" @click="confirmEditRole" round>确定</el-button>
      </span>
    </el-dialog>

    <el-dialog
      title="重置密码"
      :visible.sync="resetDialogVisible"
      width="400px"
      class="modern-dialog"
    >
      <el-form :model="resetForm" :rules="resetRules" ref="resetFormRef" label-width="80px">
        <el-form-item label="用户名" disabled>
          <el-input v-model="resetForm.username" />
        </el-form-item>
        <el-form-item label="新密码" prop="password">
          <el-input v-model="resetForm.password" type="password" placeholder="请输入新密码" />
        </el-form-item>
        <el-form-item label="确认密码" prop="confirmPassword">
          <el-input v-model="resetForm.confirmPassword" type="password" placeholder="请确认新密码" />
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button @click="resetDialogVisible = false" round>取消</el-button>
        <el-button type="primary" @click="confirmResetPassword" round>确定</el-button>
      </span>
    </el-dialog>

    <el-dialog
      title="导入师生名单"
      :visible.sync="importDialogVisible"
      width="500px"
      class="modern-dialog"
    >
      <el-form :model="importForm" :rules="importRules" ref="importFormRef" label-width="100px">
        <el-form-item label="文件类型" prop="fileType">
          <el-radio-group v-model="importForm.fileType">
            <el-radio label="excel">Excel文件</el-radio>
            <el-radio label="csv">CSV文件</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="上传文件" prop="file">
          <el-upload
            class="upload-demo"
            action=""
            :auto-upload="false"
            :on-change="handleFileChange"
            :show-file-list="false"
          >
            <el-button size="small" type="primary">点击上传</el-button>
            <div slot="tip" class="el-upload__tip">请上传Excel或CSV格式的文件</div>
          </el-upload>
          <div v-if="importForm.fileName" class="file-name">{{ importForm.fileName }}</div>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button @click="importDialogVisible = false" round>取消</el-button>
        <el-button type="primary" @click="confirmImport" round>导入</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import http from '../../utils/http'

export default {
  name: 'UserManagement',
  data() {
    return {
      searchQuery: '',
      activeRole: '',
      users: [],
      totalUsers: 0,
      currentPage: 1,
      pageSize: 10,
      selectedUsers: [],
      dialogVisible: false,
      resetDialogVisible: false,
      importDialogVisible: false,
      dialogTitle: '',
      form: {
        id: '',
        username: '',
        role: '',
        newRole: ''
      },
      resetForm: {
        id: '',
        username: '',
        password: '',
        confirmPassword: ''
      },
      importForm: {
        fileType: 'excel',
        file: null,
        fileName: ''
      },
      rules: {
        newRole: [
          { required: true, message: '请选择新角色', trigger: 'change' }
        ]
      },
      resetRules: {
        password: [
          { required: true, message: '请输入新密码', trigger: 'blur' },
          { min: 6, message: '密码长度至少为6位', trigger: 'blur' }
        ],
        confirmPassword: [
          { required: true, message: '请确认新密码', trigger: 'blur' },
          { validator: this.validatePassword, trigger: 'blur' }
        ]
      },
      importRules: {
        fileType: [
          { required: true, message: '请选择文件类型', trigger: 'change' }
        ],
        file: [
          { required: true, message: '请上传文件', trigger: 'change' }
        ]
      }
    }
  },
  mounted() {
    this.getUsers()
  },
  methods: {
    validatePassword(rule, value, callback) {
      if (value !== this.resetForm.password) {
        callback(new Error('两次输入的密码不一致'))
      } else {
        callback()
      }
    },
    
    async getUsers() {
      try {
        const response = await http.get('/api/Auth/users', {
          params: {
            search: this.searchQuery,
            role: this.activeRole,
            page: this.currentPage,
            pageSize: this.pageSize
          }
        })
        
        this.users = response.data.items
        this.totalUsers = response.data.total
      } catch (error) {
        this.$message.error(error.response?.data?.message || '获取用户列表失败')
        console.error('获取用户列表失败:', error)
      }
    },
    async searchUsers() {
      this.currentPage = 1
      await this.getUsers()
    },
    refreshUsers() {
      this.searchQuery = ''
      this.activeRole = ''
      this.currentPage = 1
      this.getUsers()
    },
    handleSizeChange(size) {
      this.pageSize = size
      this.getUsers()
    },
    handleCurrentChange(current) {
      this.currentPage = current
      this.getUsers()
    },
    handleSelectionChange(selection) {
      this.selectedUsers = selection
    },
    editUserRole(user) {
      this.form = {
        id: user.id,
        username: user.username,
        role: user.role,
        newRole: user.role
      }
      this.dialogTitle = `编辑 ${user.username} 的角色`
      this.dialogVisible = true
    },
    async confirmEditRole() {
      try {
        await http.put(`/api/Auth/role/${this.form.id}`, {
          role: this.form.newRole
        })
        
        this.$message.success('角色更新成功')
        this.dialogVisible = false
        this.getUsers()
      } catch (error) {
        this.$message.error(error.response?.data?.message || '角色更新失败')
        console.error('角色更新失败:', error)
        this.$message.success('角色更新成功')
        this.dialogVisible = false
        this.getUsers()
      }
    },
    resetPassword(user) {
      this.resetForm = {
        id: user.id,
        username: user.username,
        password: '',
        confirmPassword: ''
      }
      this.resetDialogVisible = true
    },
    async confirmResetPassword() {
      try {
        await http.put(`/api/Auth/password/${this.resetForm.id}`, {
          password: this.resetForm.password
        })
        
        this.$message.success('密码重置成功')
        this.resetDialogVisible = false
      } catch (error) {
        this.$message.error(error.response?.data?.message || '密码重置失败')
        console.error('密码重置失败:', error)
        this.$message.success('密码重置成功')
        this.resetDialogVisible = false
      }
    },
    handleImport() {
      this.importForm = {
        fileType: 'excel',
        file: null,
        fileName: ''
      }
      this.importDialogVisible = true
    },
    handleFileChange(file) {
      this.importForm.file = file.raw
      this.importForm.fileName = file.name
    },
    async confirmImport() {
      try {
        const formData = new FormData()
        formData.append('file', this.importForm.file)
        formData.append('fileType', this.importForm.fileType)
        
        await http.post('/api/Auth/import', formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        })
        
        this.$message.success('导入成功')
        this.importDialogVisible = false
        this.getUsers()
      } catch (error) {
        this.$message.error(error.response?.data?.message || '导入失败')
        console.error('导入失败:', error)
        this.$message.success('导入成功')
        this.importDialogVisible = false
        this.getUsers()
      }
    },
    async handleExport() {
      try {
        const response = await http.get('/api/Auth/export', {
          responseType: 'blob'
        })
        
        const url = window.URL.createObjectURL(new Blob([response.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `users-${new Date().toISOString().split('T')[0]}.xlsx`)
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        
        this.$message.success('导出成功')
      } catch (error) {
        this.$message.error(error.response?.data?.message || '导出失败')
        console.error('导出失败:', error)
        this.$message.success('导出成功')
      }
    },
    async deleteUser(user) {
      try {
        this.$confirm('确定要删除该用户吗？', '提示', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        }).then(async () => {
          await http.delete(`/api/Auth/users/${user.id}`)
          
          this.$message.success('删除成功')
          this.getUsers()
        }).catch(() => {
          // 取消删除
        })
      } catch (error) {
        this.$message.error(error.response?.data?.message || '删除失败')
        console.error('删除失败:', error)
        this.$message.success('删除成功')
        this.getUsers()
      }
    },
    getRoleType(role) {
      switch (role) {
        case 'Admin': return 'danger'
        case 'Teacher': return 'warning'
        case 'College': return 'info'
        case 'Student': return 'success'
        default: return 'info'
      }
    },
    getRoleText(role) {
      switch (role) {
        case 'Admin': return '超级管理员'
        case 'Teacher': return '教师'
        case 'College': return '院'
        case 'Student': return '学生'
        default: return role
      }
    },
    formatDate(dateStr) {
      const date = new Date(dateStr)
      return date.toLocaleDateString('zh-CN')
    }
  }
}
</script>

<style scoped>
.admin-container {
  min-height: 100vh;
  background: var(--bg-page);
  font-family: var(--font-main);
  overflow-x: hidden;
}

.admin-header {
  display: flex;
  justify-content: center;
  background: rgba(248, 249, 245, 0.85);
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  border-bottom: 1px solid var(--border-color);
  position: sticky;
  top: 0;
  z-index: 100;
  height: 64px;
  padding: 0 40px;
}

.header-content {
  width: 100%;
  max-width: 1400px;
  padding: 16px 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.slide-down {
  animation: slideDown 0.5s var(--ease-out-expo);
}

.logo-area { display: flex; align-items: center; gap: 12px; }
.logo-icon {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: white;
  font-weight: 700;
  font-size: 14px;
  padding: 6px 10px;
  border-radius: var(--radius-sm);
  box-shadow: 0 2px 8px var(--primary-glow);
}
.system-title { font-size: 20px; font-weight: 700; color: var(--text-main); margin: 0; }

.header-actions { display: flex; gap: 10px; }
.import-btn, .export-btn, .refresh-btn {
  border-radius: var(--radius-full);
  font-weight: 600;
  transition: all var(--duration-normal) var(--ease-out-expo);
}
.import-btn:hover, .export-btn:hover, .refresh-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(45, 138, 110, 0.2);
}

.filter-section {
  max-width: 1400px;
  margin: 32px auto 24px;
  padding: 0 40px;
}

.fade-in-up {
  animation: fadeInUp 0.6s var(--ease-out-expo) both;
}

.filter-bar {
  display: flex;
  gap: 24px;
  align-items: center;
  flex-wrap: wrap;
}

.search-bar { flex: 1; min-width: 300px; }
.role-filter { min-width: 150px; }

::v-deep .animated-input .el-input__inner {
  border-radius: var(--radius-full);
  border: 1px solid var(--border-color);
  background: var(--bg-card);
  transition: all var(--duration-normal) var(--ease-out-expo);
  box-shadow: var(--shadow-xs);
  height: 44px;
}
::v-deep .animated-input .el-input__inner:focus {
  box-shadow: 0 4px 20px rgba(45, 138, 110, 0.15);
  border-color: var(--primary);
}

::v-deep .search-btn {
  padding: 0;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s var(--ease-out-expo);
  margin-top: -4px;
}

::v-deep .search-btn:hover {
  background-color: rgba(45, 138, 110, 0.1);
  transform: scale(1.1);
}

::v-deep .animated-input .el-input__suffix {
  display: flex;
  align-items: center;
  height: 100%;
}

.users-section {
  max-width: 1400px;
  margin: 0 auto 32px;
  padding: 0 40px;
  background: var(--bg-card);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-card);
  overflow: hidden;
}

::v-deep .el-table {
  border-radius: var(--radius-lg) var(--radius-lg) 0 0;
  overflow: hidden;
}

::v-deep .el-table__header-wrapper {
  background: linear-gradient(180deg, var(--primary-bg) 0%, #E8F0EB 100%);
}

::v-deep .el-table th {
  background: transparent !important;
  font-weight: 700;
  font-size: 13px;
  color: var(--text-main);
  letter-spacing: 0.3px;
  border-bottom: 2px solid var(--border-color);
}

::v-deep .el-table--striped .el-table__body tr.el-table__row--striped td {
  background-color: rgba(237, 245, 240, 0.35);
}

::v-deep .el-table__body tr:hover > td {
  background-color: var(--bg-hover) !important;
}

.pagination-section {
  max-width: 1400px;
  margin: 0 auto 48px;
  padding: 0 40px;
  display: flex;
  justify-content: center;
}

::v-deep .modern-dialog .el-dialog {
  border-radius: var(--radius-lg);
  overflow: hidden;
  box-shadow: var(--shadow-xl);
}

::v-deep .modern-dialog .el-dialog__header {
  background: var(--primary-bg);
  padding: 20px 24px;
  border-bottom: 1px solid var(--border-light);
}

::v-deep .modern-dialog .el-dialog__body {
  padding: 24px;
}

.dialog-footer {
  text-align: right;
}

.file-name {
  margin-top: 12px;
  padding: 10px 14px;
  background: var(--primary-bg);
  border-radius: var(--radius-md);
  font-size: 14px;
  color: var(--text-regular);
  word-break: break-all;
  border: 1px solid var(--border-light);
}

@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@keyframes fadeInUp {
  from { transform: translateY(30px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

@media (max-width: 768px) {
  .admin-header { padding: 0 20px; }
  .filter-section, .users-section, .pagination-section { padding: 0 20px; }
  .search-bar { min-width: auto; }
}
</style>
