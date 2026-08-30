
package = "token-exchange"
version = "1.0.0-1"
source = {
  url = "git://github.com/curityio/token-exchange",
  tag = "v1.0.0"
}
description = {
  summary = "A Lua plugin to perform token exchange operations",
  homepage = "https://curity.io/resources/aiagents/",
  license = "Apache 2.0",
  detailed = [[
        This token exchange plugin runs in an external gateway to process incoming access tokens.
        The gateway sends the incoming token in a token exchange request, to get a downscoped JWT access token.
        The gateway then caches the result for subsequent requests with the same incoming access token.
  ]]
}
dependencies = {
  "lua >= 5.1",
  "lua-resty-http >= 0.16.1-0"
}
build = {
  type = "builtin",
  modules = {
    ["kong.plugins.token-exchange.access"]  = "src/access.lua",
    ["kong.plugins.token-exchange.handler"] = "src/handler.lua",
    ["kong.plugins.token-exchange.schema"]  = "src/schema.lua"
  }
}
