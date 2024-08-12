using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Com.Zoho.Officeintegrator.Exception;
using Com.Zoho.Officeintegrator.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This abstract class is to construct API request and response.
    /// </summary>
    public abstract class Converter
    {
        protected CommonAPIHandler commonAPIHandler;

        /// <summary>
        /// Creates a Converter class instance with no parameters.
        /// </summary>
        public Converter()
        {
        }

        /// <summary>
        /// Creates a Converter class instance with the CommonAPIHandler class instance.
        /// </summary>
        /// <param name="commonAPIHandler">A CommonAPIHandler class instance.</param>
        public Converter(CommonAPIHandler commonAPIHandler)
        {
            this.commonAPIHandler = commonAPIHandler;
        }

        public abstract object GetResponse(object response, string pack, string groupType);

        public abstract object GetWrappedRequest(object response, JObject pack);

        public abstract object FormRequest(object requestInstance, string pack, int? instanceNumber, JObject memberDetail, string groupType);

        /// <summary>
        /// This abstract method is to construct the API request body.
        /// </summary>
        /// <param name="requestBase">A HttpWebRequest class instance.</param>
        /// <param name="requestObject">A object containing the API request body object.</param>
        public abstract void AppendToRequest(HttpWebRequest requestBase, object requestObject);

        public abstract List<object> GetWrappedResponse(object response, JArray pack);

        /// <summary>
        /// This method is to validate if the input values satisfy the constraints for the respective fields.
        /// </summary>
        /// <param name="className"> A string containing the class name.</param>
        /// <param name="memberName">A string containing the member name.</param>
        /// <param name="keyDetails">A JObject containing the key JSON details.</param>
        /// <param name="value">A object containing the key value.</param>
        /// <param name="uniqueValuesMap">A Dictionary&lt;string,List&lt;object&gt;&gt; containing the construct objects.</param>
        /// <param name="instanceNumber">An int containing the POJO class instance list number.</param>
        /// <returns>A bool representing the key value is expected pattern, unique, length, and values.</returns>
        public bool ValueChecker(string className, string memberName, JObject keyDetails, object value, Dictionary<string, List<object>> uniqueValuesMap, int? instanceNumber)
        {
            JObject detailsJO = new JObject();
            string name = keyDetails.GetValue(Constants.NAME).ToString();
            string type = keyDetails.GetValue(Constants.TYPE).ToString();
            string varType = null;
            bool check = true;
            if (value != null)
            {
                var valueType = value.GetType();
                if (value is IList)
                {
                    varType = valueType.Namespace + "." + valueType.Name;
                }
                else
                {
                    varType = valueType.Namespace + "." + valueType.Name.Replace("`1", "");
                }
                check = varType.Equals(type, StringComparison.OrdinalIgnoreCase);
                if (!check)
                {
                    System.Type minterface = valueType.GetInterface(type);
                    if (minterface != null)
                    {
                        if (value is IList)
                        {
                            varType = minterface.Namespace + "." + minterface.Name;
                        }
                        else
                        {
                            varType = minterface.Namespace + "." + minterface.Name.Replace("`1", "");
                        }

                        check = varType.Equals(type, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            if (value is IList)
            {
                bool expectedListType = true;
                if (keyDetails.ContainsKey(Constants.STRUCTURE_NAME))
                {
                    string structureName = keyDetails.GetValue(Constants.STRUCTURE_NAME).ToString();
                    int index = 0;
                    IList listValue = (IList)value;
                    foreach (object data in listValue)
                    {
                        var dataType = data.GetType();
                        if (data is IList)
                        {
                            className = dataType.Namespace + "." + dataType.Name;
                        }
                        else
                        {
                            className = dataType.Namespace + "." + dataType.Name.Replace("`1", "");
                        }
                        check = className.Equals(structureName, StringComparison.OrdinalIgnoreCase);
                        if (!check)
                        {
                            System.Type minterface = dataType.GetInterface(structureName);
                            if (minterface != null)
                            {
                                if (data is IList)
                                {
                                    className = minterface.Namespace + "." + minterface.Name;
                                }
                                else
                                {
                                    className = minterface.Namespace + "." + minterface.Name.Replace("`1", "");
                                }
                                check = className.Equals(structureName, StringComparison.OrdinalIgnoreCase);
                            }
                        }
                        if (!check)
                        {
                            instanceNumber = index;
                            type = Constants.LIST_NAMESPACE + "(" + structureName + ")";
                            varType = Constants.LIST_NAMESPACE + "(" + className + ")";
                            expectedListType = false;
                            break;
                        }
                        index++;
                    }
                }
                if (expectedListType)
                {
                    check = varType.Equals(type, StringComparison.OrdinalIgnoreCase) ? true : varType.Equals(Constants.LIST_NAMESPACE);
                }
            }
            else if (value is IDictionary)
            {
                check = varType.Equals(type) ? true : varType.Equals(Constants.MAP_NAMESPACE, StringComparison.OrdinalIgnoreCase);
            }
            if (!check && !type.Equals(Constants.CSHARP_OBJECT_NAME, StringComparison.OrdinalIgnoreCase))
            {
                detailsJO.Add(Constants.FIELD, memberName);
                detailsJO.Add(Constants.CLASS, className);
                detailsJO.Add(Constants.INDEX, instanceNumber);
                detailsJO.Add(Constants.EXPECTED_TYPE, type);
                detailsJO.Add(Constants.GIVEN_TYPE, varType);
                throw new SDKException(Constants.TYPE_ERROR, detailsJO);
            }
            if (keyDetails.ContainsKey(Constants.VALUES) && (!keyDetails.ContainsKey(Constants.PICKLIST) || ((bool)keyDetails[Constants.PICKLIST] && Initializer.GetInitializer().SDKConfig.PickListValidation)))
            {
                JArray valuesJArray = (JArray)keyDetails.GetValue(Constants.VALUES);
                string[] pickListValue = valuesJArray.ToObject<string[]>();
                if (value is IList)
                {
                    bool isNotExist = false;
                    if (value is List<Choice<string>> stringValueList)
                    {
                        isNotExist = false;
                        foreach (Choice<string> value_2 in stringValueList)
                        {
                            if (!CheckPickListValue(value_2, pickListValue))
                            {
                                isNotExist = true;
                                break;
                            }
                        }
                    }
                    else if (value is List<Choice<long>> longValueList)
                    {
                        isNotExist = false;
                        foreach (Choice<long> value_2 in longValueList)
                        {
                            if (!CheckPickListValue(value_2, pickListValue))
                            {
                                isNotExist = true;
                                break;
                            }
                        }
                    }
                    else if (value is List<Choice<int>> intValueList)
                    {
                        isNotExist = false;
                        foreach (Choice<int> value_2 in intValueList)
                        {
                            if (!CheckPickListValue(value_2, pickListValue))
                            {
                                isNotExist = true;
                                break;
                            }
                        }
                    }
                    else if (value is List<Choice<bool>> boolValueList)
                    {
                        isNotExist = false;
                        foreach (Choice<bool> value_2 in boolValueList)
                        {
                            if (!CheckPickListValue(value_2, pickListValue))
                            {
                                isNotExist = true;
                                break;
                            }
                        }
                    }
                    if (isNotExist)
                    {
                        detailsJO.Add(Constants.FIELD, memberName);
                        detailsJO.Add(Constants.CLASS, className);
                        detailsJO.Add(Constants.INDEX, instanceNumber);
                        detailsJO.Add(Constants.GIVEN_VALUE, JToken.FromObject(value));
                        detailsJO.Add(Constants.ACCEPTED_VALUES, valuesJArray);
                        throw new SDKException(Constants.UNACCEPTED_VALUES_ERROR, detailsJO);
                    }
                }
                else
                {
                    if (value.GetType().FullName.Contains(Constants.CHOICE_NAME))
                    {
                        value = GetChoiceValue(value);
                    }
                    if (!pickListValue.Contains(Convert.ToString(value)))
                    {
                        detailsJO.Add(Constants.FIELD, memberName);
                        detailsJO.Add(Constants.CLASS, className);
                        if (instanceNumber != null)
                        {
                            detailsJO.Add(Constants.INDEX, instanceNumber);
                        }
                        detailsJO.Add(Constants.GIVEN_VALUE, JToken.FromObject(value));
                        detailsJO.Add(Constants.ACCEPTED_VALUES, valuesJArray);
                        throw new SDKException(Constants.UNACCEPTED_VALUES_ERROR, detailsJO);
                    }
                }
            }
            if (keyDetails.ContainsKey(Constants.UNIQUE_FOR) && (keyDetails.GetValue(Constants.UNIQUE_FOR).ToString().Equals(Constants.ALL, StringComparison.OrdinalIgnoreCase) || keyDetails.GetValue(Constants.UNIQUE_FOR).ToString().Equals(Constants.REQUEST, StringComparison.OrdinalIgnoreCase)))
            {
                List<object> valuesArray = uniqueValuesMap.ContainsKey(name) ? uniqueValuesMap[name] : null;
                if (valuesArray != null && valuesArray.Contains(value))
                {
                    detailsJO.Add(Constants.FIELD, memberName);
                    detailsJO.Add(Constants.CLASS, className);
                    detailsJO.Add(Constants.FIRST_INDEX, valuesArray.IndexOf(value));
                    if (instanceNumber != null)
                    {
                        detailsJO.Add(Constants.NEXT_INDEX, instanceNumber);
                    }
                    throw new SDKException(Constants.UNIQUE_KEY_ERROR, detailsJO);
                }
                else
                {
                    if (valuesArray == null)
                    {
                        valuesArray = new List<object>();
                    }
                    valuesArray.Add(value);
                    uniqueValuesMap[name] = valuesArray;
                }
            }
            if (keyDetails.ContainsKey(Constants.MIN_LENGTH) || keyDetails.ContainsKey(Constants.MAX_LENGTH))
            {
                int count = value.ToString().Length;
                if (value is IList)
                {
                    count = ((IList)value).Count;
                }
                if (keyDetails.ContainsKey(Constants.MAX_LENGTH) && count > (int)keyDetails.GetValue(Constants.MAX_LENGTH))
                {
                    detailsJO.Add(Constants.FIELD, memberName);
                    detailsJO.Add(Constants.CLASS, className);
                    detailsJO.Add(Constants.GIVEN_LENGTH, count);
                    detailsJO.Add(Constants.MAXIMUM_LENGTH, keyDetails.GetValue(Constants.MAX_LENGTH));
                    throw new SDKException(Constants.MAXIMUM_LENGTH_ERROR, detailsJO);
                }
                if (keyDetails.ContainsKey(Constants.MIN_LENGTH) && count < (int)keyDetails.GetValue(Constants.MIN_LENGTH))
                {
                    detailsJO.Add(Constants.FIELD, memberName);
                    detailsJO.Add(Constants.CLASS, className);
                    detailsJO.Add(Constants.GIVEN_LENGTH, count);
                    detailsJO.Add(Constants.MINIMUM_LENGTH, keyDetails.GetValue(Constants.MIN_LENGTH));
                    throw new SDKException(Constants.MINIMUM_LENGTH_ERROR, detailsJO);
                }
            }
            return true;
        }

        private object GetChoiceValue(object value)
        {
            Type type = value.GetType();
            PropertyInfo prop = type.GetProperty("Value");
            return prop.GetValue(value);
        }

        public static FieldInfo GetPrivateFieldInfo(System.Type type, string fieldName)
        {
            BindingFlags Flags = BindingFlags.Instance
                                            | BindingFlags.GetProperty
                                            | BindingFlags.SetProperty
                                            | BindingFlags.GetField
                                            | BindingFlags.SetField
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Public;

            FieldInfo[] fields = type.GetFields(Flags);
            return fields.FirstOrDefault(fieldInfo => fieldInfo.Name == fieldName);
        }

        public static string GetType(JTokenType tokenType)
        {
            string type = "";
            if (tokenType == JTokenType.Object)
            {
                type = Constants.MAP_NAMESPACE;
            }
            else if (tokenType == JTokenType.Array)
            {
                type = Constants.LIST_NAMESPACE;
            }
            else if (tokenType == JTokenType.String)
            {
                type = Constants.CSHARP_STRING_NAME;
            }
            else if (tokenType == JTokenType.Boolean)
            {
                type = Constants.CSHARP_BOOLEAN_NAME;
            }
            else if (tokenType == JTokenType.Float)
            {
                type = Constants.CSHARP_DOUBLE_NAME;
            }
            else if (tokenType == JTokenType.Integer)
            {
                type = Constants.CSHARP_INT_NAME;
            }
            return type;
        }

        private bool CheckPickListValue(object value_2, string[] pickListValue)
        {
            object value_3 = value_2;
            if (value_2.GetType().FullName.Contains(Constants.CHOICE_NAME))
            {
                value_3 = GetChoiceValue(value_2);
            }
            if (!pickListValue.Contains(Convert.ToString(value_3)))
            {
                return false;
            }
            return true;
        }

        public object GetClassInstance(string packageName)
        {
            object instance = null;
            try
            {
                // create an instance of that type
                instance = Activator.CreateInstance(Type.GetType(packageName));
            }
            catch (MissingMethodException) //when there is no public constructor- invoke the private constructor
            {
                instance = Activator.CreateInstance(Type.GetType(packageName), true);
            }
            return instance;
        }

        public JArray GetJSONArrayResponse(object response)
        {
            JValue value = response is JValue ? (JValue)response : null;
            if (response == null || response.ToString().Equals("null") || (response.ToString().Trim()).Length == 0 || response.ToString().Equals("[]") || (value != null && value.Value == null))
            {
                return null;
            }
            return new JArray(response);
        }

        public JObject GetJSONResponse(object response)
        {
            JValue value = response is JValue ? (JValue)response : null;
            if (response == null || response.ToString().Equals("null") || (response.ToString().Trim()).Length == 0 || response.ToString().Equals("{}") || (value != null && value.Value == null))
            {
                return null;
            }
            return (JObject)response;
        }

        public JArray ValidateInterfaceClass(JObject orderedStructures, JArray classes)
        {
            JArray validClasses = new JArray();
            foreach (object className in classes)
            {
                bool isValid = false;
                foreach (KeyValuePair<string, JToken> index in orderedStructures)
                {
                    JObject orderedStructure = (JObject)index.Value;
                    if (!orderedStructure.ContainsKey(Constants.MEMBERS))
                    {
                        if (className.ToString().Equals(orderedStructure[Constants.STRUCTURE_NAME].ToString()))
                        {
                            isValid = true;
                            break;
                        }
                    }
                }
                if (!isValid)
                {
                    validClasses.Add(className);
                }
            }
            return validClasses;
        }

        public JArray ValidateStructure(JObject orderedStructures, JArray extraDetails)
        {
            JArray validStructure = new JArray();
            foreach (object extraDetail in extraDetails)
            {
                JObject extraDetail1 = (JObject)extraDetail;
                if (!extraDetail1.ContainsKey(Constants.MEMBERS))
                {
                    bool isValid = false;
                    foreach (KeyValuePair<string, JToken> index in orderedStructures)
                    {
                        JObject orderedStructure = (JObject)index.Value;
                        if (!orderedStructure.ContainsKey(Constants.MEMBERS))
                        {
                            string extraDetailStructureName = extraDetail1.GetValue(Constants.STRUCTURE_NAME).ToString();
                            if (extraDetailStructureName.Equals(orderedStructure[Constants.STRUCTURE_NAME].ToString()))
                            {
                                isValid = true;
                                break;
                            }
                        }
                    }
                    if (!isValid)
                    {
                        validStructure.Add(extraDetail1);
                    }
                }
                else
                {
                    if (extraDetail1.ContainsKey(Constants.MEMBERS))
                    {
                        bool isValid = true;
                        foreach (KeyValuePair<string, JToken> index in orderedStructures)
                        {
                            JObject orderedStructure = (JObject)index.Value;
                            if (orderedStructure.ContainsKey(Constants.MEMBERS))
                            {
                                JObject extraDetailStructureMembers = (JObject)extraDetail1.GetValue(Constants.MEMBERS);
                                JObject orderedStructureMembers = (JObject)orderedStructure.GetValue(Constants.MEMBERS);
                                if (extraDetailStructureMembers.Count == orderedStructureMembers.Count)
                                {
                                    foreach (KeyValuePair<string, JToken> extraDetailStructureMember1 in extraDetailStructureMembers)
                                    {
                                        JObject extraDetailStructureMember = (JObject)extraDetailStructureMember1.Value;

                                        if (orderedStructureMembers.ContainsKey(extraDetailStructureMember1.Key))
                                        {
                                            JObject orderedStructureMember = (JObject)orderedStructureMembers.GetValue(extraDetailStructureMember1.Key);
                                            if (extraDetailStructureMember.ContainsKey(Constants.TYPE) && orderedStructureMember.ContainsKey(Constants.TYPE) && !(extraDetailStructureMember.GetValue(Constants.TYPE).ToString().Equals(orderedStructureMember.GetValue(Constants.TYPE).ToString())))
                                            {
                                                isValid = false;
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        if (!isValid)
                        {
                            validStructure.Add(extraDetail1);
                        }
                    }
                }
            }
            return validStructure;
        }

        public float FindRatio(string className, Dictionary<string, object> response)
        {
            JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(className);
            return FindRatio(classDetail, response);
        }

        public float FindRatio(JObject classDetail, Dictionary<string, object> response)
        {
            float totalPoints = classDetail.Count;
            float matches = 0;
            if (totalPoints == 0)
            {
                return 0;
            }
            else
            {
                foreach (KeyValuePair<string, JToken> memberName in classDetail)
                {
                    JObject memberDetail = (JObject)memberName.Value;
                    string keyName = memberDetail.ContainsKey(Constants.NAME) ? memberDetail.GetValue(Constants.NAME).ToString() : null;
                    if (keyName != null && response.ContainsKey(keyName) && response[keyName] != null)
                    {// key not empty
                        object keyData = response[keyName];
                        Type tokenType = keyData.GetType();
                        string structureName = memberDetail.ContainsKey(Constants.STRUCTURE_NAME) ? memberDetail[Constants.STRUCTURE_NAME].ToString() : null;
                        string type = tokenType.FullName;
                        if (keyData is JObject)
                        {
                            type = Constants.MAP_NAMESPACE;
                        }
                        else if (keyData is JArray)
                        {
                            type = Constants.LIST_NAMESPACE;
                        }
                        if (type.Equals(memberDetail[Constants.TYPE].ToString()))
                        {
                            matches++;
                        }
                        else if (memberDetail[Constants.TYPE].ToString().Equals(Constants.CHOICE_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (object value in (JArray)memberDetail[Constants.VALUES])
                            {
                                if (value.Equals(keyData))
                                {
                                    matches++;
                                    break;
                                }
                            }
                        }
                        if (structureName != null && structureName.Equals(memberDetail[Constants.TYPE]))
                        {
                            if (memberDetail.ContainsKey(Constants.VALUES))
                            {
                                foreach (object value in (JArray)memberDetail[Constants.VALUES])
                                {
                                    if (value.Equals(keyData))
                                    {
                                        matches++;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                matches++;
                            }
                        }
                        if (keyData is Stream && memberDetail[Constants.SPEC_TYPE].ToString().Equals(Constants.TFILE_TYPE))
                        {
                            matches++;
                        }
                    }
                }
            }
            return matches / totalPoints;
        }

        public float FindRatio(String className, JObject responseJson)
        {
            JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(className);
            return FindRatio(classDetail, responseJson);
        }

        public float FindRatio(JObject classDetail, JObject responseJson)
        {
            float totalPoints = classDetail.Count;
            float matches = 0;
            if (totalPoints == 0)
            {
                return 0;
            }
            else
            {
                foreach (KeyValuePair<string, JToken> memberName in classDetail)
                {
                    JObject memberDetail = (JObject)classDetail.GetValue(memberName.Key);
                    string keyName = memberDetail.ContainsKey(Constants.NAME) ? memberDetail[Constants.NAME].ToString() : null;
                    if ((keyName != null && !string.IsNullOrEmpty(keyName) && !string.IsNullOrWhiteSpace(keyName)) && responseJson.ContainsKey(keyName) && responseJson[keyName] != null)
                    {// key not empty
                        JToken keyData = responseJson[keyName];
                        JTokenType tokenType = keyData.Type;
                        string structureName = memberDetail.ContainsKey(Constants.STRUCTURE_NAME) ? (string)memberDetail[Constants.STRUCTURE_NAME] : null;
                        string type = GetType(tokenType);
                        if (type.Equals((string)memberDetail[Constants.TYPE]))
                        {
                            matches++;
                        }
                        else if (keyName.Equals(Constants.COUNT, StringComparison.OrdinalIgnoreCase) && type.Equals(Constants.CSHARP_INT_NAME, StringComparison.OrdinalIgnoreCase) && tokenType == JTokenType.Integer)
                        {
                            matches++;
                        }
                        else if (((string)memberDetail[Constants.TYPE]).Equals(Constants.CHOICE_NAMESPACE, StringComparison.OrdinalIgnoreCase))
                        {
                            JArray values = (JArray)memberDetail[Constants.VALUES];

                            foreach (object value in values)
                            {
                                if (value.Equals(keyData))
                                {
                                    matches++;

                                    break;
                                }
                            }
                        }
                        if ((structureName != null && !string.IsNullOrEmpty(structureName) && !string.IsNullOrWhiteSpace(structureName)) && structureName.Equals((string)memberDetail[Constants.TYPE]))
                        {
                            if (memberDetail.ContainsKey(Constants.VALUES))
                            {
                                JArray values = (JArray)memberDetail[Constants.VALUES];

                                foreach (object value in values)
                                {
                                    if (value.Equals(keyData))
                                    {
                                        matches++;

                                        break;
                                    }
                                }
                            }
                            else
                            {
                                matches++;
                            }
                        }
                    }
                }
            }
            return matches / totalPoints;
        }

        public JObject FindMatchExtraDetail(JArray extraDetails, JObject responseObject)
        {
            float ratio = 0;
            int index = 0;
            for (int i = 0; i < extraDetails.Count; i++)
            {
                JObject classJSON = (JObject)extraDetails[i];
                if (!classJSON.ContainsKey(Constants.MEMBERS))
                {
                    float matchRatio = FindRatio(classJSON.GetValue(Constants.STRUCTURE_NAME).ToString(), responseObject);
                    if (matchRatio == 1.0)
                    {
                        index = i;
                        break;
                    }
                    else if (matchRatio > ratio)
                    {
                        index = i;
                        ratio = matchRatio;
                    }
                }
                else
                {
                    if (classJSON.ContainsKey(Constants.MEMBERS))
                    {
                        float matchRatio = FindRatio((JObject)classJSON.GetValue(Constants.MEMBERS), responseObject);
                        if (matchRatio == 1.0)
                        {
                            index = i;
                            break;
                        }
                        else if (matchRatio > ratio)
                        {
                            index = i;
                            ratio = matchRatio;
                        }
                    }

                }
            }
            return (JObject)extraDetails[index];
        }


        public string FindMatchClass(JArray classes, JObject responseJson)
        {
            string pack = "";
            float ratio = 0;
            foreach (Object className in classes)
            {
                float matchRatio = FindRatio((string)className, responseJson);
                if (matchRatio == 1.0)
                {
                    pack = (string)className;
                    break;
                }
                else if (matchRatio > ratio)
                {
                    pack = (string)className;
                    ratio = matchRatio;
                }
            }
            return pack;
        }

        public string FindMatchClass(JArray classes, Dictionary<string, Object> response)
        {
            string pack = "";
            float ratio = 0;
            foreach (Object className in classes)
            {
                float matchRatio = FindRatio((string)className, response);
                if (matchRatio == 1.0)
                {
                    pack = (string)className;
                    break;
                }
                else if (matchRatio > ratio)
                {
                    pack = (string)className;
                    ratio = matchRatio;
                }
            }
            return pack;
        }

        public string BuildName(string keyName)
        {
            List<string> name = keyName.ToLower().Split('_').ToList();
            string sdkName;
            int index;
            sdkName = String.Concat(name[0].Substring(0, 1).ToLower(), name[0].Substring(1));
            index = 1;
            for (int nameIndex = index; nameIndex < name.Count(); nameIndex++)
            {
                string firstLetterUppercase = "";
                if (String.IsNullOrEmpty(name[nameIndex]) && String.IsNullOrWhiteSpace(name[nameIndex]))
                {
                    firstLetterUppercase = String.Concat(name[nameIndex].Substring(0, 1).ToUpper(), name[nameIndex].Substring(1));
                }
                sdkName = String.Concat(sdkName, firstLetterUppercase);
            }
            return sdkName;
        }

        public static List<T> DeserializeSingleOrList<T>(JsonReader jsonReader)
        {
            if (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonToken.StartArray:
                        return new JsonSerializer().Deserialize<List<T>>(jsonReader);

                    case JsonToken.StartObject:
                        var instance = new JsonSerializer().Deserialize<T>(jsonReader);
                        return new List<T> { instance };
                }
            }

            throw new InvalidOperationException("Unexpected JSON input");
        }

        public JObject FindMatchResponseClass(JArray contents, JsonTextReader responseObject)
        {
            JObject response = null;
            if (responseObject.TokenType is JsonToken.StartObject)
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    DateParseHandling = DateParseHandling.None
                };
                JObject responseJson = serializer.Deserialize<JObject>(responseObject);
                response = this.GetJSONResponse(responseJson);
            }
            else if (responseObject.TokenType is JsonToken.StartArray)
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    DateParseHandling = DateParseHandling.None
                };
                JArray responseArray = serializer.Deserialize<JArray>(responseObject);
                response = (JObject)this.GetJSONArrayResponse(responseArray)[0];
            }
            if (response != null)
            {
                float ratio = 0;
                int structure = 0;
                for (int i = 0; i < contents.Count(); i++)
                {
                    JObject content = (JObject)contents[i];
                    float ratio1 = 0;
                    JArray classes;
                    if (content.ContainsKey(Constants.INTERFACE) && (bool)content.GetValue(Constants.INTERFACE))
                    {
                        string interfaceName = (string)((JArray)content.GetValue(Constants.CLASSES))[0];
                        JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(interfaceName);
                        JObject groupType1 = (JObject)classDetail.GetValue(content[Constants.GROUP_TYPE].ToString());
                        if (groupType1 == null)
                        {
                            return null;
                        }
                        classes = (JArray)groupType1.GetValue(Constants.CLASSES);
                    }
                    else
                    {
                        classes = (JArray)content.GetValue(Constants.CLASSES);
                    }
                    if (classes == null || classes.Count() == 0)
                    {
                        return null;
                    }
                    foreach (Object className in classes)
                    {
                        float matchRatio = FindRatio((string)className, response);
                        if (matchRatio == 1.0)
                        {
                            return (JObject)contents[i];
                        }
                        else if (matchRatio > ratio1)
                        {
                            ratio1 = matchRatio;
                        }
                    }
                    if (ratio < ratio1)
                    {
                        structure = i;
                    }
                }
                return (JObject)contents[structure];
            }
            return null;
        }
    }
}
