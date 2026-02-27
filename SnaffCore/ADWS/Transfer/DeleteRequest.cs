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
    internal class DeleteRequest
    {
        string Instance { get; set; }
        string BaseUri { get; set; }
        NetworkCredential Credentials { get; set; }

        NetTcpBinding Binding { get; set; }
        MessageVersion Version { get; set; }

        public DeleteRequest(ADWSConnection adwsConnection)
        {
            this.Instance = adwsConnection.Instance;
            this.BaseUri = adwsConnection.BaseUri;
            this.Binding = adwsConnection.Binding;
            this.Credentials = adwsConnection.Credentials;
        }

        public Message DeleteRequestMessage(string distinguishedName)
        {
            var endpointAddress = new System.ServiceModel.EndpointAddress(this.BaseUri + "ActiveDirectoryWebServices/Windows/Resource");
            var resourceClient = new Microsoft.ActiveDirectory.WebServices.Proxy.ResourceClient(this.Binding, endpointAddress);
            UpdateCredentials(resourceClient.ClientCredentials);

            DirectoryControl[] controls = new DirectoryControl[0];

            ADDeleteRequestMsg Request = new ADDeleteRequestMsg(this.Instance, distinguishedName, controls);

            Message addResponse = resourceClient.Delete(Request);
            return addResponse;
        }

        public void UpdateCredentials(ClientCredentials c)
        {
            c.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            c.Windows.ClientCredential = this.Credentials;
        }
    }
}
