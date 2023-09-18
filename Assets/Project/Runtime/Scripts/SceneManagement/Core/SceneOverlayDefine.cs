/*===============================================================
 * Product:		Com2Verse
 * File Name:	SceneOverlayDefine.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-02 14:28
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com2Verse
{
	[Serializable]
	public sealed record SceneOverlayDefine : ISerializationCallbackReceiver
	{
		[field: SerializeField]
		public eEnumMatchType ServiceFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(ServiceFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(ServiceFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eServiceType ServiceType { get; private set; }

		[field: SerializeField]
		public eEnumMatchType SpaceTypeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(SpaceTypeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(SpaceTypeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eSpaceType SpaceType { get; private set; }

		[field: SerializeField]
		public eEnumMatchType SpaceCodeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(SpaceCodeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(SpaceCodeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eSpaceCode SpaceCode { get; private set; }

		[field: SerializeField]
		public ePrimitiveMatchType SceneBuildingIdFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(SceneBuildingIdFilter), ePrimitiveMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(SceneBuildingIdFilter), ePrimitiveMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public int BuildingId { get; private set; }

		[field: SerializeField]
		public eEnumMatchType CommunicationTypeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(CommunicationTypeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(CommunicationTypeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eSpaceOptionCommunication CommunicationType { get; private set; }

		[field: SerializeField]
		public eEnumMatchType ChattingUITypeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(ChattingUITypeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(ChattingUITypeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eSpaceOptionChattingUI ChattingUIType { get; private set; }

		[field: SerializeField]
		public eEnumMatchType ToolbarUITypeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(ToolbarUITypeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(ToolbarUITypeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eSpaceOptionToolbarUI ToolbarUIType { get; private set; }

		[field: SerializeField]
		public eEnumMatchType MapTypeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(MapTypeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(MapTypeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eSpaceOptionMap MapType { get; private set; }

		[field: SerializeField]
		public eEnumMatchType GhostAvatarTypeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(GhostAvatarTypeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(GhostAvatarTypeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eSpaceOptionGhostAvatar GhostAvatarType { get; private set; }

		[field: SerializeField]
		public eEnumMatchType ViewportTypeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(ViewportTypeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(ViewportTypeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eSpaceOptionViewport ViewportType { get; private set; }

		[field: SerializeField]
		public eEnumMatchType MotionTrackingTypeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(MotionTrackingTypeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(MotionTrackingTypeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public eSpaceOptionMotionTracking MotionTrackingType { get; private set; }

		[field: SerializeField]
		public ePrimitiveMatchType DebugTypeFilter { get; private set; }
		[field: SerializeField]
		[field: DrawIf(nameof(DebugTypeFilter), eEnumMatchType.IGNORE, DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		[field: DrawIf(nameof(DebugTypeFilter), eEnumMatchType.ANY,    DrawIfAttribute.eDisablingType.DONT_DRAW, true)]
		public bool DebugType { get; private set; }

#if UNITY_EDITOR
		[Header("Overlay")]
		[SerializeField]
		private List<AssetReference> _overlays = new();
#endif // UNITY_EDITOR

		[field: SerializeField, NonReorderable, ReadOnly] public List<string> OverlayAddressableNames { get; private set; } = new();

		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			OverlayAddressableNames.Clear();
			foreach (var overlay in _overlays)
			{
				var editorAsset = overlay.editorAsset;
				if (editorAsset == null)
					continue;

				var addressableName = editorAsset.name;
				OverlayAddressableNames.Add(addressableName);
			}
#endif // UNITY_EDITOR
		}

		public void OnAfterDeserialize() { }

		public bool IsPropertyMatch(SceneProperty? sceneProperty)
		{
			if (!IsServiceTypeMatch(sceneProperty?.ServiceType))
				return false;

			if (!IsSpaceTypeMatch(sceneProperty?.SpaceTemplate?.SpaceType))
				return false;

			if (!IsSpaceCodeMatch(sceneProperty?.SpaceTemplate?.SpaceCode))
				return false;

			if (!IsBuildingIdMatch(sceneProperty?.SpaceTemplate?.BuildingID))
				return false;

			if (!IsCommunicationTypeMatch(sceneProperty?.CommunicationType))
				return false;

			if (!IsChattingUITypeMatch(sceneProperty?.ChattingUIType))
				return false;

			if (!IsToolbarUITypeMatch(sceneProperty?.ToolbarUIType))
				return false;

			if (!IsMapTypeMatch(sceneProperty?.MapType))
				return false;

			if (!IsGhostAvatarTypeMatch(sceneProperty?.GhostAvatarType))
				return false;

			if (!IsViewportTypeMatch(sceneProperty?.ViewportType))
				return false;

			if (!IsMotionTrackingTypeMatch(sceneProperty?.MotionTrackingType))
				return false;

			if (!IsDebugTypeMatch(sceneProperty?.IsDebug))
				return false;

			return true;
		}

		public bool IsServiceTypeMatch(eServiceType? serviceType)
		{
			if (ServiceFilter is eEnumMatchType.IGNORE)
				return true;

			return serviceType?.IsFilterMatch(ServiceType, ServiceFilter) ?? false;
		}

		public bool IsSpaceTypeMatch(eSpaceType? spaceType)
		{
			if (SpaceTypeFilter is eEnumMatchType.IGNORE)
				return true;

			return spaceType?.IsFilterMatch(SpaceType, SpaceTypeFilter) ?? false;
		}

		public bool IsSpaceCodeMatch(eSpaceCode? spaceCode)
		{
			if (SpaceCodeFilter is eEnumMatchType.IGNORE)
				return true;

			return spaceCode?.IsFilterMatch(SpaceCode, SpaceCodeFilter) ?? false;
		}

		// TODO : 현재 공간의 건물 식별하는 방법이 추가되면 코드 제거
		private bool IsBuildingIdMatch(int? buildingId)
		{
			if (SceneBuildingIdFilter is ePrimitiveMatchType.IGNORE)
				return true;

			if (!buildingId.HasValue)
				return true;

			return SceneBuildingIdFilter switch
			{
				ePrimitiveMatchType.ANY        => true,
				ePrimitiveMatchType.EQUALS     => buildingId.Value == BuildingId,
				ePrimitiveMatchType.NOT_EQUALS => buildingId.Value != BuildingId,
				_                              => false,
			};
		}

		public bool IsCommunicationTypeMatch(eSpaceOptionCommunication? communicationType)
		{
			if (CommunicationTypeFilter is eEnumMatchType.IGNORE)
				return true;

			return communicationType?.IsFilterMatch(CommunicationType, CommunicationTypeFilter) ?? false;
		}

		public bool IsChattingUITypeMatch(eSpaceOptionChattingUI? chattingUIType)
		{
			if (ChattingUITypeFilter is eEnumMatchType.IGNORE)
				return true;

			return chattingUIType?.IsFilterMatch(ChattingUIType, ChattingUITypeFilter) ?? false;
		}

		public bool IsToolbarUITypeMatch(eSpaceOptionToolbarUI? toolbarUIType)
		{
			if (ToolbarUITypeFilter is eEnumMatchType.IGNORE)
				return true;

			return toolbarUIType?.IsFilterMatch(ToolbarUIType, ToolbarUITypeFilter) ?? false;
		}

		public bool IsMapTypeMatch(eSpaceOptionMap? mapType)
		{
			if (MapTypeFilter is eEnumMatchType.IGNORE)
				return true;

			return mapType?.IsFilterMatch(MapType, MapTypeFilter) ?? false;
		}

		public bool IsGhostAvatarTypeMatch(eSpaceOptionGhostAvatar? ghostAvatarType)
		{
			if (GhostAvatarTypeFilter is eEnumMatchType.IGNORE)
				return true;

			return ghostAvatarType?.IsFilterMatch(GhostAvatarType, GhostAvatarTypeFilter) ?? false;
		}

		public bool IsViewportTypeMatch(eSpaceOptionViewport? viewportType)
		{
			if (ViewportTypeFilter is eEnumMatchType.IGNORE)
				return true;

			return viewportType?.IsFilterMatch(ViewportType, ViewportTypeFilter) ?? false;
		}

		public bool IsMotionTrackingTypeMatch(eSpaceOptionMotionTracking? motionTrackingType)
		{
			if (MotionTrackingTypeFilter is eEnumMatchType.IGNORE)
				return true;

			return motionTrackingType?.IsFilterMatch(MotionTrackingType, MotionTrackingTypeFilter) ?? false;
		}

		public bool IsDebugTypeMatch(bool? debugType)
		{
			if (DebugTypeFilter is ePrimitiveMatchType.IGNORE)
				return true;

			if (!debugType.HasValue)
				return true;

			return DebugTypeFilter switch
			{
				ePrimitiveMatchType.ANY        => true,
				ePrimitiveMatchType.EQUALS     => debugType.Value == DebugType,
				ePrimitiveMatchType.NOT_EQUALS => debugType.Value != DebugType,
				_                              => false,
			};
		}
	}
}
