using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor mHttpContextAcessor;
        private readonly UserManager mUserManager;

        public LogService(ILogger<TParent> logger,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager userManager)
        {
            mLogger = logger;
            mDbContext = context;
            mHttpContextAcessor = httpContextAccessor;
            mUserManager = userManager;
        }

        public async Task LogAsync(string message, 
            LogHistory.LogType type, 
            LogLevel level = LogLevel.Information, 
            object payload = null, 
            int accountId = 0, 
            long data = 0)
        {
            if (accountId == 0 && mHttpContextAcessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var user = await mUserManager.GetUserAsync(mHttpContextAcessor.HttpContext.User);
                if (user != null)
                    accountId = user.Id;
            }

            mLogger.Log(level, $"[{type.ToString():-16}] -> AccountId: {accountId}, Data: {data}, {message}");

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
