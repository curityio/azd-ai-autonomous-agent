#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

###################################################################################
# The predown hook is called for both the 'base' and 'identity' provisioning layers
###################################################################################

set -euo pipefail
echo 'Running preprovision logic ...'

#
# Sanity checks
#
if [ -z "${AZURE_ENV_NAME:-}" ]; then
  echo "Preprovision error: AZURE_ENV_NAME not set - please create an environment before running azd up."
  exit 1
fi

#
# During the initial base provisioning there is no EXTERNAL_DOMAIN_NAME yet
#
if [ -z "${EXTERNAL_DOMAIN_NAME:-}" ]; then
  PROVISIONING_STAGE='BASE'
else
  PROVISIONING_STAGE='IDENTITY'
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

  echo "Creating secret: $KEY ..."
  az keyvault secret set --vault-name "$KEY_VAULT_NAME" --name "$KEY" --value "$VALUE" 1>/dev/null

  ENV_KEY="${KEY//-/_}"
  ENV_VALUE="akvs://${AZURE_SUBSCRIPTION_ID}/${KEY_VAULT_NAME}/${KEY}"
  echo "$ENV_KEY=\"$ENV_VALUE\"" >> ../../.azure/${AZURE_ENV_NAME}/.env 
}

#
# Set a password secret if required
#
function setPasswordSecret() {

  local KEY="$1"

  EXISTS=$(az keyvault secret list --vault-name "$KEY_VAULT_NAME" --query "contains([].id, 'https://$KEY_VAULT_NAME.vault.azure.net/secrets/$KEY')")
  if [ $EXISTS == false ]; then
    setSecret "$KEY" "$(generatePassword)"
  fi
}

#
# Set a license secret if required
#
function setLicenseSecret() {

  local KEY='LICENSE-KEY'
  EXISTS=$(az keyvault secret list --vault-name "$KEY_VAULT_NAME" --query "contains([].id, 'https://$KEY_VAULT_NAME.vault.azure.net/secrets/$KEY')")
  if [ $EXISTS == false ]; then

    if [ ! -f ../../tools/idsvr/license.json ]; then
      echo 'Unable to find a license file for the Curity Identity Server'
      exit 1
    fi

    setSecret "$KEY" "$(cat ../../tools/idsvr/license.json | jq -r .License)"
  fi
}

#
# Implement identity provisioning logic
#
if [ "$PROVISIONING_STAGE" == 'IDENTITY' ]; then

  #
  # In GitHub workflows, secrets are supplied as secure environment variables
  # In local Azure deployments, create secrets on the first deployment
  #
  if [ -z "${GITHUB_ACTION:-}" ]; then

    setPasswordSecret 'SQL-ADMIN-PASSWORD'
    setPasswordSecret 'ADMIN-PASSWORD'
    setPasswordSecret 'GATEWAY-TOKEN-EXCHANGE-SECRET'
    setPasswordSecret 'AGENT-TOKEN-EXCHANGE-SECRET'
    setPasswordSecret 'SMTP-SECRET'
    setLicenseSecret
  fi

  #
  # Run other logic, to configure each component and deploy custom Docker containers
  #
  ./gateway-external/preprovision.sh
  ./gateway-internal/preprovision.sh
  ./idsvr/preprovision.sh
fi
