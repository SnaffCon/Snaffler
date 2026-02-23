using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000049 RID: 73
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	[Serializable]
	public class AttributeTypeNotValidForEntry
	{
		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000177 RID: 375 RVA: 0x00003064 File Offset: 0x00001264
		// (set) Token: 0x06000178 RID: 376 RVA: 0x0000306C File Offset: 0x0000126C
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

		// Token: 0x040001FD RID: 509
		private string attributeTypeField;
	}
}
