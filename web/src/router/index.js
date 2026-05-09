import Vue from 'vue'
import VueRouter from 'vue-router'
import { getUser, hasRole } from '../utils/auth'

Vue.use(VueRouter)

const originalPush = VueRouter.prototype.push

VueRouter.prototype.push = function push(location, onResolve, onReject) {
  if (onResolve || onReject) {
    return originalPush.call(this, location, onResolve, onReject)
  }

  return originalPush.call(this, location).catch(error => {
    if (error && (error.name === 'NavigationDuplicated' || error.name === 'NavigationAborted')) {
      return error
    }

    return Promise.reject(error)
  })
}

const routes = [
  {
    path: '/',
    redirect: '/auth/login'
  },
  {
    path: '/auth/login',
    name: 'AuthLogin',
    component: () => import('../views/auth/Login.vue'),
    meta: { public: true }
  },
  {
    path: '/auth/register',
    name: 'AuthRegister',
    component: () => import('../views/auth/Register.vue'),
    meta: { public: true }
  },
  {
    path: '/works/explore',
    name: 'WorksExplore',
    component: () => import('../views/works/Home.vue')
  },
  {
    path: '/works/manage',
    name: 'WorksManage',
    component: () => import('../views/works/WorkManagement.vue'),
    meta: { roles: ['Admin', 'Teacher', 'Student'] }
  },
  {
    path: '/works',
    name: 'WorksList',
    component: () => import('../views/works/WorkDisplay.vue')
  },
  {
    path: '/works/:id',
    name: 'WorkDetail',
    component: () => import('../views/works/WorkDetail.vue')
  },
  // 合并后的个人空间（原：个人信息、我的收藏、个人作品集、系统公告）
  {
    path: '/account/space',
    name: 'AccountSpace',
    component: () => import('../views/account/PersonalSpace.vue')
  },
  {
    path: '/profile/:userId',
    name: 'PublicProfile',
    component: () => import('../views/account/PublicProfile.vue'),
    meta: { public: true }
  },
  {
    path: '/account/settings',
    name: 'AccountSettings',
    component: () => import('../views/account/Settings.vue')
  },
  {
    path: '/account/notifications',
    name: 'AccountNotifications',
    component: () => import('../views/account/NotificationList.vue')
  },
  {
    path: '/account/feedback',
    name: 'AccountFeedback',
    component: () => import('../views/account/Feedback.vue')
  },
  {
    path: '/admin/analytics',
    name: 'AdminAnalytics',
    component: () => import('../views/admin/DataAnalysis.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/admin/users',
    name: 'AdminUsers',
      component: () => import('../views/admin/UserManagement.vue'),
    meta: { roles: ['Admin'] }
  },
  {
    path: '/admin/moderation',
    name: 'AdminModeration',
    component: () => import('../views/admin/ContentModeration.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/admin/announcements',
    name: 'AdminAnnouncements',
    component: () => import('../views/admin/AnnouncementManagement.vue'),
    meta: { roles: ['Admin'] }
  },
  {
    path: '/admin/feedbacks',
    name: 'AdminFeedbacks',
    component: () => import('../views/admin/FeedbackManagement.vue'),
    meta: { roles: ['Admin'] }
  },
  {
    path: '/teaching/overview',
    name: 'TeachingOverview',
    component: () => import('../views/teaching/TeachingOverview.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/teaching/students',
    name: 'TeachingStudents',
    component: () => import('../views/teaching/TeachingStudents.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/teaching/works',
    name: 'TeachingWorks',
    component: () => import('../views/teaching/TeachingWorks.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/lab/model-test',
    name: 'LabModelTest',
    component: () => import('../views/lab/ModelTest.vue')
  },
  {
    path: '/not-found',
    name: 'NotFound',
    component: () => import('../views/NotFound.vue')
  },
  {
    path: '*',
    redirect: '/not-found'
  }
]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

// 路由守卫
router.beforeEach((to, from, next) => {
  // 公开页面：不需要登录
  if (to.meta && to.meta.public) return next()

  const user = getUser()
  if (!user) {
    // 未登录：统一跳转到登录页，并记录来源（便于登录后返回）
    return next({ path: '/auth/login', query: { redirect: to.fullPath } })
  }

  // 角色权限：使用 meta.roles 定义
  const roles = to.meta?.roles
  if (roles && roles.length > 0 && !hasRole(roles)) {
    // 无权限：如果已经在用户中心页面，保持不变；否则跳转到首页
    if (from.path.startsWith('/account')) {
      return next(false) // 取消导航，保持当前页面
    }
    return next('/works/explore')
  }

  return next()
})

export default router