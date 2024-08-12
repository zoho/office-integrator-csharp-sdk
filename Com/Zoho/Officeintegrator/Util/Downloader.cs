using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This class is to process the download file and stream response.
    /// </summary>
    public class Downloader : Converter
    {
        public Downloader(CommonAPIHandler commonAPIHandler) : base(commonAPIHandler) {}

        public override object GetWrappedRequest(object response, JObject pack)
        {
            return null;
        }

        public override object FormRequest(object requestInstance, string pack, int? instanceNumber, JObject memberDetail, string groupType)
        {
            throw new NotImplementedException();
        }

        public override void AppendToRequest(HttpWebRequest requestBase, object requestObject)
        {
            throw new NotImplementedException();
        }

        public override List<object> GetWrappedResponse(object response, JArray contents)
        {
            if (contents.Count() >= 1)
            {
                JObject pack = (JObject)contents[0];
                if (pack.ContainsKey(Constants.INTERFACE) && (bool)pack[Constants.INTERFACE])
                {
                    JArray classes = (JArray)pack.GetValue(Constants.CLASSES);
                    return new List<object>() { GetResponse(response, classes[0].ToString(), pack[Constants.GROUP_TYPE].ToString()) };
                }
                else
                {
                    JArray classes = (JArray)pack.GetValue(Constants.CLASSES);
                    String className = classes[0].ToString();
                    if (className.Contains(Constants.FILEBODYWRAPPER))
                    {
                        return new List<object>() { GetResponse(response, className, null) };
                    }
                    return new List<object>() { GetStreamInstance(response, className) };
                }
            }
            return null;
        }

        private object GetStreamInstance(Object response, string type)
        {
            HttpWebResponse responseEntity = ((HttpWebResponse)response);
            WebHeaderCollection collection = responseEntity.Headers;
            string contentDisposition = collection[Constants.CONTENT_DISPOSITION];
            string fileName = contentDisposition.Split(new string[] { "=" }, StringSplitOptions.None)[1];
            if (fileName.Contains("''"))
            {
                fileName = fileName.Split(new string[] { "''" }, StringSplitOptions.None)[1];
            }
            fileName = fileName.Trim('\"');
            return Activator.CreateInstance(Type.GetType(type), new object[] { fileName, responseEntity.GetResponseStream() });
        }

        public override object GetResponse(object response, string pack, String groupType)
        {
            JObject recordJsonDetails = (JObject)Initializer.jsonDetails.GetValue(pack); // JSONdetails of class
            object instance = null;
            if (recordJsonDetails.ContainsKey(Constants.INTERFACE) && (bool)recordJsonDetails.GetValue(Constants.INTERFACE))
            {
                if (recordJsonDetails.ContainsKey(groupType))
                {
                    JToken groupType1 = recordJsonDetails[groupType];
                    if (groupType1 != null)
                    {
                        JObject groupType11 = (JObject)groupType1;
                        JArray classes = (JArray)groupType11.GetValue(Constants.CLASSES);
                        foreach (object classObject in classes)
                        {
                            string className = classObject.ToString();
                            if (className.Contains(Constants.FILEBODYWRAPPER))
                            {
                                return GetResponse(response, className, null);
                            }
                        }
                    }
                }
                return instance;
            }
            else
            {
                instance = Activator.CreateInstance(Type.GetType(pack));
                foreach (KeyValuePair<string, JToken> memberName in recordJsonDetails)
                {
                    JObject memberJsonDetails = (JObject)recordJsonDetails[memberName.Key];// JSONdetails of the member
                    FieldInfo field = GetPrivateFieldInfo(instance.GetType(), memberName.Key);
                    if (field != null)
                    {
                        string type = (string)memberJsonDetails[Constants.TYPE];
                        if (type.Equals(Constants.STREAM_WRAPPER_CLASS_PATH, StringComparison.OrdinalIgnoreCase))
                        {
                            HttpWebResponse responseEntity = ((HttpWebResponse)response);
                            WebHeaderCollection collection = responseEntity.Headers;
                            string contentDisposition = collection[Constants.CONTENT_DISPOSITION];
                            string fileName = contentDisposition.Split(new string[] { "=" }, StringSplitOptions.None)[1];
                            if (fileName.Contains("''"))
                            {
                                fileName = fileName.Split(new string[] { "''" }, StringSplitOptions.None)[1];
                            }
                            fileName = fileName.Trim('\"');
                            object instanceValue = Activator.CreateInstance(Type.GetType(type), new object[] { fileName, responseEntity.GetResponseStream() });
                            field.SetValue(instance, instanceValue);
                        }
                    }
                }
            }
            return instance;
        }
    }
}
