using DSInternals.Common.Data;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SnaffCore.ADWS
{
    public class ADObject
    {
        public string Class { get; set; }
        public int AdminCount { get; set; }
        public X509Certificate2Collection CACertificate { get; set; }
        public string[] CertificateTemplates { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string DistinguishedName { get; set; }
        public string DNSHostName { get; set; }
        public string Cn { get; set; }
        public byte[] DnsRecord { get; set; }
        public int DSMachineAccountQuota { get; set; }
        public string GPCFileSysPath { get; set; }
        public string IsDeleted { get; set; }
        public string GPLink { get; set; }
        public int GPOptions { get; set; }
        public DateTime LastLogon { get; set; }
        public DateTime LastLogonTimestamp { get; set; }
        public string[] Member { get; set; }
        public ActiveDirectorySecurity MsDSAllowedToActOnBehalfOfOtherIdentity { get; set; }
        public KeyCredential[] MsDSKeyCredentialLink { get; set; }
        public string[] MsDSAllowedToDelegateTo { get; set; }
        public int FunctionalLevel { get; set; }
        public long MsMCSAdmPwdExpirationTime { get; set; }
        public int MsPKICertificateNameFlag { get; set; }
        public int MsPKIMinimalKeySize { get; set; }
        public int MsPKIEnrollmentFlag { get; set; }
        public int MsPKIPrivateKeyFlag { get; set; }
        public string Name { get; set; }
        public ActiveDirectorySecurity NTSecurityDescriptor { get; set; }
        public Guid ObjectGUID { get; set; }
        public SecurityIdentifier ObjectSid { get; set; }
        public string OperatingSystem { get; set; }
        public string[] PKIExtendedKeyUsage { get; set; }
        public int PrimaryGroupID { get; set; }
        public DateTime PwdLastSet { get; set; }
        public string SAMAccountName { get; set; }
        public string ScriptPath { get; set; }
        public SecurityIdentifier SecurityIdentifier { get; set; }
        public string[] ServicePrincipalName { get; set; }
        public SecurityIdentifier[] SIDHistory { get; set; }
        public int TrustAttributes { get; set; }
        public int TrustDirection { get; set; }
        public int UserAccountControl { get; set; }
        public DateTime WhenCreated { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string HomeDirectory { get; set; }
        public string UserPassword { get; set; }
        public string UnixUserPassword { get; set; }
        public string UnicodePassword { get; set; }
        public string MsSFU30Password { get; set; }
        public byte[] PKIExpirationPeriod { get; set; }
        public byte[] PKIOverlapPeriod { get; set; }
        public ADObject()
        {
        }
    }


}
