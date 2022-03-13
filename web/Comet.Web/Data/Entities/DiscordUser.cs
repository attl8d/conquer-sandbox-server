using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("discord_user")]
    public class DiscordUser
    {
        [Key] public uint Identity { get; set; }

        public ulong DiscordUserId { get; set; }
        public uint? AccountId { get; set; }
        public uint? GameUserId { get; set; }
        public string AccountName { get; set; }
        public string GameName { get; set; }
        public string Name { get; set; }
        public string Discriminator { get; set; }
        public int CreatedAt { get; set; }
        public ulong MessagesSent { get; set; }
        public ulong CharactersSent { get; set; }

        //public static async Task<DbDiscordUser> GetAsync(ulong idDiscord)
        //{
        //    await using var ctx = new DatabaseContext();
        //    return await ctx.DiscordUsers.FirstOrDefaultAsync(x => x.DiscordUserId == idDiscord);
        //}
    }
}