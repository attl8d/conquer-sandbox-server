using System;
using System.Collections.Generic;

namespace Comet.Web.Model.Game
{
    public class GameAccountModel
    {
        public List<GameAccountCharacterModel> Characters = new();
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? BanExpire { get; set; }
    }
}