using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x020000B4 RID: 180
	internal class XmlUtility
	{
		// Token: 0x060004CA RID: 1226 RVA: 0x00011A78 File Offset: 0x0000FC78
		private XmlUtility()
		{
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x00011A80 File Offset: 0x0000FC80
		public static bool SplitLdapAttributeOnOption(string ldapAttribute, ref string splitAttribute, ref string splitOption)
		{
			int num = ldapAttribute.IndexOf(LdapOptionConstants.LdapOptionSeperator, StringComparison.Ordinal);
			if (-1 == num)
			{
				return false;
			}
			splitAttribute = ldapAttribute.Substring(0, num);
			splitOption = ldapAttribute.Substring(num + 1);
			if (string.IsNullOrEmpty(splitOption))
			{
				throw new ArgumentException("Invalid Format");
			}
			return true;
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x00011ACC File Offset: 0x0000FCCC
		public static void SerializeLdapAttributeOption(XmlDictionaryWriter writer, string ldapOptions)
		{
			Match match = LdapOptionConstants.RangeOptionRegex.Match(ldapOptions);
			if (!match.Success || !match.Groups[LdapOptionConstants.HighRangeIndex].Success || !match.Groups[LdapOptionConstants.LowRangeIndex].Success)
			{
				throw new ArgumentException("Invalid Format");
			}
			writer.WriteAttributeString("RangeLow", string.Empty, match.Groups[LdapOptionConstants.LowRangeIndex].Value);
			writer.WriteAttributeString("RangeHigh", string.Empty, match.Groups[LdapOptionConstants.HighRangeIndex].Value);
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x00011B70 File Offset: 0x0000FD70
		public static bool DeserializeLdapAttributeOption(XmlReader reader, ref string ldapAttribute)
		{
			if (reader.AttributeCount > 1)
			{
				string attribute = reader.GetAttribute("RangeLow", string.Empty);
				if (!string.IsNullOrEmpty(attribute))
				{
					string attribute2 = reader.GetAttribute("RangeHigh", string.Empty);
					if (!string.IsNullOrEmpty(attribute2))
					{
						ldapAttribute = string.Format(LdapOptionConstants.RangeOptionFormatString, ldapAttribute, attribute, attribute2);
						return true;
					}
					throw new ADException();
				}
			}
			return false;
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x00011BD4 File Offset: 0x0000FDD4
		public static void SerializeAttributeList(XmlDictionaryWriter writer, string xmlAttribute, string ns, string syntheticPrefix, string attrPrefix, IList<string> attributes)
		{
			bool flag = false;
			foreach (string text in attributes)
			{
				if (AttributeNs.IsSynthetic(text, SyntheticAttributeOperation.Read, ref flag))
				{
					writer.WriteElementString(xmlAttribute, ns, flag ? text : XmlUtility.AddPrefix(syntheticPrefix, text));
				}
				else
				{
					string text2 = null;
					string text3 = null;
					if (XmlUtility.SplitLdapAttributeOnOption(text, ref text2, ref text3))
					{
						writer.WriteStartElement(xmlAttribute, ns);
						XmlUtility.SerializeLdapAttributeOption(writer, text3);
						writer.WriteValue(XmlUtility.AddPrefix(attrPrefix, text2));
						writer.WriteEndElement();
					}
					else
					{
						writer.WriteElementString(xmlAttribute, ns, XmlUtility.AddPrefix(attrPrefix, text));
					}
				}
			}
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x00011C84 File Offset: 0x0000FE84
		public static void SerializeExpires(XmlDictionaryWriter writer, DateTime? dateTimeFormat, TimeSpan? timeSpanFormat)
		{
			if (dateTimeFormat != null || timeSpanFormat != null)
			{
				writer.WriteStartElement("Expires", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
				if (dateTimeFormat != null)
				{
					writer.WriteValue(XmlConvert.ToString(dateTimeFormat.Value, XmlDateTimeSerializationMode.Utc));
				}
				else
				{
					writer.WriteValue(XmlConvert.ToString(timeSpanFormat.Value));
				}
				writer.WriteEndElement();
			}
		}

		// Token: 0x060004D0 RID: 1232 RVA: 0x00011CEC File Offset: 0x0000FEEC
		public static void DeserializeExpiresIfNeeded(XmlDictionaryReader reader, ref DateTime? dateTimeFormat, ref TimeSpan? timeSpanFormat)
		{
			if (reader.IsStartElement("Expires", "http://schemas.xmlsoap.org/ws/2004/09/enumeration"))
			{
				string text = reader.ReadElementString();
				if (text.StartsWith("P", StringComparison.OrdinalIgnoreCase))
				{
					timeSpanFormat = new TimeSpan?(XmlConvert.ToTimeSpan(text));
					return;
				}
				dateTimeFormat = new DateTime?(XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.Utc));
			}
		}

		// Token: 0x060004D1 RID: 1233 RVA: 0x00011D44 File Offset: 0x0000FF44
		public static string ConvertADAttributeValToXml(object value)
		{
			if (value is byte[])
			{
				return Convert.ToBase64String((byte[])value);
			}
			return (string)value;
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x00011D60 File Offset: 0x0000FF60
		public static void SerializeEnumerationContext(XmlDictionaryWriter writer, string context)
		{
			writer.WriteElementString("EnumerationContext", "http://schemas.xmlsoap.org/ws/2004/09/enumeration", context);
		}

		// Token: 0x060004D3 RID: 1235 RVA: 0x00011D73 File Offset: 0x0000FF73
		public static string DeserializeEunmerationContext(XmlDictionaryReader reader)
		{
			if (reader.IsStartElement("EnumerationContext", "http://schemas.xmlsoap.org/ws/2004/09/enumeration"))
			{
				return reader.ReadElementString("EnumerationContext", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			}
			return null;
		}

		// Token: 0x060004D4 RID: 1236 RVA: 0x00011D9C File Offset: 0x0000FF9C
		public static void WriteXsiTypeAttribute(XmlDictionaryWriter writer, string xsdType)
		{
			string text = writer.LookupPrefix("http://www.w3.org/2001/XMLSchema");
			writer.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", string.Format(CultureInfo.CurrentCulture, "{0}:{1}", text, xsdType));
		}

		// Token: 0x060004D5 RID: 1237 RVA: 0x00011DD8 File Offset: 0x0000FFD8
		public static void MarkHeaderAsUnderstood(MessageHeaders headers, string localName, string ns)
		{
			int num = headers.FindHeader(localName, ns);
			if (-1 == num)
			{
				return;
			}
			if (!headers.UnderstoodHeaders.Contains(headers[num]))
			{
				headers.UnderstoodHeaders.Add(headers[num]);
			}
		}

		// Token: 0x060004D6 RID: 1238 RVA: 0x00011E19 File Offset: 0x00010019
		public static string AddPrefix(string prefix, string element)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", prefix, element);
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x00011E2C File Offset: 0x0001002C
		public static string RemovePrefix(string prefix, string element)
		{
			if (!string.IsNullOrEmpty(element))
			{
				string text = prefix + ":";
				if (element.StartsWith(text, StringComparison.Ordinal))
				{
					return element.Substring(text.Length);
				}
			}
			return element;
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x00011E68 File Offset: 0x00010068
		public static void SplitPrefix(string name, out string prefix, out string localName)
		{
			string[] array = name.Split(new char[] { ':' });
			if (array.Length == 1)
			{
				localName = array[0];
				prefix = null;
				return;
			}
			prefix = array[0];
			localName = array[1];
		}

		// Token: 0x060004D9 RID: 1241 RVA: 0x00011EA0 File Offset: 0x000100A0
		public static bool ValidateNamespace(XmlReader reader, string value, string ns)
		{
			string[] array = value.Split(new char[] { ':' });
			if (array.Length == 1)
			{
				return string.Compare(reader.NamespaceURI, ns, StringComparison.OrdinalIgnoreCase) == 0;
			}
			string text = array[0];
			return string.Equals(reader.LookupNamespace(text), ns, StringComparison.Ordinal);
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x00011EE8 File Offset: 0x000100E8
		public static void DeserializeObjectReference(XmlReader reader, out string objectReference)
		{
			if (!reader.IsStartElement("objectReferenceProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ADWSXmlParserInvalidelement, "ad", "objectReferenceProperty"));
			}
			string text;
			objectReference = (text = reader.ReadString());
			if (text == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ADWSXmlParserInvalidelement, "ad", "objectReferenceProperty"));
			}
			reader.Read();
		}
	}
}
