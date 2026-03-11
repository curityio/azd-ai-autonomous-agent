
package = "token-audit"
version = "1.0.0-1"
source = {
  url = "git://github.com/curityio/ai-autonomous-agent",
  tag = "v1.0.0"
}
description = {
  summary = "A Lua plugin to perform token-based auditing during MCP / A2A requests from AI agents",
  homepage = "https://curity.io/resources/aiagents/",
  license = "Apache 2.0",
  detailed = [[
        An example token audit plugin that uses attributes from Curity access tokens.
        The plugin logs token business attributes as JSON.
        Audit data can be aggregated to provide people visibility of agent resource access at scale.
  ]]
}
dependencies = {
  "lua >= 5.1",
  "lua-resty-jwt >= 0.2.3-0"
}
build = {
  type = "builtin",
  modules = {
    ["kong.plugins.token-audit.access"]  = "src/access.lua",
    ["kong.plugins.token-audit.handler"] = "src/handler.lua",
    ["kong.plugins.token-audit.schema"]  = "src/schema.lua"
  }
}
