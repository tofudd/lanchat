﻿using System;
using System.IO;
using System.Security;
using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal.Commands
{
    public class SendFile : ICommand
    {
        public string Alias { get; set; } = "send";
        public int ArgsCount { get; set; } = 2;

        public void Execute(string[] args)
        {
            var node = Program.Network.Nodes.Find(x => x.ShortId == args[0]);
            if (node == null)
            {
                Ui.Log.Add(Resources._UserNotFound);
                return;
            }

            try
            {
                node.FileSender.CreateSendRequest(args[1]);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case FileNotFoundException:
                    case UnauthorizedAccessException:
                    case SecurityException:
                    case PathTooLongException:
                    case ArgumentException:
                        Ui.Log.Add(string.Format(Resources._CannotAccessFile, args[1]));
                        break;
                    
                    case InvalidOperationException:
                        Ui.Log.Add(Resources._FileTransferInProgress);
                        break;
                }
            }
        }
    }
}