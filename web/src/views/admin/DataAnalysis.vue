<template>
  <div class="data-analysis-container">
    <header class="data-analysis-header">
      <div class="header-content">
        <h1 class="system-title">数据统计与分析</h1>
        <div class="header-actions">
          <el-button
            type="info"
            round
            icon="el-icon-download"
            :loading="exporting"
            @click="handleExport"
          >
            {{ exporting ? '导出中...' : '导出报表' }}
          </el-button>
          <el-button 
            type="primary" 
            round 
            icon="el-icon-refresh"
            @click="loadData"
          >
            刷新数据
          </el-button>
        </div>
      </div>
    </header>

    <section class="stats-section">
      <div class="stats-grid">
        <div 
          v-for="(stat, index) in stats" 
          :key="stat.label"
          class="stat-card"
        >
          <div class="stat-icon" :style="{ backgroundColor: stat.color }">
            <i :class="stat.icon"></i>
          </div>
          <div class="stat-content">
            <span class="stat-number">{{ stat.value }}</span>
            <span class="stat-label">{{ stat.label }}</span>
          </div>
        </div>
      </div>
    </section>

    <section class="charts-section">
      <div class="charts-grid">
        <div class="chart-item">
          <el-card class="chart-card" shadow="never">
            <div class="chart-header">
              <h3 class="chart-title">每日上传作品数量</h3>
            </div>
            <div ref="lineChart" class="chart"></div>
          </el-card>
        </div>
        <div class="chart-item">
          <el-card class="chart-card" shadow="never">
            <div class="chart-header">
              <h3 class="chart-title">作品类别分布</h3>
            </div>
            <div ref="pieChart" class="chart"></div>
          </el-card>
        </div>
        <div class="chart-item">
          <el-card class="chart-card" shadow="never">
            <div class="chart-header">
              <h3 class="chart-title">作品状态分布</h3>
            </div>
            <div ref="statusChart" class="chart"></div>
          </el-card>
        </div>
        <div class="chart-item">
          <el-card class="chart-card" shadow="never">
            <div class="chart-header">
              <h3 class="chart-title">月度上传趋势</h3>
            </div>
            <div ref="monthChart" class="chart"></div>
          </el-card>
        </div>
      </div>
    </section>

    <div v-if="loading" class="loading-container">
      <el-spinner type="el-icon-loading" size="50px"></el-spinner>
      <p>加载中...</p>
    </div>
  </div>
</template>

<script>
import * as echarts from 'echarts'
import http from '../../utils/http'

export default {
  name: 'DataAnalysis',
  data() {
    return {
      loading: false,
      exporting: false,
      lineChart: null,
      pieChart: null,
      statusChart: null,
      monthChart: null,
      resizeTimer: null,
      dailyUploads: {
        dates: ['2026-04-14', '2026-04-15', '2026-04-16', '2026-04-17', '2026-04-18', '2026-04-19', '2026-04-20'],
        uploads: [5, 8, 12, 7, 10, 6, 9]
      },
      categoryDistribution: [
        { category: '前端开发', count: 25 },
        { category: '后端开发', count: 18 },
        { category: '人工智能', count: 12 },
        { category: '设计', count: 10 },
        { category: '其他', count: 5 }
      ],
      statusDistribution: [
        { status: '已发布', count: 45 },
        { status: '草稿', count: 15 }
      ],
      monthlyUploads: {
        months: ['1月', '2月', '3月', '4月', '5月', '6月'],
        uploads: [20, 35, 45, 55, 60, 45]
      },
      stats: [
        { label: '总作品数', value: 60, icon: 'el-icon-document', color: '#e6f7ff' },
        { label: '总用户数', value: 35, icon: 'el-icon-user', color: '#f6ffed' },
        { label: '今日上传', value: 9, icon: 'el-icon-upload', color: '#fff0f6' },
        { label: '总浏览量', value: 1200, icon: 'el-icon-view', color: '#f0f5ff' }
      ]
    }
  },
  mounted() {
    this.loadData()
    window.addEventListener('resize', this.handleResize)
  },
  beforeDestroy() {
    window.removeEventListener('resize', this.handleResize)
    this.disposeCharts()
  },
  methods: {
    disposeCharts() {
      if (this.lineChart) { this.lineChart.dispose(); this.lineChart = null }
      if (this.pieChart) { this.pieChart.dispose(); this.pieChart = null }
      if (this.statusChart) { this.statusChart.dispose(); this.statusChart = null }
      if (this.monthChart) { this.monthChart.dispose(); this.monthChart = null }
    },
    async handleExport() {
      this.exporting = true
      try {
        const res = await http.get('/api/Data/export', { params: { days: 7 }, responseType: 'blob' })
        const blob = new Blob([res.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
        const url = window.URL.createObjectURL(blob)
        let fileName = `data-report-${new Date().toISOString().slice(0, 16).replace(/[:T]/g, '-')}.xlsx`
        const contentDisposition = res.headers?.['content-disposition'] || res.headers?.['Content-Disposition']
        if (contentDisposition) {
          const match = /filename\*?=(?:UTF-8''|\"?)([^\";]+)/i.exec(contentDisposition)
          if (match && match[1]) {
            fileName = decodeURIComponent(match[1].replace(/\"/g, ''))
          }
        }
        const link = document.createElement('a')
        link.href = url
        link.download = fileName
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        window.URL.revokeObjectURL(url)
        this.$message.success('报表已导出')
      } catch (error) {
        this.$message.error(error.response?.data?.message || '导出失败，请稍后重试')
      } finally {
        this.exporting = false
      }
    },
    async loadData() {
      this.loading = true
      try {
        const [overviewData, dailyData, categoryData, statusData, monthlyData] = await Promise.all([
          this.fetchOverview(),
          this.fetchDailyUploads(),
          this.fetchCategoryDistribution(),
          this.fetchStatusDistribution(),
          this.fetchMonthlyUploads()
        ])

        this.updateStats(overviewData)
        this.dailyUploads = dailyData || this.dailyUploads
        this.categoryDistribution = categoryData || this.categoryDistribution
        this.statusDistribution = statusData || this.statusDistribution
        this.monthlyUploads = monthlyData || this.monthlyUploads
        
        this.$nextTick(() => {
          this.initCharts()
        })
      } catch (error) {
        this.$nextTick(() => {
          this.initCharts()
        })
      } finally {
        this.loading = false
      }
    },
    
    updateStats(data) {
      if (!data) return
      this.stats[0].value = data.totalWorks || 60
      this.stats[1].value = data.totalUsers || 35
      this.stats[2].value = data.todayWorks || 9
      this.stats[3].value = data.totalViews || 1200
    },
    
    async fetchOverview() {
      try {
        const response = await http.get('/api/Data/overview')
        return response.data
      } catch {
        return null
      }
    },
    
    async fetchDailyUploads() {
      try {
        const response = await http.get('/api/Data/daily-uploads')
        return response.data
      } catch {
        return null
      }
    },
    
    async fetchCategoryDistribution() {
      try {
        const response = await http.get('/api/Data/category-distribution')
        return response.data
      } catch {
        return null
      }
    },
    
    async fetchStatusDistribution() {
      try {
        const response = await http.get('/api/Data/status-distribution')
        return response.data
      } catch {
        return null
      }
    },
    
    async fetchMonthlyUploads() {
      try {
        const response = await http.get('/api/Data/monthly-uploads')
        return response.data
      } catch {
        return null
      }
    },
    
    initCharts() {
      this.disposeCharts()
      
      setTimeout(() => {
        if (this.dailyUploads.dates && this.dailyUploads.dates.length > 0) {
          this.initLineChart()
        }
        if (this.categoryDistribution && this.categoryDistribution.length > 0) {
          this.initPieChart()
        }
        if (this.statusDistribution && this.statusDistribution.length > 0) {
          this.initStatusChart()
        }
        if (this.monthlyUploads.months && this.monthlyUploads.months.length > 0) {
          this.initMonthChart()
        }
      }, 100)
    },
    
    initLineChart() {
      if (!this.$refs.lineChart) return
      this.lineChart = echarts.init(this.$refs.lineChart, null, { renderer: 'canvas' })
      const option = {
        tooltip: { 
          trigger: 'axis',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#e4e7ed',
          borderWidth: 1,
          textStyle: { color: '#333' }
        },
        animationDuration: 800,
        animationEasing: 'cubicOut',
        grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
        xAxis: { 
          type: 'category', 
          boundaryGap: false, 
          data: this.dailyUploads.dates,
          axisLine: { lineStyle: { color: '#e4e7ed' } },
          axisLabel: { color: '#666', fontSize: 12 },
          axisTick: { show: false }
        },
        yAxis: { 
          type: 'value',
          axisLine: { show: false },
          axisLabel: { color: '#666', fontSize: 12 },
          splitLine: { lineStyle: { color: '#f0f0f0', type: 'dashed' } }
        },
        series: [{
          name: '上传数量',
          type: 'line',
          smooth: true,
          symbol: 'circle',
          symbolSize: 6,
          showSymbol: false,
          lineStyle: { width: 2.5, color: '#2D8A6E' },
          itemStyle: { color: '#2D8A6E', borderColor: '#fff', borderWidth: 2 },
          areaStyle: { 
            color: {
              type: 'linear',
              x: 0, y: 0, x2: 0, y2: 1,
              colorStops: [
                { offset: 0, color: 'rgba(45, 138, 110, 0.2)' },
                { offset: 1, color: 'rgba(45, 138, 110, 0.02)' }
              ]
            }
          },
          data: this.dailyUploads.uploads
        }]
      }
      this.lineChart.setOption(option)
    },
    
    initPieChart() {
      if (!this.$refs.pieChart) return
      this.pieChart = echarts.init(this.$refs.pieChart, null, { renderer: 'canvas' })
      const data = this.categoryDistribution.map(item => ({ value: item.count, name: item.category }))
      const colors = ['#2D8A6E', '#1890FF', '#40A9FF', '#69C0FF', '#B37FEB']
      const option = {
        tooltip: { 
          trigger: 'item', 
          formatter: '{a} <br/>{b}: {c} ({d}%)',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#e4e7ed',
          borderWidth: 1,
          textStyle: { color: '#333' }
        },
        animationDuration: 800,
        animationEasing: 'cubicOut',
        legend: { orient: 'vertical', left: 10, textStyle: { color: '#666', fontSize: 12 } },
        color: colors,
        series: [{
          name: '作品类别',
          type: 'pie',
          radius: ['40%', '70%'],
          avoidLabelOverlap: false,
          label: { show: false },
          emphasis: { label: { show: true, fontSize: 16, fontWeight: 'bold', color: '#333' } },
          labelLine: { show: false },
          itemStyle: { borderRadius: 8, borderColor: '#fff', borderWidth: 2 },
          data: data
        }]
      }
      this.pieChart.setOption(option)
    },
    
    initStatusChart() {
      if (!this.$refs.statusChart) return
      this.statusChart = echarts.init(this.$refs.statusChart, null, { renderer: 'canvas' })
      const data = this.statusDistribution.map(item => ({ value: item.count, name: item.status }))
      const colors = ['#52C41A', '#8C8C8C']
      const option = {
        tooltip: { 
          trigger: 'item', 
          formatter: '{a} <br/>{b}: {c} ({d}%)',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#e4e7ed',
          borderWidth: 1,
          textStyle: { color: '#333' }
        },
        animationDuration: 800,
        animationEasing: 'cubicOut',
        legend: { orient: 'vertical', left: 10, textStyle: { color: '#666', fontSize: 12 } },
        color: colors,
        series: [{
          name: '作品状态',
          type: 'pie',
          radius: ['40%', '70%'],
          avoidLabelOverlap: false,
          label: { show: false },
          emphasis: { label: { show: true, fontSize: 16, fontWeight: 'bold', color: '#333' } },
          labelLine: { show: false },
          itemStyle: { borderRadius: 8, borderColor: '#fff', borderWidth: 2 },
          data: data
        }]
      }
      this.statusChart.setOption(option)
    },
    
    initMonthChart() {
      if (!this.$refs.monthChart) return
      this.monthChart = echarts.init(this.$refs.monthChart, null, { renderer: 'canvas' })
      const option = {
        tooltip: { 
          trigger: 'axis',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#e4e7ed',
          borderWidth: 1,
          textStyle: { color: '#333' }
        },
        animationDuration: 800,
        animationEasing: 'cubicOut',
        grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
        xAxis: { 
          type: 'category', 
          data: this.monthlyUploads.months,
          axisLine: { lineStyle: { color: '#e4e7ed' } },
          axisLabel: { color: '#666', fontSize: 12 },
          axisTick: { show: false }
        },
        yAxis: { 
          type: 'value',
          axisLine: { show: false },
          axisLabel: { color: '#666', fontSize: 12 },
          splitLine: { lineStyle: { color: '#f0f0f0', type: 'dashed' } }
        },
        series: [{
          name: '上传数量',
          type: 'bar',
          barWidth: '60%',
          itemStyle: { 
            color: '#2D8A6E',
            borderRadius: [6, 6, 0, 0]
          },
          data: this.monthlyUploads.uploads
        }]
      }
      this.monthChart.setOption(option)
    },
    
    handleResize() {
      if (this.resizeTimer) clearTimeout(this.resizeTimer)
      this.resizeTimer = setTimeout(() => {
        if (this.lineChart) this.lineChart.resize()
        if (this.pieChart) this.pieChart.resize()
        if (this.statusChart) this.statusChart.resize()
        if (this.monthChart) this.monthChart.resize()
      }, 200)
    }
  }
}
</script>

<style scoped>
.data-analysis-container {
  min-height: 100vh;
  background-color: #f5f7fa;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.data-analysis-header {
  padding: 0 24px;
  height: 64px;
  background-color: #fff;
  border-bottom: 1px solid #e8e8e8;
  position: sticky;
  top: 0;
  z-index: 100;
}

.header-content {
  max-width: 1400px;
  height: 100%;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.system-title {
  font-size: 18px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.header-actions .el-button {
  padding: 8px 16px;
}

.stats-section {
  max-width: 1400px;
  margin: 24px auto;
  padding: 0 24px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-card {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 14px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  color: #2D8A6E;
}

.stat-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-number {
  font-size: 26px;
  font-weight: 700;
  color: #1a1a1a;
}

.stat-label {
  font-size: 13px;
  color: #888;
}

.charts-section {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 24px 40px;
}

.charts-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
}

.chart-card {
  border-radius: 12px;
  border: 1px solid #e8e8e8;
  background: #fff;
}

.chart-header {
  padding: 14px 16px;
  border-bottom: 1px solid #f0f0f0;
}

.chart-title {
  font-size: 15px;
  font-weight: 600;
  color: #333;
  margin: 0;
}

.chart {
  width: 100%;
  height: 300px;
  padding: 12px;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 40px 0;
  gap: 16px;
}

@media (max-width: 768px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .charts-grid {
    grid-template-columns: 1fr;
  }
  
  .chart {
    height: 250px;
  }
}
</style>
