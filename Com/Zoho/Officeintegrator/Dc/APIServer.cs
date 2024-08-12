using Com.Zoho.API.Authenticator;

namespace Com.Zoho.Officeintegrator.Dc
{

	public class APIServer
	{
		public class Production : Environment
		{
			private string serverDomain;

			/// <summary>			/// Creates an instance of Production with the given parameters
			/// <param name="serverDomain">string</param>
			
			public Production(string serverDomain)
			{
				 this.serverDomain=serverDomain;


			}

			/// <summary>The method to get Url</summary>
			/// <returns>string representing the Url</returns>
			public override string GetUrl()
			{
				return "" + this.serverDomain + "";


			}

			/// <summary>The method to get dc</summary>
			/// <returns>string representing the dc</returns>
			public override string GetDc()
			{
				return "alldc";


			}

			/// <summary>The method to get location</summary>
			/// <returns>Instance of Location</returns>
			public override Location? GetLocation()
			{
				return null;


			}

			/// <summary>The method to get name</summary>
			/// <returns>string representing the name</returns>
			public override string GetName()
			{
				return "";


			}

			/// <summary>The method to get value</summary>
			/// <returns>string representing the value</returns>
			public override string GetValue()
			{
				return "";


			}


		}

	}
}