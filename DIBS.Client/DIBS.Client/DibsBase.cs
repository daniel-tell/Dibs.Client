using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DIBS.Client
{
    public abstract class DibsBase
    {
        public string GenerateHMAC(string key)
        {
            string message = GenereatePostMessage();
            return HMACGenerator.HashHMACHex(key, message);
        }

        private string GenereatePostMessage()
        {
            var properties = GetType().GetProperties();

            var message = properties
                .Where(property => !CheckIfIgnored(property) && property.GetValue(this) != null)
                .OrderBy(property => property.GetNameWithDibsCasing(), StringComparer.Ordinal)
                .Aggregate("", (msg, property) => msg + ("&" + property.GetNameWithDibsCasing() + "=" + property.GetValue(this)));

            if (message.Length > 0)
                message = message.TrimStart('&');

            return message;
        }

        private bool CheckIfIgnored(PropertyInfo propertyInfo)
        {
            bool ignore = Attribute.IsDefined(propertyInfo, typeof (IgnoreHashingAttribute));
            if (ignore)
            {
                ignore = CheckForIgnoreExceptions(propertyInfo);
            }
            return ignore;
        }

        private bool CheckForIgnoreExceptions(PropertyInfo propertyInfo)
        {
            var attribute =
                (IgnoreHashingAttribute)
                propertyInfo.GetCustomAttributes(typeof (IgnoreHashingAttribute), false).First();
            if (attribute.ValueIsSet)
            {
                string value = propertyInfo.GetValue(this).ToString();
                if (attribute.ExceptWhenValueIs == value)
                {
                    return false;
                }
            }
            return true;
        }

    }

    internal static class PropertyInfoExtension
    {
        internal static string GetNameWithDibsCasing(this PropertyInfo propertyInfo)
        {
            string name = propertyInfo.Name;

            if (Attribute.IsDefined(propertyInfo, typeof(CamelCaseAttribute)))
            {
                var sb = new StringBuilder();
                sb.Append(name[0].ToString().ToLower());

                foreach (char character in name.ToCharArray().Skip(1))
                {
                    sb.Append(character);
                }

                name = sb.ToString();
            }
            else
            {
                name = name.ToLower();
            }
            return name;
        }
    }
}