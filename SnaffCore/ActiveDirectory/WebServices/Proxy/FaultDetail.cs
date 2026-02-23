using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x0200003D RID: 61
	[XmlInclude(typeof(RenewFault))]
	[XmlInclude(typeof(PullFault))]
	[XmlInclude(typeof(EnumerateFault))]
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[XmlRoot(Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[Serializable]
	public class FaultDetail
	{
		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600012D RID: 301 RVA: 0x00002DF5 File Offset: 0x00000FF5
		// (set) Token: 0x0600012E RID: 302 RVA: 0x00002DFD File Offset: 0x00000FFD
		[XmlElement(Order = 0)]
		public ArgumentErrorDetail ArgumentError
		{
			get
			{
				return this.argumentErrorField;
			}
			set
			{
				this.argumentErrorField = value;
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x0600012F RID: 303 RVA: 0x00002E06 File Offset: 0x00001006
		// (set) Token: 0x06000130 RID: 304 RVA: 0x00002E0E File Offset: 0x0000100E
		[XmlElement(Order = 1)]
		public string Error
		{
			get
			{
				return this.errorField;
			}
			set
			{
				this.errorField = value;
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000131 RID: 305 RVA: 0x00002E17 File Offset: 0x00001017
		// (set) Token: 0x06000132 RID: 306 RVA: 0x00002E1F File Offset: 0x0000101F
		[XmlElement(Order = 2)]
		public DirectoryErrorDetail DirectoryError
		{
			get
			{
				return this.directoryErrorField;
			}
			set
			{
				this.directoryErrorField = value;
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000133 RID: 307 RVA: 0x00002E28 File Offset: 0x00001028
		// (set) Token: 0x06000134 RID: 308 RVA: 0x00002E30 File Offset: 0x00001030
		[XmlElement(Order = 3)]
		public string InvalidAttributeType
		{
			get
			{
				return this.invalidAttributeTypeField;
			}
			set
			{
				this.invalidAttributeTypeField = value;
			}
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000135 RID: 309 RVA: 0x00002E39 File Offset: 0x00001039
		// (set) Token: 0x06000136 RID: 310 RVA: 0x00002E41 File Offset: 0x00001041
		[XmlElement(Order = 4)]
		public string InvalidOperation
		{
			get
			{
				return this.invalidOperationField;
			}
			set
			{
				this.invalidOperationField = value;
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000137 RID: 311 RVA: 0x00002E4A File Offset: 0x0000104A
		// (set) Token: 0x06000138 RID: 312 RVA: 0x00002E52 File Offset: 0x00001052
		[XmlElement(Order = 5)]
		public ChangeType InvalidChange
		{
			get
			{
				return this.invalidChangeField;
			}
			set
			{
				this.invalidChangeField = value;
			}
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000139 RID: 313 RVA: 0x00002E5B File Offset: 0x0000105B
		// (set) Token: 0x0600013A RID: 314 RVA: 0x00002E63 File Offset: 0x00001063
		[XmlElement(Order = 6)]
		public AttributeTypeAndValue InvalidAttributeTypeOrValue
		{
			get
			{
				return this.invalidAttributeTypeOrValueField;
			}
			set
			{
				this.invalidAttributeTypeOrValueField = value;
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x0600013B RID: 315 RVA: 0x00002E6C File Offset: 0x0000106C
		// (set) Token: 0x0600013C RID: 316 RVA: 0x00002E74 File Offset: 0x00001074
		[XmlElement(Order = 7)]
		public string ShortError
		{
			get
			{
				return this.shortErrorField;
			}
			set
			{
				this.shortErrorField = value;
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x0600013D RID: 317 RVA: 0x00002E7D File Offset: 0x0000107D
		// (set) Token: 0x0600013E RID: 318 RVA: 0x00002E85 File Offset: 0x00001085
		[XmlElement(Order = 8)]
		public string UnknownAttribute
		{
			get
			{
				return this.unknownAttributeField;
			}
			set
			{
				this.unknownAttributeField = value;
			}
		}

		// Token: 0x040001DE RID: 478
		private ArgumentErrorDetail argumentErrorField;

		// Token: 0x040001DF RID: 479
		private string errorField;

		// Token: 0x040001E0 RID: 480
		private DirectoryErrorDetail directoryErrorField;

		// Token: 0x040001E1 RID: 481
		private string invalidAttributeTypeField;

		// Token: 0x040001E2 RID: 482
		private string invalidOperationField;

		// Token: 0x040001E3 RID: 483
		private ChangeType invalidChangeField;

		// Token: 0x040001E4 RID: 484
		private AttributeTypeAndValue invalidAttributeTypeOrValueField;

		// Token: 0x040001E5 RID: 485
		private string shortErrorField;

		// Token: 0x040001E6 RID: 486
		private string unknownAttributeField;
	}
}
