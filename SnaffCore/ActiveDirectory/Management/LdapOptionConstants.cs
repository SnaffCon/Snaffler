using System;
using System.Text.RegularExpressions;

namespace Microsoft.ActiveDirectory.Management
{
	// Token: 0x02000057 RID: 87
	internal class LdapOptionConstants
	{
		// Token: 0x060001A5 RID: 421 RVA: 0x000032F0 File Offset: 0x000014F0
		private LdapOptionConstants()
		{
		}

		// Token: 0x04000213 RID: 531
		internal static string LdapOptionSeperator = ";";

		// Token: 0x04000214 RID: 532
		internal static string LowRangeGroup = "LOWRANGE";

		// Token: 0x04000215 RID: 533
		internal static string HiRangeGroup = "HIRANGE";

		// Token: 0x04000216 RID: 534
		internal static string RangeOptionText = "Range=";

		// Token: 0x04000217 RID: 535
		internal static string RangeValueSeperator = "-";

		// Token: 0x04000218 RID: 536
		internal static string RangeOptionFormatString = string.Concat(new string[]
		{
			"{0}",
			LdapOptionConstants.LdapOptionSeperator,
			LdapOptionConstants.RangeOptionText,
			"{1}",
			LdapOptionConstants.RangeValueSeperator,
			"{2}"
		});

		// Token: 0x04000219 RID: 537
		internal static Regex RangeOptionRegex = new Regex(string.Concat(new string[]
		{
			LdapOptionConstants.RangeOptionText,
			"(?<",
			LdapOptionConstants.LowRangeGroup,
			">[0-9]*)",
			LdapOptionConstants.RangeValueSeperator,
			"(?<",
			LdapOptionConstants.HiRangeGroup,
			">[0-9]*|\\*)$"
		}), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.RightToLeft | RegexOptions.CultureInvariant);

		// Token: 0x0400021A RID: 538
		internal static int LowRangeIndex = LdapOptionConstants.RangeOptionRegex.GroupNumberFromName(LdapOptionConstants.LowRangeGroup);

		// Token: 0x0400021B RID: 539
		internal static int HighRangeIndex = LdapOptionConstants.RangeOptionRegex.GroupNumberFromName(LdapOptionConstants.HiRangeGroup);
	}
}
