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

/**
 * 获取头像URL
 * @param {string|null} avatar - 头像文件名
 * @returns {string} 头像URL或空字符串
 */
export function getAvatarUrl(avatar) {
  if (!avatar) return ''
  return `/api/File/download?fileName=${encodeURIComponent(avatar)}`
}

/**
 * 根据文件扩展名返回文件图标（Emoji形式）
 * @param {string} fileName - 文件名
 * @returns {string} 文件图标
 */
export function getFileIcon(fileName) {
  if (!fileName) return '📄'
  const ext = fileName.toLowerCase().split('.').pop()
  const iconMap = {
    'pdf': '📕',
    'doc': '📘',
    'docx': '📘',
    'ppt': '📗',
    'pptx': '📗',
    'xls': '📙',
    'xlsx': '📙',
    'zip': '📦',
    'rar': '📦',
    '7z': '📦',
    'jpg': '🖼️',
    'jpeg': '🖼️',
    'png': '🖼️',
    'gif': '🖼️',
    'bmp': '🖼️',
    'webp': '🖼️',
    'svg': '🖼️',
    'mp4': '🎬',
    'avi': '🎬',
    'mov': '🎬',
    'wmv': '🎬',
    'mp3': '🎵',
    'wav': '🎵',
    'flac': '🎵',
    'txt': '📝',
    'md': '📝',
    'html': '🌐',
    'css': '🎨',
    'js': '📜',
    'ts': '📜',
    'json': '📋',
    'py': '🐍',
    'java': '☕',
    'c': '⚙️',
    'cpp': '⚙️',
    'h': '⚙️',
    'glb': '🎲',
    'gltf': '🎲',
    'obj': '🎲',
    'fbx': '🎲',
    'stl': '🎲',
    'dae': '🎲',
    '3ds': '🎲',
    'blend': '🎲'
  }
  return iconMap[ext] || '📄'
}

/**
 * 获取文件扩展名
 * @param {string} fileName - 文件名
 * @returns {string} 扩展名（不含点）
 */
export function getFileExtension(fileName) {
  if (!fileName) return ''
  return fileName.toLowerCase().split('.').pop() || ''
}

/**
 * 3D 模型文件扩展名列表
 */
export const MODEL_EXTENSIONS = ['.glb', '.gltf', '.obj', '.stl', '.fbx', '.dae', '.3ds', '.ply', '.wrl', '.iges', '.igs', '.step', '.stp']

/**
 * 判断文件是否为 3D 模型格式
 * @param {string} fileName - 文件名
 * @returns {boolean}
 */
export function isModelFile(fileName) {
  if (!fileName) return false
  const ext = fileName.substring(fileName.lastIndexOf('.')).toLowerCase()
  return MODEL_EXTENSIONS.includes(ext)
}

/**
 * 获取预览图 URL（若有）
 * @param {object} work - 作品对象
 * @returns {string|null}
 */
export function getPreviewImageUrl(work) {
  if (!work) return null
  const previewFile = work.previewImage || work.preview || null
  if (!previewFile) return null
  return `/api/File/download?fileName=${encodeURIComponent(previewFile)}`
}

/**
 * 获取文件下载 URL
 * @param {object} work - 作品对象
 * @returns {string|null}
 */
export function getFileDownloadUrl(work) {
  if (!work) return null
  const fileName = work.fileName || work.filePath || work.preview || null
  if (!fileName) return null
  return `/api/File/download?fileName=${encodeURIComponent(fileName)}`
}
