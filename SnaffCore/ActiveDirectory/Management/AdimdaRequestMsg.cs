using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x020000BA RID: 186
	internal abstract class AdimdaRequestMsg : AdwsRequestMsg
	{
		// Token: 0x06000506 RID: 1286 RVA: 0x000124A1 File Offset: 0x000106A1
		protected AdimdaRequestMsg(string instance)
			: base(instance)
		{
			this.AddHeaders();
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x000124B0 File Offset: 0x000106B0
		protected AdimdaRequestMsg(string instance, string objectReferenceProperty)
			: base(instance, objectReferenceProperty)
		{
			this.AddHeaders();
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x000124C0 File Offset: 0x000106C0
		private void AddHeaders()
		{
			this.Headers.Add(MessageHeader.CreateHeader("IdentityManagementOperation", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess", new object(), true));
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x000124E2 File Offset: 0x000106E2
		protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
		{
			base.OnWriteStartEnvelope(writer);
			writer.WriteXmlnsAttribute("da", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
		}
	}
}
