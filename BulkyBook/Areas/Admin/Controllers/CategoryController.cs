using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
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
        public IActionResult Index()
        {
            return View();
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
                return Json(new {success = false, message = "Error while dealing"});
            }

            await _UnitOfWork.Category.RemoveAsync(category);
            _UnitOfWork.Save();
            return Json(new { success = true, message = "Successfully Deleted" });
        }

        #endregion
    }
}