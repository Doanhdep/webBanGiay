using MyStore.Models;

namespace MyStore.myDTO
{
    public class Address_DTO
    {
        public int? UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public int? Id { get; set; } // Đảm bảo có Id
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public bool IsDefault { get; set; }
        public int? CityId { get; set; }       // ✅ Thêm ID của thành phố
        public int? DistrictId { get; set; }   // ✅ Thêm ID của quận/huyện
        public int? WardId { get; set; }       // ✅ Thêm ID của phường/xã
    }
}
