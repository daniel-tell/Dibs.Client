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
            PropertyInfo[] propertyInfos = GetType().GetProperties();

            var message = propertyInfos
                .Where(x => !CheckIfIgnored(x))
                .ToDictionary(k => k, FixCasing)
                .OrderBy(x => x.Value, StringComparer.Ordinal)
                .Aggregate("", (current, propertyPair) => current + ("&" + propertyPair.Value + "=" + propertyPair.Key.GetValue(this)));

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

        private static string FixCasing(PropertyInfo propertyInfo)
        {
            string name = propertyInfo.Name;

            if (Attribute.IsDefined(propertyInfo, typeof (CamelCaseAttribute)))
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