using System;
using Com.Zoho.Officeintegrator.Util;
using System.Collections.Generic;
using Com.Zoho.Officeintegrator.Exception;

namespace Com.Zoho.API.Authenticator
{
    public class Auth : IToken
    {
        private AuthenticationSchema authenticationSchema;
        private Dictionary<String, String> parameterMap = new Dictionary<String, String>();
        private Dictionary<String, String> headerMap = new Dictionary<String, String>();

        public AuthenticationSchema AuthenticationSchema
        {
            get
            {
                return this.authenticationSchema;
            }           
            set
            {
                this.authenticationSchema = value;
            }
        }

        public AuthenticationSchema GetAuthenticationSchema()
        {
            return AuthenticationSchema;
        }

        public void Authenticate(APIHTTPConnector urlConnection, Object config)
        {
            if (this.headerMap.Count > 0)
            {
                foreach (KeyValuePair<string, string> headers in this.headerMap)
                {
                    if (!string.IsNullOrEmpty(headers.Value))
                    {
                        urlConnection.AddHeader(headers.Key, headers.Value);
                    }
                }
            }
            if (this.parameterMap.Count > 0)
            {
                foreach (KeyValuePair<string, string> parameter in this.parameterMap)
                {
                    if (!string.IsNullOrEmpty(parameter.Value))
                    {
                        urlConnection.AddParam(parameter.Key, parameter.Value);
                    }
                }
            }
        }

        public void Remove()
        {
        }

        public void GenerateToken()
        {
        }

        public String GetId()
        {
            return null;
        }

        private Auth(Dictionary<String, String> parameterMap, Dictionary<String, String> headerMap, AuthenticationSchema authenticationSchema)
        {
            this.parameterMap = parameterMap;
            this.headerMap = headerMap;
            this.authenticationSchema = authenticationSchema;
        }

        public class Builder
        {
            private AuthenticationSchema authenticationSchema;
            private Dictionary<String, String> parameterMap = new Dictionary<String, String>();
            private Dictionary<String, String> headerMap = new Dictionary<String, String>();

            public Builder AddParam(String paramName, String paramValue)
            {
                if(parameterMap.ContainsKey(paramName) && parameterMap.ContainsValue(paramName))
                {
                    String existingParamValue = parameterMap[paramName];
                    existingParamValue = existingParamValue + "," + paramValue;
                    parameterMap[paramName] = existingParamValue;
                }
                else
                {
                    parameterMap[paramName] = paramValue;
                }
                return this;
            }

            public Builder AddHeader(String headerName, String headerValue)
            {
                if(headerMap.ContainsKey(headerName) && headerMap.ContainsValue(headerName))
                {
                    String existingParamValue = headerMap[headerName];
                    existingParamValue = existingParamValue + "," + headerValue;
                    headerMap[headerName] = existingParamValue;
                }
                else
                {
                    headerMap[headerName] = headerValue;
                }
                return this;
            }

            public Builder ParameterMap(Dictionary<String, String> parameterMap)
            {
                this.parameterMap = parameterMap;
                return this;
            }

            public Builder HeaderMap(Dictionary<String, String> headerMap)
            {
                this.headerMap = headerMap;
                return this;
            }

            public Builder AuthenticationSchema(AuthenticationSchema authenticationSchema)
            {
                this.authenticationSchema = authenticationSchema;
                return this;
            }

            public Auth Build()
            {
                if (this.authenticationSchema == null)
                {
                    throw new SDKException(Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR + "-" + Constants.OAUTH_MANDATORY_KEYS_1);
                }
                return new Auth(this.parameterMap, this.headerMap , this.authenticationSchema);
            }
        }
	}
}

