using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
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

            coverType = _unitOfWork.CoverType.Get(id.GetValueOrDefault());

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
            if (ModelState.IsValid)
            {
                if (coverType.Id == 0)
                {
                    _unitOfWork.CoverType.Add(coverType);
                }
                else
                {
                    _unitOfWork.CoverType.Update(coverType);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(_unitOfWork);
        }

        #region api call

        public IActionResult GetAll()
        {


            return Json(new
            {
                data = _unitOfWork.CoverType.GetAll()
            });

        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var coverType = _unitOfWork.CoverType.Get(id);
            if (coverType == null)
            {
                return Json(new { success = false, message = "Error while dealing" });
            }

            _unitOfWork.CoverType.Remove(coverType);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Successfully Deleted" });
        }

        #endregion

    }
}