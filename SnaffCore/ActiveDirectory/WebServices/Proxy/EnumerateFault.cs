using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000044 RID: 68
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[XmlRoot(Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[Serializable]
	public class EnumerateFault : FaultDetail
	{
		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000160 RID: 352 RVA: 0x00002FA3 File Offset: 0x000011A3
		// (set) Token: 0x06000161 RID: 353 RVA: 0x00002FAB File Offset: 0x000011AB
		[XmlElement(Order = 0)]
		public string InvalidProperty
		{
			get
			{
				return this.invalidPropertyField;
			}
			set
			{
				this.invalidPropertyField = value;
			}
		}

		// Token: 0x040001F4 RID: 500
		private string invalidPropertyField;
	}
}
