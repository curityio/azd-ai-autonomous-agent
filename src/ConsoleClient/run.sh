#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

##################################################################################
# Run the minimal client, which makes a secure A2A request against a local backend
##################################################################################

. ./.env
rm *.sln 2>/dev/null

if [ "$AI_EXTERNAL_URL" == '' ]; then
  export AUTONOMOUS_AGENT_URL='http://localhost:3000'
else
  export AUTONOMOUS_AGENT_URL="$AI_EXTERNAL_URL/autonomous-agent"
fi

dotnet run
