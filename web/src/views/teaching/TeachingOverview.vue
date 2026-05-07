<template>
  <div class="teaching-page">
    <section class="hero-card">
      <div>
        <p class="eyebrow">Teaching Collaboration</p>
        <h1>{{ pageTitle }}</h1>
        <p class="hero-copy">{{ pageSubtitle }}</p>
      </div>
      <div class="hero-actions">
        <el-button type="primary" @click="$router.push('/teaching/works')">进入作品管理</el-button>
        <el-button @click="$router.push('/teaching/students')">查看我的学生</el-button>
      </div>
    </section>

    <section class="stats-grid">
      <article v-for="card in statCards" :key="card.key" class="stat-card">
        <p class="stat-label">{{ card.label }}</p>
        <h2>{{ card.value }}</h2>
        <span class="stat-tip">{{ card.tip }}</span>
      </article>
    </section>

    <section class="panel-grid">
      <article class="panel">
        <div class="panel-head">
          <h3>推荐页面</h3>
        </div>
        <div class="link-list">
          <button class="link-card" @click="$router.push('/teaching/students')">
            <strong>我的学生</strong>
            <span>查看学生名单、学号和作品数量</span>
          </button>
          <button class="link-card" @click="$router.push('/teaching/works')">
            <strong>学生作品管理</strong>
            <span>按学生、分类、状态筛选并更新作品状态</span>
          </button>
          <button class="link-card" @click="$router.push('/works/manage')">
            <strong>作品管理</strong>
            <span>继续使用现有作品管理能力处理全站内容</span>
          </button>
        </div>
      </article>

      <article class="panel">
        <div class="panel-head">
          <h3>当前重点</h3>
        </div>
        <ul class="focus-list">
          <li>
            <span class="focus-badge">{{ overview.recentWorkCount || 0 }}</span>
            <div>
              <strong>近 7 天新增作品</strong>
              <p>适合优先检查近期提交内容。</p>
            </div>
          </li>
          <li>
            <span class="focus-badge">{{ overview.draftCount || 0 }}</span>
            <div>
              <strong>未发布作品</strong>
              <p>可进入学生作品页统一处理状态。</p>
            </div>
          </li>
          <li>
            <span class="focus-badge">{{ overview.excellentCount || 0 }}</span>
            <div>
              <strong>优秀作品积累</strong>
              <p>可作为课程成果展示与推荐素材。</p>
            </div>
          </li>
        </ul>
      </article>
    </section>
  </div>
</template>

<script>
import http from '../../utils/http'
import { getUser } from '../../utils/auth'

export default {
  name: 'TeachingOverview',
  data() {
    return {
      overview: {
        studentCount: 0,
        workCount: 0,
        publishedCount: 0,
        excellentCount: 0,
        recentWorkCount: 0,
        draftCount: 0
      }
    }
  },
  computed: {
    pageTitle() {
      const user = getUser()
      return user?.role === 'Admin' ? '教学协同管理' : '教学协同总览'
    },
    pageSubtitle() {
      const user = getUser()
      return user?.role === 'Admin' ? '管理员可查看所有学生数据和作品' : '集中查看你负责学生的作品进度、发布情况和优秀作品沉淀。'
    },
    statCards() {
      return [
        { key: 'students', label: '学生人数', value: this.overview.studentCount, tip: '当前归属你的学生' },
        { key: 'works', label: '作品总数', value: this.overview.workCount, tip: '学生累计上传作品' },
        { key: 'published', label: '已发布', value: this.overview.publishedCount, tip: '已完成发布的作品' },
        { key: 'excellent', label: '优秀作品', value: this.overview.excellentCount, tip: '已标记为优秀的作品' }
      ]
    }
  },
  async mounted() {
    await this.loadOverview()
  },
  methods: {
    async loadOverview() {
      try {
        const { data } = await http.get('/api/TeachingCollaboration/overview')
        this.overview = data
      } catch (error) {
        this.$message.error(error.response?.data?.message || '加载教学协同总览失败')
      }
    }
  }
}
</script>

<style scoped>
.teaching-page {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.hero-card,
.panel,
.stat-card {
  border: 1px solid #d8e2ef;
  border-radius: 20px;
  background: #ffffff;
  box-shadow: 0 18px 50px rgba(25, 58, 100, 0.08);
}

.hero-card {
  display: flex;
  justify-content: space-between;
  gap: 24px;
  padding: 32px;
  background:
    radial-gradient(circle at top right, rgba(227, 112, 54, 0.18), transparent 24%),
    linear-gradient(135deg, #fff8ee 0%, #eef6ff 52%, #ffffff 100%);
}

.eyebrow {
  margin: 0 0 10px;
  color: #8c4a24;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.16em;
  text-transform: uppercase;
}

.hero-card h1 {
  margin: 0 0 12px;
  color: #1d2a3a;
  font-size: 32px;
}

.hero-copy {
  max-width: 560px;
  margin: 0;
  color: #526171;
  line-height: 1.7;
}

.hero-actions {
  display: flex;
  align-items: flex-start;
  gap: 12px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
}

.stat-card {
  padding: 22px;
}

.stat-label {
  margin: 0 0 12px;
  color: #66758a;
  font-size: 13px;
}

.stat-card h2 {
  margin: 0;
  color: #163047;
  font-size: 34px;
}

.stat-tip {
  display: block;
  margin-top: 10px;
  color: #8a98ab;
  font-size: 12px;
}

.panel-grid {
  display: grid;
  grid-template-columns: 1.1fr 0.9fr;
  gap: 16px;
}

.panel {
  padding: 24px;
}

.panel-head h3 {
  margin: 0 0 16px;
  color: #1d2a3a;
  font-size: 20px;
}

.link-list {
  display: grid;
  gap: 12px;
}

.link-card {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 6px;
  padding: 18px;
  border: 1px solid #e3e8ef;
  border-radius: 16px;
  background: linear-gradient(180deg, #fcfdff 0%, #f6f9fc 100%);
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease;
}

.link-card:hover {
  transform: translateY(-2px);
  border-color: #b8cbe4;
  box-shadow: 0 14px 28px rgba(33, 78, 130, 0.1);
}

.link-card strong {
  color: #1d2a3a;
  font-size: 16px;
}

.link-card span {
  color: #647487;
  font-size: 13px;
  line-height: 1.6;
  text-align: left;
}

.focus-list {
  display: grid;
  gap: 14px;
  margin: 0;
  padding: 0;
  list-style: none;
}

.focus-list li {
  display: flex;
  gap: 14px;
  align-items: flex-start;
  padding: 14px 0;
  border-bottom: 1px solid #edf1f5;
}

.focus-list li:last-child {
  border-bottom: 0;
}

.focus-badge {
  min-width: 44px;
  height: 44px;
  border-radius: 14px;
  background: #183b56;
  color: #fff;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
}

.focus-list strong {
  display: block;
  margin-bottom: 4px;
  color: #1d2a3a;
}

.focus-list p {
  margin: 0;
  color: #647487;
  line-height: 1.6;
  font-size: 13px;
}

@media (max-width: 960px) {
  .hero-card,
  .panel-grid {
    grid-template-columns: 1fr;
  }

  .hero-card {
    flex-direction: column;
  }

  .stats-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }

  .hero-actions {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>
