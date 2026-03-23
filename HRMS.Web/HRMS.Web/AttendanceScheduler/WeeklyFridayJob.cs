using HRMS.Models.Common;
using HRMS.Models.ExportEmployeeExcel;
using HRMS.Web.BusinessLayer;
using Newtonsoft.Json;
using Quartz;

namespace HRMS.Web.AttendanceScheduler
{
    public class WeeklyFridayJob : IJob
    {
        private readonly ILogger<WeeklyFridayJob> _logger;
        private readonly IBusinessLayer _businessLayer;
        private readonly IConfiguration _configuration;

        public WeeklyFridayJob(ILogger<WeeklyFridayJob> logger, IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _logger = logger;
            _businessLayer = businessLayer;
            _configuration = configuration;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                UpcomingWeekOffRosterParams models = new UpcomingWeekOffRosterParams();
                DateTime today = DateTime.Now;

                // Calculate NEXT MONDAY
                DateTime nextMonday = GetNextMonday(today);
                models.WeekStartDate = nextMonday;

                // Call API
                var response = await _businessLayer.SendPostAPIRequest(
                    models,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.AttendenceList,
                        APIApiActionConstants.GetEmployeesWithoutUpcomingWeekOffRoster
                    ),
                    "",
                    true
                );

                var result = JsonConvert.DeserializeObject<List<UpcomingWeekOffRoster>>(response.ToString());

                if (result == null || result.Count == 0)
                {
                    // Nothing missing
                    var notify = new sendEmailProperties
                    {
                        emailSubject = $"WeekOff Roster Scheduler - All Good ({nextMonday:dd-MMM-yyyy})",
                        emailBody = $"Hi, all employees have uploaded their week-off roster for week starting {nextMonday:dd-MMM-yyyy}"
                    };
                    notify.EmailToList.Add(_configuration["AppSettings:SchedulerEmail"]);
                    EmailSender.SendEmail(notify);
                    return;
                }

                // Send one email per manager
                foreach (var manager in result)
                {
                    if (string.IsNullOrWhiteSpace(manager.ManagerEmailID))
                        continue;

                    string employeeList = string.Join("<br/>",
                        manager.Employees.Select(e => $"{e.EmployeeNumber} - {e.EmployeeName}"));

                    var mail = new sendEmailProperties
                    {
                        emailSubject = $"WeekOff Roster Missing - Week Starting {nextMonday:dd-MMM-yyyy}",
                        emailBody = $@"
<div style='font-family: Arial, sans-serif; font-size: 14px; color: #000;'>
    Hi {manager.ManagerName},<br/><br/>
    The following employees reporting to you have not uploaded their week-off roster for the week starting <b>{nextMonday:dd-MMM-yyyy}</b>.<br/><br/>

    <table style='width: 100%; max-width: 600px; border-collapse: collapse; border: 1px solid #000;'>
        <thead style='background-color: #f2f2f2;'>
            <tr>
                <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Employee Number</th>
                <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Employee Name</th>
                <th style='border: 1px solid #000; padding: 8px; text-align: left;'>Manager Name</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", manager.Employees.Select(e => $@"
            <tr>
                <td style='border: 1px solid #000; padding: 8px;'>{e.EmployeeNumber}</td>
                <td style='border: 1px solid #000; padding: 8px;'>{e.EmployeeName}</td>
                <td style='border: 1px solid #000; padding: 8px;'>{e.ImmediateManagerName }</td>
            </tr>"))}
        </tbody>
    </table><br/>

    <p style='color: #000; font-size: 13px;'>
     Protalk Solutions is an ISO 27001:2022 certified.<br/>
     This email and its attachments are confidential and intended solely for the use of the individual or entity addressed. Protalk Solutions prioritizes the security and privacy of information, adhering to the Information Security Management System (ISMS) standards, and leading cybersecurity practices.<br/>
     We enforce a robust data retention and deletion policy, ensuring all sensitive data is securely handled and automatically removed after the retention period, in strict compliance with applicable laws.<br/>
     If you are not the intended recipient or responsible for delivering this message, any unauthorized use, dissemination, copying, or action taken based on its contents is prohibited.<br/>
     If you received this email in error, please notify us immediately at 
     <a href=""mailto:it.protalk@protalkbiz.com"">it.protalk@protalkbiz.com</a> to resolve the matter.
    </p>
</div>"
                    };

                    mail.EmailToList.Add(manager.ManagerEmailID);
                    EmailSender.SendEmail(mail);
                }
            }
            catch (Exception ex)
            {
                // Send exception notification
                var errorMail = new sendEmailProperties
                {
                    emailSubject = "Exception On WeekOff Scheduler",
                    emailBody = $"Hi,<br/> Exception occurred in WeeklyFridayJob: {ex.Message}"
                };
                errorMail.EmailToList.Add(_configuration["AppSettings:SchedulerEmail"]);
                EmailSender.SendEmail(errorMail);
            }
        }

        public static DateTime GetNextMonday(DateTime currentDate)
        {
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)currentDate.DayOfWeek + 7) % 7;
            return currentDate.AddDays(daysUntilMonday == 0 ? 7 : daysUntilMonday);
        }
    }
}
