namespace IO.Curity.AutonomousAgent.Security
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Logging;

    /*
     * Cache token exchange results on each agent instance to make token exchange perform efficiently
     */
    public sealed class TokenCache
    {
        private readonly Configuration configuration;
        private readonly IDistributedCache cache;
        private readonly ILogger<TokenCache> logger;

        public TokenCache(Configuration configuration, IDistributedCache cache, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.cache = cache;
            this.logger = loggerFactory.CreateLogger<TokenCache>();
        }

        /*
         * Write an exchanged access token to the cache
         */
        public async Task SetItemAsync(string receivedAccessToken, string exchangedAccessToken)
        {
            var hash = this.Sha256(receivedAccessToken);

            var now = DateTimeOffset.UtcNow;
            var options = new DistributedCacheEntryOptions
            {   
                AbsoluteExpiration = now.AddSeconds(this.configuration.TokenExchangeCacheSeconds),
            };

            this.logger.LogTrace($"Caching exchanged token for: {hash}");
            var bytes = Encoding.UTF8.GetBytes(exchangedAccessToken);
            await this.cache.SetAsync(hash, bytes, options);
        }

        /*
         * MCP flows can be chatty, with 5 or so HTTP requests for a single call with an MCP client
         * This code reduces the frequency of token exchange requests, to improve efficiency
         */
        public async Task<string?> GetItemAsync(string receivedAccessToken)
        {
            var hash = this.Sha256(receivedAccessToken);
            var bytes = await this.cache.GetAsync(hash);
            if (bytes == null)
            {
                this.logger.LogTrace($"No existing token in cache for: {hash}");
                return null;
            }

            this.logger.LogTrace($"Found existing token in cache for: {hash}");
            return Encoding.UTF8.GetString(bytes);
        }

        /*
         * Use the hash of a received access token as the cache key
         */
        private string Sha256(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}