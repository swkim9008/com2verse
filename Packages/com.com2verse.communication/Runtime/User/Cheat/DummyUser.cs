#if ENABLE_CHEATING

/*===============================================================
* Product:		Com2Verse
* File Name:	DummyUser.cs
* Developer:	urun4m0r1
* Date:			2022-08-03 15:53
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Com2Verse.Communication.Cheat
{
	/// <inheritdoc cref="ICommunicationUser"/>
	/// <summary>
	/// 테스트용 원격 유저 Mock 클래스입니다.
	/// <br/>일정 시간마다 볼륨 정보나 텍스쳐가 무작위로 변경됩니다.
	/// <br/>
	/// <br/><see cref="MediaSdk"/> 하위의 트랙 통신 기능과는 호환되지 않습니다.
	/// </summary>
	public class DummyUser : CommunicationUser, IRemoteUser, IViewModelUser
	{
		public static readonly List<string> DummyNames = new()
		{
			"김직원",
			"이사원",
			"박사장",
			"최대리",
			"정과장",
			"황차장",
			"남궁회장",
			"선우인턴",
			"Foo Bar",
			"Bar Foo",
			"John Smith",
			"Anthony Smith",
			"Cho Smith",
		};

		public MediaModules Modules { get; }

		private readonly Video? _camera;

		private readonly RandomTextureProvider? _randomTextureProvider;

		/// <summary>
		/// 랜덤한 이름과 UID를 가진 유저를 생성합니다.
		/// </summary>
		public static DummyUser CreateInstance(ChannelInfo channelInfo, eUserRole role = eUserRole.UNDEFINED)
		{
			var randomUser = CreateRandomUser();
			return new DummyUser(channelInfo, randomUser, role);
		}

		private static User CreateRandomUser()
		{
			var randomUid  = Random.Range(1, int.MaxValue);
			var randomName = DummyNames[Random.Range(0, DummyNames.Count)]!;
			return new User(randomUid, $"{randomName} (D)");
		}

		private DummyUser(ChannelInfo channelInfo, User user, eUserRole role) : base(channelInfo, user, role)
		{
			_randomTextureProvider = new RandomTextureProvider
			{
				IsRunning = true,
			};

			_camera = new Video(_randomTextureProvider);

			Modules = new(null, GetVideo);
		}

		private Video? GetVideo(eTrackType trackType) => trackType switch
		{
			eTrackType.CAMERA => _camera,
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
				_randomTextureProvider?.Dispose();

				_camera?.Dispose();

				Modules.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}

#endif // ENABLE_CHEATING
