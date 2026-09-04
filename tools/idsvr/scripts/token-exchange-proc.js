/**
 * @param {se.curity.identityserver.procedures.context.OAuthTokenExchangeUnInitializedProcedureContext} context
 */
function result(context) {
  
  var tokenData = context.getPresentedSubjectToken();
  var presentedDelegation = context.getPresentedSubjectTokenDelegation();
  
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
  
  var fullContext = context.getInitializedContext(
    context.subjectAttributes(),
    context.contextAttributes(),
    tokenData.aud,
    scopes
  );

  var newTokenData = fullContext.getDefaultAccessTokenData();
  if (fullContext.client.properties.agent_role && fullContext.client.properties.agent_department) {
    newTokenData.act = {
      sub: fullContext.client.id,
      agent_role: fullContext.client.properties.agent_role,
      agent_department: fullContext.client.properties.agent_department,
    };
  }

  var issuedAccessToken = fullContext
    .getDefaultAccessTokenJwtIssuer()
    .issue(newTokenData, presentedDelegation);

  return {
    scope: scopes,
    access_token: issuedAccessToken,
    token_type: 'bearer',
    expires_in: secondsUntil(newTokenData.exp),
    issued_token_type: 'urn:ietf:params:oauth:token-type:access_token',
  };
}
