using Backup.Service.Helpers.Interfaces;
using Quartz;
using System.Threading.Tasks;

namespace Backup.Service.Helpers
{
    public class WeeklyJob : IWeeklyJob
    {
        public IHelperService _helperService;
        public WeeklyJob(IHelperService helperService)
        {
            _helperService = helperService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await _helperService.PerformService(BackupSchedule.Weekly);
        }
    }
}
