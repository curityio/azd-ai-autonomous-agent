FROM kong/kong:3.9-ubuntu

USER root
COPY ./token-audit-plugin/*.lua    /usr/local/share/lua/5.1/kong/plugins/token-audit/
USER kong
