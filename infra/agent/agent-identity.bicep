targetScope = 'resourceGroup'

param name string
param location string = resourceGroup().location
param tags object

// An identity for the autonomous agent
resource agentIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: name
  location: location
  tags: tags
}

output id string = agentIdentity.id
output principalId string = agentIdentity.properties.principalId
output clientId string = agentIdentity.properties.clientId
