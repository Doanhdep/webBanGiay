namespace MyStore.myDTO
{
    public class OrderStatus_DTO
    {
        public int OrderId { get; set; }
        public DateTime EstimatedDelivery { get; set; } // Ngày giao hàng dự kiến
        public string ShippingCompany { get; set; } // Tên công ty vận chuyển
        public string ShippingPhone { get; set; } // Số điện thoại công ty vận chuyển
        public string OrderStatus { get; set; } // Trạng thái đơn hàng
        public string TrackingNumber { get; set; } // Mã Tracking

        public String Title { get; set; }
        public string? Filename { get; set; }
        public decimal Price_Default { get; set; } // Giá gốc
        public decimal? Discount { get; set; }
        public decimal FinalPrice => Price_Default - (Price_Default * (Discount ?? 0) / 100); // Giá sau giảm

        // Thêm danh sách sản phẩm
        public int Quantity { get; set; }
        public List<Product_Price_DTO> Products { get; set; } = new List<Product_Price_DTO>();
    }

}
