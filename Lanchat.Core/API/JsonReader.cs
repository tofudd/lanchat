using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lanchat.Core.API
{
    internal class JsonReader
    {
        private readonly JsonSerializerOptions serializerOptions;
        internal readonly List<Type> KnownModels = new();

        public JsonReader()
        {
            serializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };
        }

        internal T Deserialize<T>(string item)
        {
            var wrapper = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(item, serializerOptions);
            return JsonSerializer.Deserialize<T>(wrapper!.Values.First().ToString()!, serializerOptions);
        }

        internal object Deserialize(string item)
        {
            var wrapper = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(item, serializerOptions);
            if (wrapper == null)
            {
                throw new ArgumentException(item);
            }

            var type = KnownModels.First(x => x.Name == wrapper.Keys.First());
            var serializedContent = wrapper.Values.First().ToString();
            if (serializedContent == null)
            {
                throw new ArgumentException(item);
            }

            return JsonSerializer.Deserialize(serializedContent, type, serializerOptions);
        }
    }
}