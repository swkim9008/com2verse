/*===============================================================
* Product:    Com2Verse
* File Name:  StubActionMapWASD.cs
* Developer:  eugene9721
* Date:       2022-05-03 15:15
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

// #define SONG_TEST

using System;
using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using UnityEngine;
using UnityEngine.InputSystem;

#if SONG_TEST
using Com2Verse.AssetSystem; 
#endif // SONG_TEST

namespace Com2Verse.Project.InputSystem
{
	public sealed class ActionMapCharacterControl : ActionMap
	{
		private bool _isDetectedObjectClick;
		private bool _isDetectedObjectRightClick;
		private bool _isDesiredClickMoveButton;
		private bool _isOnUI;

#region Constructor
		public ActionMapCharacterControl()
		{
			_name = "Player";

			// Move - Type: Value, Vector2
			_actionList.Add(OnMoveStarted);
			_actionList.Add(OnMovePerformed);
			_actionList.Add(OnMoveCanceled);

			// Jump - Type: Button
			_actionList.Add(OnJumpStarted);
			_actionList.Add(OnJumpPerformed);
			_actionList.Add(OnJumpCanceled);

			// Sprint - Type: Button
			_actionList.Add(OnSprintStarted);
			_actionList.Add(OnSprintPerformed);
			_actionList.Add(OnSprintCanceled);

			// Sprint - Type: Button
			_actionList.Add(OnInteractionStarted);
			_actionList.Add(OnInteractionPerformed);
			_actionList.Add(OnInteractionCanceled);

			// Drag - Type: Value, Vector2
			_actionList.Add(OnDragStarted);
			_actionList.Add(OnDragPerformed);
			_actionList.Add(OnDragCanceled);

			// Zoom - Type: Value, Axis
			_actionList.Add(OnZoomStarted);
			_actionList.Add(OnZoomPerformed);
			_actionList.Add(OnZoomCanceled);

			// Chat - Type: Button
			_actionList.Add(OnChatUIStarted);
			_actionList.Add(OnChatUIPerformed);
			_actionList.Add(OnChatUICanceled);

			// Emotion Shortcut - Type: Pass Through
			_actionList.Add(OnShortcut1Performed);
			_actionList.Add(OnShortcut2Performed);
			_actionList.Add(OnShortcut3Performed);
			_actionList.Add(OnShortcut4Performed);
			_actionList.Add(OnShortcut5Performed);
			_actionList.Add(OnShortcut6Performed);
			_actionList.Add(OnShortcut7Performed);
			_actionList.Add(OnShortcut8Performed);
			_actionList.Add(OnShortcut9Performed);
			_actionList.Add(OnShortcut0Performed);

			// Emotion - Type: Button
			_actionList.Add(OnEmotionUIStarted);
			_actionList.Add(OnEmotionUIPerformed);
			_actionList.Add(OnEmotionUICanceled);

			// Control - Type: Button
			_actionList.Add(OnControlStarted);
			_actionList.Add(OnControlPerformed);
			_actionList.Add(OnControlCanceled);

			// Minimap - Type: Button
			_actionList.Add(OnMinimapUIStarted);
			_actionList.Add(OnMinimapUIPerformed);
			_actionList.Add(OnMinimapUICanceled);
			
			// Escape - Type: Button
			_actionList.Add(OnEscapeUIStarted);
			_actionList.Add(OnEscapeUIPerformed);
			_actionList.Add(OnEscapeUICanceled);

			_targetAction.Add("Up");
			_targetAction.Add("Down");
			_targetAction.Add("Left");
			_targetAction.Add("Right");
			_targetAction.Add("Fire");
			_targetAction.Add("Reload");

			MouseLeftButtonPressed  += OnMouseLeftPressed;
			MouseLeftButtonDrag     += OnMouseLeftDrag;
			MouseLeftButtonClick    += OnMouseLeftClick;
			MouseLeftButtonRelease  += OnMouseLeftReleased;
			MouseRightButtonPressed += OnMouseRightPressed;
			MouseRightButtonDrag    += OnMouseRightDrag;
			MouseRightButtonClick   += OnMouseRightClick;
			MouseRightButtonRelease += OnMouseRightReleased;
		}
#endregion Constructor

#region Override
		public override void UpdateOnSyncTime()
		{
			if (_isDesiredClickMoveButton)
			{
				_isDesiredClickMoveButton = false;
				ClickPositionDetect();
			}

			_isOnUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
		}

		public override void ClearActions()
		{
			CharacterMoveAction      = null;
			JumpAction               = null;
			SprintAction             = null;
			ClickWorldPositionAction = null;
			ClickObjectAction        = null;
			RightClickObjectAction   = null;
			ClickMoveStartAction     = null;
			ClickMoveEndAction       = null;
			LookStartAction          = null;
			LookEndAction            = null;
			MoveAction               = null;
			LookAction               = null;
			ZoomAction               = null;
			ChatAction               = null;
			Shortcut0Action          = null;
			Shortcut1Action          = null;
			Shortcut2Action          = null;
			Shortcut3Action          = null;
			Shortcut4Action          = null;
			Shortcut5Action          = null;
			Shortcut6Action          = null;
			Shortcut7Action          = null;
			Shortcut8Action          = null;
			Shortcut9Action          = null;
			Shortcut0Action          = null;
			EmotionAction            = null;
			ControlAction            = null;
			MinimapAction            = null;
			EscapeAction             = null;
		}
#endregion Override

#region InputActions
		public Action           CharacterMoveAction      { get; set; }
		public Action           JumpAction               { get; set; }
		public Action<bool>     SprintAction             { get; set; }
		public Action<Vector3>  ClickWorldPositionAction { get; set; }
		public Action<Collider> ClickObjectAction        { get; set; }
		public Action<Collider> RightClickObjectAction   { get; set; }

		public Action          ClickMoveStartAction { get; set; }
		public Action          ClickMoveEndAction   { get; set; }
		public Action          LookStartAction      { get; set; }
		public Action          LookEndAction        { get; set; }
		public Action<Vector2> MoveAction           { get; set; }
		public Action<Vector2> LookAction           { get; set; }
		public Action<float>   ZoomAction           { get; set; }
		public Action          ChatAction           { get; set; }
		public Action          Shortcut1Action      { get; set; }
		public Action          Shortcut2Action      { get; set; }
		public Action          Shortcut3Action      { get; set; }
		public Action          Shortcut4Action      { get; set; }
		public Action          Shortcut5Action      { get; set; }
		public Action          Shortcut6Action      { get; set; }
		public Action          Shortcut7Action      { get; set; }
		public Action          Shortcut8Action      { get; set; }
		public Action          Shortcut9Action      { get; set; }
		public Action          Shortcut0Action      { get; set; }
		public Action          EmotionAction        { get; set; }
		public Action<bool>    ControlAction        { get; set; }
		public Action          MinimapAction        { get; set; }
		public Action          EscapeAction         { get; set; }
#endregion InputActions

#region MouseEvent
		private void OnMouseLeftPressed()
		{
			ClickMoveStartAction?.Invoke();
			_isDesiredClickMoveButton = true;
		}

		private void OnMouseLeftClick()
		{
			_isDetectedObjectClick = DetectClickObject();
		}

		private void OnMouseLeftDrag()
		{
			ClickMoveStartAction?.Invoke();
			_isDesiredClickMoveButton = true;
		}

		private void OnMouseLeftReleased()
		{
			ClickMoveEndAction?.Invoke();
			_isDesiredClickMoveButton = false;
			_isDetectedObjectClick    = false;
		}

		private void OnMouseRightPressed()
		{
			LookStartAction?.Invoke();
		}

		private void OnMouseRightClick()
		{
			_isDetectedObjectRightClick = DetectClickObject(isLeft: false);
		}

		private void OnMouseRightDrag() { }

		private void OnMouseRightReleased()
		{
			_isDetectedObjectRightClick = false;
			LookEndAction?.Invoke();
		}

		private void ClickPositionDetect()
		{
			Ray ray = CameraManager.Instance.GetRayFromCamera();
			if (ray.Equals(default)) return;

			var hits = Physics.RaycastAll(ray);
			if (hits is { Length: > 0 })
			{
				var hitCount = hits.Length;
				Array.Sort(hits!, (x, y) => x.distance.CompareTo(y.distance));
				for (int i = 0; i < hitCount; ++i)
				{
					var collider = hits[i].collider;
					if (collider.IsUnityNull()) continue;
					if (collider!.isTrigger)
					{
						C2VDebug.LogCategory("RayCast", "Click Hit collider is trigger");
						continue;
					}
					if (InputSystemManager.Instance.GroundMask != 1 << collider.gameObject.layer)
					{
						C2VDebug.LogCategory("RayCast", $"Click Hit Layer {1 << collider.gameObject.layer}, but ground layer {InputSystemManager.Instance.GroundMask}");
						return;
					}
					ClickWorldPositionAction?.Invoke(hits[i].point);
					return;
				}
			}
		}

		private bool DetectClickObject(bool isLeft = true)
		{
			if (!CameraManager.InstanceExists) return false;

			Ray ray = CameraManager.Instance.GetRayFromCamera();
			if (ray.Equals(default)) return false;

			if (Physics.Raycast(ray, out RaycastHit hitObject, 1000.0f, InputSystemManager.Instance.ObjectMask))
			{
				var collider = hitObject.collider;
				if (isLeft) ClickObjectAction?.Invoke(collider);
				else RightClickObjectAction?.Invoke(collider);
				return true;
			}

			return false;
		}
#endregion

#region Move
		private void OnMoveStarted(InputAction.CallbackContext callback) { }

		private void OnMovePerformed(InputAction.CallbackContext callback)
		{
			// if (!Loading.LoadingManager.Instance.LoadingFinished)
			// 	return;

			MoveAction?.Invoke(callback.action.ReadValue<Vector2>());
			CharacterMoveAction?.Invoke();
		}

		private void OnMoveCanceled(InputAction.CallbackContext callback)
		{
			MoveAction?.Invoke(Vector2.zero);
		}
#endregion Move

#region JUMP
		private void OnJumpStarted(InputAction.CallbackContext callback)
		{
			JumpAction?.Invoke();
			CharacterMoveAction?.Invoke();
		}

		private void OnJumpPerformed(InputAction.CallbackContext callback) { }

		private void OnJumpCanceled(InputAction.CallbackContext callback) { }
#endregion JUMP

#region SPRINT
		private void OnSprintStarted(InputAction.CallbackContext callback)
		{
			SprintAction?.Invoke(true);
		}

		private void OnSprintPerformed(InputAction.CallbackContext callback) { }

		private void OnSprintCanceled(InputAction.CallbackContext callback)
		{
			SprintAction?.Invoke(false);
		}
#endregion SPRINT

#region INTERACTION
		private void OnInteractionStarted(InputAction.CallbackContext callback) { }

		private void OnInteractionPerformed(InputAction.CallbackContext callback) { }

		private void OnInteractionCanceled(InputAction.CallbackContext callback) { }
#endregion INTERACTION

#region ZOOM
		private void OnZoomStarted(InputAction.CallbackContext callback) { }

		private void OnZoomPerformed(InputAction.CallbackContext callback)
		{
			if (_isOnUI) return;
			ZoomAction?.Invoke(callback.action.ReadValue<float>());
		}

		private void OnZoomCanceled(InputAction.CallbackContext callback) { }
#endregion ZOOM

#region DRAG
		private void OnDragStarted(InputAction.CallbackContext callback) { }

		private void OnDragPerformed(InputAction.CallbackContext callback)
		{
			LookAction?.Invoke(callback.action.ReadValue<Vector2>());
		}

		private void OnDragCanceled(InputAction.CallbackContext callback)
		{
			LookAction?.Invoke(Vector2.zero);
		}
#endregion DRAG

#region CHATTING
		private void OnChatUIStarted(InputAction.CallbackContext callback)
		{
			ChatAction?.Invoke();
		}

		private void OnChatUIPerformed(InputAction.CallbackContext callback) { }

		private void OnChatUICanceled(InputAction.CallbackContext callback) { }
#endregion CHATTING

#region EMOTION_SHORTCUT
		private void OnShortcut1Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut1Action?.Invoke();
		}

		private void OnShortcut2Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut2Action?.Invoke();
		}

		private void OnShortcut3Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut3Action?.Invoke();
		}

		private void OnShortcut4Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut4Action?.Invoke();
		}

		private void OnShortcut5Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut5Action?.Invoke();
		}

		private void OnShortcut6Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut6Action?.Invoke();
		}

		private void OnShortcut7Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut7Action?.Invoke();
		}

		private void OnShortcut8Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut8Action?.Invoke();
		}

		private void OnShortcut9Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut9Action?.Invoke();
		}

		private void OnShortcut0Performed(InputAction.CallbackContext callback)
		{
			if (callback.ReadValueAsButton())
				Shortcut0Action?.Invoke();
		}
#endregion EMOTION_SHORTCUT

#region EMOTION
		private void OnEmotionUIStarted(InputAction.CallbackContext callback)
		{
			if (_canClick && InputSystemManager.CanClick())
			{
				StartTimerWhenKeyboardPress();
				EmotionAction?.Invoke();
			}
		}

		private void OnEmotionUIPerformed(InputAction.CallbackContext callback) { }

		private void OnEmotionUICanceled(InputAction.CallbackContext callback) { }
#endregion EMOTION

#region CONTROL
		private void OnControlStarted(InputAction.CallbackContext callback)
		{
			ControlAction?.Invoke(true);
		}

		private void OnControlPerformed(InputAction.CallbackContext callback) { }

		private void OnControlCanceled(InputAction.CallbackContext callback)
		{
			ControlAction?.Invoke(false);
		}
#endregion CONTROL

#region MINIMAP
		private void OnMinimapUIStarted(InputAction.CallbackContext callback)
		{
			if (_canClick && InputSystemManager.CanClick())
			{
				StartTimerWhenKeyboardPress();
				MinimapAction?.Invoke();
			}
		}

		private void OnMinimapUIPerformed(InputAction.CallbackContext callback) { }

		private void OnMinimapUICanceled(InputAction.CallbackContext callback) { }
#endregion MINIMAP

#region MINIMAP
		private void OnEscapeUIStarted(InputAction.CallbackContext callback)
		{
			if (_canClick && InputSystemManager.CanClick())
			{
				StartTimerWhenKeyboardPress();
				EscapeAction?.Invoke();
			}
		}

		private void OnEscapeUIPerformed(InputAction.CallbackContext callback) { }

		private void OnEscapeUICanceled(InputAction.CallbackContext callback) { }
#endregion MINIMAP

		private bool _canClick = true;

		private void StartTimerWhenKeyboardPress()
		{
			if (InputSystemManager.BlockInterval > 0)
			{
				_canClick = false;
				InputSystemManager.RequestKeyboardBlock();
				UIManager.Instance.StartTimer(InputSystemManager.BlockInterval,
				                              () =>
				                              {
					                              _canClick = true;
					                              InputSystemManager.RequestKeyboardUnblock();
				                              });
			}
		}
	}
}
