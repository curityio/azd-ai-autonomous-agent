//
// Provision the main infrastructure before deploying application components
// https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/quickstart-create-bicep-use-visual-studio-code?tabs=azure-cli
//

targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment, which can be used as part of a resource naming convention')
param environmentName string

@minLength(1)
@description('Deployment location for all resources')
param location string = resourceGroup().location

// Tags that should be applied to all resources.
var tags = {
  'azd-env-name': environmentName
}

@description('A unique prefix for resources that must be globally unique')
param uniquePrefix string

@description('Storage account name for file uploads')
param storageAccountName string

@description('Container apps environment ID to deploy to')
param containerAppsEnvironmentId string

@description('The external domain name for public HTTPS URLs')
param externalDomainName string

@description('The Azure container registry name')
param containerRegistryName string

@description('The deployment identity to pull images from the Docker registry')
param identityId string

@description('The Docker image for the external gateway')
param gatewayExternalImageName string

@description('The Docker image for the internal gateway')
param gatewayInternalImageName string

@description('The Docker image for the Curity Identity Server')
param idsvrImageName string

@description('SQL Server administrator username')
param sqlAdminUsername string = 'sqladmin'

@secure()
@description('SQL Server administrator password')
param sqlAdminPassword string

@description('The Docker image for the database initialization job')
param dbinitImageName string

@secure()
@description('Curity Identity Server admin password')
param adminPassword string

@secure()
@description('License key for the Curity Identity Server')
param licenseKey string

@description('Entra ID app registration client ID')
param entraClientId string

@secure()
@description('Entra ID app registration client secret')
param entraClientSecret string

@description('OpenID Connect metadata document URL for Entra ID (v2)')
param entraOidcMetadataUrl string

@secure()
@description('The token exchange client ID secret that the external gateway uses.')
param gatewayTokenExchangeClientSecret string

@secure()
@description('The token exchange client ID secret that the backend agent uses.')
param agentTokenExchangeClientSecret string

// Configuration upload for the external gateway
module externalGatewayConfiguration 'gateway-external/upload-configuration.bicep' = {
  name: 'upload-gateway-external-configuration'
  params: {
    storageAccountName: storageAccountName
    fileShareName: 'config-files'
    location: location
    azureRoutesContent: loadTextContent('../../tools/gateway-external/azure-external-routes.yml')
  }
}

// Container app for the external gateway
module externalGatewayContainerApp 'gateway-external/container-app.bicep' = {
  name: 'container-app-gateway-external'
  params: {
    name: 'gateway-external-${environmentName}'
    location: location
    tags: tags
    identityId: identityId
    containerAppsEnvironmentId: containerAppsEnvironmentId
    containerRegistryName: containerRegistryName
    imageName: gatewayExternalImageName
  }
}

// Configuration upload for the internal gateway
module internalGatewayConfiguration 'gateway-internal/upload-configuration.bicep' = {
  name: 'upload-gateway-internal-configuration'
  params: {
    storageAccountName: storageAccountName
    fileShareName: 'config-files'
    location: location
    azureRoutesContent: loadTextContent('../../tools/gateway-internal/azure-internal-routes.yml')
  }
}

// Container app for the internal gateway
module internalGatewayContainerApp 'gateway-internal/container-app.bicep' = {
  name: 'container-app-gateway-internal'
  params: {
    name: 'gateway-internal-${environmentName}'
    location: location
    tags: tags
    identityId: identityId
    containerAppsEnvironmentId: containerAppsEnvironmentId
    containerRegistryName: containerRegistryName
    imageName: gatewayInternalImageName
  }
}

// Create a SQL Server and database for the Curity Identity Server
var sqlServerName = uniquePrefix
var databaseName = 'curity-db'
module sqlServer 'idsvr/sqlserver.bicep' = {
  name: 'sql-server'
  params: {
    name: sqlServerName
    location: location
    tags: tags
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    databaseName: databaseName
  }
}

// Container app for the internal gateway
module dbinitContainerApp 'idsvr/dbinit-container-app.bicep' = {
  name: 'container-app-dbinit'
  dependsOn: [sqlServer]
  params: {
    name: 'dbinit-${environmentName}'
    location: location
    tags: tags
    identityId: identityId
    containerAppsEnvironmentId: containerAppsEnvironmentId
    containerRegistryName: containerRegistryName
    imageName: dbinitImageName
    sqlServerName: sqlServerName
    sqlDatabaseName: databaseName
    sqlAdminUsername: sqlAdminUsername
    sqlAdminPassword: sqlAdminPassword
  }
}

// Upload Curity Identity Server configuration files to Azure
module uploadConfigFiles 'idsvr/upload-configuration.bicep' = {
  name: 'upload-config-files'
  params: {
    storageAccountName: storageAccountName
    fileShareName: 'config-files'
    location: location
    clusterXmlContent: loadTextContent('../../tools/idsvr/cluster.xml')
  }
}

// Deploy the admin workload for the Curity Identity Server
module containerAppAdmin 'idsvr/idsvr-container-app.bicep' = {
  name: 'container-app-admin'
  params: {
    name: 'idsvr-admin-${environmentName}'
    location: location
    tags: tags
    identityId: identityId
    containerAppsEnvironmentId: containerAppsEnvironmentId
    containerRegistryName: containerRegistryName
    imageName: idsvrImageName
    targetPort: 6749
    minReplicas: 1
    maxReplicas: 1
    cpu: '2'
    memory: '4Gi'
    storageAccountName: storageAccountName
    additionalPorts: [
      {
        external: false
        targetPort: 6789
        exposedPort: 6789
      }
      {
        external: false
        targetPort: 6790
        exposedPort: 6790
      }
      {
        external: false
        targetPort: 4465
        exposedPort: 4465
      }
      {
        external: false
        targetPort: 4466
        exposedPort: 4466
      }
    ]
    env: [
      {
        name: 'ADMIN'
        value: 'true'
      }
      {
        name: 'ADMIN_PASSWORD'
        value: adminPassword
      }
      {
        name: 'SERVICE_ROLE'
        value: 'admin'
      }
      {
        name: 'ADMIN_UI_HTTP_MODE'
        value: 'true'
      }
      {
        name: 'LICENSE_KEY'
        value: licenseKey
      }
      {
        name: 'IDSVR_ADMIN_URL'
        value: 'https://idsvr-admin-${environmentName}.${externalDomainName}'
      }
      {
        name: 'IDSVR_RUNTIME_URL'
        value: 'https://idsvr-runtime-${environmentName}.${externalDomainName}'
      }
      {
        name: 'SQL_SERVER_NAME'
        value: sqlServerName
      }
      {
        name: 'SQL_DATABASE_NAME'
        value: databaseName
      }
      {
        name: 'SQL_ADMIN_PASSWORD'
        value: sqlAdminPassword
      }
      {
        name: 'SQL_ADMIN_USERNAME'
        value: sqlAdminUsername
      }
      {
        name: 'ENTRA_CLIENT_ID'
        value: entraClientId
      }
      {
        name: 'ENTRA_CLIENT_SECRET'
        value: entraClientSecret
      }
      {
        name: 'ENTRA_OIDC_METADATA_URL'
        value: entraOidcMetadataUrl
      }
      {
        name: 'GATEWAY_TOKEN_EXCHANGE_SECRET'
        value: gatewayTokenExchangeClientSecret
      }
      {
        name: 'AGENT_TOKEN_EXCHANGE_SECRET'
        value: agentTokenExchangeClientSecret
      }
    ]
  }
}

// Deploy runtime workloads for the Curity Identity Server
module containerAppRuntime 'idsvr/idsvr-container-app.bicep' = {
  name: 'container-app-runtime'
  params: {
    name: 'idsvr-runtime-${environmentName}'
    location: location
    tags: tags
    identityId: identityId
    containerAppsEnvironmentId: containerAppsEnvironmentId
    containerRegistryName: containerRegistryName
    imageName: idsvrImageName
    targetPort: 8443
    minReplicas: 1
    maxReplicas: 5
    cpu: '2'
    memory: '4Gi'
    storageAccountName: storageAccountName
    additionalPorts: [
      {
        external: false
        targetPort: 6790
        exposedPort: 6790
      }
      {
        external: false
        targetPort: 4465
        exposedPort: 4465
      }
      {
        external: false
        targetPort: 4466
        exposedPort: 4466
      }
    ]
    env: [
      {
        name: 'ADMIN'
        value: 'false'
      }
      {
        name: 'ADMIN_PASSWORD'
        value: adminPassword
      }
      {
        name: 'SERVICE_ROLE'
        value: 'default'
      }
      {
        name: 'LICENSE_KEY'
        value: licenseKey
      }
      {
        name: 'ADMIN_UI_HTTP_MODE'
        value: 'true'
      }
      {
        name: 'ADMIN_REVISION'
        value: containerAppAdmin.outputs.latestRevisionName
      }
      {
        name: 'IDSVR_ADMIN_URL'
        value: 'https://idsvr-admin-${environmentName}.${externalDomainName}'
      }
      {
        name: 'IDSVR_RUNTIME_URL'
        value: 'https://idsvr-runtime-${environmentName}.${externalDomainName}'
      }
      {
        name: 'SQL_SERVER_NAME'
        value: sqlServerName
      }
      {
        name: 'SQL_DATABASE_NAME'
        value: databaseName
      }
      {
        name: 'SQL_ADMIN_PASSWORD'
        value: sqlAdminPassword
      }
      {
        name: 'SQL_ADMIN_USERNAME'
        value: sqlAdminUsername
      }
      {
        name: 'ENTRA_CLIENT_ID'
        value: entraClientId
      }
      {
        name: 'ENTRA_CLIENT_SECRET'
        value: entraClientSecret
      }
      {
        name: 'ENTRA_OIDC_METADATA_URL'
        value: entraOidcMetadataUrl
      }
      {
        name: 'GATEWAY_TOKEN_EXCHANGE_SECRET'
        value: gatewayTokenExchangeClientSecret
      }
      {
        name: 'AGENT_TOKEN_EXCHANGE_SECRET'
        value: agentTokenExchangeClientSecret
      }
    ]
  }
}

// Outputs are written to a location like .azure/dev/.env and can be used for subsequent service deployments
output IDSVR_ADMIN_URL string = '${containerAppAdmin.outputs.uri}/admin'
output IDSVR_RUNTIME_URL string = containerAppRuntime.outputs.uri
output A2A_EXTERNAL_URL string = externalGatewayContainerApp.outputs.uri
