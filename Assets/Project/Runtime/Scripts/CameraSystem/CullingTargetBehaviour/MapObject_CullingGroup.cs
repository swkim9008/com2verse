/*===============================================================
* Product:		Com2Verse
* File Name:	MapObject_CullingGroup.cs
* Developer:	eugene9721
* Date:			2022-12-22 16:24
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.CameraSystem;
using UnityEngine;

namespace Com2Verse.Network
{
	public enum eHudCullingState
	{
		SPEECH_BUBBLE_DISPLAY_MAX,
		SPEECH_BUBBLE_DISPLAY_MEDIUM,
		ENABLE_HUD_DISPLAY,
		DISABLE_HUD_DISPLAY,
	}

	public partial class MapObject : ICullingTarget
	{
		public event Action<MapObject> BecomeOccluded;
		public event Action<MapObject> BecomeUnoccluded;

		public event Action<MapObject, eHudCullingState> OnChangeHudCullingState;

		public bool IsOccluded => HudCullingState == eHudCullingState.DISABLE_HUD_DISPLAY;

		public eHudCullingState HudCullingState
		{
			get => _hudCullingState;
			private set
			{
				if (_hudCullingState != value)
					OnChangeHudCullingState?.Invoke(this, value);
				_hudCullingState = value;
			}
		}

		[SerializeField]
		private eCullingUpdateMode _boundingSphereUpdateMode = eCullingUpdateMode.DYNAMIC;

		[SerializeField]
		private float _cullingBoundingradius = 0.5f;

		private BoundingSphere _boundingSphere;

		private eHudCullingState _hudCullingState = eHudCullingState.DISABLE_HUD_DISPLAY;

		public eCullingUpdateMode BoundingSphereUpdateMode => _boundingSphereUpdateMode;
		public BoundingSphere     BoundingSphere           => _boundingSphere;

		public BoundingSphere UpdateAndGetBoundingSphere()
		{
			_boundingSphere.position = transform.position + Vector3.up * ObjectHeight;
			_boundingSphere.radius   = _cullingBoundingradius;
			return _boundingSphere;
		}

		public CullingGroupProxy HudCullingGroup { get; set; }

		public CullingGroup.StateChanged OnHudStateChanged { get; private set; }

		private void OnHudCullingGroupStateChanged(CullingGroupEvent cullingGroupEvent)
		{
			var prevState = HudCullingState;
			SetHudCullingState(cullingGroupEvent);

			if (prevState == HudCullingState)
				return;

			OnChangePrevState(prevState);
			OnChangeState(HudCullingState);
		}

		private void OnChangePrevState(eHudCullingState prevState)
		{
			switch (prevState)
			{
				case eHudCullingState.SPEECH_BUBBLE_DISPLAY_MAX:
					break;
				case eHudCullingState.SPEECH_BUBBLE_DISPLAY_MEDIUM:
					break;
				case eHudCullingState.ENABLE_HUD_DISPLAY:
					break;
				case eHudCullingState.DISABLE_HUD_DISPLAY:
					BecomeUnoccluded?.Invoke(this);
					break;
			}
		}

		private void OnChangeState(eHudCullingState state)
		{
			switch (state)
			{
				case eHudCullingState.SPEECH_BUBBLE_DISPLAY_MAX:
					break;
				case eHudCullingState.SPEECH_BUBBLE_DISPLAY_MEDIUM:
					break;
				case eHudCullingState.ENABLE_HUD_DISPLAY:
					break;
				case eHudCullingState.DISABLE_HUD_DISPLAY:
					BecomeOccluded?.Invoke(this);
					break;
			}
		}

		private void SetHudCullingState(CullingGroupEvent cullingGroupEvent)
		{
			if (!cullingGroupEvent.isVisible)
			{
				HudCullingState = eHudCullingState.DISABLE_HUD_DISPLAY;
				return;
			}

			switch (cullingGroupEvent.currentDistance)
			{
				case 0:
					HudCullingState = eHudCullingState.SPEECH_BUBBLE_DISPLAY_MAX;
					break;
				case 1:
					HudCullingState = eHudCullingState.SPEECH_BUBBLE_DISPLAY_MEDIUM;
					break;
				case 2:
					HudCullingState = eHudCullingState.ENABLE_HUD_DISPLAY;
					break;
				case 3:
					HudCullingState = eHudCullingState.DISABLE_HUD_DISPLAY;
					break;
			}
		}
	}
}
