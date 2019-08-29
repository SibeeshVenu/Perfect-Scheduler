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
