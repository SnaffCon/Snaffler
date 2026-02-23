using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
    // Token: 0x02000306 RID: 774
    internal class ADCreateRequestMsg : AdimdaRequestMsg
    {
        // Token: 0x06001AB5 RID: 6837 RVA: 0x00052B5C File Offset: 0x00050D5C
        public ADCreateRequestMsg(string instance, string parent, string relativeDistinguishedName, IList<DirectoryControl> controls)
            : base(instance, null)
        {
            this._relativeDistinguishedName = relativeDistinguishedName;
            this._parentContainer = parent;
            this._controls = controls;
        }

        // Token: 0x06001AB6 RID: 6838 RVA: 0x00052B7C File Offset: 0x00050D7C
        public ADCreateRequestMsg(string instance, string parent, string relativeDistinguishedName, IList<DirectoryControl> controls, IList<DirectoryAttribute> attributes)
            : this(instance, parent, relativeDistinguishedName, controls)
        {
            this._attributes = attributes;
        }

        // Token: 0x1700090E RID: 2318
        // (get) Token: 0x06001AB7 RID: 6839 RVA: 0x00052B91 File Offset: 0x00050D91
        public override string Action
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/09/transfer/Create";
            }
        }

        // Token: 0x06001AB8 RID: 6840 RVA: 0x00052B98 File Offset: 0x00050D98
        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("AddRequest", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
            writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
            if (this._attributes != null)
            {
                foreach (DirectoryAttribute directoryAttribute in this._attributes)
                {
                    AttributeTypeAndValueSerializer.Serialize(writer, directoryAttribute);
                }
            }
            AttributeTypeAndValueSerializer.Serialize(writer, "http://schemas.microsoft.com/2008/1/ActiveDirectory", "relativeDistinguishedName", this._relativeDistinguishedName);
            AttributeTypeAndValueSerializer.Serialize(writer, "http://schemas.microsoft.com/2008/1/ActiveDirectory", "container-hierarchy-parent", this._parentContainer);
            if (this._controls != null)
            {
                DirectoryControlSerializer.Serialize(writer, this._controls);
            }
            writer.WriteEndElement();
        }

        // Token: 0x04000C74 RID: 3188
        private IList<DirectoryControl> _controls;

        // Token: 0x04000C75 RID: 3189
        private IList<DirectoryAttribute> _attributes;

        // Token: 0x04000C76 RID: 3190
        private string _relativeDistinguishedName;

        // Token: 0x04000C77 RID: 3191
        private string _parentContainer;
    }
}
