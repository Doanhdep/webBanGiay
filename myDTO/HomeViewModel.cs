namespace MyStore.myDTO
{
    public class HomeViewModel
    {
        public IEnumerable<Product_DTO> LatestProducts { get; set; } = new List<Product_DTO>();
        public IEnumerable<Product_DTO> TopRatedProducts { get; set; } = new List<Product_DTO>();
        public IEnumerable<ProductVoucher_DTO> DiscountedProducts { get; set; } = new List<ProductVoucher_DTO>();
        public IEnumerable<Product_DTO> BestSellingProducts { get; set; } = new List<Product_DTO>(); // Thêm dòng này
    }
}
