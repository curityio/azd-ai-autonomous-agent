#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

#######################################################
# Prepare infrastructure for the Curity Identity Server
#######################################################

echo 'Running preprovision logic for the Curity Identity Server ...'
cd ../../../tools/idsvr

# ------------------------------------------------------
# Create a custom Docker image for the database init job
# ------------------------------------------------------
if [ -z "${DBINIT_IMAGE_NAME:-}" ]; then

  echo 'Creating a custom Docker image for the database init job ...'
  cd dbinit
  echo 'Getting SQL scripts ...'

  # Use a utility Docker container to get the script
  docker pull curity.azurecr.io/curity/idsvr
  docker run --name curity -d -e PASSWORD=Password1 curity.azurecr.io/curity/idsvr
  docker cp curity:/opt/idsvr/etc/mssql-create_database.sql .
  docker rm --force curity
  if [ ! -f mssql-create_database.sql ]; then
    echo 'Failed to get the SQL schema creation script'
    exit 1
  fi

  # When running on Windows computers, fix up newlines before copying scripts to Docker containers
  chmod 644 ./mssql-create_database.sql
  if [[ "$(uname -s)" == MINGW64* ]]; then
    sed -i 's/\r$//' ./entrypoint.sh
    sed -i 's/\r$//' ./initdb.sh
  fi
  
  # Build and push the image
  az acr login --name "$CONTAINER_REGISTRY_NAME"
  TAG="$(date +%Y%m%d%H%M%S)"
  docker build --no-cache --platform linux/amd64 -t "dbinit:$TAG" .
  if [ $? -ne 0 ]; then
    exit 1
  fi 

  IMAGE="$CONTAINER_REGISTRY_NAME.azurecr.io/dbinit:$TAG"
  docker tag "dbinit:$TAG" "$IMAGE"
  docker push "$IMAGE"
  if [ $? -ne 0 ]; then
    exit 1
  fi 
  azd env set DBINIT_IMAGE_NAME "$IMAGE" >/dev/null
  cd ..
fi

# -----------------------------------------------------------
# Create a custom Docker image for the Curity Identity Server
# -----------------------------------------------------------
if [ -z "${IDSVR_IMAGE_NAME:-}" ]; then

  echo 'Creating a custom Docker image for the Curity Identity Server ...'
  cd docker
  rm *.xml 2>/dev/null
  cp ../config-base.xml .
  cp ../config-azure.xml .
  
  az acr login --name "$CONTAINER_REGISTRY_NAME"
  TAG="$(date +%Y%m%d%H%M%S)"
  docker build --no-cache --platform linux/amd64 -t "idsvr:$TAG" .
  if [ $? -ne 0 ]; then
    exit 1
  fi 

  IMAGE="$CONTAINER_REGISTRY_NAME.azurecr.io/idsvr:$TAG"
  docker tag "idsvr:$TAG" "$IMAGE"
  docker push "$IMAGE"
  if [ $? -ne 0 ]; then
    exit 1
  fi 
  azd env set IDSVR_IMAGE_NAME "$IMAGE" >/dev/null
  cd ..
fi

# --------------------------------------------------------------------------------
# Generate a cluster.xml file with a key that secures inter-workload communication
# --------------------------------------------------------------------------------
echo 'Generating cluster.xml with keystore using Docker...'

ADMIN_WORKLOAD_NAME="idsvr-admin-${AZURE_ENV_NAME}"
CLUSTER_XML=$(docker run --rm curity.azurecr.io/curity/idsvr:latest \
    genclust -c "$ADMIN_WORKLOAD_NAME" 2>/dev/null)

if [ $? -ne 0 ] || [ -z "$CLUSTER_XML" ]; then
    echo 'Failed to generate the cluster.xml file'
    exit 1
fi

echo "$CLUSTER_XML" > cluster.xml
