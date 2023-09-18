/*===============================================================
 * Product:		Com2Verse
 * File Name:	SceneProperty.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-31 21:40
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Linq;
using Com2Verse.Data;
using Com2Verse.SceneManagement;

namespace Com2Verse
{
	public sealed class SceneProperty
	{
		public static readonly SceneProperty Empty = new();

		public string?        AddressableName { get; init; }
		public SpaceTemplate? SpaceTemplate   { get; init; }
		public eServiceType?  ServiceType     { get; init; }

		public eSpaceOptionCommunication  CommunicationType  { get; init; }
		public eSpaceOptionChattingUI     ChattingUIType     { get; init; }
		public eSpaceOptionToolbarUI      ToolbarUIType      { get; init; }
		public eSpaceOptionMap            MapType            { get; init; }
		public eSpaceOptionGhostAvatar    GhostAvatarType    { get; init; }
		public eSpaceOptionViewport       ViewportType       { get; init; }
		public eSpaceOptionMotionTracking MotionTrackingType { get; init; }

		public bool IsDebug { get; init; }

		public int SessionTimeout { get; init; } = -1;

		public string? GetSceneName()
		{
			if (!string.IsNullOrWhiteSpace(AddressableName!))
				return AddressableName;

			if (!string.IsNullOrWhiteSpace(SpaceTemplate?.SpaceTemplateName!))
				return SpaceTemplate.SpaceTemplateName;

			return null;
		}

		public static SceneProperty? CreateWorldSceneProperty()
		{
			var spaceOptions = SceneManager.Instance.SpaceOptionSettings?.Datas;
			var worldOption  = spaceOptions?.FirstOrDefault(x => x.ServiceType is eServiceType.WORLD);
			if (worldOption == null)
				return null;

			return new SceneProperty
			{
				AddressableName    = Define.SceneWorldName,
				ServiceType        = eServiceType.WORLD,
				CommunicationType  = worldOption.Communication,
				ChattingUIType     = worldOption.ChattingUI,
				ToolbarUIType      = worldOption.ToolbarUI,
				MapType            = worldOption.Map,
				GhostAvatarType    = worldOption.GhostAvatar,
				ViewportType       = worldOption.Viewport,
				MotionTrackingType = default,
				SessionTimeout     = worldOption.SessionTimeOut,
			};
		}

		public static SceneProperty? Convert(SpaceTemplate? templateData)
		{
			var serviceType = GetServiceType(templateData?.SpaceType);
			if (serviceType == null)
				return null;

			var spaceOptions = SceneManager.Instance.SpaceOptionSettings?.Datas;
			var spaceOption  = spaceOptions?.FirstOrDefault(x => x.ServiceType == serviceType.Value && x.SpaceCode == templateData?.SpaceCode);
			if (spaceOption == null)
				return null;

			return new SceneProperty
			{
				SpaceTemplate      = templateData,
				ServiceType        = serviceType,
				CommunicationType  = spaceOption.Communication,
				ChattingUIType     = spaceOption.ChattingUI,
				ToolbarUIType      = spaceOption.ToolbarUI,
				MapType            = spaceOption.Map,
				GhostAvatarType    = spaceOption.GhostAvatar,
				ViewportType       = spaceOption.Viewport,
				MotionTrackingType = default,
				SessionTimeout     = spaceOption.SessionTimeOut,
			};

			static eServiceType? GetServiceType(eSpaceType? templateDataSpaceType) => templateDataSpaceType switch
			{
				eSpaceType.OFFICE => eServiceType.OFFICE,
				eSpaceType.MICE   => eServiceType.MICE,
				_                 => null,
			};
		}
	}
}
