#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

# ------------------------------------------------------------------------------
# Pre-down hook to clean up Entra ID app registration created in preprovision.sh
# ------------------------------------------------------------------------------

echo 'Running predown logic for the Curity Identity Server ...'

ENTRA_CLIENT_ID="${ENTRA_CLIENT_ID:-}"
ENTRA_APP_DISPLAY_NAME="${ENTRA_APP_DISPLAY_NAME:-}"

# Resolve the client id if not present (fallback to deterministic name)
if [ -z "$ENTRA_CLIENT_ID" ]; then
  if [ -z "$ENTRA_APP_DISPLAY_NAME" ] && [ -n "${AZURE_ENV_NAME:-}" ]; then
    ENTRA_APP_DISPLAY_NAME="curity-idsvr-${AZURE_ENV_NAME}"
  fi

  if [ -n "$ENTRA_APP_DISPLAY_NAME" ]; then
    ENTRA_CLIENT_ID="$(az ad app list --display-name "$ENTRA_APP_DISPLAY_NAME" --query "[0].appId" -o tsv 2>/dev/null || true)"
  fi
fi

if [ -z "$ENTRA_CLIENT_ID" ]; then
  echo 'No ENTRA_CLIENT_ID was found'
  exit 0
fi

# Deleting the app will also remove the service identity and associated credentials
echo "Deleting Entra app registration: $ENTRA_CLIENT_ID"
az ad app delete --id "$ENTRA_CLIENT_ID" 2>/dev/null || {
  echo 'Failed to delete Entra ID app'
  exit 0
}

# Clean up azd env values so subsequent runs don't reference stale IDs
azd env unset ENTRA_CLIENT_ID >/dev/null 2>&1 || true
azd env unset ENTRA_CLIENT_SECRET >/dev/null 2>&1 || true
azd env unset ENTRA_OIDC_METADATA_URL >/dev/null 2>&1 || true
echo "✓ Entra ID app registration deleted"
