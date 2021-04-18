﻿using Lanchat.Core.Encryption;
using Lanchat.Core.Json;
using Lanchat.Core.Network;

namespace Lanchat.Core.API
{
    /// <summary>
    ///     Send data other of type not belonging to standard Lanchat.Core set.
    /// </summary>
    public class Output : IOutput
    {
        private readonly IModelEncryption encryption;
        private readonly JsonUtils jsonUtils;
        private readonly INetworkElement networkElement;
        private readonly INodeInternals nodeInternals;

        internal Output(INetworkElement networkElement, INodeInternals nodeInternals, IModelEncryption encryption)
        {
            this.networkElement = networkElement;
            this.nodeInternals = nodeInternals;
            this.encryption = encryption;
            jsonUtils = new JsonUtils();
        }

        /// <summary>
        ///     Send data.
        /// </summary>
        /// <param name="content">Object to send.</param>
        public void SendData(object content)
        {
            if (!nodeInternals.Ready)
            {
                return;
            }

            encryption.EncryptObject(content);
            networkElement.Send(jsonUtils.Serialize(content));
        }

        /// <summary>
        ///     Send the data before marking the node as ready (Handshake, KeyInfo...).
        /// </summary>
        /// <param name="content">Object to send.</param>
        public void SendPrivilegedData(object content)
        {
            encryption.EncryptObject(content);
            networkElement.Send(jsonUtils.Serialize(content));
        }
    }
}