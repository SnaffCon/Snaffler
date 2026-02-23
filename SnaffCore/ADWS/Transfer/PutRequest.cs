using Microsoft.ActiveDirectory.Management.IMDA;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace SnaffCore.ADWS.Transfer
{
    internal class PutRequest
    {
        string Instance { get; set; }
        string BaseUri { get; set; }
        NetworkCredential Credentials { get; set; }

        NetTcpBinding Binding { get; set; }
        MessageVersion Version { get; set; }

        public PutRequest(ADWSConnection adwsConnection)
        {
            this.Instance = adwsConnection.Instance;
            this.BaseUri = adwsConnection.BaseUri;
            this.Binding = adwsConnection.Binding;
            this.Credentials = adwsConnection.Credentials;
        }

        public Message ModifyRequest(string distinguishedName, DirectoryAttributeOperation operation, string attribute, string attributeValue)
        {
            var endpointAddress = new System.ServiceModel.EndpointAddress(this.BaseUri + "ActiveDirectoryWebServices/Windows/Resource");
            var resourceClient = new Microsoft.ActiveDirectory.WebServices.Proxy.ResourceClient(this.Binding, endpointAddress);
            UpdateCredentials(resourceClient.ClientCredentials);

            DirectoryControl[] controls = new DirectoryControl[2];
            controls[0] = new PermissiveModifyControl();
            controls[1] = new SecurityDescriptorFlagControl();

            DirectoryAttributeModification[] attributeModifications = new DirectoryAttributeModification[1];
            attributeModifications[0] = new DirectoryAttributeModification();

            if (operation == DirectoryAttributeOperation.Replace)
            {
                attributeModifications[0].Name = attribute;
                attributeModifications[0].Operation = DirectoryAttributeOperation.Replace;
                attributeModifications[0].Add(attributeValue);
            }
            if (operation == DirectoryAttributeOperation.Add)
            {
                attributeModifications[0].Name = attribute;
                attributeModifications[0].Operation = DirectoryAttributeOperation.Add;
                attributeModifications[0].Add(attributeValue);
            }
            if (operation == DirectoryAttributeOperation.Delete)
            {
                attributeModifications[0].Name = attribute;
                attributeModifications[0].Operation = DirectoryAttributeOperation.Delete;
                attributeModifications[0].Add(attributeValue);
            }

            ADPutRequestMsg Request = new ADPutRequestMsg(this.Instance, distinguishedName, controls, attributeModifications);

            Message modifyResponse = resourceClient.Put(Request);

            return modifyResponse;
        }

        public Message ModifyRequest(string distinguishedName, DirectoryAttributeOperation operation, string attribute, byte[] attributeValue)
        {
            var endpointAddress = new System.ServiceModel.EndpointAddress(this.BaseUri + "ActiveDirectoryWebServices/Windows/Resource");
            var resourceClient = new Microsoft.ActiveDirectory.WebServices.Proxy.ResourceClient(this.Binding, endpointAddress);
            UpdateCredentials(resourceClient.ClientCredentials);

            DirectoryControl[] controls = new DirectoryControl[2];
            controls[0] = new PermissiveModifyControl();
            controls[1] = new SecurityDescriptorFlagControl(System.DirectoryServices.Protocols.SecurityMasks.Dacl);

            DirectoryAttributeModification[] attributeModifications = new DirectoryAttributeModification[1];
            attributeModifications[0] = new DirectoryAttributeModification();

            if (operation == DirectoryAttributeOperation.Replace)
            {
                attributeModifications[0].Name = attribute;
                attributeModifications[0].Operation = DirectoryAttributeOperation.Replace;
                attributeModifications[0].Add(attributeValue);
            }
            if (operation == DirectoryAttributeOperation.Add)
            {
                attributeModifications[0].Name = attribute;
                attributeModifications[0].Operation = DirectoryAttributeOperation.Add;
                attributeModifications[0].Add(attributeValue);
            }
            if (operation == DirectoryAttributeOperation.Delete)
            {
                attributeModifications[0].Name = attribute;
                attributeModifications[0].Operation = DirectoryAttributeOperation.Delete;
                attributeModifications[0].Add(attributeValue);
            }

            ADPutRequestMsg Request = new ADPutRequestMsg(this.Instance, distinguishedName, controls, attributeModifications);

            Message modifyResponse = resourceClient.Put(Request);

            return modifyResponse;
        }

        public void UpdateCredentials(ClientCredentials c)
        {
            c.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            c.Windows.ClientCredential = this.Credentials;
        }
    }
}
