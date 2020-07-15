using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BulkyBook.Uility
{
    public class TwilioSettings
    {
        public string PhoneNumber { get; set; }
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
       
    }
}
