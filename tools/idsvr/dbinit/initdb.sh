#!/bin/bash

cd "$(dirname "${BASH_SOURCE[0]}")"

################################
# Database schema initialization
################################

#
# See if tables already exist in the Curity Identity Server database
#
echo 'initdb is checking table count ...'
TABLE_COUNT=$(/opt/mssql-tools/bin/sqlcmd -S "$SQL_SERVER_NAME.database.windows.net" -U "$SQL_ADMIN_USERNAME" -P "$SQL_ADMIN_PASSWORD" -d "$SQL_DATABASE_NAME" -h -1 -t 1 -C -Q 'SET NOCOUNT ON; SELECT COUNT(1) FROM sys.tables')
if [ $? -ne 0 ]; then
  echo "$TABLE_COUNT"
  exit 1
fi

#
# Create the schema if required and use the -I option to prevent quoted identifier errors
#
if [ $TABLE_COUNT -eq 0 ]; then

  echo 'initdb is creating the schema ...'
  /opt/mssql-tools/bin/sqlcmd -S "$SQL_SERVER_NAME.database.windows.net" -U "$SQL_ADMIN_USERNAME" -P "$SQL_ADMIN_PASSWORD" -d "$SQL_DATABASE_NAME" -I -i /tmp/initscripts/mssql-create_database.sql
  if [ $? -ne 0 ]; then
    exit 1
  fi
fi
echo 'initdb completed successfully'
