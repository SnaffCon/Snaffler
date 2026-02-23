using System;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	// Token: 0x02000310 RID: 784
	internal class ADGetStatusRequest : AdwsRequestMsg
	{
		// Token: 0x06001AFE RID: 6910 RVA: 0x00053454 File Offset: 0x00051654
		public ADGetStatusRequest(string instance, string enumerationContext)
			: base(instance)
		{
			this._enumerationContext = enumerationContext;
		}

		// Token: 0x17000927 RID: 2343
		// (get) Token: 0x06001AFF RID: 6911 RVA: 0x00053464 File Offset: 0x00051664
		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/GetStatus";
			}
		}

		// Token: 0x06001B00 RID: 6912 RVA: 0x0005346B File Offset: 0x0005166B
		protected override void OnWriteStartBody(XmlDictionaryWriter writer)
		{
			base.OnWriteStartBody(writer);
			writer.WriteXmlnsAttribute("wsen", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
		}

		// Token: 0x06001B01 RID: 6913 RVA: 0x00053484 File Offset: 0x00051684
		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("GetStatus", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			XmlUtility.SerializeEnumerationContext(writer, this._enumerationContext);
			writer.WriteEndElement();
		}

		// Token: 0x04000C8E RID: 3214
		private string _enumerationContext;
	}
}
