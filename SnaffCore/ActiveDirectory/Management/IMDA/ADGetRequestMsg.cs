using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
    // Token: 0x02000308 RID: 776
    internal class ADGetRequestMsg : AdimdaRequestMsg
    {
        // Token: 0x06001AC0 RID: 6848 RVA: 0x00052D3C File Offset: 0x00050F3C
        public ADGetRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, IList<string> attributeList)
            : base(instance, objectReference)
        {
            if (attributeList == null || attributeList.Count == 0)
            {
                throw new ArgumentOutOfRangeException("attributeList");
            }
            this._controls = controls;
            this._attributeList = attributeList;
        }

        // Token: 0x17000912 RID: 2322
        // (get) Token: 0x06001AC1 RID: 6849 RVA: 0x00052D6D File Offset: 0x00050F6D
        public override string Action
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get";
            }
        }

        // Token: 0x06001AC2 RID: 6850 RVA: 0x00052D74 File Offset: 0x00050F74
        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("BaseObjectSearchRequest", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
            writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
            base.OnWriteBodyContents(writer);
            string text = writer.LookupPrefix("http://schemas.microsoft.com/2008/1/ActiveDirectory/Data");
            string text2 = writer.LookupPrefix("http://schemas.microsoft.com/2008/1/ActiveDirectory");
            if (this._attributeList != null)
            {
                XmlUtility.SerializeAttributeList(writer, "AttributeType", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess", text2, text, this._attributeList);
            }
            if (this._controls != null)
            {
                DirectoryControlSerializer.Serialize(writer, this._controls);
            }
            writer.WriteEndElement();
        }

        // Token: 0x04000C7B RID: 3195
        private IList<DirectoryControl> _controls;

        // Token: 0x04000C7C RID: 3196
        private IList<string> _attributeList;
    }
}
