using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CacheInFunctions
{
    public class SerializableObject
    {
        public string TypeName { get; set; }
        public string Data { get; set; }

        // Remove the duplicate constructor
        public SerializableObject(string typeName, string data)
        {
            TypeName = typeName;
            Data = data; // Initialize to avoid CS8618
        }

        public SerializableObject() { }

    }

}
