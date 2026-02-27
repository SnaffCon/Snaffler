using System;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	// Token: 0x02000316 RID: 790
	internal class ADRenewRequest : AdwsRequestMsg
	{
		// Token: 0x06001B25 RID: 6949 RVA: 0x00053882 File Offset: 0x00051A82
		public ADRenewRequest(string instance, string enumerationContext)
			: base(instance)
		{
			this._enumerationContext = enumerationContext;
		}

		// Token: 0x17000935 RID: 2357
		// (get) Token: 0x06001B26 RID: 6950 RVA: 0x00053892 File Offset: 0x00051A92
		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Renew";
			}
		}

		// Token: 0x06001B27 RID: 6951 RVA: 0x00053899 File Offset: 0x00051A99
		protected override void OnWriteStartBody(XmlDictionaryWriter writer)
		{
			base.OnWriteStartBody(writer);
			writer.WriteXmlnsAttribute("wsen", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
		}

		// Token: 0x06001B28 RID: 6952 RVA: 0x000538B2 File Offset: 0x00051AB2
		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Renew", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			XmlUtility.SerializeEnumerationContext(writer, this._enumerationContext);
			XmlUtility.SerializeExpires(writer, this._expirationDateTime, this._expirationTimeSpan);
			writer.WriteEndElement();
		}

		// Token: 0x06001B29 RID: 6953 RVA: 0x000538E8 File Offset: 0x00051AE8
		public virtual void SetContextExpiration(DateTime expiration)
		{
			this._expirationDateTime = new DateTime?(expiration);
			this._expirationTimeSpan = null;
		}

		// Token: 0x06001B2A RID: 6954 RVA: 0x00053902 File Offset: 0x00051B02
		public virtual void SetContextExpiration(TimeSpan expiration)
		{
			this._expirationTimeSpan = new TimeSpan?(expiration);
			this._expirationDateTime = null;
		}

		// Token: 0x04000C99 RID: 3225
		private string _enumerationContext;

		// Token: 0x04000C9A RID: 3226
		private DateTime? _expirationDateTime;

		// Token: 0x04000C9B RID: 3227
		private TimeSpan? _expirationTimeSpan;
	}
}
