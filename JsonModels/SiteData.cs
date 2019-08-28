using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MisakaDocs.JsonModels
{
    public class SiteData : JsonModelBase
    {
        public SiteData() { }

        public int SiteVisits;

        public int UserLogins;

        public int BugsReported;

        public int FeaturesSuggested;

        public List<string> UsersLoggedIn;

        public List<string> VisitorAddresses;
    }
}