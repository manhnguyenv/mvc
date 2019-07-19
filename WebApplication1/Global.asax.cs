using log4net;
using System.Configuration;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Config for Log4Net
            log4net.Config.XmlConfigurator.Configure();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //http://www.codingfusion.com/Post/How-to-configure-email-smtp-settings-in-web-config
            //https://codeshare.co.uk/blog/how-to-force-a-net-website-to-use-tls-12/
            //https://stackoverflow.com/questions/51472404/asp-net-failure-sending-mail-after-enabling-tls1-2
            if (ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12) == false)
            {
                ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls12;
            }

            // Create attendees of the meeting
            MailAddressCollection attendees = new MailAddressCollection();

            attendees.Add("recipient@domain.com");

            string errMsg = string.Empty;

            SendMail(attendees, "Test", "Test", false, null, ref errMsg);
        }

        /// <summary>
        /// http://www.codingfusion.com/Post/How-to-configure-email-smtp-settings-in-web-config
        /// </summary>
        /// <param name="ToList"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="IsBodyHtml"></param>
        /// <param name="Attachments"></param>
        /// <returns></returns>
        public static bool SendMail(MailAddressCollection ToList, string Subject, string Body, bool IsBodyHtml, System.Collections.Hashtable Attachments, ref string errMsg)
        {
            try
            {
                MailMessage MailMessageObj = new MailMessage
                {
                    Subject = Subject,
                    Body = Body,
                    BodyEncoding = System.Text.Encoding.UTF8,
                    IsBodyHtml = IsBodyHtml,
                    Priority = MailPriority.High
                };

                SmtpSection SmtpSectionObj = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                NetworkCredential NetworkCredentialObj = new NetworkCredential(SmtpSectionObj.Network.UserName, SmtpSectionObj.Network.Password);

                SmtpClient smtpClient = new SmtpClient
                {
                    Host = SmtpSectionObj.Network.Host, //---- SMTP Host Details.
                    EnableSsl = SmtpSectionObj.Network.EnableSsl, //---- Specify whether host accepts SSL Connections or not.
                    UseDefaultCredentials = true,
                    Credentials = NetworkCredentialObj,
                    Port = 587 //---- SMTP Server port number. This varies from host to host.
                };

                if (Attachments != null)
                {
                    foreach (Attachment AttachmentObj in Attachments.Values)
                    {
                        MailMessageObj.Attachments.Add(AttachmentObj);
                    }
                }
                foreach (MailAddress toAddress in ToList)
                {
                    MailMessageObj.To.Add(toAddress);
                }

                smtpClient.Send(MailMessageObj);

                return true;
            }
            catch (System.Exception ex)
            {
                errMsg = ex.ToString();

                Logger.Log.Error(errMsg);

                return false;
            }
        }
    }

    public class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Logger));

        public static ILog Log
        {
            get { return Logger.log; }
        }
    }
}