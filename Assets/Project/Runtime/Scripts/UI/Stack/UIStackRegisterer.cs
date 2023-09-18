/*===============================================================
* Product:		Com2Verse
* File Name:	UIStackRegisterer.cs
* Developer:	mikeyid77
* Date:			2023-06-02 10:56
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using UnityEngine;

namespace Com2Verse.UI
{
	[AddComponentMenu("[UI Stack]/[UI Stack] UI Stack Registerer")]
	public sealed class UIStackRegisterer : StackRegisterer
	{
		[SerializeField] private bool _needBlocker = true;
		private UIBlocker _blocker;

		private void OnEnable()
		{
			if (_needBlocker && _blocker == null)
				_blocker = UIBlocker.CreateBlocker(gameObject.transform, OnQuit);

			var targetState = (NeedCharacterMove) ? eInputSystemState.CHARACTER_CONTROL : eInputSystemState.UI;
			AddToManager(gameObject.name, targetState);
		}

		private void OnDisable()
		{
			FinishViewEvent();
			OnQuit();
		}

		public override void HideComplete()
		{
			if (_needBlocker)
			{
				_blocker?.DestroyBlocker();
				_blocker = null;
			}

			RemoveFromManager();
			gameObject.SetActive(false);
		}
	}
}
