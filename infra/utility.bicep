//
// Deploy the workload as a container app
//

targetScope = 'resourceGroup'
param location string = resourceGroup().location
param environmentName string
param containerAppsEnvironmentId string
param containerRegistryName string
param identityId string
param imageName string

var name = 'utility-${environmentName}'
var tags = {
  'azd-env-name': environmentName
  'azd-service-name':  name
}

//
// 'azd deploy' adds the image name to the .env file
//
resource utility 'Microsoft.App/containerApps@2025-07-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {
      }
    }
  }   
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      registries: [
        {
          server: '${containerRegistryName}.azurecr.io'
          identity: identityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'main'
          image: imageName
          resources: {
            cpu: 1
            memory: '2Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}
