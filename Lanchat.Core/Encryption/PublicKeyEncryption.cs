﻿using System;
using System.Security.Cryptography;
using Lanchat.Core.Config;
using Lanchat.Core.Encryption.Models;

namespace Lanchat.Core.Encryption
{
    internal class PublicKeyEncryption : IPublicKeyEncryption
    {
        private readonly IConfig config;
        private readonly RSA localRsa;
        private readonly RSA remoteRsa;

        public PublicKeyEncryption(IConfig config)
        {
            this.config = config;

            try
            {
                localRsa = RSA.Create();
                localRsa.ImportRSAPrivateKey(config.PublicKey, out _);
            }
            catch (CryptographicException)
            {
                localRsa = RSA.Create(2048);
                config.PublicKey = localRsa.ExportRSAPrivateKey();
            }
            
            remoteRsa = RSA.Create();
        }

        public void Dispose()
        {
            localRsa?.Dispose();
            remoteRsa?.Dispose();
            GC.SuppressFinalize(this);
        }

        public PublicKey ExportKey()
        {
            var parameters = localRsa.ExportParameters(false);
            return new PublicKey
            {
                RsaModulus = parameters.Modulus,
                RsaExponent = parameters.Exponent
            };
        }

        public void ImportKey(PublicKey publicKey)
        {
            var parameters = new RSAParameters
            {
                Modulus = publicKey.RsaModulus,
                Exponent = publicKey.RsaExponent
            };

            remoteRsa.ImportParameters(parameters);
            TestKeys();
        }

        public byte[] Encrypt(byte[] bytes)
        {
            return remoteRsa.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
        }

        public byte[] Decrypt(byte[] encryptedBytes)
        {
            return localRsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
        }

        public byte[] GetRemotePublicKey()
        {
            return remoteRsa.ExportRSAPublicKey();
        }

        private void TestKeys()
        {
            remoteRsa.Encrypt(new byte[] {0x10}, RSAEncryptionPadding.Pkcs1);
        }
    }
}