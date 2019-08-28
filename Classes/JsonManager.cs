using MisakaDocs.JsonModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MisakaDocs.Classes
{
    public class JsonManager<T>
        where T : JsonModelBase
    {
        private int Interval = 30000; //30 seconds
        private Timer CheckForChanges;
        private T CachedJsonObject;
        private string fileName;
        public T JsonObject { get; set; }

        public JsonManager()
        {
            fileName = typeof(T).Name + ".json";
            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, JsonConvert.SerializeObject(Activator.CreateInstance(typeof(T)), typeof(T), Formatting.Indented, new JsonSerializerSettings()));
                JsonObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName));
                CachedJsonObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName));
            }
            else
            {
                JsonObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName));
                CopyObjectToCache();
            }

            CheckForChanges = new Timer((e) =>
            {
                bool objChanged = false;
                var jsonType = this.GetType().GetGenericArguments().First();
                var members = jsonType.GetMembers().Where(x => x.MemberType == MemberTypes.Field);
                foreach(var member in members)
                {
                    var oldVal = (member as FieldInfo).GetValue(CachedJsonObject);
                    var curVal = (member as FieldInfo).GetValue(JsonObject);
                    
                    if (oldVal.ToString() != curVal.ToString())
                    {
                        Console.WriteLine($"Detected change in {jsonType.Name}, updating file.");
                        objChanged = true;
                        break;
                    }
                }
                if (objChanged)
                {
                    File.WriteAllText(fileName, JsonConvert.SerializeObject(JsonObject, Formatting.Indented));
                    CopyObjectToCache(); 
                }
                CheckForChanges.Change(Interval, 0);
            }, null, Interval, 0);
        }

        public void Save()
        {
            CopyObjectToCache();
            File.WriteAllText(fileName, JsonConvert.SerializeObject(JsonObject, Formatting.Indented));
        }

        public void Reload()
        {
            CopyObjectToCache();
            JsonObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName));
        }

        //Hacky way to make a copy of the class.
        private void CopyObjectToCache()
        {
            CachedJsonObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName));
        }
    }
}
