targetScope = 'resourceGroup'

param name string
param location string = resourceGroup().location
param tags object = {}
param environmentName string
param containerAppsEnvironmentId string
param containerRegistryName string
param identityId string
param externalDomainName string
param uniquePrefix string
param imageName string
param managedIdentityClientId string

@secure()
param tokenExchangeClientSecret string

resource autonomousagent 'Microsoft.App/containerApps@2025-07-01' = {
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
        targetPort: 3000
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
              value: '3000'
            }
            {
              name: 'EXTERNAL_BASE_URL'
              value: 'https://gateway-external-${environmentName}.${externalDomainName}/a2a'
            }
            {
              name: 'ISSUER'
              value: 'https://idsvr-runtime-${environmentName}.${externalDomainName}/oauth/v2/oauth-anonymous'
            }
            {
              name: 'AUDIENCE'
              value: 'https://agent.demo.example'
            }
            {
              name: 'ALGORITHM'
              value: 'ES256'
            }
            {
              name: 'AUTHORIZATION_URL'
              value: 'https://idsvr-runtime-${environmentName}.${externalDomainName}/oauth/v2/oauth-authorize'
            }
            {
              name: 'TOKEN_URL'
              value: 'https://idsvr-runtime-${environmentName}.${externalDomainName}/oauth/v2/oauth-token'
            }
            {
              name: 'SCOPE'
              value: 'stocks/read'
            }
            {
              name: 'TOKEN_EXCHANGE_CLIENT_ID'
              value: 'autonomous-agent'
            }
            {
              name: 'TOKEN_EXCHANGE_CLIENT_SECRET'
              value: tokenExchangeClientSecret
            }
            {
              name: 'TOKEN_EXCHANGE_TARGET_AUDIENCE'
              value: 'https://mcp.demo.example'
            }
            {
              name: 'TOKEN_EXCHANGE_CACHE_SECONDS'
              value: '300'
            }
            {
              name: 'PORTFOLIO_MCP_SERVER_URL'
              value: 'http://gateway-internal-${environmentName}/mcp'
            }
            {
              name: 'AZURE_AI_FOUNDRY_PROJECT_URL'
              value: 'https://ai-${uniquePrefix}.cognitiveservices.azure.com/api/projects/proj-default'
            }
            {
              name: 'AZURE_AI_MODEL_NAME'
              value: 'gpt-4.1-mini'
            }
            {
              name: 'MANAGED_IDENTITY_CLIENT_ID'
              value: managedIdentityClientId
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
