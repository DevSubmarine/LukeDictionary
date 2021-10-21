using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DevSubmarine.LukeDictionary
{
    // source: https://github.com/TehGM/EinherjiBot/blob/66375aa5034cd795c317acdcac3fa42c6029085c/EinherjiBot.Core/Extensions/DiscordGuildExtensions.cs
    public static class DiscordGuildExtensions
    {
        public static async Task<DiscordMember> GetMemberSafeAsync(this DiscordGuild guild, ulong id)
        {
            try
            {
                return await guild.GetMemberAsync(id).ConfigureAwait(false);
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                return null;
            }
        }
    }
}
