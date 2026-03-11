#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

#################################################################################################################
# Runs local backend components in development mode, ready to receive a natural language command from A2A clients
#################################################################################################################

#
# Get the platform
#
case "$(uname -s)" in

  Darwin)
    PLATFORM="MACOS"
 	;;

  MINGW64*)
    PLATFORM="WINDOWS"
	;;

  Linux)
    PLATFORM="LINUX"
	;;
esac

#
# First create secrets for components that need them
# Also supply a path to the license file for the Curity Identity Server
#
. ./generate-secrets.sh

#
# If required, run a tool to download the license file for the Curity Identity Server
#
../idsvr/download-license.sh
if [ $? -ne 0 ]; then
  exit 1
fi

#
# When running locally, the Portfolio MCP Server gets token signing public keys from the local authorization server
#
cd ../..
cd ./src/PortfolioMcpServer
cp .env.default .env
echo "export JWKS_URI='http://localhost:8443/oauth/v2/oauth-anonymous/jwks'" >> ./.env
cd ../..

#
# Run supporting Docker components, with the Autonomous Agent and Portfolio MCP Server on the local computer
#
if [ "$PLATFORM" == 'MACOS' ]; then

  open -a Terminal ./tools/local/docker-infrastructure.sh
  open -a Terminal ./src/PortfolioMcpServer/run.sh
  open -a Terminal ./src/AutonomousAgent/run.sh

elif [ "$PLATFORM" == 'WINDOWS' ]; then

  GIT_BASH='C:\Program Files\Git\git-bash.exe'
  "$GIT_BASH" -c ./tools/local/docker-infrastructure.sh &
  "$GIT_BASH" -c ./src/PortfolioMcpServer/run.sh &
  "$GIT_BASH" -c ./src/AutonomousAgent/run.sh &

elif [ "$PLATFORM" == 'LINUX' ]; then

  gnome-terminal -- ./tools/local/docker-infrastructure.sh
  gnome-terminal -- ./src/PortfolioMcpServer/run.sh
  gnome-terminal -- ./src/AutonomousAgent/run.sh
fi
