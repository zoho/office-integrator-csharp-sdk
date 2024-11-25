using Com.Zoho.Officeintegrator.Util;
using System.Collections.Generic;

namespace Com.Zoho.Officeintegrator.V1
{

	public class DocumentConversionParameters : Model
	{
		private StreamWrapper document;
		private string url;
		private string password;
		private DocumentConversionOutputOptions outputOptions;
		private Dictionary<string, int?> keyModified=new Dictionary<string, int?>();

		public StreamWrapper Document
		{
			/// <summary>The method to get the document</summary>
			/// <returns>Instance of StreamWrapper</returns>
			get
			{
				return  this.document;

			}
			/// <summary>The method to set the value to document</summary>
			/// <param name="document">Instance of StreamWrapper</param>
			set
			{
				 this.document=value;

				 this.keyModified["document"] = 1;

			}
		}

		public string Url
		{
			/// <summary>The method to get the url</summary>
			/// <returns>string representing the url</returns>
			get
			{
				return  this.url;

			}
			/// <summary>The method to set the value to url</summary>
			/// <param name="url">string</param>
			set
			{
				 this.url=value;

				 this.keyModified["url"] = 1;

			}
		}

		public string Password
		{
			/// <summary>The method to get the password</summary>
			/// <returns>string representing the password</returns>
			get
			{
				return  this.password;

			}
			/// <summary>The method to set the value to password</summary>
			/// <param name="password">string</param>
			set
			{
				 this.password=value;

				 this.keyModified["password"] = 1;

			}
		}

		public DocumentConversionOutputOptions OutputOptions
		{
			/// <summary>The method to get the outputOptions</summary>
			/// <returns>Instance of DocumentConversionOutputOptions</returns>
			get
			{
				return  this.outputOptions;

			}
			/// <summary>The method to set the value to outputOptions</summary>
			/// <param name="outputOptions">Instance of DocumentConversionOutputOptions</param>
			set
			{
				 this.outputOptions=value;

				 this.keyModified["output_options"] = 1;

			}
		}

		/// <summary>The method to check if the user has modified the given key</summary>
		/// <param name="key">string</param>
		/// <returns>int? representing the modification</returns>
		public int? IsKeyModified(string key)
		{
			if((( this.keyModified.ContainsKey(key))))
			{
				return  this.keyModified[key];

			}
			return null;


		}

		/// <summary>The method to mark the given key as modified</summary>
		/// <param name="key">string</param>
		/// <param name="modification">int?</param>
		public void SetKeyModified(string key, int? modification)
		{
			 this.keyModified[key] = modification;


		}


	}
}