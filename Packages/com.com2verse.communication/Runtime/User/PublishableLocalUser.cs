/*===============================================================
* Product:		Com2Verse
* File Name:	PublishableLocalUser.cs
* Developer:	urun4m0r1
* Date:			2022-09-02 16:48
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication
{
	/// <inheritdoc cref="IPublishableLocalUser"/>
	/// <summary>
	/// <see cref="IPublishableLocalUser"/>의 기본 구현체입니다.
	/// <br/>
	/// <br/>해당 클래스는 다음과 같은 하위 클래스를 가집니다.
	/// <list type="bullet">
	/// <item><see cref="MediaSdk.ChannelLocalUser"/></item>
	/// <item><see cref="MediaSdk.PeerLocalUser"/></item>
	/// </list>
	/// </summary>
	public abstract class PublishableLocalUser : CommunicationUser, IPublishableLocalUser
	{
		public AudioPublishSettings? GetAudioPublishSettings(eTrackType trackType) => trackType switch
		{
			eTrackType.VOICE => Unity.ModuleManager.Instance.VoicePublishSettings,
			_                => null,
		};

		public VideoPublishSettings? GetVideoPublishSettings(eTrackType trackType) => trackType switch
		{
			eTrackType.CAMERA => Unity.ModuleManager.Instance.CameraPublishSettings,
			eTrackType.SCREEN => Unity.ModuleManager.Instance.ScreenPublishSettings,
			_                 => null,
		};

		public MediaModules Modules { get; }

		/// <summary>
		/// <paramref name="channelInfo"/>의 <see cref="ChannelInfo.LoginUser"/>와 <see cref="ChannelInfo.UserRole"/>을 사용하여 <see cref="PublishableLocalUser"/>를 생성합니다.
		/// </summary>
		protected PublishableLocalUser(ChannelInfo channelInfo) : base(channelInfo, channelInfo.LoginUser, channelInfo.UserRole)
		{
			Modules = new(GetAudio, GetVideo);
		}

		private Audio? GetAudio(eTrackType trackType) => trackType switch
		{
			eTrackType.VOICE => Unity.ModuleManager.InstanceOrNull?.Voice,
			_                => null,
		};

		private Video? GetVideo(eTrackType trackType) => trackType switch
		{
			eTrackType.CAMERA => Unity.ModuleManager.InstanceOrNull?.Camera,
			eTrackType.SCREEN => Unity.ModuleManager.InstanceOrNull?.Screen,
			_                 => null,
		};

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				Modules.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
