/**
 * @param {se.curity.identityserver.procedures.claims.ClaimsProviderProcedureContext} context
 */
function result(context) {

  if (context.client.properties.agent_role && context.client.properties.agent_department) {
    return {
        agent_id: context.client.id,
        agent_role: context.client.properties.agent_role,
        agent_department: context.client.properties.agent_department,
    }
  }

  return {};
}
