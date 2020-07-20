using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Uility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderDetailsVM OrderDetailsVm { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderDetailsVm = new OrderDetailsVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(oh=>oh.Id == id,
                                                                                includeProperties:"ApplicationUser"),
                OrderDetailsList = _unitOfWork.OrderDetails.GetAll(od=> od.OrderId == id, 
                                                                                includeProperties:"Product")
            };
            return View(OrderDetailsVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderDetailsVm.OrderHeader.Id, includeProperties: "ApplicationUser");

            if (stripeToken != null)
            {
                // process the payment


                var option = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order Id :" + orderHeader.Id,
                    Source = stripeToken

                };

                var service = new ChargeService();

                Charge charge = service.Create(option);

                if (charge.Id == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }

                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderHeader.PaymentDate = DateTime.Now;
                }

                _unitOfWork.Save();
            }

            return RedirectToAction("Details", "Order", new {id = orderHeader.Id});
        }


        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == id);
            orderHeader.OderStatus = SD.OrderStatusInProcess;
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderDetailsVm.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderDetailsVm.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderDetailsVm.OrderHeader.Carrier;
            orderHeader.OderStatus = SD.OrderStatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }


        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions()
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Reason =  RefundReasons.RequestedByCustomer,
                    Charge =  orderHeader.TransactionId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                orderHeader.OderStatus = SD.OrderStatusRefunded;
                orderHeader.PaymentStatus = SD.OrderStatusRefunded;
            }
            else
            {
                orderHeader.OderStatus = SD.OrderStatusCancelled;
                orderHeader.PaymentStatus = SD.OrderStatusCancelled;
            }
            
            

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult UpdateOrderDetails()
        {
            var orderHEaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderDetailsVm.OrderHeader.Id);
            orderHEaderFromDb.Name = OrderDetailsVm.OrderHeader.Name;
            orderHEaderFromDb.PhoneNumber = OrderDetailsVm.OrderHeader.PhoneNumber;
            orderHEaderFromDb.StreetAddress = OrderDetailsVm.OrderHeader.StreetAddress;
            orderHEaderFromDb.City = OrderDetailsVm.OrderHeader.City;
            orderHEaderFromDb.State = OrderDetailsVm.OrderHeader.State;
            orderHEaderFromDb.PostalCode = OrderDetailsVm.OrderHeader.PostalCode;
            if (OrderDetailsVm.OrderHeader.Carrier != null)
            {
                orderHEaderFromDb.Carrier = OrderDetailsVm.OrderHeader.Carrier;
            }
            if (OrderDetailsVm.OrderHeader.TrackingNumber != null)
            {
                orderHEaderFromDb.TrackingNumber = OrderDetailsVm.OrderHeader.TrackingNumber;
            }

            _unitOfWork.Save();
            TempData["Error"] = "Order Details Updated Successfully.";
            return RedirectToAction("Details", "Order", new { id = orderHEaderFromDb.Id });
        }





        #region APi call

        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(o => o.ApplicationUserId == claim.Value,
                    includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(o => o.OderStatus == SD.OrderStatusApproved ||
                                                                 o.OderStatus == SD.OrderStatusInProcess ||
                                                                 o.OderStatus == SD.OrderStatusPending);
                    break;
                case "completed":
                    orderHeaderList = orderHeaderList.Where(o => o.OderStatus == SD.OrderStatusShipped);
                    break;
                case "rejected":
                    orderHeaderList = orderHeaderList.Where(o => o.OderStatus == SD.OrderStatusCancelled ||
                                                                 o.OderStatus == SD.OrderStatusRefunded ||
                                                                 o.OderStatus == SD.PaymentStatusRejected);
                    break;
                default:
                 break;
            }

            return Json(new {data = orderHeaderList});
        }

        #endregion
    }
}
