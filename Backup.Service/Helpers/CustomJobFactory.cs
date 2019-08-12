using Quartz;
using Quartz.Spi;
using System;

namespace Backup.Service.Helpers
{
    public class CustomJobFactory : IJobFactory
    {
        protected readonly IServiceProvider _serviceProvider;
        public CustomJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                return _serviceProvider.GetService(bundle.JobDetail.JobType) as IJob;
            }
            catch (Exception ex)
            {
                throw new CustomConfigurationException(ex.Message);
            }            
        }

        public void ReturnJob(IJob job)
        {
            var obj = job as IDisposable;
            obj?.Dispose();
        }
    }
}
