﻿using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Lanchat.ClientCore;
using Lanchat.Core.Network;
using Lanchat.Terminal.Handlers;
using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal
{
    public static class Program
    {
        public static Window Window { get; private set; }
        public static IP2P Network { get; private set; }
        public static Config Config { get; private set; }

        private static void Main(string[] args)
        {
            Config = Storage.LoadConfig();

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(Config.Language);
            }
            catch
            {
                Trace.WriteLine("Cannot load translation. Using default.");
            }

            Resources.Culture = CultureInfo.CurrentCulture;

            Window = new Window();
            Network = new P2P(Config, x => { _ = new NodeHandlers(x.Instance, Window.TabsManager); });

            CheckStartArguments(args);
            Window.Start();
            Logger.StartLogging();

            try
            {
                Network.Start();
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.AddressAlreadyInUse)
                {
                    throw;
                }

                Window.TabsManager.HomeView.AddAlert(Resources._PortBusy);
            }

            if (args.Contains("--localhost") || args.Contains("-l"))
            {
                Network.Connect(IPAddress.Loopback);
            }

            Logger.DeleteOldLogs(5);
        }

        private static void CheckStartArguments(string[] args)
        {
            if (args.Contains("--no-saved") || args.Contains("-a"))
            {
                Config.ConnectToSaved = false;
            }

            if (args.Contains("--no-udp") || args.Contains("-b"))
            {
                Config.NodesDetection = false;
            }

            if (args.Contains("--no-server") || args.Contains("-n"))
            {
                Config.StartServer = false;
            }

            if (args.Contains("--debug") || args.Contains("-d") || Debugger.IsAttached)
            {
                Config.DebugMode = true;
                Trace.Listeners.Add(new TraceListener());
            }
            else
            {
                var newVersion = UpdateChecker.CheckUpdates();
                if (newVersion != null)
                {
                    Window.TabsManager.HomeView.AddAlert($"Update available: {newVersion}");
                }
            }
        }
    }
}