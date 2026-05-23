using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.myDTO;

public class ProductListViewComponent : ViewComponent
{
    private readonly MyStoreContext _db;

    public ProductListViewComponent(MyStoreContext db)
    {
        _db = db;
    }

    public static int GetSizeIdByCategory(int categoryId) // Đổi từ private -> public
    {
        return categoryId switch
        {
            1 => 8,
            2 => 1,
            _ => 19
        };
    }

    public decimal GetPriceByProductIdAndSizeId(int productId, int sizeId) // Đổi từ private -> public
    {
        var price = _db.ProductPrices
                      .Where(pp => pp.ProductId == productId && pp.SizeId == sizeId)
                      .Select(pp => pp.Price)
                      .FirstOrDefault();

        return price.GetValueOrDefault(0);
    }


    public async Task<IViewComponentResult> InvokeAsync(int count = 9)
    {
        var discountedProducts = GetDiscountedProducts(count);
        return View(discountedProducts);
    }

    public List<Product_Price_DTO> GetDiscountedProducts(int count = 9)
    {
        var discountedProducts = _db.ProductPrices
            .Where(pp => pp.Discount > 0)
            .Include(pp => pp.Product)
            .Include(pp => pp.Product.Category)
            .Select(pp => new Product_Price_DTO
            {
                Id = pp.Id,
                ProductId = pp.Product.Id,
                Title = pp.Product.Title,
                CategoryId = pp.Product.CategoryId,
                CategoryName = pp.Product.Category.Name,
               
                Description = pp.Product.Description,
                Discount = pp.Discount ?? 0,
                Price_Default = pp.Price ?? 0, // Lấy giá gốc
                Filename = pp.Product.Filename // Thêm đường dẫn ảnh
            })
            .OrderByDescending(pp => pp.Discount)
            .Take(count)
            .ToList();

        return discountedProducts;
    }




}
