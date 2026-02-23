using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000045 RID: 69
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(TypeName = "FaultDetail", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
	[XmlRoot(Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
	[Serializable]
	public class FaultDetail1
	{
		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000163 RID: 355 RVA: 0x00002FBC File Offset: 0x000011BC
		// (set) Token: 0x06000164 RID: 356 RVA: 0x00002FC4 File Offset: 0x000011C4
		[XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
		public int SizeLimit
		{
			get
			{
				return this.sizeLimitField;
			}
			set
			{
				this.sizeLimitField = value;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000165 RID: 357 RVA: 0x00002FCD File Offset: 0x000011CD
		// (set) Token: 0x06000166 RID: 358 RVA: 0x00002FD5 File Offset: 0x000011D5
		[XmlIgnore]
		public bool SizeLimitSpecified
		{
			get
			{
				return this.sizeLimitFieldSpecified;
			}
			set
			{
				this.sizeLimitFieldSpecified = value;
			}
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000167 RID: 359 RVA: 0x00002FDE File Offset: 0x000011DE
		// (set) Token: 0x06000168 RID: 360 RVA: 0x00002FE6 File Offset: 0x000011E6
		[XmlText]
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

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000169 RID: 361 RVA: 0x00002FEF File Offset: 0x000011EF
		// (set) Token: 0x0600016A RID: 362 RVA: 0x00002FF7 File Offset: 0x000011F7
		[XmlNamespaceDeclarations]
		public XmlSerializerNamespaces xmlns
		{
			get
			{
				return this.xmlnsField;
			}
			set
			{
				this.xmlnsField = value;
			}
		}

		// Token: 0x040001F5 RID: 501
		private int sizeLimitField;

		// Token: 0x040001F6 RID: 502
		private bool sizeLimitFieldSpecified;

		// Token: 0x040001F7 RID: 503
		private string valueField;

		// Token: 0x040001F8 RID: 504
		private XmlSerializerNamespaces xmlnsField;
	}
}
