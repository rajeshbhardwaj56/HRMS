using Quartz.Spi;
using Quartz;

namespace HRMS.Web.AttendanceScheduler
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public JobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetRequiredService<QuartzJobRunner>();
        }

        public void ReturnJob(IJob job) { }
    }
}
