/*===============================================================
* Product:		Com2Verse
* File Name:	HttpRequestBuilder.cs
* Developer:	jhkim
* Date:			2023-02-06 10:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Com2Verse.Logger;

namespace Com2Verse.HttpHelper
{
	public class HttpRequestBuilder : IDisposable
	{
#region Variables
		private HttpRequestMessage _request;
		public static readonly string Content_MD5 = "Content-MD5";
#endregion // Variables

#region Properties
		public HttpRequestMessage Request => _request;
#endregion // Properties

#region Constructor & Initialization
		public static HttpRequestBuilder CreateNew(Client.eRequestType requestType, string url)
		{
			var builder = new HttpRequestBuilder(requestType, url);
			return builder;
		}

		public HttpRequestBuilder NewRequest(Client.eRequestType requestType, string url)
		{
			var httpMethod = GetHttpMethod(requestType);
			if (httpMethod == null)
			{
				C2VDebug.LogError($"invalid request type = {requestType}");
				return null;
			}

			_request?.Dispose();
			_request = new HttpRequestMessage(httpMethod, url);
			return this;
		}

		private HttpRequestBuilder(Client.eRequestType requestType, string url)
		{
			NewRequest(requestType, url);
		}
#endregion // Constructor & Initialization

#region HttpRequestMessage - Content
		public HttpRequestBuilder SetContent(HttpContent httpContent)
		{
			_request.Content = httpContent;
			return this;
		}
		public HttpRequestBuilder SetContent(string message)
		{
			_request.Content = new StringContent(message, Encoding.UTF8, "text/plain");
			return this;
		}
		public HttpRequestBuilder SetContent(string message, string mediaType)
		{
			_request.Content = new StringContent(message, Encoding.UTF8, mediaType);
			return this;
		}
		public HttpRequestBuilder SetContent(byte[] bytes, int offset = 0, int count = -1)
		{
			_request.Content = offset == 0 && count == -1 ? new ByteArrayContent(bytes) : new ByteArrayContent(bytes, offset, count);
			return this;
		}
		public HttpRequestBuilder SetContent(Stream stream, int bufferSize = 0)
		{
			var streamContent = bufferSize > 0 ? new StreamContent(stream, bufferSize) : new StreamContent(stream);
			_request.Content = streamContent;
			return this;
		}

		public HttpRequestBuilder SetContentType(string mediaType)
		{
			if (_request.Content == null) return this;

			_request.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
			return this;
		}

		public HttpRequestBuilder SetContentLength(long length)
		{
			if (length < 0 || _request.Content == null) return this;

			_request.Content.Headers.ContentLength = length;
			return this;
		}
#endregion // HttpRequestMessage - Content

#region HttpRequestMessage - Headers
		public HttpRequestBuilder AddHeader(string key, string value)
		{
			RemoveHeader(key);
			_request.Headers.Add(key, value);
			return this;
		}
		public HttpRequestBuilder AddHeader(string key, IEnumerable<string> values)
		{
			RemoveHeader(key);
			_request.Headers.Add(key, values);
			return this;
		}
		public HttpRequestBuilder RemoveHeader(string key)
		{
			if (_request.Headers.Contains(key))
				_request.Headers.Remove(key);
			return this;
		}
#endregion // HttpRequestMessage - Headers

#region HttpRequestMessage - Property
		public HttpRequestBuilder AddProperty(string key, string value)
		{
			RemoveProperty(key);
			_request.Properties.Add(key, value);
			return this;
		}

		public HttpRequestBuilder RemoveProperty(string key)
		{
			if (_request.Properties.ContainsKey(key))
				_request.Properties.Remove(key);
			return this;
		}

		public void ClearAllProperty()
		{
			foreach (var key in _request.Properties.Keys.ToArray())
				RemoveProperty(key);
		}
#endregion // HttpRequestMessage - Property

#region MultiPart
		public HttpRequestBuilder SetMultipartFormContent(params MultipartFormInfo[] subContents)
		{
			var multipartContent = new MultipartFormDataContent();
			foreach (var info in subContents)
				multipartContent.Add(info.Content, info.Name, info.FileName);

			_request.Content = multipartContent;
			return this;
		}

		public HttpRequestBuilder SetMD5ContentHeader(byte[] bytes)
		{
			_request.Content.Headers.ContentMD5 = bytes;
			return this;
		}
#endregion // MultiPart

#region All-In-One
		public static HttpRequestMessage Generate(HttpRequestMessageInfo info)
		{
			using var builder = HttpRequestBuilder.CreateNew(info.RequestMethod, info.Url);
			if (info.Content != null)
				builder.SetContent(info.Content);
			if (info.Headers != null)
			{
				foreach (var (key, value) in info.Headers)
					builder.AddHeader(key, value);
			}

			if (info.Properties != null)
			{
				foreach (var (key, value) in info.Properties)
					builder.AddProperty(key, value);
			}

			if (!string.IsNullOrWhiteSpace(info.ContentType))
				builder.SetContentType(info.ContentType);

			if (info.ContentLength > 0)
				builder.SetContentLength(info.ContentLength);
			return builder.Request;
		}
#endregion // All-In-One

#region Debug
		public void PrintInfo()
		{
			foreach (var (key, values) in _request.Headers)
			foreach (var value in values)
				C2VDebug.Log($"Header [{key}] = {value}");

			foreach (var (key, values) in _request.Content.Headers)
			foreach (var value in values)
				C2VDebug.Log($"Content Header [{key}] = {value}");
		}
#endregion // Debug

#region Private Methods
		private static HttpMethod GetHttpMethod(Client.eRequestType requestType) => requestType switch
		{
			Client.eRequestType.GET    => HttpMethod.Get,
			Client.eRequestType.POST   => HttpMethod.Post,
			Client.eRequestType.PUT    => HttpMethod.Put,
			Client.eRequestType.DELETE => HttpMethod.Delete,
			_                          => null,
		};
#endregion // Private Methods

#region Dispose
		public void Dispose()
		{
			_request?.Dispose();
		}
#endregion // Dispose
	}

	public struct HttpRequestMessageInfo
	{
		public Client.eRequestType RequestMethod;
		public string Url;
		public HttpContent Content;
		public (string, string)[] Headers;
		public (string, string)[] Properties;
		public string ContentType;
		public long ContentLength;
	}

	public struct MultipartFormInfo
	{
		public HttpContent Content;
		public string Name;
		public string FileName;
	}
}
