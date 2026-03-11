//
// Provision the main infrastructure before deploying application components
// https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/quickstart-create-bicep-use-visual-studio-code?tabs=azure-cli
//

targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment, which can be used as part of a resource naming convention')
param environmentName string

@minLength(1)
@description('Deployment location for all resources')
param location string

// Tags that should be applied to all resources.
var tags = {
  'azd-env-name': environmentName
}

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

// Get a prefix for resources that need to be globally unique, which is different per user who spins up the deployment
param uuid string = newGuid()
var uniquePrefix = uniqueString(uuid)

// Add a storage account with which to deploy files
var storageAccountName = uniquePrefix
module storageAccount 'storage/storage-account.bicep' = {
  name: 'storage-account'
  scope: rg
  params: {
    name: storageAccountName
    location: location
    tags: tags
    fileShareName: 'config-files'
  }
}

// Add a virtual network for the Container Apps Environment, to enable interaction with managed Azure resources
module vnet 'network/vnet.bicep' = {
  name: 'vnet'
  scope: rg
  params: {
    name: 'vnet-${environmentName}'
    location: location
    tags: tags
  }
}

// Add the container apps environment
module containerAppsEnvironment 'environment/container-apps-environment.bicep' = {
  name: 'container-apps-environment'
  scope: rg
  dependsOn: [storageAccount]
  params: {
    name: 'cae-${environmentName}'
    location: location
    tags: tags
    storageAccountName: storageAccountName
    fileShareName: 'config-files'
    infrastructureSubnetId: vnet.outputs.subnetId
  }
}

// Create a managed identity with permissions to pull Docker images from an Azure private registry
module identity 'environment/deployment-identity.bicep' = {
  scope: rg
  name: 'deployment-identity'
  params: {
    name: 'deployment-identity'
    location: location
    tags: tags
  }
}

// Create an Azure container registry
var registryName = uniquePrefix
module containerRegistry 'environment/container-registry.bicep' = {
  scope: rg
  name: registryName
  params: {
    name: registryName
    location: location
    principalId: identity.outputs.principalId
    tags: tags
  }
}

// Create the Azure AI Foundry resource, project and model
module aiFoundry 'ai/foundry.bicep' = {
  scope: rg
  params: {
    name: uniquePrefix
    location: location
    tags: tags
  }
}

// Outputs are written to a location like .azure/dev/.env and can be used for subsequent infrastructure layers and service deployments
output AZURE_RESOURCE_GROUP string = rg.name
output UNIQUE_PREFIX string = uniquePrefix
output AZURE_CONTAINER_ENVIRONMENT_NAME string = containerAppsEnvironment.outputs.name
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.outputs.endpoint
output STORAGE_ACCOUNT_NAME string = storageAccountName
output STORAGE_FILE_SHARE_NAME string = 'config-files'
output CONTAINER_APPS_ENVIRONMENT_ID string = containerAppsEnvironment.outputs.id
output CONTAINER_REGISTRY_NAME string = containerRegistry.outputs.name
output DEPLOYMENT_IDENTITY_ID string = identity.outputs.id
output EXTERNAL_DOMAIN_NAME string = containerAppsEnvironment.outputs.defaultDomain
