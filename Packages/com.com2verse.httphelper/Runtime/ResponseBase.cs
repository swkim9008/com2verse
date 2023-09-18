/*===============================================================
* Product:		Com2Verse
* File Name:	ResponseBase.cs
* Developer:	jhkim
* Date:			2023-05-16 12:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Com2Verse.HttpHelper
{
	public class ResponseBase<T>
	{
		public HttpStatusCode StatusCode;
		public HttpResponseMessage Response;
		public readonly T Value;

		protected ResponseBase() { }

		public ResponseBase(HttpResponseMessage response)
		{
			if (response == null) return;

			StatusCode = response.StatusCode;
			Response = response;
		}

		public ResponseBase(T value, HttpResponseMessage response) : this(response) => Value = value;
	}

	public class ResponseStream : ResponseBase<Stream>, IAsyncDisposable
	{
		public ResponseStream(HttpResponseMessage response, Stream stream) : base(stream, response) { }

		public async ValueTask DisposeAsync()
		{
			if (Value != null)
				await Value.DisposeAsync();
		}
	}
}
