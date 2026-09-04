--
-- A Lua module to handle token-based auditing in an internal gateway
--

local _M = {}
local cjson = require "cjson"
local jwt = require 'resty.jwt'

--
-- Demonstrates some basic JSON auditing of allowed requests with useful token claims
-- Gateway audit logs could be shipped to a log aggregation system
-- Log entries would enable security stakeholders to query AI agent business resource access at scale
--
local function audit_request(jwt_access_token)

    local data = jwt:load_jwt(jwt_access_token, nil)

    local payload = data.payload or {}
    local act = payload.act or {}

    if data.valid then

        local audit_data = { 
            log_type = 'audit',
            time = os.date("!%Y-%m-%dT%H:%M:%SZ", math.floor(ngx.now())),
            target_host = ngx.var.host,
            target_path = ngx.var.request_uri,
            target_method = ngx.req.get_method(),
            client_id = payload.client_id,
            audience = payload.aud,
            scope = payload.scope,
            delegation_id = payload.delegationId,
            customer_id = payload.customer_id,
            region = payload.region,
            agent_id = act.sub,
            agent_role = act.agent_role,
            agent_department = act.agent_department,
        };
        local audit_json = cjson.encode(audit_data)
        ngx.log(ngx.WARN, audit_json)
    end
end

--
-- Log Curity access tokens sent from backend AI agents through an internal gateway
--
function _M.run(config)

    if ngx.req.get_method():upper() == 'OPTIONS' then
        return
    end

    local auth_header = ngx.req.get_headers()['Authorization']
    if auth_header and string.len(auth_header) > 7 and string.lower(string.sub(auth_header, 1, 7)) == 'bearer ' then

        local access_token_untrimmed = string.sub(auth_header, 8)
        local access_token = string.gsub(access_token_untrimmed, "%s+", "")
        audit_request(access_token)
    end
end

return _M
