using Microsoft.ActiveDirectory.Management.WSE;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using SnaffCore.ADWS.Transfer;
using System.Globalization;
using System.Security.Principal;
using System.DirectoryServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.ActiveDirectory.WebServices.Proxy;
using DSInternals.Common.Data;

namespace SnaffCore.ADWS.Enumeration
{
    internal class PullRequest
    {
        string Instance { get; set; }
        string BaseUri { get; set; }
        NetworkCredential Credentials { get; set; }
        NetTcpBinding Binding { get; set; }

        public PullRequest(ADWSConnection adwsConnection)
        {
            this.Instance = adwsConnection.Instance;
            this.BaseUri = adwsConnection.BaseUri;
            this.Binding = adwsConnection.Binding;
            this.Credentials = adwsConnection.Credentials;
        }

        public List<ADObject> Pull(SearchClient searchClient, string enumerationContext, uint maxElements = 1000)
        {
            DirectoryControl[] controls = new DirectoryControl[1];
            controls[0] = new SecurityDescriptorFlagControl(System.DirectoryServices.Protocols.SecurityMasks.Dacl);

            var adObjects = new List<ADObject>();
            bool endOfSequence = false;
            while (!endOfSequence)
            {
                ADPullRequest Request = new ADPullRequest(this.Instance, enumerationContext, controls);
                Request.MaxElements = maxElements;
                Message resp = searchClient.Pull(Request);
                var pullResponse = MessageToXDocument(resp);
                adObjects.AddRange(ExtractADObjectsFromResponse(pullResponse));
                endOfSequence = pullResponse
                    .Descendants(XName.Get("EndOfSequence", "http://schemas.xmlsoap.org/ws/2004/09/enumeration"))
                    .Count() > 0;
            }
            
            return adObjects;
        }

        public void UpdateCredentials(ClientCredentials c)
        {
            c.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            c.Windows.ClientCredential = this.Credentials;
        }

        static XDocument MessageToXDocument(Message message)
        {
            return XDocument.Parse(ReplaceHexadecimalSymbols(message.ToString()));
        }

        static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        private static List<ADObject> ExtractADObjectsFromResponse(XDocument pullResponse)
        {
            XNamespace addata = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data";
            XNamespace ad = "http://schemas.microsoft.com/2008/1/ActiveDirectory";
            XNamespace wsen = "http://schemas.xmlsoap.org/ws/2004/09/enumeration";

            var entries = new List<Dictionary<string, string>>();
            var arrayKeys = new List<string> { "member", "msDS-AllowedToDelegateTo", "msDS-KeyCredentialLink", "pKIExtendedKeyUsage", "servicePrincipalName", "certificateTemplates", "cACertificate", "sIDHistory" };

            var adObjects = new List<ADObject> { };

            foreach (var element in pullResponse.Descendants(wsen + "Items").Elements())
            {
                var adobject = new ADObject();
                adobject.Class = element.Name.LocalName.ToLowerInvariant();
                foreach (var property in element.Elements())
                {
                    var propertyName = property.Name.LocalName;
                    var propertyValue = property.Element(ad + "value").Value;
                    string[] propertyValues = null;
                    if (arrayKeys.Contains(propertyName))
                    {
                        propertyValues = property.Elements(ad + "value").Select(v => v.Value).ToArray();
                    }
                    switch (propertyName)
                    {
                        case "class":
                            adobject.Class = propertyValue;
                            break;
                        case "adminCount":
                            adobject.AdminCount = int.Parse(propertyValue);
                            break;
                        case "cACertificate":
                            adobject.CACertificate = ParseX509Certificate2Collection(propertyValues);
                            break;
                        case "certificateTemplates":
                            adobject.CertificateTemplates = propertyValues;
                            break;
                        case "description":
                            adobject.Description = propertyValue;
                            break;
                        case "displayName":
                            adobject.DisplayName = propertyValue;
                            break;
                        case "distinguishedName":
                            adobject.DistinguishedName = propertyValue;
                            break;
                        case "dNSHostName":
                            adobject.DNSHostName = propertyValue;
                            break;
                        case "cn":
                            adobject.Cn = propertyValue;
                            break;
                        case "dnsRecord":
                            adobject.DnsRecord = Convert.FromBase64String(propertyValue);
                            break;
                        case "ms-DS-MachineAccountQuota":
                            adobject.DSMachineAccountQuota = int.Parse(propertyValue);
                            break;
                        case "gPCFileSysPath":
                            adobject.GPCFileSysPath = propertyValue;
                            break;
                        case "isDeleted":
                            adobject.IsDeleted = propertyValue;
                            break;
                        case "gPLink":
                            adobject.GPLink = propertyValue;
                            break;
                        case "gPOptions":
                            adobject.GPOptions = int.Parse(propertyValue);
                            break;
                        case "lastLogon":
                            adobject.LastLogon = DateTime.FromFileTime(long.Parse(propertyValue));
                            break;
                        case "lastLogonTimestamp":
                            adobject.LastLogonTimestamp = DateTime.FromFileTime(long.Parse(propertyValue));
                            break;
                        case "member":
                            adobject.Member = propertyValues;
                            break;
                        case "msDS-AllowedToActOnBehalfOfOtherIdentity":
                            adobject.MsDSAllowedToActOnBehalfOfOtherIdentity = ParseActiveDirectorySecurity(propertyValue);
                            break;
                        case "msDS-AllowedToDelegateTo":
                            adobject.MsDSAllowedToDelegateTo = propertyValues;
                            break;
                        case "msDS-KeyCredentialLink":
                            adobject.MsDSKeyCredentialLink = ParseKeyCredential(propertyValues);
                            break;
                        case "msDS-Behavior-Version":
                            adobject.FunctionalLevel = int.Parse(propertyValue);
                            break;
                        case "ms-Mcs-AdmPwdExpirationTime":
                            adobject.MsMCSAdmPwdExpirationTime = long.Parse(propertyValue);
                            break;
                        case "msPKI-Certificate-Name-Flag":
                            adobject.MsPKICertificateNameFlag = int.Parse(propertyValue);
                            break;
                        case "msPKI-Minimal-Key-Size":
                            adobject.MsPKIMinimalKeySize = int.Parse(propertyValue);
                            break;
                        case "msPKI-Enrollment-Flag":
                            adobject.MsPKIEnrollmentFlag = int.Parse(propertyValue);
                            break;
                        case "msPKI-Private-Key-Flag":
                            adobject.MsPKIPrivateKeyFlag = int.Parse(propertyValue);
                            break;
                        case "name":
                            adobject.Name = propertyValue;
                            break;
                        case "nTSecurityDescriptor":
                            adobject.NTSecurityDescriptor = ParseActiveDirectorySecurity(propertyValue);
                            break;
                        case "objectGUID":
                            adobject.ObjectGUID = new Guid(Convert.FromBase64String(propertyValue));
                            break;
                        case "objectSid":
                            adobject.ObjectSid = new SecurityIdentifier(Convert.FromBase64String(propertyValue), 0);
                            break;
                        case "operatingSystem":
                            adobject.OperatingSystem = propertyValue;
                            break;
                        case "pKIExtendedKeyUsage":
                            adobject.PKIExtendedKeyUsage = propertyValues;
                            break;
                        case "primaryGroupID":
                            adobject.PrimaryGroupID = int.Parse(propertyValue);
                            break;
                        case "pwdLastSet":
                            adobject.PwdLastSet = DateTime.FromFileTime(long.Parse(propertyValue));
                            break;
                        case "sAMAccountName":
                            adobject.SAMAccountName = propertyValue;
                            break;
                        case "scriptPath":
                            adobject.ScriptPath = propertyValue;
                            break;
                        case "securityIdentifier":
                            adobject.SecurityIdentifier = new SecurityIdentifier(Convert.FromBase64String(propertyValue), 0);
                            break;
                        case "servicePrincipalName":
                            adobject.ServicePrincipalName = propertyValues;
                            break;
                        case "sIDHistory":
                            adobject.SIDHistory = ParseSecurityIdentifierList(propertyValues);
                            break;
                        case "trustAttributes":
                            adobject.TrustAttributes = int.Parse(propertyValue);
                            break;
                        case "trustDirection":
                            adobject.TrustDirection = int.Parse(propertyValue);
                            break;
                        case "userAccountControl":
                            adobject.UserAccountControl = int.Parse(propertyValue);
                            break;
                        case "whenCreated":
                            adobject.WhenCreated = DateTime.ParseExact(propertyValue, "yyyyMMddHHmmss.f'Z'", CultureInfo.InvariantCulture);
                            break;
                        case "mail":
                            adobject.Email = propertyValue;
                            break;
                        case "title":
                            adobject.Title = propertyValue;
                            break;
                        case "homeDirectory":
                            adobject.HomeDirectory = propertyValue;
                            break;
                        case "userPassword":
                            adobject.UserPassword = propertyValue;
                            break;
                        case "unixUserPassword":
                            adobject.UnixUserPassword = propertyValue;
                            break;
                        case "unicodePassword":
                            adobject.UnicodePassword = propertyValue;
                            break;
                        case "msSFU30Password":
                            adobject.MsSFU30Password = propertyValue;
                            break;
                        case "pKIExpirationPeriod":
                            adobject.PKIExpirationPeriod = Convert.FromBase64String(propertyValue);
                            break;
                        case "pKIOverlapPeriod":
                            adobject.PKIOverlapPeriod = Convert.FromBase64String(propertyValue);
                            break;
                        default:
                            break;
                    }
                }
                adObjects.Add(adobject);
            }
            return adObjects;
        }

        private static SecurityIdentifier[] ParseSecurityIdentifierList(string[] propertyValues)
        {
            List<SecurityIdentifier> collection = new List<SecurityIdentifier>();
            if (propertyValues == null)
            {
                return collection.ToArray();
            }
            foreach (var propertyValue in propertyValues)
            {
                try
                {
                    byte[] data = Convert.FromBase64String(propertyValue);
                    collection.Add(new SecurityIdentifier(data, 0));
                }
                catch (Exception)
                {

                }
            }
            return collection.ToArray();
        }

        private static ActiveDirectorySecurity ParseActiveDirectorySecurity(string value)
        {
            byte[] data = Convert.FromBase64String(value);
            ActiveDirectorySecurity sd = new ActiveDirectorySecurity();
            sd.SetSecurityDescriptorBinaryForm(data);
            return sd;
        }

        private static X509Certificate2Collection ParseX509Certificate2Collection(string[] propertyValues)
        {
            X509Certificate2Collection collection = new X509Certificate2Collection();
            if (propertyValues == null)
            {
                return collection;
            }
            foreach (var propertyValue in propertyValues)
            {
                try
                {
                    byte[] data = Convert.FromBase64String(propertyValue);
                    collection.Add(new X509Certificate2(data));
                }
                catch (Exception)
                {

                }
            }
            return collection;
        }

        private static KeyCredential[] ParseKeyCredential(string[] propertyValues)
        {
            KeyCredential[] keyCredentialList = new KeyCredential[propertyValues.Length];
            if (propertyValues == null) 
            {
                return keyCredentialList;
            }
            for (var i = 0; i < propertyValues.Length; i++)
            {
                try
                {
                    keyCredentialList[i] = KeyCredential.ParseDNBinary(propertyValues[i]);
                }
                catch (Exception)
                {

                }
            }
            return keyCredentialList;
        }
    }
}
