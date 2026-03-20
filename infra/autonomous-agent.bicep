//
// Deploy the workload as a container app
//

targetScope = 'resourceGroup'
param location string = resourceGroup().location
param environmentName string
param aiFoundryName string
param containerAppsEnvironmentId string
param containerRegistryName string
param externalDomainName string
param imageName string

@secure()
param tokenExchangeClientSecret string

var name = 'autonomous-agent-${environmentName}'
var tags = {
  'azd-env-name': environmentName
  'azd-service-name': name
}

// Create a managed identity for the agent
module identity 'agent/agent-identity.bicep' = {
  name: 'agent-identity'
  params: {
    name: 'agent-identity'
    location: location
    tags: tags
  }
}

// Assign the managed identity roles to pull Docker images and to use Cognitive servicews
module identityRoles 'agent/agent-roles.bicep' = {
  params: {
    containerRegistryName: containerRegistryName
    principalId: identity.outputs.principalId
  }
}

// Deploy the agent as a container app with a managed identity that can connect to Azure AI foundry
module containerApp 'agent/container-app.bicep' = {
  params: {
    name: name
    location: location
    environmentName: environmentName
    containerAppsEnvironmentId: containerAppsEnvironmentId
    containerRegistryName: containerRegistryName
    identityId: identity.outputs.id
    externalDomainName: externalDomainName
    aiFoundryName: aiFoundryName
    imageName: imageName
    managedIdentityClientId: identity.outputs.clientId
    tokenExchangeClientSecret: tokenExchangeClientSecret
  }
}
