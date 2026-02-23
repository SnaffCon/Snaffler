using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000040 RID: 64
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	[Serializable]
	public class ChangeType : AttributeTypeAndValue
	{
		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600014C RID: 332 RVA: 0x00002EFB File Offset: 0x000010FB
		// (set) Token: 0x0600014D RID: 333 RVA: 0x00002F03 File Offset: 0x00001103
		[XmlAttribute]
		public string Operation
		{
			get
			{
				return this.operationField;
			}
			set
			{
				this.operationField = value;
			}
		}

		// Token: 0x040001EC RID: 492
		private string operationField;
	}
}
