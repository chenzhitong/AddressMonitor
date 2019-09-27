using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AddressMonitor
{
    public static class Email
    {
        public static void Send(string to, string subject, string body)
        {
            try
            {
                using MailMessage mail = new MailMessage
                {
                    From = new MailAddress("chenzhitong@ngd.neo.org", "Chen, Zhitong"),
                    Subject = subject,
                    BodyEncoding = Encoding.UTF8,
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(to);
                using SmtpClient smtp = new SmtpClient("smtp.office365.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                var config = JObject.Parse(File.ReadAllText("config.json"));
                smtp.Credentials = new NetworkCredential(config["email"]["username"].ToString(), config["email"]["password"].ToString());
                smtp.Send(mail);
                Console.WriteLine($"邮件发送成功 {to}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
