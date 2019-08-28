using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MisakaDocs.JsonModels
{
    [JsonObject]
    public class TokenResponse : JsonModelBase
    {
        [JsonProperty("access_token")]
        public string AccessToken;
        [JsonProperty("token_type")]
        public string TokenType;
        [JsonProperty("expires_in")]
        public int ExpiresIn;
        [JsonProperty("refresh_token")]
        public string RefreshToken;
    }
}
