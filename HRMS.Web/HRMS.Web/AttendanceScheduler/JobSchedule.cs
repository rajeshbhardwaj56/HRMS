namespace HRMS.Web.AttendanceScheduler
{
    public class JobSchedule
    {
        public JobSchedule(Type jobType, string cronExpression, TimeZoneInfo timeZone = null)
        {
            JobType = jobType;
            CronExpression = cronExpression;
            TimeZone = timeZone ?? TimeZoneInfo.Local;
        }

        public Type JobType { get; }
        public string CronExpression { get; }
        public TimeZoneInfo TimeZone { get; }
    }
}
