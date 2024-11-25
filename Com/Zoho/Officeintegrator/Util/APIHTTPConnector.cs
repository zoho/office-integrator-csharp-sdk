﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using Newtonsoft.Json;
using Com.Zoho.Officeintegrator.Logger;
using System.Linq;
using System.Text;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This module is to make HTTP connections, trigger the requests and receive the response
    /// </summary>
    public class APIHTTPConnector
    {
        private string url;
        private string requestMethod;
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        private Dictionary<string, string> parameters = new Dictionary<string, string>();
        private object requestBody;
        private string contentType;

        /// <summary>
        /// Gets or sets the ContentType.
        /// </summary>
        /// <value>A string containing the ContentType.</value>
        /// <returns>A string representing the ContentType.</returns>
        public string ContentType
        {
            get
            {
                return contentType;
            }
            set
            {
                contentType = value;
            }
        }

        /// <summary>
        /// This is a setter method to set the API URL.
        /// </summary>
        /// <value>A string containing the API Request URL.</value>
        public string URL
        {
            set
            {
                url = value;
            }
        }

        /// <summary>
        /// This is a setter method to set the API request method.
        /// </summary>
        /// <value>A string containing the API request method.</value>
        public string RequestMethod
        {
            set
            {
                requestMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets the API request headers.
        /// </summary>
        /// <value>A Dictionary&lt;string, string&gt; containing the API request headers.</value>
        /// <returns>A Dictionary&lt;string, string&gt; representing the API request headers.</returns>
        public Dictionary<string, string> Headers
        {
            get
            {
                return headers;
            }
            set
            {
                headers = value;
            }
        }

        /// <summary>
        /// This method to add API request header name and value.
        /// </summary>
        /// <param name="headerName">A string containing the API request header name.</param>
        /// <param name="headerValue">A string containing the API request header value.</param>
        public void AddHeader(string headerName, string headerValue)
        {
            Headers[headerName] = headerValue;
        }

        /// <summary>
        /// Gets or sets the API request parameters.
        /// </summary>
        /// <value>A Dictionary&lt;string, string&gt; containing the API request parameters.</value>
        /// <returns>A Dictionary&lt;string, string&gt; representing the API request parameters.</returns>
        public Dictionary<string, string> Params
        {
            get
            {
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }

        /// <summary>
        /// This method to add API request param name and value.
        /// </summary>
        /// <param name="paramName">A string containing the API request param name.</param>
        /// <param name="paramValue">A string containing the API request param value.</param>
        public void AddParam(string paramName, string paramValue)
        {
            Params[paramName] = paramValue;
        }

        /// <summary>
        /// This is a setter method to set the API request body.
        /// </summary>
        /// <value>A object containing the API request body.</value>
        public object RequestBody
        {
            set
            {
                requestBody = value;
            }
        }

        /// <summary>
        /// This method makes a Zoho Rest API request.
        /// </summary>
        /// <param name="converterInstance">A Converter class instance to call appendToRequest method.</param>
        /// <returns>HttpWebResponse class instance or null</returns>
        public HttpWebResponse FireRequest(Converter converterInstance)
        {
            SetQueryParams();
            HttpWebRequest requestObj = (HttpWebRequest)WebRequest.Create(url);
            requestObj.Timeout = (Initializer.GetInitializer().SDKConfig.Timeout) * 1000;
            RequestProxy requestProxy = Initializer.GetInitializer().RequestProxy;
            if(requestProxy != null)
            {
                //Validate proxy address
                var proxyURI = new Uri(string.Format("{0}:{1}", requestProxy.Host, requestProxy.Port));
                ICredentials credentials = null;
                if (requestProxy.User != null)
                {
                    //Set credentials
                    credentials = new NetworkCredential(requestProxy.User, requestProxy.Password, requestProxy.UserDomain);
                }
                //Set proxy
                requestObj.Proxy = new WebProxy(proxyURI, true, null, credentials);
                SDKLogger.LogInfo(this.ProxyLog(requestProxy));
            }
            SetRequestMethod(requestObj);
            if (contentType != null)
            {
                requestObj.ContentType = this.ContentType;
            }
            SetQueryHeaders(ref requestObj);
            if (requestBody !=  null)
            {
                converterInstance.AppendToRequest(requestObj, requestBody);
            }
            SDKLogger.LogInfo(ToString());
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)requestObj.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response == null) { throw; }
                response = (HttpWebResponse)e.Response;
            }
            return response;
        }

        private void SetRequestMethod(HttpWebRequest request)
        {
            switch (requestMethod)
            {
                case "GET":
                    request.Method = Constants.REQUEST_METHOD_GET;
                    return;
                case "DELETE":
                    request.Method = Constants.REQUEST_METHOD_DELETE;
                    return;
                case "POST":
                    request.Method = Constants.REQUEST_METHOD_POST;
                    return;
                case "PUT":
                    request.Method = Constants.REQUEST_METHOD_PUT;
                    return;
                case "PATCH":
                    request.Method = Constants.REQUEST_METHOD_PATCH;
                    return;
                default:
                    return;
            }
        }

        private void SetQueryHeaders(ref HttpWebRequest request)
        {
            foreach (KeyValuePair<string, string> headersMap in Headers)
            {
                if (!string.IsNullOrEmpty(headersMap.Value))
                {
                    if (WebHeaderCollection.IsRestricted(headersMap.Key))
                    {
                        if (headersMap.Key.Equals(Constants.IF_MODIFIED_SINCE))
                        {
                            DateTime dateConversion = XmlConvert.ToDateTime(RemoveEscaping(JsonConvert.SerializeObject(headersMap.Value)), XmlDateTimeSerializationMode.Utc);
                            request.IfModifiedSince = dateConversion;
                        }
                    }
                    else
                    {
                        request.Headers[headersMap.Key] = headersMap.Value;
                    }
                }
            }
        }

        private void SetQueryParams()
        {
            if (Params.Count == 0) { return; }
            this.url = url + "?" + string.Join("&", Params.Select(pp => pp.Key + "=" + Uri.EscapeDataString(pp.Value)));
        }

        public static string RemoveEscaping(string input)
        {
            input = input.Replace("\\", "");
            input = input.Replace("\"", "");
            return input;
        }

        public override string ToString()
        {
            Dictionary<string, string> reqHeaders = new Dictionary<string, string>(Headers);
            reqHeaders[Constants.AUTHORIZATION] = Constants.CANT_DISCLOSE;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(requestMethod.ToString()).Append(" - ");
            stringBuilder.Append(Constants.URL).Append(" = ").Append(url).Append(", ");
            stringBuilder.Append(Constants.HEADERS).Append(" = ").Append(JsonConvert.SerializeObject(reqHeaders)).Append(", ");
            stringBuilder.Append(Constants.PARAMS).Append(" = ").Append(JsonConvert.SerializeObject(parameters)).Append(".");
            return stringBuilder.ToString();
        }
        private string ProxyLog(RequestProxy requestProxy)
        {
            StringBuilder proxyStringBuilder = new StringBuilder();
            proxyStringBuilder.Append(Constants.PROXY_SETTINGS);
            proxyStringBuilder.Append(Constants.PROXY_HOST).Append(requestProxy.Host).Append(", ");
            proxyStringBuilder.Append(Constants.PROXY_PORT).Append(requestProxy.Port.ToString());
            if (requestProxy.User != null)
            {
                proxyStringBuilder.Append(", ").Append(Constants.PROXY_USER).Append(requestProxy.User);
            }
            if (requestProxy.UserDomain != null)
            {
                proxyStringBuilder.Append(", ").Append(Constants.PROXY_DOMAIN).Append(requestProxy.UserDomain);
            }
            return proxyStringBuilder.ToString();
        }
    }
}
