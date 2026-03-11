--
-- The Kong entry point handler
--

local access = require "kong.plugins.token-audit.access"

-- See https://github.com/Kong/kong/discussions/7193 for more about the PRIORITY field
local TokenAudit = {
    PRIORITY = 1000,
    VERSION = "2.0.1",
}

function TokenAudit:access(conf)
    access.run(conf)
end

return TokenAudit
