import http from './http'

export function getOverview() {
  return http.get('/api/Data/overview')
}

export function getDailyUploads() {
  return http.get('/api/Data/daily-uploads')
}

export function getCategoryDistribution() {
  return http.get('/api/Data/category-distribution')
}

export function getStatusDistribution() {
  return http.get('/api/Data/status-distribution')
}

export function getMonthlyUploads() {
  return http.get('/api/Data/monthly-uploads')
}

// 导出数据分析报表（Excel）
export function exportReport(params) {
  return http.get('/api/Data/export', {
    params,
    responseType: 'blob'
  })
}

