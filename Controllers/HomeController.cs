using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.myDTO;

namespace MyStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyStoreContext _db;

        public HomeController(ILogger<HomeController> logger, MyStoreContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index(int? id, int? pageNumber)
        {
            var latestProducts = GetLatestProducts(8);
            var topRatedProducts = GetTopRatedProducts(8);
            var discountedProduct = GetDiscountedProducts(8);
            var bestSellingProducts = GetBestSellingProducts(2); // Thêm danh sách sản phẩm bán chạy

            var viewModel = new HomeViewModel
            {
                LatestProducts = latestProducts,
                TopRatedProducts = topRatedProducts,
                DiscountedProducts = discountedProduct,
                BestSellingProducts = bestSellingProducts // Thêm vào ViewModel
            };

            return View(viewModel);
        }



        public List<Product_DTO> GetLatestProducts(int count = 5)
        {
            var latestProducts = _db.Products
                .Include(p => p.ProductPrices)
                .Include(p => p.Category)
                .OrderByDescending(p => p.ProductPrices.Max(pp => pp.CreatedAt)) // Sắp xếp theo ngày mới nhất của giá sản phẩm
                .Take(count)
                .Select(p => new Product_DTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Filename = p.Filename ?? "",
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    SizeId = GetSizeIdByCategory(p.CategoryId),
                    Rating = (byte?)_db.Feedbacks
                        .Where(f => f.ProductPrice.ProductId == p.Id)
                        .Select(f => (double?)f.Star)
                        .Average() ?? 0
                })
                .ToList();

            foreach (var product in latestProducts)
            {
                product.Price_Default = GetPriceByProductIdAndSizeId(product.Id, product.SizeId);
                product.FinalPrice = GetFinalPrice(product.Id, product.SizeId); // Lấy giá cuối cùng
            }

            return latestProducts;
        }








        private static int GetSizeIdByCategory(int categoryId)
        {
            return categoryId switch
            {
                1 => 8,
                2 => 1,
                _ => 19
            };
        }

        private decimal GetPriceByProductIdAndSizeId(int productId, int sizeId)
        {
            var price = _db.ProductPrices
                          .Where(pp => pp.ProductId == productId && pp.SizeId == sizeId)
                          .Select(pp => pp.Price)
                          .FirstOrDefault();

            return price.GetValueOrDefault(0);
        }



        public List<Product_DTO> GetTopRatedProducts(int count = 8)
        {
            var topRatedProducts = _db.Feedbacks
                .Where(f => f.Star == 4 || f.Star == 5)
                .GroupBy(f => f.ProductPrice.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    RatingCount = g.Count(),
                    AvgRating = g.Average(f => (double?)f.Star) ?? 0
                })
                .OrderByDescending(p => p.RatingCount)
                .Take(count)
                .Join(_db.Products,
                      f => f.ProductId,
                      p => p.Id,
                      (f, p) => new Product_DTO
                      {
                          Id = p.Id,
                          Title = p.Title,
                          Filename = p.Filename ?? "",
                          Description = p.Description,
                          CategoryId = p.CategoryId,
                          CategoryName = p.Category.Name,
                          SizeId = GetSizeIdByCategory(p.CategoryId),
                          Rating = (byte?)Math.Round(f.AvgRating, 0)
                      })
                .ToList();

            foreach (var product in topRatedProducts)
            {
                product.Price_Default = GetPriceByProductIdAndSizeId(product.Id, product.SizeId);
                product.FinalPrice = GetFinalPrice(product.Id, product.SizeId); // Lấy giá cuối cùng
            }

            return topRatedProducts;
        }


        public List<Product_DTO> GetBestSellingProducts(int count = 2)
        {
            var bestSellingProducts = _db.OrderDetails
                .GroupBy(od => od.ProductPrice.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(od => od.Quantity) // Tính tổng số lượng đã bán
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(count)
                .Join(_db.Products,
                      od => od.ProductId,
                      p => p.Id,
                      (od, p) => new Product_DTO
                      {
                          Id = p.Id,
                          Title = p.Title,
                          Filename = p.Filename ?? "",
                          Description = p.Description,
                          CategoryId = p.CategoryId,
                          CategoryName = p.Category.Name,
                          SizeId = GetSizeIdByCategory(p.CategoryId)
                      })
                .ToList();

            foreach (var product in bestSellingProducts)
            {
                product.Price_Default = GetPriceByProductIdAndSizeId(product.Id, product.SizeId);
                product.FinalPrice = GetFinalPrice(product.Id, product.SizeId); // Lấy giá cuối cùng
            }

            return bestSellingProducts;
        }




        public List<ProductVoucher_DTO> GetDiscountedProducts(int count = 8)
        {
            var discountedProducts = _db.ProductVouchers
                .Where(pv => pv.ValidFrom <= DateTime.Now && pv.ValidTo >= DateTime.Now) // Chỉ lấy voucher còn hiệu lực
                .Include(pv => pv.ProductPrice)
                    .ThenInclude(pp => pp.Product) // Lấy thông tin sản phẩm
                .Include(pv => pv.Voucher)
                .Select(pv => new ProductVoucher_DTO
                {
                    Id = pv.Id,
                    ProductPriceId = pv.ProductPriceId,
                    VoucherId = pv.VoucherId,
                    Discount = pv.Discount,
                    ValidFrom = pv.ValidFrom,
                    ValidTo = pv.ValidTo,
                    AppliedQuantity = pv.AppliedQuantity,
                    MaxDiscount = pv.MaxDiscount,

                    // Lấy thông tin sản phẩm
                    ProductTitle = pv.ProductPrice.Product.Title,
                    Filename = pv.ProductPrice.Product.Filename ?? "", // Thêm hình ảnh sản phẩm
                    OriginalPrice = pv.ProductPrice.Price ?? 0,
                    ProductPrice = pv.ProductPrice,
                    Voucher = pv.Voucher
                })
                .ToList();

            foreach (var product in discountedProducts)
            {
                var productId = product.ProductPrice?.Product?.Id ?? 0;
                var sizeId = GetSizeIdByCategory(product.ProductPrice?.Product?.CategoryId ?? 0);

                product.Price_Default = GetPriceByProductIdAndSizeId(productId, sizeId);
            }

            return discountedProducts;
        }


        private decimal GetFinalPrice(int productId, int sizeId)
        {
            decimal originalPrice = GetPriceByProductIdAndSizeId(productId, sizeId);

            var discount = _db.ProductVouchers
                              .Where(pv => pv.ProductPrice.ProductId == productId
                                           && pv.ValidFrom <= DateTime.Now
                                           && pv.ValidTo >= DateTime.Now)
                              .OrderByDescending(pv => pv.Discount) // Lấy mức giảm cao nhất
                              .Select(pv => pv.Discount)
                              .FirstOrDefault();

            if (discount > 0)
            {
                return originalPrice - (originalPrice * discount / 100);
            }

            return originalPrice;
        }






        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
