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

        public IActionResult Upsert(int? id)
        {
            var  category = new Category();
            if (id == null)
            {
                // this is  for create

                return View(category);
            }

            category = _UnitOfWork.Category.Get(id.GetValueOrDefault());

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    _UnitOfWork.Category.Add(category);
                }
                else
                {
                    _UnitOfWork.Category.Update(category);
                }

                _UnitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        #region Api Calls


        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _UnitOfWork.Category.GetAll();
            return Json(new {data= categories});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var category = _UnitOfWork.Category.Get(id);
            if (category == null)
            {
                return Json(new {success = false, message = "Error while dealing"});
            }

            _UnitOfWork.Category.Remove(category);
            _UnitOfWork.Save();
            return Json(new { success = true, message = "Successfully Deleted" });
        }

        #endregion
    }
}