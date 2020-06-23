using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Uility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public ShoppingCartVM ShoppingCartVm { get; set; }

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVm = new ShoppingCartVM()
            {
                ShoppingCarts = _unitOfWork.ShoppingCart
                                    .GetAll(s => s.ApplicationUserId == claim.Value, 
                                        includeProperties: "Product"),

                OrderHeader = new OrderHeader()
            };

            ShoppingCartVm.OrderHeader.OrderTotal = 0;
            ShoppingCartVm.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                             .GetFirstOrDefault(a => a.Id == claim.Value, 
                                                                 includeProperties: "Company");

            foreach (var shoppingCart in ShoppingCartVm.ShoppingCarts)
            {
                shoppingCart.Price = SD.GetPriceBasedOnQuatity(shoppingCart.Count, shoppingCart.Product.Price,
                                                    shoppingCart.Product.Price50, shoppingCart.Product.Price100);

                ShoppingCartVm.OrderHeader.OrderTotal += (shoppingCart.Price * shoppingCart.Count);

                shoppingCart.Product.Description = SD.ConvertToRawHtml(shoppingCart.Product.Description);

                if (shoppingCart.Product.Description.Length > 100)
                {
                    shoppingCart.Product.Description = shoppingCart.Product.Description.Substring(0, 99) + "...";
                }
            }
                    
            return View(ShoppingCartVm);
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email is empty!");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email sent. Please Check your email");
            return RedirectToAction("Index");
        }
    }
}