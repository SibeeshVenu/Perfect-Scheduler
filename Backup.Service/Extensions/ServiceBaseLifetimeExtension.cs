using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Backup.Service.Extensions
{
    public static class ServiceBaseLifetimeExtension
    {
        public static IHostBuilder UseServiceLifetime(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((hostContext, services) => services.AddSingleton<IHostLifetime, ServiceLifeTime>());
        }

        public static Task RunTheServiceAsync(this IHostBuilder hostBuilder, CancellationToken cancellationToken = default)
        {
            return hostBuilder.UseServiceLifetime().Build().RunAsync(cancellationToken);
        }
    }
}
