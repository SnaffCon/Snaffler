using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000047 RID: 71
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	[XmlRoot(Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	[Serializable]
	public class AttributeTypeNotValid
	{
		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600016F RID: 367 RVA: 0x00003021 File Offset: 0x00001221
		// (set) Token: 0x06000170 RID: 368 RVA: 0x00003029 File Offset: 0x00001229
		[XmlElement(Order = 0)]
		public AttributeTypeNotValidForDialect AttributeTypeNotValidForDialect
		{
			get
			{
				return this.attributeTypeNotValidForDialectField;
			}
			set
			{
				this.attributeTypeNotValidForDialectField = value;
			}
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000171 RID: 369 RVA: 0x00003032 File Offset: 0x00001232
		// (set) Token: 0x06000172 RID: 370 RVA: 0x0000303A File Offset: 0x0000123A
		[XmlElement(Order = 1)]
		public AttributeTypeNotValidForEntry AttributeTypeNotValidForEntry
		{
			get
			{
				return this.attributeTypeNotValidForEntryField;
			}
			set
			{
				this.attributeTypeNotValidForEntryField = value;
			}
		}

		// Token: 0x040001FA RID: 506
		private AttributeTypeNotValidForDialect attributeTypeNotValidForDialectField;

		// Token: 0x040001FB RID: 507
		private AttributeTypeNotValidForEntry attributeTypeNotValidForEntryField;
	}
}
