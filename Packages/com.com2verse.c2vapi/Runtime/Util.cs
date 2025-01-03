//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Com2Verse.HttpHelper;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;


namespace Com2Verse.WebApi
{
    
    
    public sealed class Util : Singleton<Util>
    {
        
        private string _accessToken;
        
        [UsedImplicitly()]
        private Util()
        {
        }
        
        public string AccessToken
        {
            get
            {
                return _accessToken;
            }
            set
            {
                _accessToken = value;
            }
        }
        
        public bool TrySetAuthToken()
        {
            if (!string.IsNullOrWhiteSpace(_accessToken))
            {
                Client.Auth.SetTokenAuthentication(HttpHelper.Util.MakeTokenAuthInfo(AccessToken));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
