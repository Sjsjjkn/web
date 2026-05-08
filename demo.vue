import React, { useState, useEffect } from 'react';
import { 
  ArrowLeft, 
  Heart, 
  Eye, 
  Download, 
  AlertCircle, 
  Calendar, 
  Tag, 
  Share2,
  FileImage,
  Database
} from 'lucide-react';

export default function WorkDetail() {
  const [isFavorited, setIsFavorited] = useState(false);
  const [favoriteCount, setFavoriteCount] = useState(342);
  const [loading, setLoading] = useState(true);

  // 模拟从你的接口获取的数据
  const work = {
    title: 'Neon Cyberpunk Cityscape Concept',
    category: '3D 渲染与环境艺术',
    author: 'Alex Chen',
    authorRole: '22级 数字媒体艺术学生',
    authorAvatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?q=80&w=150&auto=format&fit=crop',
    coverImage: 'https://images.unsplash.com/photo-1605806616949-1e87b487cb2a?q=80&w=1600&auto=format&fit=crop',
    views: '12.4k',
    createdAt: '2026-05-08',
    fileName: 'Cyberpunk_Final_Render_v2.blend',
    fileSize: '428 MB',
    description: `这是我为《数字环境构建》课程期末项目制作的概念图。
    
使用了 Blender 4.0 进行基础建模和灯光布置，后期导入 Photoshop 进行了色彩映射和局部光晕增强。尝试探索了高对比度霓虹色彩在雨夜环境下的漫反射效果。

核心挑战在于如何平衡画面中繁杂的赛博朋克元素，使视觉中心依然集中在主体建筑上。希望大家喜欢！`,
    tags: ['Blender', 'Photoshop', '赛博朋克', '3D建模', '场景概念', '期末作业'],
  };

  // 模拟加载效果
  useEffect(() => {
    const timer = setTimeout(() => setLoading(false), 800);
    return () => clearTimeout(timer);
  }, []);

  const handleFavorite = () => {
    setIsFavorited(!isFavorited);
    setFavoriteCount(prev => isFavorited ? prev - 1 : prev + 1);
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#F7F7F8] flex items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <div className="w-10 h-10 border-4 border-gray-200 border-t-black rounded-full animate-spin"></div>
          <p className="text-sm font-medium text-gray-500 tracking-widest uppercase">Loading Asset...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#F7F7F8] text-[#111111] font-sans selection:bg-black selection:text-white pb-24">
      
      {/* 顶部简易导航区 */}
      <div className="w-full px-6 py-6 max-w-[1600px] mx-auto">
        <div className="flex items-center gap-3 text-sm font-medium text-gray-500">
          <button className="flex items-center gap-1.5 px-4 py-2 rounded-full bg-white border border-gray-200 shadow-sm hover:border-gray-300 hover:text-black hover:shadow-md transition-all duration-300 transform hover:-translate-x-1 group">
            <ArrowLeft size={16} className="text-gray-400 group-hover:text-black transition-colors" />
            返回画廊
          </button>
          <span className="px-1 opacity-40">/</span>
          <span className="hover:text-black cursor-pointer transition-colors">所有作品</span>
          <span className="px-1 opacity-40">/</span>
          <span className="text-black truncate max-w-[200px] sm:max-w-xs">{work.title}</span>
        </div>
      </div>

      {/* 核心双栏布局：左侧图片展示，右侧信息面板 */}
      <main className="max-w-[1600px] mx-auto px-6 grid grid-cols-1 lg:grid-cols-12 gap-10 xl:gap-16 items-start">
        
        {/* === 左侧：作品视觉预览区 (Sticky 效果) === */}
        <div className="lg:col-span-7 xl:col-span-8 lg:sticky lg:top-8 flex flex-col gap-6">
          {/* 主图容器 */}
          <div className="relative w-full aspect-auto lg:h-[75vh] min-h-[500px] rounded-[2rem] bg-gray-100 border border-gray-200/60 shadow-[0_8px_30px_rgba(0,0,0,0.04)] overflow-hidden group flex items-center justify-center">
            {work.coverImage ? (
              <img 
                src={work.coverImage} 
                alt={work.title} 
                className="w-full h-full object-cover transition-transform duration-700 ease-out group-hover:scale-[1.02]"
              />
            ) : (
              <div className="text-gray-400 flex flex-col items-center gap-3">
                <FileImage size={48} strokeWidth={1} />
                <span className="text-sm font-medium tracking-wider">NO PREVIEW</span>
              </div>
            )}
            
            {/* 渐变遮罩与底部文件信息悬浮条 */}
            <div className="absolute inset-0 bg-gradient-to-t from-black/40 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none" />
            <div className="absolute bottom-6 left-6 right-6 flex items-center justify-between opacity-0 group-hover:opacity-100 transition-opacity duration-300 translate-y-2 group-hover:translate-y-0">
              <div className="flex items-center gap-4 bg-white/20 backdrop-blur-md px-5 py-3 rounded-2xl border border-white/30 text-white">
                <div className="flex items-center gap-2">
                  <Database size={16} className="opacity-80" />
                  <span className="text-sm font-bold tracking-wide">{work.fileSize}</span>
                </div>
                <div className="w-px h-4 bg-white/30"></div>
                <span className="text-sm font-medium opacity-90 truncate max-w-[200px]">{work.fileName}</span>
              </div>
            </div>
          </div>
        </div>

        {/* === 右侧：作品信息与操作区 === */}
        <div className="lg:col-span-5 xl:col-span-4 flex flex-col pt-2 lg:pt-0">
          
          {/* 状态徽标 */}
          <div className="flex items-center gap-3 mb-6">
            <span className="px-3 py-1 bg-green-100 text-green-700 rounded-full text-[11px] font-extrabold tracking-widest uppercase border border-green-200/60">
              已发布
            </span>
            <span className="px-3 py-1 bg-gray-200/60 text-gray-700 rounded-full text-[11px] font-extrabold tracking-widest uppercase">
              {work.category}
            </span>
          </div>

          {/* 标题 */}
          <h1 className="text-4xl sm:text-5xl font-extrabold tracking-tight text-gray-900 mb-8 leading-[1.1]">
            {work.title}
          </h1>

          {/* 作者名片 (Bento Style) */}
          <div className="flex items-center p-2 pr-6 mb-10 bg-white border border-gray-200 shadow-sm rounded-2xl group hover:shadow-md transition-shadow">
            <div className="p-2">
              <img 
                src={work.authorAvatar} 
                alt={work.author} 
                className="w-14 h-14 rounded-xl object-cover shadow-inner group-hover:scale-105 transition-transform" 
              />
            </div>
            <div className="ml-2 flex-1">
              <h3 className="text-base font-bold text-gray-900">{work.author}</h3>
              <p className="text-xs font-medium text-gray-500 mt-0.5">{work.authorRole}</p>
            </div>
            <button className="px-4 py-2 bg-gray-50 hover:bg-gray-100 border border-gray-200 text-gray-700 text-xs font-bold rounded-xl transition-colors">
              查看主页
            </button>
          </div>

          {/* 数据指标 (3-Grid Layout) */}
          <div className="grid grid-cols-3 gap-3 mb-10">
            <div className="bg-white border border-gray-200 rounded-2xl p-4 flex flex-col items-center justify-center text-center shadow-sm">
              <Eye size={20} className="text-gray-400 mb-2" strokeWidth={2} />
              <div className="text-lg font-extrabold text-gray-900">{work.views}</div>
              <div className="text-[10px] font-bold text-gray-400 uppercase tracking-wider mt-1">浏览量</div>
            </div>
            <div className="bg-white border border-gray-200 rounded-2xl p-4 flex flex-col items-center justify-center text-center shadow-sm">
              <Heart size={20} className="text-rose-400 mb-2" strokeWidth={2} />
              <div className="text-lg font-extrabold text-gray-900">{favoriteCount}</div>
              <div className="text-[10px] font-bold text-gray-400 uppercase tracking-wider mt-1">收藏数</div>
            </div>
            <div className="bg-white border border-gray-200 rounded-2xl p-4 flex flex-col items-center justify-center text-center shadow-sm">
              <Calendar size={20} className="text-gray-400 mb-2" strokeWidth={2} />
              <div className="text-sm font-extrabold text-gray-900 leading-tight mt-1">{work.createdAt.split('-')[1]}-{work.createdAt.split('-')[2]}</div>
              <div className="text-[10px] font-bold text-gray-400 uppercase tracking-wider mt-1">{work.createdAt.split('-')[0]}</div>
            </div>
          </div>

          {/* 核心操作按钮 */}
          <div className="flex flex-col gap-3 mb-10">
            <button className="w-full h-14 bg-black text-white rounded-2xl flex items-center justify-center gap-2 font-bold text-base hover:bg-gray-800 hover:shadow-lg hover:shadow-black/20 transition-all duration-300 transform hover:-translate-y-0.5">
              <Download size={20} />
              下载源文件
            </button>
            <div className="flex gap-3">
              <button 
                onClick={handleFavorite}
                className={`flex-1 h-12 rounded-2xl flex items-center justify-center gap-2 font-bold text-sm transition-all duration-300 border shadow-sm ${
                  isFavorited 
                    ? 'bg-rose-50 border-rose-200 text-rose-600' 
                    : 'bg-white border-gray-200 text-gray-700 hover:bg-gray-50'
                }`}
              >
                <Heart size={18} className={isFavorited ? 'fill-current' : ''} />
                {isFavorited ? '已收藏' : '收藏作品'}
              </button>
              <button className="w-12 h-12 bg-white border border-gray-200 rounded-2xl flex items-center justify-center text-gray-500 hover:text-gray-900 hover:bg-gray-50 shadow-sm transition-colors">
                <Share2 size={18} />
              </button>
            </div>
          </div>

          {/* 分割线 */}
          <div className="h-px w-full bg-gray-200/80 mb-10"></div>

          {/* 作品描述 */}
          <div className="mb-10">
            <h3 className="text-sm font-bold text-gray-900 tracking-wide uppercase mb-4 flex items-center gap-2">
              作品介绍
            </h3>
            {work.description ? (
              <div className="prose prose-sm text-gray-600 leading-relaxed max-w-none font-medium whitespace-pre-wrap">
                {work.description}
              </div>
            ) : (
              <div className="p-6 bg-white border border-gray-200 rounded-2xl text-center text-gray-400 text-sm">
                作者很懒，没有留下描述。
              </div>
            )}
          </div>

          {/* 作品标签 */}
          <div className="mb-12">
            <h3 className="text-sm font-bold text-gray-900 tracking-wide uppercase mb-4 flex items-center gap-2">
              标签
            </h3>
            <div className="flex flex-wrap gap-2">
              {work.tags.map((tag, index) => (
                <span 
                  key={index} 
                  className="px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold cursor-pointer transition-colors"
                >
                  #{tag}
                </span>
              ))}
            </div>
          </div>

          {/* 举报按钮 (静默放置在最底部) */}
          <button className="flex items-center gap-2 text-xs font-bold text-gray-400 hover:text-rose-500 transition-colors w-fit">
            <AlertCircle size={14} />
            举报该作品
          </button>

        </div>
      </main>
    </div>
  );
}