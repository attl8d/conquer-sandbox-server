using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Comet.Web.App.Services
{
    public interface ILogService<out TParent>
    {

    }

    public sealed class LogService<TParent> : ILogService<TParent>
    {
        private readonly ILogger<TParent> mLogger;
        private readonly ApplicationDbContext mDbContext;

        public LogService(ILogger<TParent> logger,
            ApplicationDbContext context)
        {
            mLogger = logger;
            mDbContext = context;
        }

        public async Task LogAsync(string message, 
            LogHistory.LogType type, 
            LogLevel level = LogLevel.Information, 
            object payload = null, 
            int accountId = 0, 
            long data = 0)
        {
            mLogger.Log(level, $"[{type:-16}] -> AccountId: {accountId}, Data: {data}, {message}");

            mDbContext.LogHistory.Add(new LogHistory
            {
                AccountId = accountId == 0 ? null : accountId,
                Data = data,
                Type = type,
                Message = message,
                Payload = JsonConvert.SerializeObject(payload)
            });
            await mDbContext.SaveChangesAsync();
        }
    }
}
