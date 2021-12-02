using Comet.Web.App.Services.Schedule;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace Comet.Web.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Add the required Quartz.NET services
                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionJobFactory();

                        JobKey key = new JobKey("DailyGameSynchronizationJob");
                        q.AddJob<DailyGameSynchronizationJob>(key);

                        q.AddTrigger(opts => opts
                            .ForJob(key)
                            .WithIdentity("DailyGameSynchronizationJob-Trigger")
                            .WithDailyTimeIntervalSchedule(x => x.OnEveryDay().StartingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(0, 0, 0))));
                    });

                    // Add the Quartz.NET hosted service
                    services.AddQuartzHostedService(
                        q => q.WaitForJobsToComplete = true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
