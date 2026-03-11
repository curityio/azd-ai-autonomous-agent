#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

###########################################################################################
# Run the autonomous agent locally, which is an A2A server that interacts with an Azure LLM
###########################################################################################

. ../../tools/local/load-secrets.sh
export TOKEN_EXCHANGE_CLIENT_SECRET="$AGENT_TOKEN_EXCHANGE_SECRET"
cd ../../src/AutonomousAgent

. ./.env
rm *.sln 2>/dev/null
dotnet run
