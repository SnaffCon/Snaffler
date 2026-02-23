using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	// Token: 0x02000312 RID: 786
	internal class ADPullRequest : AdwsRequestMsg
	{
		// Token: 0x06001B09 RID: 6921 RVA: 0x0005354D File Offset: 0x0005174D
		public ADPullRequest(string instance, string enumerationContext)
			: base(instance)
		{
			if (string.IsNullOrEmpty(enumerationContext))
			{
				throw new ArgumentNullException("enumerationContext");
			}
			this._enumerationContext = enumerationContext;
		}

		// Token: 0x06001B0A RID: 6922 RVA: 0x00053570 File Offset: 0x00051770
		public ADPullRequest(string instance, string enumerationContext, IList<DirectoryControl> controls)
			: this(instance, enumerationContext)
		{
			this._controls = controls;
		}

		// Token: 0x06001B0B RID: 6923 RVA: 0x00053581 File Offset: 0x00051781
		public ADPullRequest(string instance, string enumerationContext, TimeSpan maxTime)
			: this(instance, enumerationContext)
		{
			this._timeout = new TimeSpan?(maxTime);
			this._enumerationContext = enumerationContext;
		}

		// Token: 0x06001B0C RID: 6924 RVA: 0x0005359E File Offset: 0x0005179E
		public ADPullRequest(string instance, string enumerationContext, TimeSpan maxTime, IList<DirectoryControl> controls)
			: this(instance, enumerationContext, maxTime)
		{
			this._controls = controls;
		}

		// Token: 0x1700092B RID: 2347
		// (get) Token: 0x06001B0D RID: 6925 RVA: 0x000535B1 File Offset: 0x000517B1
		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Pull";
			}
		}

		// Token: 0x1700092C RID: 2348
		// (get) Token: 0x06001B0F RID: 6927 RVA: 0x000535ED File Offset: 0x000517ED
		// (set) Token: 0x06001B0E RID: 6926 RVA: 0x000535B8 File Offset: 0x000517B8
		public virtual uint? MaxElements
		{
			get
			{
				return this._maxElements;
			}
			set
			{
				uint num = 0U;
				uint? num2 = value;
				if ((num == num2.GetValueOrDefault()) & (num2 != null))
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._maxElements = value;
			}
		}

		// Token: 0x1700092D RID: 2349
		// (get) Token: 0x06001B10 RID: 6928 RVA: 0x000535F5 File Offset: 0x000517F5
		public virtual IList<DirectoryControl> Controls
		{
			get
			{
				if (this._controls == null)
				{
					this._controls = new List<DirectoryControl>();
				}
				return this._controls;
			}
		}

		// Token: 0x06001B11 RID: 6929 RVA: 0x00053610 File Offset: 0x00051810
		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Pull", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			if (this._enumerationContext != null)
			{
				writer.WriteElementString("EnumerationContext", "http://schemas.xmlsoap.org/ws/2004/09/enumeration", this._enumerationContext);
			}
			if (this._timeout != null)
			{
				writer.WriteStartElement("MaxTime", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
				writer.WriteValue(this._timeout.Value);
				writer.WriteEndElement();
			}
			if (this._maxElements != null)
			{
				writer.WriteStartElement("MaxElements", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
				writer.WriteValue((long)((ulong)this._maxElements.Value));
				writer.WriteEndElement();
			}
			if (this._controls != null)
			{
				DirectoryControlSerializer.Serialize(writer, this._controls);
			}
			writer.WriteEndElement();
		}

		// Token: 0x06001B12 RID: 6930 RVA: 0x000536CE File Offset: 0x000518CE
		protected override void OnWriteStartBody(XmlDictionaryWriter writer)
		{
			base.OnWriteStartBody(writer);
			writer.WriteXmlnsAttribute("wsen", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
		}

		// Token: 0x06001B13 RID: 6931 RVA: 0x000536E7 File Offset: 0x000518E7
		public virtual void SetTimeout(TimeSpan timeout)
		{
			this._timeout = new TimeSpan?(timeout);
		}

		// Token: 0x04000C91 RID: 3217
		private TimeSpan? _timeout;

		// Token: 0x04000C92 RID: 3218
		private uint? _maxElements;

		// Token: 0x04000C93 RID: 3219
		private string _enumerationContext;

		// Token: 0x04000C94 RID: 3220
		private IList<DirectoryControl> _controls;
	}
}
