FROM kong/kong:3.9-ubuntu

USER root
COPY ./token-exchange-plugin/*.lua /usr/local/share/lua/5.1/kong/plugins/token-exchange/
USER kong
