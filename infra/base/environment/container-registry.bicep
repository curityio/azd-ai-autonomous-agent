targetScope = 'resourceGroup'

param name string
param location string = resourceGroup().location
param tags object
param principalId string

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  scope: containerRegistry
  name: guid(containerRegistry.id, principalId, 'AcrPull')
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalType: 'ServicePrincipal'
  }
}

output name string = containerRegistry.name
output endpoint string = containerRegistry.properties.loginServer
