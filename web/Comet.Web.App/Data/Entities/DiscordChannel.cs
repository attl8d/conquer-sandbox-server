using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.App.Data.Entities
{
    [Table("discord_channel")]
    public class DiscordChannel
    {
        [Key] public uint Id { get; set; }
        public ulong ChannelId { get; set; }
        public string Name { get; set; }
        public int CreatedAt { get; set; }
        public bool Default { get; set; }

        //public static async Task<DbDiscordChannel> GetAsync(ulong idChannel)
        //{
        //    await using var ctx = new DatabaseContext();
        //    return await ctx.DiscordChannels.FirstOrDefaultAsync(x => x.ChannelId == idChannel);
        //}

        //public static async Task<DbDiscordChannel> GetAsync()
        //{
        //    await using var ctx = new DatabaseContext();
        //    return await ctx.DiscordChannels.FirstOrDefaultAsync(x => x.Default) ??
        //           await ctx.DiscordChannels.FirstOrDefaultAsync();
        //}
    }
}
