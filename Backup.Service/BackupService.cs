using Backup.Service.Helpers;
using Backup.Service.Helpers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace Backup.Service
{
    public class BackupService : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var scheduler = await GetScheduler();
                var serviceProvider = GetConfiguredServiceProvider();
                scheduler.JobFactory = new CustomJobFactory(serviceProvider);

                await scheduler.Start();
                await scheduler.ScheduleJob(GetDailyJob(), GetDailyJobTrigger());
                await Task.Delay(1000);
                await scheduler.ScheduleJob(GetWeeklyJob(), GetWeeklyJobTrigger());
                await Task.Delay(1000);
                await scheduler.ScheduleJob(GetMonthlyJob(), GetMonthlyJobTrigger());
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                throw new CustomConfigurationException(ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #region "Private Functions"
        private IServiceProvider GetConfiguredServiceProvider()
        {
            var services = new ServiceCollection()
                .AddScoped<IDailyJob, DailyJob>()
                .AddScoped<IWeeklyJob, WeeklyJob>()
                .AddScoped<IMonthlyJob, MonthlyJob>()
                .AddScoped<IHelperService, HelperService>();
            return services.BuildServiceProvider();
        }
        private IJobDetail GetDailyJob()
        {
            return JobBuilder.Create<IDailyJob>()
                .WithIdentity("dailyjob", "dailygroup")
                .Build();
        }
        private ITrigger GetDailyJobTrigger()
        {
            return TriggerBuilder.Create()
                 .WithIdentity("dailytrigger", "dailygroup")
                 .StartNow()
                 .WithSimpleSchedule(x => x
                     .WithIntervalInHours(24)
                     .RepeatForever())
                 .Build();
        }
        private IJobDetail GetWeeklyJob()
        {
            return JobBuilder.Create<IWeeklyJob>()
                .WithIdentity("weeklyjob", "weeklygroup")
                .Build();
        }
        private ITrigger GetWeeklyJobTrigger()
        {
            return TriggerBuilder.Create()
                 .WithIdentity("weeklytrigger", "weeklygroup")
                 .StartNow()
                 .WithSimpleSchedule(x => x
                     .WithIntervalInHours(120)
                     .RepeatForever())
                 .Build();
        }
        private IJobDetail GetMonthlyJob()
        {
            return JobBuilder.Create<IMonthlyJob>()
                .WithIdentity("monthlyjob", "monthlygroup")
                .Build();
        }
        private ITrigger GetMonthlyJobTrigger()
        {
            return TriggerBuilder.Create()
                 .WithIdentity("monthlytrigger", "monthlygroup")
                 .StartNow()
                 .WithSimpleSchedule(x => x
                     .WithIntervalInHours(720)
                     .RepeatForever())
                 .Build();
        }
        private static async Task<IScheduler> GetScheduler()
        {
            var props = new NameValueCollection { { "quartz.serializer.type", "binary" } };
            var factory = new StdSchedulerFactory(props);
            var scheduler = await factory.GetScheduler();
            return scheduler;
        }
        #endregion
    }
}
