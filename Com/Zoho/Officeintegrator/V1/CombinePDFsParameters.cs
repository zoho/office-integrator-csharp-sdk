using Com.Zoho.Officeintegrator.Util;
using System.Collections.Generic;

namespace Com.Zoho.Officeintegrator.V1
{

	public class CombinePDFsParameters : Model
	{
		private Dictionary<string, object> inputOptions;
		private CombinePDFsOutputSettings outputSettings;
		private Dictionary<string, int?> keyModified=new Dictionary<string, int?>();

		public Dictionary<string, object> InputOptions
		{
			/// <summary>The method to get the inputOptions</summary>
			/// <returns>Dictionary representing the inputOptions<String,Object></returns>
			get
			{
				return  this.inputOptions;

			}
			/// <summary>The method to set the value to inputOptions</summary>
			/// <param name="inputOptions">Dictionary<string,object></param>
			set
			{
				 this.inputOptions=value;

				 this.keyModified["input_options"] = 1;

			}
		}

		public CombinePDFsOutputSettings OutputSettings
		{
			/// <summary>The method to get the outputSettings</summary>
			/// <returns>Instance of CombinePDFsOutputSettings</returns>
			get
			{
				return  this.outputSettings;

			}
			/// <summary>The method to set the value to outputSettings</summary>
			/// <param name="outputSettings">Instance of CombinePDFsOutputSettings</param>
			set
			{
				 this.outputSettings=value;

				 this.keyModified["output_settings"] = 1;

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