/*===============================================================
* Product:		Com2Verse
* File Name:	GameObjectExtension.cs
* Developer:	tlghks1009
* Date:			2022-07-07 14:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Extension
{
	public static class GameObjectExtension
	{
		public static T GetOrAddComponent<T>( this GameObject thisGameObject ) where T : UnityEngine.Component
		{
			bool hasComponent = thisGameObject.TryGetComponent(out T component);
			if (!hasComponent) component = thisGameObject.AddComponent<T>();
			return component!;
		}

		public static async UniTask DestroyGameObjectAsync([CanBeNull] this Component target)
		{
			if (target.IsUnityNull()) return;
			await UniTaskHelper.InvokeOnMainThread(() => DestroyGameObject(target));
		}

		public static void DestroyGameObject([CanBeNull] this Component target)
		{
			if (target.IsUnityNull()) return;
			if (Application.isPlaying) Object.Destroy(target!.gameObject);
			else Object.DestroyImmediate(target!.gameObject);
		}
	}
}
