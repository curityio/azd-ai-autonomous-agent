#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

###################################################################################
# The predown hook is called for both the 'base' and 'identity' provisioning layers
###################################################################################

set -euo pipefail
echo 'Running preprovision logic ...'

#
# Get environment variables to understand the stage of processing and ensure that azd env new has been run
#
source <(azd env get-values)

#
# Use environment variables to work out the provisioning layer
#
if [ -z "${EXTERNAL_DOMAIN_NAME:-}" ]; then
  PROVISIONING_STAGE='BASE'
else
  PROVISIONING_STAGE='IDENTITY'
fi

#
# Validate any required environment variables
#
if [ -z "${AZURE_ENV_NAME:-}" ]; then
  echo "Preprovision error: AZURE_ENV_NAME not set - please create an environment before running azd up."
  exit 1
fi

#
# A utility to generate strong passwords if they do not yet exist
#
function generatePassword() {
  openssl rand 32 | base64 | tr '/+' '_-' | tr -d '='
}

#
# A utility to store secrets in the deployment's Azure key vault
# Use az to avoid prompts and update the .env file so that 'azd pipeline config' can copy the secret to GitHub
# - https://github.com/Azure/azure-dev/blob/main/cli/azd/docs/using-environment-secrets.md
#
function setSecret() {
  local KEY="$1"
  local VALUE="$2"

  VALUE=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "SQL-ADMIN-PASSWORD" --query "value" -o tsv)
  if [ "$VALUE" == '' ]; then

    echo "Creating secret: $KEY ..."
    az keyvault secret set \
        --vault-name "$KEY_VAULT_NAME" \
        --name "$KEY" \
        --value "$VALUE" 1>/dev/null
    
    ENV_KEY="${KEY//-/_}"
    ENV_VALUE="akvs://${AZURE_SUBSCRIPTION_ID}/${$KEY_VAULT_NAME}/$KEY"
    echo "$ENV_KEY=$ENV_VALUE" >> ../../.azure/${AZURE_ENV_NAME}/.env 
  fi
}

#
# Implement identity provisioning logic to generate strong secrets, create Docker containers etc
#
if [ "$PROVISIONING_STAGE" == 'IDENTITY' ]; then

  setSecret 'SQL-ADMIN-PASSWORD' 'SQL-ADMIN-PASSWORD' "$(generatePassword)"
  setSecret 'ADMIN-PASSWORD' "$(generatePassword)"
  setSecret 'GATEWAY-TOKEN-EXCHANGE-SECRET' "$(generatePassword)"
  setSecret 'AGENT-TOKEN-EXCHANGE-SECRET' "$(generatePassword)"
  
  LICENSE_KEY="$(cat ../../tools/idsvr/license.json | jq -r .License)"
  if [ "$LICENSE_KEY" == '' ]; then
    echo 'Unable to find a license key for the Curity Identity Server'
    exit 1
  fi
  setSecret 'LICENSE-KEY' "$LICENSE_KEY"

  ./gateway-external/preprovision.sh
  ./gateway-internal/preprovision.sh
  ./idsvr/preprovision.sh
fi

