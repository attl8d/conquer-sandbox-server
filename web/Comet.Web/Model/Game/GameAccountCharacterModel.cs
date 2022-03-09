namespace Comet.Web.Model.Game
{
    public class GameAccountCharacterModel
    {
        public int Id { get; set; }
        public string Server { get; set; }
        public string Name { get; set; }
        public string Mate { get; set; }
        public bool HasVip { get; set; }
    }
}
