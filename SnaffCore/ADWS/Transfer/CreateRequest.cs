using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ActiveDirectory.Management.IMDA;
using System.DirectoryServices.Protocols;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SnaffCore.ADWS.Transfer
{
    internal class CreateRequest
    {
        string Instance { get; set; }
        string BaseUri { get; set; }
        NetworkCredential Credentials { get; set; }

        NetTcpBinding Binding { get; set; }
        MessageVersion Version { get; set; }

        public CreateRequest(ADWSConnection adwsConnection)
        {
            this.Instance = adwsConnection.Instance;
            this.BaseUri = adwsConnection.BaseUri;
            this.Binding = adwsConnection.Binding;
            this.Credentials = adwsConnection.Credentials;
        }

        public Message AddRequest(string parent, string relativeDistinguishedName, IList<DirectoryAttribute> directoryAttribute)
        {
            var endpointAddress = new System.ServiceModel.EndpointAddress(this.BaseUri + "ActiveDirectoryWebServices/Windows/ResourceFactory");
            var resourceFactoryClient = new Microsoft.ActiveDirectory.WebServices.Proxy.ResourceFactoryClient(this.Binding, endpointAddress);
            UpdateCredentials(resourceFactoryClient.ClientCredentials);

            DirectoryControl[] controls = new DirectoryControl[1];
            controls[0] = new SecurityDescriptorFlagControl();

            ADCreateRequestMsg Request = new ADCreateRequestMsg(this.Instance, parent, relativeDistinguishedName, controls, directoryAttribute);

            Message addResponse = resourceFactoryClient.Create(Request);
            return addResponse;
        }

        public void UpdateCredentials(ClientCredentials c)
        {
            c.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            c.Windows.ClientCredential = this.Credentials;
        }
    }
}
