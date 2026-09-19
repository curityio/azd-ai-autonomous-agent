#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

##############################################################################
# Run integration tests with mock access tokens against the secured MCP server
# - https://xunit.net/docs/config-xunit-runner-json
##############################################################################

#
# Run tests that call the MCP server with mock access tokens
#
rm *.sln 2>/dev/null
. ./.env
dotnet run -trait "Category=Security" -reporter custom
