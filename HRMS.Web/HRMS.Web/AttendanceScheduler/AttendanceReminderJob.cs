using DocumentFormat.OpenXml.InkML;
using HRMS.Web.BusinessLayer;
using HRMS.Models.Common;
using Microsoft.Extensions.Configuration;
using Quartz;
using Newtonsoft.Json;
using HRMS.Models.Employee;

namespace HRMS.Web.AttendanceScheduler
{
    public class AttendanceReminderJob : IJob
    {
        IBusinessLayer _businessLayer;
        IConfiguration _configuration;
        public AttendanceReminderJob(IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _businessLayer = businessLayer;
            _configuration = configuration;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            AttendanceInputParams models = new AttendanceInputParams();
            DateTime previousDay = DateTime.Now.AddDays(-1);
            models.Year = previousDay.Year;
            models.Month = previousDay.Month;
            models.Day = previousDay.Day;
            var response = _businessLayer.SendPostAPIRequest(models, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetAttendance), "", true).Result.ToString();
            var result = JsonConvert.DeserializeObject<dynamic>(response.ToString());

            // Check if Status is "Error" from stored procedure
            if (result.status == "Error")
            {
                sendEmailProperties sendEmailProperties = new sendEmailProperties();
                sendEmailProperties.emailSubject = "Execption On attendance Scheduler";
                sendEmailProperties.emailBody = ("Hii," + result.message);
                sendEmailProperties.EmailToList.Add(_configuration["AppSettings:SchedulerEmail"].ToString());
                emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);
                if (responses.responseCode == "200")
                {

                }
                else
                {

                }
            }
        }
    }
}
