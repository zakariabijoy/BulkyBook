using System;
using System.Collections.Generic;
using System.Text;
using Braintree;
using Microsoft.Extensions.Options;

namespace BulkyBook.Uility
{
    public class BrainTreeGate : IBrainTreeGate
    {
        private readonly BrainTreeSettings _options;

        public IBraintreeGateway BraintreeGateway { get; set; }

        public BrainTreeGate(IOptions<BrainTreeSettings> options)
        {
            _options = options.Value;
        }
        public IBraintreeGateway CreateGateway()
        {
           return new BraintreeGateway(_options.Environment, _options.MerchantID, _options.PublicKey, _options.PrivateKey);
        }

        public IBraintreeGateway GetGateway()
        {
            if (BraintreeGateway == null)
            {
                BraintreeGateway = CreateGateway();
            }

            return BraintreeGateway;
        }
    }
}
