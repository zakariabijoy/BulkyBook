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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
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

                if (shoppingCart.Product.Description != null)
                {
                    shoppingCart.Product.Description = SD.ConvertToRawHtml(shoppingCart.Product.Description);


                    if (shoppingCart.Product.Description.Length > 100)
                    {
                        shoppingCart.Product.Description = shoppingCart.Product.Description.Substring(0, 99) + "...";
                    }
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


        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, includeProperties: "Product");
            cart.Count += 1;
            cart.Price = SD.GetPriceBasedOnQuatity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, includeProperties: "Product");


            if (cart.Count == 1)
            {
                var cnt = _unitOfWork.ShoppingCart.GetAll(sC => sC.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(SD.Session_ShoppingCart, cnt - 1);

            }
            else
            {
                cart.Count -= 1;
                cart.Price = SD.GetPriceBasedOnQuatity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

                _unitOfWork.Save();
            }


            return RedirectToAction(nameof(Index));

        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, includeProperties: "Product");


            var cnt = _unitOfWork.ShoppingCart.GetAll(sC => sC.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();

            HttpContext.Session.SetInt32(SD.Session_ShoppingCart, cnt - 1);


            return RedirectToAction(nameof(Index));

        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            ShoppingCartVm = new ShoppingCartVM()
            {
                OrderHeader = new OrderHeader(),
                ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(c=>c.ApplicationUserId == claim.Value, includeProperties:"Product" )
            };

            ShoppingCartVm.OrderHeader.ApplicationUser =
                _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value, includeProperties: "Company");

            foreach (var list in ShoppingCartVm.ShoppingCarts)
            {
                list.Price = SD.GetPriceBasedOnQuatity(list.Count, list.Product.Price,
                    list.Product.Price50, list.Product.Price100);
                ShoppingCartVm.OrderHeader.OrderTotal += (list.Price * list.Count);
            }

            ShoppingCartVm.OrderHeader.Name = ShoppingCartVm.OrderHeader.ApplicationUser.Name;
            ShoppingCartVm.OrderHeader.PhoneNumber = ShoppingCartVm.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVm.OrderHeader.StreetAddress = ShoppingCartVm.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVm.OrderHeader.City = ShoppingCartVm.OrderHeader.ApplicationUser.City;
            ShoppingCartVm.OrderHeader.State = ShoppingCartVm.OrderHeader.ApplicationUser.State;
            ShoppingCartVm.OrderHeader.PostalCode = ShoppingCartVm.OrderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartVm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost( string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVm.OrderHeader.ApplicationUser =
                _unitOfWork.ApplicationUser.GetFirstOrDefault(c => c.Id == claim.Value, includeProperties:"Company");

            ShoppingCartVm.ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == claim.Value, includeProperties:"Product");

            ShoppingCartVm.OrderHeader.OderStatus = SD.OrderStatusPending;
            ShoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVm.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartVm.OrderHeader.OrderDate = DateTime.Now;

            _unitOfWork.OrderHeader.Add(ShoppingCartVm.OrderHeader);
            _unitOfWork.Save();

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();

            foreach (var item in ShoppingCartVm.ShoppingCarts)
            {
                item.Price = SD.GetPriceBasedOnQuatity(item.Count, item.Product.Price, item.Product.Price50,
                                                                                            item.Product.Price100);
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId =item.ProductId,
                    OrderId = ShoppingCartVm.OrderHeader.Id,
                    Count = item.Count,
                    Price = item.Price
                };

                ShoppingCartVm.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
                _unitOfWork.OrderDetails.Add(orderDetails);

            }

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVm.ShoppingCarts);
            _unitOfWork.Save();

            HttpContext.Session.SetInt32(SD.Session_ShoppingCart, 0);

            if (stripeToken == null)
            {

            }
            else
            {
                // process the payment

                var option = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVm.OrderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order Id :" + ShoppingCartVm.OrderHeader.Id,
                    Source = stripeToken

                };

                var service = new ChargeService();

                Charge charge = service.Create(option);

                if (charge.BalanceTransactionId == null)
                {
                    ShoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    ShoppingCartVm.OrderHeader.TransactionId = charge.BalanceTransactionId;
                }

                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVm.OrderHeader.OderStatus = SD.OrderStatusApproved;
                    ShoppingCartVm.OrderHeader.PaymentDate = DateTime.Now;

                }
            }

            _unitOfWork.Save();

            return RedirectToAction("OrderConfirmation", "Cart", new {id = ShoppingCartVm.OrderHeader.Id});
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}