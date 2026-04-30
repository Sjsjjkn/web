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
    if (error && error.name === 'NavigationDuplicated') {
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
    component: () => import('../views/Login.vue'),
    meta: { public: true }
  },
  {
    path: '/auth/register',
    name: 'AuthRegister',
    component: () => import('../views/Register.vue'),
    meta: { public: true }
  },
  {
    path: '/works/explore',
    name: 'WorksExplore',
    component: () => import('../views/Home.vue')
  },
  {
    path: '/works/manage',
    name: 'WorksManage',
    component: () => import('../views/WorkManagement.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/works',
    name: 'WorksList',
    component: () => import('../views/WorkDisplay.vue')
  },
  {
    path: '/account/profile',
    name: 'AccountProfile',
    component: () => import('../views/Profile.vue')
  },
  {
    path: '/account/favorites',
    name: 'AccountFavorites',
    component: () => import('../views/Favorites.vue')
  },
  {
    path: '/account/works',
    name: 'AccountWorks',
    component: () => import('../views/MyWorks.vue')
  },
  {
    path: '/admin/analytics',
    name: 'AdminAnalytics',
    component: () => import('../views/DataAnalysis.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/admin/users',
    name: 'AdminUsers',
    component: () => import('../views/Admin.vue'),
    meta: { roles: ['Admin'] }
  },
  {
    path: '/admin/moderation',
    name: 'AdminModeration',
    component: () => import('../views/ContentModeration.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/teaching/overview',
    name: 'TeachingOverview',
    component: () => import('../views/TeachingOverview.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/teaching/students',
    name: 'TeachingStudents',
    component: () => import('../views/TeachingStudents.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/teaching/works',
    name: 'TeachingWorks',
    component: () => import('../views/TeachingWorks.vue'),
    meta: { roles: ['Admin', 'Teacher'] }
  },
  {
    path: '/lab/model-test',
    name: 'LabModelTest',
    component: () => import('../views/ModelTest.vue')
  },
  {
    path: '*',
    redirect: '/works/explore'
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
    // 无权限：回到作品展厅（产品化体验：尽量不要直接报错中断）
    return next('/works/explore')
  }

  return next()
})

export default router
