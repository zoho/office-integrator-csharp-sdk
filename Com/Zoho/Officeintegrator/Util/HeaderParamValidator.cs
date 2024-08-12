using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.Officeintegrator.Util
{
    public class HeaderParamValidator<T>
    {
        private readonly JObject jsonDetails = Initializer.jsonDetails;

        public string Validate(string name, string className, T value)
        {
            className = Utility.GetClassName(className);
            if (jsonDetails.ContainsKey(className))
            {
                JObject classObject = (JObject)jsonDetails.GetValue(className);
                foreach (KeyValuePair<string, JToken> data in classObject)
                {
                    JObject memberDetail = (JObject)data.Value;
                    string keyName = (string)memberDetail.GetValue(Constants.NAME);
                    if (name.Equals(keyName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                        {
                            if (value is IList list)
                            {
                                JArray jsonArray = new JArray();
							    IList requestObjects = list;
                                if (requestObjects.Count > 0)
                                {
                                    foreach (Object requestObject in requestObjects)
                                    {
                                        jsonArray.Add(new JSONConverter(null).FormRequest(requestObject, (string)memberDetail.GetValue(Constants.STRUCTURE_NAME), null, null, null));
                                    }
                                }
                                return JsonConvert.SerializeObject(jsonArray).ToString();
                            }
                            return JsonConvert.SerializeObject(new JSONConverter(null).FormRequest(value, (string)memberDetail.GetValue(Constants.STRUCTURE_NAME), null, null, null)).ToString();
                        }
                        object parseData = ParseData(value);
                        if(parseData is IDictionary || parseData is IList)
                        {
                            return JsonConvert.SerializeObject(parseData);
                        }
                        return parseData.ToString();
                    }
                }
            }
            return GetValue(value).ToString();
        }

        public object GetValue(object value)
        {
            if (value is Boolean)
            {
                return value.ToString().ToLowerInvariant();
            }
            string type = value.GetType().FullName;
            Type dataTypeConverter = Type.GetType(Constants.DATATYPECONVERTER.Replace(Constants._TYPE, type));
            MethodInfo method = dataTypeConverter.GetMethod(Constants.POST_CONVERT);
            return method.Invoke(null, new object[] { value, type });
        }

        public object ParseData(object value)
        {
            if (value is IDictionary dictionary)
            {
                JObject jsonObject = new JObject();
                IDictionary requestObject = dictionary;
                if (requestObject.Count > 0)
                {
                    foreach (var key in requestObject.Keys)
                    {
                        object data = ParseData(requestObject[key]);
                        jsonObject.Add((string)key, data != null ? JToken.FromObject(data) : JValue.CreateNull());
                    }
                }
                return jsonObject;
            }
            else if (value is IList list)
            {
                JArray jsonArray = new JArray();
                IList requestObjects = list;
                if (requestObjects.Count > 0)
                {
                    foreach (object requestObject in requestObjects)
                    {
                        jsonArray.Add(ParseData(requestObject));
                    }
                }
                return jsonArray;
            }
            else
            {
                return GetValue(value).ToString();
            }
        }
    }
}
