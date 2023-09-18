/*===============================================================
* Product:		Com2Verse
* File Name:	UnityHttpClient.cs
* Developer:	jhkim
* Date:			2023-02-01 21:14
* History:		
* Documents:	
 ================================================================*/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace Com2Verse.HttpHelper
{
    // https://ticehurst.com/2021/08/27/unity-httpclient.html
    internal class UnityHttpClient
    {
#region Static
        public static int TimeOut = 300;
        private const int handlerExpirySeconds = 120; // DNS 갱신 120초
        private static readonly object factoryLock = new object();

        private static readonly Dictionary<string, UnityHttpClient> factories =
            new()
            {
                {string.Empty, new UnityHttpClient()},
            };

        public static HttpClient Get() => factories[string.Empty].GetNewHttpClient();

        // Each unique HttpClientHandler gets a new Connection limit per origin, so create
        // a new "named" client factory to get a new handler (used by each HttpClient from
        // that factory), and thus new set of connections.
        //
        // For example, if you have a few long-running requests, you might choose to put
        // them on their own handler/connections so you don't block other faster requests
        // to the same host.
        public static HttpClient Get(string name)
        {
            UnityHttpClient factory;
            lock (factoryLock)
            {
                if (!factories.TryGetValue(name, out factory))
                {
                    factory = new UnityHttpClient();
                    factories.Add(name, factory);
                }
            }

            return factory.GetNewHttpClient();
        }

        private UnityHttpClient() { }

        public static void TerminateAll()
        {
            foreach (var http in factories.Values)
            {
                // http._cancellation.Cancel();
                http._handlerTimer.Stop();
                http._currentHandler.Dispose();
            }

            factories.Clear();
        }

        public static void ResetAll()
        {
            TerminateAll();
            factories[string.Empty] = new UnityHttpClient();
        }
#endregion // Static

#region Non-Static
        private HttpClientHandler _currentHandler = new();
        private readonly Stopwatch _handlerTimer = Stopwatch.StartNew();
        private readonly object _handlerLock = new();
        private HttpClient _client;

        private HttpClient GetNewHttpClient()
        {
            if (_client != null)
                return _client;

            _client = new HttpClient(GetHandler(), disposeHandler: false);
            _client.DefaultRequestHeaders.UserAgent.TryParseAdd($"{Application.productName}/{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");
            // _client.DefaultRequestHeaders.TransferEncodingChunked = false;
            _client.Timeout = TimeSpan.FromSeconds(TimeOut);
            return _client;
        }

        private HttpClientHandler GetHandler()
        {
            lock (_handlerLock)
            {
                if (_handlerTimer.Elapsed.TotalSeconds > handlerExpirySeconds)
                {
                    // Leave the old HttpClientHandler for the GC. DON'T Dispose() it!
                    _currentHandler = new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        AutomaticDecompression = DecompressionMethods.None,
                        Proxy = null,
                        UseProxy = false,
                        MaxAutomaticRedirections = 50,
                    };
                    _handlerTimer.Restart();
                }

                return _currentHandler;
            }
        }
#endregion // Non-Static
    }
}
