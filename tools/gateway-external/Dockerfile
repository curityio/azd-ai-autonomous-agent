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
# Install a plugin that does token exchange to deliver least privilege access tokens to the autonomous agent
# - /usr/local/share/lua/5.1/kong/plugins/token-exchange
#
COPY ./token-exchange-plugin/  /tmp/token-exchange-plugin/
RUN cd /tmp/token-exchange-plugin && luarocks make token-exchange-*.rockspec && rm -rf /tmp/token-exchange-plugin/

USER kong
