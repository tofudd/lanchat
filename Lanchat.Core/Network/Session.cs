﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace Lanchat.Core.Network
{
    internal class Session : TcpSession, INetworkElement
    {
        internal Session(TcpServer server) : base(server)
        { }

        public IPEndPoint Endpoint => (IPEndPoint) Socket.RemoteEndPoint;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<string> DataReceived;
        public event EventHandler<SocketError> SocketErrored;

        public new void SendAsync(string text)
        {
            base.SendAsync(text);
        }

        public void Close()
        {
            Disconnect();
            Dispose();
        }

        protected override void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            var json = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            DataReceived?.Invoke(this, json);
        }

        protected override void OnError(SocketError error)
        {
            SocketErrored?.Invoke(this, error);
        }
    }
}