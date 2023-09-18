/*===============================================================
* Product:		Com2Verse
* File Name:	ActiveTagProcessor.cs
* Developer:	haminjeong
* Date:			2023-05-17 15:36
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.SmallTalk;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Protocols.Communication;

namespace Com2Verse.Network
{
	[TagObjectType(eObjectType.AVATAR)]
	public sealed class ActiveTagProcessor : BaseTagProcessor
	{
		[UsedImplicitly, SuppressMessage("ReSharper", "InconsistentNaming")]
		public class Data
		{
			public string serviceName { get; set; }
			public string roomId { get; set; }
			public string mediaId { get; set; }
			public string mediaUrl { get; set; }
			public Config config { get; set; }
			public StunServer stunServer { get; set; }
			public TurnServer turnServer { get; set; }
		}

		[UsedImplicitly, SuppressMessage("ReSharper", "InconsistentNaming")]
		public class Config
		{
			public int bandwidth { get; set; }
			public int upThreshold { get; set; }
			public int downThreshold { get; set; }
		}

		[UsedImplicitly, SuppressMessage("ReSharper", "InconsistentNaming")]
		public class StunServer
		{
			public string url { get; set; }
		}

		[UsedImplicitly, SuppressMessage("ReSharper", "InconsistentNaming")]
		public class TurnServer
		{
			public string Url { get; set; }
			public string Account { get; set; }
			public string Password { get; set; }
		}

		private static readonly List<long> PrevIDList = new();
		private static readonly List<long> IDList = new();

		public override void Initialize()
		{
			SetDelegates(MapObject.NickNameTag, (value, mapObject) =>
			{
				if (string.IsNullOrEmpty(mapObject.Name) || !string.IsNullOrEmpty(value))
					(mapObject as MapObject)?.SetName(value);
			});

			SetDelegates("SmallTalk", (value, mapObject) =>
			{
				if (string.IsNullOrEmpty(value) || mapObject.IsUnityNull() || mapObject!.IsMine || mapObject is not ActiveObject activeObject)
					return;

				var jsonObj = JsonConvert.DeserializeObject(value);
				var data    = JsonConvert.DeserializeObject<Data>(jsonObj?.ToString());

				try
				{
					var otherMediaChannel = new OtherMediaChannelNotify
					{
						ChannelId = data?.roomId,
						OwnerId   = activeObject.OwnerID,
						RtcChannelInfo = new()
						{
							MediaName   = data?.mediaId,
							ServerUrl   = data?.mediaUrl,
							Direction   = Direction.Incomming,
							AccessToken = User.Instance.CurrentUserData.AccessToken,

							IceServerConfigs =
							{
								new List<IceServerConfig>()
								{
									new IceServerConfig
									{
										IceServerUrl = data?.turnServer?.Url,
										AccountName = data?.turnServer?.Account,
										Credential = data?.turnServer?.Password
									}, 
									new IceServerConfig
									{
										IceServerUrl = data?.stunServer?.url
									}
								},
							},
						},
					};

					SmallTalkDistance.Instance.OnOtherMediaChannelNotify(otherMediaChannel);
				}
				catch (Exception e)
				{
					C2VDebug.LogError($"OtherMediaChannelNotify data is null : {e.Message}");
				}
			});

			SetDelegates(TagDefine.Key.ConferenceObjectType, (value, mapObject) =>
			{
				if (mapObject is ActiveObject activeObject) activeObject.RefreshConferenceObjectType();
			});
		}
	}
}
