# OAuth Configuration

In this deployment, the Curity Identity Server's main role is a specialist token issuer.  
The Curity Identity Server does not need to store user accounts or authenticate users directly.   
The example deployment highlights some behaviors that enable you to take control over tokens.

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
To do so, follow the steps in the [Authenticate Using Microsoft Entra ID](https://curity.io/resources/learn/oicd-authenticator-azure/) tutorial.

## User Attributes

In the example deployment, the Curity Identity Server issues user attributes for `region` and `customer_id` to access tokens.  
If you store these values against user accounts in the Curity Identity Server, you would populate them during account creation.  
You could then use a [Script Claims Provider](https://curity.io/docs/identity-server/developer-guide/scripting/claims-value-provider-procedures/) to issue the values to access tokens.

If you store user accounts in an external identity system like Entra ID, you can get those values in a [Script Authentication Action](https://curity.io/docs/identity-server/profiles/authentication-profile/authentication-actions/script-transformer/).  

![Authentication action](images/authentication-action.png)

The example deployment uses the following example logic to demonstrate how to save federated user attributes to the authentication context.  
You can implement any custom logic on user attributes.

```javascript
function result(context) {
  var attributes = context.attributeMap;

  if (attributes.location && attributes.employee_id) {

    attributes.region = attributes.location;
    attributes.customer_id = attributes.employee_id;

  } else {

    attributes.region = "USA"
    attributes.customer_id = "2109"
  }

  return attributes;
}
```

## Scopes and Claims

Business scopes are defined in the Admin UI's token designer, with claim values evaluated at runtime.  
For each claim, you can resolve claim values in various ways.  
The example deployment shows how to use an `Authentication Context Claims Provider` to retrieve runtime values.

![Scopes and claims](images/scopes-and-claims.png)

## Token Exchange

Once user authentication completes, the console client receives an opaque access token.  
Tokens sent from the console client undergo 2 token exchanges that apply custom logic.  
To view the token exchange logic, navigate to `System / Procedures / Token Procedures`.  

![Token exchange logic](images/token-exchange-logic.png)
