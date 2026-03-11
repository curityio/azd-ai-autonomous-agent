#
# A job container to initialize an Azure SQL database for the Curity Identity Server
#

FROM mcr.microsoft.com/mssql-tools

RUN mkdir /tmp/initscripts
COPY *.sql /tmp/initscripts/
COPY *.sh  /tmp/initscripts/

ENTRYPOINT ["/tmp/initscripts/entrypoint.sh"]
