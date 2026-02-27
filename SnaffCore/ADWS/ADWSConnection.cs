using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.ServiceModel.Description;
using System.Globalization;
using System.Security.Principal;
using System.DirectoryServices;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using SnaffCore.ADWS;

namespace SnaffCore.ADWS
{
    internal class ADWSConnection
    {
        public string BaseUri { get; set; }
        public string Instance { get; set; }
        public string DomainName { get; set; }
        public string DefaultNamingContext { get; set; }
        public NetworkCredential Credentials { get; set; }

        public NetTcpBinding Binding { get; set; }
        public MessageVersion Version { get; set; }
        public ADWSConnection(string domainName, string instance, NetworkCredential credentials)
        {
            this.DomainName = domainName;
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "net.tcp";
            uriBuilder.Host = domainName;
            uriBuilder.Port = 9389;
            this.BaseUri = uriBuilder.ToString();

            this.Instance = instance;

            this.Binding = new NetTcpBinding();

            this.Binding.OpenTimeout = new TimeSpan(0, 10, 0);
            this.Binding.CloseTimeout = new TimeSpan(0, 10, 0);
            this.Binding.SendTimeout = new TimeSpan(0, 10, 0);
            this.Binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            this.Binding.MaxBufferSize = 1073741824;
            this.Binding.MaxReceivedMessageSize = 1073741824;
            this.Binding.ReaderQuotas.MaxDepth = 64;
            this.Binding.ReaderQuotas.MaxArrayLength = 2147483647;
            this.Binding.ReaderQuotas.MaxStringContentLength = 2147483647;
            this.Binding.ReaderQuotas.MaxNameTableCharCount = 2147483647;
            this.Binding.ReaderQuotas.MaxBytesPerRead = 2147483647;
            EnvelopeVersion envelopeVersion = EnvelopeVersion.Soap12;
            AddressingVersion addressingVersion = AddressingVersion.WSAddressing10;
            this.Version = MessageVersion.CreateVersion(envelopeVersion, addressingVersion);
            this.Credentials = credentials;

            foreach (String DC in domainName.Split('.'))
            {
                this.DefaultNamingContext += ",DC=" + DC;
            }
            this.DefaultNamingContext = this.DefaultNamingContext.TrimStart(',');
        }

        
    }
}
