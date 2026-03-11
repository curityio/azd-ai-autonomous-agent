param name string
param location string = resourceGroup().location
param tags object = {}
param identityId string
param containerAppsEnvironmentId string
param containerRegistryName string
param imageName string
param targetPort int = 80
param env array = []
param secrets array = []
param cpu string = '1'
param memory string = '3Gi'
param minReplicas int = 1
param maxReplicas int = 3
param volumes array = []
param storageAccountName string = ''
param additionalPorts array = []

// If storageAccountName is empty, no Azure Files volume/secrets are added and this resource is effectively unused.
resource existingStorageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
  name: storageAccountName
}

resource containerApp 'Microsoft.App/containerApps@2025-10-02-preview' = {
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
        targetPort: targetPort
        transport: 'auto'
        allowInsecure: false
        additionalPortMappings: additionalPorts
      }
      secrets: !empty(storageAccountName) ? union(secrets, [
        {
          name: 'storage-key'
          value: existingStorageAccount.listKeys().keys[0].value
        }
      ]) : secrets
    }
    template: {
      containers: [
        {
          name: 'main'
          image: imageName
          resources: {
            cpu: json(cpu)
            memory: memory
          }
          env: env
          volumeMounts: !empty(storageAccountName) ? [
            {
              volumeName: 'idsvr-config-volume'
              mountPath: '/opt/idsvr/etc/init/cluster.xml'
              subPath: 'cluster.xml'
            }
          ] : []
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                port: 4465
                path: '/'
                scheme: 'HTTP'
              }
              initialDelaySeconds: 30
              periodSeconds: 10
              timeoutSeconds: 5
              failureThreshold: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                port: 4465
                path: '/'
                scheme: 'HTTP'
              }
              initialDelaySeconds: 10
              periodSeconds: 5
              timeoutSeconds: 3
              failureThreshold: 3
            }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
      volumes: !empty(storageAccountName) ? union(volumes, [
        {
          name: 'idsvr-config-volume'
          storageType: 'AzureFile'
          storageName: 'config-storage'
        }
      ]) : volumes
    }
  }
}

output uri string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output name string = containerApp.name
output id string = containerApp.id
output fqdn string = containerApp.properties.configuration.ingress.fqdn
output latestRevisionName string = containerApp.properties.latestRevisionName
