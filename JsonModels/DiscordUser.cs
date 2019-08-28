using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MisakaDocs.JsonModels
{
    [JsonObject]
    public class DiscordUser : SiteData
    {
        [JsonProperty("id")]
        public string Id; 
        [JsonProperty("username")]
        public string Username;
        [JsonProperty("discriminator")]
        public string Discriminator;
        [JsonProperty("avatar")]
        public string Avatar;
        [JsonProperty("bot")]
        public bool Bot;
        [JsonProperty("mfa_enabled")]
        public bool MfaEnabled;
        [JsonProperty("verified")]
        public bool Verified;
        [JsonProperty("email")]
        public string Email;
    }
}
