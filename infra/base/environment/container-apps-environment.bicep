param name string
param location string = resourceGroup().location
param tags object = {}
param storageAccountName string = ''
param fileShareName string = ''
param infrastructureSubnetId string = ''

// This module expects a storage account to already exist. If storageAccountName is empty,
// we simply won't create the managed environment storage mapping.
resource existingStorageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
  name: storageAccountName
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-10-02-preview' = {
  name: name
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'azure-monitor'
    }
    vnetConfiguration: !empty(infrastructureSubnetId) ? {
      infrastructureSubnetId: infrastructureSubnetId
      internal: false
    } : null
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

resource storage 'Microsoft.App/managedEnvironments/storages@2025-10-02-preview' = if (!empty(storageAccountName) && !empty(fileShareName)) {
  name: 'config-storage'
  parent: containerAppsEnvironment
  properties: {
    azureFile: {
      accountName: storageAccountName
      accountKey: existingStorageAccount.listKeys().keys[0].value
      shareName: fileShareName
      accessMode: 'ReadOnly'
    }
  }
}

output id string = containerAppsEnvironment.id
output name string = containerAppsEnvironment.name
output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
