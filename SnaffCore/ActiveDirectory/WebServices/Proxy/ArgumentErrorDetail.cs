using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x0200003E RID: 62
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[Serializable]
	public class ArgumentErrorDetail
	{
		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000140 RID: 320 RVA: 0x00002E96 File Offset: 0x00001096
		// (set) Token: 0x06000141 RID: 321 RVA: 0x00002E9E File Offset: 0x0000109E
		[XmlElement(Order = 0)]
		public string Message
		{
			get
			{
				return this.messageField;
			}
			set
			{
				this.messageField = value;
			}
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000142 RID: 322 RVA: 0x00002EA7 File Offset: 0x000010A7
		// (set) Token: 0x06000143 RID: 323 RVA: 0x00002EAF File Offset: 0x000010AF
		[XmlElement(Order = 1)]
		public string ParameterName
		{
			get
			{
				return this.parameterNameField;
			}
			set
			{
				this.parameterNameField = value;
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000144 RID: 324 RVA: 0x00002EB8 File Offset: 0x000010B8
		// (set) Token: 0x06000145 RID: 325 RVA: 0x00002EC0 File Offset: 0x000010C0
		[XmlElement(Order = 2)]
		public string ShortMessage
		{
			get
			{
				return this.shortMessageField;
			}
			set
			{
				this.shortMessageField = value;
			}
		}

		// Token: 0x040001E7 RID: 487
		private string messageField;

		// Token: 0x040001E8 RID: 488
		private string parameterNameField;

		// Token: 0x040001E9 RID: 489
		private string shortMessageField;
	}
}
