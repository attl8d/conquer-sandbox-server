namespace Comet.Web.Data.Repositories
{
    public sealed class AccountRepository
    {
        private readonly ApplicationDbContext mContext;

        public AccountRepository(ApplicationDbContext ctx)
        {
            mContext = ctx;
        }
    }
}
