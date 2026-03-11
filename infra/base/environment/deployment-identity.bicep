targetScope = 'resourceGroup'

param name string
param location string = resourceGroup().location
param tags object

resource deploymentIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: name
  location: location
  tags: tags
}

output id string = deploymentIdentity.id
output principalId string = deploymentIdentity.properties.principalId
