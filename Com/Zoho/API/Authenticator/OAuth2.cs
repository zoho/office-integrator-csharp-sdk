using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Com.Zoho.API.Authenticator.Store;
using Com.Zoho.Officeintegrator.Exception;
using Com.Zoho.Officeintegrator.Logger;
using Com.Zoho.Officeintegrator.Util;
using Com.Zoho.Officeintegrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Com.Zoho.API.Authenticator
{
    /// <summary>
    /// This class gets the tokens and checks the expiry time.
    /// </summary>
    public class OAuth2 : IToken
    {
        private string clientID;
        private string clientSecret;
        private string redirectURL;
        private string grantToken;
        private string refreshToken;
        private string accessToken;
        private string expiresIn;
        private UserSignature userSignature;
        private string id;
        private AuthenticationSchema authenticationSchema;

        public OAuth2()
        {
        }

        private OAuth2(string clientID, string clientSecret, string grantToken, string refreshToken, string redirectURL, string id, string accessToken, UserSignature userSignature, AuthenticationSchema authenticationSchema)
        {
            this.clientID = clientID;
            this.clientSecret = clientSecret;
            this.grantToken = grantToken;
            this.refreshToken = refreshToken;
            this.redirectURL = redirectURL;
            this.accessToken = accessToken;
            this.id = id;
            this.userSignature = userSignature;
		    this.authenticationSchema = authenticationSchema;
        }

        /// <summary>
        /// This is a getter method to get OAuth client id.
        /// </summary>
        /// <returns>A string representing the OAuth client id.</returns>
        public string ClientId
        {
            get
            {
                return clientID;
            }
            set
            {
                clientID = value;
            }
        }

        /// <summary>
        /// This is a getter method to get OAuth client secret.
        /// </summary>
        /// <returns>A string representing the OAuth client secret.</returns>
        public string ClientSecret
        {
            get
            {
                return clientSecret;
            }
            set
            {
                clientSecret = value;
            }
        }

        /// <summary>
        /// This is a getter method to get OAuth redirect URL.
        /// </summary>
        /// <returns>A string representing the OAuth redirect URL.</returns>
        public string RedirectURL
        {
            get
            {
                return redirectURL;
            }
            set
            {
                redirectURL = value;
            }
        }

        /// <summary>
        /// This is a getter method to get grant token.
        /// </summary>
        /// <returns>A string representing the grant token.</returns>
        public string GrantToken
        {
            get
            {
                return grantToken;
            }
            set
            {
                grantToken = value;
            }
        }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>A string containing the refresh token.</value>
        /// <returns>A string representing the refresh token.</returns>
        public string RefreshToken
        {
            get
            {
                return refreshToken;
            }
            set
            {
                refreshToken = value;
            }
        }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>A string containing the access token.</value>
        /// <returns>A string representing the access token.</returns>
        public string AccessToken
        {
            get
            {
                return accessToken;
            }
            set
            {
                accessToken = value;
            }
        }

        /// <summary>
        /// Gets or sets the token expire time.
        /// </summary>
        /// <value>A string containing the token expire time.</value>
        /// <returns>A string representing the token expire time.</returns>
        public string ExpiresIn
        {
            get
            {
                return expiresIn;
            }
            set
            {
                expiresIn = value;
            }
        }

        public UserSignature UserSignature
        {
            get
            {
                return userSignature;
            }
            set
            {
                userSignature = value;
            }
        }

        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public AuthenticationSchema AuthenticationSchema
        {
            get
            {
                return authenticationSchema;
            }
            set
            {
                authenticationSchema = value;
            }
        }

        public String GetId()
        {
            return id;
        }

        public AuthenticationSchema GetAuthenticationSchema()
        {
            return AuthenticationSchema;
        }

        public void GenerateToken()
        {
            GetToken();
        }

        public String GetToken()
        {
            lock (this)
            {
                String refreshUrl = authenticationSchema.GetRefreshUrl();
		        String tokenUrl = authenticationSchema.GetTokenUrl();
                Initializer initializer = Initializer.GetInitializer();
                ITokenStore store = initializer.Store;
                OAuth2 oauthToken = null;
                if (this.Id != null)
                {
                    oauthToken = (OAuth2)store.FindTokenById(this.Id);
                    MergeObjects(this, oauthToken);
                }
                else
                {
                    oauthToken = (OAuth2)store.FindToken(this);
                }
                if (oauthToken == null)
                {
                    if(this.UserSignature != null)
                    {
                        CheckTokenDetails();
                    }
                    oauthToken = this;
                }
                if (String.IsNullOrEmpty(oauthToken.AccessToken) || String.IsNullOrWhiteSpace(oauthToken.AccessToken))
                {
                    if (!String.IsNullOrEmpty(oauthToken.RefreshToken) && !String.IsNullOrWhiteSpace(oauthToken.RefreshToken))
                    {
                        SDKLogger.LogInfo(Constants.ACCESS_TOKEN_USING_REFRESH_TOKEN_MESSAGE);
                        oauthToken.RefreshAccessToken(oauthToken, store, refreshUrl);
                    }
                    else
                    {
                        SDKLogger.LogInfo(Constants.ACCESS_TOKEN_USING_GRANT_TOKEN_MESSAGE);
                        oauthToken.GenerateAccessToken(oauthToken, store, tokenUrl);
                    }
                }
                else if (oauthToken.ExpiresIn != null && GetExpiryLapseInMillis(oauthToken.ExpiresIn) < 5L)//access token will expire in next 5 seconds or less
                {
                    SDKLogger.LogInfo(Constants.REFRESH_TOKEN_MESSAGE);
                    oauthToken.RefreshAccessToken(oauthToken, store, refreshUrl);
                }
                else if (oauthToken.ExpiresIn == null && oauthToken.AccessToken != null && oauthToken.Id == null)
                {
                    store.SaveToken(oauthToken);
                }
                return oauthToken.AccessToken;
            }
        }

        private void CheckTokenDetails()
        {
            if (AreAllObjectsNull(this.grantToken, this.refreshToken))
            {
                throw new SDKException(Constants.MANDATORY_VALUE_ERROR, Constants.GET_TOKEN_BY_USER_NAME_ERROR + " - " + string.Join(", ", Constants.OAUTH_MANDATORY_KEYS2));
            }
        }
        
        public void Authenticate(APIHTTPConnector urlConnection, Object config)
        {
            lock (this)
            {
                if(config != null)
                {
                    JObject tokenConfig = (JObject) config;
                    if(tokenConfig.ContainsKey(Constants.LOCATION) && tokenConfig.ContainsKey(Constants.NAME))
                    {
                        if(tokenConfig[Constants.LOCATION] != null && tokenConfig[Constants.LOCATION].Value<string>().Equals(Constants.HEADER, StringComparison.OrdinalIgnoreCase))
                        {
                            urlConnection.AddHeader(tokenConfig[Constants.NAME].Value<string>(), Constants.OAUTH_HEADER_PREFIX + GetToken());
                        }
                        else if (tokenConfig[Constants.LOCATION] != null &&  tokenConfig[Constants.LOCATION].Value<string>().Equals(Constants.PARAM, StringComparison.OrdinalIgnoreCase))
                        {
                            urlConnection.AddParam(tokenConfig[Constants.NAME].Value<string>(), Constants.OAUTH_HEADER_PREFIX + GetToken());
                        }
                    }
                }
                else
                {
                    urlConnection.AddHeader(Constants.AUTHORIZATION, Constants.OAUTH_HEADER_PREFIX + GetToken());
                }
            }
        }

        private string GetResponseFromServer(Dictionary<string, string> requestParams, String url)
        {
            try
            {
                string USER_AGENT = Constants.USER_AGENT;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                string urlParameters = null;
                if (requestParams != null && requestParams.Count != 0)
                {
                    foreach (KeyValuePair<string, string> param in requestParams)
                    {
                        if (urlParameters == null)
                        {
                            urlParameters = param.Key + "=" + param.Value;
                        }
                        else
                        {
                            urlParameters += "&" + param.Key + "=" + param.Value;
                        }
                    }
                }
                request.UserAgent = USER_AGENT;
                var data = Encoding.UTF8.GetBytes(urlParameters);
                request.ContentType = Constants.URL_ENCODEED;
                request.ContentLength = data.Length;
                request.Method = Constants.REQUEST_METHOD_POST;
                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(data, 0, data.Length);
                }
                SDKLogger.LogInfo(this.ToString(url));
                var response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (System.Exception ex)
            {
                throw new SDKException(ex);
            }
        }

        public string ToString(String url)
	    {
		    return "POST - " + Constants.URL + " = " + url + "."; // No I18N
	    }

        private void RefreshAccessToken(OAuth2 oauthToken, ITokenStore store, String url)
        {
            try
            {
                Dictionary<string, string> requestParams = new Dictionary<string, string>
                {
                    { Constants.CLIENT_ID, oauthToken.ClientId },
                    { Constants.CLIENT_SECRET, oauthToken.ClientSecret },
                    { Constants.GRANT_TYPE, Constants.REFRESH_TOKEN },
                    { Constants.REFRESH_TOKEN, oauthToken.RefreshToken }
                };
                ParseResponse(oauthToken, GetResponseFromServer(requestParams, url));
                store.SaveToken(oauthToken);
            }
            catch (System.Exception ex) when (!(ex is SDKException))
            {
                throw new SDKException(Constants.REFRESH_TOKEN_ERROR, ex);
            }
        }


        private void GenerateAccessToken(OAuth2 oauthToken, ITokenStore store, String url)
        {
            try
            {
                Dictionary<string, string> requestParams = new Dictionary<string, string>
                {
                    { Constants.CLIENT_ID, oauthToken.ClientId },
                    { Constants.CLIENT_SECRET, oauthToken.ClientSecret }
                };
                if (oauthToken.RedirectURL != null)
                {
                    requestParams.Add(Constants.REDIRECT_URI, oauthToken.RedirectURL);
                }
                requestParams.Add(Constants.GRANT_TYPE, Constants.GRANT_TYPE_AUTH_CODE);
                requestParams.Add(Constants.CODE, oauthToken.GrantToken);
                ParseResponse(oauthToken, GetResponseFromServer(requestParams, url));
                store.SaveToken(oauthToken);
            }
            catch (System.Exception ex) when (!(ex is SDKException))
            {
                throw new SDKException(Constants.ACCESS_TOKEN_ERROR, ex);
            }
        }

        private void ParseResponse(OAuth2 oauthToken, string response)
        {
            JObject responseJSON = JObject.Parse(response);
            if (!responseJSON.ContainsKey(Constants.ACCESS_TOKEN))
            {
                throw new SDKException(Constants.INVALID_TOKEN_ERROR, (responseJSON.ContainsKey(Constants.ERROR_KEY))? responseJSON[Constants.ERROR_KEY].ToString() : Constants.NO_ACCESS_TOKEN_ERROR);
            }
            oauthToken.AccessToken = (string)responseJSON[Constants.ACCESS_TOKEN];
            oauthToken.ExpiresIn = GetTokenExpiresIn(responseJSON).ToString();
            if (responseJSON.ContainsKey(Constants.REFRESH_TOKEN))
            {
                oauthToken.RefreshToken = (string)responseJSON[Constants.REFRESH_TOKEN];
            }
        }

        private long GetTokenExpiresIn(JObject response)
        {
            long expiresInTime = response.ContainsKey(Constants.EXPIRES_IN_SEC) ? Convert.ToInt64(response[Constants.EXPIRES_IN]) : Convert.ToInt64(response[Constants.EXPIRES_IN]) * 1000;
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + expiresInTime - 600000;
        }

        public long GetExpiryLapseInMillis(string ExpiryTime)
        {
            long time = Convert.ToInt64(ExpiryTime) - (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            return time;
        }

        public void Remove()
        {
            try
            {
                if (Initializer.GetInitializer() == null)
                {
                    throw new SDKException(Constants.SDK_UNINITIALIZATION_ERROR, Constants.SDK_UNINITIALIZATION_MESSAGE);
                }
                Initializer.GetInitializer().Store.DeleteToken(this.Id);
            }
            catch (System.Exception ex) when (!(ex is SDKException))
            {
                throw ex;
            }
        }

        public static T MergeObjects<T>(T first, T second)
        {
            try
            {
                Type type = typeof(T);
                T result = (T)Activator.CreateInstance(type);
                foreach (PropertyInfo property in type.GetProperties())
                {
                    object value1 = property.GetValue(first);
                    object value2 = property.GetValue(second);
                    Object value = (value1 != null) ? value1 : value2;
                    property.SetValue(result, value);
                }
                return result;
            }
            catch (System.Exception ex)
            {
                throw new SDKException(Constants.MERGE_OBJECT, ex);
            }
        }

        public static bool AreAllObjectsNull(params Object[] objects)
        {
            foreach (Object o in objects)
            {
                if (o != null)
                {
                    return false;
                }
            }
            return true;
        }

        public class Builder
        {
            private string clientId;
            private string clientSecret;
            private string redirectURL;
            private string refreshToken;
            private string grantToken;
            private string accessToken;
            private string id;
            private UserSignature userSignature;
            private AuthenticationSchema authenticationSchema;

            public Builder Id(string id)
            {
                this.id = id;
                return this;
            }

            public Builder ClientId(string clientId)
            {
                Utility.AssertNotNull(clientId, Constants.TOKEN_ERROR, Constants.CLIENT_ID_NULL_ERROR_MESSAGE);
                this.clientId = clientId;
                return this;
            }

            public Builder ClientSecret(string clientSecret)
            {
                Utility.AssertNotNull(clientSecret, Constants.TOKEN_ERROR, Constants.CLIENT_SECRET_NULL_ERROR_MESSAGE);
                this.clientSecret = clientSecret;
                return this;
            }

            public Builder RedirectURL(string redirectURL)
            {
                this.redirectURL = redirectURL;
                return this;
            }

            public Builder RefreshToken(string refreshToken)
            {
                this.refreshToken = refreshToken;
                return this;
            }

            public Builder GrantToken(string grantToken)
            {
                this.grantToken = grantToken;
                return this;
            }

            public Builder AccessToken(string accessToken)
            {
                this.accessToken = accessToken;
                return this;
            }

            public Builder UserSignature(UserSignature userSignature)
            {
                this.userSignature = userSignature;
                return this;
            }

            public Builder AuthenticationSchema(AuthenticationSchema authenticationSchema)
            {
                this.authenticationSchema = authenticationSchema;
                return this;
            }

            public OAuth2 Build()
            {
                if (AreAllObjectsNull(this.grantToken, this.refreshToken, this.id, this.accessToken, this.userSignature, this.authenticationSchema))
			    {
				    throw new SDKException(Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR + " - " + string.Join(", ", Constants.OAUTH_MANDATORY_KEYS));
			    }
			    if (!AreAllObjectsNull(this.grantToken, this.refreshToken))
			    {
				    if (AreAllObjectsNull(this.clientId, this.clientSecret, this.authenticationSchema))
				    {
					    throw new SDKException(Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR + " - " + string.Join(", ", Constants.OAUTH_MANDATORY_KEYS1));
				    }
                    else if(this.accessToken != null && this.authenticationSchema == null)
                    {
                        throw new SDKException(Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR + "-" + Constants.OAUTH_MANDATORY_KEYS_1);
                    }
				    else
				    {
					    Utility.AssertNotNull(this.clientId, Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR + " - " + Constants.CLIENT_ID);
					    Utility.AssertNotNull(this.clientSecret, Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR + " - " + Constants.CLIENT_SECRET);
                        Utility.AssertNotNull(this.authenticationSchema, Constants.MANDATORY_VALUE_ERROR, Constants.MANDATORY_KEY_ERROR + " - " + Constants.AUTHENTICATION_SCHEMA);
				    }
			    }
			    return new OAuth2(this.clientId, this.clientSecret, this.grantToken, this.refreshToken, this.redirectURL, this.id, this.accessToken, this.userSignature, this.authenticationSchema);
            }
        }

    }
}
