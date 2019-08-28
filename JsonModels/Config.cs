using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MisakaDocs.JsonModels
{
    public class Config : JsonModelBase
    {
        public Config() { }

        public string DiscordClientId { get; set; }

        public string DiscordClientSecret { get; set; }

        public string MisakaClientId { get; set; }

        public string BaseUrl { get; set; }

        public int Permissions { get; set; }

        public string EmailAddress { get; set; }

        public string EmailPassword { get; set; }

        public string TargetEmailAddress { get; set; }
    }
}
