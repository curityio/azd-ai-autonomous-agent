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
eval $(azd env get-values 2>/dev/null) || true
if [ -z "${AZURE_ENV_NAME:-}" ]; then
  echo "Preprovision error: AZURE_ENV_NAME not set. Please run 'azd env new' first."
  exit 1
fi

#
# A utility to generate strong passwords if they do not yet exist
#
function generatePassword() {
  openssl rand 32 | base64 | tr '/+' '_-' | tr -d '='
}

#
# Scripted automation for the identity layer
#
if [ ! -z "${EXTERNAL_DOMAIN_NAME:-}" ]; then

  # For deployments triggered locally, generate some strong secrets
  if [ -z "${SQL_ADMIN_PASSWORD:-}" ]; then
    azd env set SQL_ADMIN_PASSWORD "$(generatePassword)"
  fi
  if [ -z "${ADMIN_PASSWORD:-}" ]; then
    azd env set ADMIN_PASSWORD "$(generatePassword)"
  fi
  if [ -z "${GATEWAY_TOKEN_EXCHANGE_SECRET:-}" ]; then
    azd env set GATEWAY_TOKEN_EXCHANGE_SECRET "$(generatePassword)"
  fi
  if [ -z "${AGENT_TOKEN_EXCHANGE_SECRET:-}" ]; then
    azd env set AGENT_TOKEN_EXCHANGE_SECRET "$(generatePassword)"
  fi
  if [ -z "${LICENSE_KEY:-}" ]; then
    LICENSE_KEY="$(cat ../../tools/idsvr/license.json | jq -r .License)"
    if [ "$LICENSE_KEY" == '' ]; then
      echo 'Unable to find a license key for the Curity Identity Server'
      exit 1
    fi
    azd env set LICENSE_KEY "$LICENSE_KEY"
  fi

  # Preprovisioning for the external gateway
  if [ -z "${GATEWAY_EXTERNAL_IMAGE_NAME:-}" ]; then
    ./gateway-external/preprovision.sh
  fi

  # Preprovisioning for the internal gateway
  if [ -z "${GATEWAY_INTERNAL_IMAGE_NAME:-}" ]; then
    ./gateway-internal/preprovision.sh
  fi

  # Preprovisioning for the Curity Identity Server
  ./idsvr/preprovision.sh
fi
