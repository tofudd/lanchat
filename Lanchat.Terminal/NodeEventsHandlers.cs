﻿using System;
using System.Net.Sockets;
using Lanchat.Core;
using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal
{
    public class NodeEventsHandlers
    {
        private readonly Node node;

        public NodeEventsHandlers(Node node)
        {
            this.node = node;
            node.NetworkInput.MessageReceived += OnMessageReceived;
            node.Connected += OnConnected;
            node.Disconnected += OnDisconnected;
            node.HardDisconnect += OnHardDisconnected;
            node.SocketErrored += OnSocketErrored;
            node.NicknameChanged += OnNicknameChanged;
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Ui.Log.Add($"{node.Nickname} {Resources.Info_Connected}");
            Ui.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Ui.Log.Add($"{node.Nickname} {Resources.Info_Reconnecting}");
            Ui.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnHardDisconnected(object sender, EventArgs e)
        {
            Ui.Log.Add($"{node.Nickname} {Resources.Info_Disconnected}");
            Ui.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnMessageReceived(object sender, string e)
        {
            Ui.Log.AddMessage(e, node.Nickname);
        }

        private void OnSocketErrored(object sender, SocketError e)
        {
            Ui.Log.Add($"{Resources.Info_ConnectionError}: {node.Nickname} / {e}");
        }

        private void OnNicknameChanged(object sender, string e)
        {
            Ui.Log.Add($"{e} {Resources.Info_NicknameChanged} {node.Nickname}");
        }
    }
}