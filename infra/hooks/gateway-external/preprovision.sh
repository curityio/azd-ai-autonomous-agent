#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

#####################################################################
# Configure routes using the newly created container apps domain name
# Also push Docker containers using the newly created Docker registry
#####################################################################

echo 'Running preprovision logic for the external gateway ...'
export AZURE_ENV_NAME=$(azd env get-value AZURE_ENV_NAME)
export EXTERNAL_DOMAIN_NAME=$(azd env get-value EXTERNAL_DOMAIN_NAME)

if [ -z "${GATEWAY_TOKEN_EXCHANGE_SECRET:-}" ]; then
  export GATEWAY_TOKEN_EXCHANGE_SECRET=$(azd env get-value GATEWAY_TOKEN_EXCHANGE_SECRET)
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
  
  CONTAINER_REGISTRY_NAME=$(azd env get-value CONTAINER_REGISTRY_NAME)
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