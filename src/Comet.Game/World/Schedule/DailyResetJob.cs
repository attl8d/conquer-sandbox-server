using Comet.Game.Database;
using Comet.Game.Database.Models;
using Comet.Game.States;
using Comet.Game.States.Events;
using Comet.Shared;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comet.Game.World.Schedule
{
    [DisallowConcurrentExecution]
    public sealed class DailyResetJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            ArenaQualifier qualifier = Kernel.EventThread.GetEvent<ArenaQualifier>();

            uint today = uint.Parse(DateTime.Now.ToString("yyyyMMdd"));
            var users = await DbCharacter.GetDailyResetAsync();
            for (int i = users.Count - 1; i >= 0; i--)
            {
                try
                {
                    DbCharacter dbUser = users[i];

                    Character player = Kernel.RoleManager.GetUser(dbUser.Identity);
                    player ??= new Character(dbUser, null);

                    // arena
                    if (qualifier != null)
                    {
                        if (qualifier.GetReward(player.Identity, out var reward))
                        {
                            player.HonorPoints += (uint)reward.Honor;
                            player.HistoryHonorPoints += (uint)reward.Honor;
                        }
                    }

                    player.QualifierPoints = ArenaQualifier.GetInitialPoints(dbUser.Level);
                    player.QualifierDayWins = 0;
                    player.QualifierDayLoses = 0;

                    player.DayResetDate = today;

                    await player.SaveAsync();
                    await Log.WriteLogAsync(LogLevel.Info, $"Daily Reset [{dbUser.Name:-16}] done.");
                }
                catch (Exception ex)
                {
                    await Log.WriteLogAsync(LogLevel.Error, $"Daily Reset [{users[i].Name:-16}] error.");
                    await Log.GmLogAsync("daily_reset_err", ex.ToString());
                }
            }

            await Kernel.FlowerManager.DailyResetAsync().ConfigureAwait(false);

            sw.Stop();
            await BaseRepository.ScalarAsync($"INSERT INTO `daily_reset` (run_time, ms) VALUES (NOW(), {sw.ElapsedMilliseconds});");
            await Log.WriteLogAsync(LogLevel.Info, $"Daily reset has run in {sw.ElapsedMilliseconds}ms.");
        }
    }
}
