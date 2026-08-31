--
-- The Kong entry point handler
--

local access = require "kong.plugins.token-exchange.access"

-- See https://github.com/Kong/kong/discussions/7193 for more about the PRIORITY field
local TokenExchange = {
    PRIORITY = 1001,
    VERSION = "1.0.0",
}

function TokenExchange:access(conf)
    access.run(conf)
end

return TokenExchange
