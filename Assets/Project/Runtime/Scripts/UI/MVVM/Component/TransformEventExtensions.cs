/*===============================================================
* Product:		Com2Verse
* File Name:	TransformEventExtensions.cs
* Developer:	jhkim
* Date:			2022-10-14 17:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] TransformEventExtensions")]
	public sealed class TransformEventExtensions : MonoBehaviour
	{
		[HideInInspector] public UnityEvent<Transform> _onStart;
		[HideInInspector] public UnityEvent<Transform> _onEnable;
		[HideInInspector] public UnityEvent<Transform> _onDisable;

		private void Start() => _onStart?.Invoke(transform);
		private void OnEnable() => _onEnable?.Invoke(transform);
		private void OnDisable() => _onDisable?.Invoke(transform);
	}
}
