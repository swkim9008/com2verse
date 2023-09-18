// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver_ServiceChange.cs
//  * Developer:	haminjeong
//  * Date:		2023-07-01 오후 6:06
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Protocols.CommonLogic;

namespace Com2Verse.Network.CommonLogic
{
	public partial class PacketReceiver
	{
		public event Action<ServiceChangeNotify> ServiceChangeNotify;

		public void RaiseServiceChangeNotify(ServiceChangeNotify response)
		{
			ServiceChangeNotify?.Invoke(response);
		}

		private void DisposeServiceChange()
		{
			ServiceChangeNotify = null;
		}
	}
}
