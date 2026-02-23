using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x0200003F RID: 63
	[XmlInclude(typeof(ChangeType))]
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	[Serializable]
	public class AttributeTypeAndValue
	{
		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000147 RID: 327 RVA: 0x00002ED1 File Offset: 0x000010D1
		// (set) Token: 0x06000148 RID: 328 RVA: 0x00002ED9 File Offset: 0x000010D9
		[XmlElement(Order = 0)]
		public DataSet AttributeType
		{
			get
			{
				return this.attributeTypeField;
			}
			set
			{
				this.attributeTypeField = value;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000149 RID: 329 RVA: 0x00002EE2 File Offset: 0x000010E2
		// (set) Token: 0x0600014A RID: 330 RVA: 0x00002EEA File Offset: 0x000010EA
		[XmlElement(Order = 1)]
		public DataSet AttributeValue
		{
			get
			{
				return this.attributeValueField;
			}
			set
			{
				this.attributeValueField = value;
			}
		}

		// Token: 0x040001EA RID: 490
		private DataSet attributeTypeField;

		// Token: 0x040001EB RID: 491
		private DataSet attributeValueField;
	}
}
