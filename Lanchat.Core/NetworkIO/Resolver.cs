using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lanchat.Core.NodeHandlers;

namespace Lanchat.Core.NetworkIO
{
    internal class Resolver
    {
        private readonly INodeState nodeState;
        internal readonly List<IApiHandler> Handlers = new();
        internal readonly List<Type> Models = new();
        private readonly JsonSerializerOptions serializerOptions;

        public Resolver(INodeState nodeState)
        {
            this.nodeState = nodeState;
            serializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };
        }

        internal void Handle(string jsonType, string jsonValue)
        {
            var type = Models.FirstOrDefault(x => x.Name == jsonType);

            if (type == null)
            {
                throw new ArgumentException($"{nodeState.Id} received data of unknown type.", jsonType);
            }

            var data = JsonSerializer.Deserialize(jsonValue, type, serializerOptions);
            var handler = Handlers.FirstOrDefault(x => x.HandledType == type);
            if (handler == null)
            {
                throw new ArgumentException($"{nodeState.Id} has no handler for received data.", jsonType);
            }

            if (!nodeState.Ready && handler.Privileged == false)
            {
                throw new InvalidOperationException($"{nodeState.Id} must be ready to handle this type of data.");
            }
            
            Validator.ValidateObject(data!, new ValidationContext(data), true);
            Trace.WriteLine($"Node {nodeState.Id} received {jsonType}");
            handler.Handle(data);
        }
    }
}