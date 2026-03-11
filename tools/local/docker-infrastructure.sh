#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

################################################################
# Run local Docker infrastructure to provide identity components
################################################################

#
# First ensure that we have some secrets
#
if [ ! -f ./load-secrets.sh ]; then
  echo 'Generate some secrets before deploying local infrastructure'
  exit 1
fi
. ./load-secrets.sh

#
# If required, run a tool to download the license file for the Curity Identity Server
#
cd ../idsvr
./download-license.sh
if [ $? -ne 0 ]; then
  exit 1
fi

if [ ! -f license.json ]; then
  echo 'Unable to find a license file for the Curity Identity Server'
  exit 1
fi

LICENSE_KEY="$(cat license.json | jq -r .License)"
if [ "$LICENSE_KEY" == '' ]; then
  echo 'Unable to find a license key for the Curity Identity Server'
  exit 1
fi

#
# Supply other environment variables for the local deployment
#
export IDSVR_ADMIN_URL='http://localhost:6749'
export IDSVR_RUNTIME_URL='http://localhost:8443'
export LICENSE_KEY

#
# Pull up to date base Docker images
#
docker pull kong/kong:3.9-ubuntu
docker pull curity.azurecr.io/curity/idsvr:latest

#
# Build the external API gateway Docker image, with an introspection plugin
#
cd ../gateway-external
envsubst < local-routes-template.yml > local-routes.yml
if [ $? -ne 0 ]; then
  exit 1
fi

docker build --no-cache -t gateway-external:1.0.0 .
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Build the internal API gateway Docker image, with a token auditing plugin
#
cd ../gateway-internal
docker build --no-cache -t gateway-internal:1.0.0 .
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Build a custom Docker image for the Curity Identity Server, with configuration and scripts
#
cd ../idsvr/docker
rm *.xml 2>/dev/null
cp ../config-base.xml .
cp ../config-local.xml .
docker build --no-cache -t idsvr:1.0.0 .
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Ensure no leftover configuration database in the Curity Identity Server Docker image
#
cd ..
rm -rf cdb 2>/dev/null
mkdir cdb
chmod 777 cdb

#
# Deploy the Curity Identity Server, the external gateway and the internal gateway
#
cd ../local
docker compose up
if [ $? -ne 0 ]; then
  exit 1
fi
