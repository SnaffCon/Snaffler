using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000048 RID: 72
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	[Serializable]
	public class AttributeTypeNotValidForDialect
	{
		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000174 RID: 372 RVA: 0x0000304B File Offset: 0x0000124B
		// (set) Token: 0x06000175 RID: 373 RVA: 0x00003053 File Offset: 0x00001253
		[XmlElement(Order = 0)]
		public string AttributeType
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

		// Token: 0x040001FC RID: 508
		private string attributeTypeField;
	}
}
