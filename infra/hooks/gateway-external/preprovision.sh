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
# In GitHub workflows, this value is provided as an environment variable.
# In local deployments to Azure, read the value from the Azure key vault.
#
if [ -z "${GATEWAY_TOKEN_EXCHANGE_SECRET:-}" ]; then
  export GATEWAY_TOKEN_EXCHANGE_SECRET=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "GATEWAY-TOKEN-EXCHANGE-SECRET" --query "value" -o tsv)
  if [ $? -ne 0 ] || [ "$GATEWAY_TOKEN_EXCHANGE_SECRET" == '' ]; then
    echo 'Unable to read the API gateway token exchange secret'
    exit 1
  fi
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

#
# Build and push Docker containers
#
if [ -z "${GATEWAY_EXTERNAL_IMAGE_NAME:-}" ]; then
  
  az acr login --name "$CONTAINER_REGISTRY_NAME"

  TAG="$(date +%Y%m%d%H%M%S)"
  docker build --no-cache --platform linux/amd64 -t "gateway-external:$TAG" .
  if [ $? -ne 0 ]; then
    exit 1
  fi 

  IMAGE="$CONTAINER_REGISTRY_NAME.azurecr.io/gateway-external:$TAG"
  docker tag "gateway-external:$TAG" "$IMAGE"
  docker push "$IMAGE"
  if [ $? -ne 0 ]; then
    exit 1
  fi 

  azd env set GATEWAY_EXTERNAL_IMAGE_NAME "$IMAGE" >/dev/null
fi