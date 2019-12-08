﻿using System;
using System.Diagnostics;
using System.Net;
using Lanchat.Common.HostLib.Types;

namespace Lanchat.Common.NetworkLib
{
    public partial class Network
    {
        // Create new node
        public void CreateNode(Guid id, int port, IPAddress ip)
        {
            // Create new node with parameters
            var node = new Node(id, port, ip);
            // Add node to list
            NodeList.Add(node);
            // Create connection with node
            node.CreateConnection();
            // Send handshake to node
            node.Client.SendHandshake(new Handshake(Nickname, PublicKey, Id, HostPort));

            // Log
            Trace.WriteLine("New node created");
            Trace.Indent();
            Trace.WriteLine(node.Ip);
            Trace.WriteLine(node.Port.ToString());
            Trace.Unindent();
        }
    }
}