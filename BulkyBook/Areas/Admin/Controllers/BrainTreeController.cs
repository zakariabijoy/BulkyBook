using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Braintree;
using BulkyBook.Uility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{   
    [Area("Admin")]
    public class BrainTreeController : Controller
    {
        private readonly IBrainTreeGate _brain;

        public BrainTreeController(IBrainTreeGate brain)
        {
            _brain = brain;
        }
        public IActionResult Index()
        {
            var gateway = _brain.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(IFormCollection collection)
        {
            Random rnd = new Random();
            string nonceFormtheClient = collection["payment_method_nonce"];
            
            var request = new TransactionRequest()
            {
                Amount = rnd.Next(1,100),
                PaymentMethodNonce = nonceFormtheClient,
                OrderId = rnd.Next(1000,10000).ToString(),
                Options = new TransactionOptionsRequest()
                {
                    SubmitForSettlement = true
                }
            };

            var gateway = _brain.GetGateway();
            Result<Transaction> result = gateway.Transaction.Sale(request);

            if (result.Target.ProcessorResponseText == "Approved")
            {
                TempData["Success"] =
                    $"Transaction was successful , Transaction Id: {result.Target.Id}, Amount Charged : ${result.Target.Amount}, Order Id: {result.Target.OrderId}";
            }

            return RedirectToAction("Index");

        }
    }
}
