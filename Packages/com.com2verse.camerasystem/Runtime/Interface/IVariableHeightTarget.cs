/*===============================================================
* Product:		Com2Verse
* File Name:	IVariableLengthTarget.cs
* Developer:	eugene9721
* Date:			2023-01-06 12:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.CameraSystem
{
	/// <summary>
	/// 카메라의 타겟(Follow, Look At)이 오브젝트의 피봇이 아닌 특정 위치를 보게 하고싶은 경우 적용되는 인터페이스
	/// (예: 캐릭터의 머리위치를 보게 하고싶은 경우)
	/// </summary>
	public interface IVariableHeightTarget
	{
		float Height { get; set; }
	}
}
