/**
 * 文件类型工具函数
 */

/**
 * 根据文件类型返回对应的 Element UI 图标类名
 * @param {string} fileType - 文件类型 (image, video, audio, document, model, code, other)
 * @returns {string} Element UI 图标类名
 */
export function getWorkIcon(fileType) {
  const iconMap = {
    image: 'el-icon-picture',
    video: 'el-icon-video-play',
    audio: 'el-icon-headset',
    document: 'el-icon-document',
    model: 'el-icon-cpu',
    code: 'el-icon-tickets',
    other: 'el-icon-folder'
  }
  return iconMap[fileType] || 'el-icon-folder'
}

/**
 * 根据文件类型返回中文标签
 * @param {string} fileType - 文件类型
 * @returns {string} 中文标签
 */
export function getFileTypeLabel(fileType) {
  const labelMap = {
    image: '图片',
    video: '视频',
    audio: '音频',
    document: '文档',
    model: '模型',
    code: '代码',
    other: '其他'
  }
  return labelMap[fileType] || '其他'
}