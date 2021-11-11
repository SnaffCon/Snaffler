using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Principal;

namespace SnaffCore.ActiveDirectory.LDAP
{
    internal static class Extensions
    {
        /// <summary>
        /// Helper function to print attributes of a SearchResultEntry. Not used currently
        /// </summary>
        /// <param name="searchResultEntry"></param>
        public static void PrintEntry(this SearchResultEntry searchResultEntry)
        {
            foreach (var propertyName in searchResultEntry.Attributes.AttributeNames)
            {
                var property = propertyName.ToString();
                Console.WriteLine(property);
                Console.WriteLine(searchResultEntry.GetProperty(property));
            }
        }


        #region SearchResultEntry
        /// <summary>
        /// Gets the specified property as a string from the SearchResultEntry
        /// </summary>
        /// <param name="searchResultEntry"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetProperty(this SearchResultEntry searchResultEntry, string property)
        {
            if (!searchResultEntry.Attributes.Contains(property))
                return null;

            var collection = searchResultEntry.Attributes[property];
            //Use getvalues to auto-convert to the proper type
            var lookups = collection.GetValues(typeof(string));
            if (lookups.Length == 0)
                return null;

            if (!(lookups[0] is string prop) || prop.Length == 0)
                return null;

            return prop;
        }

        /// <summary>
        /// Gets the objectsid property as a string from the SearchResultEntry
        /// </summary>
        /// <param name="searchResultEntry"></param>
        /// <returns></returns>
        public static string GetSid(this SearchResultEntry searchResultEntry)
        {
            try
            {
                if (!searchResultEntry.Attributes.Contains("objectsid")) return null;
            }
            catch (ArgumentNullException)
            {
                return null;
            }

            object[] s;
            try
            {
                s = searchResultEntry.Attributes["objectsid"].GetValues(typeof(byte[]));
            }
            catch (NotSupportedException)
            {
                return null;
            }

            if (s.Length == 0)
                return null;

            if (!(s[0] is byte[] sidBytes) || sidBytes.Length == 0)
                return null;

            try
            {
                var sid = new SecurityIdentifier(sidBytes, 0);
                return sid.Value.ToUpper();
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the specified property as a string array from the SearchResultEntry
        /// </summary>
        /// <param name="searchResultEntry"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string[] GetPropertyAsArray(this SearchResultEntry searchResultEntry, string property)
        {
            if (!searchResultEntry.Attributes.Contains(property))
                return new string[0];

            var values = searchResultEntry.Attributes[property];
            var strings = values.GetValues(typeof(string));

            if (!(strings is string[] result))
                return null;

            return result;
        }

        /// <summary>
        /// Gets the specified property as an array of byte arrays from the SearchResultEntry
        /// Used for SIDHistory
        /// </summary>
        /// <param name="searchResultEntry"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static byte[][] GetPropertyAsArrayOfBytes(this SearchResultEntry searchResultEntry, string property)
        {
            var list = new List<byte[]>();
            if (!searchResultEntry.Attributes.Contains(property))
                return list.ToArray();

            var values = searchResultEntry.Attributes[property];
            var bytes = values.GetValues(typeof(byte[]));

            if (!(bytes is byte[][] result))
                return null;

            return result;
        }

        /// <summary>
        /// Gets the specified property as a byte array
        /// </summary>
        /// <param name="searchResultEntry"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static byte[] GetPropertyAsBytes(this SearchResultEntry searchResultEntry, string property)
        {
            if (!searchResultEntry.Attributes.Contains(property))
                return null;

            var collection = searchResultEntry.Attributes[property];
            var lookups = collection.GetValues(typeof(byte[]));
            if (lookups.Length == 0)
                return null;

            if (!(lookups[0] is byte[] bytes) || bytes.Length == 0)
                return null;

            return bytes;
        }

        /// <summary>
        /// Gets the objectsid/objectguid
        /// </summary>
        /// <param name="searchResultEntry"></param>
        /// <returns></returns>
        public static string GetObjectIdentifier(this SearchResultEntry searchResultEntry)
        {
            if (!searchResultEntry.Attributes.Contains("objectsid") &&
                !searchResultEntry.Attributes.Contains("objectguid"))
                return null;

            if (searchResultEntry.Attributes.Contains("objectsid"))
            {
                return searchResultEntry.GetSid();
            }

            if (searchResultEntry.Attributes.Contains("objectguid"))
            {
                var guidBytes = searchResultEntry.GetPropertyAsBytes("objectguid");

                return new Guid(guidBytes).ToString().ToUpper();
            }

            return null;
        }

        /// <summary>
        /// Extension method to determine the type of a SearchResultEntry.
        /// Requires objectsid, samaccounttype, objectclass
        /// </summary>
        /// <param name="searchResultEntry"></param>
        /// <returns></returns>
        public static LdapTypeEnum GetLdapType(this SearchResultEntry searchResultEntry)
        {
            var objectId = searchResultEntry.GetObjectIdentifier();
            if (objectId == null)
                return LdapTypeEnum.Unknown;

            if (searchResultEntry.GetPropertyAsBytes("msds-groupmsamembership") != null)
            {
                return LdapTypeEnum.User;
            }

            var objectType = LdapTypeEnum.Unknown;
            var samAccountType = searchResultEntry.GetProperty("samaccounttype");
            //Its not a common principal. Lets use properties to figure out what it actually is
            if (samAccountType != null)
            {
                if (samAccountType == "805306370")
                    return LdapTypeEnum.Unknown;

                objectType = Helpers.SamAccountTypeToType(samAccountType);
            }
            else
            {
                var objectClasses = searchResultEntry.GetPropertyAsArray("objectClass");
                if (objectClasses == null)
                {
                    objectType = LdapTypeEnum.Unknown;
                }
                else if (objectClasses.Contains("groupPolicyContainer"))
                {
                    objectType = LdapTypeEnum.GPO;
                }
                else if (objectClasses.Contains("organizationalUnit"))
                {
                    objectType = LdapTypeEnum.OU;
                }
                else if (objectClasses.Contains("domain"))
                {
                    objectType = LdapTypeEnum.Domain;
                }
            }

            //Override GMSA object type
            if (searchResultEntry.GetPropertyAsBytes("msds-groupmsamembership") != null)
                objectType = LdapTypeEnum.User;

            return objectType;
        }

        #endregion
    }
}