using System;
using System.Collections.Generic;
using Com.Zoho.Officeintegrator.Exception;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This class handles module field details.
    /// </summary>
    public class Utility
    {
        public static void AssertNotNull(object value, string errorCode, string errorMessage)
        {
            if(value == null)
            {
                throw new SDKException(errorCode, errorMessage);
            }
        }

        public static JObject GetJSONObject(JObject json, string key)
        {
            foreach(var entry in json)
            {
                string keyInJSON = entry.Key;
                if(keyInJSON.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return (JObject)entry.Value;
                }
            }
            return null;
        }


        public static string GetClassName(string canonicalName)
        {
            string[] packages = canonicalName.Split('.');
            List<string> fileName = new List<string>();
            for (int i = 0; i < packages.Length; i++)
            {
                string name = packages[i];
                string packageName = Capitalize(name);
                if (packageName != null && !canonicalName.Contains("java."))
                {
                    if (packageName.Equals("api", StringComparison.OrdinalIgnoreCase))
                    {
                        packageName = packageName.ToUpper();
                    }
                    fileName.Add(packageName);
                }
                else
                {
                    if (name.Equals("api", StringComparison.OrdinalIgnoreCase))
                    {
                        name = name.ToUpper();
                    }
                    fileName.Add(name);
                }
            }
            return string.Join(".", fileName);
        }

        public static string Capitalize(string str)
        {
            if (str == null || string.IsNullOrEmpty(str))
            {
                return str;
            }
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }
    }
}
