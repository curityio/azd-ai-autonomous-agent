targetScope = 'resourceGroup'

param name string
param location string = resourceGroup().location
param tags object = {}
param containerAppsEnvironmentId string
param containerRegistryName string
param identityId string

@secure()
param smtpSecret string

resource smtpServer 'Microsoft.App/containerApps@2025-07-01' = {
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
        targetPort: 1080
        allowInsecure: false
      }
    }
    template: {
      containers: [
        {
          name: 'main'
          image: 'maildev/maildev:latest'
          resources: {
            cpu: 1
            memory: '2Gi'
          }
          env: [
            {
              name: 'MAILDEV_INCOMING_USER'
              value: 'noreply@curitydemo.example'
            }
            {
              name: 'MAILDEV_INCOMING_PASSWORD'
              value: smtpSecret
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
