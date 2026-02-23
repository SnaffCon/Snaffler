using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x0200004A RID: 74
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[XmlRoot(Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[Serializable]
	public class SupportedSelectOrSortDialect
	{
		// Token: 0x1700007D RID: 125
		// (get) Token: 0x0600017A RID: 378 RVA: 0x0000307D File Offset: 0x0000127D
		// (set) Token: 0x0600017B RID: 379 RVA: 0x00003085 File Offset: 0x00001285
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

		// Token: 0x040001FE RID: 510
		private string valueField;
	}
}
