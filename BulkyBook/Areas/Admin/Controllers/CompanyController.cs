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
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            var  company = new Company();
            if (id == null)
            {
                // this is  for create

                return View(company);
            }

            //this is for edit
            company = _UnitOfWork.Company.Get(id.GetValueOrDefault());

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _UnitOfWork.Company.Add(company);
                }
                else
                {
                    _UnitOfWork.Company.Update(company);
                }

                _UnitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        #region Api Calls


        [HttpGet]
        public IActionResult GetAll()
        {
            var companies = _UnitOfWork.Company.GetAll();
            return Json(new {data= companies});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var company = _UnitOfWork.Company.Get(id);
            if (company == null)
            {
                return Json(new {success = false, message = "Error while dealing"});
            }

            _UnitOfWork.Company.Remove(company);
            _UnitOfWork.Save();
            return Json(new { success = true, message = "Successfully Deleted" });
        }

        #endregion
    }
}