using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000041 RID: 65
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[Serializable]
	public class DirectoryErrorDetail
	{
		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600014F RID: 335 RVA: 0x00002F14 File Offset: 0x00001114
		// (set) Token: 0x06000150 RID: 336 RVA: 0x00002F1C File Offset: 0x0000111C
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

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000151 RID: 337 RVA: 0x00002F25 File Offset: 0x00001125
		// (set) Token: 0x06000152 RID: 338 RVA: 0x00002F2D File Offset: 0x0000112D
		[XmlElement(Order = 1)]
		public string ErrorCode
		{
			get
			{
				return this.errorCodeField;
			}
			set
			{
				this.errorCodeField = value;
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000153 RID: 339 RVA: 0x00002F36 File Offset: 0x00001136
		// (set) Token: 0x06000154 RID: 340 RVA: 0x00002F3E File Offset: 0x0000113E
		[XmlElement(Order = 2)]
		public string ExtendedErrorMessage
		{
			get
			{
				return this.extendedErrorMessageField;
			}
			set
			{
				this.extendedErrorMessageField = value;
			}
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000155 RID: 341 RVA: 0x00002F47 File Offset: 0x00001147
		// (set) Token: 0x06000156 RID: 342 RVA: 0x00002F4F File Offset: 0x0000114F
		[XmlElement(Order = 3)]
		public string MatchedDN
		{
			get
			{
				return this.matchedDNField;
			}
			set
			{
				this.matchedDNField = value;
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000157 RID: 343 RVA: 0x00002F58 File Offset: 0x00001158
		// (set) Token: 0x06000158 RID: 344 RVA: 0x00002F60 File Offset: 0x00001160
		[XmlElement("Referral", Order = 4)]
		public string[] Referral
		{
			get
			{
				return this.referralField;
			}
			set
			{
				this.referralField = value;
			}
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000159 RID: 345 RVA: 0x00002F69 File Offset: 0x00001169
		// (set) Token: 0x0600015A RID: 346 RVA: 0x00002F71 File Offset: 0x00001171
		[XmlElement(Order = 5)]
		public string Win32ErrorCode
		{
			get
			{
				return this.win32ErrorCodeField;
			}
			set
			{
				this.win32ErrorCodeField = value;
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600015B RID: 347 RVA: 0x00002F7A File Offset: 0x0000117A
		// (set) Token: 0x0600015C RID: 348 RVA: 0x00002F82 File Offset: 0x00001182
		[XmlElement(Order = 6)]
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

		// Token: 0x040001ED RID: 493
		private string messageField;

		// Token: 0x040001EE RID: 494
		private string errorCodeField;

		// Token: 0x040001EF RID: 495
		private string extendedErrorMessageField;

		// Token: 0x040001F0 RID: 496
		private string matchedDNField;

		// Token: 0x040001F1 RID: 497
		private string[] referralField;

		// Token: 0x040001F2 RID: 498
		private string win32ErrorCodeField;

		// Token: 0x040001F3 RID: 499
		private string shortMessageField;
	}
}
