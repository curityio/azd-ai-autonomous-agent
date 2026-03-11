param name string
param location string = resourceGroup().location
param tags object = {}
param fileShareName string = 'curity-config'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2025-06-01' = {
  name: 'default'
  parent: storageAccount
}

resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2025-06-01' = {
  name: fileShareName
  parent: fileService
  properties: {
    shareQuota: 1
  }
}

output id string = storageAccount.id
output name string = storageAccount.name
output fileShareName string = fileShare.name
