
using System;
using Com.Zoho.Officeintegrator.Util;

namespace Com.Zoho.API.Authenticator
{
    /// <summary>
    /// This interface verifies and sets token to APIHTTPConnector instance.
    /// </summary>
    public interface IToken
    {
        void Authenticate(APIHTTPConnector urlConnection, Object config);

        void Remove();

        void GenerateToken();

        string GetId();

        AuthenticationSchema GetAuthenticationSchema();
    }

    public enum Location
    {
        HEADER, PARAM, VARIABLE
    }

    public enum AuthenticationType
    {
        OAUTH2, TOKEN
    }
}
