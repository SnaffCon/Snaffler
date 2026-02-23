using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DSInternals.Common
{
    public static class RSAExtensions
    {
        private const int BCryptKeyBlobHeaderSize = 6 * sizeof(uint);
        private const uint BCryptRSAPublicKeyMagic = 0x31415352; // "RSA1" in ASCII
        private const int TPM20KeyBlobHeaderSize = 4 * sizeof(int) + 9 * sizeof(uint);
        private const uint TPM20PublicKeyMagic = 0x4d504350; // "MPCP" in ASCII
        private const byte DERSequenceTag = 0x30;
        private const int DERPublicKeyMinSize = 260; // At least 2K RSA modulus + 3B public exponent + 1B sequence tag

        /// <summary>
        /// OID 1.2.840.113549.1.1.1 - Identifier for RSA encryption for use with Public Key Cryptosystem One defined by RSA Inc. 
        /// </summary>
        private static readonly Oid RsaOid = Oid.FromFriendlyName("RSA", OidGroup.PublicKeyAlgorithm);

        /// <summary>
        /// ASN.1 Tag NULL
        /// </summary>
        private static readonly AsnEncodedData Asn1Null = new AsnEncodedData(new byte[] { 5, 0 });

        /// <summary>
        /// BCRYPT_PUBLIC_KEY_BLOB Format
        /// </summary>
        private static readonly CngKeyBlobFormat BCryptRSAPublicKeyFormat = new CngKeyBlobFormat("RSAPUBLICBLOB");


        /// <summary>
        /// Converts a RSA public key to BCRYPT_RSAKEY_BLOB.
        /// </summary>
        public static byte[] ExportRSAPublicKeyBCrypt(this X509Certificate2 certificate)
        {
            Validator.AssertNotNull(certificate, nameof(certificate));

            using (var rsa = (RSACng)certificate.GetRSAPublicKey())
            {
                using(var key = rsa.Key)
                {
                    return key.Export(BCryptRSAPublicKeyFormat);
                }
            }
        }

        /// <summary>
        /// Decodes a public key from a BCRYPT_RSAKEY_BLOB structure.
        /// </summary>
        public static RSAParameters ImportRSAPublicKeyBCrypt(this byte[] blob)
        {
            Validator.AssertNotNull(blob, nameof(blob));

            using (var key = CngKey.Import(blob, BCryptRSAPublicKeyFormat))
            {
                using (var rsa = new RSACng(key))
                {
                    return rsa.ExportParameters(false);
                }
            }
        }

        /// <summary>
        /// Exports a RSA public key to the DER format.
        /// </summary>
        public static byte[] ExportRSAPublicKeyDER(this X509Certificate2 certificate)
        {
            Validator.AssertNotNull(certificate, nameof(certificate));

            return certificate.PublicKey.EncodedKeyValue.RawData;
        }

        /// <summary>
        /// Decodes a DER RSA public key.
        /// </summary>
        public static RSAParameters ImportRSAPublicKeyDER(this byte[] blob)
        {
            Validator.AssertNotNull(blob, nameof(blob));

            var asn1Key = new AsnEncodedData(blob);
            var publicKey = new PublicKey(RsaOid, Asn1Null, asn1Key);
            using (var rsaKey = (RSACryptoServiceProvider)publicKey.Key)
            {
                return rsaKey.ExportParameters(false);
            }
        }

        /// <summary>
        /// Checks whether the input blob is in the BCRYPT_RSAKEY_BLOB format.
        /// </summary>
        public static bool IsBCryptRSAPublicKeyBlob(this byte[] blob)
        {
            if (blob == null || blob.Length < BCryptKeyBlobHeaderSize)
            {
                return false;
            }

            // Check if the byte sequence starts with the magic
            return BitConverter.ToUInt32(blob, 0) == BCryptRSAPublicKeyMagic;
        }

        /// <summary>
        /// Checks whether the input blob is in the PCP_KEY_BLOB_WIN8 format.
        /// </summary>
        public static bool IsTPM20PublicKeyBlob(this byte[] blob)
        {
            if (blob == null || blob.Length < TPM20KeyBlobHeaderSize)
            {
                return false;
            }

            // Check if the byte sequence starts with the magic
            return BitConverter.ToUInt32(blob, 0) == TPM20PublicKeyMagic;
        }

        /// <summary>
        /// Checks whether the input blob is a DER-encoded public key.
        /// </summary>
        public static bool IsDERPublicKeyBlob(this byte[] blob)
        {
            if (blob == null || blob.Length < DERPublicKeyMinSize)
            {
                return false;
            }

            // Check if the byte sequence starts with a DER sequence tag. This is a very vague test.
            return blob[0] == DERSequenceTag;
        }

        public static bool IsWeakKey(this RSAParameters publicKey)
        {
            return false;
        }
    }
}
