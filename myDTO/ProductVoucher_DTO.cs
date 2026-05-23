using MyStore.Models;

namespace MyStore.myDTO
{
    public class ProductVoucher_DTO
    {
        public int Id { get; set; }
        public int ProductPriceId { get; set; }
        public int VoucherId { get; set; }

        public decimal Discount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int? AppliedQuantity { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? Price_Default { get; set; } // get to  product_price size anySize 

        // Thông tin sản phẩm
        public string ProductTitle { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice => OriginalPrice - (OriginalPrice * Discount / 100); // Giá sau giảm

        public virtual ProductPrice ProductPrice { get; set; } = null!;
        public virtual Voucher Voucher { get; set; } = null!;
        public int SizeId { get; set; }
        public string? Filename { get; set; }

    }

}
