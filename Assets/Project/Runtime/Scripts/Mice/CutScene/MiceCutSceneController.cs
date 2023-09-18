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
using Cinemachine;
using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Com2Verse.Mice
{
	public sealed partial class MiceCutSceneController : MonoBehaviour
	{
		public enum eType
		{
			INTRO,
			TUTORIAL,
		}

		public struct Parameter
		{
			public struct SlotBinding
			{
				public MiceCutSceneSlot.eType Type;
				public GameObject             Target;
			}

			public SlotBinding[] SlotBindings;
		}

		public static readonly string CameraTrackName = "Cinemachine Track";

		[SerializeField] private eType              _type;
		[SerializeField] private MiceCutSceneSlot[] _slots;


		private PlayableDirector _playableDirector = default;

		public eType                                Type             => _type;
		public PlayableDirector                     PlayableDirector => _playableDirector;
		public event Action<MiceCutSceneController> OnStopped;

		private void Awake()
		{
			_playableDirector = GetComponent<PlayableDirector>();
			_playableDirector.stopped += (_) =>
			{
				RemoveSlotBindings();
				RemoveGameObjectBinding(CameraTrackName);
				gameObject.SetActive(false);
				OnStopped?.Invoke(this);
			};
		}

		public void Play(Parameter parameter)
		{
#region Camera Binding
			SetGameObjectBinding(CameraTrackName, CameraManager.Instance.MainCamera!.gameObject);
#endregion
			foreach (var binding in parameter.SlotBindings)
			{	
				SetSlotBinding(binding.Type, binding.Target);
			}
			_playableDirector.Play();
		}

		public void Stop()
		{
			_playableDirector.Stop();
		}

		private void Update()
		{
			if (!IsPlaying()) return;

			foreach (var slot in _slots)
			{
				slot.SyncTransform();
			}
		}

		public void SetSlotBinding(MiceCutSceneSlot.eType type, GameObject target)
		{
			if (_slots == null) return;

			var slot = Array.Find(_slots, (slot) => slot.Type == type);
			if (slot == null) return;

			slot.Bind(this, target);
		}

		public void RemoveSlotBindings()
		{
			if (_slots == null) return;
			foreach (var slot in _slots)
			{
				slot.Unbind(this);
			}
		}

		public bool IsPlaying()
		{
			return _playableDirector.state == PlayState.Playing;
		}

		public void SetGameObjectBinding(string trackName, GameObject targetObject)
		{
			_playableDirector.SetGameObjectBinding(trackName, targetObject);
		}

		public void RemoveGameObjectBinding(string trackName)
		{
			_playableDirector.RemoveBinding(trackName);
		}
	}

#if UNITY_EDITOR
	public sealed partial class MiceCutSceneController
	{
		public void SetActivePreview(bool active)
		{
			foreach (var slot in _slots)
			{
				if (active) slot.CreatePreview(this);
				else slot.RemovePreview(this);
			}
		}

		public void OnValidate()
		{
			_playableDirector = GetComponent<PlayableDirector>();
		}
	}
	
	[CustomEditor(typeof(MiceCutSceneController))]
	public class MiceCutSceneControllerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.BeginHorizontal("Box");
			if (GUILayout.Button("프리뷰 보기"))
			{
				var controller = target as MiceCutSceneController;
				controller.SetActivePreview(true);
			}

			if (GUILayout.Button("프리뷰 제거"))
			{
				var controller = target as MiceCutSceneController;
				controller.SetActivePreview(false);
			}

			EditorGUILayout.EndHorizontal();
		}
	}
#endif
}


