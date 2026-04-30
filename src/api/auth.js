import http from './http'

export function login(payload) {
  return http.post('/api/Auth/login', payload)
}

export function register(payload) {
  return http.post('/api/Auth/register', payload)
}

export function getProfile() {
  return http.get('/api/Auth/profile')
}

export function updateProfile(payload) {
  return http.put('/api/Auth/profile', payload)
}

export function changePassword(payload) {
  return http.put('/api/Auth/change-password', payload)
}

