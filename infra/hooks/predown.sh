#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

###################################################################################
# The predown hook is called for both the 'base' and 'identity' provisioning layers
###################################################################################

set -euo pipefail
echo 'Running predown logic ...'

#
# Run the Entra ID cleanup script if the environment variable exists
#
eval $(azd env get-values 2>/dev/null) || true
if [ ! -z "${ENTRA_CLIENT_ID:-}" ]; then
  ./idsvr/predown.sh
fi
