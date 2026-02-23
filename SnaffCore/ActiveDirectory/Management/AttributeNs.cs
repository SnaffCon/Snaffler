using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x020000B6 RID: 182
	internal class AttributeNs
	{
		// Token: 0x060004DB RID: 1243 RVA: 0x00011F59 File Offset: 0x00010159
		private AttributeNs()
		{
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x00011F64 File Offset: 0x00010164
		static AttributeNs()
		{
			AttributeNs._SyntheticNsDict.Add("objectReferenceProperty", SyntheticAttributeOperation.Read | SyntheticAttributeOperation.Write);
			AttributeNs._SyntheticNsDict.Add("container-hierarchy-parent", SyntheticAttributeOperation.Read | SyntheticAttributeOperation.Write);
			AttributeNs._SyntheticNsDict.Add("relativeDistinguishedName", SyntheticAttributeOperation.Read | SyntheticAttributeOperation.Write);
			AttributeNs._SyntheticNsDict.Add("ad:all", SyntheticAttributeOperation.Read | SyntheticAttributeOperation.Write);
			AttributeNs._SyntheticNsDict.Add("distinguishedName", SyntheticAttributeOperation.Read);
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x00011FCC File Offset: 0x000101CC
		public static string LookupNs(string attribute, SyntheticAttributeOperation operation)
		{
			SyntheticAttributeOperation syntheticAttributeOperation;
			if (!AttributeNs._SyntheticNsDict.TryGetValue(attribute, out syntheticAttributeOperation))
			{
				return "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data";
			}
			if (string.Equals(attribute, "ad:all", StringComparison.Ordinal))
			{
				return string.Empty;
			}
			if (operation != (syntheticAttributeOperation & operation))
			{
				return "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data";
			}
			return "http://schemas.microsoft.com/2008/1/ActiveDirectory";
		}

		// Token: 0x060004DE RID: 1246 RVA: 0x00012014 File Offset: 0x00010214
		public static bool IsSynthetic(string attribute, SyntheticAttributeOperation operation)
		{
			SyntheticAttributeOperation syntheticAttributeOperation;
			return AttributeNs._SyntheticNsDict.TryGetValue(attribute, out syntheticAttributeOperation) && operation == (syntheticAttributeOperation & operation);
		}

		// Token: 0x060004DF RID: 1247 RVA: 0x00012038 File Offset: 0x00010238
		public static bool IsSynthetic(string attribute, SyntheticAttributeOperation operation, ref bool hasPrefix)
		{
			hasPrefix = false;
			if (AttributeNs.IsSynthetic(attribute, operation))
			{
				if (string.Equals(attribute, "ad:all", StringComparison.Ordinal))
				{
					hasPrefix = true;
				}
				return true;
			}
			return false;
		}

		// Token: 0x040004BB RID: 1211
		private static Dictionary<string, SyntheticAttributeOperation> _SyntheticNsDict = new Dictionary<string, SyntheticAttributeOperation>(5);
	}
}
