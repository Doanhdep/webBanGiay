using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Helpers;
using MyStore.Models;
using MyStore.myDTO;
using System;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MyStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly MyStoreContext db;

        public ProductController(MyStoreContext _content)
        {
            db = _content;
        }
        private IQueryable<Product_DTO> GetProductsWithDetails(int? supplier = null, string query = null)
        {

            var products = db.Products.AsQueryable();
            // Lọc theo Supplier (Nhà cung cấp)
            if (supplier.HasValue)
            {
                products = products.Where(p => p.SupplierId == supplier.Value);
            }
            //  **Lọc theo từ khóa tìm kiếm**
            if (!string.IsNullOrEmpty(query))
            {
                products = products.Where(p => p.Title.Contains(query) || p.Description.Contains(query));
            }
            var productQuery = from p in products
                               join pp in db.ProductPrices
                                   .GroupBy(pp => pp.ProductId)
                                   .Select(g => new { ProductId = g.Key, Price = g.Min(x => x.Price) })
                                   on p.Id equals pp.ProductId into priceJoin
                               from price in priceJoin.DefaultIfEmpty()

                               join f in db.Feedbacks
                                   .Where(f => f.Star >= 1 && f.Star <= 5)
                                   .GroupBy(f => f.ProductPrice.ProductId)
                                   .Select(g => new { ProductId = g.Key, AvgRating = g.Average(f => (double?)f.Star) ?? 0 })
                                   on p.Id equals f.ProductId into feedbackJoin
                               from feedback in feedbackJoin.DefaultIfEmpty()

                               join g in db.Galleries
                                   .GroupBy(g => g.ProductId)
                                   .Select(g => new { ProductId = g.Key, FirstImage = g.OrderBy(x => x.Id).Select(x => x.Filename).FirstOrDefault() })
                                   on p.Id equals g.ProductId into galleryJoin
                               from gallery in galleryJoin.DefaultIfEmpty()

                               select new Product_DTO
                               {
                                   Id = p.Id,
                                   Title = p.Title,
                                   Filename = p.Filename ?? "",
                                   Filename2 = gallery.FirstImage ?? "", // Ảnh từ ProductGallery
                                   Description = p.Description,
                                   CategoryId = p.CategoryId,
                                   CategoryName = p.Category.Name,
                                   Rating = (byte?)Math.Round(feedback.AvgRating, 0),
                                   Price_Default = price.Price ?? 0
                               };

            return productQuery;
        }

        [Route("San-Pham/Giay-ao-quan-dong-ho/{id?}")]
        public IActionResult Index(int? id, int? pageNumber, int? pageSize, string? sortOrder = "asc")
        {
            int defaultPageSize = 6;
            int size = pageSize ?? defaultPageSize;
            int page = pageNumber ?? 1;
            var productPrices = db.ProductPrices.AsQueryable();
            var productQuery = GetProductsWithDetails();
            ViewBag.MaxPriceLimit = productPrices.Any() ? productPrices.Max(p => p.Price) : 1000;
            // 🟢 Sắp xếp giá sản phẩm
            productQuery = sortOrder == "asc"
                ? productQuery.OrderBy(x => x.Price_Default)
                : productQuery.OrderByDescending(x => x.Price_Default);

            // 🟢 Chuyển thành danh sách
            var result = productQuery.Select(x => new Product_DTO
            {
                Id = x.Id,
                Title = x.Title,
                Filename = x.Filename,
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                SizeId = GetSizeIdByCategory(x.CategoryId),
                Price_Default = x.Price_Default,
                Rating = x.Rating,
                Filename2 = x.Filename2
            }).ToList();

            // 🟢 Cấu hình phân trang
            ViewBag.PageSize = size;
            ViewBag.PageSizeList = new List<SelectListItem>
    {
        new SelectListItem { Value = "6", Text = "Show 6", Selected = (size == 6) },
        new SelectListItem { Value = "12", Text = "Show 12", Selected = (size == 12) },
        new SelectListItem { Value = "24", Text = "Show 24", Selected = (size == 24) }
    };

            return View(PaginatedList<Product_DTO>.Create(result, page, size));
        }
        [HttpGet]
        public IActionResult PageList(int? pageNumber, int? pageSize, string query = null, int? supplier = null, decimal? minPrice = null, decimal? maxPrice = null, string? sortOrder = null, int? rating = null)
        {
            int defaultPageSize = 6;
            int size = pageSize ?? defaultPageSize;
            int page = pageNumber ?? 1;


            ViewBag.CurrentSupplier = supplier;
            ViewBag.PageSize = pageSize;
            ViewBag.SortOrder = sortOrder;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SelectedRating = rating;
            ViewBag.SearchQuery = query;
            var products = db.Products.AsQueryable();
            var productPrices = db.ProductPrices.AsQueryable();

            ViewBag.MaxPriceLimit = productPrices.Any() ? productPrices.Max(p => p.Price) : 1000;


            var productQuery = GetProductsWithDetails(supplier, query);
            //  **Lọc theo khoảng giá**
            if (minPrice.HasValue)
            {
                productQuery = productQuery.Where(x => x.Price_Default >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productQuery = productQuery.Where(x => x.Price_Default <= maxPrice.Value);
            }
            // Lọc theo đánh giá (Rating)
            if (rating.HasValue)
            {
                productQuery = productQuery.Where(x => x.Rating >= rating.Value);
            }
            if (sortOrder == "asc")
            {
                productQuery = productQuery.OrderBy(x => x.Price_Default);
            }
            else
            {
                productQuery = productQuery.OrderByDescending(x => x.Price_Default);
            }

            // Chuyển thành danh sách sau khi sắp xếp
            var result = productQuery.Select(x => new Product_DTO
            {
                Id = x.Id,
                Title = x.Title,
                Filename = x.Filename ?? "",
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                SizeId = GetSizeIdByCategory(x.CategoryId),
                Price_Default = x.Price_Default, // 
                Rating = (byte)(x.Rating ?? 0), // Chuyển đổi về byte (giá trị mặc định là 0 nếu null)
                Filename2 = x.Filename2
            }).ToList(); // 


            // Cấu hình phân trang
            ViewBag.PageSize = size;
            ViewBag.PageSizeList = new List<SelectListItem>
            {
                new SelectListItem { Value = "6", Text = "Show 6", Selected = (size == 6) },
                new SelectListItem { Value = "12", Text = "Show 12", Selected = (size == 12) },
                new SelectListItem { Value = "24", Text = "Show 24", Selected = (size == 24) }
            };

            return PartialView("filterbar", PaginatedList<Product_DTO>.Create(result, page, size));
        }

        public IActionResult Search(int? id, int? pageNumber, int? pageSize, string query = null, int? supplier = null, decimal? minPrice = null, decimal? maxPrice = null, string? sortOrder = null, int? rating = null)
        {
            int defaultPageSize = 6;
            int size = pageSize ?? defaultPageSize;
            int page = pageNumber ?? 1;

            ViewBag.SearchQuery = query;
            ViewBag.CurrentSupplier = supplier;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SelectedRating = rating;

            var products = db.Products.AsQueryable();
            var productPrices = db.ProductPrices.AsQueryable();

            ViewBag.MaxPriceLimit = productPrices.Any() ? productPrices.Max(p => p.Price) : 1000;




            var productQuery = GetProductsWithDetails(supplier, query);
            //  **Lọc theo khoảng giá**
            if (minPrice.HasValue)
            {
                productQuery = productQuery.Where(x => x.Price_Default >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productQuery = productQuery.Where(x => x.Price_Default <= maxPrice.Value);
            }

            //  **Lọc theo đánh giá (Rating)**
            if (rating.HasValue)
            {
                productQuery = productQuery.Where(x => x.Rating >= rating.Value);
            }

            //  **Sắp xếp theo giá**
            if (sortOrder == "asc")
            {
                productQuery = productQuery.OrderBy(x => x.Price_Default);
            }
            else
            {
                productQuery = productQuery.OrderByDescending(x => x.Price_Default);
            }

            // **Chuyển thành danh sách sau khi sắp xếp**
            var result = productQuery.Select(x => new Product_DTO
            {
                Id = x.Id,
                Title = x.Title,
                Filename = x.Filename ?? "",
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                SizeId = GetSizeIdByCategory(x.CategoryId),
                Price_Default = x.Price_Default, // 
                Rating = (byte)(x.Rating ?? 0), // Chuyển đổi về byte (giá trị mặc định là 0 nếu null)
                Filename2 = x.Filename2
            }).ToList(); // 

            //  **Cấu hình phân trang**
            ViewBag.PageSize = size;
            ViewBag.PageSizeList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "6", Text = "Show 6", Selected = (size == 6) },
                    new SelectListItem { Value = "12", Text = "Show 12", Selected = (size == 12) },
                    new SelectListItem { Value = "24", Text = "Show 24", Selected = (size == 24) }
                };

            return View(PaginatedList<Product_DTO>.Create(result, page, size));
        }
        [HttpGet]
        public JsonResult GetProductSuggestions(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new List<string>());
            }

            var suggestions = db.Products
                .Where(p => p.Title.Contains(query))
                .OrderBy(p => p.Title)
                .Select(p => p.Title)
                .Take(5) // Giới hạn số lượng gợi ý
                .ToList();

            return Json(suggestions);
        }
        public List<Product_DTO> GetRatedProducts()
        {
            var RatedProducts = db.Feedbacks
                .Where(f => f.Star >= 1 && f.Star <= 5) // Đảm bảo rating hợp lệ
                .GroupBy(f => f.ProductPrice.ProductId) // Nhóm theo ProductId của ProductPrice
                .Select(g => new
                {
                    ProductId = g.Key,
                    RatingCount = g.Count(),
                    AvgRating = g.Average(f => (double?)f.Star) ?? 0 // Trung bình rating
                })
                .OrderByDescending(p => p.RatingCount) // Sắp xếp theo số lượt đánh giá
                .Join(db.Products, // Join với bảng Products để lấy thông tin sản phẩm
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

            // Cập nhật giá mặc định theo SizeId của sản phẩm
            foreach (var product in RatedProducts)
            {
                product.Price_Default = GetPriceByProductIdAndSizeId(product.Id, product.SizeId);
            }

            return RatedProducts;
        }


        private static int GetSizeIdByCategory(int categoryId)
        {
            if (categoryId == 1)
            {
                return 8;
            }
            else if (categoryId == 2)
            {
                return 1;
            }
            else
            {
                return 19;
            }
        }


        private decimal GetPriceByProductIdAndSizeId(int productId, int sizeId)
        {
            var price = db.ProductPrices
                          .Where(pp => pp.ProductId == productId && pp.SizeId == sizeId)
                          .Select(pp => pp.Price)
                          .FirstOrDefault();

            return price ?? 0; // Return 0 if no price found
        }
        public IActionResult FilterByPrice(decimal minPrice, decimal maxPrice)
        {
            var filteredProducts = db.Products
                .Where(p => db.ProductPrices.Any(pp => pp.ProductId == p.Id && pp.Price >= minPrice && pp.Price <= maxPrice))
                .Select(p => new Product_DTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Filename = p.Filename ?? "",
                    CategoryId = p.CategoryId,
                    SizeId = GetSizeIdByCategory(p.CategoryId),
                    Price_Default = GetPriceByProductIdAndSizeId(p.Id, GetSizeIdByCategory(p.CategoryId))
                })
                .ToList();

            return PartialView("ProductItem", filteredProducts);
        }



        public IActionResult Detail(int ProductId = 1)
        {
            var product = db.ProductPrices
                .Include(p => p.Product)
                .Where(p => p.ProductId == ProductId)
                .Select(p => new Product_Price_DTO
                {
                    CategoryId = p.Product.CategoryId,
                    CategoryName = p.Product.Category.Name,
                    SupplierName = p.Product.Supplier.Name,
                    Description = p.Product.Description,
                    Id = p.Id,
                    Title = p.Product.Title,
                    ProductId = p.ProductId,
                    Color = p.Product.Color,
                    CountryOfOrigin = p.Product.CountryOfOrigin,
                    ManufactureDate = p.Product.ManufactureDate,
                    Material = p.Product.Material,
                    WarrantyPeriod = p.Product.WarrantyPeriod,
                    Weight = p.Product.Weight,



                }).FirstOrDefault();


            // pass into Size ,Price min , max  and file_img 
            // List img for product detail
            var listFilename = db.Galleries
                .Where(g => g.ProductId == ProductId)
                .Select(g => g.Filename)
                .ToList();

            // List size for product detail
            var listSize = db.ProductPrices
                 .Where(g => g.ProductId == ProductId)
                .Select(g => g.Size.SizeName)
                .ToList();

            // after discount 
            var pricesAfterDiscount = db.ProductPrices
              .Where(g => g.ProductId == ProductId)
             .Select(g => g.Price * (1 - g.Discount));

            var min_price_afterdis = pricesAfterDiscount.Min();
            var max_price_afterdis = pricesAfterDiscount.Max();

            // Gán vào ViewBag
            ViewBag.listFilename = listFilename;
            ViewBag.listSize = listSize;
            ViewBag.min_price_afterdis = min_price_afterdis;
            ViewBag.max_price_afterdis = max_price_afterdis;

            return View(product);
        }
        // ajax price for size 
        [HttpGet]
        public IActionResult GetPrice(int id, string sizename)
        {
            var productPrice = db.ProductPrices
                .Where(p => p.ProductId == id && p.Size.SizeName == sizename)
               .FirstOrDefault();

            if (productPrice == null)
            {
                return Json(new { success = false });
            }

            return Json(new
            {
                success = true,
                price = productPrice.Price,
                discount = productPrice.Discount
            });
        }
        // ajax for  feedback 
        [HttpGet]
        public IActionResult GetFeedbacks(int? productId, int page = 1, int pageSize = 2, int fillter = 0)
        {
            try
            {
                // Lấy danh sách phản hồi gốc (ParentId == null)
                var query = db.Feedbacks
                    .Where(f => f.ParentId == 0 && f.ProductPrice.ProductId == productId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new FeedBack_DTO
                    {
                        Id = f.Id,
                        Fullname = f.User.Username,
                        Note = f.Note,
                        star = f.Star,
                        CreatedAt = f.CreatedAt,
                        UpdatedAt = f.UpdatedAt,
                        UserId = f.UserId,
                        filenameuser = f.User.Filename ?? "",
                        ProductPriceId = f.ProductPriceId,
                        ProductName = f.ProductPrice.Product.Title,
                        filenames = db.FeedbackImages
                            .Where(img => img.FeedbackId == f.Id)
                            .Select(img => img.FileName)
                            .ToList(),

                        // Lấy danh sách phản hồi (reply)
                        fullnamerely = db.Feedbacks
                            .Where(r => r.ParentId == f.Id)
                            .Select(r => r.User.Fullname)
                            .FirstOrDefault() ?? string.Empty,
                        filenameuserrely = db.Feedbacks
                            .Where(r => r.ParentId == f.Id)
                            .Select(r => r.User.Filename)
                            .FirstOrDefault() ?? string.Empty,

                        noterely = db.Feedbacks
                            .Where(r => r.ParentId == f.Id)
                            .Select(r => r.Note)
                            .FirstOrDefault() ?? string.Empty
                    });
                if (query != null)
                {
                    switch (fillter)
                    {
                        case 0:
                            break;
                        case 1:
                            query = query.Where(q => q.star == 1);
                            break;
                        case 2:
                            query = query.Where(q => q.star == 2);
                            break;
                        case 3:
                            query = query.Where(q => q.star == 3);
                            break;
                        case 4:
                            query = query.Where(q => q.star == 4);
                            break;
                        case 5:
                            query = query.Where(q => q.star == 5);
                            break;
                        case 6:
                            query = query.Where(q => q.filenames.ToList().Count != 0);
                            break;

                    }
                }

                int totalRecords = query.Count();
                var feedbacks = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return Json(new
                {
                    success = true,
                    data = feedbacks,
                    totalRecords = totalRecords,
                    pageSize = pageSize,
                    fillter = fillter,
                    currentPage = page,
                    productId = productId,
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách phản hồi!",
                    error = ex.Message
                });
            }
        }
        // ajax count feedback and rating of product 

        [HttpGet]
        public IActionResult GetRating(int productId)
        {
            var reviews = db.Feedbacks.Where(r => r.ProductPrice.ProductId == productId && r.ParentId == 0).ToList();

            if (reviews.Count == 0)
            {
                return Json(new { success = true, rating = 0, totalReviews = 0 });
            }


            var averageRating = reviews.Average(r => r.Star ?? 0.0);

            return Json(new
            {
                success = true,
                rating = Math.Round(averageRating, 1),
                totalReviews = reviews.Count,
                oneStar = reviews.Count(r => r.Star == 1),
                twoStars = reviews.Count(r => r.Star == 2),
                threeStars = reviews.Count(r => r.Star == 3),
                fourStars = reviews.Count(r => r.Star == 4),
                fiveStars = reviews.Count(r => r.Star == 5),
                Anh = db.FeedbackImages.Where(p => p.Id == productId).Count()

            });
        }





    }
}
