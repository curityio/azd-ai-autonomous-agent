/**
 * @param {se.curity.identityserver.procedures.context.OAuthTokenExchangeUnInitializedProcedureContext} context
 */
function result(context) {
  
  var tokenData = context.getPresentedSubjectToken();
  var presentedDelegation = context.getPresentedSubjectTokenDelegation();

  if (!tokenData.client_id) {
    tokenData.client_id = presentedDelegation.clientId;
  }
  
  var newAudience = context.request.getFormParameter('audience');
  if (newAudience) {
    tokenData.aud = [newAudience];
  }

  var scopes = tokenData.scope.split(' ');
  var newScope = context.request.getFormParameter('scope');
  if (newScope) {
    tokenData.scope = newScope;
    scopes = newScope.split(' ');
  }

  var clientType = context.client.properties['client_type'];
  if (clientType) {
    tokenData.client_type = clientType;
    if (clientType == 'ai-agent') {
      var agentClientId = context.request.getFormParameter('client_id');
      if (agentClientId) {
        tokenData.agent_id = agentClientId;
      }
    }
  }
  
  var fullContext = context.getInitializedContext(
    context.subjectAttributes(),
    context.contextAttributes(),
    tokenData.aud,
    scopes
  );

  var issuedAccessToken = fullContext
    .getDefaultAccessTokenJwtIssuer()
    .issue(tokenData, presentedDelegation);

  return {
    scope: scopes,
    access_token: issuedAccessToken,
    token_type: 'bearer',
    expires_in: secondsUntil(tokenData.exp),
    issued_token_type: 'urn:ietf:params:oauth:token-type:access_token',
  };
}
