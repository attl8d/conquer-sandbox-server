using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Comet.Web.App.Services.Discord.Handlers.Commands
{
    public class StatisticsHandler : ModuleBase<SocketCommandContext>
    {
        [Command("statistic")]
        [Summary("Query statistics about the current user.")]
        public async Task QueryStatisticAsync(
            [Summary("The identity of the user to get information. If not filled get the user who executed the command.")]
            SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await Context.Channel.SendMessageAsync("");
        }
    }
}
