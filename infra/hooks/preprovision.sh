#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

###################################################################################
# The predown hook is called for both the 'base' and 'identity' provisioning layers
###################################################################################

set -euo pipefail
echo 'Running preprovision logic ...'

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
  openssl rand 32 | base64 | tr -d '=/_-'
}

#
# A utility to store secrets in the deployment's Azure key vault
# Use az to avoid prompts and update the .env file so that 'azd pipeline config' can copy the secret to GitHub
# - https://github.com/Azure/azure-dev/blob/main/cli/azd/docs/using-environment-secrets.md
#
function setSecret() {
  local KEY="$1"
  local VALUE="$2"

  EXISTS=$(az keyvault secret list --vault-name "$KEY_VAULT_NAME" --query "contains([].id, 'https://$KEY_VAULT_NAME.vault.azure.net/secrets/$KEY')")
  if [ $EXISTS == false ]; then
  
    echo "Creating secret: $KEY ..."
    az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "$KEY" --value "$VALUE" 1>/dev/null

    ENV_KEY="${KEY//-/_}"
    ENV_VALUE="akvs://${AZURE_SUBSCRIPTION_ID}/${KEY_VAULT_NAME}/${KEY}"
    echo "$ENV_KEY=\"$ENV_VALUE\"" >> ../../.azure/${AZURE_ENV_NAME}/.env 
  fi
}

#
# Implement identity provisioning logic
#
if [ "$PROVISIONING_STAGE" == 'IDENTITY' ]; then

  #
  # In GitHub workflows, secrets are supplied as environment variables
  # In local Azure deployments, we create them on the first deployment
  #
  if [ -z "${SQL_ADMIN_PASSWORD:-}" ]; then
    setSecret 'SQL-ADMIN-PASSWORD' "$(generatePassword)"
  fi

  if [ -z "${ADMIN_PASSWORD:-}" ]; then
    setSecret 'ADMIN-PASSWORD' "$(generatePassword)"
  fi

  if [ -z "${GATEWAY_TOKEN_EXCHANGE_SECRET:-}" ]; then
    setSecret 'GATEWAY-TOKEN-EXCHANGE-SECRET' "$(generatePassword)"
  fi

  if [ -z "${AGENT_TOKEN_EXCHANGE_SECRET:-}" ]; then
    setSecret 'AGENT-TOKEN-EXCHANGE-SECRET' "$(generatePassword)"
  fi
  
  if [ -z "${LICENSE_KEY:-}" ]; then
    LICENSE_KEY="$(cat ../../tools/idsvr/license.json | jq -r .License)"
    if [ "$LICENSE_KEY" == '' ]; then
      echo 'Unable to find a license key for the Curity Identity Server'
      exit 1
    fi
    setSecret 'LICENSE-KEY' "$LICENSE_KEY"
  fi

  #
  # Run other logic, to configure each component and deploy custom Docker containers
  #
  ./gateway-external/preprovision.sh
  ./gateway-internal/preprovision.sh
  ./idsvr/preprovision.sh
fi

