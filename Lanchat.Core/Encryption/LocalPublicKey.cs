using System;
using System.Security.Cryptography;
using Lanchat.Core.Config;

namespace Lanchat.Core.Encryption
{
    internal class LocalPublicKey : ILocalPublicKey
    {
        public RSA LocalRsa { get; }

        public LocalPublicKey(IRsaDatabase rsaDatabase)
        {
            try
            {
                LocalRsa = RSA.Create();
                LocalRsa.ImportFromPem(rsaDatabase.GetLocalPem());
            }
            catch (ArgumentException)
            {
                LocalRsa = RSA.Create(2048);
                var pemFile = PemEncoding.Write("RSA PRIVATE KEY", LocalRsa.ExportRSAPrivateKey());
                rsaDatabase.SaveLocalPem(new string(pemFile));
            }
        }
    }
}