namespace Com.Zoho.Officeintegrator
{
    /// <summary>
    /// This class represents the user email.
    /// </summary>
    public class UserSignature
    {
        private string name;

        public UserSignature(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }
    }
}
