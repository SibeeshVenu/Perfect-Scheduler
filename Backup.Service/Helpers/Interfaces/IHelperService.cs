using System.Threading.Tasks;

namespace Backup.Service.Helpers.Interfaces
{
    public interface IHelperService
    {
        Task PerformService(string schedule);
    }
}
