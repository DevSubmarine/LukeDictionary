using Microsoft.Extensions.Options;

namespace DevSubmarine.LukeDictionary.Discord
{
    /// <summary>Validates instances of <see cref="DiscordOptions"/>.</summary>
    public class DiscordOptionsValidator : IValidateOptions<DiscordOptions>
    {
        /// <inheritdoc/>
        public ValidateOptionsResult Validate(string name, DiscordOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.BotToken))
                return ValidateOptionsResult.Fail($"{nameof(options.BotToken)} is required and cannot be null or empty.");

            return ValidateOptionsResult.Success;
        }
    }
}
