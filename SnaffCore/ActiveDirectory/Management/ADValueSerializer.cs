using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x020000BC RID: 188
	internal sealed class ADValueSerializer
	{
		// Token: 0x0600050C RID: 1292 RVA: 0x0001250C File Offset: 0x0001070C
		private ADValueSerializer()
		{
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x00012514 File Offset: 0x00010714
		static ADValueSerializer()
		{
			ADValueSerializer._typeMappingToClr.Add("string", typeof(string));
			ADValueSerializer._typeMappingToClr.Add("base64Binary", typeof(byte[]));
			ADValueSerializer._typeMappingToClr.Add("boolean", typeof(bool));
			ADValueSerializer._typeMappingToClr.Add("int", typeof(int));
			ADValueSerializer._typeMappingToClr.Add("long", typeof(long));
			ADValueSerializer._typeMappingToClr.Add("dateTime", typeof(DateTime));
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x000125C4 File Offset: 0x000107C4
		private static void Deserialize(XmlReader reader, bool useInternStrings, out object value, out Type type)
		{
			object obj = null;
			List<object> list = null;
			int num = 0;
			type = null;
			while (reader.IsStartElement("value", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
			{
				num++;
				if (num > 1 && list == null)
				{
					list = new List<object>();
					list.Add(obj);
				}
				string text;
				string text2;
				XmlUtility.SplitPrefix(reader.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance"), out text, out text2);
				type = ADValueSerializer._typeMappingToClr[text2];
				obj = reader.ReadElementContentAs(type, null, "value", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				if (list != null)
				{
					if (useInternStrings && type == typeof(string))
					{
						list.Add(string.Intern((string)obj));
					}
					else
					{
						list.Add(obj);
					}
				}
			}
			value = ((list == null) ? obj : list.ToArray());
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x00012688 File Offset: 0x00010888
		public static void DeserializeSingleValue<T>(XmlReader reader, out T value)
		{
			object obj;
			Type type;
			ADValueSerializer.Deserialize(reader, false, out obj, out type);
			value = (T)((object)obj);
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x000126AC File Offset: 0x000108AC
		public static void Serialize(XmlDictionaryWriter writer, object value)
		{
			bool flag = false;
			writer.LookupPrefix("http://www.w3.org/2001/XMLSchema");
			writer.WriteStartElement("value", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
			string text;
			bool flag2;
			if (!(value.GetType() == typeof(byte[])))
			{
				switch (Type.GetTypeCode(value.GetType()))
				{
				case TypeCode.Boolean:
					text = "boolean";
					flag2 = false;
					value = (((bool)value) ? "true" : "false");
					goto IL_11E;
				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
					text = "int";
					flag2 = false;
					goto IL_11E;
				case TypeCode.Byte:
					text = "base64Binary";
					flag2 = true;
					flag = false;
					goto IL_11E;
				case TypeCode.Int64:
				case TypeCode.UInt64:
					text = "long";
					flag2 = false;
					goto IL_11E;
				case TypeCode.DateTime:
					text = "dateTime";
					flag2 = false;
					value = XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Utc);
					goto IL_11E;
				case TypeCode.String:
					text = "string";
					flag2 = false;
					goto IL_11E;
				}
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ADWSXmlParserUnexpectedElement, value.GetType().ToString()));
			}
			flag2 = true;
			flag = true;
			text = "base64Binary";
			IL_11E:
			XmlUtility.WriteXsiTypeAttribute(writer, text);
			if (flag2)
			{
				byte[] array;
				if (flag)
				{
					array = (byte[])value;
				}
				else
				{
					array = new byte[] { (byte)value };
				}
				writer.WriteBase64(array, 0, array.Length);
			}
			else
			{
				writer.WriteString(value.ToString());
			}
			writer.WriteEndElement();
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x00012824 File Offset: 0x00010A24
		public static void Deserialize(XmlReader reader, bool useInternStrings, out object value)
		{
			Type type;
			ADValueSerializer.Deserialize(reader, useInternStrings, out value, out type);
		}

		// Token: 0x040004C3 RID: 1219
		private static Dictionary<string, Type> _typeMappingToClr = new Dictionary<string, Type>(6);
	}
}
