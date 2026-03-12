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
#if [ -z "${DBINIT_IMAGE_NAME:-}" ]; then

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
#fi

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

# ---------------------------
# Generate a cluster.xml file
# ---------------------------
echo 'Generating cluster.xml with keystore using Docker...'

ADMIN_WORKLOAD_NAME="idsvr-admin-${AZURE_ENV_NAME}"
CLUSTER_XML=$(docker run --rm curity.azurecr.io/curity/idsvr:latest \
    genclust -c "$ADMIN_WORKLOAD_NAME" 2>/dev/null)

if [ $? -ne 0 ] || [ -z "$CLUSTER_XML" ]; then
    echo 'Failed to generate the cluster.xml file'
    exit 1
fi

echo "$CLUSTER_XML" > cluster.xml

# --------------------------------------------------------------------------
# Create an Entra ID App Registration for OpenID Connect user authentication
# --------------------------------------------------------------------------

echo "Creating Entra ID app registration ..."

# Get the tenant ID for the OIDC metadata URL configured in the Curity Identity Server
TENANT_ID="${AZURE_TENANT_ID:-}"
if [ -z "$TENANT_ID" ]; then
  TENANT_ID="$(az account show --query tenantId -o tsv 2>/dev/null || true)"
fi
if [ -z "$TENANT_ID" ]; then
  echo 'Could not find the Entra ID tenant ID'
  exit 1
fi

# Lookup or create the app
ENTRA_APP_DISPLAY_NAME="curity-idsvr-${AZURE_ENV_NAME}"
IDSVR_RUNTIME_URL="https://idsvr-runtime-${AZURE_ENV_NAME}.${EXTERNAL_DOMAIN_NAME}"
ENTRA_CLIENT_ID="$(az ad app list --display-name "$ENTRA_APP_DISPLAY_NAME" --query "[0].appId" -o tsv 2>/dev/null || true)"
if [ -z "$ENTRA_CLIENT_ID" ]; then
  echo "Creating Entra app registration: $ENTRA_APP_DISPLAY_NAME"
  ENTRA_CLIENT_ID="$(az ad app create \
    --display-name "$ENTRA_APP_DISPLAY_NAME" \
    --sign-in-audience AzureADMyOrg \
    --web-redirect-uris "${IDSVR_RUNTIME_URL}/authn/authentication/entra/callback" \
    --query appId -o tsv)"
else
  echo "Reusing existing Entra app registration: $ENTRA_APP_DISPLAY_NAME ($ENTRA_CLIENT_ID)"
fi

# Ensure service principal exists, which can take a few seconds to propagate
if ! az ad sp show --id "$ENTRA_CLIENT_ID" -o none 2>/dev/null; then
  echo "Creating service principal for app..."
  az ad sp create --id "$ENTRA_CLIENT_ID" -o none 2>/dev/null || true
  for i in 1 2 3 4 5; do
    if az ad sp show --id "$ENTRA_CLIENT_ID" -o none 2>/dev/null; then
      break
    fi
    sleep 2
  done
fi

# Create a client secret, if not already present in the azd environment
ENTRA_CLIENT_SECRET="${ENTRA_CLIENT_SECRET:-}"
if [ -z "$ENTRA_CLIENT_SECRET" ]; then
  echo "Creating Entra client secret (stored in azd env as ENTRA_CLIENT_SECRET)..."
  ENTRA_CLIENT_SECRET="$(az ad app credential reset \
    --id "$ENTRA_CLIENT_ID" \
    --append \
    --display-name "azd-${AZURE_ENV_NAME}" \
    --years 1 \
    --query password -o tsv)"
else
  echo "ENTRA_CLIENT_SECRET already set in azd env; leaving it unchanged."
fi

ENTRA_OIDC_METADATA_URL="https://login.microsoftonline.com/${TENANT_ID}/v2.0/.well-known/openid-configuration"

# Persist values to azd env so that the deployment can use them
azd env set ENTRA_TENANT_ID "$TENANT_ID" >/dev/null
azd env set ENTRA_APP_DISPLAY_NAME "$ENTRA_APP_DISPLAY_NAME" >/dev/null
azd env set ENTRA_CLIENT_ID "$ENTRA_CLIENT_ID" >/dev/null
azd env set ENTRA_OIDC_METADATA_URL "$ENTRA_OIDC_METADATA_URL" >/dev/null
azd env set ENTRA_CLIENT_SECRET "$ENTRA_CLIENT_SECRET" >/dev/null

# Indicate success
echo "✓ Entra ID app registration is ready"
echo "  ENTRA_CLIENT_ID: $ENTRA_CLIENT_ID"
echo "  ENTRA_OIDC_METADATA_URL: $ENTRA_OIDC_METADATA_URL"
