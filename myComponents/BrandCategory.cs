using Microsoft.AspNetCore.Mvc;
using MyStore.Data;
using MyStore.myDTO;

public class BrandCategory : ViewComponent
{
    private readonly MyStoreContext db;

    public BrandCategory(MyStoreContext context) => db = context;

    public IViewComponentResult Invoke(List<int?> supplierId)
    {
        if (supplierId == null || !supplierId.Any())
        {
            return View(new List<Supplier_DTO>()); // Trả về danh sách rỗng nếu không có supplierId nào
        }

        var data = db.Suppliers
            .Where(s => supplierId.Contains(s.Id)) // Chỉ lấy những supplier có trong danh sách
            .Select(lo => new Supplier_DTO
            {
                Name = lo.Name,
                Id = lo.Id,
                coutSupplier = lo.Products.Count
            })
            .OrderBy(p => p.Id)
            .ToList();

        return View(data);
    }
}
