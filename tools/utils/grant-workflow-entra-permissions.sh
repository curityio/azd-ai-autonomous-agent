#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

###############################################################################################
# A script to grant a GitHub workflow managed identity permissions to create an Entra ID client
# - https://blog.asfritecx.com/assign-entra-ad-graph-permissions-to-managed-identity/
# - https://gist.github.com/garrytrinder/6352326eadbc9d00e808022ec724188e
###############################################################################################

AZURE_CLIENT_ID=$(az ad sp list --display-name "msi-ai-autonomous-agent" --query "[0].appId" --output tsv)
if [ "$AZURE_CLIENT_ID" == '' ]; then
  echo 'Unable to get the Azure Client ID for the deployment'
  exit 1
fi

TENANT_DOMAIN=$(az account show --query tenantDefaultDomain -o tsv)
if [ "$TENANT_DOMAIN" == '' ]; then
  echo 'Unable to get your Entra ID tenant domain'
  exit 1
fi

#
# Find the managed principal ID for the Azure client ID
#
echo 'Finding the GitHub workflow managed identity details ...'
MANAGED_IDENTITY_ID=$(az ad sp list --filter "appId eq '$AZURE_CLIENT_ID'" --query [].id --output tsv)
if [ "$MANAGED_IDENTITY_ID" == '' ]; then
  echo 'Unable to find the managed principal id for the GitHub workflow client'
  exit 1
fi

#
# Find the Graph service principal name
#
echo 'Finding the Graph service principal id ...'
GRAPH_APP_ID='00000003-0000-0000-c000-000000000000'
GRAPH_SPN_ID=$(az ad sp list --filter "appId eq '$GRAPH_APP_ID'" --query [].id --output tsv)
if [ "$GRAPH_SPN_ID" == '' ]; then
  echo 'Unable to find the Graph service principal id'
  exit 1
fi

#
# Find the Graph role to enable creation of Entra ID app registrations
#
PERMISSION='Application.ReadWrite.All'
echo "Finding the Graph role for the $PERMISSION permission ..."
GRAPH_ROLE_ID=$(az ad sp list --filter "appId eq '$GRAPH_APP_ID'" --query "[].appRoles [?value=='$PERMISSION'].id" --output tsv)
if [ "$GRAPH_ROLE_ID" == '' ]; then
  echo 'Unable to find the Graph role id'
  exit 1
fi

#
# Form the full URL
#
echo "Granting the GitHub workflow identity permissions to create an Entra ID client ..."
URL="https://graph.windows.net/$TENANT_DOMAIN/servicePrincipals/$MANAGED_IDENTITY_ID/appRoleAssignments?api-version=1.6"
BODY="{ \"principalId\": \"$MANAGED_IDENTITY_ID\", \"resourceId\": \"$GRAPH_SPN_ID\", \"id\": \"$GRAPH_ROLE_ID\"  }"
az rest --method POST --uri "$URL" --body "$BODY" 1>/dev/null
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Indicate success
#
echo 'The Graph service principal was successfully updated'
