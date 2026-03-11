#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

#
# Create the database schema if required
#
/tmp/initscripts/initdb.sh

#
# If required, keep the container running for debug purposes
#
/bin/sleep infinity
