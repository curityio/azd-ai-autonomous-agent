targetScope = 'resourceGroup'

param containerRegistryName string
param principalId string

// Get the container registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-11-01' existing = {
  name: containerRegistryName
}

// Add permissions for the deployment identity to pull from the container registry
resource roleAssignment1 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  scope: containerRegistry
  name: guid(containerRegistry.id, principalId, 'AcrPull')
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalType: 'ServicePrincipal'
  }
}

// Grant the agent identity the 'Azure AI user' role for Cognitive services
resource roleAssignment2 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  name: guid(resourceGroup().id, principalId, 'CognitiveServices')
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'a97b65f3-24c7-4388-baec-2e87135dc908')
    principalType: 'ServicePrincipal'
  }
}
