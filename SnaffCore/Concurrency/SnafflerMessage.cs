using SnaffCore.Classifiers;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using static SnaffCore.Config.Options;

namespace SnaffCore.Concurrency
{
    public struct SnafflerMessageComponents
    {
        public string DateTime;
        public string Type;
        public string Message;
        public string Triage;
        public string Permissions;
    }

    public class SnafflerMessage
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public SnafflerMessageType Type { get; set; }
        public FileResult FileResult { get; set; }
        public ShareResult ShareResult { get; set; }
        public DirResult DirResult { get; set; }

        private static int _typePaddingLength;
        private static int _triagePaddingLength;
        private static string _hostString;

        private static int GetTypePaddingLength()
        {
            if (_typePaddingLength == 0)
            {
                int typePaddingLength = 0;
                var enumValues = Enum.GetValues(typeof(SnafflerMessageType));

                foreach (var enumValue in enumValues)
                {
                    var fieldInfo = typeof(SnafflerMessageType).GetField(enumValue.ToString());
                    var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
                    string readableName = descriptionAttribute == null ? enumValue.ToString() : descriptionAttribute.Description;

                    if (readableName.Length > typePaddingLength)
                    {
                        typePaddingLength = readableName.Length;
                    }
                }

                _typePaddingLength = typePaddingLength;
            }

            return _typePaddingLength;
        }

        private static int GetTriagePaddingLength()
        {
            if (_triagePaddingLength == 0)
            {
                int triagePaddingLength = 0;
                var enumValues = Enum.GetValues(typeof(Triage));

                foreach (var enumValue in enumValues)
                {
                    var fieldInfo = typeof(Triage).GetField(enumValue.ToString());
                    var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
                    string readableName = descriptionAttribute == null ? enumValue.ToString() : descriptionAttribute.Description;

                    if (readableName.Length > triagePaddingLength)
                    {
                        triagePaddingLength = readableName.Length;
                    }
                }

                _triagePaddingLength = triagePaddingLength;
            }

            return _triagePaddingLength;
        }

        private static string GetHostString()
        {
            if (string.IsNullOrWhiteSpace(_hostString))
            {
                _hostString = "[" + System.Security.Principal.WindowsIdentity.GetCurrent().Name + "@" + System.Net.Dns.GetHostName() + "]";
            }

            return _hostString;
        }

        //  get a more user friendly name for a type variant if provided
        private string GetUserReadableType()
        {
            var typeVariantField = Type.GetType().GetField(Type.ToString());
            var typeVariantAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(typeVariantField, typeof(DescriptionAttribute));
            return typeVariantAttribute == null ? Type.ToString() : typeVariantAttribute.Description;
        }

        private static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }

        public SnafflerMessageComponents ToStringComponents()
        {
            return ToStringComponents(true, true);
        }

        public SnafflerMessageComponents ToStringComponents(bool includeTriage, bool includePermissions)
        {

            // this is everything that stays the same between message types
            string dateTime = String.Format("{1}{0}{2:u}", MyOptions.Separator, GetHostString(), DateTime.ToUniversalTime());
            string readableType = GetUserReadableType();
            string formattedMessage = Message;

            string triageString = "";
            string permissionsString = "";

            switch (Type)
            {
                case SnafflerMessageType.FileResult:
                    string fileResultTemplate = MyOptions.LogTSV ? "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}" : "{1}{0}<{2}|{3}|{4}|{5:u}>{0}{6}";

                    try
                    {
                        string matchedclassifier = FileResult.MatchedRule.RuleName;
                        DateTime modifiedStamp = FileResult.FileInfo.LastWriteTime.ToUniversalTime();

                        string matchedstring = "";

                        long fileSize = FileResult.FileInfo.Length;

                        string fileSizeString;

                        // TSV output will probably be machine-consumed.  Don't pretty it up.
                        if (MyOptions.LogTSV)
                        {
                            fileSizeString = fileSize.ToString();
                        }
                        else
                        {
                            fileSizeString = BytesToString(fileSize);
                        }

                        string filepath = FileResult.FileInfo.FullName;

                        string matchcontext = "";
                        if (FileResult.TextResult != null)
                        {
                            matchedstring = FileResult.TextResult.MatchedStrings[0];
                            matchcontext = FileResult.TextResult.MatchContext;
                            matchcontext = Regex.Replace(matchcontext, @"\r\n?|\n", "\\n"); // Replace newlines with \n for consistent log lines
                        }

                        triageString = FileResult.MatchedRule.Triage.ToString();
                        permissionsString = FileResult.RwStatus.ToString();

                        formattedMessage = string.Format(fileResultTemplate, MyOptions.Separator, filepath, matchedclassifier, matchedstring, fileSizeString, modifiedStamp, matchcontext);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine(FileResult.FileInfo.FullName);
                    }
                    break;
                case SnafflerMessageType.DirResult:
                    triageString = DirResult.Triage.ToString();
                    permissionsString = DirResult.RwStatus.ToString();
                    formattedMessage = DirResult.DirPath;
                    break;
                case SnafflerMessageType.ShareResult:
                    string shareResultTemplate = MyOptions.LogTSV ? "{1}{0}{2}" : "{1}{2}";

                    triageString = ShareResult.Triage.ToString();
                    permissionsString = ShareResult.RwStatus.ToString();

                    // this lets us do conditional formatting, we don't want angled brackets around a blank comment
                    string shareComment = ShareResult.ShareComment;
                    if (ShareResult.ShareComment.Length > 0) {
                        shareComment = MyOptions.Separator + "<" + shareComment + ">";
                    }

                    formattedMessage = string.Format(shareResultTemplate, MyOptions.Separator, ShareResult.SharePath, shareComment);
                    break;
            }

            return new SnafflerMessageComponents
            {
                DateTime = dateTime,
                Type = readableType,
                Message = formattedMessage,
                Triage = triageString,
                Permissions = permissionsString,
            };
        }

        public static string StringFromComponents(SnafflerMessageComponents components)
        {
            return StringFromComponents(components, true, true);
        }

        public static string StringFromComponents(SnafflerMessageComponents components, bool includeTriage, bool includePermissions)
        {
            string paddedType = ("[" + components.Type + "]");
            if (!MyOptions.LogTSV) paddedType = paddedType.PadRight(GetTypePaddingLength() + 2);

            string triageString = "";
            if (includeTriage && components.Triage.Length > 0)
            {
                triageString = ("[" + components.Triage + "]");
                if (!MyOptions.LogTSV) triageString = triageString.PadRight(GetTriagePaddingLength() + 2);
                triageString += MyOptions.Separator;
            }

            string permissionsString = (includePermissions && components.Permissions.Length > 0) ? String.Format(MyOptions.LogTSV ? "{1}{0}" : "[{1}]{0}", MyOptions.Separator, components.Permissions) : "";

            return String.Format("{1}{0}{2}{0}{3}{4}{0}{5}", MyOptions.Separator, components.DateTime, paddedType, triageString, permissionsString, components.Message);
        }

        public override string ToString()
        {
            SnafflerMessageComponents components = ToStringComponents();
            return SnafflerMessage.StringFromComponents(components);
        }
    }
}