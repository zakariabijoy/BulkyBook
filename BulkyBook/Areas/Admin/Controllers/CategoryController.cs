using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Uility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(int productPage = 1)
        {
            CategoryVm categoryVm = new CategoryVm()
            {
                Categories = await _UnitOfWork.Category.GetAllAsync()
            };

            var count = categoryVm.Categories.Count();
            categoryVm.Categories =
                categoryVm.Categories.OrderBy(c => c.Name).Skip((productPage - 1) * 2).Take(2).ToList();

            categoryVm.PagingInfo = new PagingInfo()
            {
                CurrentPage = productPage,
                ItemPerPage = 2,
                TotalItem = count,
                UrlParam = "/Admin/Category/Index?productPage=:"
            };

            return View(categoryVm);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            var  category = new Category();
            if (id == null)
            {
                // this is  for create

                return View(category);
            }

            category = await _UnitOfWork.Category.GetAsync(id.GetValueOrDefault());

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    await _UnitOfWork.Category.AddAsync(category);
                }
                else
                {
                   await _UnitOfWork.Category.UpdateAsync(category);
                }

                _UnitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        #region Api Calls


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _UnitOfWork.Category.GetAllAsync();
            return Json(new {data= categories});
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _UnitOfWork.Category.GetAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Error while deleting category";
                return Json(new {success = false, message = "Error while deleting"});
            }

            await _UnitOfWork.Category.RemoveAsync(category);
            _UnitOfWork.Save();

            TempData["Success"] = "Category Successfully Deleted";
            return Json(new { success = true, message = "Successfully Deleted" });
        }

        #endregion
    }
}