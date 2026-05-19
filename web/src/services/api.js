/**
 * API 服务层 - 统一管理所有后端接口调用
 */
import http from '../utils/http'

// ==================== 用户认证 ====================

export const authApi = {
  login(data) {
    return http.post('/api/Auth/login', data)
  },
  register(data) {
    return http.post('/api/Auth/register', data)
  },
  getProfile() {
    return http.get('/api/Auth/profile')
  },
  updateProfile(data) {
    return http.put('/api/Auth/profile', data)
  },
  uploadAvatar(file) {
    const formData = new FormData()
    formData.append('file', file)
    return http.put('/api/Auth/avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },
  getPreferences() {
    return http.get('/api/Auth/preferences')
  },
  updatePreferences(data) {
    return http.put('/api/Auth/preferences', data)
  }
}

// ==================== 用户管理 (Admin) ====================

export const userApi = {
  getUsers(params) {
    return http.get('/api/Auth/users', { params })
  },
  updateRole(id, role) {
    return http.put(`/api/Auth/role/${id}`, { role })
  },
  resetPassword(id, password) {
    return http.put(`/api/Auth/password/${id}`, { password })
  },
  deleteUser(id) {
    return http.delete(`/api/Auth/users/${id}`)
  },
  importUsers(formData) {
    return http.post('/api/Auth/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },
  exportUsers() {
    return http.get('/api/Auth/export', { responseType: 'blob' })
  }
}

// ==================== 作品管理 ====================

export const workApi = {
  getWorks(params) {
    return http.get('/api/Work', { params })
  },
  getWorkById(id) {
    return http.get(`/api/Work/${id}`)
  },
  createWork(data) {
    return http.post('/api/Work', data)
  },
  updateWork(id, data) {
    return http.put(`/api/Work/${id}`, data)
  },
  deleteWork(id) {
    return http.delete(`/api/Work/${id}`)
  },
  uploadWorkFile(workId, file) {
    const formData = new FormData()
    formData.append('file', file)
    return http.post(`/api/Work/${workId}/file`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },
  incrementViews(id) {
    return http.get(`/api/Work/${id}/view`)
  }
}

// ==================== 收藏管理 ====================

export const favoriteApi = {
  getFavorites(params) {
    return http.get('/api/Collection', { params })
  },
  toggleFavorite(workId) {
    return http.post(`/api/Collection/toggle/${workId}`)
  },
  checkFavorite(workId) {
    return http.get(`/api/Collection/check/${workId}`)
  }
}

// ==================== 文件管理 ====================

export const fileApi = {
  getDownloadUrl(fileName) {
    return `/api/File/download?fileName=${encodeURIComponent(fileName)}`
  },
  previewFile(fileName) {
    return http.get('/api/File/preview', { params: { fileName }, responseType: 'blob' })
  },
  deleteFile(fileName) {
    return http.delete('/api/File', { params: { fileName } })
  }
}

// ==================== 教学协同 ====================

export const teachingApi = {
  getStudents(params) {
    return http.get('/api/TeachingCollaboration/students', { params })
  },
  getStudentWorks(params) {
    return http.get('/api/TeachingCollaboration/student-works', { params })
  },
  getTeachingOverview() {
    return http.get('/api/TeachingCollaboration/overview')
  },
  addStudent(studentId) {
    return http.post('/api/TeachingCollaboration/student', { studentId })
  },
  removeStudent(studentId) {
    return http.delete(`/api/TeachingCollaboration/student/${studentId}`)
  }
}

// ==================== 内容审核 ====================

export const moderationApi = {
  getModerationItems(params) {
    return http.get('/api/ContentModeration', { params })
  },
  approveWork(id) {
    return http.put(`/api/ContentModeration/${id}/approve`)
  },
  rejectWork(id, reason) {
    return http.put(`/api/ContentModeration/${id}/reject`, { reason })
  },
  reportWork(workId, reason) {
    return http.post('/api/ContentModeration/report', { workId, reason })
  }
}

// ==================== 通知管理 ====================

export const notificationApi = {
  getNotifications(params) {
    return http.get('/api/Notification', { params })
  },
  getUnreadCount() {
    return http.get('/api/Notification/unread-count')
  },
  markAsRead(id) {
    return http.put(`/api/Notification/${id}/read`)
  },
  markAllAsRead() {
    return http.put('/api/Notification/all/read')
  },
  deleteNotification(id) {
    return http.delete(`/api/Notification/${id}`)
  },
  sendAnnouncement(data) {
    return http.post('/api/Notification/announcement', data)
  }
}

// ==================== 公告管理 ====================

export const announcementApi = {
  getAnnouncements(params) {
    return http.get('/api/Announcement', { params })
  },
  getAllAnnouncements(params) {
    return http.get('/api/Announcement/admin/list', { params })
  },
  getAnnouncementById(id) {
    return http.get(`/api/Announcement/${id}`)
  },
  createAnnouncement(data) {
    return http.post('/api/Announcement', data)
  },
  updateAnnouncement(id, data) {
    return http.put(`/api/Announcement/${id}`, data)
  },
  deleteAnnouncement(id) {
    return http.delete(`/api/Announcement/${id}`)
  }
}

// ==================== 意见反馈 ====================

export const feedbackApi = {
  getFeedbacks(params) {
    return http.get('/api/Feedback', { params })
  },
  getFeedbackById(id) {
    return http.get(`/api/Feedback/${id}`)
  },
  createFeedback(data) {
    return http.post('/api/Feedback', data)
  },
  replyFeedback(id, data) {
    return http.put(`/api/Feedback/${id}/reply`, data)
  },
  updateFeedbackStatus(id, data) {
    return http.put(`/api/Feedback/${id}/status`, data)
  },
  deleteFeedback(id) {
    return http.delete(`/api/Feedback/${id}`)
  }
}

// ==================== 数据分析 ====================

export const dataApi = {
  getOverview() {
    return http.get('/api/Data/overview')
  },
  getWorksStatistics(params) {
    return http.get('/api/Data/works-statistics', { params })
  },
  getUserStatistics(params) {
    return http.get('/api/Data/user-statistics', { params })
  }
}