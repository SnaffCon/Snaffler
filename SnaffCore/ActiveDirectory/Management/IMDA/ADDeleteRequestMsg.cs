using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
    // Token: 0x02000302 RID: 770
    internal class ADDeleteRequestMsg : AdwsRequestMsg
    {
        // Token: 0x06001AA0 RID: 6816 RVA: 0x000529AF File Offset: 0x00050BAF
        public ADDeleteRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls)
            : base(instance, objectReference)
        {
            this._controls = controls;
        }

        // Token: 0x17000905 RID: 2309
        // (get) Token: 0x06001AA1 RID: 6817 RVA: 0x000529C0 File Offset: 0x00050BC0
        public override string Action
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/09/transfer/Delete";
            }
        }

        // Token: 0x06001AA2 RID: 6818 RVA: 0x000529C7 File Offset: 0x00050BC7
        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            base.OnWriteBodyContents(writer);
            if (this._controls != null)
            {
                DirectoryControlSerializer.Serialize(writer, this._controls);
            }
        }

        // Token: 0x04000C6F RID: 3183
        private IList<DirectoryControl> _controls;
    }
}
