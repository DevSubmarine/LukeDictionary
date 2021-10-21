using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace DevSubmarine.LukeDictionary.PasteMyst.Services
{
    public class PasteMystClient : IPasteMystClient
    {
        private readonly HttpClient _client;
        private readonly ILogger _log;

        public PasteMystClient(HttpClient client, IOptionsMonitor<PasteMystOptions> options, ILogger<PasteMystClient> log)
        {
            this._client = client;
            this._log = log;

            this._client.DefaultRequestHeaders.Add("User-Agent", options.CurrentValue.UserAgent);
            if (!string.IsNullOrWhiteSpace(options.CurrentValue.AuthorizationToken))
                this._client.DefaultRequestHeaders.Add("Authorization", options.CurrentValue.AuthorizationToken);
        }

        public async Task<Paste> CreatePasteAsync(Paste paste, CancellationToken cancellationToken = default)
        {
            if (paste == null)
                throw new ArgumentNullException(nameof(paste));
            if (paste.Pasties?.Any() != true)
                throw new ArgumentException("At least one pasty is required", nameof(paste.Pasties));

            this._log.LogDebug("Posting a new paste to PasteMyst");
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://paste.myst.rs/api/v2/paste");
            request.Content = new StringContent(
                JObject.FromObject(paste).ToString(Newtonsoft.Json.Formatting.None),
                Encoding.UTF8,
                "application/json");

            try
            {
                using HttpResponseMessage response = await this._client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return JObject.Parse(body).ToObject<Paste>();
            }
            catch (HttpRequestException ex) when (ex.LogAsError(this._log, "Request to PasteMyst resulted in an error"))
            {
                // rethrow after logging
                throw;
            }

        }
    }
}
