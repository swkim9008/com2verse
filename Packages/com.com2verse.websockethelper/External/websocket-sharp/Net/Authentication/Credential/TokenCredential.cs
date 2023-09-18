// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TokenCredential.cs
//  * Developer:	yangsehoon
//  * Date:		2023-07-14 오후 5:58
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;

namespace WebSocketSharp.Net
{
  /// <summary>
  /// Provides the credentials for the Token authentication.
  /// </summary>
	public class TokenCredential : NetworkCredential
	{
		private string _token;

#region Public Properties
    public string Token
    {
      get { return _token ?? String.Empty; }

      internal set { _token = value; }
    }
#endregion

    #region Public Constructors

    public TokenCredential(string token)
    {
      _token = token;
    }

    #endregion
	}
}
