targetScope = 'resourceGroup'

param name string
param location string
param tags object = {}
param identityId string
param containerAppsEnvironmentId string
param containerRegistryName string
param imageName string
param sqlServerName string
param sqlDatabaseName string
param sqlAdminUsername string
@secure()
param sqlAdminPassword string

resource containerApp 'Microsoft.App/containerApps@2025-07-01' = {
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
      ingress: {
        external: true
        targetPort: 8000
        allowInsecure: false
      }
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
          env: [
            {
              name: 'SQL_SERVER_NAME'
              value: sqlServerName
            }
            {
              name: 'SQL_DATABASE_NAME'
              value: sqlDatabaseName
            }
            {
              name: 'SQL_ADMIN_USERNAME'
              value: sqlAdminUsername
            }
            {
              name: 'SQL_ADMIN_PASSWORD'
              value: sqlAdminPassword
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}
