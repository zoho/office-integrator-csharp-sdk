using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This class is the common API response object.
    /// </summary>
    /// <typeparam name="T">A T is POJO class type</typeparam>
    public class APIResponse<T>
    {
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        private int statusCode;
        private Object @object;
        private JObject responseJSON;
        private string statusDescription;

        /// <summary>
        /// Creates an APIResponse&lt;T&gt; class instance with the specified parameters.
        /// </summary>
        /// <param name="headers">A Dictionary&lt;string, string&gt; containing the API response headers. </param>
        /// <param name="statusCode">A int containing the API response HTTP status code.</param>
        /// <param name="Object">A object containing the API response POJO class instance.</param>
        /// <param name="responseJSON">A JObject containing the API response.</param>
        public APIResponse(Dictionary<string, string> headers, int statusCode, Object Object, JObject responseJSON, string statusDescription)
        {
            this.headers = headers;
            this.statusCode = statusCode;
            this.@object = Object;
            this.responseJSON = responseJSON;
            this.statusDescription = statusDescription;
        }

        /// <summary>
        /// Gets or sets the API response headers.
        /// </summary>
        /// <value>A Dictionary&lt;string, string&gt; containing the API response headers.</value>
        /// <returns>A Dictionary&lt;string, string&gt; representing the API response headers.</returns>
        public Dictionary<string, string> Headers
        {
            get
            {
                return this.headers;
            }
        }

        /// <summary>
        /// This is a getter method to get the API response HTTP status code.
        /// </summary>
        /// <returns>A int representing the API response HTTP status code.</returns>
        public int StatusCode
        {
            get
            {
                return this.statusCode;
            }
        }

        /// <summary>
        /// This is a getter method to get the API response Model Interface instance.
        /// </summary>
        /// <returns>A object instance.</returns>
        public Object Model
        {
            get
            {
                return this.@object;
            }
        }

        /// <summary>
        /// This method to get an API response POJO class instance.
        /// </summary>
        /// <returns>A POJO class instance.</returns>
        public T Object
        {
            get
            {
                try
                {
                    if(@object.GetType() == typeof(T))
                    {
                        return (T)Convert.ChangeType(this.@object, typeof(T));
                    }
                    return (T)@object;
                }
                catch (InvalidCastException)
                {
                    return default(T);
                }
            }
        }

        public JObject ResponseJSON
        {
            get
            {
                return this.responseJSON;
            }
        }

        /// <summary>
        /// This is a getter method to get the API response HTTP status description.
        /// </summary>
        /// <returns>A int representing the API response HTTP status description.</returns>
        public string StatusDescription
        {
            get
            {
                return this.statusDescription;
            }
        }
    }
}
