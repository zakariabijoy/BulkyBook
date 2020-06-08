using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Uility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public IActionResult Index()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _unitOfWork.ShoppingCart
                    .GetAll(s => s.ApplicationUserId == claim.Value)
                    .ToList().Count();
               
                HttpContext.Session.SetInt32(SD.Session_ShoppingCart, count);
            }

            return View(productList);
        }

        public IActionResult Details(int id)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(p=>p.Id == id,includeProperties: "Category,CoverType");
                          
            var cart = new ShoppingCart()
            {
                Product = product,
                ProductId = product.Id
            };
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            
            if (ModelState.IsValid)
            {
                //then we will add to cart
                var claimsIdentity = (ClaimsIdentity) User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.ApplicationUserId = claim.Value;

                var shoppingCartFromDb = _unitOfWork.ShoppingCart
                    .GetFirstOrDefault(
                        s => s.ApplicationUserId == shoppingCart.ApplicationUserId &&
                             s.ProductId == shoppingCart.ProductId, includeProperties: "Product");
                if (shoppingCartFromDb == null)
                {
                    //no records exists in database for that product for that user, so we create new data 
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                }
                else
                {
                    shoppingCartFromDb.Count += shoppingCart.Count;

                   // _unitOfWork.ShoppingCart.Update(shoppingCartFromDb);
                }

                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == shoppingCart.ApplicationUserId)
                    .ToList().Count();
                // HttpContext.Session.SetObject(SD.Session_ShoppingCart, shoppingCartFromDb);     // we can store an object into session by using SetObject (session extension method)
                HttpContext.Session.SetInt32(SD.Session_ShoppingCart, count);

                return RedirectToAction("Index");

            }
            else
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == shoppingCart.ProductId, includeProperties: "Category,CoverType");

                var cart = new ShoppingCart()
                {
                    Product = product,
                    ProductId = product.Id
                };

                return View(cart);
            }
            
            
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
