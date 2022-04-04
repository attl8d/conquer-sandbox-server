using Comet.Web.Data;

namespace Comet.Web.Services
{
    public sealed class DownloadsService
    {
        private readonly ApplicationDbContext mContext;

        public DownloadsService(ApplicationDbContext context)
        {
            mContext = context;
        }


    }
}
