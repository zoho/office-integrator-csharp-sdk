using Com.Zoho.API.Authenticator;

namespace Com.Zoho.Officeintegrator.V1
{

	public class Authentication
	{
		public class TokenFlow : AuthenticationSchema
		{
			/// <summary>The method to get Token Url</summary>
			/// <returns>string representing the TokenUrl</returns>
			public override string GetTokenUrl()
			{
				return "/zest/v1/__internal/ticket";


			}

			/// <summary>The method to get Authentication Url</summary>
			/// <returns>string representing the AuthenticationUrl</returns>
			public string GetAuthenticationUrl()
			{
				return "";


			}

			/// <summary>The method to get Refresh Url</summary>
			/// <returns>string representing the RefreshUrl</returns>
			public override string GetRefreshUrl()
			{
				return "";


			}

			/// <summary>The method to get Schema</summary>
			/// <returns>string representing the Schema</returns>
			public override string GetSchema()
			{
				return "TokenFlow";


			}

			/// <summary>The method to get Authentication Type</summary>
			/// <returns>Instance of AuthenticationType</returns>
			public override AuthenticationType GetAuthenticationType()
			{
				return AuthenticationType.TOKEN;


			}


		}

	}
}