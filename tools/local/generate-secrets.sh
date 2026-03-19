#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

###########################################
# Generate secrets for the local deployment
###########################################

function generatePassword() {
  openssl rand 32 | base64 | tr -d '=/_-'
}

#
# The local deployment persists secrets as a developer convenience, to avoid secret loss
# It also ensures that child terminals can inherit secrets generated in parent terminals
#
if [ ! -f ./load-secrets.sh ]; then
  
  echo "export SQL_ADMIN_PASSWORD='$(generatePassword)'" > load-secrets.sh
  echo "export ADMIN_PASSWORD='$(generatePassword)'" >> load-secrets.sh
  echo "export GATEWAY_TOKEN_EXCHANGE_SECRET='$(generatePassword)'" >>  load-secrets.sh
  echo "export AGENT_TOKEN_EXCHANGE_SECRET='$(generatePassword)'" >> load-secrets.sh
  chmod +x load-secrets.sh
fi

. ./load-secrets.sh
