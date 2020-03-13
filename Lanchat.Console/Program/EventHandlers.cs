﻿using Lanchat.Common.NetworkLib.EventsArgs;
using Lanchat.Common.Types;
using Lanchat.Console.Ui;
using System.Diagnostics;

namespace Lanchat.Console.ProgramLib
{
    public class EventHandlers
    {
        public EventHandlers(Program program)
        {
            this.program = program;
        }

        // Main program reference
        private readonly Program program;

        // Input
        public void OnRecievedInput(object o, InputEventArgs e)
        {
            var input = e.Input;

            // Check is input command
            if (input.StartsWith("/"))
            {
                program.Commands.Execute(input.Substring(1));
            }

            // Or message
            else
            {
                Prompt.Out(input, null, program.Config.Nickname);
                program.Network.Methods.SendAll(input);
            }
        }

        // Host started
        public void OnHostStarted(object o, HostStartedEventArgs e)
        {
            Trace.WriteLine($"[APP] Host started on port {e.Port}");
            if (!program.DebugMode)
            {
                Prompt.Notice($"Host started on port {e.Port}");
            }
        }

        // Recieved message
        public void OnReceivedMessage(object o, ReceivedMessageEventArgs e)
        {
            if (!program.DebugMode)
            {
                if (e.Target == MessageTarget.Private)
                {
                    Prompt.Out(e.Content.Trim(), null, e.Node.Nickname + " -> " + program.Config.Nickname);
                }
                else
                {
                    Prompt.Out(e.Content.Trim(), null, e.Node.Nickname);
                }
            }
        }

        // Node connection
        public void OnNodeConnected(object o, NodeConnectionStatusEventArgs e)
        {
            if (!program.DebugMode)
            {
                Prompt.Notice(e.Node.Nickname + " connected");
            }

            if (program.Config.Muted.Exists(x => x == e.Node.Ip.ToString()))
            {
                var node = program.Network.NodeList.Find(x => x.Ip.Equals(e.Node.Ip));
                node.Mute = true;
            }
        }

        // Node disconnection
        public void OnNodeDisconnected(object o, NodeConnectionStatusEventArgs e)
        {
            if (!program.DebugMode)
            {
                Prompt.Notice(e.Node.Nickname + " disconnected");
            }
        }

        // Node suspended
        public void OnNodeSuspended(object o, NodeConnectionStatusEventArgs e)
        {
            if (!program.DebugMode)
            {
                Prompt.Notice(e.Node.Nickname + " suspended. Waiting for reconnect");
            }
        }

        // Node resumed
        public void OnNodeResumed(object o, NodeConnectionStatusEventArgs e)
        {
            if (!program.DebugMode)
            {
                Prompt.Notice(e.Node.Nickname + " reconnected");
            }
        }

        // Changed nickname
        public void OnChangedNickname(object o, ChangedNicknameEventArgs e)
        {
            if (!program.DebugMode)
            {
                Prompt.Notice($"{e.OldNickname} changed nickname to {e.NewNickname}");
            }
        }
    }
}