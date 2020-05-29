using System;
using System.Collections.Generic;
using System.IO;
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

            return View(productVM);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(ProductVM productVm)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    var extension = Path.GetExtension(files[0].FileName);

                    if (productVm.Product.ImageUrl != null)
                    {
                        //this is an edit and we need to remove old image
                        var imagepath = Path.Combine(webRootPath, productVm.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagepath))
                        {
                            System.IO.File.Delete(imagepath);
                        }
                    }

                    using (var fileStream =new FileStream(Path.Combine(uploads,fileName+extension),FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVm.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                else
                {
                    // update when they do not change the image
                    if (productVm.Product.Id != 0)
                    {
                        Product objFromDb = _UnitOfWork.Product.Get(productVm.Product.Id);
                        productVm.Product.ImageUrl = objFromDb.ImageUrl;
                    }

                    
                }
                
                
                if (productVm.Product.Id == 0)
                {
                    _UnitOfWork.Product.Add(productVm.Product);
                }
                else
                {
                    _UnitOfWork.Product.Update(productVm.Product);
                }

                _UnitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(productVm);
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