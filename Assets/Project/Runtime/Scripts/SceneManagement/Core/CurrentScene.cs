/*===============================================================
 * Product:		Com2Verse
 * File Name:	CurrentScene.cs
 * Developer:	urun4m0r1
 * Date:		2023-07-12 21:43
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Data;

namespace Com2Verse
{
	public static class CurrentScene
	{
		public static SceneBase Scene => SceneManager.InstanceOrNull?.CurrentScene ?? SceneEmpty.Empty;

		public static bool IsWorldScene => Scene.IsWorldScene;

		public static SceneProperty SceneProperty => Scene.SceneProperty;

		public static string        SceneName   => Scene.SceneName;
		public static eServiceType? ServiceType => Scene.ServiceType;

		public static SpaceTemplate? SpaceTemplate => Scene.SpaceTemplate;
		public static eSpaceType?    SpaceType     => Scene.SpaceType;
		public static eSpaceCode?    SpaceCode     => Scene.SpaceCode;

		public static eSpaceOptionCommunication  CommunicationType  => Scene.CommunicationType;
		public static eSpaceOptionChattingUI     ChattingUIType     => Scene.ChattingUIType;
		public static eSpaceOptionToolbarUI      ToolbarUIType      => Scene.ToolbarUIType;
		public static eSpaceOptionMap            MapType            => Scene.MapType;
		public static eSpaceOptionGhostAvatar    GhostAvatarType    => Scene.GhostAvatarType;
		public static eSpaceOptionViewport       ViewportType       => Scene.ViewportType;
		public static eSpaceOptionMotionTracking MotionTrackingType => Scene.MotionTrackingType;

		public static bool UseVoiceModule  => Scene.UseVoiceModule;
		public static bool UseCameraModule => Scene.UseCameraModule;
		public static bool UseScreenModule => Scene.UseScreenModule;

		public static int SessionTimeout => Scene.SessionTimeout;
	}
}
