using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Com.Zoho.Officeintegrator.Exception;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This class is to process the upload file and stream.
    /// </summary>
    public class FormDataConverter : Converter
    {
        public FormDataConverter(CommonAPIHandler commonAPIHandler) : base(commonAPIHandler) {}

        private Dictionary<string, List<object>> uniqueValuesMap = new Dictionary<string, List<object>>();

        public override void AppendToRequest(HttpWebRequest requestBase, object requestObject)
        {
            string boundary = String.Format("----------{0:N}", Guid.NewGuid());
            requestBase.ContentType = "multipart/form-data; boundary=" + boundary;
            Stream fileDataStream = new MemoryStream();
            var boundarybytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            var endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--");
            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" + "Content-Type: application/octet-stream\r\n\r\n";
            if (requestObject is IDictionary)
            {
                this.AddFileBody(requestObject, fileDataStream, boundarybytes, endBoundaryBytes, headerTemplate);
            }
            fileDataStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            requestBase.ContentLength = fileDataStream.Length;
            using (Stream requestStream = requestBase.GetRequestStream())
            {
                fileDataStream.Position = 0;
                byte[] tempBuffer = new byte[fileDataStream.Length];
                fileDataStream.Read(tempBuffer, 0, tempBuffer.Length);
                fileDataStream.Close();
                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            }
        }

        private void AddFileBody(object requestObject, Stream fileDataStream, byte[] boundarybytes, byte[] endBoundaryBytes, string headerTemplate)
        {
            Dictionary<string, object> requestObjectMap = (Dictionary<string, object>)requestObject;
            foreach (KeyValuePair<string, object> requestData in requestObjectMap)
            {
                if (requestData.Value is IList)
                {
                    IList keysDetail = (IList)requestData.Value;
                    if(keysDetail.Count == 0)
                    {
                        continue;
                    }
                    if (keysDetail != null && keysDetail[0] is StreamWrapper)
                    {
                        foreach (object fileObject in keysDetail)
                        {
                            if (fileObject is StreamWrapper)
                            {
                                StreamWrapper streamWrapper = (StreamWrapper)fileObject;
                                fileDataStream.Write(boundarybytes, 0, boundarybytes.Length);
                                var header = string.Format(headerTemplate, requestData.Key, streamWrapper.Name);
                                var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                                fileDataStream.Write(headerbytes, 0, headerbytes.Length);
                                var buffer = new byte[1024];
                                var bytesRead = 0;
                                while ((bytesRead = streamWrapper.Stream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    fileDataStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                    else
                    {
                        string headerTemplate1 = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                        fileDataStream.Write(boundarybytes, 0, boundarybytes.Length);
                        var header = string.Format(headerTemplate1, requestData.Key, keysDetail);
                        var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                        fileDataStream.Write(headerbytes, 0, headerbytes.Length);
                    }
                }
                else if (requestData.Value is StreamWrapper)
                {
                    StreamWrapper streamWrapper = (StreamWrapper)requestData.Value;
                    fileDataStream.Write(boundarybytes, 0, boundarybytes.Length);
                    var header = string.Format(headerTemplate, requestData.Key, streamWrapper.Name);
                    var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                    fileDataStream.Write(headerbytes, 0, headerbytes.Length);
                    var buffer = new byte[1024];
                    var bytesRead = 0;
                    while ((bytesRead = streamWrapper.Stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        fileDataStream.Write(buffer, 0, bytesRead);
                    }
                }
                else
                {
                    string headerTemplate1 = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                    if (requestData.Value is IDictionary)
                    {
                        JObject json = new JObject(JsonConvert.SerializeObject(requestData.Value));
                        fileDataStream.Write(boundarybytes, 0, boundarybytes.Length);
                        var header = string.Format(headerTemplate1, requestData.Key, json);
                        var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                        fileDataStream.Write(headerbytes, 0, headerbytes.Length);
                    }
                    else
                    {
                        fileDataStream.Write(boundarybytes, 0, boundarybytes.Length);
                        var header = string.Format(headerTemplate1, requestData.Key, requestData.Value);
                        var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                        fileDataStream.Write(headerbytes, 0, headerbytes.Length);
                    }
                }
            }
        }

        public override object GetWrappedRequest(Object requestInstance, JObject pack)
        {
            string groupType = pack.GetValue(Constants.GROUP_TYPE).ToString();
            return FormRequest(requestInstance, requestInstance.GetType().FullName, null, null, groupType);
        }

        public override object FormRequest(object requestInstance, string pack, int? instanceNumber, JObject classMemberDetail, string groupType)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            if(!Initializer.jsonDetails.ContainsKey(pack))
            {
                return request;
            }
            JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(pack); // JSONdetails of class
            foreach (KeyValuePair<string, JToken> data in classDetail) // all members
            {
                object modification = null;
                string memberName = data.Key;
                MethodInfo method = null;
                JObject memberDetail = (JObject)classDetail.GetValue(memberName);
                bool found = false;
                if(memberDetail.ContainsKey(Constants.REQUEST_SUPPORTED) || !memberDetail.ContainsKey(Constants.NAME))
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
                try
                {
                    method = requestInstance.GetType().GetMethod(Constants.IS_KEY_MODIFIED);
                    object[] param = new object[1];
                    param[0] = memberDetail.GetValue(Constants.NAME).ToString();
                    modification = method.Invoke(requestInstance, param);
                }
                catch (System.Exception ex)
                {
                    throw new SDKException(Constants.EXCEPTION_IS_KEY_MODIFIED , ex);
                }
                // check required
                if ((modification == null || (int)modification == 0) && (memberDetail.ContainsKey(Constants.REQUIRED_FOR) && (memberDetail[Constants.REQUIRED_FOR].ToString().Equals(Constants.ALL) || memberDetail[Constants.REQUIRED_FOR].ToString().Equals(Constants.REQUEST))))
                {
                    throw new SDKException(Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR + memberName);
                }
                FieldInfo field = GetPrivateFieldInfo(requestInstance.GetType(), memberName);
                if (field != null)
                {
                    object fieldValue = field.GetValue(requestInstance);// value of the member
                    if (modification != null && (int)modification != 0 && fieldValue != null && this.ValueChecker(requestInstance.GetType().FullName, memberName, memberDetail, fieldValue, uniqueValuesMap, instanceNumber))
                    {
                        string keyName = (string)memberDetail.GetValue(Constants.NAME);
                        string type = (string)memberDetail.GetValue(Constants.TYPE);
                        string memberGroupType = memberDetail.ContainsKey(Constants.GROUP_TYPE) ? memberDetail.GetValue(Constants.GROUP_TYPE).ToString() : null;
                        if (type.Equals(Constants.LIST_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                        {
                            request.Add(keyName, SetJSONArray(fieldValue, memberDetail, memberGroupType));
                        }
                        else if (type.Equals(Constants.MAP_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                        {
                            request.Add(keyName, SetJSONObject(fieldValue, memberDetail));
                        }
                        else if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                        {
                            object fieldData = FormRequest(fieldValue, (string)memberDetail.GetValue(Constants.STRUCTURE_NAME), 1, memberDetail, memberGroupType);
                            request.Add(keyName, fieldData != null ? JToken.FromObject(fieldData) : JValue.CreateNull());
                        }
                        else
                        {
                            request.Add(keyName, fieldValue);
                        }
                    }
                }
            }

            return request;
        }

        private JObject IsNotRecordRequest(Object requestInstance, JObject classDetail, int instanceNumber, JObject classMemberDetail)
        {
            bool lookUp = false;
            bool skipMandatory = false;
            String classMemberName = null;
            if (classMemberDetail != null)
            {
                lookUp = (bool)classMemberDetail.GetValue(Constants.LOOKUP);
                classMemberName = this.BuildName(classMemberDetail.GetValue(Constants.NAME).ToString());
            }
            JObject requestJSON = new JObject();
            Dictionary<string, bool> requiredKeys = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, JToken> memberName in classDetail)
            {
                object modification = null;
                JObject memberDetail = (JObject) memberName.Value;
                bool found = false;
                if(memberDetail.ContainsKey(Constants.REQUEST_SUPPORTED) || !memberDetail.ContainsKey(Constants.NAME))
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
                string keyName = memberDetail.GetValue(Constants.NAME).ToString();
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
                if (memberDetail.ContainsKey(Constants.REQUIRED_FOR) && (memberDetail.GetValue(Constants.REQUIRED_FOR).ToString().Equals(Constants.ALL) || memberDetail.GetValue(Constants.REQUIRED_FOR).ToString().Equals(Constants.REQUEST)))
                {
                    requiredKeys.Add(keyName, true);
                }
                if (modification != null && (int) modification != 0)
                {
                    FieldInfo field = GetPrivateFieldInfo(requestInstance.GetType(), memberName.Key);
                    object fieldValue = field.GetValue(requestInstance);
                    if (fieldValue != null)
                    {
                        if (this.ValueChecker(requestInstance.GetType().FullName, memberName.Key, memberDetail, fieldValue, uniqueValuesMap, instanceNumber))
                        {
                            requiredKeys.Remove(keyName);
                        }
                        object recordData = SetData(memberDetail, fieldValue);
                        requestJSON.Add(keyName, recordData != null ? JToken.FromObject(recordData) : JValue.CreateNull());
                    }
                }
            }
            if (!skipMandatory)
            {
                CheckException(classMemberName, requestInstance, instanceNumber, requiredKeys);
            }
            return requestJSON;
        }

        private void CheckException(string memberName, object requestInstance, int instanceNumber, Dictionary<String, Boolean> requiredKeys)
        {
            if (this.commonAPIHandler.CategoryMethod.Equals(Constants.REQUEST_CATEGORY_CREATE, StringComparison.OrdinalIgnoreCase))
            {
                if (requiredKeys.Count > 0)
                {
                    JObject error = new JObject();
                    error.Add(Constants.FIELD, memberName);
                    error.Add(Constants.TYPE, requestInstance.GetType().FullName);
                    error.Add(Constants.KEYS, string.Join(",", requiredKeys.Keys));
                    error.Add(Constants.INSTANCE_NUMBER, instanceNumber);
                    throw new SDKException(Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR, error, null);
                }
            }
        }

        private object SetData(JObject memberDetail, Object fieldValue)
        {
            if (fieldValue != null)
            {
                string groupType = memberDetail.ContainsKey(Constants.GROUP_TYPE) ? memberDetail.GetValue(Constants.GROUP_TYPE).ToString() : null;
                string type = memberDetail.GetValue(Constants.TYPE).ToString();
                if (type.Equals(Constants.LIST_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                {
                    return SetJSONArray(fieldValue, memberDetail, groupType);
                }
                else if (type.Equals(Constants.MAP_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                {
                    return SetJSONObject(fieldValue, memberDetail);
                }
                else if (type.Equals(Constants.CHOICE_NAMESPACE) || (memberDetail.ContainsKey(Constants.STRUCTURE_NAME) && memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString().Equals(Constants.CHOICE_NAMESPACE)))
                {
                    Type t = fieldValue.GetType();
                    PropertyInfo prop = t.GetProperty("Value");
                    return prop.GetValue(fieldValue);
                }
                else if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                {
                    return IsNotRecordRequest(fieldValue, (JObject)Initializer.jsonDetails.GetValue(Constants.STRUCTURE_NAME), 0, memberDetail);
                }
                else
                {
                    Type t = Type.GetType(Constants.DATATYPECONVERTER.Replace(Constants._TYPE, type));
                    MethodInfo method = t.GetMethod(Constants.POST_CONVERT);
                    return method.Invoke(null, new object[] { fieldValue, type });
                }
            }
            return JValue.CreateNull();
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
                            return IsNotRecordRequest(fieldValue, members, 0, null);
                        }
                    }
                    else
                    {
                        foreach (var key in requestObject.Keys)
                        {
                            object data = RedirectorForObjectToJSON(requestObject[key]);
                            jsonObject.Add((string) key, data != null ? JToken.FromObject(data) : JValue.CreateNull());
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
                JObject extraDetail = (JObject) extraDetail1;
                if(!extraDetail.ContainsKey(Constants.MEMBERS))
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
                    if(extraDetail.ContainsKey(Constants.MEMBERS))
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

        private IList SetJSONArray(object fieldValue, JObject memberDetail, string groupType)
        {
            IList listObject = new List<object>();
            IList requestObjects = (IList)fieldValue;
            if (memberDetail == null)
            {
                foreach (object request in requestObjects)
                {
                    listObject.Add(RedirectorForObjectToJSON(request));
                }
            }
            else
            {
                if (memberDetail.ContainsKey(Constants.STRUCTURE_NAME))
                {
                    int instanceCount = 0;
                    string pack = (string)memberDetail.GetValue(Constants.STRUCTURE_NAME);
                    foreach (object request in requestObjects)
                    {
                        listObject.Add(IsNotRecordRequest(request, (JObject)Initializer.jsonDetails.GetValue(pack), ++instanceCount, memberDetail));
                    }
                }
                else
                {
                    foreach (object request in requestObjects)
                    {
                        listObject.Add(RedirectorForObjectToJSON(request));
                    }
                }
            }
            return listObject;
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
            else
            {
                return request;
            }
        }

        public override List<object> GetWrappedResponse(object response, JArray contents)
        {
            JObject pack;
            if(contents.Count == 1)
            {
                pack = (JObject)contents[0];
                if(pack.ContainsKey(Constants.GROUP_TYPE))
                {
                    List<Object> data = this.FindMatchAndParseResponseClass(contents, response);
                    if(data !=null)
                    {
                        pack = (JObject) data[0];
                        string groupType = pack.GetValue(Constants.GROUP_TYPE).ToString();
                        Dictionary<String, Object> responseData = (Dictionary<String, Object>) data[1];
                        return new List<object>() { GetResponse(responseData, FindMatchClass((JArray)pack.GetValue(Constants.CLASSES), responseData), groupType) };
                    }
                }
                else
                {
                    return new List<object>() { GetStreamInstance(response, ((JArray)pack.GetValue(Constants.CLASSES))[0].ToString())};
                }
            }
            return null;
        }

        private object GetResponse(Dictionary<String, Object> response, String packageName, String groupType)
        {
            JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(packageName);
            Object instance = null;
            if (classDetail.ContainsKey(Constants.INTERFACE) && (bool)classDetail.GetValue(Constants.INTERFACE))
            {
                JObject classDetail1 = (JObject)Initializer.jsonDetails.GetValue(packageName);
                JObject groupType1 = (JObject)classDetail1.GetValue(groupType);
                if (groupType1 != null)
                {
                    JArray classes = (JArray)groupType1.GetValue(Constants.CLASSES);
                    String className = FindMatchClass(classes, response);
                    instance = this.GetClassInstance(className);
                    this.GetMapResponse(instance, response, classDetail);
                }
            }
            else
            {
                instance = this.GetClassInstance(packageName);
                this.GetMapResponse(instance, response, classDetail);
            }
            return instance;
        }

        private void GetMapResponse(object instance, Dictionary<String, Object> response, JObject classDetail)
        {
            foreach (KeyValuePair<string,JToken> memberName in classDetail)
            {
                JObject keyDetail = (JObject)memberName.Value;
                string keyName = keyDetail.ContainsKey(Constants.NAME) ? keyDetail.ContainsKey(Constants.NAME).ToString() : null;
                if (keyName != null && response.ContainsKey(keyName) && response[keyName] != null)
                {
                    object keyData = response[keyName];
                    FieldInfo field = GetPrivateFieldInfo(instance.GetType(), memberName.Key);
                    object memberValue = GetData(keyData, keyDetail, field);
                    field.SetValue(instance, memberValue);
                }
            }
        }

        public override Object GetResponse(object response, string packageName, string groupType)
        {
            JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(packageName);
            JObject responseJson = GetJSONResponse(response);
            object instance = null;
            if(responseJson != null)
            {
                if (classDetail.ContainsKey(Constants.INTERFACE) && (bool)classDetail[Constants.INTERFACE])// if interface
                {
                    JObject classDetail1 = (JObject)Initializer.jsonDetails.GetValue(packageName);
                    JObject groupType1 = (JObject)classDetail1.GetValue(groupType);
                    if(groupType1 != null)
                    {
                        JArray classes = (JArray)groupType1.GetValue(Constants.CLASSES);
                        instance = FindMatch(classes, responseJson, groupType);// find match returns instance(calls getResponse() recursively)
                    }
                }
                else
                {
                    instance = this.GetClassInstance(packageName);
                    this.NotRecordResponse(instance, responseJson, classDetail);// based on json details data will be assigned
                }
            }
            return instance;
        }

        private void NotRecordResponse(Object instance, JObject responseJson, JObject classDetail)
        {
            foreach (KeyValuePair<string, JToken> memberName in classDetail)
            {
                JObject keyDetail = (JObject)memberName.Value;
                string keyName = keyDetail.ContainsKey(Constants.NAME) ? keyDetail.GetValue(Constants.NAME).ToString() : null;
                if ((keyName != null && !string.IsNullOrEmpty(keyName) && !string.IsNullOrWhiteSpace(keyName)) && responseJson.ContainsKey(keyName) && responseJson[keyName] != null)
                {
                    object keyData = responseJson[keyName];
                    FieldInfo field = GetPrivateFieldInfo(instance.GetType(), memberName.Key);
                    object memberValue = GetData(keyData, keyDetail, field);
                    field.SetValue(instance, memberValue);
                }
            }
        }

        private object GetData(Object keyData, JObject memberDetail, FieldInfo field)
        {
            object memberValue = null;
            if (keyData != null)
            {
                if ((keyData is JValue && ((JValue)keyData).Value == null) || keyData.ToString() == Constants.NULL_VALUE)
                {
                    return memberValue;
                }
                string groupType = memberDetail.ContainsKey(Constants.GROUP_TYPE) ? memberDetail.GetValue(Constants.GROUP_TYPE).ToString() : null;
                string type = (string)memberDetail[Constants.TYPE];
                if (keyData is Stream)
                {
                    return GetStreamInstance(keyData, type);
                }
                if (type.Equals(Constants.LIST_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                {
                    memberValue = GetCollectionsData((JArray) keyData, memberDetail, groupType);
                }
                else if (type.Equals(Constants.MAP_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                {
                    memberValue = GetMapData((JObject) keyData, memberDetail);
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
                    memberValue = GetResponse(keyData, memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString(), groupType);
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

        private List<Object> GetCollectionsData(JArray responses, JObject memberDetail, String groupType)
        {
            List<Object> values = new List<Object>();
            if (responses.Count > 0)
            {
                if (memberDetail == null)
                {
                    foreach (Object response in responses)
                    {
                        values.Add(RedirectorForJSONToObject(response));
                    }
                }
                else
                {
                    String specType = (String) memberDetail.GetValue(Constants.SPEC_TYPE);
                    if(groupType != null)
                    {
                        if(specType.Equals(Constants.TARRAY_TYPE))
                        {
                            return GetTArrayResponse(memberDetail, groupType, responses);
                        }
                        else
                        {
                            JObject orderedStructures = null;
                            if(memberDetail.ContainsKey(Constants.ORDERED_STRUCTURES))
                            {
                                orderedStructures = (JObject)memberDetail.GetValue(Constants.ORDERED_STRUCTURES);
                                if(orderedStructures.Count > responses.Count())
                                {
                                    return values;
                                }
                                foreach(KeyValuePair<string, JToken> orderedStructureValue in orderedStructures)
                                {
                                    JObject orderedStructure = (JObject)orderedStructureValue.Value;
                                    if (!orderedStructure.ContainsKey(Constants.MEMBERS))
                                    {
                                        values.Add(GetResponse(responses[orderedStructureValue.Key], orderedStructure.GetValue(Constants.STRUCTURE_NAME).ToString(), groupType));
                                    }
                                    else
                                    {
                                        if(orderedStructure.ContainsKey(Constants.MEMBERS))
                                        {
                                            values.Add(GetMapData((JObject)responses[orderedStructureValue.Key], (JObject)orderedStructure.GetValue(Constants.MEMBERS)));
                                        }
                                    }
                                }
                            }
                            if(groupType.Equals(Constants.ARRAY_OF) && memberDetail.ContainsKey(Constants.INTERFACE) && (bool)memberDetail.GetValue(Constants.INTERFACE))
                            {
                                String interfaceName = memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString();
                                JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(interfaceName);
                                JObject groupType1 = (JObject)classDetail.GetValue(Constants.ARRAY_OF);
                                if(groupType1 != null)
                                {
                                    JArray classes = (JArray)groupType1.GetValue(Constants.CLASSES);
                                    if(orderedStructures != null)
                                    {
                                        classes = ValidateInterfaceClass(orderedStructures, (JArray)groupType1.GetValue(Constants.CLASSES));
                                    }
                                    values.Add(GetArrayOfResponse(responses, classes, groupType)[0]);
                                }
                            }
                            else if(groupType.Equals(Constants.ARRAY_OF) && memberDetail.ContainsKey(Constants.EXTRA_DETAILS))
                            {
                                JArray extraDetails = (JArray)memberDetail.GetValue(Constants.EXTRA_DETAILS);
                                if(orderedStructures != null)
                                {
                                    extraDetails = ValidateStructure(orderedStructures, extraDetails);
                                }
                                int i = 0;
                                foreach(Object responseObject in responses)
                                {
                                    if(i == extraDetails.Count())
                                    {
                                        i = 0;
                                    }
                                    JObject extraDetail = (JObject)extraDetails[i];
                                    if(!extraDetail.ContainsKey(Constants.MEMBERS))
                                    {
                                        values.Add(GetResponse(responseObject, extraDetail.GetValue(Constants.STRUCTURE_NAME).ToString(), groupType));
                                    }
                                    else
                                    {
                                        if(extraDetail.ContainsKey(Constants.MEMBERS))
                                        {
                                            values.Add(GetMapData((JObject)responseObject, (JObject)extraDetail.GetValue(Constants.MEMBERS)));
                                        }
                                    }
                                    i++;
                                }
                            }
                            else
                            {
                                if(memberDetail.ContainsKey(Constants.INTERFACE) && (bool)memberDetail.GetValue(Constants.INTERFACE))
                                {
                                    if(orderedStructures != null)
                                    {
                                        String interfaceName = memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString();
                                        JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(interfaceName);
                                        JObject groupType1 = (JObject)classDetail.GetValue(Constants.ARRAY_OF);
                                        if(groupType1 != null)
                                        {
                                            JArray classes = ValidateInterfaceClass(orderedStructures, (JArray)groupType1.GetValue(Constants.CLASSES));
                                            foreach (Object response in responses)
                                            {
                                                String packName = FindMatchClass(classes, (JObject) response);
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
                                    if(memberDetail.ContainsKey(Constants.EXTRA_DETAILS))
                                    {
                                        JArray extraDetails = (JArray)memberDetail.GetValue(Constants.EXTRA_DETAILS);
                                        if(orderedStructures != null)
                                        {
                                            extraDetails = ValidateStructure(orderedStructures, extraDetails);
                                        }
                                        foreach (Object responseObject in responses)
                                        {
                                            JObject extraDetail = FindMatchExtraDetail(extraDetails, (JObject) responseObject);
                                            if(!extraDetail.ContainsKey(Constants.MEMBERS))
                                            {
                                                values.Add(GetResponse(responseObject, extraDetail.GetValue(Constants.STRUCTURE_NAME).ToString(), groupType));
                                            }
                                            else
                                            {
                                                if(extraDetail.ContainsKey(Constants.MEMBERS))
                                                {
                                                    values.Add(GetMapData((JObject)responseObject, (JObject)extraDetail.GetValue(Constants.MEMBERS)));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        String pack = (string)memberDetail.GetValue(Constants.STRUCTURE_NAME);
                                        foreach (Object response in responses)
                                        {
                                            values.Add(GetResponse(response, pack, groupType));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else// need to have structure Name in memberDetail
                    {
                        String pack = memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString();
                        if (pack.Equals(Constants.CHOICE_NAMESPACE))
                        {
                            foreach (Object response in responses)
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
            return values;
        }

        private List<Object> GetTArrayResponse(JObject memberDetail, String groupType, JArray responses)
        {
            List<Object> values = new List<Object>();
            if (memberDetail.ContainsKey(Constants.INTERFACE) && (Boolean)memberDetail.GetValue(Constants.INTERFACE) && memberDetail.ContainsKey(Constants.STRUCTURE_NAME))// if interface
            {
                JObject classDetail1 = (JObject)Initializer.jsonDetails.GetValue(memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString());
                JObject groupType1 = (JObject)classDetail1.GetValue(groupType);
                if(groupType1 != null)
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
                        values.Add(GetResponse(response, memberDetail.GetValue(Constants.STRUCTURE_NAME).ToString(), null));
                    }
                }
                else
                {
                    if (memberDetail.ContainsKey(Constants.EXTRA_DETAILS))
                    {
                        JArray extraDetails = (JArray)memberDetail.GetValue(Constants.EXTRA_DETAILS);
                        if(extraDetails != null && extraDetails.Count() > 0)
                        {
                            foreach (Object response in responses)
                            {
                                JObject extraDetail = FindMatchExtraDetail(extraDetails, (JObject) response);
                                if(!extraDetail.ContainsKey(Constants.MEMBERS))
                                {
                                    values.Add(GetResponse(response, extraDetail.GetValue(Constants.STRUCTURE_NAME).ToString(), null));
                                }
                                else
                                {
                                    if(extraDetail.ContainsKey(Constants.MEMBERS))
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

        private Dictionary<String, Object> GetMapData(JObject response, JObject memberDetail)
        {
            Dictionary<String, Object> mapInstance = new Dictionary<String, Object>();
            if (response.Count > 0)
            {
                if (memberDetail == null)
                {
                    IList<string> keys = response.Properties().Select(p => p.Name).ToList();
                    foreach (string key in keys)
                    {
                        mapInstance.Add(key, RedirectorForJSONToObject(response.GetValue(key)));
                    }
                }
                else
                {
                    IList<string> responseKeys = response.Properties().Select(p => p.Name).ToList();
                    if (memberDetail.ContainsKey(Constants.EXTRA_DETAILS))// if structure name is null the property add in extra_details.
                    {
                        JArray extraDetails = (JArray)memberDetail.GetValue(Constants.EXTRA_DETAILS);
                        JObject extraDetail = FindMatchExtraDetail(extraDetails, response);
                        if(extraDetail.ContainsKey(Constants.MEMBERS))
                        {
                            JObject memberDetails = (JObject)extraDetail.GetValue(Constants.MEMBERS);
                            foreach(string key in responseKeys)
                            {
                                if(memberDetails.ContainsKey(key))
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

        private Dictionary<String, Object> ParseMultipartResponse(object httpEntity)
        {
            Dictionary<String, Object> response = new Dictionary<String, Object>();
            //var parser = MultipartFormDataParser.Parse(stream);
            //FileItemFactory factory = new DiskFileItemFactory();
            //ServletFileUpload upload = new ServletFileUpload(factory);
            //List<FileItem> items = upload.parseRequest((RequestContext) httpEntity);
            //for (FileItem item : items)
            //{
            //    if (item.isFormField())
            //    {
            //        response.Add(item.getFieldName(), item.getString());
            //    }
            //    else
            //    {
            //        response.Add(item.getName(), item.getInputStream());
            //    }
            //}
            return response;
        }

        public List<object> FindMatchAndParseResponseClass(JArray contents, Object response)
        {
            HttpWebResponse responseEntity = ((HttpWebResponse)response);
            Dictionary<String, Object> responseDict = ParseMultipartResponse(responseEntity.GetResponseStream());
            if(responseDict.Count > 0)
            {
                float ratio = 0;
                int structure = 0;
                for(int i = 0; i < contents.Count ; i++)
                {
                    JObject content = (JObject) contents[i];
                    float ratio1 = 0;
                    JArray classes;
                    if (content.ContainsKey(Constants.INTERFACE) && (bool)content.GetValue(Constants.INTERFACE))
                    {
                        String interfaceName = (String)((JArray)content.GetValue(Constants.CLASSES))[0];
                        JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(interfaceName);
                        JObject groupType1 = (JObject)classDetail.GetValue(content.GetValue(Constants.GROUP_TYPE).ToString());
                        if(groupType1 == null)
                        {
                            return null;
                        }
                        classes = (JArray)groupType1.GetValue(Constants.CLASSES);
                    }
                    else
                    {
                        classes = (JArray)content.GetValue(Constants.CLASSES);
                    }
                    if(classes == null || classes.Count() > 0)
                    {
                        return null;
                    }
                    foreach (Object className in classes)
                    {
                        float matchRatio = FindRatio((String) className, responseDict);
                        if (matchRatio == 1.0)
                        {
                            return new List<object>() { (JObject)contents[i], responseDict };
                        }
                        else if (matchRatio > ratio1)
                        {
                            ratio1 = matchRatio;
                        }
                    }
                    if(ratio < ratio1)
                    {
                        structure = i;
                    }
                }
                return new List<object>() { (JObject)contents[structure], responseDict };
            }
            return null;
        }

        private Object FindMatch(JArray classes, JObject responseJson, String groupType)
        {
            if(classes.Count == 1)
            {
                return GetResponse(responseJson, classes[0].ToString(), groupType);
            }
            String pack = "";
            float ratio = 0;
            foreach (Object className in classes)
            {
                float matchRatio = FindRatio((String) className, responseJson);
                if (matchRatio == 1.0)
                {
                    pack = (String) className;
                    break;
                }
                else if (matchRatio > ratio)
                {
                    pack = (String) className;
                    ratio = matchRatio;
                }
            }
            return GetResponse(responseJson, pack, groupType);
        }

        public List<Object> GetArrayOfResponse(object responseObject, JArray classes, String groupType)
        {
            JArray responseArray = this.GetJSONArrayResponse(responseObject);
            if (responseArray == null)
            {
                return null;
            }
            int i = 0;
            List<Object> responseClass = new List<Object>();
            foreach(Object responseArray1 in responseArray)
            {
                if(i == classes.Count)
                {
                    i = 0;
                }
                responseClass.Add(GetResponse(responseArray1, classes[i].ToString(), groupType));
                i++;
            }
            return new List<object>() { responseClass, responseArray };
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
            foreach(Type genericArgument in type.GenericTypeArguments)
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
    }
}
