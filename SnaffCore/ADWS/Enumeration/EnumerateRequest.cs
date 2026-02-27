using Microsoft.ActiveDirectory.Management.IMDA;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ActiveDirectory.Management.WSE;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace SnaffCore.ADWS.Enumeration
{
    internal class EnumerateRequest
    {
        ADWSConnection adwsConnection = null;
        string Instance { get; set; }
        string BaseUri { get; set; }
        NetworkCredential Credentials { get; set; }

        NetTcpBinding Binding { get; set; }
        MessageVersion Version { get; set; }

        public EnumerateRequest(ADWSConnection adwsConnection)
        {
            this.adwsConnection = adwsConnection;
            this.Instance = adwsConnection.Instance;
            this.BaseUri = adwsConnection.BaseUri;
            this.Binding = adwsConnection.Binding;
            this.Credentials = adwsConnection.Credentials;
        }

        public List<ADObject> Enumerate(string filter, string searchBase, string searchScope, IList<string> attributeList)
        {
            var endpointAddress = new System.ServiceModel.EndpointAddress(this.BaseUri + "ActiveDirectoryWebServices/Windows/Enumeration");
            var searchClient = new Microsoft.ActiveDirectory.WebServices.Proxy.SearchClient(this.Binding, endpointAddress);
            UpdateCredentials(searchClient.ClientCredentials);

            DirectoryControl[] controls = new DirectoryControl[2];
            controls[0] = new PageResultRequestControl();
            controls[1] = new SecurityDescriptorFlagControl(System.DirectoryServices.Protocols.SecurityMasks.Dacl);

            ADEnumerateLdapRequest Request = new ADEnumerateLdapRequest(this.Instance, filter, searchBase, searchScope, attributeList);

            Message resp = searchClient.Enumerate(Request);

            var enumerateResponse = MessageToXDocument(resp);
            string enumerationContext = enumerateResponse
                .Descendants(XName.Get("EnumerationContext", "http://schemas.xmlsoap.org/ws/2004/09/enumeration"))
                .FirstOrDefault()?
                .Value;

            PullRequest pullRequest = new PullRequest(adwsConnection);
            return pullRequest.Pull(searchClient, enumerationContext);

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
    }
}
