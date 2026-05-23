using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.myDTO;
using System.Linq;
using System.Security.Claims;

public class AddressController : Controller
{
    private readonly MyStoreContext _context;

    public AddressController(MyStoreContext context)
    {
        _context = context;
    }

    public ActionResult Index()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            Console.WriteLine("Không tìm thấy userId trong Claims.");
            return RedirectToAction("Login", "Account");
        }

        Console.WriteLine($"UserId lấy được từ Claims: {userId}");

        var addresses = _context.Addresses
            .Where(a => a.UserId == userId)
            .Select(a => new Address_DTO
            {
                Id = a.Id,
                UserId = a.UserId,
                FullName = string.IsNullOrEmpty(a.FullName) ? "Chưa nhập tên" : a.FullName,
                PhoneNumber = string.IsNullOrEmpty(a.PhoneNumber) ? "Chưa nhập số điện thoại" : a.PhoneNumber,
                AddressLine = a.AddressLine,
                City = a.City,
                Ward = a.Ward,
                District = a.District,
                IsDefault = a.IsDefault
            }).ToList();

        if (!addresses.Any())
        {
            Console.WriteLine("Không tìm thấy địa chỉ nào cho UserId: " + userId);
        }

        return View(addresses);
    }



    [HttpPost]
    public JsonResult Add(Address_DTO dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            Console.WriteLine("Không tìm thấy userId trong Claims.");
            return Json(new { success = false, message = "Người dùng chưa đăng nhập!" });
        }

        dto.UserId = userId;

        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState không hợp lệ:");
            foreach (var key in ModelState.Keys)
            {
                foreach (var error in ModelState[key].Errors)
                {
                    Console.WriteLine($"Trường: {key} - Lỗi: {error.ErrorMessage}");
                }
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
        }

        try
        {
            // Kiểm tra nếu `IsDefault = true`, thì phải bỏ `IsDefault` của các địa chỉ khác
            if (dto.IsDefault)
            {
                var existingAddresses = _context.Addresses.Where(a => a.UserId == userId).ToList();
                foreach (var address in existingAddresses)
                {
                    address.IsDefault = false;
                }
            }
            else
            {
                // Nếu user chưa có địa chỉ nào, thì đặt địa chỉ đầu tiên làm mặc định
                bool hasAddresses = _context.Addresses.Any(a => a.UserId == userId);
                if (!hasAddresses)
                {
                    dto.IsDefault = true;
                }
            }

            // Tạo địa chỉ mới
            var newAddress = new Address
            {
                UserId = userId,
                FullName = dto.FullName,
                AddressLine = dto.AddressLine,
                PhoneNumber = dto.PhoneNumber,
                City = dto.City,
                District = dto.District,
                Ward = dto.Ward,
                CityId = dto.CityId,
                DistrictId = dto.DistrictId,
                WardId = dto.WardId,
                IsDefault = dto.IsDefault
            };

            _context.Addresses.Add(newAddress);
            _context.SaveChanges();
            Console.WriteLine("Đã lưu địa chỉ vào database.");

            return Json(new { success = true, message = "Thêm địa chỉ thành công!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi lưu vào database: {ex.Message}");
            return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại!" });
        }
    }


    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var address = _context.Addresses.Find(id);
        if (address == null)
        {
            return Json(new { success = false, message = "Địa chỉ không tồn tại." });
        }

        _context.Addresses.Remove(address);
        _context.SaveChanges();

        return Json(new { success = true });
    }
    // Sửa địa chỉ
    [HttpPost]
    public IActionResult Update([FromBody] Address model)
    {
        var address = _context.Addresses.FirstOrDefault(a => a.Id == model.Id);
        if (address == null)
        {
            return Json(new { success = false, message = "Không tìm thấy địa chỉ để cập nhật!" });
        }

        // ✅ Cập nhật cả tên và ID
        address.FullName = model.FullName;
        address.PhoneNumber = model.PhoneNumber;
        address.AddressLine = model.AddressLine;

        address.CityId = model.CityId;  // ✅ Thêm ID
        address.DistrictId = model.DistrictId;  // ✅ Thêm ID
        address.WardId = model.WardId;  // ✅ Thêm ID

        address.City = model.City;  // Lưu cả tên
        address.District = model.District;
        address.Ward = model.Ward;

        address.IsDefault = model.IsDefault;

        // Nếu đặt địa chỉ mặc định, bỏ mặc định các địa chỉ khác
        if (model.IsDefault)
        {
            var userAddresses = _context.Addresses.Where(a => a.UserId == model.UserId);
            foreach (var addr in userAddresses)
            {
                addr.IsDefault = false;
            }
        }

        _context.SaveChanges();
        return Json(new { success = true, message = "Cập nhật thành công!" });
    }



    [HttpGet]
    public JsonResult GetById(int id)
    {
        var address = _context.Addresses
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.FullName,
                a.PhoneNumber,
                a.AddressLine,
                a.CityId,
                a.DistrictId,
                a.WardId,
                a.IsDefault,
                a.Latitude,  // ✅ Thêm vào đây
                a.Longitude  // ✅ Thêm vào đây
            })
            .FirstOrDefault();

        if (address == null)
        {
            return Json(new { success = false, message = "Không tìm thấy địa chỉ!" });
        }

        return Json(new { success = true, data = address });
    }




    // Đặt địa chỉ mặc định
    [HttpPost]
    public JsonResult SetDefault(int id)
    {
        try
        {
            // Lấy UserId từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập!" });
            }

            // Tìm địa chỉ của User cần cập nhật
            var address = _context.Addresses.FirstOrDefault(a => a.Id == id && a.UserId == userId);
            if (address == null)
            {
                return Json(new { success = false, message = "Địa chỉ không tồn tại hoặc không thuộc về bạn!" });
            }

            // Reset tất cả địa chỉ của User thành không mặc định
            var userAddresses = _context.Addresses.Where(a => a.UserId == userId).ToList();
            foreach (var addr in userAddresses)
            {
                addr.IsDefault = false;
            }

            // Đặt địa chỉ được chọn là mặc định
            address.IsDefault = true;
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã cập nhật địa chỉ mặc định!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }


    public ActionResult Map()
    {
        return View();
    }
    [HttpGet]
    public JsonResult GetAddresses()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Json(new { success = false, message = "Người dùng chưa đăng nhập!" });
        }

        Console.WriteLine($"🔹 UserId lấy từ Claims: {userId}");

        var addresses = _context.Addresses
            .Where(a => a.UserId == userId)
            .Select(a => new Address_DTO
            {
                Id = a.Id,
                UserId = a.UserId,
                FullName = a.FullName ?? "Chưa nhập tên",
                PhoneNumber = a.PhoneNumber ?? "Chưa nhập số điện thoại",
                AddressLine = a.AddressLine,
                City = a.City,
                District = a.District,
                Ward = a.Ward,
                IsDefault = a.IsDefault
            }).ToList();

        return Json(new { success = true, addresses });
    }

}
