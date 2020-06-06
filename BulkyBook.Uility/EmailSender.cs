using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BulkyBook.Uility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("1e8c185c2383b1", "8ee6408a1d3161"),
                EnableSsl = true
            };
            var mail = new MailMessage();
            mail.IsBodyHtml = true;
            mail.From = new MailAddress("BulkyBook@gmail.com", "BulkyBook");
            mail.To.Add(email);
            mail.Subject = subject;
            mail.Body = htmlMessage;

            return client.SendMailAsync(mail);
        }
    }
}
