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

namespace SnaffCore.ADWS.Transfer
{
    internal class GetRequest
    {
        string Instance { get; set; }
        string BaseUri { get; set; }
        NetworkCredential Credentials { get; set; }

        NetTcpBinding Binding { get; set; }
        MessageVersion Version { get; set; }

        public GetRequest(ADWSConnection adwsConnection)
        {
            this.Instance = adwsConnection.Instance;
            this.BaseUri = adwsConnection.BaseUri;
            this.Binding = adwsConnection.Binding;
            this.Credentials = adwsConnection.Credentials;
        }

        public List<ADObject> BaseObjectSearchRequest(string distinguishedName, IList<string> attributeList)
        {
            var endpointAddress = new System.ServiceModel.EndpointAddress(this.BaseUri + "ActiveDirectoryWebServices/Windows/Resource");
            var resourceClient = new Microsoft.ActiveDirectory.WebServices.Proxy.ResourceClient(this.Binding, endpointAddress);
            UpdateCredentials(resourceClient.ClientCredentials);

            DirectoryControl[] controls = new DirectoryControl[2];
            controls[0] = new PageResultRequestControl();
            controls[1] = new SecurityDescriptorFlagControl(System.DirectoryServices.Protocols.SecurityMasks.Dacl);

            ADGetRequestMsg Request = new ADGetRequestMsg(this.Instance, distinguishedName, controls, attributeList);

            Message resp = resourceClient.Get(Request);
            Console.WriteLine(resp.ToString());
            var adObjects = new List<ADObject>();

            return adObjects;
        }

        public void UpdateCredentials(ClientCredentials c)
        {
            c.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            c.Windows.ClientCredential = this.Credentials;
        }
    }
}
