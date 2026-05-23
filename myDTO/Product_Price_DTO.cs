using MyStore.Models;

namespace MyStore.myDTO
{
    public class Product_Price_DTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public String  Title  { get; set; }
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public string CategoryName { get; set; }
        public string? SupplierName { get; set; }
        public int SizeId { get; set; }
        public string? TaxName { get; set; }
        public decimal? RateTax { get; set; }
        public string? Color { get; set; }
        public string? WarrantyPeriod { get; set; }

        public string? CountryOfOrigin { get; set; }

        public DateOnly? ManufactureDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Material { get; set; }

        public string? Weight { get; set; }
        public decimal? Discount { get; set; }
        public string? Filename { get; set; } // Ảnh sản phẩm

        public decimal Price_Default { get; set; } // Giá gốc
        public decimal FinalPrice => Price_Default - (Price_Default * (Discount ?? 0) / 100); // Giá sau giảm

        public int Quantity { get; set; }
        public string ShippingCompany { get; set; } // Tên công ty vận chuyển
        public string OrderStatus { get; set; } // Trạng thái đơn hàng
    }
}





