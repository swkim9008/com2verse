#region License
/*
 * AuthenticationResponse.cs
 *
 * ParseBasicCredentials is derived from System.Net.HttpListenerContext.cs of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace WebSocketSharp.Net
{
  internal abstract class AuthenticationResponse : AuthenticationBase
  {
    #region Private Fields

    private uint _nonceCount;

    #endregion

    #region Private Constructors

    protected AuthenticationResponse (AuthenticationSchemes scheme, NameValueCollection parameters) : base (scheme, parameters) { }

    #endregion

    #region Internal Constructors

    internal AuthenticationResponse (NetworkCredential credentials)
      : this (AuthenticationSchemes.Basic, new NameValueCollection (), credentials, 0)
    {
    }

    internal AuthenticationResponse (
      AuthenticationChallenge challenge, NetworkCredential credentials, uint nonceCount)
      : this (challenge.Scheme, challenge.Parameters, credentials, nonceCount)
    {
    }

    internal AuthenticationResponse (
      AuthenticationSchemes scheme,
      NameValueCollection parameters,
      NetworkCredential credentials,
      uint nonceCount)
      : base (scheme, parameters)
    {
      switch (scheme)
      {
        case AuthenticationSchemes.Basic:
        case AuthenticationSchemes.Digest:
          BasicCredential basicCredential = (credentials as BasicCredential);
          Parameters["username"] = basicCredential.Username;
          Parameters["password"] = basicCredential.Password;
          Parameters["uri"] = basicCredential.Domain;
          
          break;
        case AuthenticationSchemes.Bearer:
          TokenCredential tokenCredential = (credentials as TokenCredential);
          Parameters["token"] = tokenCredential.Token;
          break;
      }
      _nonceCount = nonceCount;
      if (scheme == AuthenticationSchemes.Digest)
        initAsDigest ();
    }

    #endregion

    #region Internal Properties

    internal uint NonceCount {
      get {
        return _nonceCount < UInt32.MaxValue
               ? _nonceCount
               : 0;
      }
    }

    #endregion

    #region Public Properties

    public string Cnonce {
      get {
        return Parameters["cnonce"];
      }
    }

    public string Nc {
      get {
        return Parameters["nc"];
      }
    }

    public string Password {
      get {
        return Parameters["password"];
      }
    }

    public string Response {
      get {
        return Parameters["response"];
      }
    }

    public string Uri {
      get {
        return Parameters["uri"];
      }
    }

    public string UserName {
      get {
        return Parameters["username"];
      }
    }

    #endregion

    #region Private Methods

    private static string createA1 (string username, string password, string realm)
    {
      return String.Format ("{0}:{1}:{2}", username, realm, password);
    }

    private static string createA1 (
      string username, string password, string realm, string nonce, string cnonce)
    {
      return String.Format (
        "{0}:{1}:{2}", hash (createA1 (username, password, realm)), nonce, cnonce);
    }

    private static string createA2 (string method, string uri)
    {
      return String.Format ("{0}:{1}", method, uri);
    }

    private static string createA2 (string method, string uri, string entity)
    {
      return String.Format ("{0}:{1}:{2}", method, uri, hash (entity));
    }

    private static string hash (string value)
    {
      var src = Encoding.UTF8.GetBytes (value);
      var md5 = MD5.Create ();
      var hashed = md5.ComputeHash (src);

      var res = new StringBuilder (64);
      foreach (var b in hashed)
        res.Append (b.ToString ("x2"));

      return res.ToString ();
    }

    private void initAsDigest ()
    {
      var qops = Parameters["qop"];
      if (qops != null) {
        if (qops.Split (',').Contains (qop => qop.Trim ().ToLower () == "auth")) {
          Parameters["qop"] = "auth";
          Parameters["cnonce"] = CreateNonceValue ();
          Parameters["nc"] = String.Format ("{0:x8}", ++_nonceCount);
        }
        else {
          Parameters["qop"] = null;
        }
      }

      Parameters["method"] = "GET";
      Parameters["response"] = CreateRequestDigest (Parameters);
    }

    #endregion

    #region Internal Methods

    internal static AuthenticationResponse CreateAuthenticationResponse(AuthenticationChallenge challenge, NetworkCredential credentials, uint nonceCount)
    {
      switch (challenge.Scheme)
      {
        case AuthenticationSchemes.Basic:
          return new BasicResponse(challenge, credentials, nonceCount);
        case AuthenticationSchemes.Digest:
          return new DigestResponse(challenge, credentials, nonceCount);
        case AuthenticationSchemes.Bearer:
          return new BearerResponse(challenge, credentials, nonceCount);
      }

      return null;
    }

    internal static AuthenticationResponse CreateAuthenticationResponse(NetworkCredential credentials)
    {
      switch (credentials)
      {
        case BasicCredential basicCredential:
          return new BasicResponse(AuthenticationSchemes.Basic, new NameValueCollection(), credentials, 0);
        case TokenCredential tokenCredential:
          return new BearerResponse(AuthenticationSchemes.Bearer, new NameValueCollection(), credentials, 0);
      }

      return null;
    }
    
    internal static string CreateRequestDigest (NameValueCollection parameters)
    {
      var user = parameters["username"];
      var pass = parameters["password"];
      var realm = parameters["realm"];
      var nonce = parameters["nonce"];
      var uri = parameters["uri"];
      var algo = parameters["algorithm"];
      var qop = parameters["qop"];
      var cnonce = parameters["cnonce"];
      var nc = parameters["nc"];
      var method = parameters["method"];

      var a1 = algo != null && algo.ToLower () == "md5-sess"
               ? createA1 (user, pass, realm, nonce, cnonce)
               : createA1 (user, pass, realm);

      var a2 = qop != null && qop.ToLower () == "auth-int"
               ? createA2 (method, uri, parameters["entity"])
               : createA2 (method, uri);

      var secret = hash (a1);
      var data = qop != null
                 ? String.Format ("{0}:{1}:{2}:{3}:{4}", nonce, nc, cnonce, qop, hash (a2))
                 : String.Format ("{0}:{1}", nonce, hash (a2));

      return hash (String.Format ("{0}:{1}", secret, data));
    }

    internal static AuthenticationResponse Parse (string value)
    {
      try {
        var cred = value.Split (new[] { ' ' }, 2);
        if (cred.Length != 2)
          return null;

        var schm = cred[0].ToLower ();

        switch (schm)
        {
          case "basic":
            return new BasicResponse(AuthenticationSchemes.Basic, BasicResponse.ParseBasicCredentials(cred[1]));
          case "digest":
            return new DigestResponse(AuthenticationSchemes.Digest, ParseParameters(cred[1]));
          case "bearer":
            throw new Exception("Bearer Challenge is not implemented");
          default:
            return null;
        }
      }
      catch {
      }

      return null;
    }

    #endregion

    #region Public Methods

    public IIdentity ToIdentity ()
    {
      var schm = Scheme;
      return schm == AuthenticationSchemes.Basic
             ? new HttpBasicIdentity (Parameters["username"], Parameters["password"]) as IIdentity
             : schm == AuthenticationSchemes.Digest
               ? new HttpDigestIdentity (Parameters)
               : null;
    }

    #endregion
  }
}
