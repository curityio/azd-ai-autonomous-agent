FROM curity.azurecr.io/curity/idsvr:latest

COPY --chown=10001:10000 *.xml /opt/idsvr/etc/init/
COPY --chown=10001:10000 post-login-proc.js     /opt/idsvr/etc/init/transformation-procedures/post-login-proc.js
COPY --chown=10001:10000 token-exchange-proc.js /opt/idsvr/etc/init/token-procedures/oauth-token-oauth-token-exchange/token-exchange-proc.js
