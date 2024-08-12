using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.Officeintegrator.Util
{
	public class TextConverter : Converter
    {
		public TextConverter(CommonAPIHandler commonAPIHandler) : base(commonAPIHandler) { }

        public override object GetWrappedRequest(object response, JObject pack)
        {
            throw new NotImplementedException();
        }

        public override object FormRequest(object requestInstance, string pack, int? instanceNumber, JObject memberDetail, string groupType)
        {
            throw new NotImplementedException();
        }

        public override void AppendToRequest(HttpWebRequest requestBase, object requestObject)
        {
            throw new NotImplementedException();
        }

        public override List<object> GetWrappedResponse(object response, JArray pack)
        {
            HttpWebResponse responseEntity = ((HttpWebResponse)response);
            string responseString = new StreamReader(responseEntity.GetResponseStream()).ReadToEnd();
            responseEntity.Close();
            if (responseString != null && !string.IsNullOrEmpty(responseString) && !string.IsNullOrWhiteSpace(responseString))
            {
				return new List<object>() { GetResponse(responseString, null, null), null };
			}
			return null;
        }

		public override object GetResponse(object response, string pack, string groupType)
        {
            return response;
        }
    }
}

