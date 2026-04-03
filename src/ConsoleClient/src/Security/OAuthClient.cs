namespace IO.Curity.ConsoleClient.Security
{
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Web;
    using A2A;
    using IO.Curity.ConsoleClient;

    /*
     * The example OAuth client class runs a code flow from RFC 8252 for a console or desktop application
     */
    public class OAuthClient
    {
        private readonly Configuration configuration;
        private readonly AuthorizationCodeOAuthFlow serverCodeFlowSettings;
        private string? accessToken;

        /*
         * The OAuth client uses settings from the server and its own static client configuration
         */
        public OAuthClient(Configuration configuration, SecurityScheme? serverOAuthScheme)
        {
            this.configuration = configuration;
            this.accessToken = null;

            if (serverOAuthScheme != null)
            {
                var serverCodeFlowSettings = serverOAuthScheme?.OAuth2SecurityScheme?.Flows?.AuthorizationCode;
                if (serverCodeFlowSettings != null)
                {
                    this.serverCodeFlowSettings = serverCodeFlowSettings;
                    return;
                }
            }

            throw new InvalidDataException("Invalid OAuth security scheme data received from the A2A server");
        }

        /*
         * Create the authorization request URL and run the system browser
         */
        public async Task LoginAsync()
        {
            // Get a free low-privilege port on which to run an HTTP listener
            int port = this.FindFreeLoopbackPort();

            // Get values to store between front channel and back channel requests
            var state = RandomStringGenerator.CreateState();
            var (codeVerifier, codeChallenge) = RandomStringGenerator.CreateCodeVerifier();

            // Execute the front channel request
            var tcs = new TaskCompletionSource<Uri?>();
            this.StartLogin(port, state, codeChallenge, tcs);
            var authorizationResponseUrl = await tcs.Task;

            // Execute the back channel request
            await this.EndLoginAsync(port, state, codeVerifier, authorizationResponseUrl);
        }

        /*
         * Create the authorization request URL and receive the response from the system browser
         */
        private void StartLogin(int port, string state, string codeChallenge, TaskCompletionSource<Uri?> tcs)
        { 
            // Create an HTTP listener on the port of the redirect URI
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            listener.Start();

            // The browser invokes the HTTP listener when the authorization server returns the authorization response to the callback URL
            AsyncCallback responseCallback = (result) =>
            {
                var context = listener.EndGetContext(result);
                tcs.SetResult(context.Request.Url);
                context.Response.Redirect(this.configuration.PostLoginWebsiteUrl);
                context.Response.Close();
                listener.Stop();
                listener.Close();
            };
            listener.BeginGetContext(responseCallback, listener);

            // Use the A2A server's OAuth settings and the client's configuration to create an OAuth authorization request
            var scope = this.configuration.Scope;

            var url = new StringBuilder();
            url.Append(this.serverCodeFlowSettings.AuthorizationUrl);
            url.Append($"?client_id={HttpUtility.UrlEncode(this.configuration.ClientId)}");
            url.Append($"&redirect_uri={HttpUtility.UrlEncode($"http://127.0.0.1:{port}/callback")}");
            url.Append("&response_type=code");
            url.Append($"&scope={HttpUtility.UrlEncode(scope)}");
            url.Append($"&state={state}");
            url.Append($"&code_challenge={codeChallenge}");
            url.Append("&code_challenge_method=S256");
            
            // Uncomment this to force a new login every time you run the console client
            url.Append("&prompt=login");

            // Open the browser at the authorization URL to trigger user authentication
            Process.Start(new ProcessStartInfo() { FileName = url.ToString(), UseShellExecute = true });
        }

        /*
         * End a login by swapping the authorization code for a set of tokens
         */
        private async Task EndLoginAsync(int port, string requestState, string codeVerifier, Uri? authorizationResponseUrl)
        {
            if (authorizationResponseUrl == null)
            {
                throw new ClientError("login_response_error", "No authorization response URL was received");
            }
            
            var args = HttpUtility.ParseQueryString(authorizationResponseUrl.Query);
            var code = args["code"];
            var responseState = args["state"];
            var authorizationError = args["error"];
            var authorizationErrorDescription = args["error_description"];
            if (!string.IsNullOrWhiteSpace(authorizationError))
            {
                throw new ClientError(authorizationError, authorizationErrorDescription ?? "The authorization response returned an error");
            }

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(responseState)) {
                throw new ClientError("login_response_error", "The authorization response contains unexpected data");
            }

            if (responseState != requestState)
            {
                throw new ClientError("invalid_response_state", "The login response contained an invalid state value");
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("accept", "application/json");
                var requestData = new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", this.configuration.ClientId),
                    new KeyValuePair<string, string>("redirect_uri", $"http://127.0.0.1:{port}/callback"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("code_verifier", codeVerifier),
                };

                try
                {
                    var response = await client.PostAsync(this.serverCodeFlowSettings.TokenUrl, new FormUrlEncodedContent(requestData));
                    var responseText = await response.Content.ReadAsStringAsync();
                    var responseData = JsonNode.Parse(responseText);

                    if (!response.IsSuccessStatusCode)
                    {
                        var tokenErrorCode = responseData?["error"]?.GetValue<string>() ??
                            "token_response_error";
                        var tokenErrorDescription = responseData?["error_description"]?.GetValue<string>() ??
                            "Problem encountered exchanging the authorization code for tokens";

                        var tokenError = new ClientError(tokenErrorCode, tokenErrorDescription)
                        {
                            StatusCode = (int)response.StatusCode
                        };
                        throw tokenError;
                    }

                    var tokenData = JsonNode.Parse(responseText);
                    var accessToken = responseData?["access_token"]?.GetValue<string>();;
                    if (string.IsNullOrWhiteSpace(accessToken))
                    {
                        throw new ClientError("token_response_error", "No access token was received");
                    }

                    this.accessToken = accessToken;
                }
                catch (HttpRequestException exception)
                {
                    throw new ClientError("token_request_error", "Unable to connect to the token endpoint", exception);
                }
            }
        }

        /*
         * Return the access token after login
         */
        public string? GetAccessToken()
        {
            return this.accessToken;
        }

        /*
         * Get and close a port between 1024 and 65535
         */
        private int FindFreeLoopbackPort()
        {
            using (var listener = new TcpListener(IPAddress.Loopback, 0))
            {
                listener.Start();
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
        }
    }
}
