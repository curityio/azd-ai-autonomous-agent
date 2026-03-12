#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

#########################################################################################
# The following files are dynamically created but must exist for azd validations to work
# So this repository checks a dummy file into source control to enable validation success
# Running the following commands prevent checkins when the dummy files get updated
#########################################################################################

git update-index --assume-unchanged ../idsvr/cluster.xml
git update-index --assume-unchanged ../gateway-internal/azure-internal-routes.yml
git update-index --assume-unchanged ../gateway-external/azure-external-routes.yml
