namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using Jose;
    using Xunit;

    /*
     * A simple HTTP server that uses a keypair to serve a JWKS URI and to issue mock access tokens
     */
    public class MockAuthorizationServer : IDisposable
    {
        private readonly ITestContext testContext;
        private readonly ECDsa keypair;
        private readonly Jwk tokenSigningPrivateKey;
        private readonly JwkSet jwks;
        private readonly string keyId;
        private HttpListener httpServer;

        public MockAuthorizationServer(Configuration configuration, ITestContext testContext)
        {
            this.testContext = testContext;
            this.testContext.SendDiagnosticMessage(">>> Starting mock authorization server ...");
            
            this.keypair = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            this.tokenSigningPrivateKey = new Jwk(this.keypair, true);
            this.keyId = Guid.NewGuid().ToString();
            
            Jwk tokenSigningPublicKey = new Jwk(this.keypair, false)
            {
                Alg = configuration.Algorithm,
                KeyId = this.keyId,
            };
            this.jwks = new JwkSet(tokenSigningPublicKey);

            this.httpServer = new HttpListener();
            this.httpServer.Prefixes.Add($"http://localhost:{configuration.JwksUriPort}/");
            this.httpServer.Start();
            this.httpServer.BeginGetContext(this.onJwksRequest, this.httpServer);
        }

        /*
         * Serve a JWKS URI at http://localhost:3002/jwks to provide the token signing public key to the MCP server
         */
        private void onJwksRequest(IAsyncResult request)
        {
            var context = this.httpServer.EndGetContext(request);

            using (HttpListenerResponse response = context.Response)
            {
                response.Headers.Set("content-type", "application/json");
                response.StatusCode = (int)HttpStatusCode.OK;;

                using (Stream output = response.OutputStream)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(this.jwks.ToJson());
                    response.ContentLength64 = bytes.Length;
                    output.Write(bytes, 0, bytes.Length);
                }
            }

            this.httpServer.BeginGetContext(this.onJwksRequest, this.httpServer);
        }

        /*
         * Issue a mock access token with the token signing private key
         */
        public string IssueAccessToken(MockTokenOptions options)
        {
            var now = DateTimeOffset.Now;
            var exp = now.AddMinutes(options.ExpiryMinutes);

            var headers = new Dictionary<string, object>()
            {
                { "kid", this.keyId },
            };

            var payload = new Dictionary<string, object>()
            {
                { "iss", options.Issuer },
                { "aud", options.Audience },
                { "scope", options.Scope },
                { "exp", exp.ToUnixTimeSeconds() },
                { "sub", options.Subject },
                { "customer_id", options.CustomerId },
                { "region", options.Region },
                { "client_type", "ai-agent" },
            };

            return JWT.Encode(payload, this.tokenSigningPrivateKey, JwsAlgorithm.ES256, headers);
        }

        /*
         * Free resources
         */
        public void Dispose()
        {
            this.testContext.SendDiagnosticMessage(">>> Stopping mock authorization server ...");
            this.keypair.Dispose();
            this.httpServer.Stop();
            this.httpServer.Close();
        }
    }
}