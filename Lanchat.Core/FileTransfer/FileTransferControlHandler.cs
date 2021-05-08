using System.Diagnostics;
using Lanchat.Core.Api;
using Lanchat.Core.Models;

namespace Lanchat.Core.FileTransfer
{
    internal class FileTransferControlHandler : ApiHandler<FileTransferControl>
    {
        private readonly FileReceiver fileReceiver;
        private readonly FileSender fileSender;

        internal FileTransferControlHandler(FileReceiver fileReceiver, FileSender fileSender)
        {
            this.fileReceiver = fileReceiver;
            this.fileSender = fileSender;
        }

        protected override void Handle(FileTransferControl request)
        {
            switch (request.Status)
            {
                case FileTransferStatus.Accepted:
                    fileSender.SendFile();
                    break;

                case FileTransferStatus.Rejected:
                    fileSender.HandleReject();
                    break;

                case FileTransferStatus.Finished:
                    fileReceiver.FinishReceive();
                    break;

                case FileTransferStatus.ReceiverError:
                    fileSender.HandleError();
                    break;

                case FileTransferStatus.SenderError:
                    fileReceiver.HandleError();
                    break;
                
                default:
                    Trace.Write("Node received file exchange request of unknown type.");
                    break;
            }
        }
    }
}