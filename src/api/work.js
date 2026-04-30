import http from './http'

export function getWorks(params) {
  return http.get('/api/Work', { params })
}

export function getPublicWorks(params) {
  return http.get('/api/Work/public', { params })
}

export function getRecentWorks(params) {
  return http.get('/api/Work/recent', { params })
}

export function getWorkById(id) {
  return http.get(`/api/Work/${id}`)
}

export function createWork(payload) {
  return http.post('/api/Work', payload)
}

export function updateWork(id, payload) {
  return http.put(`/api/Work/${id}`, payload)
}

export function deleteWork(id) {
  return http.delete(`/api/Work/${id}`)
}

export function reviewWork(id, payload) {
  return http.post(`/api/Work/${id}/review`, payload)
}

export function addView(id) {
  return http.get(`/api/Work/${id}/view`)
}

export function favoriteWork(id) {
  return http.post(`/api/Work/${id}/favorite`)
}

export function unfavoriteWork(id) {
  return http.delete(`/api/Work/${id}/favorite`)
}

export function isFavorite(id) {
  return http.get(`/api/Work/${id}/is-favorite`)
}

