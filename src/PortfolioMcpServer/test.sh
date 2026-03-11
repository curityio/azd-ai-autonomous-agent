#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

##########################################################
# Demonstrates a technique for testing MCP server security
##########################################################

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
# Point the Portfolio MCP server to a test JWKS URI
#
cp .env.default .env
echo "export JWKS_URI='http://localhost:3002/jwks'" >> .env

#
# Run the autonomous agent and use a JWKS URI for testing
#
if [ "$PLATFORM" == 'MACOS' ]; then

  open -a Terminal ./run.sh

elif [ "$PLATFORM" == 'WINDOWS' ]; then

  GIT_BASH='C:\Program Files\Git\git-bash.exe'
  "$GIT_BASH" -c ./run.sh &

elif [ "$PLATFORM" == 'LINUX' ]; then

  gnome-terminal -- ./run.sh
fi

#
# Wait for the service to become available
#
while [ "$(curl -k -s -X GET -o /dev/null -w '%{http_code}' "http://localhost:3001/.well-known/oauth-protected-resource")" != '200' ]; do
  sleep 1
done

#
# Run tests that call the MCP server with mock access tokens
# - https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test?tabs=dotnet-test-with-vstest
#
cd security-tests
rm *.sln 2>/dev/null
. ./.env
dotnet test --logger "console;verbosity=normal" --tl:off
