using System.Data;
using System.Threading.Tasks;
using Comet.Web.Data;

namespace Comet.Web.Services
{
    public sealed class RankingService
    {
        private const int UserMinLevel = 40;
        private const int SyndicateMinMembers = 1;
        private const int FamilyMinMembers = 1;
        private const int Ipp = 50;

        private readonly ApplicationDbContext mContext;

        public RankingService(ApplicationDbContext context)
        {
            mContext = context;
        }

        public Task<DataTable> GetPlayerRankAsync(int pageIdx, int idxServer)
        {
            return mContext.SelectAsync($"CALL GetTopPlayers({UserMinLevel}, {Ipp}, {pageIdx * Ipp}, {idxServer});");
        }

        public Task<DataTable> GetProfessionRankAsync(int prof, int pageIdx, int idxServer)
        {
            int minProf = prof * 10;
            int maxProf = minProf+ 9;
            return mContext.SelectAsync($"CALL GetTopProfession({UserMinLevel}, {minProf}, {maxProf}, {Ipp}, {pageIdx * Ipp}, {idxServer});");
        }

        public Task<DataTable> GetMoneyBagAsync(int pageIdx, int idxServer)
        {
            return mContext.SelectAsync($"CALL GetTopMoneybag({Ipp}, {pageIdx * Ipp}, {idxServer});");
        }

        public Task<DataTable> GetSyndicateRankingAsync(int pageIdx, int idxServer)
        {
            return mContext.SelectAsync($"CALL GetTopSyndicate({SyndicateMinMembers}, {Ipp}, {pageIdx * Ipp}, {idxServer});");
        }

        public Task<DataTable> GetFamilyRankingAsync(int pageIdx, int idxServer)
        {
            return mContext.SelectAsync($"CALL GetTopFamily({FamilyMinMembers}, {Ipp}, {pageIdx * Ipp}, {idxServer});");
        }

        public Task<DataTable> GetGuildWarWinnersRankinAsync(int pageIdx, int idxServer)
        {
            return mContext.SelectAsync($"CALL GetTopGuildWinners({Ipp}, {pageIdx * Ipp}, {idxServer});");
        }

        public Task<DataTable> GetSuperManRankingAsync(int pageIdx, int idxServer)
        {
            return mContext.SelectAsync($"CALL GetTopSuperman({Ipp}, {pageIdx * Ipp}, {idxServer});");
        }

        public Task<DataTable> GetNobleRankingAsync(int pageIdx, int idxServer)
        {
            return mContext.SelectAsync($"CALL GetTopNoble({Ipp}, {pageIdx * Ipp}, {idxServer});");
        }
    }
}
