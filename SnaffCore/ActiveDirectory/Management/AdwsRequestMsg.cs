using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x020000B8 RID: 184
	internal abstract class AdwsRequestMsg : AdwsMessage
	{
		// Token: 0x060004E9 RID: 1257 RVA: 0x00012120 File Offset: 0x00010320
		private AdwsRequestMsg()
		{
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x00012134 File Offset: 0x00010334
		protected AdwsRequestMsg(string instance)
		{
			this._messageHeaders = new MessageHeaders(this.Version, 7);
			this.Headers.Action = this.Action;
			this.Headers.Add(MessageHeader.CreateHeader("instance", "http://schemas.microsoft.com/2008/1/ActiveDirectory", instance));
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x00012190 File Offset: 0x00010390
		protected AdwsRequestMsg(string instance, string objectReferenceProperty)
			: this(instance)
		{
			if (!string.IsNullOrEmpty(objectReferenceProperty))
			{
				this.Headers.Add(MessageHeader.CreateHeader("objectReferenceProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory", objectReferenceProperty));
			}
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x060004EC RID: 1260 RVA: 0x000121BC File Offset: 0x000103BC
		public override MessageHeaders Headers
		{
			get
			{
				return this._messageHeaders;
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x060004ED RID: 1261 RVA: 0x000121C4 File Offset: 0x000103C4
		public override MessageProperties Properties
		{
			get
			{
				return this._messageProperties;
			}
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x060004EE RID: 1262
		public abstract string Action { get; }

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x060004EF RID: 1263 RVA: 0x000121CC File Offset: 0x000103CC
		public override MessageVersion Version
		{
			get
			{
				return MessageVersion.Soap12WSAddressing10;
			}
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x000121D3 File Offset: 0x000103D3
		protected virtual void AddPrefixIfNeeded(XmlDictionaryWriter writer, string prefix, string ns)
		{
			if (writer.LookupPrefix(ns) == null)
			{
				writer.WriteXmlnsAttribute(prefix, ns);
			}
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x000121E6 File Offset: 0x000103E6
		protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
		{
			base.OnWriteStartHeaders(writer);
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x000121F0 File Offset: 0x000103F0
		protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
		{
			base.OnWriteStartEnvelope(writer);
			writer.WriteXmlnsAttribute("addata", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data");
			writer.WriteXmlnsAttribute("ad", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
			writer.WriteXmlnsAttribute("xsd", "http://www.w3.org/2001/XMLSchema");
			writer.WriteXmlnsAttribute("xsi", "http://www.w3.org/2001/XMLSchema-instance");
			if (writer.LookupPrefix("http://www.w3.org/2005/08/addressing") == null)
			{
				writer.WriteXmlnsAttribute("wsa", "http://www.w3.org/2005/08/addressing");
			}
		}

		// Token: 0x040004BC RID: 1212
		private MessageHeaders _messageHeaders;

		// Token: 0x040004BD RID: 1213
		private MessageProperties _messageProperties = new MessageProperties();

		// Token: 0x040004BE RID: 1214
		private const int _initialHeaderBufferSize = 7;
	}
}
