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
using NLog;
using System.Configuration;

namespace Backup.Service
{
    public class BackupService : IHostedService
    {
        private static ILogger _logger;
        public BackupService()
        {
            SetUpNLog();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var scheduler = await GetScheduler();
                var serviceProvider = GetConfiguredServiceProvider();
                scheduler.JobFactory = new CustomJobFactory(serviceProvider);
                await scheduler.Start();
                await ConfigureDailyJob(scheduler);
                await ConfigureWeeklyJob(scheduler);
                await ConfigureMonthlyJob(scheduler);
            }
            catch (Exception ex)
            {
                _logger.Error(new CustomConfigurationException(ex.Message));
            }
        }

        private async Task ConfigureDailyJob(IScheduler scheduler)
        {
            var dailyJob = GetDailyJob();
            if (await scheduler.CheckExists(dailyJob.Key))
            {
                await scheduler.DeleteJob(dailyJob.Key);
                _logger.Info($"The job key {dailyJob.Key} was already existed, thus deleted the same");
            }
            await scheduler.ScheduleJob(dailyJob, GetDailyJobTrigger());
        }

        private async Task ConfigureWeeklyJob(IScheduler scheduler)
        {
            var weklyJob = GetWeeklyJob();
            if (await scheduler.CheckExists(weklyJob.Key))
            {
                await scheduler.DeleteJob(weklyJob.Key);
                _logger.Info($"The job key {weklyJob.Key} was already existed, thus deleted the same");
            }
            await scheduler.ScheduleJob(weklyJob, GetWeeklyJobTrigger());
        }

        private async Task ConfigureMonthlyJob(IScheduler scheduler)
        {
            var monthlyJob = GetMonthlyJob();
            if (await scheduler.CheckExists(monthlyJob.Key))
            {
                await scheduler.DeleteJob(monthlyJob.Key);
                _logger.Info($"The job key {monthlyJob.Key} was already existed, thus deleted the same");
            }
            await scheduler.ScheduleJob(monthlyJob, GetMonthlyJobTrigger());
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
            // Comment this if you don't want to use database start
            var config = (NameValueCollection)ConfigurationManager.GetSection("quartz");
            var factory = new StdSchedulerFactory(config);
            // Comment this if you don't want to use database end

            // Uncomment this if you want to use RAM instead of database start
            //var props = new NameValueCollection { { "quartz.serializer.type", "binary" } };
            //var factory = new StdSchedulerFactory(props);
            // Uncomment this if you want to use RAM instead of database end
            var scheduler = await factory.GetScheduler();
            return scheduler;
        }
        private void SetUpNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "backupclientlogfile_backupservice.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);
            // Apply config           
            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();
        }
        #endregion
    }
}
