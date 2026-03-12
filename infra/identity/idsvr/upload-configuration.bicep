targetScope = 'resourceGroup'

param storageAccountName string
param fileShareName string
param location string
param tags object = {}
param utcTime string = utcNow()

@secure()
param clusterXmlContent string

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
  name: storageAccountName
}

resource uploadScript 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: 'upload-config-files'
  location: location
  kind: 'AzureCLI'
  tags: tags
  properties: {
    azCliVersion: '2.52.0'
    retentionInterval: 'PT1H'
    timeout: 'PT10M'
    cleanupPreference: 'OnSuccess'
    forceUpdateTag: utcTime
    environmentVariables: [
      {
        name: 'STORAGE_ACCOUNT_NAME'
        value: storageAccountName
      }
      {
        name: 'STORAGE_ACCOUNT_KEY'
        secureValue: storageAccount.listKeys().keys[0].value
      }
      {
        name: 'FILE_SHARE_NAME'
        value: fileShareName
      }
      {
        name: 'CLUSTER_XML_CONTENT'
        secureValue: clusterXmlContent
      }
    ]
    scriptContent: '''
      set -euo pipefail

      # Delete any existing cluster.xml file
      az storage file delete \
        --account-name "$STORAGE_ACCOUNT_NAME" \
        --account-key "$STORAGE_ACCOUNT_KEY" \
        --share-name "$FILE_SHARE_NAME" \
        --path cluster.xml 2>/dev/null || true

      # Write and upload cluster.xml
      echo "$CLUSTER_XML_CONTENT" > cluster.xml
      az storage file upload \
        --account-name "$STORAGE_ACCOUNT_NAME" \
        --account-key "$STORAGE_ACCOUNT_KEY" \
        --share-name "$FILE_SHARE_NAME" \
        --source cluster.xml \
        --path cluster.xml
    '''
  }
}
