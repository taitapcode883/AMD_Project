namespace PasteService.Models
{
    public class Paste
    {
        public int Id { get; set; }
        public string Code { get; set; }        // mã ngắn dùng cho URL, vd "aZ3x9K"
        public string Content { get; set; }
        public string Language { get; set; }
        public string Visibility { get; set; }   // "public" | "private"
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }  // null nếu expiry = "never"
        public int? OwnerId { get; set; }         // null nếu tạo ẩn danh (public paste)
        public int ViewCount { get; set; }
    }
}