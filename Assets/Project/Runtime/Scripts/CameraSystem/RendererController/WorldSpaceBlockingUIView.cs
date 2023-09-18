/*===============================================================
 * Product:		Com2Verse
 * File Name:	WorldSpaceBlockingUI.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-12 14:35
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.UI;
using UnityEngine;

namespace Com2Verse.Project.CameraSystem
{
	/// <inheritdoc />
	[RequireComponent(typeof(UIView))]
	[AddComponentMenu("[CameraSystem]/[CameraSystem] World Space Blocking UI View")]
	public sealed class WorldSpaceBlockingUIView : WorldSpaceBlockingObject
	{
		private GUIView? _guiView;

		protected override void OnEnable()
		{
			// Ignore
		}

		protected override void OnDisable()
		{
			// Ignore
		}

		private void Awake()
		{
			_guiView = GetComponent<GUIView>()!;

			_guiView.OnOpenedEvent  += OnOpened;
			_guiView.OnClosingEvent += OnClosing;
		}

		private void OnDestroy()
		{
			if (_guiView == null) return;

			_guiView.OnOpenedEvent  -= OnOpened;
			_guiView.OnClosingEvent -= OnClosing;
		}

		private void OnOpened(GUIView view)
		{
			// UI 카메라로의 전환은 GUIView가 완전히 열린 후에 이루어져야 한다.
			RegisterBlockingObject();
		}

		private void OnClosing(GUIView view)
		{
			// 기본 카메라로의 전환은 GUIView가 닫히기 시작하자마자 이루어져야 한다.
			UnregisterBlockingObject();
		}

		private void RegisterBlockingObject()
		{
			CameraMediator.Instance.TryAddWorldSpaceBlockingObject(this);
		}

		private void UnregisterBlockingObject()
		{
			CameraMediator.Instance.RemoveWorldSpaceBlockingObject(this);
		}
	}
}
