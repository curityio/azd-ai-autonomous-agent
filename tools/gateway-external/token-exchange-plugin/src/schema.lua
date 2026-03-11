return {
    name = "token-exchange",
    fields = {{
        config = {
            type = "record",
            fields = {
                { token_endpoint = { type = "string", required = false } },
                { client_id = { type = "string", required = true } },
                { client_secret = { type = "string", required = true } },
                { target_scope = { type = "string", required = false } },
                { target_audience = { type = "string", required = false } },
                { token_cache_seconds = { type = "number", required = false, default = 300 } }
            }
        }}
    }
}
