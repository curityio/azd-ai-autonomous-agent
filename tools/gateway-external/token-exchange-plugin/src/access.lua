--
-- A Lua module to handle token exchange operations
--

local _M = {}
local cjson = require "cjson"
local http = require "resty.http"

--
-- Get values into an array that can be iterated multiple times
--
local function iterator_to_array(iterator)
    
    local i = 1;
    local array = {};

    for item in iterator do
      array[i] = item;
      i = i + 1
    end

    return array
end

--
-- A utility for finding an item in an array
--
local function array_has_value(arr, val)

    for _, item in ipairs(arr) do
        if val == item then
            return true
        end
    end

    return false
end

--
-- Return errors due to invalid tokens or technical problems
--
local function error_response(status, code, message)

    local method = ngx.req.get_method():upper()
    if method ~= 'HEAD' then
    
        ngx.status = status
        ngx.header['content-type'] = 'application/json'
        if status == 401 then
            ngx.header['WWW-Authenticate'] = 'Bearer error="' .. code .. '", ' .. 'error_description="' .. message .. '"'
        end
        
        local jsonData = '{"code":"' .. code .. '","message":"' .. message .. '"}'
        ngx.say(jsonData)
    end
    
    ngx.exit(status)
end

--
-- Return a generic message for all three of these error categories
--
local function unauthorized_error_response()
    error_response(ngx.HTTP_UNAUTHORIZED, 'invalid_token', 'Missing, invalid or expired access token')
end

local function server_error_response(config)
    error_response(ngx.HTTP_INTERNAL_SERVER_ERROR, 'server_error', 'Problem encountered processing the request')
end

--
-- Exchange the access token according to the plugin configuration
--
local function exchange_access_token(received_access_token, config)

    local httpc = http:new()
    local client_credential = config.client_id .. ':' .. config.client_secret
    local basic_auth_header = 'Basic ' .. ngx.encode_base64(client_credential)
    local new_access_token = nil

    local request_body = 'grant_type=urn:ietf:params:oauth:grant-type:token-exchange'
    request_body = request_body .. '&subject_token=' .. received_access_token
    request_body = request_body .. '&subject_token_type=urn:ietf:params:oauth:token-type:access_token'
    if config.target_scope then
        request_body = request_body .. '&scope=' .. config.target_scope
    end
    if config.target_audience then
        request_body = request_body .. '&audience=' .. config.target_audience
    end

    local response, error = httpc:request_uri(config.token_endpoint, {
        method = 'POST',
        body = request_body,
        headers = { 
            ['authorization'] = basic_auth_header,
            ['content-type'] = 'application/x-www-form-urlencoded',
            ['accept'] = 'application/json'
        }
    })

    if response.status == 200 then
        local data = cjson.decode(response.body)
        if not data or not data.access_token then
            ngx.log(ngx.WARN, 'No access token was received in a token exchange response')
            return { status = 500 }
        end

        new_access_token = data.access_token
    end
    
    if error then
        local connection_message = 'A technical problem occurred during token exchange'
        ngx.log(ngx.WARN, connection_message .. error)
        return { status = 500 }
    end

    if not response then
        return { status = 500 }
    end

    if response.status ~= 200 then
        return { status = response.status }
    end

    -- Get the time to cache from the cache-control header's max-age value
    local expiry = 0
    if response.headers then
        local cache_header = response.headers['cache-control']
        if cache_header then
            local _, _, expiry_match = string.find(cache_header, "max.-age=(%d+)")
            if expiry_match then
                expiry = tonumber(expiry_match)
            end
        end
    end

    return { status = response.status, jwt = new_access_token, expiry = expiry }
end

--
-- Get the exchanged access token from the cache, or perform an exchange operation if not found
--
local function process_access_token(access_token, config)

    local result = { status = 401 }

    -- See if there is a result in the cache
    local dict = ngx.shared['token-exchange']
    local existing_jwt = dict:get(access_token)
    if existing_jwt then

        -- Return cached results for the same token
        result = { status = 200, jwt = existing_jwt }
    
    else

        result = exchange_access_token(access_token, config)
        if result.status == 200 then

            local time_to_live = config.token_cache_seconds
            if result.expiry > 0 and result.expiry < config.token_cache_seconds then
                time_to_live = result.expiry
            end

            -- Cache the result so that token exchange is efficient under load
            -- The opaque access token is already a unique string similar to a GUID so use it as a cache key
            -- The cache is atomic and thread safe so is safe to use across concurrent requests
            -- The expiry value is a number of seconds from the current time
            -- https://github.com/openresty/lua-nginx-module#ngxshareddictset
            dict:set(access_token, result.jwt, time_to_live)
        end
    end

    return result
end

--
-- The public entry point to get an optimal token for the upstream API
--
function _M.run(config)

    if ngx.req.get_method():upper() == 'OPTIONS' then
        return
    end

    local auth_header = ngx.req.get_headers()['Authorization']
    if auth_header and string.len(auth_header) > 7 and string.lower(string.sub(auth_header, 1, 7)) == 'bearer ' then

        local access_token_untrimmed = string.sub(auth_header, 8)
        local access_token = string.gsub(access_token_untrimmed, "%s+", "")
        local result = process_access_token(access_token, config)
    
        if result.status == 500 then
            error_response(ngx.HTTP_INTERNAL_SERVER_ERROR, 'server_error', 'Problem encountered processing the HTTP request')
        end

        if result.status ~= 200 then
            ngx.log(ngx.WARN, 'Received a ' .. result.status .. ' token response')
            unauthorized_error_response()
        end

        ngx.req.set_header('Authorization', 'Bearer ' .. result.jwt)
    else

        ngx.log(ngx.WARN, 'No valid access token was found in the HTTP Authorization header')
        unauthorized_error_response()
    end
end

return _M
