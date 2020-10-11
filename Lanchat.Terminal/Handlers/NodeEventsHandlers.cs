﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using Lanchat.Core;
using Lanchat.Terminal.Ui;

namespace Lanchat.Terminal.Handlers
{
    public class NodeEventsHandlers
    {
        private readonly Node node;

        public NodeEventsHandlers(Node node)
        {
            this.node = node;
            node.NetworkInput.MessageReceived += OnMessageReceived;
            node.NetworkInput.PingReceived += OnPingReceived;
            node.Connected += OnNodeConnected;
            node.Disconnected += OnNodeDisconnected;
            node.SocketErrored += OnSocketErrored;
        }

        private void OnNodeConnected(object sender, EventArgs e)
        {
            Prompt.Log.Add($"{node.Nickname} connected");
            Prompt.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnNodeDisconnected(object sender, EventArgs e)
        {
            Prompt.Log.Add($"{node.Nickname} disconnected");
            Prompt.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }
        
        private void OnMessageReceived(object sender, string e)
        {
            Prompt.Log.AddMessage(e, node.Nickname);
        }
        
        private void OnPingReceived(object sender, EventArgs e)
        {
            Prompt.Log.Add($"{node.Nickname} sent ping");
        }
        
        private void OnSocketErrored(object sender, SocketError e)
        {
            Trace.WriteLine($"[NODE] Socket error {node.Id} / {e}");
            Prompt.Log.Add($"Connection error {e}");
        }
    }
}