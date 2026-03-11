#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

##################################################################################
# Run the minimal client, which makes a secure A2A request against a local backend
##################################################################################

. ./.env
rm *.sln 2>/dev/null

if [ "$A2A_EXTERNAL_URL" == '' ]; then
  export AUTONOMOUS_AGENT_URL='http://localhost/a2a'
else
  export AUTONOMOUS_AGENT_URL="$A2A_EXTERNAL_URL/a2a"
fi

dotnet run
