using MyStore.Models;

namespace MyStore.myDTO
{
    public class Product_DTO
    {
        public int? coutProduct { get; set; }
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = null!;
        public string? Filename2 { get; set; }
        public string? Filename { get; set; }
        public string? Description { get; set; }
        public decimal? Price_Default { get; set; } // Giá gốc
        public decimal? DiscountPrice { get; set; } // Giá sau khi giảm
        public string? CategoryName { get; set; }
        public int SizeId { get; set; }
        public DateTime? Date { get; set; }
        public byte? Rating { get; set; }
     
        public decimal? FinalPrice { get; set; } // Giá cuối cùng sau giảm giá
        public ProductVoucher_DTO? DiscountedProduct { get; set; } // Chứa thông tin giảm giá
        public List<int?> SupplierId { get; set; } // Chuyển thành danh sách int?

    }
}
