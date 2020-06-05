﻿using AuctionOnline.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionOnline.ViewComponents
{
    [ViewComponent(Name = "Navbar")]
    public class NavbarViewComponent : ViewComponent
    {
        private AuctionDbContext db;
        public NavbarViewComponent(AuctionDbContext _category)
        {
            db = _category;
        }
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            ViewBag.NavCategories = db.Categories.FirstOrDefault(a => a.ParentId == id);
            return View("Index", ViewBag.NavCategories);
        }
    }
}
