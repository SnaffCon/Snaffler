using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x0200007C RID: 124
	[Serializable]
	public class ADException : Exception
	{
		// Token: 0x060002F3 RID: 755 RVA: 0x000075C5 File Offset: 0x000057C5
		public ADException()
		{
		}

		// Token: 0x060002F4 RID: 756 RVA: 0x000075CD File Offset: 0x000057CD
		public ADException(string message)
			: base(message)
		{
		}

		// Token: 0x060002F5 RID: 757 RVA: 0x000075D6 File Offset: 0x000057D6
		public ADException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		// Token: 0x060002F6 RID: 758 RVA: 0x000075E0 File Offset: 0x000057E0
		protected ADException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			this._errorCode = info.GetInt32("errorCode");
			this._serverErrorMessage = (string)info.GetValue("serverErrorMessage", typeof(string));
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x0000761B File Offset: 0x0000581B
		public ADException(string message, int errorCode)
			: base(message)
		{
			this._errorCode = errorCode;
		}

		// Token: 0x060002F8 RID: 760 RVA: 0x0000762B File Offset: 0x0000582B
		public ADException(string message, Exception inner, int errorCode)
			: base(message, inner)
		{
			this._errorCode = errorCode;
		}

		// Token: 0x060002F9 RID: 761 RVA: 0x0000763C File Offset: 0x0000583C
		public ADException(string message, string serverErrorMessage)
			: base(message)
		{
			this._serverErrorMessage = serverErrorMessage;
		}

		// Token: 0x060002FA RID: 762 RVA: 0x0000764C File Offset: 0x0000584C
		public ADException(string message, int errorCode, string serverErrorMessage)
			: base(message)
		{
			this._errorCode = errorCode;
			this._serverErrorMessage = serverErrorMessage;
		}

		// Token: 0x060002FB RID: 763 RVA: 0x00007663 File Offset: 0x00005863
		public ADException(string message, Exception inner, string serverErrorMessage)
			: base(message, inner)
		{
			this._serverErrorMessage = serverErrorMessage;
		}

		// Token: 0x060002FC RID: 764 RVA: 0x00007674 File Offset: 0x00005874
		public ADException(string message, Exception inner, int errorCode, string serverErrorMessage)
			: base(message, inner)
		{
			this._errorCode = errorCode;
			this._serverErrorMessage = serverErrorMessage;
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x060002FD RID: 765 RVA: 0x0000768D File Offset: 0x0000588D
		public int ErrorCode
		{
			get
			{
				return this._errorCode;
			}
		}

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x060002FE RID: 766 RVA: 0x00007695 File Offset: 0x00005895
		public string ServerErrorMessage
		{
			get
			{
				return this._serverErrorMessage;
			}
		}

		// Token: 0x060002FF RID: 767 RVA: 0x000076A0 File Offset: 0x000058A0
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext streamingContext)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("errorCode", this._errorCode);
			info.AddValue("serverErrorMessage", this._serverErrorMessage, typeof(string));
			base.GetObjectData(info, streamingContext);
		}

		// Token: 0x0400030D RID: 781
		private int _errorCode;

		// Token: 0x0400030E RID: 782
		private string _serverErrorMessage;
	}
}
