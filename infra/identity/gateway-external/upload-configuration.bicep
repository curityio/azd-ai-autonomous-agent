targetScope = 'resourceGroup'

param storageAccountName string
param fileShareName string
param location string
param tags object = {}
param utcTime string = utcNow()

@secure()
param azureRoutesContent string

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
  name: storageAccountName
}

resource uploadScript 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: 'upload-external-gateway-routes'
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
        name: 'AZURE_ROUTES_CONTENT'
        secureValue: azureRoutesContent
      }
    ]
    scriptContent: '''
      set -euo pipefail

      az storage file delete \
        --account-name "$STORAGE_ACCOUNT_NAME" \
        --account-key "$STORAGE_ACCOUNT_KEY" \
        --share-name "$FILE_SHARE_NAME" \
        --path kong-external.yml 2>/dev/null || true

      echo "$AZURE_ROUTES_CONTENT" > kong-external.yml
      az storage file upload \
        --account-name "$STORAGE_ACCOUNT_NAME" \
        --account-key "$STORAGE_ACCOUNT_KEY" \
        --share-name "$FILE_SHARE_NAME" \
        --source kong-external.yml \
        --path kong-external.yml
    '''
  }
}
