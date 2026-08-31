FROM kong/kong:3.9-ubuntu

USER root
#RUN apt-get update \
#    && apt-get install -y --no-install-recommends git

COPY ./token-exchange-plugin/*.lua /usr/local/share/lua/5.1/kong/plugins/token-exchange/
COPY ./token-audit-plugin/*.lua    /usr/local/share/lua/5.1/kong/plugins/token-audit/

USER kong
