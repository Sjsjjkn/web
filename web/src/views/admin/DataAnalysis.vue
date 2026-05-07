<template>
  <div class="data-analysis-container">
    <header class="data-analysis-header slide-down">
      <div class="header-content">
        <div class="logo-area">
          <div class="logo-icon">BDU</div>
          <h1 class="system-title">数据统计与分析</h1>
        </div>
        <div class="header-actions">
          <el-button
            type="info"
            round
            class="export-btn"
            icon="el-icon-download"
            :loading="exporting"
            @click="handleExport"
          >
            {{ exporting ? '导出中...' : '导出报表' }}
          </el-button>
          <el-button 
            type="primary" 
            round 
            class="refresh-btn"
            icon="el-icon-refresh"
            @click="loadData"
          >
            刷新数据
          </el-button>
        </div>
      </div>
    </header>

    <section class="stats-section fade-in-up">
      <div class="stats-grid">
        <div 
          v-for="(stat, index) in stats" 
          :key="stat.label"
          class="stat-card stagger-fade-in"
          :style="{ animationDelay: `${index * 0.1 + 0.2}s` }"
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
        <div class="chart-item stagger-fade-in" style="animation-delay: 0.4s">
          <el-card class="chart-card" shadow="never">
            <div class="chart-header">
              <h3 class="chart-title">每日上传作品数量</h3>
            </div>
            <div ref="lineChart" class="chart"></div>
          </el-card>
        </div>
        <div class="chart-item stagger-fade-in" style="animation-delay: 0.5s">
          <el-card class="chart-card" shadow="never">
            <div class="chart-header">
              <h3 class="chart-title">作品类别分布</h3>
            </div>
            <div ref="pieChart" class="chart"></div>
          </el-card>
        </div>
        <div class="chart-item stagger-fade-in" style="animation-delay: 0.6s">
          <el-card class="chart-card" shadow="never">
            <div class="chart-header">
              <h3 class="chart-title">作品状态分布</h3>
            </div>
            <div ref="statusChart" class="chart"></div>
          </el-card>
        </div>
        <div class="chart-item stagger-fade-in" style="animation-delay: 0.7s">
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
      totalWorks: 60,
      totalUsers: 35,
      todayWorks: 9,
      totalViews: 1200,
      lineChart: null,
      pieChart: null,
      statusChart: null,
      monthChart: null,
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
      ],
      charts: [
        { title: '每日上传作品数量', ref: 'lineChart' },
        { title: '作品类别分布', ref: 'pieChart' },
        { title: '作品状态分布', ref: 'statusChart' },
        { title: '月度上传趋势', ref: 'monthChart' }
      ]
    }
  },
  mounted() {
    this.loadData()
    window.addEventListener('resize', this.handleResize)
  },
  beforeDestroy() {
    window.removeEventListener('resize', this.handleResize)
    if (this.lineChart) this.lineChart.dispose()
    if (this.pieChart) this.pieChart.dispose()
    if (this.statusChart) this.statusChart.dispose()
    if (this.monthChart) this.monthChart.dispose()
  },
  methods: {
    async handleExport() {
      this.exporting = true
      try {
        // 导出最近7天的统计（可扩展为可配置）
        const res = await http.get('/api/Data/export', { params: { days: 7 }, responseType: 'blob' })
        const blob = new Blob([res.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
        const url = window.URL.createObjectURL(blob)

        // 从响应头里尝试取文件名（后端 File(...) 会带 filename）
        const contentDisposition = res.headers?.['content-disposition'] || res.headers?.['Content-Disposition']
        let fileName = `data-report-${new Date().toISOString().slice(0, 16).replace(/[:T]/g, '-')}.xlsx`
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
        console.error('导出报表失败:', error)
      } finally {
        this.exporting = false
      }
    },
    async loadData() {
      this.loading = true
      try {
        // 数据统计接口需要登录；token 会由统一 http 实例自动注入
        const [overviewData, dailyData, categoryData, statusData, monthlyData] = await Promise.all([
          this.fetchOverview(),
          this.fetchDailyUploads(),
          this.fetchCategoryDistribution(),
          this.fetchStatusDistribution(),
          this.fetchMonthlyUploads()
        ])

        this.totalWorks = overviewData.totalWorks || 60
        this.totalUsers = overviewData.totalUsers || 35
        this.todayWorks = overviewData.todayWorks || 9
        this.totalViews = overviewData.totalViews || 1200
        
        this.stats[0].value = this.totalWorks
        this.stats[1].value = this.totalUsers
        this.stats[2].value = this.todayWorks
        this.stats[3].value = this.totalViews
        
        this.dailyUploads = dailyData || {
          dates: ['2026-04-14', '2026-04-15', '2026-04-16', '2026-04-17', '2026-04-18', '2026-04-19', '2026-04-20'],
          uploads: [5, 8, 12, 7, 10, 6, 9]
        }
        this.categoryDistribution = categoryData || [
          { category: '前端开发', count: 25 },
          { category: '后端开发', count: 18 },
          { category: '人工智能', count: 12 },
          { category: '设计', count: 10 },
          { category: '其他', count: 5 }
        ]
        this.statusDistribution = statusData || [
          { status: '已发布', count: 45 },
          { status: '草稿', count: 15 }
        ]
        this.monthlyUploads = monthlyData || {
          months: ['1月', '2月', '3月', '4月', '5月', '6月'],
          uploads: [20, 35, 45, 55, 60, 45]
        }
        
        this.initCharts()
      } catch (error) {
        console.error('加载统计数据失败，已回退到模拟数据:', error)
        // 使用模拟数据
        this.totalWorks = 60
        this.totalUsers = 35
        this.todayWorks = 9
        this.totalViews = 1200
        
        this.stats[0].value = 60
        this.stats[1].value = 35
        this.stats[2].value = 9
        this.stats[3].value = 1200
        
        this.dailyUploads = {
          dates: ['2026-04-14', '2026-04-15', '2026-04-16', '2026-04-17', '2026-04-18', '2026-04-19', '2026-04-20'],
          uploads: [5, 8, 12, 7, 10, 6, 9]
        }
        this.categoryDistribution = [
          { category: '前端开发', count: 25 },
          { category: '后端开发', count: 18 },
          { category: '人工智能', count: 12 },
          { category: '设计', count: 10 },
          { category: '其他', count: 5 }
        ]
        this.statusDistribution = [
          { status: '已发布', count: 45 },
          { status: '草稿', count: 15 }
        ]
        this.monthlyUploads = {
          months: ['1月', '2月', '3月', '4月', '5月', '6月'],
          uploads: [20, 35, 45, 55, 60, 45]
        }
        
        this.initCharts()
      } finally {
        this.loading = false
      }
    },
    
    async fetchOverview() {
      // 统一走 /api 相对路径，由开发代理转发
      const response = await http.get('/api/Data/overview')
      return response.data
    },
    
    async fetchDailyUploads() {
      const response = await http.get('/api/Data/daily-uploads')
      return response.data
    },
    
    async fetchCategoryDistribution() {
      const response = await http.get('/api/Data/category-distribution')
      return response.data
    },
    
    async fetchStatusDistribution() {
      const response = await http.get('/api/Data/status-distribution')
      return response.data
    },
    
    async fetchMonthlyUploads() {
      const response = await http.get('/api/Data/monthly-uploads')
      return response.data
    },
    
    initCharts() {
      this.$nextTick(() => {
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
        }, 200)
      })
    },
    
    initLineChart() {
      if (!this.$refs.lineChart) return
      if (this.lineChart) {
        this.lineChart.dispose()
      }
      this.lineChart = echarts.init(this.$refs.lineChart)
      const option = {
        tooltip: { 
          trigger: 'axis',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#e4e7ed',
          borderWidth: 1,
          textStyle: { color: '#333' },
          axisPointer: {
            type: 'cross',
            label: { backgroundColor: '#6a7985' }
          }
        },
        grid: { 
          left: '3%', 
          right: '4%', 
          bottom: '3%', 
          containLabel: true 
        },
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
          symbolSize: 8,
          showSymbol: false,
          lineStyle: { 
            width: 3,
            color: 'var(--primary)',
            shadowColor: 'rgba(45, 138, 110, 0.3)',
            shadowBlur: 10
          },
          itemStyle: { 
            color: 'var(--primary)',
            borderColor: '#fff',
            borderWidth: 2
          },
          areaStyle: { 
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: 'rgba(45, 138, 110, 0.3)' },
              { offset: 1, color: 'rgba(45, 138, 110, 0.08)' }
            ])
          },
          data: this.dailyUploads.uploads
        }]
      }
      this.lineChart.setOption(option)
    },
    
    initPieChart() {
      if (!this.$refs.pieChart) return
      if (this.pieChart) {
        this.pieChart.dispose()
      }
      this.pieChart = echarts.init(this.$refs.pieChart)
      const data = this.categoryDistribution.map(item => ({ value: item.count, name: item.category }))
      const colors = ['var(--primary)', '#1890FF', '#40A9FF', '#69C0FF', '#91D5FF', '#B37FEB', '#722ED1']
      const option = {
        tooltip: { 
          trigger: 'item', 
          formatter: '{a} <br/>{b}: {c} ({d}%)',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#e4e7ed',
          borderWidth: 1,
          textStyle: { color: '#333' }
        },
        legend: { 
          orient: 'vertical', 
          left: 10,
          textStyle: { color: '#666', fontSize: 12 }
        },
        color: colors,
        series: [{
          name: '作品类别',
          type: 'pie',
          radius: ['40%', '70%'],
          avoidLabelOverlap: false,
          label: { show: false, position: 'center' },
          emphasis: { 
            label: { show: true, fontSize: 18, fontWeight: 'bold', color: '#333' },
            itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0, 0, 0, 0.2)' }
          },
          labelLine: { show: false },
          itemStyle: {
            borderRadius: 8,
            borderColor: '#fff',
            borderWidth: 2
          },
          data: data
        }]
      }
      this.pieChart.setOption(option)
    },
    
    initStatusChart() {
      if (!this.$refs.statusChart) return
      if (this.statusChart) {
        this.statusChart.dispose()
      }
      this.statusChart = echarts.init(this.$refs.statusChart)
      const data = this.statusDistribution.map(item => ({ value: item.count, name: item.status }))
      const colors = ['#52C41A', '#FAAD14', '#FF4D4F', '#8C8C8C']
      const option = {
        tooltip: { 
          trigger: 'item', 
          formatter: '{a} <br/>{b}: {c} ({d}%)',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#e4e7ed',
          borderWidth: 1,
          textStyle: { color: '#333' }
        },
        legend: { 
          orient: 'vertical', 
          left: 10,
          textStyle: { color: '#666', fontSize: 12 }
        },
        color: colors,
        series: [{
          name: '作品状态',
          type: 'pie',
          radius: ['40%', '70%'],
          avoidLabelOverlap: false,
          label: { show: false, position: 'center' },
          emphasis: { 
            label: { show: true, fontSize: 18, fontWeight: 'bold', color: '#333' },
            itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0, 0, 0, 0.2)' }
          },
          labelLine: { show: false },
          itemStyle: {
            borderRadius: 8,
            borderColor: '#fff',
            borderWidth: 2
          },
          data: data
        }]
      }
      this.statusChart.setOption(option)
    },
    
    initMonthChart() {
      if (!this.$refs.monthChart) return
      if (this.monthChart) {
        this.monthChart.dispose()
      }
      this.monthChart = echarts.init(this.$refs.monthChart)
      const option = {
        tooltip: { 
          trigger: 'axis',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#e4e7ed',
          borderWidth: 1,
          textStyle: { color: '#333' },
          axisPointer: {
            type: 'shadow',
            label: { backgroundColor: '#6a7985' }
          }
        },
        grid: { 
          left: '3%', 
          right: '4%', 
          bottom: '3%', 
          containLabel: true 
        },
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
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: 'var(--primary)' },
              { offset: 1, color: '#85A5FF' }
            ]),
            borderRadius: [6, 6, 0, 0],
            shadowColor: 'rgba(45, 138, 110, 0.3)',
            shadowBlur: 10,
            shadowOffsetY: 4
          },
          emphasis: {
            itemStyle: {
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                { offset: 0, color: '#1890FF' },
                { offset: 1, color: '#40A9FF' }
              ]),
              shadowColor: 'rgba(24, 144, 255, 0.5)',
              shadowBlur: 15,
              shadowOffsetY: 6
            }
          },
          data: this.monthlyUploads.uploads
        }]
      }
      this.monthChart.setOption(option)
    },
    
    handleResize() {
      if (this.lineChart) this.lineChart.resize()
      if (this.pieChart) this.pieChart.resize()
      if (this.statusChart) this.statusChart.resize()
      if (this.monthChart) this.monthChart.resize()
    }
  }
}
</script>

<style scoped>
.data-analysis-container {
  min-height: 100vh;
  background-color: #f8fafc;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  overflow-x: hidden;
}

.data-analysis-header {
  display: flex;
  justify-content: center;
  padding: 0 40px;
  height: 64px;
  background-color: rgba(255,255,255,0.9);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(0,0,0,0.05);
  position: sticky;
  top: 0;
  z-index: 100;
}

.slide-down {
  animation: slideDown 0.5s ease-out;
}

.header-content {
  width: 100%;
  max-width: 1200px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo-area { display: flex; align-items: center; gap: 12px; }
.logo-icon { background: var(--primary); color: white; font-weight: bold; padding: 4px 8px; border-radius: 6px; }
.system-title { font-size: 20px; font-weight: 600; color: #1a1a1a; margin: 0; }

.refresh-btn {
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.refresh-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(45, 138, 110, 0.3);
}

.stats-section {
  max-width: 1200px;
  margin: 40px auto 32px;
  padding: 0 24px;
}

.fade-in-up {
  animation: fadeInUp 0.6s ease-out both;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 24px;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  border: 1px solid rgba(0,0,0,0.05);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}

.stat-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 16px 32px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  width: 64px;
  height: 64px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  color: var(--primary);
  flex-shrink: 0;
}

.stat-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-number {
  font-size: 32px;
  font-weight: 700;
  color: #333;
  line-height: 1.2;
}

.stat-label {
  font-size: 14px;
  color: #666;
}

.charts-section {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px 60px;
}

.charts-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 24px;
}

.stagger-fade-in {
  opacity: 0;
  animation: fadeInUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.chart-card {
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(0,0,0,0.05);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  background: white;
}

.chart-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 16px 32px rgba(0, 0, 0, 0.1);
}

.chart-header {
  padding: 16px 20px;
  border-bottom: 1px solid #f0f0f0;
}

.chart-title {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  margin: 0;
}

.chart {
  width: 100%;
  height: 350px;
  padding: 16px;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 40px 0;
  gap: 16px;
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
  .data-analysis-header {
    padding: 0 20px;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .charts-grid {
    grid-template-columns: 1fr;
  }
  
  .chart {
    height: 300px;
  }
}
</style>