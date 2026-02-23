using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	// Token: 0x0200030C RID: 780
	internal class ADEnumerateLdapRequest : ADEnumerateRequest
	{
		// Token: 0x06001AD8 RID: 6872 RVA: 0x0005303C File Offset: 0x0005123C
		public ADEnumerateLdapRequest(string instance)
			: base(instance)
		{
		}

		// Token: 0x06001AD9 RID: 6873 RVA: 0x00053045 File Offset: 0x00051245
		public ADEnumerateLdapRequest(string instance, string filter, string searchBase, string searchScope)
			: this(instance)
		{
			this.Filter = filter;
			this.SearchBase = searchBase;
			this.SearchScope = searchScope;
		}

		// Token: 0x06001ADA RID: 6874 RVA: 0x00053064 File Offset: 0x00051264
		public ADEnumerateLdapRequest(string instance, string filter, string searchBase, string searchScope, IList<string> attributes)
			: base(instance, attributes)
		{
			this.Filter = filter;
			this.SearchBase = searchBase;
			this.SearchScope = searchScope;
		}

		// Token: 0x1700091A RID: 2330
		// (get) Token: 0x06001ADB RID: 6875 RVA: 0x00053085 File Offset: 0x00051285
		protected override bool IsFilterPresent
		{
			get
			{
				return this._filter != null;
			}
		}

		// Token: 0x1700091B RID: 2331
		// (get) Token: 0x06001ADD RID: 6877 RVA: 0x00053099 File Offset: 0x00051299
		// (set) Token: 0x06001ADC RID: 6876 RVA: 0x00053090 File Offset: 0x00051290
		private string Filter
		{
			get
			{
				return this._filter;
			}
			set
			{
				this._filter = value;
			}
		}

		// Token: 0x1700091C RID: 2332
		// (get) Token: 0x06001ADF RID: 6879 RVA: 0x000530AA File Offset: 0x000512AA
		// (set) Token: 0x06001ADE RID: 6878 RVA: 0x000530A1 File Offset: 0x000512A1
		private string SearchBase
		{
			get
			{
				return this._searchBase;
			}
			set
			{
				this._searchBase = value;
			}
		}

		// Token: 0x1700091D RID: 2333
		// (get) Token: 0x06001AE1 RID: 6881 RVA: 0x000530BB File Offset: 0x000512BB
		// (set) Token: 0x06001AE0 RID: 6880 RVA: 0x000530B2 File Offset: 0x000512B2
		private string SearchScope
		{
			get
			{
				return this._searchScope;
			}
			set
			{
				this._searchScope = value;
			}
		}

		// Token: 0x06001AE2 RID: 6882 RVA: 0x000530C3 File Offset: 0x000512C3
		protected override void OnWriteStartFilterElement(XmlDictionaryWriter writer)
		{
			base.OnWriteStartFilterElement(writer);
			writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery");
			writer.WriteXmlnsAttribute("adlq", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery");
		}

		// Token: 0x06001AE3 RID: 6883 RVA: 0x000530EC File Offset: 0x000512EC
		protected override void OnWriteFilterElementContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("LdapQuery", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery");
			writer.WriteElementString("Filter", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery", this.Filter);
			writer.WriteElementString("BaseObject", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery", this.SearchBase);
			writer.WriteElementString("Scope", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/LdapQuery", this.SearchScope);
			writer.WriteEndElement();
		}

		// Token: 0x04000C84 RID: 3204
		private string _filter;

		// Token: 0x04000C85 RID: 3205
		private string _searchBase;

		// Token: 0x04000C86 RID: 3206
		private string _searchScope;
	}
}
