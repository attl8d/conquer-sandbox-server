using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Comet.Web.Model.Game.Enums;
using Comet.Web.Services;
using Comet.Web.Services.Realms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Ranking.Pages
{
    public class IndexModel : PageModel
    {
        private readonly RealmService mRealmService;
        private readonly LanguageService mLanguageService;
        private readonly RankingService mRankingService;

        public IndexModel(RealmService realmService, LanguageService languageService, RankingService rankingService)
        {
            mRealmService = realmService;
            mLanguageService = languageService;
            mRankingService = rankingService;
        }

        [BindProperty(SupportsGet = true)] public string RankType { get; set; }
        [BindProperty(SupportsGet = true)] public string ServerName { get; set; }
        [BindProperty(SupportsGet = true)] public int PageNum { get; set; }

        public List<RealmService.RealmPublicListData> Servers = new();
        public string RankName { get; set; }
        public DataTable DataTable { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await mRealmService.LoadRealmsAsync();
            Servers = mRealmService.GetRealmList();

            int idxServer = (int) Servers
                                  .FirstOrDefault(
                                      x => x.Active && x.RealmName.Equals(ServerName, StringComparison.InvariantCultureIgnoreCase))
                                  .RealmID;

            if (idxServer == 0)
                idxServer = -1;

            int pageNum = Math.Max(0, PageNum - 1);

            switch (RankType.ToLower(CultureInfo.InvariantCulture))
            {
                default:
                    DataTable = await mRankingService.GetPlayerRankAsync(pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopPlayers");
                    break;

                case "trojan":
                    DataTable = await mRankingService.GetProfessionRankAsync(1, pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopTrojan");
                    break;
                case "warrior":
                    DataTable = await mRankingService.GetProfessionRankAsync(2, pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopWarrior");
                    break;
                case "archer":
                    DataTable = await mRankingService.GetProfessionRankAsync(4, pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopArcher");
                    break;
                case "ninja":
                    DataTable = await mRankingService.GetProfessionRankAsync(5, pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopNinja");
                    break;
                case "monk":
                    DataTable = await mRankingService.GetProfessionRankAsync(6, pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopMonkSaint");
                    break;
                case "pirate":
                    DataTable = await mRankingService.GetProfessionRankAsync(7, pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopPirate");
                    break;
                case "watertaoist":
                    DataTable = await mRankingService.GetProfessionRankAsync(13, pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopWaterTaoist");
                    break;
                case "firetaoist":
                    DataTable = await mRankingService.GetProfessionRankAsync(14, pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopFireTaoist");
                    break;

                case "moneybag":
                    DataTable = await mRankingService.GetMoneyBagAsync(pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopMoneybag");
                    break;

                case "syndicate":
                    DataTable = await mRankingService.GetSyndicateRankingAsync(pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopSyndicate");
                    break;
                case "family":
                    DataTable = await mRankingService.GetFamilyRankingAsync(pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopFamily");
                    break;

                case "guildwarwinners":
                    DataTable = await mRankingService.GetGuildWarWinnersRankinAsync(pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopGuildWarWinners");
                    break;

                case "superman":
                    DataTable = await mRankingService.GetSuperManRankingAsync(pageNum, idxServer);
                    RankName = mLanguageService.GetString("TopSuperman");
                    break;

                case "noble":
                    DataTable = await mRankingService.GetNobleRankingAsync(pageNum, idxServer);
                    break;
            }

            if (DataTable.Columns.Contains("Profession"))
            {
                foreach (DataRow row in DataTable.Rows)
                {
                    row["Profession"] = mLanguageService.GetString($"StrPro{(ProfessionType)int.Parse(row["Profession"].ToString() ?? "0")}");
                }
            }

            return Page();
        }

        public string GetPositionHtml(int position)
        {
            if (position == 1)
                return "<span class=\"rank-1\"></span>";
            if (position == 2)
                return "<span class=\"rank-2\"></span>";
            if (position == 3)
                return "<span class=\"rank-3\"></span>";
            if (position == 4)
                return "<span class=\"rank-4\"></span>";
            if (position == 5)
                return "<span class=\"rank-5\"></span>";
            if (position is >= 6 and <= 10)
                return "<span class=\"rank-6\"></span>";
            if (position is >= 11 and <= 25)
                return "<span class=\"rank-7\"></span>";
            if (position is >= 26 and <= 50)
                return "<span class=\"rank-8\"></span>";
            return position.ToString();
        }

        public struct ServerInfo
        {
            public int Identity { get; set; }
            public string Name { get; set; }
        }
    }
}
