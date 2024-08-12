
using System.Collections.Generic;
using Com.Zoho.API;

namespace Com.Zoho.API.Authenticator.Store
{
    /// <summary>
    /// This interface stores the user token details.
    /// </summary>
    public interface ITokenStore
    {
        IToken FindToken(IToken token);

        IToken FindTokenById(string id);

        void SaveToken(IToken token);

        void DeleteToken(string id);

        List<IToken> GetTokens();

        void DeleteTokens();
    }
}
