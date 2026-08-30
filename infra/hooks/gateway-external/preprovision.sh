#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

#####################################################################
# Configure routes using the newly created container apps domain name
# Also push Docker containers using the newly created Docker registry
#####################################################################

echo 'Running preprovision logic for the external gateway ...'
export AZURE_ENV_NAME
export EXTERNAL_DOMAIN_NAME

#
# In local Azure deployments, we need to read the value from the Azure key vault
# In GitHub workflows, this value is provided as a GitHub secret.
#
if [ -z "${GITHUB_ACTION:-}" ]; then
  export GATEWAY_TOKEN_EXCHANGE_SECRET=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "GATEWAY-TOKEN-EXCHANGE-SECRET" --query "value" -o tsv)
else
  export GATEWAY_TOKEN_EXCHANGE_SECRET
fi

#
# Update gateway hostname based routes
#
cd ../../../tools/gateway-external
envsubst < azure-routes-template.yml > azure-external-routes.yml
if [ $? -ne 0 ]; then
  echo 'envsubst failed for external gateway'
  exit 1
fi
