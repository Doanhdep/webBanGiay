//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Http;
//using MyStore.Data;
//using MyStore.Helpers;
//using MyStore.myDTO;
//using MyStore.Models;

//public class CartController : Controller
//{
//    private readonly MyStoreContext db;
//    private readonly string _currentUser; 
//    public CartController(MyStoreContext context, IHttpContextAccessor accessor)
//    {
//        db = context;

        
//        _currentUser = accessor.HttpContext.Session.GetString("Username")??"";

    
//    }

//    public IActionResult Index()
//    {
//        var username = HttpContext.Session.GetString("Username");
//        if (string.IsNullOrEmpty(username))
//        {
//            return RedirectToAction("Login"); 
//        }
//        var user = db.Users.FirstOrDefault(u => u.Username == username);
//        if (user == null)
//        {
//            return RedirectToAction("Login"); 
//        }
//        var order = db.Orders.FirstOrDefault(o => o.UserId == user.Id && o.Status == 0);
//        if (order == null)
//        {
//            return View(new List<CartItem_DTO>()); 
//        }
//        int OrderId = order.Id; 

//        var result = (from c in db.OrderDetails
//                      where c.OrderId == OrderId 
//                      select new CartItem_DTO
//                      {
//                          Id = c.Id,
//                          OrderId = c.OrderId,
//                          Username = username,
//                          ProductName = c.ProductPrice.Product.Title,
//                          Price = c.Price,
//                          Filename = c.ProductPrice.Product.Filename,
//                          Quantity = c.Quantity,
//                          Discount = c.Discount,
//                      }).ToList();

//        return View(result);
//    }
//    [HttpPost]
//    public IActionResult AddToCart(int productId, int quantity = 1)
//    {
//        // 🔹 Kiểm tra đăng nhập
//        var username = HttpContext.Session.GetString("Username");
//        if (string.IsNullOrEmpty(username))
//        {
//            return RedirectToAction("Login");
//        }

//        // 🔹 Lấy UserId từ database
//        var user = db.Users.FirstOrDefault(u => u.Username == username);
//        if (user == null)
//        {
//            return RedirectToAction("Login");
//        }

//        // 🔹 Tìm giỏ hàng hiện tại (Status = 0)
//        var order = db.Orders.FirstOrDefault(o => o.UserId == user.Id && o.Status == 0);

//        // 🔹 Nếu chưa có giỏ hàng, tạo mới
//        if (order == null)
//        {
//            order = new Order
//            {
//                UserId = user.Id,
//                Status = 0, // Giỏ hàng
//                CreatedAt = DateTime.Now
//            };
//            db.Orders.Add(order);
//            db.SaveChanges();
//        }

//        int orderId = order.Id;

//        // 🔹 Kiểm tra sản phẩm đã có trong giỏ hàng chưa
//        //var orderDetail = db.OrderDetails.FirstOrDefault(od => od.OrderId == orderId && od.ProductId == productId);

//        if (orderDetail != null)
//        {
//            // 🔹 Nếu có, cập nhật số lượng
//            orderDetail.Quantity += quantity;
//        }
//        else
//        {
//            // 🔹 Nếu chưa có, thêm mới vào OrderDetails
//            var productPrice = db.ProductPrices.FirstOrDefault(p => p.ProductId == productId);
//            if (productPrice == null)
//            {
//                return NotFound("Sản phẩm không tồn tại");
//            }

//            orderDetail = new OrderDetail
//            {
//                OrderId = orderId,
//                ProductPriceId = productId,
//                Quantity = quantity,
//                //Price = productPrice.Price,
//                Discount = productPrice.Discount
//            };

//            db.OrderDetails.Add(orderDetail);
//        }

//        db.SaveChanges();

//        return RedirectToAction("Index"); // 🔹 Quay lại trang giỏ hàng
//    }




//}
