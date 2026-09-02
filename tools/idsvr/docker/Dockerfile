FROM curity.azurecr.io/curity/idsvr:latest

COPY --chown=10001:10000 *.xml /opt/idsvr/etc/init/
COPY --chown=10001:10000 post-login-proc.js                 /opt/idsvr/etc/init/transformation-procedures/
COPY --chown=10001:10000 token-exchange-proc.js             /opt/idsvr/etc/init/token-procedures/oauth-token-oauth-token-exchange/
COPY --chown=10001:10000 authorization-code-proc.js         /opt/idsvr/etc/init/token-procedures/oauth-token-authorization-code/
COPY --chown=10001:10000 script-claims-provider-proc.js     /opt/idsvr/etc/init/claims-provider-procedures/
