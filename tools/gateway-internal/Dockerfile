FROM kong/kong:3.9-ubuntu

#
# Install luarocks and enable its required git behaviors
#
USER root
RUN apt-get update && \
    apt-get install -y git unzip wget && \
    git config --global url."https://".insteadOf git:// && \
    git config --global advice.detachedHead false

#
# Install a plugin that does token-based auditing of agent requests with Curity access tokens
# - /usr/local/share/lua/5.1/kong/plugins/token-audit
#
COPY ./token-audit-plugin/  /tmp/token-audit-plugin/
RUN cd /tmp/token-audit-plugin && luarocks make token-audit-*.rockspec && rm -rf /tmp/token-audit-plugin/

USER kong
