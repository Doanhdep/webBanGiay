using MyStore.Data;
using MyStore.myDTO;
using Microsoft.AspNetCore.Mvc;
using MyStore.Models;

namespace MyStore.ViewComponents
{
	public class MenuCategoryViewComponent : ViewComponent
	{
		private readonly MyStoreContext db;

		public MenuCategoryViewComponent(MyStoreContext context) => db = context;
		public IViewComponentResult Invoke()
		{
			var data = db.Categories
				.Select(lo => new Product_DTO
				{
					CategoryId = lo.Id,
					CategoryName = lo.Name,
					coutProduct = lo.Products.Count,
					SupplierId = lo.Products.Select(p => p.SupplierId).Distinct().ToList() // Lấy danh sách SupplierID duy nhất

                })
				.OrderBy(p => p.CategoryName)
				.ToList();

			return View(data);
		}

	}
}