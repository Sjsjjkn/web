import Vue from 'vue'

/**
 * 全局事件总线 - 用于跨组件通信
 * 用法: 
 *   import eventBus from '@/utils/eventBus'
 *   eventBus.$emit('user-info-updated', userInfo)  // 发送
 *   eventBus.$on('user-info-updated', callback)     // 监听
 */
export const eventBus = new Vue()
export default eventBus
