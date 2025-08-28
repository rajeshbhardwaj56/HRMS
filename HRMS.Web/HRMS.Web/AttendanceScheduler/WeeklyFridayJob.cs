using HRMS.Models.Common;
using HRMS.Models.ExportEmployeeExcel;
using HRMS.Web.BusinessLayer;
using Newtonsoft.Json;
using Quartz;

namespace HRMS.Web.AttendanceScheduler
{
    //public class WeeklyFridayJob : IJob
    //{
    //    private readonly ILogger<WeeklyFridayJob> _logger;
    //    IBusinessLayer _businessLayer;
    //    IConfiguration _configuration;
    //    public WeeklyFridayJob(ILogger<WeeklyFridayJob> logger, IConfiguration configuration, IBusinessLayer businessLayer)
    //    {
    //        _logger = logger;
    //        _businessLayer = businessLayer;
    //        _configuration = configuration;
    //    }
    //    //public async Task Execute(IJobExecutionContext context)
    //    //{

    //    //    UpcomingWeekOffRosterParams models = new UpcomingWeekOffRosterParams();
    //    //    DateTime today = DateTime.Now;
    //    //    DateTime nextMonday = GetNextMonday(today);
    //    //    models.WeekStartDate = nextMonday;

    //    //    var response = _businessLayer.SendPostAPIRequest(models, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.AttendenceList, APIApiActionConstants.GetEmployeesWithoutUpcomingWeekOffRoster), "", true).Result.ToString();
         
    //    //    List<UpcomingWeekOffRoster> result =
    //    //JsonConvert.DeserializeObject<List<UpcomingWeekOffRoster>>(response.ToString());


    //    //    if (result.status == "Error")
    //    //    {
    //    //        sendEmailProperties sendEmailProperties = new sendEmailProperties();
    //    //        sendEmailProperties.emailSubject = "Execption On attendance Scheduler";
    //    //        sendEmailProperties.emailBody = ("Hii," + result.message);
    //    //        sendEmailProperties.EmailToList.Add(_configuration["AppSettings:SchedulerEmail"].ToString());
    //    //        emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);
    //    //        if (responses.responseCode == "200")
    //    //        {

    //    //        }
    //    //        else
    //    //        {

    //    //        }
    //    //    }
    //    //    else
    //    //    {
    //    //        sendEmailProperties sendEmailProperties = new sendEmailProperties();
    //    //        sendEmailProperties.emailSubject = "Done On attendance Scheduler";
    //    //        sendEmailProperties.emailBody = ("Hii," + result.message);
    //    //        sendEmailProperties.EmailToList.Add(_configuration["AppSettings:SchedulerEmail"].ToString());
    //    //        emailSendResponse responses = EmailSender.SendEmail(sendEmailProperties);
    //    //    }
    //    //}



    //    public static DateTime GetNextMonday(DateTime currentDate)
    //    {
    //        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)currentDate.DayOfWeek + 7) % 7;
    //        return currentDate.AddDays(daysUntilMonday == 0 ? 7 : daysUntilMonday);
    //    }

    //}
}
