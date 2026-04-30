import http from './http'

export function getTeachingOverview() {
  return http.get('/api/TeachingCollaboration/overview')
}

export function getTeachingStudents(params) {
  return http.get('/api/TeachingCollaboration/students', { params })
}

export function getTeachingWorks(params) {
  return http.get('/api/TeachingCollaboration/works', { params })
}

export function reviewTeachingWork(id, payload) {
  return http.put(`/api/TeachingCollaboration/works/${id}/review`, payload)
}
