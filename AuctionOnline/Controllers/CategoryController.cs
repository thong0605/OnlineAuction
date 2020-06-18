﻿using AuctionOnline.Data;
using AuctionOnline.Models;
using AuctionOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionOnline.Controllers
{
    public class CategoryController : Controller
    {
        private AuctionDbContext db;
        public CategoryController(AuctionDbContext _category)
        {
            db = _category;
        }

        public async Task<IActionResult> Index()
        {
            var allCategory = db.Categories.Include(c => c.Parent);
            //Map model to viewmodels
            var viewmodel = new ViewModel()
            {
                CategoryViewModel = await allCategory.ToListAsync()
            };
            return View(viewmodel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await db.Categories
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            var categoryVM = new CategoryVM
            {
                Name = category.Name,
                ParentId = category.ParentId,
                CreatedAt = category.CreatedAt

            };

            return View(categoryVM);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["ParentId"] = new SelectList(db.Categories, "Id", "Id");
            var cateVM = new CategoryVM();
            cateVM.Categories = db.Categories.Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.Id.ToString(),
                                      Text = a.Name
                                  }).ToList();
            return View(cateVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(/*[Bind("Id,Name,CreatedAt,ParentId")]*/ CategoryVM categoryVM)
        {

            ViewBag.CategoryAdd = "Failed";
            categoryVM.CreatedAt = DateTime.Now;
            if (categoryVM != null)
            {
                var category = new Category
                {
                    Name = categoryVM.Name,
                    ParentId = categoryVM.ParentId,
                    CreatedAt = categoryVM.CreatedAt
                };
                db.Add(category);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(db.Categories, "Id", "Id", categoryVM.ParentId);
            return View(categoryVM);
        }

        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var category = await db.Categories.FindAsync(id);
            var categoryVM = new CategoryVM()
            {

                Name = category.Name,
                ParentId = category.ParentId,
                CreatedAt = category.CreatedAt

            };
            categoryVM.Categories = db.Categories.Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.Id.ToString(),
                                      Text = a.Name
                                  }).ToList();
            if (category == null)
            {
                return NotFound();
            }


            ViewData["ParentId"] = new SelectList(db.Categories, "Id", "Id", category.ParentId);
            return View(categoryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, /*[Bind("Id,Name,CreatedAt,ParentId")]*/ CategoryVM categoryVM, Category category)
        {
            //Map model to viewmodels
            category.Name = categoryVM.Name;
            category.CreatedAt = categoryVM.CreatedAt;
            category.ParentId = categoryVM.ParentId;

            if (id != category.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    db.Update(category);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                    }

                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(db.Categories, "Id", "Id", categoryVM.ParentId);
            return View(categoryVM);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await db.Categories
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            var categoryVM = new CategoryVM()
            {
                Name = category.Name,
                CreatedAt = category.CreatedAt,
                ParentId = category.ParentId
            };

            return View(categoryVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await db.Categories.FindAsync(id);

            //delete related category - item table
            var categoryitems = db.CategoryItems.Where(c => c.CategoryId == id);
            foreach (var categoryitem in categoryitems)
            {
                category.CategoryItems.Remove(categoryitem);
            }

            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Route("model")]
        public IActionResult Model()
        {
            var categories = db.Categories.ToList();
            return View("Index", categories);
        }

        public IActionResult GetallMenu()
        {
            List<Category> category = new List<Category>();
            List<Category> categories = db.Categories.ToList();
            category = categories
                .Where(c => c.ParentId == null)
                .Select(c => new Category()
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    Children = GetChildren(categories, c.Id)
                }).ToList();

            return View(category);
        }

        public static List<Category> GetChildren(List<Category> categories, int parentId)
        {
            return categories
                .Where(c => c.ParentId == parentId)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    Parent = c,
                    Children = GetChildren(categories, c.Id)
                }).ToList();
        }
        private bool CategoryExists(int id)
        {
            return db.Categories.Any(e => e.Id == id);
        }
    }
}