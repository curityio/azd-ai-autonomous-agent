//
// Deploy the workload as a container app
//

targetScope = 'resourceGroup'
param location string = resourceGroup().location
param environmentName string
param containerAppsEnvironmentId string
param containerRegistryName string
param externalDomainName string
param identityId string
param imageName string

var name = 'portfolio-mcp-server-${environmentName}'
var tags = {
  'azd-env-name': environmentName
  'azd-service-name': name
}

//
// 'azd deploy' adds the image name to the .env file
//
resource portfoliomcpserver 'Microsoft.App/containerApps@2025-07-01' = {
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
        targetPort: 3001
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
              name: 'ENV'
              value: environmentName
            }
            {
              name: 'PORT'
              value: '3001'
            }
            {
              name: 'EXTERNAL_BASE_URL'
              value: 'https://gateway-external-${environmentName}.${externalDomainName}/mcp'
            }
            {
              name: 'AUTHORIZATION_SERVER_BASE_URL'
              value: 'https://idsvr-runtime-${environmentName}.${externalDomainName}'
            }
            {
              name: 'ISSUER'
              value: 'https://idsvr-runtime-${environmentName}.${externalDomainName}/oauth/v2/oauth-anonymous'
            }
            {
              name: 'AUDIENCE'
              value: 'https://mcp.demo.example'
            }
            {
              name: 'ALGORITHM'
              value: 'ES256'
            }
            {
              name: 'SCOPE'
              value: 'stocks/read'
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
