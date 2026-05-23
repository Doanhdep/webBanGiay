using MyStore.Models;

namespace MyStore.myDTO
{
    public class CartItem_DTO
    {
        public int Id { get; set; } // DetailId  
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string? Username { get; set; }
        public string? Filename { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; }
        public decimal TotalPrice => Quantity * Price;

    }
}
