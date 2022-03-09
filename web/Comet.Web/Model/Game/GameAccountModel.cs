using System;
using System.Collections.Generic;

namespace Comet.Web.Model.Game
{
    public class GameAccountModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? BanExpire { get; set; }

        public List<GameAccountCharacterModel> Characters = new List<GameAccountCharacterModel>();
    }
}
