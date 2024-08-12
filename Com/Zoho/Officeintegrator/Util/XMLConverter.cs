using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This class processes the API response object to the POJO object and POJO object to an XML object.
    /// </summary>
    public class XMLConverter : Converter
    {
        public XMLConverter(CommonAPIHandler commonAPIHandler): base(commonAPIHandler)
        {
        }
        
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
            throw new NotImplementedException();
        }

        public override object GetResponse(object response, string pack, string groupType)
        {
            throw new NotImplementedException();
        }
    }
}
