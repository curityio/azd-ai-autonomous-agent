/**
 * @param {se.curity.identityserver.procedures.context.OpenIdConnectAuthorizationCodeTokenProcedureContext} context
 */
function result(context) {
  var delegationData = context.getDefaultDelegationData();
  var issuedDelegation = context.delegationIssuer.issue(delegationData);

  var accessTokenType = context.client.properties['access_token_type'] || '';
  var accessTokenIssuer = (accessTokenType === 'jwt') ? context.getDefaultAccessTokenJwtIssuer() : context.accessTokenIssuer;

  var accessTokenData = context.getDefaultAccessTokenData();
  accessTokenData.client_id = context.client.id;
  var issuedAccessToken = accessTokenIssuer.issue(accessTokenData, issuedDelegation);

  var refreshTokenData = context.getDefaultRefreshTokenData();
  var issuedRefreshToken = context.refreshTokenIssuer.issue(refreshTokenData, issuedDelegation);

  var responseData = {
    access_token: issuedAccessToken,
    scope: accessTokenData.scope,
    refresh_token: issuedRefreshToken,
    token_type: 'bearer',
    expires_in: secondsUntil(accessTokenData.exp),
  };

  var idTokenData = context.getDefaultIdTokenData();
  if (idTokenData) {
    var idTokenIssuer = context.idTokenIssuer;
    idTokenData.at_hash = idTokenIssuer.atHash(issuedAccessToken);
    responseData.id_token = idTokenIssuer.issue(idTokenData, issuedDelegation);
  }

  return responseData;
}