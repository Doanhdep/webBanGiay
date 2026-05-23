using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.myDTO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyStore.Controllers
{
    public class OrderStatusController : Controller
    {
        private readonly MyStoreContext _context;

        public OrderStatusController(MyStoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.ShippingCompany)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductPrice)
                .ThenInclude(pp => pp.Product)
                .Select(o => new OrderStatus_DTO
                {
                    OrderId = o.Id,
                    EstimatedDelivery = o.CreatedAt.HasValue ? o.CreatedAt.Value.AddDays(3) : DateTime.Now.AddDays(3),
                    ShippingCompany = o.ShippingCompany != null ? o.ShippingCompany.Name : "Không có thông tin",
                    OrderStatus = GetOrderStatusDescription(o.Status),
                    TrackingNumber = "BD045903594059",
                    Products = o.OrderDetails.Select(od => new Product_Price_DTO
                    {
                        Id = od.ProductPrice != null ? od.ProductPrice.Id : 0,
                        Price_Default = od.ProductPrice != null ? od.ProductPrice.Price ?? 0 : 0,
                        Filename = od.ProductPrice != null && od.ProductPrice.Product != null
                            ? od.ProductPrice.Product.Filename
                            : string.Empty,
                        Title = od.ProductPrice != null && od.ProductPrice.Product != null
                            ? od.ProductPrice.Product.Title
                            : "Không có sản phẩm",
                        Quantity = od.Quantity
                      
                    }).ToList(),
                })
                .ToListAsync();

            return View(orders);
        }


        [HttpGet]
        public async Task<IActionResult> FilterOrders(string status)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                {
                    return BadRequest("Tham số status không hợp lệ");
                }

                var query = _context.Orders
                    .Include(o => o.ShippingCompany)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductPrice)
                            .ThenInclude(pp => pp.Product)
                    .AsQueryable();

                if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(status, out int statusValue))
                    {
                        query = query.Where(o => o.Status == statusValue);
                    }
                    else
                    {
                        return BadRequest("Giá trị status không hợp lệ");
                    }
                }


                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new OrderStatus_DTO
                    {
                        OrderId = o.Id,
                        EstimatedDelivery = o.CreatedAt.HasValue
                            ? o.CreatedAt.Value.AddDays(3)
                            : DateTime.Now.AddDays(3),
                        ShippingCompany = o.ShippingCompany != null
                            ? o.ShippingCompany.Name
                            : "Không có thông tin",
                        OrderStatus = GetOrderStatusDescription(o.Status),

                        Products = o.OrderDetails.Select(od => new Product_Price_DTO
                        {
                            Id = od.ProductPrice != null ? od.ProductPrice.Id : 0,
                            Price_Default = od.ProductPrice != null ? od.ProductPrice.Price ?? 0 : 0,
                            Filename = od.ProductPrice != null && od.ProductPrice.Product != null
                                ? od.ProductPrice.Product.Filename
                                : string.Empty,
                            Title = od.ProductPrice != null && od.ProductPrice.Product != null
                                ? od.ProductPrice.Product.Title
                                : "Không có sản phẩm",
                            Quantity = od.Quantity
                        }).ToList()
                    })
                    .ToListAsync();

                return PartialView("Order_Status", orders);
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây nếu cần
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi xử lý yêu cầu",
                    error = ex.Message
                });
            }
        }


        // Hàm chuyển đổi mã trạng thái sang mô tả
        private static string GetOrderStatusDescription(int? status)
        {
            return status switch
            {
                0 => "Chờ Xử Lý",
                1 => "Đã Thanh Toán",
                2 => "Đang Vận Chuyển",
                3 => "Chờ Giao Hàng",
                4 => "Hoàn Thành",
                5 => "Đã Hủy",
                6 => "Trả Hàng/Hoàn Tiền",
                _ => "Không Xác Định"
            };
        }


        //hàm gửi feedback
        [HttpPost]
        public async Task<IActionResult> PostFeedback([FromForm] FeedbackRequest model)
        {
            Console.WriteLine($"Nhận đánh giá: {model.Note}, {model.Star} sao, sản phẩm: {model.ProductPriceId}");
            Console.WriteLine($"Số lượng ảnh nhận được: {model.Images?.Count ?? 0}");

            if (string.IsNullOrWhiteSpace(model.Note))
            {
                return BadRequest(new { message = "Nội dung đánh giá không được để trống!" });
            }

            if (model.Star < 1 || model.Star > 5)
            {
                return BadRequest(new { message = "Số sao không hợp lệ!" });
            }

            if (model.ProductPriceId == 0)
            {
                return BadRequest(new { message = "Không tìm thấy sản phẩm để đánh giá!" });
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var feedback = new Feedback
                    {
                        Note = model.Note,
                        Star = model.Star,
                        UserId = model.UserId,
                        ProductPriceId = model.ProductPriceId,
                        ParentId = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Feedbacks.Add(feedback);
                    await _context.SaveChangesAsync();

                    // 🛑 Kiểm tra danh sách ảnh trước khi lưu
                    if (model.Images != null && model.Images.Count > 0)
                    {
                        var feedbackImages = new List<FeedbackImage>();

                        foreach (var file in model.Images)
                        {
                            Console.WriteLine($"Xử lý ảnh: {file.FileName}");

                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine("wwwroot/Image", fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                feedbackImages.Add(new FeedbackImage
                                {
                                    FeedbackId = feedback.Id,
                                    FileName = fileName
                                });
                            }
                        }

                        _context.FeedbackImages.AddRange(feedbackImages);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine("Không có ảnh nào được gửi!");
                    }

                    transaction.Commit();
                    return Ok(new { message = "Đánh giá đã được lưu thành công!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, new { message = "Có lỗi xảy ra!", error = ex.Message });
                }
            }
        }


    }
}