using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Net.Mail;
using System.Configuration;

namespace StarReportingCorrection
{
    class SendEmailHelper
    {
        public void SendEmail(string mailBody)
        {
            string strSubject = null;
            strSubject = "STAR Reporting corrections done.";

            string strToAddress = ConfigurationManager.AppSettings.Get("ToAddress").ToString();
            string strFromAddress = ConfigurationManager.AppSettings.Get("FromAddress").ToString();
            MailMessage message = new MailMessage(strFromAddress, strToAddress, strSubject, mailBody);
            message.Priority = MailPriority.High;
            message.IsBodyHtml = true;
            message.Headers.Add("Reply-To", "STARASM@bestbuy.com");
            SmtpClient emailClient = new SmtpClient(ConfigurationManager.AppSettings.Get("SMTPServer").ToString());
            emailClient.Send(message);
        }
    }
}
