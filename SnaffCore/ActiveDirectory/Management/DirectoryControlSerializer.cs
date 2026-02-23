using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x020000BE RID: 190
	internal sealed class DirectoryControlSerializer
	{
		// Token: 0x0600051D RID: 1309 RVA: 0x000128A3 File Offset: 0x00010AA3
		private DirectoryControlSerializer()
		{
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x000128AC File Offset: 0x00010AAC
		public static void Serialize(XmlDictionaryWriter writer, IList<DirectoryControl> controls)
		{
			if (controls == null || controls.Count == 0)
			{
				return;
			}
			writer.WriteStartElement("ad", "controls", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
			foreach (DirectoryControl directoryControl in controls)
			{
				byte[] value = directoryControl.GetValue();
				writer.WriteStartElement("ad", "control", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				writer.WriteAttributeString("type", directoryControl.Type.ToLowerInvariant());
				writer.WriteAttributeString("criticality", directoryControl.IsCritical ? "true" : "false");
				if (value != null && value.Length != 0)
				{
					writer.WriteStartElement("ad", "controlValue", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
					XmlUtility.WriteXsiTypeAttribute(writer, "base64Binary");
					writer.WriteBase64(value, 0, value.Length);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x000129AC File Offset: 0x00010BAC
		public static void Deserialize(XmlDictionaryReader reader, out IList<DirectoryControl> controls, bool mustBePresent, bool fullChecks)
		{
			controls = new List<DirectoryControl>();
			if (!mustBePresent && !reader.IsStartElement("controls", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
			{
				return;
			}
			reader.ReadFullStartElement("controls", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
			while (reader.IsStartElement("control", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
			{
				string attribute = reader.GetAttribute("type");
				string attribute2 = reader.GetAttribute("criticality");
				reader.Read();
				byte[] array;
				if (reader.IsStartElement("controlValue", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
				{
					string attribute3 = reader.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
					if (attribute3 == null)
					{
						throw new ArgumentException();
					}
					string text;
					string text2;
					XmlUtility.SplitPrefix(attribute3, out text, out text2);
					array = reader.ReadElementContentAsBase64();
				}
				else
				{
					array = null;
				}
				bool flag = string.Equals("true", attribute2);
				DirectoryControl directoryControl = new DirectoryControl(attribute, array, flag, true);
				controls.Add(directoryControl);
				reader.Read();
			}
		}
	}
}
