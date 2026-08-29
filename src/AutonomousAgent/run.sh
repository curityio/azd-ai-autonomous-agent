#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

###########################################################################################
# Run the autonomous agent locally, which is an A2A server that interacts with an Azure LLM
###########################################################################################

cd ../../src/AutonomousAgent
. ./.env
rm *.sln 2>/dev/null
dotnet run
