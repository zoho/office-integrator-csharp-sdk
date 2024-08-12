using System;
using System.Collections.Generic;
using System.Diagnostics;
using Org.BouncyCastle.Utilities;

namespace Com.Zoho.Officeintegrator.Util
{
    /// <summary>
    /// This class uses the SDK constants name reference.
    /// </summary>
    public static class Constants
    {
        public static readonly Dictionary<string, string> DATATYPE;
        public static readonly List<int> FaultyResponseCodes;
        static Constants()
        {
            DATATYPE = new Dictionary<string, string>()
            {
                {"com.zoho.spec.template.type.TString", "System.String"},
                {"com.zoho.spec.template.type.TLong", "System.Int64"},
                {"com.zoho.spec.template.type.TMap", "System.Collections.Generic.Dictionary`2[[System.Object],[System.Object]]"},
                {"com.zoho.spec.template.type.TInteger", "System.Int32"},
                {"text", "System.String"}
            };
            FaultyResponseCodes = new List<int>
            {
               204,
               304
            };
        }
        public static readonly string URL = "URL";
        public static readonly string HEADERS = "HEADERS";
        public static readonly string PARAMS = "PARAMS";
        public static readonly string PARAM = "PARAM";
        public static readonly string STATUS_CODE = "STATUS_CODE";
        public static readonly string RESPONSE_JSON = "RESPONSE_JSON";
        public static readonly string RESPONSE_HEADERS = "RESPONSE_HEADERS";
        public static readonly string EXCEPTION_LOG_MSG = "ZOHO - ";
        public static readonly string SDK_ERROR = "ZOHO_INTERNAL_ERROR";
        public static readonly string AUTHENTICATION_FAILURE = "AUTHENTICATION_FAILURE";
        public static readonly string OAUTH_HEADER_PREFIX = "Bearer ";
        public static readonly string CODE_ERROR = "error";
        public static readonly string CODE_SUCCESS = "success";
        public static readonly string MESSAGE = "message";
        public static readonly string CODE = "code";
        public static readonly string STATUS = "status";
        public static readonly string DETAILS = "details";
        public static readonly string DATA = "data";
        public static readonly string USERS = "users";
        public static readonly string TAGS = "tags";
        public static readonly string TAXES = "taxes";
        public static readonly string INFO = "info";
        public static readonly string PER_PAGE = "per_page";
        public static readonly string PAGE = "page";
        public static readonly string COUNT = "count";
        public static readonly string MORE_RECORDS = "more_records";
        public static readonly string ALLOWED_COUNT = "allowed_count";
        public static readonly string INVALID_ID_MSG = "The given id seems to be invalid.";
        public static readonly string INVALID_DATA = "INVALID_DATA";
        public static readonly string API_MAX_RECORDS_MSG = "Cannot process more than 100 records at a time.";
        public static readonly string API_MAX_TAGS_MSG = "Cannot process more than 50 tags at a time.";
        public static readonly string API_MAX_RECORD_TAGS_MSG = "Cannot process more than 10 tags at a time.";
        public static readonly string ACTION = "action";
        public static readonly string DUPLICATE_FIELD = "duplicate_field";
        public static readonly string GMT = "GMT";
        public static readonly string LOG_FILE_NAME = "sdk_logs.log";
        public static readonly string TOKEN_FILE = "sdk_tokens.txt";
        public static readonly string ZOHO_SDK = "X-ZOHO-SDK";
        public static readonly string SDK_VERSION = "1.0.0";
        public static readonly string MODULEPACKAGENAME = "modulePackageName";
        public static readonly string MODULEDETAILS = "moduleDetails";
        public static readonly string DATATYPECONVERTER = "Com.Zoho.Officeintegrator.Util.DataTypeConverter`1[[$type]], OfficeIntegrator";
        public static readonly string CHOICE = "Com.Zoho.Officeintegrator.Util.Choice`1[[$type]], OfficeIntegrator";
        public static readonly string CHOICE_NAME = "Choice";
        public static readonly string CHOICE_NAMESPACE = "Com.Zoho.Officeintegrator.Util.Choice";
        public static readonly int MAX_ALLOWED_FILE_SIZE_IN_MB = 20;
        internal static readonly string GRANT_TYPE_AUTH_CODE = "authorization_code";
        internal static readonly string ACCESS_TOKEN = "access_token";
        internal static readonly string EXPIRES_IN = "expires_in";
        internal static readonly string EXPIRES_IN_SEC = "expires_in_sec";
        internal static readonly string REFRESH_TOKEN = "refresh_token";
        internal static readonly string CLIENT_ID = "client_id";
        internal static readonly string CLIENT_SECRET = "client_secret";
        internal static readonly string REDIRECT_URL = "redirect_url";
        internal static readonly string REDIRECT_URI = "redirect_uri";
        public static readonly Dictionary<string, TraceLevel> LOGGER_LEVELS;
        public static readonly string LOGGER = "Logger";
        public static readonly string LOGGER_NAME = "CSharpSDK Logger";
        public static readonly Dictionary<string, string> ACCESS_TYPE;
        public static readonly string TYPE = "type";
        public static readonly string LIST_NAMESPACE = "System.Collections.Generic.List`1";
        public static readonly string MAP_NAMESPACE = "System.Collections.Generic.Dictionary`2";
        public static readonly string CSHARP_STRING_NAME = "System.String";
        public static readonly string CSHARP_LONG_NAME = "System.Int64";
        public static readonly string CSHARP_INT_NAME = "System.Int32";
        public static readonly string CSHARP_BOOLEAN_NAME = "System.Boolean";
        public static readonly string CSHARP_DOUBLE_NAME = "System.Double";
        public static readonly string CSHARP_FLOAT_NAME = "System.Single";
        public static readonly string CSHARP_DECIMAL_NAME = "System.Decimal";
        public static readonly string CSHARP_OBJECT_NAME = "System.Object";
        public static readonly string CSHARP_NULL_TYPE_NAME = "System.Nullable`1";
        public static readonly string STRUCTURE_NAME = "structure_name";
        public static readonly string NAME = "name";
        public static readonly string INTERFACE = "interface";
        public static readonly string CLASSES = "classes";
        public static readonly string KEYS = "keys";
        public static readonly string LOG_INFO = "INFO";
        public static readonly string LOG_ERROR = "ERROR";
        public static readonly string LOG_WARNING = "WARNING";
        public static readonly string VALUES = "values";
        public static readonly Dictionary<string, string> KEY_VS_INVENTORY_MODULE;
        public static readonly Dictionary<string, string> DEFAULT_MODULENAME_VS_APINAME;
        public static readonly List<string> PHOTO_SUPPORTED_MODULES = new List<string> { "leads", "contacts", "accounts", "products", "vendors" };
        public static readonly string REQUEST_METHOD_GET = "GET";
        public static readonly string REQUEST_METHOD_POST = "POST";
        public static readonly string REQUEST_METHOD_PUT = "PUT";
        public static readonly string REQUEST_METHOD_DELETE = "DELETE";
        public static readonly string REQUEST_METHOD_PATCH = "PATCH";
        public static readonly string REQUEST_CATEGORY_CREATE = "CREATE";
        public static readonly string REQUEST_CATEGORY_UPDATE = "UPDATE";
        public static readonly string REQUEST_CATEGORY_READ = "READ";
        public static readonly string REQUEST_CATEGORY_ACTION = "ACTION";
        public static readonly string APPLICATION_JSON = "application/json";
        public static readonly string READ_ONLY = "read-only";
        public static readonly string WRITE_ONLY = "write-only";
	    public static readonly string UPDATE_ONLY = "update-only";
        public static readonly string IS_KEY_MODIFIED = "IsKeyModified";
        public static readonly string EXCEPTION_IS_KEY_MODIFIED = "Exception in calling IsKeyModified : ";
        public static readonly string STREAM_WRAPPER_CLASS_PATH = "Com.Zoho.Officeintegrator.Util.StreamWrapper";
        public static readonly string FILE_NAMESPACE = "Com.Zoho.Officeintegrator.Util.StreamWrapper";
        public static readonly string MYSQL_HOST = "localhost";
        public static readonly string MYSQL_DATABASE_NAME = "zohooauth";
        public static readonly string MYSQL_TABLE_NAME = "oauthtoken";
        public static readonly string MYSQL_USER_NAME = "root";
        public static readonly string MYSQL_PORT_NUMBER = "3306";
        public static readonly string SERVER = "server";
        public static readonly string USERNAME = "username";
        public static readonly string PASSWORD = "password";
        public static readonly string DATABASE = "database";
        public static readonly string PORT = "port";
        public static readonly string EXPIRY_TIME = "expiry_time";
        public static readonly string TOKEN_STORE = "TOKEN_STORE";
        public static readonly string GET_TOKEN_DB_ERROR = "Exception in GetToken - DBStore : ";
        public static readonly string GET_TOKEN_BY_ID_DB_ERROR = "Exception in GetTokenById - DBStore : Given ID is invalid";
        public static readonly string GET_TOKENS_DB_ERROR = "Exception in GetTokens - DBStore : ";
        public static readonly string SAVE_TOKEN_DB_ERROR = "Exception in SaveToken - DBStore : ";
        public static readonly string DELETE_TOKEN_DB_ERROR = "Exception in DeleteToken - DBStore : ";
        public static readonly string DELETE_TOKENS_DB_ERROR = "Exception in DeleteTokens - DBStore : ";
        public static readonly string USER_MAIL = "user_mail";
        public static readonly string GRANT_TOKEN = "grant_token";
        public static readonly string GET_TOKEN_FILE_ERROR = "Exception in GetToken - FileStore : ";
        public static readonly string GET_TOKEN_BY_ID_FILE_ERROR = "Exception in GetTokenById - FileStore : Given ID is invalid";
        public static readonly string GET_TOKENS_FILE_ERROR = "Exception in getTokens - FileStore : ";
        public static readonly string SAVE_TOKEN_FILE_ERROR = "Exception in SaveToken - FileStore : ";
        public static readonly string DELETE_TOKEN_FILE_ERROR = "Exception in DeleteToken - FileStore : ";
        public static readonly string DELETE_TOKENS_FILE_ERROR = "Exception in deleteTokens - FileStore : ";
        public static readonly string AUTHORIZATION = "Authorization";
        public static readonly string INVALID_CLIENT_ERROR = "INVALID CLIENT ERROR";
        public static readonly string GET_TOKEN_ERROR = "Exception in getting access token";
        public static readonly string ERROR_KEY = "error";
        public static readonly string USER_AGENT = "Mozilla/5.0";
        public static readonly string URL_ENCODEED = "application/x-www-form-urlencoded";
        public static readonly string SAVE_TOKEN_ERROR = "Exception in saving tokens";
        public static readonly string IF_MODIFIED_SINCE = "If-Modified-Since";
        public static readonly string DISCLOSE = "## can't disclose ##";
        public static readonly string FIELD = "field";
        public static readonly string CLASS = "class";
        public static readonly string INDEX = "index";
        public static readonly string ACCEPTED_VALUES = "accepted-values";
        public static readonly string UNACCEPTED_VALUES_ERROR = "UNACCEPTED VALUES ERROR";
        public static readonly string UNIQUE = "unique";
        public static readonly string FIRST_INDEX = "first-index";
        public static readonly string NEXT_INDEX = "next-index";
        public static readonly string UNIQUE_KEY_ERROR = "UNIQUE KEY ERROR";
        public static readonly string MIN_LENGTH = "min-length";
        public static readonly string MAX_LENGTH = "max-length";
        public static readonly string MAXIMUM_LENGTH = "maximum-length";
        public static readonly string MAXIMUM_LENGTH_ERROR = "MAXIMUM LENGTH ERROR";
        public static readonly string MINIMUM_LENGTH = "minimum-length";
        public static readonly string MINIMUM_LENGTH_ERROR = "MINIMUM LENGTH ERROR";
        public static readonly string REGEX = "regex";
        public static readonly string INSTANCE_NUMBER = "instance-number";
        public static readonly string REGEX_MISMATCH_ERROR = "REGEX MISMATCH ERROR";
        public static readonly string CONTENT_DISPOSITION = "Content-Disposition";
        public static readonly string REQUIRED = "required";
        public static readonly string SET_KEY_MODIFIED = "SetKeyModified";
        public static readonly string EXCEPTION_SET_KEY_MODIFIED = "Exception in calling SetKeyModified : ";
        public static readonly string MANDATORY_KEY_ERROR = "Value missing or null for mandatory key(s) :";
        public static readonly string PRIMARY_KEY_ERROR = "Value missing or null for required key(s) : ";
        public static readonly string MANDATORY_VALUE_ERROR = "MANDATORY VALUE ERROR";
        public static readonly string POST_CONVERT = "PostConvert";
        public static readonly string _TYPE = "$type";
        public static readonly string OPTION_NAMESPACE = "Com.Zoho.Officeintegrator.Util.Option";
        public static readonly string PACKAGE_NAMESPACE = "Com.Zoho.Officeintegrator";
        public static readonly string MODULE = "module";
        public static readonly string KEY_VALUES = "keyValues";
        public static readonly string KEY_MODIFIED = "keyModified";
        public static readonly string SYSTEM_DATETIME = "System.DateTime";
        public static readonly string SYSTEM_DATETIME_OFFSET = "System.DateTimeOffset";
        public static readonly string PRE_CONVERT = "PreConvert";
        public static readonly string JSON_DETAILS_FILE_PATH = ".Resources.JSONDetails.json";
        public static readonly string EMAIL_REGEX = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
        public static readonly string EMAIL = "email";
        public static readonly string EXPECTED_TYPE = "expected-type";
        public static readonly string USER_ERROR = "USER ERROR";
        public static readonly string LOOKUP = "lookup";
        public static readonly string REFRESH_TOKEN_MESSAGE = "Access Token has expired. Hence refreshing.";
        public static readonly string CANT_DISCLOSE = " ## can't disclose ## ";
        public static readonly string SET_API_URL_EXCEPTION = "Exception in setting API URL : ";
        public static readonly string AUTHENTICATION_EXCEPTION = "Exception in authenticating current request : ";
        public static readonly string FORM_REQUEST_EXCEPTION = "Exception in forming request body : ";
        public static readonly string API_CALL_EXCEPTION = "Exception in current API call execution : ";
        public static readonly string API_ERROR_RESPONSE = "Error response :";
        public static readonly string INVALID_URL_ERROR = "Invalid URL Error : ";
        public static readonly string HTTP = "http";
        public static readonly string CONTENT_API_URL = "content.zohoapis.com";
        public static readonly string FILEBODYWRAPPER = "FileBodyWrapper";
        public static readonly string ATTACHMENT_ID = "attachment_id";
        public static readonly string FILE_ID = "file_id";
        public static readonly string FIELD_DETAILS_DIRECTORY = "Resources";
        public static readonly string DELETE_FIELD_FILE_ERROR = "Exception in deleting Current User Fields file : ";
        public static readonly string DELETE_FIELD_FILES_ERROR = "Exception in deleting all Fields files : ";
        public static readonly string DELETE_MODULE_FROM_FIELDFILE_ERROR = "Exception in deleting module from Fields file : ";
        public static readonly string INITIALIZATION_SUCCESSFUL = "Initialization successful ";
        public static readonly string INITIALIZATION_SWITCHED = "Initialization switched ";
        public static readonly string IN_ENVIRONMENT = " in Environment : ";
        public static readonly string FOR_EMAIL_ID = "for Email Id : ";
        public static readonly string JSON_DETAILS_ERROR = "ERROR IN READING JSONDETAILS FILE";
        public static readonly string RESOURCE_PATH_ERROR_MESSAGE = "Resource Path MUST NOT be null/empty.";
        public static readonly string RESOURCE_PATH = "EMPTY_RESOURCE_PATH";
        public static readonly string INITIALIZATION_ERROR = "Exception in initialization : ";
        public static readonly string FIELDS_LAST_MODIFIED_TIME = "FIELDS-LAST-MODIFIED-TIME";
        public static readonly string EXCEPTION = "Exception";
        public static readonly string UNDERSCORE = "_";
        public static readonly string API_NAME = "api_name";
        public static readonly int NO_CONTENT_STATUS_CODE = 204;
        public static readonly int NOT_MODIFIED_STATUS_CODE = 304;
        public static readonly string HREF = "href";
        public static readonly string API_EXCEPTION = "API_EXCEPTION";
        public static readonly string COMMENTS = "Comments";
        public static readonly string PRIMARY = "primary";
        public static readonly string ID = "id";
        public static readonly string SDK_UNINITIALIZATION_ERROR = "SDK UNINITIALIZED ERROR";
        public static readonly string SDK_UNINITIALIZATION_MESSAGE = "SDK is UnInitialized";
        public static readonly string SKIP_MANDATORY = "skip_mandatory";
        public static readonly string REQUIRED_IN_UPDATE = "required_in_update";
        public static readonly string REFRESH_SINGLE_MODULE_FIELDS_ERROR = "Exception in refreshing fields of module : ";
        public static readonly string REFRESH_ALL_MODULE_FIELDS_ERROR = "Exception in refreshing fields of all modules : ";
        public static readonly string NULL_VALUE = "null";
        public static readonly string UNSUPPORTED_IN_API = "API UNSUPPORTED OPERATION";
        public static readonly string UNSUPPORTED_IN_API_MESSAGE = " Operation is not supported by API";
        public static readonly string FORMULA = "formula";
        public static readonly string HEADER_NULL_ERROR = "NULL HEADER ERROR";
        public static readonly string PARAM_NAME_NULL_ERROR = "NULL PARAM NAME ERROR";
        public static readonly string HEADER_NAME_NULL_ERROR = "NULL HEADER NAME ERROR";
        public static readonly string PARAM_NAME_NULL_ERROR_MESSAGE = "Param Name MUST NOT be null";
        public static readonly string HEADER_NAME_NULL_ERROR_MESSAGE = "Header Name MUST NOT be null";
        public static readonly string PARAM_INSTANCE_NULL_ERROR = "Param<T> Instance MUST NOT be null";
        public static readonly string HEADER_INSTANCE_NULL_ERROR = "Header<T> Instance MUST NOT be null";
        public static readonly string NULL_VALUE_ERROR_MESSAGE = " MUST NOT be null";
        public static readonly string USERSIGNATURE_ERROR_MESSAGE = "UserSignature MUST NOT be null.";
        public static readonly string ENVIRONMENT_ERROR_MESSAGE = "Environment MUST NOT be null.";
        public static readonly string TOKEN_ERROR_MESSAGE = "Token MUST NOT be null.";
        public static readonly string STORE_ERROR_MESSAGE = "Store MUST NOT be null.";
        public static readonly string INITIALIZATION_EXCEPTION = "Exception in initialization : ";
        public static readonly string SWITCH_USER_ERROR = "SWITCH USER ERROR";
        public static readonly string REQUEST_PROXY_ERROR = "REQUESTPROXY ERROR";
        public static readonly string HOST_ERROR_MESSAGE = "Host MUST NOT be null.";
        public static readonly string PORT_ERROR_MESSAGE = "Port MUST NOT be null.";
        public static readonly string USER_SIGNATURE_ERROR = "USERSIGNATURE ERROR";
        public static readonly string PARAMETER_NULL_ERROR = "NULL PARAMETER ERROR";
        public static readonly string PROXY_SETTINGS = "Proxy settings - ";
        public static readonly string PROXY_HOST = "Host: ";
        public static readonly string PROXY_PORT = "Port: ";
        public static readonly string PROXY_USER = "User: ";
        public static readonly string PROXY_DOMAIN = "Domain: ";
        public static readonly string USER_MAIL_NULL_ERROR = "USER MAIL NULL ERROR";
        public static readonly string USER_MAIL_NULL_ERROR_MESSAGE = "User Mail MUST NOT be null. Use setUserMail() to set value.";
        public static readonly string JSON_FILE_EXTENSION = ".json";
        public static readonly string SDK_CONFIG_ERROR_MESSAGE = "sdkConfig MUST NOT be null.";
        public static readonly string FILE_ERROR = "file_error";
        public static readonly string FILE_DOES_NOT_EXISTS = "file does not exists";
        public static readonly string GIVEN_LENGTH = "given-length";
        public static readonly string CONSENT_LOOKUP = "consent_lookup";
        public static readonly string TOKEN_ERROR = "TOKEN ERROR";
        public static readonly string CLIENT_ID_NULL_ERROR_MESSAGE = "ClientId MUST NOT be null";
        public static readonly string CLIENT_SECRET_NULL_ERROR_MESSAGE = "ClientSecret MUST NOT be null";
        public static readonly string INVALID_TOKEN_ERROR = "INVALID TOKEN ERROR";
        public static readonly string NO_ACCESS_TOKEN_ERROR = "ACCESS TOKEN IS NOT PRESENT IN RESPONSE";
        public static readonly string MULTI_SELECT_LOOKUP = "multiselectlookup";
        public static readonly string MULTI_USER_LOOKUP = "multiuserlookup";
        public static readonly string RESOURCE_PATH_INVALID_ERROR_MESSAGE = "Resource Path MUST be a valid directory.";
        public static readonly string GENERATED_TYPE = "generated_type";
        public static readonly string GENERATED_TYPE_CUSTOM = "custom";
        public static readonly string UPLOAD_PHOTO_UNSUPPORTED_ERROR = "UPLOAD PHOTO UNSUPPORTED MODULE";
        public static readonly string UPLOAD_PHOTO_UNSUPPORTED_MESSAGE = "Photo Upload Operation is not supported by the module: ";
        public static readonly string GIVEN_TYPE = "given-type";
        public static readonly string TYPE_ERROR = "TYPE ERROR";
        public static readonly string GIVEN_VALUE = "given-value";
        public static readonly List<string> GENERATE_REQUEST_BODY = new List<string>() { Constants.REQUEST_METHOD_PATCH, Constants.REQUEST_METHOD_POST, Constants.REQUEST_METHOD_PUT };
        public static readonly List<string> OAUTH_MANDATORY_KEYS = new List<string>() { "grantToken", "refreshToken", "id", "accessToken" };
        public static readonly string DISCOUNT = "Discount";
        public static readonly string CSHARP = "csharp_";
        internal static readonly string GRANT_TYPE = "grant_type";
        public static readonly List<string> OAUTH_MANDATORY_KEYS_1 = new List<string>() { "AuthenticationType" }; // No I18N
        public static readonly string ACCESS_TOKEN_USING_REFRESH_TOKEN_MESSAGE = "Access token can be generated by refresh token"; // No I18N
        public static readonly string ACCESS_TOKEN_USING_GRANT_TOKEN_MESSAGE = "Access token can be generated by grant token."; // No I18N
        public static readonly string GET_TOKEN_BY_USER_NAME_ERROR = "Exception in GetToken() - OAuthToken : Given user_name token details not found, Please give required item"; // No I18N
        public static readonly List<string> OAUTH_MANDATORY_KEYS2 = new List<string>() { Constants.GRANT_TOKEN, Constants.REFRESH_TOKEN };
        public static readonly string MERGE_OBJECT = "MERGE_OBJECT"; // No I18N
        public static readonly string LOCATION = "location"; // No I18N
        public static readonly string HEADER = "header"; // No I18N
        public static readonly string AUTHENTICATION_SCHEMA = "authentication_schema"; // No I18N
        public static readonly List<string> OAUTH_MANDATORY_KEYS1 = new List<string>() { Constants.CLIENT_ID, Constants.CLIENT_SECRET };
        public static readonly string ACCESS_TOKEN_ERROR = "Exception in generate Access token method: ";
        public static readonly string REFRESH_TOKEN_ERROR = "Exception in refresh the Access token : ";
        public static readonly string RESPONSE = "response";// No I18N
        public static readonly string REQUEST = "request";// No I18N
        public static readonly string SCHEMA_NAME = "schema_name"; // No I18N
        public static readonly string AUTHENTICATION = "authentication"; // No I18N
        public static readonly string SDK_OPERATIONS_METHOD_DETAILS_NOT_FOUND_IN_JSON_DETAILS_FILE = "SDK Operations method details not found in JSON details file"; // No I18N
	    public static readonly string SDK_OPERATIONS_CLASS_DETAILS_NOT_FOUND_IN_JSON_DETAILS_FILE = "SDK Operations class details not found in JSON details file"; // No I18N
        public static readonly string PICKLIST = "picklist"; // No I18N
        public static readonly string UNIQUE_FOR = "unique_for"; // No I18N
        public static readonly string ALL ="all"; // No I18N
        public static readonly string MEMBERS = "members";// No I18N
        public static readonly string TFILE_TYPE = "com.zoho.api.spec.template.type.TFile";// No I18N
        public static readonly string SPEC_TYPE = "spec_type"; // No I18N
        public static readonly string GROUP_TYPE = "group_type";// No I18N
        public static readonly string REQUEST_SUPPORTED = "request_supported"; // No I18N
        public static readonly string REQUIRED_FOR = "required_for"; // No I18N
        public static readonly string EXTRA_DETAILS = "extra_details";// No I18N
        public static readonly string TARRAY_TYPE = "com.zoho.api.spec.template.type.TArray";// No I18N
        public static readonly string ORDERED_STRUCTURES = "ordered_structures";// No I18N
        public static readonly string ARRAY_OF = "array-of";// No I18N
        public static readonly string SUB_TYPE = "sub_type"; // No I18N
        public static readonly string HEADER_EXCEPTION = "Exception in forming request Header : ";
        public static readonly string PARAM_EXCEPTION = "Exception in forming request Parameter : ";
        public static readonly string USER_NAME = "user_name"; // No I18N
        public static readonly string GET_TOKEN_FILE_ERROR1 = "Exception in FileStore : Given user authentication details not found."; // No I18N
        public static readonly string TOKEN_BY_ID_FILE_ERROR = "Exception in FileStore : Given Id not found"; // No I18N
        public static readonly string GET_TOKEN_DB_ERROR1 = "Exception in DBStore : Given user authentication details not found."; // No I18N
        public static readonly string GENERATE_TOKEN_ID_ERROR = "Exception in generateId() - DBStore"; // No I18N
    }
}
