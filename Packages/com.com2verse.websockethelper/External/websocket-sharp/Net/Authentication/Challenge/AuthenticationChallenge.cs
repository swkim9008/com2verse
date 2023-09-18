#region License
/*
 * AuthenticationChallenge.cs
 *
 * The MIT License
 *
 * Copyright (c) 2013-2014 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System;
using System.Collections.Specialized;

namespace WebSocketSharp.Net
{
  internal abstract class AuthenticationChallenge : AuthenticationBase
  {
    #region Private Constructors

    protected AuthenticationChallenge (AuthenticationSchemes scheme, NameValueCollection parameters)
      : base (scheme, parameters)
    {
    }

    #endregion

    #region Internal Constructors

    protected internal AuthenticationChallenge (AuthenticationSchemes scheme, string realm)
      : base (scheme, new NameValueCollection ())
    {
      Parameters["realm"] = realm;
      if (scheme == AuthenticationSchemes.Digest) {
        Parameters["nonce"] = CreateNonceValue ();
        Parameters["algorithm"] = "MD5";
        Parameters["qop"] = "auth";
      }
    }

    #endregion

    #region Internal Methods

    internal static AuthenticationChallenge Parse (string value)
    {
      var chal = value.Split (new[] { ' ' }, 2);
      if (chal.Length != 2)
        return null;

      var schm = chal[0].ToLower ();
      switch (schm)
      {
        case "basic":
          return new BasicChallenge(AuthenticationSchemes.Basic, ParseParameters(chal[1]));
        case "digest":
          return new DigestChallenge(AuthenticationSchemes.Digest, ParseParameters(chal[1]));
        case "bearer":
          throw new Exception("Bearer Challenge is not implemented");
        default:
          return null;
      }
    }

    internal static AuthenticationChallenge CreateAuthenticationChallenge(AuthenticationSchemes scheme, string realm)
    {
      switch (scheme)
      {
        case AuthenticationSchemes.Basic:
          return new BasicChallenge(scheme, realm);
        case AuthenticationSchemes.Digest:
          return new DigestChallenge(scheme, realm);
        case AuthenticationSchemes.Bearer:
          return new MockBearerChallenge(scheme, realm);
        default:
          throw new ($"challenge for scheme : {scheme} is not implemented");
      }
    }

    #endregion
  }
}
