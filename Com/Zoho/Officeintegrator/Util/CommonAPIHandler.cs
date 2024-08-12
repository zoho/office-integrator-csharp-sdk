using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Mime;
using Com.Zoho.Officeintegrator.Exception;
using Com.Zoho.Officeintegrator.Logger;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using static Com.Zoho.API.Authenticator.IToken;
using Com.Zoho.API.Authenticator;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data.Common;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This class is to process the API request and its response.
    /// Construct the objects that are to be sent as parameters or in the request body with the API.
    /// The Request parameter, header and body objects are constructed here.
    /// Process the response JSON and converts it to relevant objects in the library.
    /// </summary>
    public class CommonAPIHandler
    {
        private string apiPath;
        private ParameterMap param = new ParameterMap();
        private HeaderMap header = new HeaderMap();
        private object request;
        private string httpMethod;
        private string moduleAPIName;
        private string contentType;
        private string categoryMethod;
        private string methodName;
        private string operationClassName;

        /// <summary>
        /// This is a setter method to set the API request URL.
        /// </summary>
        /// <value>A string containing the API request URL.</value>
        public string APIPath
        {
            get
            {
                return apiPath;
            }
            set
            {
                apiPath = value;
            }
        }

        /// <summary>
        /// This method is to add an API request parameter.
        /// </summary>
        /// <typeparam name="T">A T containing the specified method type.</typeparam>
        /// <param name="paramInstance">A Param instance containing the API request parameter.</param>
        /// <param name="paramValue">A T containing the API request parameter value.</param>
        public void AddParam<T>(Param<T> paramInstance, T paramValue)
        {
            if (paramValue == null)
            {
                return;
            }
            if (param == null)
            {
                param = new ParameterMap();
            }
            param.Add(paramInstance, paramValue);
        }

        /// <summary>
        /// This method to add an API request header.
        /// </summary>
        /// <typeparam name="T">A T containing the specified method type.</typeparam>
        /// <param name="headerInstance">A Header instance the API request header.</param>
        /// <param name="headerValue">A T containing the API request header value.</param>
        public void AddHeader<T>(Header<T> headerInstance, T headerValue)
        {
            if (headerValue == null)
            {
                return;
            }
            if (header == null)
            {
                header = new HeaderMap();
            }
            header.Add(headerInstance, headerValue);
        }

        /// <summary>
        /// This is a setter method to set the API request parameter map.
        /// </summary>
        /// <value>A ParameterMap class instance containing the API request parameter.</value>
        public ParameterMap Param
        {
            set
            {
                if(value == null)
                {
                    return;
                }
                if(param.ParameterMaps != null && param.ParameterMaps.Count > 0)
                {
                    value.ParameterMaps.ToList().ForEach(x => param.ParameterMaps[x.Key] = x.Value);
                }
                else
                {
                    param = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Zoho module API name.
        /// </summary>
        /// <value>A string containing the Zoho module API name.</value>
        /// <returns>A string representing the Zoho module API name.</returns>
        public string ModuleAPIName
        {
            get
            {
                return moduleAPIName;
            }
            set
            {
                moduleAPIName = value;
            }
        }
        
        /// <summary>
        /// This is a setter method to set the API request header map.
        /// </summary>
        /// <value>A HeaderMap class instance containing the API request header.</value>
        public HeaderMap Header
        {
            set
            {
                if (value == null)
                {
                    return;
                }
                if (header.HeaderMaps != null && header.HeaderMaps.Count > 0)
                {
                    value.HeaderMaps.ToList().ForEach(x => header.HeaderMaps[x.Key] = x.Value);
                }
                else
                {
                    header = value;
                }
            }
        }

        /// <summary>
        /// This is a setter method to set the API request body object.
        /// </summary>
        /// <value>A object containing the API request body object.</value>
        public object Request
        {
            set
            {
                request = value;
            }
        }

        /// <summary>
        /// This is a setter method to set the HTTP API request method.
        /// </summary>
        /// <value>A string containing the HTTP API request method.</value>
        public string HttpMethod
        {
            get
            {
                return httpMethod;
            }
            set
            {
                httpMethod = value;
            }
        }

        /// <summary>
        /// This method is used in constructing API request and response details. To make the Zoho API calls.
        /// </summary>
        /// <typeparam name="T">A T containing the specified type method.</typeparam>
        /// <returns>A APIResponse&lt;T&gt; representing the Zoho API response instance or null. </returns>
        public APIResponse<T> APICall<T>()
        {
            if (Initializer.GetInitializer() == null)
            {
                throw new SDKException(Constants.SDK_UNINITIALIZATION_ERROR, Constants.SDK_UNINITIALIZATION_MESSAGE);
            }
            APIHTTPConnector connector = new APIHTTPConnector
            {
                RequestMethod = httpMethod
            };
            try
            {
                SetAPIURL(connector);
            }
            catch (SDKException e)
            {
                SDKLogger.LogError(Constants.SET_API_URL_EXCEPTION + JsonConvert.SerializeObject(e));
                throw e;
            }
            catch (System.Exception e)
            {
                SDKException exception = new SDKException(e);
                SDKLogger.LogError(Constants.SET_API_URL_EXCEPTION + JsonConvert.SerializeObject(exception));
                throw exception;
            }
            Dc.Environment environment = Initializer.GetInitializer().Environment;
            if (header != null && header.HeaderMaps.Count > 0)
            {
                connector.Headers = header.HeaderMaps;
                if(environment.GetLocation() == Location.HEADER)
                {
                    connector.AddHeader(environment.GetName(), environment.GetValue());
                }
            }
            if (param != null && param.ParameterMaps.Count > 0)
            {
                connector.Params = param.ParameterMaps;
                if(environment.GetLocation() == Location.PARAM)
                {
                    connector.AddParam(environment.GetName(), environment.GetValue());
                }
            }
            try
            {
                if(Initializer.GetInitializer().Tokens != null && Initializer.GetInitializer().Tokens.Count() > 0)
                {
                    List<Object> tokenConfig = GetToken();
                    ((IToken)tokenConfig[0]).Authenticate(connector, tokenConfig[1]);
                }
            }
            catch (SDKException e)
            {
                SDKLogger.LogError(Constants.AUTHENTICATION_EXCEPTION + JsonConvert.SerializeObject(e));
                throw e;
            }
            catch (System.Exception e)
            {
                SDKException exception = new SDKException(e);
                SDKLogger.LogError(Constants.AUTHENTICATION_EXCEPTION + JsonConvert.SerializeObject(exception));
                throw exception;
            }
            Converter convertInstance = null;
            if (Constants.GENERATE_REQUEST_BODY.Contains(httpMethod.ToUpper()) && this.request != null)
            {
                object request;
                try
                {
                    Object pack = GetClassName(false, null, null);
                    if(pack != null)
                    {
                        convertInstance = GetConverterClassInstance(contentType.ToLower());
                        connector.ContentType = contentType;
                        bool isSet = false;
                        if(pack is JObject)
                        {
                            JObject pack1 = (JObject)pack;
                            if(pack1.ContainsKey(Constants.CLASSES))
                            {
                                JArray classes = (JArray)pack1[Constants.CLASSES];
                                if(classes.Count == 1 && ((string)classes[0]).Equals(typeof(Object).FullName, StringComparison.OrdinalIgnoreCase))
                                {
                                    connector.RequestBody = this.request;
                                    isSet = true;
                                }
                            }
                        }
                        if(!isSet)
                        {
                            request = convertInstance.GetWrappedRequest(this.request, (JObject) pack);
                            connector.RequestBody = request;	
                        }
                    }
                }
                catch (SDKException e)
                {
                    SDKLogger.LogError(Constants.FORM_REQUEST_EXCEPTION + JsonConvert.SerializeObject(e));
                    throw e;
                }
                catch (System.Exception e)
                {
                    SDKException exception = new SDKException(e);
                    SDKLogger.LogError(Constants.FORM_REQUEST_EXCEPTION + JsonConvert.SerializeObject(exception));
                    throw exception;
                }
            }
            try
            {
                HttpWebResponse response = connector.FireRequest(convertInstance);
                int statusCode = (int)response.StatusCode;
                string statusDescription = response.StatusDescription;
                Dictionary<string, string> headerMap = GetHeaders(response.Headers);
                string mimeType = response.ContentType;
                Object returnObject = null;
                JObject responseJSON = null;
                if (!string.IsNullOrEmpty(mimeType) && !string.IsNullOrWhiteSpace(mimeType))
                {
                    if (mimeType.Contains(";"))
                    {
                        mimeType = mimeType.Split(';')[0];
                    }
                    convertInstance = GetConverterClassInstance(mimeType.ToLower());
                    Object pack = GetClassName(true, statusCode, mimeType);
                    if(pack != null)
                    {
                        List<Object> responseObject = (List<object>)convertInstance.GetWrappedResponse(response, (JArray)pack);
                        if(responseObject != null)
                        {
                            returnObject = responseObject[0];
                            if (responseObject.Count == 2)
                            {
                                responseJSON = (JObject) responseObject[1];
                            }
                        }
                    }
                }
                else
                {
                    if(response != null)
                    {
                        HttpWebResponse responseEntity = ((HttpWebResponse)response);
                        string responseString = new StreamReader(responseEntity.GetResponseStream()).ReadToEnd();
                        SDKLogger.LogError(Constants.API_ERROR_RESPONSE + responseString);
                        responseEntity.Close();
                    }
                }
                return new APIResponse<T>(headerMap, statusCode, returnObject, responseJSON, statusDescription);
            }
            catch (SDKException e)
            {
                SDKLogger.LogError(Constants.API_CALL_EXCEPTION + JsonConvert.SerializeObject(e));
                throw e;
            }
            catch (System.Exception e)
            {
                SDKException exception = new SDKException(e);
                SDKLogger.LogError(Constants.API_CALL_EXCEPTION + JsonConvert.SerializeObject(exception));
                throw exception;
            }
        }

        /// <summary>
        /// This method is used to get a Converter class instance.
        /// </summary>
        /// <param name="encodeType">A String containing the API response content type.</param>
        /// <returns>A Converter class instance.</returns>
        public Converter GetConverterClassInstance(string encodeType)
        {
            switch (encodeType)
            {
                case "text/plain":
                    return new TextConverter(this);
                case "application/json":
                case "application/ld+json":
                    return new JSONConverter(this);
                case "application/xml":
                case "text/xml":
                    return new XMLConverter(this);
                case "multipart/form-data":
                    return new FormDataConverter(this);
                case "image/png":
                case "image/jpeg":
                case "image/gif":
                case "image/tiff":
                case "image/svg+xml":
                case "image/bmp":
                case "image/webp":
                case "text/csv":
                case "text/html":
                case "text/css":
                case "text/javascript":
                case "text/calendar":
                case "application/x-download":
                case "application/zip":
                case "application/pdf":
                case "application/java-archive":
                case "application/javascript":
                case "application/octet-stream":
                case "application/xhtml+xml":
                case "application/x-bzip":
                case "application/msword":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                case "application/gzip":
                case "application/x-httpd-php":
                case "application/vnd.ms-powerpoint":
                case "application/vnd.rar":
                case "application/x-sh":
                case "application/x-tar":
                case "application/vnd.ms-excel":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                case "application/x-7z-compressed":
                case "audio/mpeg":
                case "audio/x-ms-wma":
                case "audio/vnd.rn-realaudio":
                case "audio/x-wav":
                case "audio/3gpp":
                case "audio/3gpp2":
                case "video/mpeg":
                case "video/mp4":
                case "video/webm":
                case "video/3gpp":
                case "video/3gpp2":
                case "font/ttf":
                    return new Downloader(this);
                default:
                    return null;
            }
        }

        /// <summary>
        /// This method to get API response headers.
        /// </summary>
        /// <param name="headers">A System.Net.WebHeaderCollection class instance containing the API response headers.</param>
        /// <returns>A Dictionary&lt;String,String&gt; representing the API response headers.</returns>
        public Dictionary<string, string> GetHeaders(WebHeaderCollection headers)
        {
            Dictionary<string, string> headerMap = new Dictionary<string, string>();
            for (int i = 0; i < headers.Count; ++i)
            {
                string headerKey = headers.GetKey(i);
                string headerValue = "";
                foreach (string value in headers.GetValues(i))
                {
                    headerValue = string.Concat(headerValue, value);
                }
                headerMap.Add(headerKey, headerValue);
            }
            return headerMap;
        }

        private void SetAPIURL(APIHTTPConnector connector)
        {
            string APIPath = "";
            if (apiPath.Contains(Constants.HTTP))
            {
                if (apiPath.Substring(0, 1).Equals("/"))
                {
                    apiPath = apiPath.Substring(1);
                }
            }
            else
            {
                APIPath = string.Concat(APIPath, Initializer.GetInitializer().Environment.GetUrl());
            }
            APIPath = string.Concat(APIPath, apiPath);
            connector.URL = APIPath;
        }

        private JObject GetRequestClassName(JObject requests)
	    {
		    String name = this.request.GetType().FullName;
            if(this.request is IList)
            {
                name = ((IList) this.request)[0].GetType().FullName;
            }
            foreach (KeyValuePair<string, JToken> type in requests)
            {
                JArray contents = (JArray)type.Value;
                foreach(Object content1 in contents)
                {
                    JObject content = (JObject) content1;
                    if(content.ContainsKey(Constants.INTERFACE) && (bool)content[Constants.INTERFACE])
                    {
                        String interfaceName = (String) ((JArray)content[Constants.CLASSES])[0];
                        if(interfaceName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            this.contentType = type.Key;
                            return content;
                        }
                        JObject classDetail = (JObject)Initializer.jsonDetails.GetValue(interfaceName);
                        foreach (KeyValuePair<string, JToken> groupType in classDetail)
                        {
                            JObject groupTypeContent = (JObject)groupType.Value;
                            JArray classes = (JArray)groupTypeContent.GetValue(Constants.CLASSES);
                            foreach(Object className in classes)
                            {
                                if(className.ToString().Equals(name, StringComparison.OrdinalIgnoreCase))
                                {
                                    this.contentType = type.Key;
                                    return content;
                                }
                            }
                        }
                    }
                    else
                    {
                        JArray classes = (JArray)content.GetValue(Constants.CLASSES);
                        foreach(Object className in classes)
                        {
                            if(className.ToString().Equals(name, StringComparison.OrdinalIgnoreCase))
                            {
                                this.contentType = type.Key;
                                return content;
                            }
                        }
                        if(classes.Count == 1 && classes[0].Equals(typeof(Object).FullName))
                        {
                            this.contentType = type.Key;
                            return content;
                        }
                    }
                }
            }
            return null;
        }

        private Object GetClassName(bool isResponse, int? statusCode, String mimeType)
        {
            JObject jsonDetails = Initializer.jsonDetails;
            if(jsonDetails.ContainsKey(this.OperationClassName.ToLower()))
            {
                JObject methods = (JObject)jsonDetails.GetValue(this.operationClassName.ToLower());
                String methodName = this.MethodName;
                if(methods.ContainsKey(methodName))
                {
                    JObject methodDetails = (JObject)methods.GetValue(methodName);
                    if(isResponse)
                    {
                        if(methodDetails.ContainsKey(Constants.RESPONSE))
                        {
                            JObject response = (JObject)methodDetails.GetValue(Constants.RESPONSE);
                            if(response.ContainsKey(Convert.ToString(statusCode)))
                            {
                                JArray contentResponse = (JArray)response.GetValue(Convert.ToString(statusCode));
                                foreach(Object content in contentResponse)
                                {
                                    JObject contentJSON = (JObject)content;
                                    if(contentJSON.ContainsKey(mimeType))
                                    {
                                        return (JArray)contentJSON.GetValue(mimeType);
                                    }
                                }
                                SDKLogger.LogError(Constants.API_CALL_EXCEPTION);
                            }
                            else
                            {
                                SDKLogger.LogError(Constants.API_CALL_EXCEPTION);
                            }
                        }
                    }
                    else
                    {
                        if(methodDetails.ContainsKey(Constants.REQUEST))
                        {
                            return GetRequestClassName((JObject)methodDetails.GetValue(Constants.REQUEST));
                        }
                    }
                }
                else
                {
                    SDKLogger.LogError(Constants.API_CALL_EXCEPTION);
                }
            }
            else
            {
                SDKLogger.LogError(Constants.API_CALL_EXCEPTION);
            }
            return null;
        }

        public string CategoryMethod
        {
            get
            {
                return categoryMethod;
            }
            set
            {
                categoryMethod = value;
            }
        }

        public string MethodName
        {
            get
            {
                return methodName;
            }
            set
            {
                methodName = BuildName((value.Split('_').ToList()), false);
            }
        }

        public string OperationClassName
        {
            get
            {
                return operationClassName;
            }
            set
            {
                operationClassName = value;
            }
        }

        private List<Object> GetToken()
        {
            JArray authenticationTypes = GetRequestMethodDetails(this.operationClassName);
            if(authenticationTypes != null)
            {
                foreach (IToken token in Initializer.GetInitializer().Tokens)
                {
                    foreach (Object authenticationType in authenticationTypes)
                    {
                        JObject authentication = (JObject) authenticationType;
                        String schemaName = (string)authentication.GetValue(Constants.SCHEMA_NAME);
                        if(schemaName.Equals(token.GetAuthenticationSchema().GetSchema()))
                        {
                            return new List<object>() { token, authentication };
                        }
                    }
                }
            }
            return new List<object>() { Initializer.GetInitializer().Tokens[0], null };
        }

        private JArray GetRequestMethodDetails(String operationsClassName)
        {
            try
            {
                if(Initializer.jsonDetails.ContainsKey(operationsClassName.ToLower()))
                {
                    JObject classDetails = (JObject)Initializer.jsonDetails.GetValue(operationsClassName.ToLower());
                    String methodName = this.MethodName;
                    if(classDetails.ContainsKey(methodName))
                    {
                        JObject methodDetails = (JObject)classDetails.GetValue(methodName);
                        if(methodDetails.ContainsKey(Constants.AUTHENTICATION))
                        {
                            return (JArray)methodDetails[Constants.AUTHENTICATION];
                        }
                        else if (classDetails.ContainsKey(Constants.AUTHENTICATION))
                        {
                            return (JArray)classDetails[Constants.AUTHENTICATION];
                        }
                        return null;
                    }
                    else
                    {
                        throw new System.Exception(Constants.SDK_OPERATIONS_METHOD_DETAILS_NOT_FOUND_IN_JSON_DETAILS_FILE);
                    }
                }
                else
                {
                    throw new System.Exception(Constants.SDK_OPERATIONS_CLASS_DETAILS_NOT_FOUND_IN_JSON_DETAILS_FILE);
                }
            }
            catch(System.Exception ex)
            {
                SDKException exception = new SDKException(ex);
                SDKLogger.LogError(Constants.API_CALL_EXCEPTION + JsonConvert.SerializeObject(exception));
                throw exception;
            }
        }

        public String BuildName(List<String> name, Boolean isType)
        {
            String sdkName = "";
            int index;
            if (isType)
            {
                index = 0;
            }
            else
            {
                if (name != null && name.Count > 0)
                {
                    sdkName = name[0];
                    sdkName = sdkName.Contains("$") ? sdkName.Replace("$", "") : sdkName;
                }
                index = 1;
            }
            for (int nameIndex = index; nameIndex < name.Count(); nameIndex++)
            {
                String fullName = name[nameIndex];
                if (fullName.Length > 0)
                {
                    String firstLetterUppercase = GetFieldName(fullName);
                    if (firstLetterUppercase.Equals("api", StringComparison.OrdinalIgnoreCase))
                    {
                        sdkName = string.Concat(sdkName, firstLetterUppercase.ToUpper());
                    }
                    else
                    {
                        sdkName = string.Concat(sdkName, firstLetterUppercase);
                    }
                }
            }
            return sdkName.Replace("(\\(|\\)|'|[|]|\\{|})", "");
        }

        private static String GetFieldName(String fullName)
	    {
            String varName = fullName;
            if (fullName.Contains("$"))
            {
                varName = fullName.Replace("$", "");
            }
            else if (fullName.Contains("_"))
            {
                varName = fullName.Replace("_", "");
            }
            return string.Concat(varName.Substring(0, 1).ToUpper(), varName.Substring(1));
        }
    }
}