/*===============================================================
* Product:		Com2Verse
* File Name:	FakeSystemVolume.cs
* Developer:	urun4m0r1
* Date:			2022-08-05 17:50
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication.Unity
{
	/// <summary>
	/// Unity 기본 API는 운영체제 오디오 장치 볼륨 변경을 지원하지 않습니다.
	/// </summary>
	public sealed class NullSystemVolume : SystemVolume
	{
		public NullSystemVolume() : base(1f) { }

		protected override float GetSystemLevel() => 0f;

		protected override void SetSystemLevel(float value) { }

		protected override bool GetSystemAudible() => false;

		protected override void SetSystemAudible(bool value) { }
	}
}
