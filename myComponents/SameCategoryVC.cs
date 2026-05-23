using Microsoft.AspNetCore.Mvc;
using MyStore.Data;
using MyStore.myDTO;
using System.Linq;

namespace MyStore.myComponents
{
    public class SameCategoryVC : ViewComponent
    {
        private readonly MyStoreContext _db;

        public SameCategoryVC(MyStoreContext context)
        {
            _db = context;
        }

        public IViewComponentResult Invoke(int categoryId, int productId)
        {
            var query = (from pp in _db.ProductPrices
                         join p in _db.Products on pp.ProductId equals p.Id
                         where p.CategoryId == categoryId && p.Id != productId
                         group new { pp, p } by pp.ProductId into g
                         select new Same_Category_DTO
                         {
                             ProductId = g.Key,
                             Price = g.Min(x => x.pp.Price * (1 - (x.pp.Discount ?? 0))),
                             filename = g.First().p.Filename,
                             Discount = g.First().pp.Discount,
                             Title = g.First().p.Title
                         }).ToList().Take(9);

            return View(query);


            
        }
    }
}
