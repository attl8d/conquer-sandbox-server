using Comet.Web.App.Data;
using Microsoft.Extensions.Configuration;

namespace Comet.Web.App.Services
{
    public sealed class GameDatabaseService
    {
        private readonly IConfiguration mConfiguration;
        private readonly ApplicationDbContext mDbContext;

        public GameDatabaseService(IConfiguration config,
            ApplicationDbContext dbContext)
        {
            mConfiguration = config;
            mDbContext = dbContext;
        }

        public string GetConnectionString(string serverName)
        {
            return string.Empty;
        }
    }
}
