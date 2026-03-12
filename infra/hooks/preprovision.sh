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
# Work out the provisioning stage from environment variables
#
if [ -z "${EXTERNAL_DOMAIN_NAME:-}" ]; then
  PROVISIONING_STAGE='BASE'
else
  PROVISIONING_STAGE='IDENTITY'
fi

#
# During identity provisioning from a local computer, generate some secrets
# GitHub workflows use configured secrets instead
#
if [ "$PROVISIONING_STAGE" == 'IDENTITY' ]; then

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
fi

#
# Do extra work at the start of identity provisioning
#
if [ "$PROVISIONING_STAGE" == 'IDENTITY' ]; then
  ./gateway-external/preprovision.sh
  ./gateway-internal/preprovision.sh
  ./idsvr/preprovision.sh
fi
