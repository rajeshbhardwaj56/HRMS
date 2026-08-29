
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
namespace HRMSConsoleApp.Utilities
{
 

        public class EmailSender
        {
            public static IConfiguration configuration;
            public EmailSender(IConfiguration _configuration)
            {
                configuration = _configuration;
            }

            public static emailSendResponse SendEmail(sendEmailProperties emailProperties)
            {
                emailSendResponse response = new emailSendResponse();
                try
                {
                    MailMessage message = new MailMessage
                    {
                        From = new MailAddress(configuration["AppSettings:fromemail"]),
                        Subject = emailProperties.emailSubject,
                        IsBodyHtml = true,
                        Body = emailProperties.emailBody
                    };

                    if (emailProperties.attachments != null)
                    {
                        foreach (var attachment in emailProperties.attachments)
                        {
                            message.Attachments.Add(attachment);
                        }
                    }

                    foreach (var cc in emailProperties.EmailCCList)
                    {
                        message.CC.Add(new MailAddress(cc));
                    }

                    foreach (var to in emailProperties.EmailToList)
                    {
                        message.To.Add(new MailAddress(to));
                    }

                    using (var smtp = new SmtpClient())
                    {
                        smtp.Port = Convert.ToInt32(configuration["AppSettings:port"]);
                        smtp.Host = configuration["AppSettings:host"];
                        smtp.UseDefaultCredentials = Convert.ToInt32(configuration["AppSettings:defaultcredential"]) == 1;
                        smtp.Credentials = new NetworkCredential(configuration["AppSettings:username"], configuration["AppSettings:password"]);
                        smtp.EnableSsl = Convert.ToInt32(configuration["AppSettings:enablessl"]) == 1;

                        smtp.Send(message);
                        response.responseCode = "200";
                        response.responseMessages = "Email sent successfully.";
                    }
                }
                catch (Exception ex)
                {
                    response.responseCode = "500";
                    response.responseFailed = $"Exception in sending email: {ex}";
                    Console.WriteLine(response.responseFailed);
                }
                return response;
            }
        }

        public class sendEmailProperties
        {
            public string emailSubject { get; set; }
            public string emailBody { get; set; }
            public List<string> EmailToList { get; set; } = new List<string>();
            public List<string> EmailCCList { get; set; } = new List<string>();
            public List<Attachment> attachments { get; set; } = new List<Attachment>();
        }

        public class emailSendResponse
        {
            public string responseCode { get; set; }
            public string responseMessages { get; set; }
            public string responseFailed { get; set; }
        }
    }


