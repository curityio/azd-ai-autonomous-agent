#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

#####################################################################
# Configure routes using the newly created container apps domain name
# Also push Docker containers using the newly created Docker registry
#####################################################################

export AZURE_ENV_NAME=$(azd env get-value AZURE_ENV_NAME)
echo 'Running preprovision logic for the internal gateway ...'

#
# Update gateway hostname based routes
#
cd ../../../tools/gateway-internal
envsubst < azure-routes-template.yml > azure-internal-routes.yml
if [ $? -ne 0 ]; then
  echo 'envsubst failed for internal gateway'
  exit 1
fi

#
# Build and push Docker containers
#
if [ -z "${GATEWAY_INTERNAL_IMAGE_NAME:-}" ]; then

  az acr login --name "$CONTAINER_REGISTRY_NAME"
  CONTAINER_REGISTRY_NAME=$(azd env get-value CONTAINER_REGISTRY_NAME)

  TAG="$(date +%Y%m%d%H%M%S)"
  docker build --no-cache --platform linux/amd64 -t "gateway-internal:$TAG" .
  if [ $? -ne 0 ]; then
    exit 1
  fi 

  IMAGE="$CONTAINER_REGISTRY_NAME.azurecr.io/gateway-internal:$TAG"
  docker tag "gateway-internal:$TAG" "$IMAGE"
  docker push "$IMAGE"
  if [ $? -ne 0 ]; then
    exit 1
  fi 

  azd env set GATEWAY_INTERNAL_IMAGE_NAME "$IMAGE" >/dev/null
fi
