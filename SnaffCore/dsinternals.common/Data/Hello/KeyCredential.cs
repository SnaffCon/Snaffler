namespace DSInternals.Common.Data
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    ///  This class represents a single AD/AAD key credential.
    /// </summary>
    /// <remarks>
    /// In Active Directory, this structure is stored as the binary portion of the msDS-KeyCredentialLink DN-Binary attribute
    /// in the KEYCREDENTIALLINK_BLOB format.
    /// The Azure Active Directory Graph API represents this structure in JSON format.
    /// </remarks>
    /// <see>https://msdn.microsoft.com/en-us/library/mt220505.aspx</see>
    public class KeyCredential
    {
        /// <summary>
        /// Minimum length of the structure.
        /// </summary>
        private const int MinLength = sizeof(uint); // Version

        /// <summary>
        /// V0 structure alignment in bytes.
        /// </summary>
        private const ushort PackSize = 4;

        /// <summary>
        /// Defines the version of the structure.
        /// </summary>
        public KeyCredentialVersion Version
        {
            get;
            private set;
        }

        /// <summary>
        /// A SHA256 hash of the Value field of the RawKeyMaterial entry.
        /// </summary>
        /// <remarks>
        /// Version 1 keys had a guid in this field instead if a hash.
        /// </remarks>
        public string Identifier
        {
            get;
            private set;
        }

        public bool IsWeak
        {
            get
            {
                var key = this.RSAPublicKey;
                return key.HasValue && key.Value.IsWeakKey();
            }
        }

        public KeyUsage Usage
        {
            get;
            private set;
        }

        public string LegacyUsage
        {
            get;
            private set;
        }

        public KeySource Source
        {
            get;
            private set;
        }

        /// <summary>
        /// Key material of the credential.
        /// </summary>
        public byte[] RawKeyMaterial
        {
            get;
            private set;
        }

        public RSAParameters? RSAPublicKey
        {
            get
            {
                if(this.RawKeyMaterial == null)
                {
                    return null;
                }

                if(this.Usage == KeyUsage.NGC || this.Usage == KeyUsage.STK)
                {
                    // The RSA public key can be stored in at least 3 different formats.

                    if (this.RawKeyMaterial.IsBCryptRSAPublicKeyBlob())
                    {
                        // This public key is in DER format. This is typically true for device/computer keys.
                        return this.RawKeyMaterial.ImportRSAPublicKeyBCrypt();
                    }
                    else if(this.RawKeyMaterial.IsTPM20PublicKeyBlob())
                    {
                        // This public key is encoded as PCP_KEY_BLOB_WIN8. This is typically true for device keys protected by TPM.
                        // The PCP_KEY_BLOB_WIN8 structure is not yet supported by DSInternals.
                        return null;
                    }
                    else if(this.RawKeyMaterial.IsDERPublicKeyBlob())
                    {
                        // This public key is encoded as BCRYPT_RSAKEY_BLOB. This is typically true for user keys.
                        return this.RawKeyMaterial.ImportRSAPublicKeyDER();
                    }
                }

                // Other key usages probably do not contain any public keys.
                return null;
            }
        }

        public string RSAModulus
        {
            get
            {
                var publicKey = this.RSAPublicKey;
                return publicKey.HasValue ? Convert.ToBase64String(publicKey.Value.Modulus) : null;
            }
        }

        public CustomKeyInformation CustomKeyInfo
        {
            get;
            private set;
        }

        public Guid? DeviceId
        {
            get;
            private set;
        }

        /// <summary>
        /// The approximate time this key was created.
        /// </summary>
        public DateTime CreationTime
        {
            get;
            private set;
        }

        /// <summary>
        /// The approximate time this key was last used.
        /// </summary>
        public DateTime? LastLogonTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Distinguished name of the AD object (UPN in case of AAD objects) that holds this key credential.
        /// </summary>
        public string Owner
        {
            get;
            // We need to update this property after JSON deserialization, so it is internal instead of private.
            internal set;
        }

        public KeyCredential(X509Certificate2 certificate, Guid? deviceId, string owner, DateTime? currentTime = null, bool isComputerKey = false)
        {
            Validator.AssertNotNull(certificate, nameof(certificate));

            // Computer NGC keys are DER-encoded, while user NGC keys are encoded as BCRYPT_RSAKEY_BLOB.
            byte[] publicKey = isComputerKey ? certificate.ExportRSAPublicKeyDER() : certificate.ExportRSAPublicKeyBCrypt();
            this.Initialize(publicKey, deviceId, owner, currentTime, isComputerKey);
        }

        public KeyCredential(byte[] publicKey, Guid? deviceId, string owner, DateTime? currentTime = null, bool isComputerKey = false)
        {
            Validator.AssertNotNull(publicKey, nameof(publicKey));
            this.Initialize(publicKey, deviceId, owner, currentTime, isComputerKey);
        }

        private void Initialize(byte[] publicKey, Guid? deviceId, string owner, DateTime? currentTime, bool isComputerKey)
        {
            // Prodess owner DN/UPN
            Validator.AssertNotNullOrEmpty(owner, nameof(owner));
            this.Owner = owner;

            // Initialize the Key Credential based on requirements stated in MS-KPP Processing Details:
            this.Version = KeyCredentialVersion.Version2;
            this.Identifier = ComputeKeyIdentifier(publicKey, this.Version);
            this.CreationTime = currentTime.HasValue ? currentTime.Value.ToUniversalTime() : DateTime.UtcNow;
            this.RawKeyMaterial = publicKey;
            this.Usage = KeyUsage.NGC;
            this.Source = KeySource.AD;
            this.DeviceId = deviceId;

            // Computer NGC keys have to meet some requirements to pass the validated write
            // The CustomKeyInformation entry is not present.
            // The KeyApproximateLastLogonTimeStamp entry is not present.
            if (!isComputerKey)
            {
                this.LastLogonTime = this.CreationTime;
                this.CustomKeyInfo = new CustomKeyInformation(KeyFlags.None);
            }
        }

        public KeyCredential(byte[] blob, string owner)
        {
            // Input validation
            Validator.AssertNotNull(blob, nameof(blob));
            Validator.AssertMinLength(blob, MinLength, nameof(blob));
            Validator.AssertNotNullOrEmpty(owner, nameof(owner));
            
            // Init
            this.Owner = owner;

            // Parse binary input
            using (var stream = new MemoryStream(blob, false))
            {
                using (var reader = new BinaryReader(stream))
                {
                    this.Version = (KeyCredentialVersion) reader.ReadUInt32();

                    // Read all entries corresponding to the KEYCREDENTIALLINK_ENTRY structure:
                    do
                    {
                        // A 16-bit unsigned integer that specifies the length of the Value field.
                        ushort length = reader.ReadUInt16();

                        // An 8-bit unsigned integer that specifies the type of data that is stored in the Value field.
                        KeyCredentialEntryType entryType = (KeyCredentialEntryType) reader.ReadByte();

                        // A series of bytes whose size and meaning are defined by the Identifier field.
                        byte[] value = reader.ReadBytes(length);

                        if(this.Version == KeyCredentialVersion.Version0)
                        {
                            // Data used to be aligned to 4B in this legacy format.
                            int paddingLength = (PackSize - length % PackSize) % PackSize;
                            reader.ReadBytes(paddingLength);
                        }

                        // Now parse the value of the current entry based on its type:
                        switch (entryType)
                        {
                            case KeyCredentialEntryType.KeyID:
                                this.Identifier = ConvertFromBinaryIdentifier(value, this.Version);
                                break;
                            case KeyCredentialEntryType.KeyHash:
                                // We do not need to validate the integrity of the data by the hash
                                break;
                            case KeyCredentialEntryType.KeyMaterial:
                                this.RawKeyMaterial = value;
                                break;
                            case KeyCredentialEntryType.KeyUsage:
                                if(length == sizeof(byte))
                                {
                                    // This is apparently a V2 structure
                                    this.Usage = (KeyUsage)value[0];
                                }
                                else
                                {
                                    // This is a legacy structure that contains a string-encoded key usage instead of enum.
                                    this.LegacyUsage = System.Text.Encoding.UTF8.GetString(value);
                                }
                                break;
                            case KeyCredentialEntryType.KeySource:
                                this.Source = (KeySource)value[0];
                                break;
                            case KeyCredentialEntryType.DeviceId:
                                this.DeviceId = new Guid(value);
                                break;
                            case KeyCredentialEntryType.CustomKeyInformation:
                                this.CustomKeyInfo = new CustomKeyInformation(value);
                                break;
                            case KeyCredentialEntryType.KeyApproximateLastLogonTimeStamp:
                                this.LastLogonTime = ConvertFromBinaryTime(value, this.Source, this.Version);
                                break;
                            case KeyCredentialEntryType.KeyCreationTime:
                                this.CreationTime = ConvertFromBinaryTime(value, this.Source, this.Version);
                                break;
                            default:
                                // Unknown entry type. We will just ignore it.
                                break;
                        }
                    } while (reader.BaseStream.Position != reader.BaseStream.Length);
                }
            }
        }

        /// <summary>
        /// This constructor is only used for JSON deserialization.
        /// </summary>
        private KeyCredential()
        {
            this.Source = KeySource.AzureAD;
            this.Version = KeyCredentialVersion.Version2;
        }

        public override string ToString()
        {
            return String.Format(
                "Id: {0}, Source: {1}, Version: {2}, Usage: {3}, CreationTime: {4}",
                this.Identifier,
                this.Source,
                this.Version,
                this.Usage,
                this.CreationTime);
        }

        public byte[] ToByteArray()
        {
            // Note that we do not support the legacy V1 format.

            // Serialize properties 3-9 first, as property 2 must contain their hash:
            byte[] binaryProperties;
            using (var propertyStream = new MemoryStream())
            {
                using (var propertyWriter = new BinaryWriter(propertyStream))
                {
                    // Key Material
                    propertyWriter.Write((ushort)this.RawKeyMaterial.Length);
                    propertyWriter.Write((byte)KeyCredentialEntryType.KeyMaterial);
                    propertyWriter.Write(this.RawKeyMaterial);

                    // Key Usage
                    propertyWriter.Write((ushort)sizeof(KeyUsage));
                    propertyWriter.Write((byte)KeyCredentialEntryType.KeyUsage);
                    propertyWriter.Write((byte)this.Usage);

                    // Key Source
                    propertyWriter.Write((ushort)sizeof(KeySource));
                    propertyWriter.Write((byte)KeyCredentialEntryType.KeySource);
                    propertyWriter.Write((byte)this.Source);

                    // Device ID
                    if(this.DeviceId.HasValue)
                    {
                        byte[] binaryGuid = this.DeviceId.Value.ToByteArray();
                        propertyWriter.Write((ushort)binaryGuid.Length);
                        propertyWriter.Write((byte)KeyCredentialEntryType.DeviceId);
                        propertyWriter.Write(binaryGuid);
                    }

                    // Custom Key Information
                    if(this.CustomKeyInfo != null)
                    {
                        byte[] binaryKeyInfo = this.CustomKeyInfo.ToByteArray();
                        propertyWriter.Write((ushort)binaryKeyInfo.Length);
                        propertyWriter.Write((byte)KeyCredentialEntryType.CustomKeyInformation);
                        propertyWriter.Write(binaryKeyInfo);
                    }

                    // Last Logon Time
                    if(this.LastLogonTime.HasValue)
                    {
                        byte[] binaryLastLogonTime = ConvertToBinaryTime(this.LastLogonTime.Value, this.Source, this.Version);
                        propertyWriter.Write((ushort)binaryLastLogonTime.Length);
                        propertyWriter.Write((byte)KeyCredentialEntryType.KeyApproximateLastLogonTimeStamp);
                        propertyWriter.Write(binaryLastLogonTime);
                    }

                    // Creation Time
                    byte[] binaryCreationTime = ConvertToBinaryTime(this.CreationTime, this.Source, this.Version);
                    propertyWriter.Write((ushort)binaryCreationTime.Length);
                    propertyWriter.Write((byte)KeyCredentialEntryType.KeyCreationTime);
                    propertyWriter.Write(binaryCreationTime);
                }
                binaryProperties = propertyStream.ToArray();
            }

            using (var blobStream = new MemoryStream())
            {
                using (var blobWriter = new BinaryWriter(blobStream))
                {
                    // Version
                    blobWriter.Write((uint)this.Version);

                    // Key Identifier
                    byte[] binaryKeyId = ConvertToBinaryIdentifier(this.Identifier, this.Version);
                    blobWriter.Write((ushort)binaryKeyId.Length);
                    blobWriter.Write((byte)KeyCredentialEntryType.KeyID);
                    blobWriter.Write(binaryKeyId);

                    // Key Hash
                    byte[] keyHash = ComputeHash(binaryProperties);
                    blobWriter.Write((ushort)keyHash.Length);
                    blobWriter.Write((byte)KeyCredentialEntryType.KeyHash);
                    blobWriter.Write(keyHash);

                    // Append the remaining entries
                    blobWriter.Write(binaryProperties);
                }
                return blobStream.ToArray();
            }
        }

        public string ToDNWithBinary()
        {
            // This method should only be used when the owner is in the form of a Distinguished Name.
            return new DNWithBinary(this.Owner, this.ToByteArray()).ToString();
        }

        public static KeyCredential ParseDNBinary(string dnWithBinary)
        {
            Validator.AssertNotNullOrEmpty(dnWithBinary, nameof(dnWithBinary));
            var parsed = DNWithBinary.Parse(dnWithBinary);
            return new KeyCredential(parsed.Binary, parsed.DistinguishedName);
        }

        private static DateTime ConvertFromBinaryTime(byte[] binaryTime, KeySource source, KeyCredentialVersion version)
        {
            long timeStamp = BitConverter.ToInt64(binaryTime, 0);

            // AD and AAD use a different time encoding.
            switch (version)
            {
                case KeyCredentialVersion.Version0:
                    return new DateTime(timeStamp);
                case KeyCredentialVersion.Version1:
                    return DateTime.FromBinary(timeStamp);
                case KeyCredentialVersion.Version2:
                default:
                    return source == KeySource.AD ? DateTime.FromFileTime(timeStamp) : DateTime.FromBinary(timeStamp);
            }
        }

        private static byte[] ConvertToBinaryTime(DateTime time, KeySource source, KeyCredentialVersion version)
        {
            long timeStamp;
            switch (version)
            {
                case KeyCredentialVersion.Version0:
                    timeStamp = time.Ticks;
                    break;
                case KeyCredentialVersion.Version1:
                    timeStamp = time.ToBinary();
                    break;
                case KeyCredentialVersion.Version2:
                default:
                    timeStamp = source == KeySource.AD ? time.ToFileTime() : time.ToBinary();
                    break;
            }

            return BitConverter.GetBytes(timeStamp);
        }

        private static byte[] ComputeHash(byte[] data)
        {
            using (var sha256 = new SHA256Managed())
            {
                return sha256.ComputeHash(data);
            }
        }

        private static string ComputeKeyIdentifier(byte[] keyMaterial, KeyCredentialVersion version)
        {
            byte[] binaryId = ComputeHash(keyMaterial);
            return ConvertFromBinaryIdentifier(binaryId, version);
        }

        private static string ConvertFromBinaryIdentifier(byte[] binaryId, KeyCredentialVersion version)
        {
            switch (version)
            {
                case KeyCredentialVersion.Version0:
                case KeyCredentialVersion.Version1:
                    return binaryId.ToHex(true);
                case KeyCredentialVersion.Version2:
                default:
                    return Convert.ToBase64String(binaryId);
            }
        }

        private static byte[] ConvertToBinaryIdentifier(string keyIdentifier, KeyCredentialVersion version)
        {
            switch (version)
            {
                case KeyCredentialVersion.Version0:
                case KeyCredentialVersion.Version1:
                    return keyIdentifier.HexToBinary();
                case KeyCredentialVersion.Version2:
                default:
                    return Convert.FromBase64String(keyIdentifier);
            }
        }
    }
}
