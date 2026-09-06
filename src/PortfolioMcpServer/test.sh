#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

##############################################################################
# Run integration tests with mock access tokens against the secured MCP server
##############################################################################

#
# Run tests that call the MCP server with mock access tokens
#
cd security-tests
rm *.sln 2>/dev/null

#
# See Microsoft guides for XUnit test options
# - https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test?tabs=dotnet-test-with-vstest
#
. ./.env
dotnet test --logger "console;verbosity=detailed" --tl:off
