# OAuth Configuration

In this deployment, the Curity Identity Server's main role is a specialist token issuer.  
The Curity Identity Server does not need to store user accounts or authenticate users directly.   

## Admin UI

After deployment, run the Admin UI for the Curity Identity Server, e.g.:

```bash
open $(azd env get-value IDSVR_ADMIN_URL)
```

Sign in with the following details:

- User: `admin`
- Password: The `ADMIN_PASSWORD` environment variable value

If you run the local deployment, navigate to `http://localhost:6749/admin` instead.  
You can find the generated `ADMIN_PASSWORD` in the `tools/local/load-secrets.sh` file.

## OAuth Clients

In the Admin UI, view the OAuth clients:

![Clients](images/clients.png)

The following components use the OAuth client settings to get access tokens:

- The console client runs a code flow with Entra ID user authentication
- The external gateway is a token exchange client
- The autonomous agent is also a token exchange client

## User Authentication

The example deployment uses [Passkeys](PASSKEYS.md) as the default authentication method.  
You can change that as required, for example to use Entra ID for user account storage and user logins.  

```xml
<authenticator>
  <id>entra</id>
  <authentication-actions>
    <login>post-login</login>
  </authentication-actions>
  <oidc xmlns="https://curity.se/ns/conf/authenticators/oidc">
    <configuration-url>#{ENTRA_OIDC_METADATA_URL}</configuration-url>
    <client-id>#{ENTRA_CLIENT_ID}</client-id>
    <client-secret>#{ENTRA_CLIENT_SECRET}</client-secret>
    <fetch-userinfo>
    </fetch-userinfo>
    <prompt-login>if-requested-by-client</prompt-login>
  </oidc>
</authenticator>
```

In Entra ID you would need to create an app registration for the Curity Identity Server.  
The Entra ID client should use a redirect URI of `https://<domain-name>/authn/authentication/<authenticator name>/callback`.

![Entra client](images/entra-client.png)

## User Attributes

In the example deployment, the Curity Identity Server uses user attributes for `region` and `customer_id`.  
You might configure those values in Entra ID in the following attributes:

![Entra user attributes](images/entra-user-attributes.png)

Once authentication completes, the Curity Identity Server can use a Script Authentication Action to receive attributes:

![Authentication action](images/authentication-action.png)

The following JavaScript logic runs, to transform Entra ID attributes and save them to the authentication context:

```javascript
function result(context) {
  var attributes = context.attributeMap;

  if (attributes.location && attributes.employee_id) {
    attributes.region = attributes.location;
    attributes.customer_id = attributes.employee_id;
  }

  return attributes;
}
```

## Scopes and Claims

Business scopes are defined in the Admin UI's token designer, with claim values evaluated at runtime.  
For each claim, the `Authentication Context Claims Provider` provides the value.

![Scopes and claims](images/scopes-and-claims.png)

You could use the token designer to resolve other claim values from Entra ID user attributes.  
For example, the console client could receive name details in its ID tokens:

- Create a `profile` scope that contains the `given_name` and `family_name` claims.
- Add the `profile` scope to the console client.
- Drag the `given_name` and `family_name` claims into the ID token pane.
- Use the the claims provider to set name claim values from Entra ID user attributes.

## Token Exchange

Once user authentication completes, the console client receives an opaque access token.  
Tokens sent from the console client undergo 2 token exchanges that apply custom logic.  
To view the token exchange logic, navigate to `System / Procedures / Token Procedures`.  

![Token exchange logic](images/token-exchange-logic.png)
