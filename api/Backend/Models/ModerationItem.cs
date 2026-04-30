using System;

namespace Backend.Models
{
    public class ModerationItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // image, video, document, text
        public string FileName { get; set; } = string.Empty;
        public string Preview { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SubmitTime { get; set; } = DateTime.Now;
        public string RiskLevel { get; set; } = string.Empty; // low, medium, high
        public string RiskDetails { get; set; } = string.Empty; // 用|分隔多个风险
        public string Status { get; set; } = "pending"; // pending, approved, rejected
    }
}