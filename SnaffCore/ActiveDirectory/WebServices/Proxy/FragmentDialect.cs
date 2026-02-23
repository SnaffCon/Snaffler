using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000046 RID: 70
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
	[XmlRoot(Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
	[Serializable]
	public class FragmentDialect
	{
		// Token: 0x17000078 RID: 120
		// (get) Token: 0x0600016C RID: 364 RVA: 0x00003008 File Offset: 0x00001208
		// (set) Token: 0x0600016D RID: 365 RVA: 0x00003010 File Offset: 0x00001210
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

		// Token: 0x040001F9 RID: 505
		private string valueField;
	}
}
