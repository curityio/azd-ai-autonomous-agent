#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

##################################################################################
# Run Portfolio MCP Server locally, which authorizes access to protected resources
##################################################################################

. ./.env
rm *.sln 2>/dev/null
dotnet run
