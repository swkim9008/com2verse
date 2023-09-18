/*===============================================================
* Product:		Com2Verse
* File Name:	TweenBasePropertyExtensions.cs
* Developer:	eugene9721
* Date:			2023-04-13 14:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;
using Com2Verse.Extension;
using Com2Verse.Logger;
using JetBrains.Annotations;

namespace Com2Verse.Tweener
{
	[AddComponentMenu("[DB]/[DB] TweenBasePropertyExtensions")]
	public sealed class TweenBasePropertyExtensions : MonoBehaviour
	{
		private TweenBase? _tweenBase;

		[UsedImplicitly]
		public Transform? ChangeTweenController
		{
			get => _tweenBase.IsUnityNull() ? null : _tweenBase!.transform;
			set
			{
				if (_tweenBase.IsUnityNull() || value.IsUnityNull()) return;

				var tweenController = value!.GetComponent<TweenController>();
				if (tweenController.IsReferenceNull()) return;

				_tweenBase!.ChangeController(tweenController!);
			}
		}

		private void Awake()
		{
			_tweenBase = GetComponent<TweenBase>();

			if (_tweenBase.IsUnityNull())
				C2VDebug.LogErrorCategory(nameof(TweenBasePropertyExtensions), "TweenBase is null");
		}
	}
}
