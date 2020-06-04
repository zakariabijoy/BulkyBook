using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Uility;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }   
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            var coverType = new CoverType();
            if (id == null)
            {
                // create
                return View(coverType);
            }
            // edit
            var parameter = new DynamicParameters();
            parameter.Add("@Id",id);
            coverType = _unitOfWork.SP_Call.OneRecord<CoverType>(SD.Proc_CoverType_Get, parameter);

            if (coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Name",coverType.Name);
            if (ModelState.IsValid)
            {
                if (coverType.Id == 0)
                {
                    _unitOfWork.SP_Call.Execute(SD.Proc_CoverType_Create,parameter );
                }
                else
                {
                    parameter.Add("@Id",coverType.Id);
                    _unitOfWork.SP_Call.Execute(SD.Proc_CoverType_Update,parameter);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(_unitOfWork);
        }

        #region api call

        public IActionResult GetAll()
        {
            var coverTypes = _unitOfWork.SP_Call.List<CoverType>(SD.Proc_CoverType_GetAll,null);

            return Json(new {data = coverTypes});

        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Id",id);
            var coverType = _unitOfWork.SP_Call.OneRecord<CoverType>(SD.Proc_CoverType_Get, parameter);
            if (coverType == null)
            {
                return Json(new { success = false, message = "Error while dealing" });
            }

            _unitOfWork.SP_Call.Execute(SD.Proc_CoverType_Delete,parameter);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Successfully Deleted" });
        }

        #endregion

    }
}