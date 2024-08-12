using System;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.API.Authenticator
{
	public abstract class AuthenticationSchema
	{
        public abstract AuthenticationType GetAuthenticationType();
        public abstract String GetTokenUrl();
        public abstract String GetRefreshUrl();
        public abstract String GetSchema();
    }
}
