import http from './http'

export function uploadChunk(formData) {
  return http.post('/api/File/upload-chunk', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}

export function mergeChunks(payload) {
  return http.post('/api/File/merge-chunks', payload)
}

export function getUploadStatus(params) {
  return http.get('/api/File/upload-status', { params })
}

export function downloadFile(params) {
  return http.get('/api/File/download', {
    params,
    responseType: 'blob'
  })
}

