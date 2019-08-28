using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MisakaDocs.JsonModels
{
    public class ArgumentInfo : JsonModelBase
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class CommandInfo : JsonModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ArgumentInfo> Arguments { get; set; } = new List<ArgumentInfo>();
    }

    public class ModuleInfo : JsonModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string GlyphIcon { get; set; }
        public List<CommandInfo> Commands { get; set; } = new List<CommandInfo>();
    }

    public class ModuleContainer : JsonModelBase
    {
        public List<ModuleInfo> Modules { get; set; } = new List<ModuleInfo>();
    }
}
