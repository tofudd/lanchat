﻿// Lanchat 2
// Let's all love lain

using Lanchat.Common.NetworkLib;
using Lanchat.Console.Commands;
using Lanchat.Console.Ui;
using System;
using System.Diagnostics;
using System.Threading;

namespace Lanchat.Console.ProgramLib
{
    public class Program
    {
        private bool _DeugMode;
        private TraceListener consoleTraceListener;
        public Command Commands { get; set; }
        public bool DebugMode
        {
            get
            {
                return _DeugMode;
            }

            set
            {
                _DeugMode = value;
                if (value)
                {
                    consoleTraceListener = new TextWriterTraceListener(System.Console.Out, "Console");
                    Trace.Listeners.Add(consoleTraceListener);
                    Prompt.Notice("Debug mode enabled");
                }
                else
                {
                    Trace.Listeners.Remove(consoleTraceListener);
                    consoleTraceListener.Dispose();
                    Prompt.Notice("Debug mode disabled");
                }
            }
        }

        public Network Network { get; set; }
        public Prompt Prompt { get; set; }
        public void Start()
        {
            // Check is debug enabled
            Debug.Assert(DebugMode = true);

            Config.Load();

            // Start log
            Trace.Listeners.Add(new TextWriterTraceListener($"{Config.Path}{DateTime.Now.ToString("yyyy_MM_dd")}.log", "LogFile"));
            
            Trace.AutoFlush = true;

            Prompt.Welcome();

            // Check nickname
            if (string.IsNullOrWhiteSpace(Config.Nickname))
            {
                var nick = Prompt.Query("Nickname:").Trim();

                while (nick.Length >= 20 || string.IsNullOrWhiteSpace(nick))
                {
                    Prompt.Alert("Nick cannot be blank or longer than 20 characters");
                    nick = Prompt.Query("Choose nickname:").Trim();
                }
                Config.Nickname = nick;
            }

            // Initialize commands module
            Commands = new Command(this);

            // Initialize event handlers
            var eventHandlers = new EventHandlers(this);

            // Initialize prompt
            Prompt = new Prompt();
            Prompt.RecievedInput += eventHandlers.OnRecievedInput;
            new Thread(Prompt.Init).Start();

            // Initialize network
            Network = new Network(Config.BroadcastPort, Config.Nickname, Config.HostPort);
            Network.Events.HostStarted += eventHandlers.OnHostStarted;
            Network.Events.ReceivedMessage += eventHandlers.OnRecievedMessage;
            Network.Events.NodeConnected += eventHandlers.OnNodeConnected;
            Network.Events.NodeDisconnected += eventHandlers.OnNodeDisconnected;
            Network.Events.NodeSuspended += eventHandlers.OnNodeSuspended;
            Network.Events.NodeResumed += eventHandlers.OnNodeResumed;
            Network.Events.ChangedNickname += eventHandlers.OnChangedNickname;
            Network.Start();
        }

        private static void Main(string[] args)
        {
            var program = new Program();
            program.Start();
        }
    }
}