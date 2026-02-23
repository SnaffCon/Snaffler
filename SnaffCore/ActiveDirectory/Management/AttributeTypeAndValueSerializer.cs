using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x020000C0 RID: 192
	internal sealed class AttributeTypeAndValueSerializer
	{
		// Token: 0x06000520 RID: 1312 RVA: 0x00012A94 File Offset: 0x00010C94
		private AttributeTypeAndValueSerializer()
		{
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x00012A9C File Offset: 0x00010C9C
		private static string FormatAttributeName(string prefix, string attribute)
		{
			return XmlUtility.AddPrefix(prefix, attribute);
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x00012AA8 File Offset: 0x00010CA8
		private static void InternalSerialize(XmlDictionaryWriter writer, ChangeOperation ChangeOperation, string ns, string property, object value)
		{
			string text = writer.LookupPrefix(ns);
			writer.LookupPrefix("http://schemas.microsoft.com/2008/1/ActiveDirectory");
			if (ChangeOperation == ChangeOperation.None)
			{
				writer.WriteStartElement("AttributeTypeAndValue", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
			}
			else
			{
				writer.WriteStartElement("Change", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
				switch (ChangeOperation)
				{
				case ChangeOperation.Add:
					writer.WriteAttributeString("Operation", "add");
					break;
				case ChangeOperation.Delete:
					writer.WriteAttributeString("Operation", "delete");
					break;
				case ChangeOperation.Replace:
					writer.WriteAttributeString("Operation", "replace");
					break;
				}
			}
			writer.WriteElementString("AttributeType", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess", AttributeTypeAndValueSerializer.FormatAttributeName(text, property));
			if (value != null)
			{
				if (value is ICollection)
				{
					ICollection collection = (ICollection)value;
					if (collection.Count > 0)
					{
						writer.WriteStartElement("AttributeValue", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
						foreach (object obj in collection)
						{
							ADValueSerializer.Serialize(writer, obj);
						}
						writer.WriteEndElement();
					}
				}
				else
				{
					writer.WriteStartElement("AttributeValue", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
					if (value is DirectoryAttributeModification)
					{
						DirectoryAttributeModification directoryAttributeModification = (DirectoryAttributeModification)value;
						ADValueSerializer.Serialize(writer, directoryAttributeModification[0]);
					}
					else
					{
						ADValueSerializer.Serialize(writer, value);
					}
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x00012C18 File Offset: 0x00010E18
		public static void Serialize(XmlDictionaryWriter writer, DirectoryAttribute attribute)
		{
			AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation.None, AttributeNs.LookupNs(attribute.Name, SyntheticAttributeOperation.Write), attribute.Name, attribute);
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x00012C34 File Offset: 0x00010E34
		public static void Serialize(XmlDictionaryWriter writer, ChangeOperation ChangeOperation, DirectoryAttributeModification attribute)
		{
			AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation, AttributeNs.LookupNs(attribute.Name, SyntheticAttributeOperation.Write), attribute.Name, attribute);
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x00012C50 File Offset: 0x00010E50
		public static void Serialize(XmlDictionaryWriter writer, DirectoryAttributeModification attribute)
		{
			ChangeOperation changeOperation = ChangeOperation.None;
			switch (attribute.Operation)
			{
			case DirectoryAttributeOperation.Add:
				changeOperation = ChangeOperation.Add;
				break;
			case DirectoryAttributeOperation.Delete:
				changeOperation = ChangeOperation.Delete;
				break;
			case DirectoryAttributeOperation.Replace:
				changeOperation = ChangeOperation.Replace;
				break;
			}
			AttributeTypeAndValueSerializer.InternalSerialize(writer, changeOperation, AttributeNs.LookupNs(attribute.Name, SyntheticAttributeOperation.Write), attribute.Name, attribute);
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x00012C9E File Offset: 0x00010E9E
		public static void Serialize(XmlDictionaryWriter writer, string ns, string property, object value)
		{
			AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation.None, ns, property, value);
		}

		// Token: 0x06000527 RID: 1319 RVA: 0x00012CAA File Offset: 0x00010EAA
		public static void Serialize(XmlDictionaryWriter writer, ChangeOperation ChangeOperation, string ns, string property, object value)
		{
			AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation, ns, property, value);
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x00012CB8 File Offset: 0x00010EB8
		public static void Serialize(XmlDictionaryWriter writer, ChangeOperation ChangeOperation, IList<DirectoryAttributeModification> attributes)
		{
			foreach (DirectoryAttributeModification directoryAttributeModification in attributes)
			{
				AttributeTypeAndValueSerializer.InternalSerialize(writer, ChangeOperation, AttributeNs.LookupNs(directoryAttributeModification.Name, SyntheticAttributeOperation.Write), directoryAttributeModification.Name, directoryAttributeModification);
			}
		}
	}
}
