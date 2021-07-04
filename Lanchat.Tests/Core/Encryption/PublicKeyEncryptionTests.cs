using System;
using System.Net;
using System.Security.Cryptography;
using Lanchat.Core.Encryption;
using Lanchat.Core.Encryption.Models;
using Lanchat.Tests.Mock.Config;
using NUnit.Framework;

namespace Lanchat.Tests.Core.Encryption
{
    public class PublicKeyEncryptionTests
    {
        private NodePublicKey nodePublicKeyTest;

        [SetUp]
        public void Setup()
        {
            nodePublicKeyTest = new NodePublicKey(new RsaDatabaseMock(), new EncryptionAlerts());
            nodePublicKeyTest.ImportKey(nodePublicKeyTest.ExportKey(), IPAddress.Loopback);
        }

        [Test]
        public void BytesEncryption()
        {
            var testBytes = new byte[] {0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70};
            var encryptedBytes = nodePublicKeyTest.Encrypt(testBytes);
            var decryptedBytes = nodePublicKeyTest.Decrypt(encryptedBytes);
            Assert.AreEqual(testBytes, decryptedBytes);
        }

        [Test]
        public void ImportInvalidRsa()
        {
            Assert.Catch<CryptographicException>(() =>
            {
                nodePublicKeyTest.ImportKey(new PublicKey
                {
                    RsaExponent = new byte[] {0x10},
                    RsaModulus = new byte[] {0x10}
                }, IPAddress.Loopback);
            });
        }

        [Test]
        public void Dispose()
        {
            nodePublicKeyTest.Dispose();
            Assert.Catch<ObjectDisposedException>(() => { nodePublicKeyTest.Encrypt(new byte[] {0x10}); });
        }
    }
}