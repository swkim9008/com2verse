/*===============================================================
* Product:		Com2Verse
* File Name:	MiceCutSceneController.cs
* Developer:	wlemon
* Date:			2023-07-14 16:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Extension;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Com2Verse.Mice
{
	public sealed class MiceCutSceneManager : MonoBehaviour
	{
		public static MiceCutSceneManager Instance { get; private set; } = default;

		[SerializeField]
		private Canvas _controlCanvas = default;

		[SerializeField]
		private Button _buttonSkip;

		private Dictionary<MiceCutSceneController.eType, MiceCutSceneController> _cutSceneControllers       = new();
		private MiceCutSceneController                                           _playingCutSceneController = default;

		public bool IsSkipped { get; private set; } = false;

		private void Awake()
		{
			Instance = this;

			_buttonSkip.onClick.AddListener(() =>
			{
				IsSkipped = true;
				Stop();
			});
			var controllers = this.GetComponentsInChildren<MiceCutSceneController>(true);
			foreach (var controller in controllers)
			{
				controller.gameObject.SetActive(false);
				controller.OnStopped += OnStopped;
				_cutSceneControllers.Add(controller.Type, controller);
			}

			_controlCanvas.gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public bool Play(MiceCutSceneController.eType type, MiceCutSceneController.Parameter parameter)
		{
			if (!_cutSceneControllers.TryGetValue(type, out var result)) return false;

			_controlCanvas.gameObject.SetActive(true);
			_playingCutSceneController = result;
			_playingCutSceneController.gameObject.SetActive(true);
			_playingCutSceneController.Play(parameter);
			IsSkipped = false;

			return true;
		}

		public void Stop()
		{
			if (_playingCutSceneController.IsUnityNull()) return;

			_playingCutSceneController.Stop();
		}

		public bool IsPlaying()
		{
			if (_playingCutSceneController.IsUnityNull()) return false;

			return _playingCutSceneController.IsPlaying();
		}

		private void OnStopped(MiceCutSceneController cutSceneController)
		{
			if (_playingCutSceneController.IsUnityNull()) return;
			if (_playingCutSceneController != cutSceneController) return;

			_controlCanvas.gameObject.SetActive(false);
			_playingCutSceneController = null;
		}
	}
}
