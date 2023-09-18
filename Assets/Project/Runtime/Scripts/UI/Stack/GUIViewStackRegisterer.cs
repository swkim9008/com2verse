/*===============================================================
* Product:		Com2Verse
* File Name:	GUIViewStackRegisterer.cs
* Developer:	mikeyid77
* Date:			2023-06-01 11:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.UI
{
	[AddComponentMenu("[UI Stack]/[UI Stack] GUI View Stack Registerer")]
	public sealed class GUIViewStackRegisterer : StackRegisterer
	{
		private GUIView _guiView = null;

		private void Awake()
		{
			_guiView = gameObject.GetComponent<GUIView>();
			if (_guiView.IsUnityNull())
			{
				C2VDebug.LogErrorCategory("UIStackManager", $"${nameof(gameObject.name)} : Need GUI View");
			}
			else
			{
				var targetState = (NeedCharacterMove) ? eInputSystemState.CHARACTER_CONTROL : eInputSystemState.UI;
				_guiView.OnOpeningEvent += (view) => AddToManager(_guiView.ViewName, targetState);
				_guiView.OnClosingEvent += (view) => RemoveFromManager();
				_guiView.OnOpenedEvent  += (view) => FinishViewEvent();
				_guiView.OnClosedEvent  += (view) => FinishViewEvent();
			}
		}

		private void OnDestroy()
		{
			RemoveFromManager();
			_guiView = null;
		}

		public override void HideComplete()
		{
			if (!_guiView.IsUnityNull())
				_guiView.Hide();
		}
	}
}
