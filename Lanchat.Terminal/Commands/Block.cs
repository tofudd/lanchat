﻿using System.Net;
using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal.Commands
{
    public static class Block
    {
        public static void Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Ui.Log.Add(Resources.Manual_Block);
                return;
            }

            var correct = IPAddress.TryParse(args[0], out var parsedIp);
            if (correct)
            {
                Program.Config.AddBlocked(parsedIp);
                Program.Network.Nodes.Find(x => Equals(x.Endpoint.Address, parsedIp))?.Disconnect();
                Ui.Log.Add($"{parsedIp} {Resources.Info_Blocked}");
            }
            else
            {
                Ui.Log.Add(Resources.Info_IncorrectValues);
            }
        }
    }
}