targetScope = 'resourceGroup'

param name string
param location string
param tags object = {}
param identityId string
param containerAppsEnvironmentId string
param containerRegistryName string
param imageName string

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
              name: 'KONG_DATABASE'
              value: 'off'
            }
            {
              name: 'KONG_DECLARATIVE_CONFIG'
              value: '/usr/local/kong/declarative/kong-external.yml'
            }
            {
              name: 'KONG_PROXY_LISTEN'
              value: '0.0.0.0:8000'
            }
            {
              name: 'KONG_LOG_LEVEL'
              value: 'warn'
            }
            {
              name: 'KONG_PLUGINS'
              value: 'bundled,token-exchange'
            }
            {
              name: 'KONG_NGINX_HTTP_LUA_SHARED_DICT'
              value: 'token-exchange 10m'
            }
          ]
          volumeMounts: [
            {
              volumeName: 'gateway-external-routes'
              mountPath: '/usr/local/kong/declarative/kong-external.yml'
              subPath: 'kong-external.yml'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
      volumes: [
        {
          name: 'gateway-external-routes'
          storageType: 'AzureFile'
          storageName: 'config-storage'
        }
      ]
    }
  }
}

output uri string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
