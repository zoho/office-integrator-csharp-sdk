using Com.Zoho.Officeintegrator.Util;
using System.Collections.Generic;

namespace Com.Zoho.Officeintegrator.V1
{

	public class EditorSettings : Model
	{
		private string unit;
		private string language;
		private string view;
		private Dictionary<string, int?> keyModified=new Dictionary<string, int?>();

		public string Unit
		{
			/// <summary>The method to get the unit</summary>
			/// <returns>string representing the unit</returns>
			get
			{
				return  this.unit;

			}
			/// <summary>The method to set the value to unit</summary>
			/// <param name="unit">string</param>
			set
			{
				 this.unit=value;

				 this.keyModified["unit"] = 1;

			}
		}

		public string Language
		{
			/// <summary>The method to get the language</summary>
			/// <returns>string representing the language</returns>
			get
			{
				return  this.language;

			}
			/// <summary>The method to set the value to language</summary>
			/// <param name="language">string</param>
			set
			{
				 this.language=value;

				 this.keyModified["language"] = 1;

			}
		}

		public string View
		{
			/// <summary>The method to get the view</summary>
			/// <returns>string representing the view</returns>
			get
			{
				return  this.view;

			}
			/// <summary>The method to set the value to view</summary>
			/// <param name="view">string</param>
			set
			{
				 this.view=value;

				 this.keyModified["view"] = 1;

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