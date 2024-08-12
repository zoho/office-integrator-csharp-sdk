using System;
using Com.Zoho.API.Authenticator;

namespace Com.Zoho.Officeintegrator.Dc
{
    public abstract class Environment
    {
        public abstract string GetUrl();

        public abstract string GetDc();

        public abstract Location? GetLocation();
        
        public abstract String GetName();
        
        public abstract String GetValue();
    }
}
