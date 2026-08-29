using Microsoft.Extensions.Configuration;
using HRMSConsoleApp.Model;
using HRMSConsoleApp.Services;
using HRMSConsoleApp.Utilities;

namespace AttendanceSchedulerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                
                var exePath = AppDomain.CurrentDomain.BaseDirectory;

                var config = new ConfigurationBuilder()
                    .SetBasePath(exePath) // looks in the exe directory
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var service = new AttendanceService(config);

                // Initialize EmailSender with configuration
                new EmailSender(config);

                // Run for yesterday’s date
                var previousDay = DateTime.Now.AddDays(-1);
                var model = new AttendanceInputParams
                {
                    Year = previousDay.Year,
                    Month = previousDay.Month,
                    Day = previousDay.Day,
                    UserId = 0
                };

                var result = service.GetAttendance(model);

                var email = new sendEmailProperties
                {
                    emailSubject = result.IsSuccess ? "Done On Attendance Scheduler" : "Exception On Attendance Scheduler",
                    emailBody = "Hi, " + result.Message
                };
                email.EmailToList.Add(config["AppSettings:SchedulerEmail"]);

                var response = EmailSender.SendEmail(email);

                Console.WriteLine($"[INFO] Job finished at {DateTime.Now}. Email Response: {response.responseCode} - {response.responseMessages}");
            }
            catch (Exception ex)
            {
                // Log error to a file
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SchedulerError.log");
                File.AppendAllText(logPath,
                    $"[{DateTime.Now}] Unhandled Exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}");

                Console.WriteLine("An error occurred. Check SchedulerError.log for details.");
            }
        }
    }
}
