#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

# ------------------------------------------------------------------------------
# Pre-down hook to clean up Entra ID app registration created in preprovision.sh
# ------------------------------------------------------------------------------

#
# By default, the GitHub workflow's managed identity does not have permissions to edit Entra ID.
# To activate the below code for a GitHub workflow, you would need to run the below script first.
# - ./tools/utils/grant-workflow-entra-permissions.sh
#
if [ ! -z "${GITHUB_ACTION:-}" ]; then
  exit 0
fi

# Find the client ID from the deterministic name
echo 'Running predown logic for the Curity Identity Server ...'
ENTRA_APP_DISPLAY_NAME="curity-idsvr-${AZURE_ENV_NAME}"
ENTRA_CLIENT_ID="$(az ad app list --display-name "$ENTRA_APP_DISPLAY_NAME" --query "[0].appId" -o tsv 2>/dev/null || true)"
if [ -z "$ENTRA_CLIENT_ID" ]; then
  echo 'No ENTRA_CLIENT_ID was found'
  exit 0
fi


# Deleting the app will also remove the service identity and associated credentials
echo "Deleting Entra app registration: $ENTRA_CLIENT_ID"
az ad app delete --id "$ENTRA_CLIENT_ID" 2>/dev/null || {
  echo 'Failed to delete Entra ID app registration'
  exit 0
}
echo "✓ Entra ID app registration deleted"
