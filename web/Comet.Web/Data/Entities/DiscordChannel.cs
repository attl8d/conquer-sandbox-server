using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("discord_channel")]
    public class DiscordChannel
    {
        [Key] public uint Id { get; set; }
        public ulong ChannelId { get; set; }
        public string Name { get; set; }
        public int CreatedAt { get; set; }
        public bool Default { get; set; }
    }
}
