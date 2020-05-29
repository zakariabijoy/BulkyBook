using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _UnitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            var  productVM = new ProductVM()
            {
                Product = new Product(),
                CategorieList = _UnitOfWork.Category.GetAll().Select(c=> new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                CoverTypesList = _UnitOfWork.CoverType.GetAll().Select(c=> new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };
            if (id == null)
            {
                // this is  for create

                return View(productVM);
            }

            productVM.Product = _UnitOfWork.Product.Get(id.GetValueOrDefault());

            if (productVM.Product == null)
            {
                return NotFound();
            }

            return View(productVM.Product);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.Id == 0)
                {
                    _UnitOfWork.Product.Add(product);
                }
                else
                {
                    _UnitOfWork.Product.Update(product);
                }

                _UnitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        #region Api Calls


        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _UnitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new {data= products });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var product = _UnitOfWork.Product.Get(id);
            if (product == null)
            {
                return Json(new {success = false, message = "Error while dealing"});
            }

            _UnitOfWork.Product.Remove(product);
            _UnitOfWork.Save();
            return Json(new { success = true, message = "Successfully Deleted" });
        }

        #endregion
    }
}