using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        private readonly string _tempPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
        private readonly AppDbContext _context;

        public FileController(AppDbContext context)
        {
            _context = context;
            // 确保上传目录和临时目录存在
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
            
            if (!Directory.Exists(_tempPath))
                Directory.CreateDirectory(_tempPath);
        }

        /// <summary>
        /// 从JWT token中获取当前用户ID
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return null;
            }
            return userId;
        }

        /// <summary>
        /// 上传文件分片
        /// </summary>
        /// <returns>上传结果</returns>
        [HttpPost("upload-chunk")]
        public async Task<IActionResult> UploadChunk()
        {
            try
            {
                // 从FormData获取文件
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "文件不能为空" });

                // 从FormData获取其他参数
                if (!Request.Form.TryGetValue("chunkIndex", out var chunkIndexValues) ||
                    !int.TryParse(chunkIndexValues.ToString(), out int chunkIndex))
                    return BadRequest(new { message = "缺少或无效的chunkIndex参数" });

                if (!Request.Form.TryGetValue("totalChunks", out var totalChunksValues) ||
                    !int.TryParse(totalChunksValues.ToString(), out int totalChunks))
                    return BadRequest(new { message = "缺少或无效的totalChunks参数" });

                if (!Request.Form.TryGetValue("md5", out var md5Values) ||
                    string.IsNullOrEmpty(md5Values.ToString()))
                    return BadRequest(new { message = "缺少md5参数" });

                if (!Request.Form.TryGetValue("fileName", out var fileNameValues) ||
                    string.IsNullOrEmpty(fileNameValues.ToString()))
                    return BadRequest(new { message = "缺少fileName参数" });

                var md5 = md5Values.ToString();
                var fileName = fileNameValues.ToString();

                // 创建临时目录（以MD5命名）
                var tempDir = Path.Combine(_tempPath, md5);
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                // 保存分片文件
                var chunkPath = Path.Combine(tempDir, $"chunk_{chunkIndex}.tmp");
                using (var stream = new FileStream(chunkPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new 
                {
                    message = $"分片{chunkIndex}上传成功",
                    chunkIndex = chunkIndex,
                    totalChunks = totalChunks
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"分片上传失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 合并文件分片
        /// </summary>
        /// <param name="request">合并请求</param>
        /// <returns>合并结果</returns>
        [HttpPost("merge-chunks")]
        public async Task<IActionResult> MergeChunks([FromBody] MergeChunksRequest request)
        {
            try
            {
                // 验证参数
                if (string.IsNullOrEmpty(request.Md5) || string.IsNullOrEmpty(request.FileName))
                    return BadRequest(new { message = "MD5和文件名不能为空" });

                // 检查临时目录是否存在
                var tempDir = Path.Combine(_tempPath, request.Md5);
                if (!Directory.Exists(tempDir))
                    return BadRequest(new { message = "临时目录不存在，请先上传分片" });

                // 检查所有分片是否完整
                for (int i = 0; i < request.TotalChunks; i++)
                {
                    var chunkPath = Path.Combine(tempDir, $"chunk_{i}.tmp");
                    if (!System.IO.File.Exists(chunkPath))
                        return BadRequest(new { message = $"分片{i}不存在，文件不完整" });
                }

                // 生成最终文件名（避免重名）
                var fileExtension = Path.GetExtension(request.FileName);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(request.FileName);
                var finalFileName = $"{fileNameWithoutExt}_{System.DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var finalFilePath = Path.Combine(_uploadPath, finalFileName);

                // 合并文件
                using (var finalStream = new FileStream(finalFilePath, FileMode.Create))
                {
                    for (int i = 0; i < request.TotalChunks; i++)
                    {
                        var chunkPath = Path.Combine(tempDir, $"chunk_{i}.tmp");
                        using (var chunkStream = new FileStream(chunkPath, FileMode.Open))
                        {
                            await chunkStream.CopyToAsync(finalStream);
                        }

                        // 删除已合并的分片
                        System.IO.File.Delete(chunkPath);
                    }
                }

                // 删除临时目录
                Directory.Delete(tempDir);

                return Ok(new 
                {
                    message = "文件合并成功",
                    filePath = finalFileName,
                    fullPath = finalFilePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"文件合并失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 检查文件分片上传状态实现断点续传
        /// </summary>
        /// <param name="md5">文件MD5</param>
        /// <param name="totalChunks">总分片数</param>
        /// <returns>上传状态</returns>
        [HttpGet("upload-status")]
        public IActionResult GetUploadStatus(string md5, int totalChunks)
        {
            try
            {
                if (string.IsNullOrEmpty(md5))
                    return BadRequest(new { message = "MD5不能为空" });

                var tempDir = Path.Combine(_tempPath, md5);
                if (!Directory.Exists(tempDir))
                    return Ok(new { exists = false, uploadedChunks = new int[0] });

                // 获取已上传的分片索引
                var uploadedChunks = new List<int>();
                for (int i = 0; i < totalChunks; i++)
                {
                    var chunkPath = Path.Combine(tempDir, $"chunk_{i}.tmp");
                    if (System.IO.File.Exists(chunkPath))
                        uploadedChunks.Add(i);
                }

                return Ok(new 
                {
                    exists = true,
                    uploadedChunks = uploadedChunks.ToArray(),
                    totalChunks = totalChunks
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取上传状态失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 上传预览图（简单上传，不分片）
        /// </summary>
        /// <returns>上传结果</returns>
        [HttpPost("upload-preview")]
        public async Task<IActionResult> UploadPreviewImage()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "文件不能为空" });

                // 验证文件类型
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "仅支持JPG、PNG、GIF格式图片" });

                // 验证文件大小（5MB）
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "文件大小不能超过5MB" });

                // 生成唯一文件名
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                var finalFileName = $"preview_{fileNameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var finalFilePath = Path.Combine(_uploadPath, finalFileName);

                // 保存文件
                using (var stream = new FileStream(finalFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new
                {
                    message = "预览图上传成功",
                    fileName = finalFileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"预览图上传失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 上传头像（简单上传，不分片）
        /// </summary>
        /// <returns>上传结果</returns>
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { message = "未授权" });

                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "文件不能为空" });

                // 验证文件类型
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "仅支持JPG、PNG、GIF格式图片" });

                // 验证文件大小（5MB）
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "文件大小不能超过5MB" });

                // 生成唯一文件名
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                var finalFileName = $"avatar_{userId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var finalFilePath = Path.Combine(_uploadPath, finalFileName);

                // 保存文件
                using (var stream = new FileStream(finalFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 更新用户头像
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    // 删除旧头像
                    if (!string.IsNullOrEmpty(user.Avatar))
                    {
                        var oldAvatarPath = Path.Combine(_uploadPath, user.Avatar);
                        if (System.IO.File.Exists(oldAvatarPath))
                        {
                            System.IO.File.Delete(oldAvatarPath);
                        }
                    }
                    user.Avatar = finalFileName;
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "头像上传成功",
                    fileName = finalFileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"头像上传失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件流</returns>
        [HttpGet("download")]
        [AllowAnonymous]
        public IActionResult DownloadFile(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return BadRequest(new { message = "文件名不能为空" });

                var filePath = Path.Combine(_uploadPath, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    // 精确匹配失败，尝试递归搜索子目录中的精确匹配
                    var foundFile = Directory.GetFiles(_uploadPath, fileName, SearchOption.AllDirectories).FirstOrDefault();
                    if (foundFile == null)
                    {
                        // 仍然找不到，尝试模糊匹配（文件名包含请求的文件名）
                        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        var allFiles = Directory.GetFiles(_uploadPath, "*.*", SearchOption.AllDirectories)
                            .Where(f => Path.GetFileNameWithoutExtension(f).Contains(nameWithoutExt))
                            .FirstOrDefault();
                        if (allFiles != null)
                            filePath = allFiles;
                        else
                            return NotFound(new { message = "文件不存在" });
                    }
                    else
                    {
                        filePath = foundFile;
                    }
                }

                // 获取文件MIME类型
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(filePath, out var contentType))
                    contentType = "application/octet-stream";

                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                // 添加 no-cache 头，防止浏览器缓存导致头像/缩略图更新后仍显示旧文件
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"文件下载失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 计算文件MD5（用于验证）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>MD5哈希值</returns>
        private async Task<string> CalculateFileMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                var hash = await md5.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    /// <summary>
    /// 合并文件分片请求模型
    /// </summary>
    public class MergeChunksRequest
    {
        public string Md5 { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int TotalChunks { get; set; }
    }
}