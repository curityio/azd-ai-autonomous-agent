#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

#################################################################################################################
# Runs local backend components in development mode, ready to receive a natural language command from A2A clients
#################################################################################################################

#
# If required, run a tool to download the license file for the Curity Identity Server
#
../idsvr/download-license.sh
if [ $? -ne 0 ]; then
  exit 1
fi

#
# Generate secrets for components that need them
#
. ./generate-secrets.sh

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
cd -

#
# Supply other environment variables for the local deployment
#
export IDSVR_ADMIN_URL='http://localhost:6749'
export IDSVR_RUNTIME_URL='http://localhost:8443'
export LICENSE_KEY

#
# Pull up to date Docker images
#
docker pull kong/kong:3.9-ubuntu
docker pull curity.azurecr.io/curity/idsvr:latest

#
# Build the Docker image for the Portfolio MCP server
#
cd ../../src/PortfolioMcpServer
docker build --no-cache -t portfolio-mcp-server:1.0.0 .
if [ $? -ne 0 ]; then
  exit 1
fi
cd -

#
# Build the external API gateway Docker image, with a token exchange plugin
#
cd ../gateway-external
docker build --no-cache -t gateway-external:1.0.0 .
if [ $? -ne 0 ]; then
  exit 1
fi
cd -

#
# Build the internal API gateway Docker image, with a token auditing plugin
#
cd ../gateway-internal
docker build --no-cache -t gateway-internal:1.0.0 .
if [ $? -ne 0 ]; then
  exit 1
fi
cd -

#
# Build a custom Docker image for the Curity Identity Server, with local configuration
#
cd ../idsvr
docker build --no-cache -f Dockerfile.local -t idsvr:1.0.0 .
if [ $? -ne 0 ]; then
  exit 1
fi
cd -

#
# Create a local deployed environment with components to support agent development
#
docker compose up --force-recreate
if [ $? -ne 0 ]; then
  read -n 1  
  exit 1
fi
