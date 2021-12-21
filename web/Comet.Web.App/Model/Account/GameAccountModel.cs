using System;
using System.Collections.Generic;

namespace Comet.Web.App.Model.Account
{
    public class GameAccountModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? BanExpire { get; set; }

        public List<GameAccountCharactersModel> Characters = new List<GameAccountCharactersModel>();
    }

    public class GameAccountCharactersModel
    {
        public int Id { get; set; }
        public string Server { get; set; }
        public string Name { get; set; }
        public string Mate { get; set; }
        public bool HasVip { get; set; }
    }
}
