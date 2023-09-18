/*===============================================================
* Product:		Com2Verse
* File Name:	GameObjectEventExtensions.cs
* Developer:	jhkim
* Date:			2022-12-11 15:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	// GameObject 상태변환 이벤트 처리 (임시클래스)
	// GameObjectPropertyExtensions 내에서 이벤트 추가로 인한 예외상황이 없을 시 GameObjectPropertyExtensions로 이동
	[AddComponentMenu("[DB]/[DB] GameObjectPropertyEventExtensions")]
	public sealed class GameObjectPropertyEventExtensions : GameObjectPropertyExtensions
	{
		[HideInInspector] public UnityEvent<bool> _onChangeActive;

		public bool ActiveStateWithEvent
		{
			get => gameObject.activeSelf;
			set
			{
				gameObject.SetActive(value);
				_onChangeActive?.Invoke(value);
			}
		}
	}
}
