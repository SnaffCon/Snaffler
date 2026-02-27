using System;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x020000B7 RID: 183
	internal abstract class AdwsMessage : Message
	{
		// Token: 0x060004E0 RID: 1248 RVA: 0x0001205A File Offset: 0x0001025A
		protected AdwsMessage()
		{
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x00012062 File Offset: 0x00010262
		private AdwsMessage(Message msg)
		{
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x0001206A File Offset: 0x0001026A
		internal static MessageBuffer MessageToBuffer(Message msg)
		{
			return msg.CreateBufferedCopy(int.MaxValue);
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x00012077 File Offset: 0x00010277
		internal static Message BufferToMessage(MessageBuffer buffer)
		{
			return buffer.CreateMessage();
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x00012080 File Offset: 0x00010280
		internal static string MessageToString(Message msg, bool indent)
		{
			MessageBuffer messageBuffer = msg.CreateBufferedCopy(5242880);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.OmitXmlDeclaration = true;
			xmlWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
			xmlWriterSettings.Encoding = Encoding.UTF8;
			if (indent)
			{
				xmlWriterSettings.Indent = true;
				xmlWriterSettings.IndentChars = "    ";
				xmlWriterSettings.NewLineOnAttributes = false;
			}
			StringBuilder stringBuilder = new StringBuilder();
			XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings);
			XmlDictionaryWriter xmlDictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
			messageBuffer.CreateMessage().WriteMessage(xmlDictionaryWriter);
			xmlWriter.Flush();
			xmlDictionaryWriter.Close();
			return stringBuilder.ToString();
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x00012103 File Offset: 0x00010303
		protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
		{
			base.OnWriteStartHeaders(writer);
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x0001210C File Offset: 0x0001030C
		protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
		{
			base.OnWriteStartEnvelope(writer);
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x00012115 File Offset: 0x00010315
		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x00012117 File Offset: 0x00010317
		public virtual string ToString(bool indent)
		{
			return AdwsMessage.MessageToString(this, indent);
		}
	}
}
