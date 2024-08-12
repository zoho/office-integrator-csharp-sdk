using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Com.Zoho.Officeintegrator.Exception;
using Com.Zoho.Officeintegrator.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This class processes the API response object to the POJO object and POJO object to a JSON object.
    /// </summary>
    public class JSONConverter : Converter
    {
        public JSONConverter(CommonAPIHandler commonAPIHandler) : base(commonAPIHandler) { }

        private Dictionary<string, List<object>> uniqueValuesMap = new Dictionary<string, List<object>>();

        public override void AppendToRequest(HttpWebRequest requestBase, object requestObject)
        {
            string dataString = requestObject.ToString();
            var data = Encoding.UTF8.GetBytes(dataString);
            int dataLength = data.Length;
            requestBase.ContentLength = dataLength;
            using (var writer = requestBase.GetRequestStream())
            {
                writer.Write(data, 0, dataLength);
            }
        }

        public override object GetWrappedRequest(Object requestInstance, JObject pack)
        {
            string groupType = (string)pack.GetValue(Constants.GROUP_TYPE);
            if (groupType.Equals(Constants.ARRAY_OF))
            {
                if (pack.ContainsKey(Constants.INTERFACE) && (bool)pack.GetValue(Constants.INTERFACE))
                {
                    if (requestInstance is IList)
                    {
                        IList requestObjects = (IList)requestInstance;
                        if (requestObjects.Count > 0)
                        {
                            JArray jsonArray = new JArray();
                            int instanceCount = 0;
                            foreach (Object request in requestObjects)
                            {
                                jsonArray.Add(FormRequest(request, request.GetType().FullName, instanceCount, null, groupType));
                                instanceCount++;
                            }
                            return jsonArray;
                        }
                    }
                    else
                    {
                        return FormRequest(requestInstance, requestInstance.GetType().FullName, null, null, groupType);
                    }
                }
                else
                {
                    return FormRequest(requestInstance, requestInstance.GetType().FullName, null, null, groupType);
                }
            }
            else
            {
                return FormRequest(requestInstance, requestInstance.GetType().FullName, null, null, groupType);
            }
            return null;
        }

        public override object FormRequest(object requestInstance, string pack, int? instanceNumber, JObject memberDetail, String groupType)
        {
            JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(pack); // JSONdetails of class
            if (classDetail.ContainsKey(Constants.INTERFACE) && (bool)classDetail.GetValue(Constants.INTERFACE))
            {
                JObject groupType1 = (JObject)classDetail.GetValue(groupType);
                if (groupType1 != null)
                {
                    JArray classes = (JArray)groupType1[Constants.CLASSES];
                    string requestObjectClassName = requestInstance.GetType().FullName;
                    foreach (object className in classes)
                    {
                        if (Convert.ToString(className).Equals(requestObjectClassName, StringComparison.OrdinalIgnoreCase))
                        {
                            classDetail = (JObject)Initializer.jsonDetails.GetValue(requestObjectClassName);
                            break;
                        }
                    }
                }
            }
            return IsNotRecordRequest(requestInstance, classDetail, instanceNumber, memberDetail);
        }

        private JObject IsNotRecordRequest(object requestInstance, JObject classDetail, int? instanceNumber, JObject classMemberDetail)
        {
            bool lookUp = false;
            bool skipMandatory = false;
            string classMemberName = null;
            if (classMemberDetail != null)
            {
                lookUp = classMemberDetail.ContainsKey(Constants.LOOKUP) ? (bool)classMemberDetail[Constants.LOOKUP] : false;
                string name = classMemberDetail.ContainsKey(Constants.NAME) ? classMemberDetail[Constants.NAME].ToString() : "";
                classMemberName = BuildName(name);
            }

            JObject requestJSON = new JObject();
            Dictionary<string, bool> requiredKeys = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, JToken> data in classDetail) // all members
            {
                string memberName = data.Key;
                object modification = null;
                JObject memberDetail = (JObject)data.Value;
                bool found = false;
                if (memberDetail.ContainsKey(Constants.REQUEST_SUPPORTED) || !memberDetail.ContainsKey(Constants.NAME))
                {
                    JArray requestSupported = (JArray)memberDetail.GetValue(Constants.REQUEST_SUPPORTED);
                    for (int i = 0; i < requestSupported.Count; i++)
                    {
                        if (requestSupported[i].ToString().Equals(this.commonAPIHandler.CategoryMethod.ToLower()))
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    continue;
                }
                string keyName = (string)memberDetail[Constants.NAME];
                try
                {
                    MethodInfo isKeyModified = requestInstance.GetType().GetMethod(Constants.IS_KEY_MODIFIED);
                    object[] param = new object[1];
                    param[0] = memberDetail.GetValue(Constants.NAME).ToString();
                    modification = isKeyModified.Invoke(requestInstance, param);
                }
                catch (System.Exception ex)
                {
                    throw new SDKException(Constants.EXCEPTION_IS_KEY_MODIFIED, ex);
                }
                if (memberDetail.ContainsKey(Constants.REQUIRED_FOR) && (memberDetail[Constants.REQUIRED_FOR].ToString().Equals(Constants.ALL) || memberDetail[Constants.REQUIRED_FOR].ToString().Equals(Constants.REQUEST)))
                {
                    requiredKeys.Add(keyName, true);
                }
                object fieldValue = null;
                if (modification != null && (int)modification != 0)
                {
                    FieldInfo field = GetPrivateFieldInfo(requestInstance.GetType(), memberName);
                    fieldValue = field.GetValue(requestInstance);// value of the member
                    if (fieldValue != null)
                    {
                        if (this.ValueChecker(requestInstance.GetType().FullName, memberName, memberDetail, fieldValue, uniqueValuesMap, instanceNumber) == true)// set if not null
                        {
                            requiredKeys.Remove(keyName);
                        }
                    }
                    object recordData = SetData(memberDetail, fieldValue);
                    requestJSON.Add(keyName, recordData != null ? JToken.FromObject(recordData) : JValue.CreateNull());
                }
            }
            if (!skipMandatory)
            {
                CheckException(classMemberName, requestInstance, instanceNumber, lookUp, requiredKeys);
            }
            return requestJSON;
        }

        private void CheckException(string memberName, object requestInstance, int? instanceNumber, bool lookUp, Dictionary<string, bool> requiredKeys)
        {
            if (this.commonAPIHandler != null && this.commonAPIHandler.CategoryMethod.Equals(Constants.REQUEST_CATEGORY_CREATE, StringComparison.OrdinalIgnoreCase))
            {
                if (requiredKeys.Count > 0)
                {
                    JObject error = new JObject();
                    error.Add(Constants.FIELD, memberName);
                    error.Add(Constants.TYPE, requestInstance.GetType().FullName);
                    error.Add(Constants.KEYS, string.Join(",", requiredKeys.Keys));
                    if (instanceNumber != null)
                    {
                        error.Add(Constants.INSTANCE_NUMBER, instanceNumber);
                    }
                    throw new SDKException(Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR, error, null);
                }
            }
        }

        private object SetData(JObject memberDetail, object fieldValue)
        {
            if (fieldValue != null)
            {
                string type = (string)memberDetail[Constants.TYPE];
                return SetDataValue(type, memberDetail, fieldValue);
            }
            return JValue.CreateNull();
        }

        private Object SetDataValue(String type, JObject memberDetail, Object fieldValue)
        {
            string groupType = memberDetail.ContainsKey(Constants.GROUP_TYPE) ? (string)memberDetail.GetValue(Constants.GROUP_TYPE) : null;
            if (type.Equals(Constants.LIST_NAMESPACE, StringComparison.OrdinalIgnoreCase))
            {
                return SetJSONArray(fieldValue, memberDetail, groupType);
            }
            else if (type.Equals(Constants.MAP_NAMESPACE, StringComparison.OrdinalIgnoreCase))
            {
                return SetJSONObject(fieldValue, memberDetail);
            }
            else if (type.Equals(Constants.CHOICE_NAMESPACE) || (memberDetail.ContainsKey(Constants.STRUCTURE_NAME) && ((string)memberDetail[Constants.STRUCTURE_NAME]).Equals(Constants.CHOICE_NAMESPACE)))
            {
                Type t = fieldValue.GetType();
                PropertyInfo prop = t.GetProperty("Value");
                return prop.GetValue(fieldValue);
            }
            else if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
            {
                return FormRequest(fieldValue, (string)memberDetail[Constants.STRUCTURE_NAME], null, memberDetail, groupType);
            }
            else
            {
                Type t = Type.GetType(Constants.DATATYPECONVERTER.Replace(Constants._TYPE, type));
                MethodInfo method = t.GetMethod(Constants.POST_CONVERT);
                return method.Invoke(null, new object[] { fieldValue, type });
            }
        }

        private JObject SetJSONObject(object fieldValue, JObject memberDetail)
        {
            JObject jsonObject = new JObject();
            IDictionary requestObject = (IDictionary)fieldValue;
            if (requestObject.Count > 0)
            {
                if (memberDetail == null)
                {
                    foreach (var key in requestObject.Keys)
                    {
                        object data = RedirectorForObjectToJSON(requestObject[key]);
                        jsonObject.Add((string)key, data != null ? JToken.FromObject(data) : JValue.CreateNull());
                    }
                }
                else
                {
                    if (memberDetail.ContainsKey(Constants.EXTRA_DETAILS))
                    {
                        JArray extraDetails = (JArray)memberDetail.GetValue(Constants.EXTRA_DETAILS);
                        if (extraDetails != null && extraDetails.Count > 0)
                        {
                            JObject members = GetValidStructure(extraDetails, requestObject.Keys);
                            return IsNotRecordRequest(fieldValue, members, null, null);
                        }
                        else
                        {
                            object keyValue = null;
                            foreach (string keyName in requestObject.Keys)
                            {
                                keyValue = requestObject[keyName];
                                object data = RedirectorForObjectToJSON(keyValue);
                                jsonObject.Add(keyName, data != null ? JToken.FromObject(data) : JValue.CreateNull());
                            }
                        }
                    }
                }
            }
            return jsonObject;
        }

        private JObject GetValidStructure(JArray extraDetails, ICollection keys)
        {
            foreach (Object extraDetail1 in extraDetails)
            {
                JObject extraDetail = (JObject)extraDetail1;
                if (!extraDetail.ContainsKey(Constants.MEMBERS))
                {
                    JObject members = (JObject)Initializer.jsonDetails.GetValue(extraDetail.GetValue(Constants.TYPE).ToString());
                    IList<string> keys1 = members.Properties().Select(p => p.Name).ToList();
                    if (keys.Equals(keys1))
                    {
                        return members;
                    }
                }
                else
                {
                    if (extraDetail.ContainsKey(Constants.MEMBERS))
                    {
                        JObject members = (JObject)extraDetail.GetValue(Constants.MEMBERS);
                        IList<string> keys1 = members.Properties().Select(p => p.Name).ToList();
                        if (keys.Equals(keys1))
                        {
                            return members;
                        }
                    }
                }
            }
            return null;
        }

        private JArray SetJSONArray(object fieldValue, JObject memberDetail, string groupType)
        {
            JArray jsonArray = new JArray();
            IList requestObjects = (IList)fieldValue;
            if (requestObjects.Count > 0)
            {
                if (memberDetail == null || (memberDetail != null && !memberDetail.ContainsKey(Constants.STRUCTURE_NAME)))
                {
                    if (memberDetail != null && memberDetail.ContainsKey(Constants.SUB_TYPE))
                    {
                        JObject subType = (JObject)memberDetail.GetValue(Constants.SUB_TYPE);
                        String type = subType.GetValue(Constants.TYPE).ToString();
                        if (type.Equals(Constants.CHOICE_NAMESPACE))
                        {
                            foreach (Object response in requestObjects)
                            {
                                Type t = response.GetType();
                                PropertyInfo prop = t.GetProperty("Value");
                                jsonArray.Add(prop.GetValue(response));
                            }
                        }
                        else
                        {
                            foreach (Object response in requestObjects)
                            {
                                jsonArray.Add(SetDataValue(type, memberDetail, response));
                            }
                        }
                    }
                    else
                    {
                        foreach (object request in requestObjects)
                        {
                            jsonArray.Add(RedirectorForObjectToJSON(request));
                        }
                    }
                }
                else
                {
                    if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                    {
                        string pack = (string)memberDetail.GetValue(Constants.STRUCTURE_NAME);
                        if (pack.Equals(Constants.CHOICE_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (object request in requestObjects)
                            {
                                FieldInfo field = GetPrivateFieldInfo(request.GetType(), "value");
                                jsonArray.Add(field.GetValue(request));
                            }
                        }
                        else
                        {
                            int instanceCount = 0;
                            foreach (object request in requestObjects)
                            {
                                jsonArray.Add(FormRequest(request, pack, instanceCount, memberDetail, groupType));
                                instanceCount++;
                            }
                        }
                    }
                    else
                    {
                        int instanceCount = 0;
                        foreach (Object request in requestObjects)
                        {
                            if (request is IDictionary)
                            {
                                JArray extraDetails = (JArray)memberDetail.GetValue(Constants.EXTRA_DETAILS);
                                if (extraDetails != null && extraDetails.Count > 0)
                                {
                                    JObject members = GetValidStructure(extraDetails, ((IDictionary)request).Keys);
                                    jsonArray.Add(IsNotRecordRequest(request, members, null, null));
                                }
                                else
                                {
                                    jsonArray.Add(RedirectorForObjectToJSON(request));
                                }
                            }
                            else
                            {
                                jsonArray.Add(FormRequest(request, request.GetType().FullName, instanceCount, memberDetail, groupType));
                            }
                            instanceCount++;
                        }
                    }
                }
            }
            return jsonArray;
        }

        private object RedirectorForObjectToJSON(object request)
        {
            if (request is IList)
            {
                return SetJSONArray(request, null, null);
            }
            else if (request is IDictionary)
            {
                return SetJSONObject(request, null);
            }
            else if (request.GetType().FullName.Equals(Constants.CHOICE_NAMESPACE))
            {
                Type t = request.GetType();
                PropertyInfo prop = t.GetProperty("Value");
                return prop.GetValue(request);
            }
            else
            {
                return request;
            }
        }

        public override List<object> GetWrappedResponse(object response, JArray contents)
        {
            HttpWebResponse responseEntity = ((HttpWebResponse)response);
            string responseString = new StreamReader(responseEntity.GetResponseStream()).ReadToEnd();
            responseEntity.Close();
            if (responseString != null && !string.IsNullOrEmpty(responseString) && !string.IsNullOrWhiteSpace(responseString))
            {
                // convert string to stream
                byte[] byteArray = Encoding.UTF8.GetBytes(responseString);
                MemoryStream stream = new MemoryStream(byteArray);
                JsonTextReader responseStream = new JsonTextReader(new StreamReader(stream));
                JObject pack;
                if (contents.Count() == 1)
                {
                    pack = (JObject)contents[0];
                }
                else
                {
                    pack = FindMatchResponseClass(contents, responseStream);
                }
                if (pack != null)
                {
                    String groupType = pack.GetValue(Constants.GROUP_TYPE).ToString();
                    if (groupType.Equals(Constants.ARRAY_OF))
                    {
                        JsonSerializer serializer = new JsonSerializer
                        {
                            DateParseHandling = DateParseHandling.None
                        };
                        JArray responseArray = serializer.Deserialize<JArray>(responseStream);
                        responseArray = GetJSONArrayResponse(responseArray);
                        if (pack.ContainsKey(Constants.INTERFACE) && (bool)pack.GetValue(Constants.INTERFACE))
                        {
                            String interfaceName = (String)((JArray)pack.GetValue(Constants.CLASSES))[0];
                            JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(interfaceName);
                            JObject groupType1 = (JObject)classDetail.GetValue(Constants.ARRAY_OF);
                            if (groupType1 != null)
                            {
                                return GetArrayOfResponse(responseArray, (JArray)groupType1.GetValue(Constants.CLASSES), groupType);
                            }
                        }
                        else
                        {
                            return GetArrayOfResponse(responseArray, (JArray)pack.GetValue(Constants.CLASSES), groupType);
                        }
                    }
                    else
                    {
                        JsonSerializer serializer = new JsonSerializer
                        {
                            DateParseHandling = DateParseHandling.None
                        };
                        JObject responseJSON = serializer.Deserialize<JObject>(responseStream);
                        responseJSON = GetJSONResponse(responseJSON);
                        if (pack.ContainsKey(Constants.INTERFACE) && (bool)pack.GetValue(Constants.INTERFACE))// if interface
                        {
                            string interfaceName = (string)pack.GetValue(Constants.CLASSES)[0];
                            return new List<object>() { GetResponse(responseJSON, interfaceName, groupType), responseJSON };
                        }
                        else
                        {
                            string packName = FindMatchClass((JArray)pack.GetValue(Constants.CLASSES), responseJSON);
                            return new List<object>() { GetResponse(responseJSON, packName, groupType), responseJSON };
                        }
                    }
                }
            }
            return null;
        }

        public override object GetResponse(object response, string packageName, string groupType)
        {
            JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(packageName); // JSONdetails of class
            JObject responseJson = GetJSONResponse(response);
            object instance = null;
            if (responseJson != null)
            {
                if (classDetail.ContainsKey(Constants.INTERFACE) && (bool)classDetail[Constants.INTERFACE])// if interface
                {
                    JObject classDetail1 = (JObject)Initializer.jsonDetails.GetValue(packageName);
                    JObject groupType1 = (JObject)classDetail1.GetValue(groupType);
                    if (groupType1 != null)
                    {
                        JArray classes = (JArray)groupType1[Constants.CLASSES];
                        instance = FindMatch(classes, responseJson, groupType);// find match returns instance(calls getResponse() recursively)
                    }
                }
                else
                {
                    try
                    {
                        // create an instance of that type
                        instance = Activator.CreateInstance(Type.GetType(packageName));
                    }
                    catch (MissingMethodException) //when there is no public constructor- invoke the private constructor
                    {
                        instance = Activator.CreateInstance(Type.GetType(packageName), true);
                    }
                    instance = NotRecordResponse(instance, responseJson, classDetail);// based on json details data will be assigned
                }
            }
            return instance;
        }

        private object NotRecordResponse(object instance, JObject responseJson, JObject classDetail)
        {
            foreach (KeyValuePair<string, JToken> member in classDetail)
            {
                string memberName = member.Key;
                JObject keyDetail = (JObject)classDetail[memberName];// JSONdetails of the member
                string keyName = keyDetail.ContainsKey(Constants.NAME) ? (string)keyDetail[Constants.NAME] : null;// api-name of the member
                if ((keyName != null && !string.IsNullOrEmpty(keyName) && !string.IsNullOrWhiteSpace(keyName)) && responseJson.ContainsKey(keyName) && responseJson[keyName] != null)
                {
                    object keyData = responseJson[keyName];
                    FieldInfo field = GetPrivateFieldInfo(instance.GetType(), memberName);
                    object memberValue = GetData(keyData, keyDetail, field);
                    field.SetValue(instance, memberValue);
                }
            }
            return instance;
        }

        private object GetData(object keyData, JObject memberDetail, FieldInfo field)
        {
            object memberValue = null;
            string groupType = memberDetail.ContainsKey(Constants.GROUP_TYPE) ? memberDetail.GetValue(Constants.GROUP_TYPE).ToString() : null;
            if (keyData != null)
            {
                if ((keyData is JValue && ((JValue)keyData).Value == null) || keyData.ToString() == Constants.NULL_VALUE)
                {
                    return memberValue;
                }
                string type = (string)memberDetail[Constants.TYPE];
                if (type.Equals(Constants.LIST_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                {
                    memberValue = GetCollectionsData((JArray)keyData, memberDetail, groupType);
                }
                else if (type.Equals(Constants.MAP_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                {
                    memberValue = GetMapData((JObject)keyData, memberDetail);
                }
                else if (type.Equals(Constants.CHOICE_NAMESPACE) || (memberDetail.ContainsKey(Constants.STRUCTURE_NAME) && ((string)memberDetail[Constants.STRUCTURE_NAME]).Equals(Constants.CHOICE_NAMESPACE)))
                {
                    if (field != null && field.FieldType.FullName.Contains(Constants.CSHARP_NULL_TYPE_NAME))
                    {
                        Type t = Type.GetType(CSharpName(field.FieldType));
                        memberValue = Activator.CreateInstance(field.FieldType, ChangeType(((JValue)keyData).Value, t));
                    }
                    else
                    {
                        string valueType = ((JValue)keyData).Value.GetType().FullName;
                        Type t = Type.GetType(Constants.CHOICE.Replace(Constants._TYPE, valueType));
                        memberValue = Activator.CreateInstance(t, ((JValue)keyData).Value);
                    }
                }
                else if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                {
                    memberValue = GetResponse(keyData, (string)memberDetail[Constants.STRUCTURE_NAME], groupType);
                }
                else
                {
                    Type t = Type.GetType(Constants.DATATYPECONVERTER.Replace(Constants._TYPE, type));
                    MethodInfo method = t.GetMethod(Constants.PRE_CONVERT);
                    memberValue = method.Invoke(null, new object[] { keyData, type });
                }
            }
            return memberValue;
        }


        public static object ChangeType(object value, Type conversion)
        {
            var t = conversion;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }
                t = Nullable.GetUnderlyingType(t);
            }
            return Convert.ChangeType(value, t);
        }


        public static string CSharpName(Type type)
        {
            var sb = new StringBuilder();
            var name = type.Name;
            if (!type.IsGenericType) return name;
            foreach (Type genericArgument in type.GenericTypeArguments)
            {
                var sb1 = new StringBuilder();
                sb1.Append(genericArgument.Namespace).Append(".").Append(genericArgument.Name);
                if (sb1.ToString().Equals(Constants.CSHARP_NULL_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Type genericArgument1 in genericArgument.GenericTypeArguments)
                    {
                        sb.Append(genericArgument1.Namespace).Append(".").Append(genericArgument1.Name);
                    }
                }
            }
            return sb.ToString();
        }

        private object GetCollectionsData(JArray responses, JObject memberDetail, string groupType)
        {
            List<object> values = new List<object>();
            if (responses.Count > 0)
            {
                if (memberDetail == null)
                {
                    foreach (object response in responses)
                    {
                        values.Add(RedirectorForJSONToObject(response));
                    }
                }
                else// need to have structure Name in memberDetail
                {
                    String specType = (String)memberDetail.GetValue(Constants.SPEC_TYPE);
                    if (groupType != null)
                    {
                        if (specType.Equals(Constants.TARRAY_TYPE))
                        {
                            return GetTArrayResponse(memberDetail, groupType, responses);
                        }
                        else
                        {
                            JObject orderedStructures = null;
                            if (memberDetail.ContainsKey(Constants.ORDERED_STRUCTURES))
                            {
                                orderedStructures = (JObject)memberDetail.GetValue(Constants.ORDERED_STRUCTURES);
                                if (orderedStructures.Count > responses.Count)
                                {
                                    return values;
                                }
                                IList<string> keys1 = orderedStructures.Properties().Select(p => p.Name).ToList();
                                foreach (String index in keys1)
                                {
                                    JObject orderedStructure = (JObject)orderedStructures.GetValue(index);
                                    if (!orderedStructure.ContainsKey(Constants.MEMBERS))
                                    {
                                        values.Add(GetResponse((JObject)responses[index], orderedStructure.GetValue(Constants.STRUCTURE_NAME).ToString(), groupType));
                                    }
                                    else
                                    {
                                        if (orderedStructure.ContainsKey(Constants.MEMBERS))
                                        {
                                            values.Add(GetMapData((JObject)responses[index], (JObject)orderedStructure.GetValue(Constants.MEMBERS)));
                                        }
                                    }
                                }
                            }
                            if (groupType.Equals(Constants.ARRAY_OF) && memberDetail.ContainsKey(Constants.INTERFACE) && (bool)memberDetail.GetValue(Constants.INTERFACE))
                            {
                                String interfaceName = memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString();
                                JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(interfaceName);
                                JObject groupType1 = (JObject)classDetail.GetValue(Constants.ARRAY_OF);
                                if (groupType1 != null)
                                {
                                    JArray classes = (JArray)groupType1.GetValue(Constants.CLASSES);
                                    if (orderedStructures != null)
                                    {
                                        classes = ValidateInterfaceClass(orderedStructures, (JArray)groupType1.GetValue(Constants.CLASSES));
                                    }
                                    values.Add(GetArrayOfResponse(responses, classes, groupType)[0]);
                                }
                            }
                            else if (groupType.Equals(Constants.ARRAY_OF) && memberDetail.ContainsKey(Constants.EXTRA_DETAILS))
                            {
                                JArray extraDetails = (JArray)memberDetail.GetValue(Constants.EXTRA_DETAILS);
                                if (orderedStructures != null)
                                {
                                    extraDetails = ValidateStructure(orderedStructures, extraDetails);
                                }
                                int i = 0;
                                foreach (Object responseObject in responses)
                                {
                                    if (i == extraDetails.Count())
                                    {
                                        i = 0;
                                    }
                                    JObject extraDetail = (JObject)extraDetails[i];
                                    if (!extraDetail.ContainsKey(Constants.MEMBERS))
                                    {
                                        values.Add(GetResponse(responseObject, extraDetail.GetValue(Constants.STRUCTURE_NAME).ToString(), groupType));
                                    }
                                    else
                                    {
                                        if (extraDetail.ContainsKey(Constants.MEMBERS))
                                        {
                                            values.Add(GetMapData((JObject)responseObject, (JObject)extraDetail.GetValue(Constants.MEMBERS)));
                                        }
                                    }
                                    i++;
                                }
                            }
                            else
                            {
                                if (memberDetail.ContainsKey(Constants.INTERFACE) && (bool)memberDetail.GetValue(Constants.INTERFACE))
                                {
                                    if (orderedStructures != null)
                                    {
                                        String interfaceName = memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString();
                                        JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(interfaceName);
                                        JObject groupType1 = (JObject)classDetail.GetValue(Constants.ARRAY_OF);
                                        if (groupType1 != null)
                                        {
                                            JArray classes = ValidateInterfaceClass(orderedStructures, (JArray)groupType1.GetValue(Constants.CLASSES));
                                            foreach (Object response in responses)
                                            {
                                                String packName = FindMatchClass(classes, (JObject)response);
                                                values.Add(GetResponse(response, packName, groupType));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (Object response in responses)
                                        {
                                            values.Add(GetResponse(response, memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString(), groupType));
                                        }
                                    }
                                }
                                else
                                {
                                    if (memberDetail.ContainsKey(Constants.EXTRA_DETAILS))
                                    {
                                        JArray extraDetails = (JArray)memberDetail.GetValue(Constants.EXTRA_DETAILS);
                                        if (orderedStructures != null)
                                        {
                                            extraDetails = ValidateStructure(orderedStructures, extraDetails);
                                        }
                                        foreach (Object responseObject in responses)
                                        {
                                            JObject extraDetail = FindMatchExtraDetail(extraDetails, (JObject)responseObject);
                                            if (!extraDetail.ContainsKey(Constants.MEMBERS))
                                            {
                                                values.Add(GetResponse(responseObject, extraDetail.GetValue(Constants.STRUCTURE_NAME).ToString(), groupType));
                                            }
                                            else
                                            {
                                                if (extraDetail.ContainsKey(Constants.MEMBERS))
                                                {
                                                    values.Add(GetMapData((JObject)responseObject, (JObject)extraDetail.GetValue(Constants.MEMBERS)));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        String pack = null;
                                        if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                                        {
                                            pack = (string)memberDetail.GetValue(Constants.STRUCTURE_NAME);
                                        }
                                        else if (memberDetail.ContainsKey(Constants.SUB_TYPE))
                                        {
                                            pack = ((JObject)memberDetail.GetValue(Constants.SUB_TYPE)).GetValue(Constants.TYPE).ToString();
                                        }
                                        if (pack != null)
                                        {
                                            foreach (Object response in responses)
                                            {
                                                values.Add(GetResponse(response, pack, groupType));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string pack = null;
                        if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                        {
                            pack = (string)memberDetail[Constants.STRUCTURE_NAME];
                        }
                        else if (memberDetail.ContainsKey(Constants.SUB_TYPE))
                        {
                            pack = ((JObject)memberDetail.GetValue(Constants.SUB_TYPE)).GetValue(Constants.TYPE).ToString();
                        }
                        if (pack.Equals(Constants.CHOICE_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (object response in responses)
                            {
                                JToken keyData = (JToken)response;
                                JTokenType tokenType = keyData.Type;
                                if (response.GetType().Name.Equals("JValue", StringComparison.OrdinalIgnoreCase))
                                {
                                    Type type = Type.GetType(Constants.CHOICE.Replace(Constants._TYPE, GetType(tokenType)));
                                    values.Add(Activator.CreateInstance(type, ((JValue)response).Value));
                                }
                                else
                                {
                                    Type type = Type.GetType(Constants.CHOICE.Replace(Constants._TYPE, GetType(tokenType)));
                                    values.Add(Activator.CreateInstance(type, ((JValue)response).Value));
                                }
                            }
                        }
                        else
                        {
                            foreach (Object response in responses)
                            {
                                values.Add(GetResponse(response, pack, null));
                            }
                        }
                    }
                }
            }
            IList list = null;
            if (values is List<Object> && memberDetail != null)
            {
                String pack = null;
                if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                {
                    pack = (string)memberDetail.GetValue(Constants.STRUCTURE_NAME);
                }
                else if (memberDetail.ContainsKey(Constants.SUB_TYPE))
                {
                    pack = ((JObject)memberDetail.GetValue(Constants.SUB_TYPE)).GetValue(Constants.TYPE).ToString();
                }
                List<Object> list1 = (List<Object>)values;
                string listTypeName = memberDetail[Constants.TYPE] + "[" + pack + "]";
                string type = list1.Count > 0 ? list1[0].GetType().FullName : null;
                string type1 = type;
                if (type != null && (type.Contains("JValue") || (pack != null && pack.ToString().Equals(Constants.CHOICE_NAMESPACE, StringComparison.OrdinalIgnoreCase))))
                {
                    type1 = type;
                    if (type.Contains("JValue"))
                    {
                        JToken keyData = (JToken)list1[0];
                        JTokenType tokenType = keyData.Type;
                        if (memberDetail[Constants.TYPE].Contains("Choice"))
                        {
                            type1 = Constants.CHOICE.Replace(Constants._TYPE, GetType(tokenType));
                        }
                        else
                        {
                            type1 = GetType(tokenType);
                        }
                    }
                    listTypeName = memberDetail[Constants.TYPE] + "[" + type1 + "]";
                }
                Type t = Type.GetType(listTypeName);
                if (list == null && t != null && (pack != null || type1 != null))
                {
                    list = (IList)Activator.CreateInstance(t);
                }
                foreach (Object record in list1)
                {
                    JValue value = record is JValue ? (JValue)record : null;
                    if (record != null && (value != null && value.Value == null) && record.GetType().Name.Equals("JValue"))
                    {
                        JToken keyData = (JToken)list1[0];
                        JTokenType tokenType = keyData.Type;
                        if (record.GetType().Name.Equals("JValue", StringComparison.OrdinalIgnoreCase))
                        {
                            t = Type.GetType(Constants.CHOICE.Replace(Constants._TYPE, GetType(tokenType)));
                            list.Add(Activator.CreateInstance(t, ((JValue)record).Value));
                        }
                        else
                        {
                            t = Type.GetType(Constants.CHOICE.Replace(Constants._TYPE, GetType(tokenType)));
                            list.Add(Activator.CreateInstance(t, ((JValue)record).Value));
                        }
                    }
                    else
                    {
                        if (pack == null)
                        {
                            Type dataTypeConverter = Type.GetType(Constants.DATATYPECONVERTER.Replace(Constants._TYPE, type1));
                            MethodInfo method = dataTypeConverter.GetMethod(Constants.PRE_CONVERT);
                            list.Add(method.Invoke(null, new Object[] { record, type }));
                        }
                        else
                        {
                            list.Add(record);
                        }
                    }
                }
                return list;
            }
            return values;
        }

        private object GetMapData(JObject response, JObject memberDetail)
        {
            Dictionary<string, object> mapInstance = new Dictionary<string, object>();
            if (response.Count > 0)
            {
                if (memberDetail == null)
                {
                    foreach (KeyValuePair<string, JToken> responseData in response)
                    {
                        mapInstance.Add(responseData.Key, RedirectorForJSONToObject(responseData.Value));
                    }
                }
                else
                {
                    IList<string> responseKeys = response.Properties().Select(p => p.Name).ToList();
                    if (memberDetail.ContainsKey(Constants.EXTRA_DETAILS))
                    {
                        JArray extraDetails = (JArray)memberDetail[Constants.EXTRA_DETAILS];
                        JObject extraDetail = FindMatchExtraDetail(extraDetails, response);
                        if (extraDetail.ContainsKey(Constants.MEMBERS))
                        {
                            JObject memberDetails = (JObject)extraDetail.GetValue(Constants.MEMBERS);
                            foreach (String key in responseKeys)
                            {
                                if (memberDetails.ContainsKey(key))
                                {
                                    JObject memberDetail1 = (JObject)memberDetails.GetValue(key);
                                    mapInstance.Add(memberDetail1.GetValue(Constants.NAME).ToString(), GetData(response.GetValue(key), memberDetail1, null));
                                }
                            }
                        }
                    }
                }
            }
            return mapInstance;
        }

        private object RedirectorForJSONToObject(object keyData)
        {
            if (keyData is JObject)
            {
                return GetMapData((JObject)keyData, null);
            }
            else if (keyData is JArray)
            {
                return GetCollectionsData((JArray)keyData, null, null);
            }
            else
            {
                return keyData;
            }
        }

        private object FindMatch(JArray classes, JObject responseJson, String groupType)
        {
            if (classes.Count() == 1)
            {
                return GetResponse(responseJson, classes[0].ToString(), groupType);
            }
            string pack = "";
            float ratio = 0;
            foreach (string className in classes)
            {
                float matchRatio = FindRatio(className, responseJson);
                if (matchRatio == 1.0)
                {
                    pack = (string)className;
                    ratio = 1;
                    break;
                }
                else if (matchRatio > ratio)
                {
                    ratio = matchRatio;
                    pack = (string)className;
                }
            }
            return GetResponse(responseJson, pack, groupType);
        }

        private List<Object> GetTArrayResponse(JObject memberDetail, String groupType, JArray responses)
        {
            List<Object> values = new List<Object>();
            if (memberDetail.ContainsKey(Constants.INTERFACE) && (bool)memberDetail.GetValue(Constants.INTERFACE) && memberDetail.ContainsKey(Constants.STRUCTURE_NAME))// if interface
            {
                JObject classDetail1 = (JObject)Initializer.jsonDetails.GetValue(memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString());
                JObject groupType1 = (JObject)classDetail1.GetValue(groupType);
                if (groupType1 != null)
                {
                    String className = FindMatchClass((JArray)groupType1.GetValue(Constants.CLASSES), (JObject)responses[0]);
                    foreach (Object response in responses)
                    {
                        values.Add(GetResponse(response, className, null));
                    }
                }
            }
            else
            {
                if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                {
                    foreach (Object response in responses)
                    {
                        values.Add(GetResponse(response, (string)memberDetail.GetValue(Constants.STRUCTURE_NAME), null));
                    }
                }
                else
                {
                    if (memberDetail.ContainsKey(Constants.EXTRA_DETAILS))
                    {
                        JArray extraDetails = (JArray)memberDetail.GetValue(Constants.EXTRA_DETAILS);
                        if (extraDetails != null && extraDetails.Count > 0)
                        {
                            foreach (Object response in responses)
                            {
                                JObject extraDetail = FindMatchExtraDetail(extraDetails, (JObject)response);
                                if (!extraDetail.ContainsKey(Constants.MEMBERS))
                                {
                                    values.Add(GetResponse(response, extraDetail.GetValue(Constants.STRUCTURE_NAME).ToString(), null));
                                }
                                else
                                {
                                    if (extraDetail.ContainsKey(Constants.MEMBERS))
                                    {
                                        values.Add(GetMapData((JObject)response, (JObject)extraDetail.GetValue(Constants.MEMBERS)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return values;
        }

        public List<object> GetArrayOfResponse(JArray responseArray, JArray classes, String groupType)
        {
            if (responseArray == null)
            {
                return null;
            }
            int i = 0;
            List<Object> responseClass = new List<Object>();
            foreach (Object responseArray1 in responseArray)
            {
                if (i == classes.Count)
                {
                    i = 0;
                }
                responseClass.Add(GetResponse(responseArray1, classes[i].ToString(), groupType));
                i++;
            }
            return new List<object>() { responseClass, responseArray };
        }

        public static string GetProperMethodName(string methodName)
        {
            string method = "";

            if (!string.IsNullOrEmpty(methodName))
            {
                if (methodName[0].Equals("_"))
                {
                    method = char.ToUpper(methodName[1]) + methodName.Substring(2);
                }
                else if (methodName.Contains("_"))
                {
                    string[] splits = methodName.Split('_');

                    for (int i = 0; i < splits.Length; i++)
                    {
                        method += char.ToUpper(splits[i][0]) + splits[i].Substring(1);
                    }
                }
                else
                {
                    method += char.ToUpper(methodName[0]) + methodName.Substring(1);
                }
            }
            return method;
        }

        public static object ConvertList(List<object> value, Type type)
        {
            IList list = (IList)Activator.CreateInstance(type);
            foreach (var item in value)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
