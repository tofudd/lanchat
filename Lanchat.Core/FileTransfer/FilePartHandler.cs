using System;
using Lanchat.Core.Api;
using Lanchat.Core.Models;

namespace Lanchat.Core.FileTransfer
{
    internal class FilePartHandler : ApiHandler<FilePart>
    {
        private readonly FileReceiver fileReceiver;

        public FilePartHandler(FileReceiver fileReceiver)
        {
            this.fileReceiver = fileReceiver;
        }

        protected override void Handle(FilePart filePart)
        {
            if (fileReceiver.Request is not {Accepted: true})
            {
                return;
            }

            try
            {
                SavePart(filePart);
            }
            catch
            {
                fileReceiver.CancelReceive();
                fileReceiver.OnFileTransferError();
            }
        }

        private void SavePart(FilePart filePart)
        {
            var base64Data = filePart.Data;
            var data = Convert.FromBase64String(base64Data);
            fileReceiver.WriteFileStream.Write(data);
            fileReceiver.Request.PartsTransferred++;
        }
    }
}