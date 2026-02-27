using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x0200004B RID: 75
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.xmlsoap.org/ws/2004/09/enumeration")]
	[XmlRoot(Namespace = "http://schemas.xmlsoap.org/ws/2004/09/enumeration")]
	[Serializable]
	public class SupportedDialect
	{
		// Token: 0x1700007E RID: 126
		// (get) Token: 0x0600017D RID: 381 RVA: 0x00003096 File Offset: 0x00001296
		// (set) Token: 0x0600017E RID: 382 RVA: 0x0000309E File Offset: 0x0000129E
		[XmlText(DataType = "anyURI")]
		public string Value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}

		// Token: 0x040001FF RID: 511
		private string valueField;
	}
}
