targetScope = 'resourceGroup'

param name string
param location string = resourceGroup().location
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
        external: false
        targetPort: 8000
        allowInsecure: true
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
              value: '/usr/local/kong/declarative/kong-internal.yml'
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
              value: 'bundled,token-audit'
            }
            {
              name: 'KONG_NGINX_HTTP_LOG_FORMAT'
              value: 'json_combined escape=json \'{ "time_iso8601": "$$time_iso8601", "remote_addr": "$$remote_addr", "method": "$$request_method", "uri": "$$request_uri", "status": $$status, "body_bytes_sent": $$body_bytes_sent, "request_time": $$request_time, "upstream_response_time": "$$upstream_response_time", "upstream_addr": "$$upstream_addr", "http_x_forwarded_for": "$$http_x_forwarded_for", "kong_request_id": "$$http_kong_request_id" }\''
            }
            {
              name: 'KONG_PROXY_ACCESS_LOG'
              value: '/dev/stdout json_combined'
            }
          ]
          volumeMounts: [
            {
              volumeName: 'gateway-internal-routes'
              mountPath: '/usr/local/kong/declarative/kong-internal.yml'
              subPath: 'kong-internal.yml'
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
          name: 'gateway-internal-routes'
          storageType: 'AzureFile'
          storageName: 'config-storage'
        }
      ]
    }
  }
}
