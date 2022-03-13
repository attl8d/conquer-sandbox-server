using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("discord_message")]
    public class DiscordMessage
    {
        [Key] public ulong Id { get; set; }

        public ulong UserId { get; set; }
        public string CurrentUserName { get; set; }
        public ulong ChannelId { get; set; }
        public string Message { get; set; }
        public int Timestamp { get; set; }
    }
}