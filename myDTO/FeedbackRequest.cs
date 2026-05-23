namespace MyStore.myDTO
{
    public class FeedbackRequest
    {
        public string Note { get; set; }
        public int Star { get; set; }
        public int UserId { get; set; }
        public int ProductPriceId { get; set; }
        public List<IFormFile> Images { get; set; } // Nhận danh sách ảnh tải lên
    }
}
