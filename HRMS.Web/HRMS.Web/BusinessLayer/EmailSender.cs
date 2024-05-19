using System.Net.Mail;
using System.Net;

namespace HRMS.Web.BusinessLayer
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
                MailMessage message = new MailMessage();
                message.From = new MailAddress(configuration["AppSettings:fromemail"]);
                message.Subject = emailProperties.emailSubject;
                message.IsBodyHtml = true; //to make message body as html  CREDITLIMIT
                message.Body = emailProperties.emailBody;

                if (emailProperties != null)
                {
                    if (emailProperties.attachments != null)
                    {
                        foreach (var attachment in emailProperties.attachments)
                        {
                            message.Attachments.Add(attachment);
                        }
                    }

                    foreach (var item in emailProperties.EmailToList)
                    {
                        message.To.Clear();
                        message.To.Add(new MailAddress(item));
                        using (var smtp = new SmtpClient())
                        {
                            try
                            {
                                smtp.Port = Convert.ToInt32(configuration["AppSettings:port"]);
                                smtp.Host = configuration["AppSettings:host"]; //for gmail host  
                                smtp.UseDefaultCredentials = Convert.ToInt32(configuration["AppSettings:defaultcredential"]) == 1 ? true : false;
                                smtp.Credentials = new NetworkCredential(configuration["AppSettings:username"], configuration["AppSettings:password"]);
                                smtp.EnableSsl = Convert.ToInt32(configuration["AppSettings:enablessl"]) == 1 ? true : false;
                                //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                                smtp.Send(message);
                                response.responseCode = "200";
                                response.responseMessages = "Email send successfully.";
                            }
                            catch (SmtpFailedRecipientsException ex)
                            {
                                for (int i = 0; i < ex.InnerExceptions.Length; i++)
                                {
                                    SmtpStatusCode status = ex.InnerExceptions[i].StatusCode;
                                    if (status == SmtpStatusCode.MailboxBusy ||
                                        status == SmtpStatusCode.MailboxUnavailable)
                                    {
                                        Console.WriteLine("Delivery failed - retrying in 5 seconds.");

                                        response.responseFailed = string.Format("Delivery failed - retrying in 5 seconds.");

                                        System.Threading.Thread.Sleep(5000);
                                        smtp.Send(message);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Failed to deliver message to {0}",
                                            ex.InnerExceptions[i].FailedRecipient);

                                        response.responseFailed = string.Format("Failed to deliver message to {0}",
                                            ex.InnerExceptions[i].FailedRecipient);
                                    }

                                    response.responseCode = status.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                response.responseFailed = string.Format("Exception caught in RetryIfBusy(): {0}",
                                        ex.ToString());
                                Console.WriteLine("Exception caught in RetryIfBusy(): {0}",
                                        ex.ToString());
                            }
                            finally
                            {

                            }
                        }
                    }
                }
            }
            finally
            {

            }
            return response;
        }
    }


    public class sendEmailProperties
    {
        public string emailSubject { get; set; }
        public string emailBody { get; set; }
        public List<string> EmailToList { get; set; } = new List<string>();
        public List<Attachment> attachments { get; set; } = new List<Attachment>();
    }

    public class emailSendResponse
    {
        public string responseCode { get; set; }
        public string responseMessages { get; set; }
        public string responseFailed { get; set; }
    }
}
