using System;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	// Token: 0x02000314 RID: 788
	internal class ADReleaseRequest : AdwsRequestMsg
	{
		// Token: 0x06001B1C RID: 6940 RVA: 0x0005380B File Offset: 0x00051A0B
		public ADReleaseRequest(string instance, string enumerationContext)
			: base(instance)
		{
			this._enumerationContext = enumerationContext;
		}

		// Token: 0x17000932 RID: 2354
		// (get) Token: 0x06001B1D RID: 6941 RVA: 0x0005381B File Offset: 0x00051A1B
		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Release";
			}
		}

		// Token: 0x06001B1E RID: 6942 RVA: 0x00053822 File Offset: 0x00051A22
		protected override void OnWriteStartBody(XmlDictionaryWriter writer)
		{
			base.OnWriteStartBody(writer);
			writer.WriteXmlnsAttribute("wsen", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
		}

		// Token: 0x06001B1F RID: 6943 RVA: 0x0005383B File Offset: 0x00051A3B
		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Release", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			XmlUtility.SerializeEnumerationContext(writer, this._enumerationContext);
			writer.WriteEndElement();
		}

		// Token: 0x04000C98 RID: 3224
		private string _enumerationContext;
	}
}
