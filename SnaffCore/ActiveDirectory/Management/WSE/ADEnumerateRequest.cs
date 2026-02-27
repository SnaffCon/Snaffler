using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	// Token: 0x0200030E RID: 782
	internal abstract class ADEnumerateRequest : AdwsRequestMsg
	{
		// Token: 0x06001AE7 RID: 6887 RVA: 0x0005316A File Offset: 0x0005136A
		protected ADEnumerateRequest(string instance)
			: base(instance)
		{
		}

		// Token: 0x06001AE8 RID: 6888 RVA: 0x00053173 File Offset: 0x00051373
		protected ADEnumerateRequest(string instance, IList<string> attributes)
			: this(instance)
		{
			this._attributes = attributes;
		}

		// Token: 0x1700091E RID: 2334
		// (get) Token: 0x06001AE9 RID: 6889
		protected abstract bool IsFilterPresent { get; }

		// Token: 0x1700091F RID: 2335
		// (get) Token: 0x06001AEA RID: 6890 RVA: 0x00053183 File Offset: 0x00051383
		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Enumerate";
			}
		}

		// Token: 0x17000920 RID: 2336
		// (get) Token: 0x06001AEB RID: 6891 RVA: 0x0005318A File Offset: 0x0005138A
		public virtual IList<string> Attributes
		{
			get
			{
				if (this._attributes == null)
				{
					this._attributes = new List<string>(0);
				}
				return this._attributes;
			}
		}

		// Token: 0x17000921 RID: 2337
		// (get) Token: 0x06001AED RID: 6893 RVA: 0x000531AF File Offset: 0x000513AF
		// (set) Token: 0x06001AEC RID: 6892 RVA: 0x000531A6 File Offset: 0x000513A6
		public virtual SortKey SortKey
		{
			get
			{
				return this._sortKey;
			}
			set
			{
				this._sortKey = value;
			}
		}

		// Token: 0x17000922 RID: 2338
		// (get) Token: 0x06001AEF RID: 6895 RVA: 0x000531C0 File Offset: 0x000513C0
		// (set) Token: 0x06001AEE RID: 6894 RVA: 0x000531B7 File Offset: 0x000513B7
		protected DateTime? ExpirationDateTime
		{
			get
			{
				return this._expirationDateTime;
			}
			set
			{
				this._expirationDateTime = value;
			}
		}

		// Token: 0x17000923 RID: 2339
		// (get) Token: 0x06001AF1 RID: 6897 RVA: 0x000531D1 File Offset: 0x000513D1
		// (set) Token: 0x06001AF0 RID: 6896 RVA: 0x000531C8 File Offset: 0x000513C8
		protected TimeSpan? ExpirationTimeSpan
		{
			get
			{
				return this._expirationTimeSpan;
			}
			set
			{
				this._expirationTimeSpan = value;
			}
		}

		// Token: 0x06001AF2 RID: 6898 RVA: 0x000531DC File Offset: 0x000513DC
		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Enumerate", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			XmlUtility.SerializeExpires(writer, this._expirationDateTime, this._expirationTimeSpan);
			if (this.IsFilterPresent)
			{
				this.OnWriteStartFilterElement(writer);
				this.OnWriteFilterElementContents(writer);
				writer.WriteEndElement();
			}
			if (this._attributes != null && this._attributes.Count > 0)
			{
				writer.WriteStartElement("Selection", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
				XmlUtility.SerializeAttributeList(writer, "SelectionProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory", "ad", "addata", this._attributes);
				writer.WriteEndElement();
			}
			if (this._sortKey != null)
			{
				writer.WriteStartElement("Sorting", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
				writer.WriteStartElement("SortingProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				if (this._sortKey.ReverseOrder)
				{
					writer.WriteAttributeString("Ascending", "false");
				}
				string text = (AttributeNs.IsSynthetic(this._sortKey.AttributeName, SyntheticAttributeOperation.Read) ? "ad" : "addata");
				writer.WriteValue(XmlUtility.AddPrefix(text, this._sortKey.AttributeName));
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		// Token: 0x06001AF3 RID: 6899
		protected abstract void OnWriteFilterElementContents(XmlDictionaryWriter writer);

		// Token: 0x06001AF4 RID: 6900 RVA: 0x00053324 File Offset: 0x00051524
		protected virtual void OnWriteStartFilterElement(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Filter", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
		}

		// Token: 0x06001AF5 RID: 6901 RVA: 0x00053336 File Offset: 0x00051536
		protected override void OnWriteStartBody(XmlDictionaryWriter writer)
		{
			base.OnWriteStartBody(writer);
			writer.WriteXmlnsAttribute("wsen", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			writer.WriteXmlnsAttribute("adlq", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery");
		}

		// Token: 0x06001AF6 RID: 6902 RVA: 0x0005335F File Offset: 0x0005155F
		public virtual void SetContextExpiration(DateTime expiration)
		{
			this._expirationDateTime = new DateTime?(expiration);
			this._expirationTimeSpan = null;
		}

		// Token: 0x06001AF7 RID: 6903 RVA: 0x00053379 File Offset: 0x00051579
		public virtual void SetContextExpiration(TimeSpan expiration)
		{
			this._expirationTimeSpan = new TimeSpan?(expiration);
			this._expirationDateTime = null;
		}

		// Token: 0x04000C87 RID: 3207
		private TimeSpan? _expirationTimeSpan;

		// Token: 0x04000C88 RID: 3208
		private DateTime? _expirationDateTime;

		// Token: 0x04000C89 RID: 3209
		private IList<string> _attributes;

		// Token: 0x04000C8A RID: 3210
		private SortKey _sortKey;
	}
}
