#if ENABLE_CHEATING

/*===============================================================
* Product:		Com2Verse
* File Name:	CheatKey_Rendering.cs
* Developer:	ljk
* Date:			2023-01-02 09:58
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#define _ENABLE_MAP_DATA_RENDER

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cinemachine;
using Com2Verse.AssetSystem;
using Com2Verse.Avatar;
using Com2Verse.CameraSystem;
using Com2Verse.CustomizeLayer;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.Rendering;
using Com2Verse.Rendering.Instancing;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
#if ENABLE_MAP_DATA_RENDER
using Com2Verse.MapDataRender;
#endif
using Com2Verse.Rendering.Interactable;

namespace Com2Verse.Cheat
{
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static partial class CheatKey
	{

#region Rendering
		[MetaverseCheat("Cheat/Rendering/[ AVATAR ] Create Avatar Sample")] [HelpText("갯수","거리")]
		private static void CreateSampleAvatar(string count = "1",string distance = "0",string typestring = "PC01_M")
		{
			int iCount = 0;
			eAvatarType type = eAvatarType.PC01_M;
			float distanceFromMe = 0;
			if (!int.TryParse(count, out iCount))
			{
				return;
			}

			if (!Enum.TryParse(typestring, out type))
			{
				return;
			}

			float.TryParse(distance, out distanceFromMe);

			for (int _ = 0; _ < iCount; _++)
			{
				var info = AvatarInfo.GetTestInfo();

				float rad = Random.Range(0, 1f);
				float dist = (1 - Random.Range(0, 1f)*Random.Range(0, 1f) )*distanceFromMe;
				Vector3 offset = new Vector3( dist*Mathf.Cos(rad*Mathf.PI*2),0,dist*Mathf.Sin(rad*Mathf.PI*2) );

				CreatAvatarAsync(info, eAnimatorType.WORLD,Camera.main.transform.position + offset, (int)Define.eLayer.CHARACTER, typestring).Forget();
			}
		}

		private static async UniTask CreatAvatarAsync(AvatarInfo avatarInfo, eAnimatorType animatorType, Vector3 initialPosition, int layer, string typestring)
		{
			var controller = await AvatarCreator.CreateAvatarAsync(avatarInfo, animatorType, initialPosition, layer);

			controller.GetComponent<GhostAvatarController>().IsTestAvatar    = true;
			controller.GetComponent<GhostAvatarController>().ModelTypeString = avatarInfo.AvatarType.ToString();
			controller.gameObject.SetActive(true);
			controller.gameObject.name            = $"{typestring}_FAKE_AVATAR";
			controller.transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
		}


		[MetaverseCheat("Cheat/Rendering/[ AVATAR ] GhostAvatar Distance")] [HelpText("시작거리")]
		private static void GhostAvatar(string start = "0",string complete = "0")
		{
			if (float.TryParse(start, out float distance))
			{
				GPUInstancingManager.GhostAvatarStartDistance = (distance >= 0 ? distance : 0);
				GPUInstancingManager.GhostAvatarCompleteDistance = (distance >= 0 ? distance : 0);
			}
		}

		[MetaverseCheat("Cheat/Rendering/[ AVATAR ] GhostAvatar Fast Gen")] [HelpText("W / M")]
		private static void ImmediateGhostAvatar(string assetType = "W")
		{

			GPUInstancingManager.GhostAvatarStartDistance = 1;
			GPUInstancingManager.GhostAvatarCompleteDistance = 2;
			for (int _ = 0; _ < 50; _++)
			{
				var info = AvatarInfo.GetTestInfo(assetType.Equals("W") ? eAvatarType.PC01_W : eAvatarType.PC01_M);
				int idstart =assetType.Equals("W") ? 15000000 : 16000000;
				info.UpdateFashionItem(new FashionItemInfo(idstart+10000+Random.Range(1,5)));
				info.UpdateFashionItem(new FashionItemInfo(  idstart+20000+Random.Range(1,5)));
				info.UpdateFashionItem(new FashionItemInfo(idstart+30000+Random.Range(1,5)));
				
				float rad = Random.Range(0, 1f);
				float dist = (1 - Random.Range(0, 1f)*Random.Range(0, 1f) )*20;
				Vector3 offset = new Vector3( dist*Mathf.Cos(rad*Mathf.PI*2),0,dist*Mathf.Sin(rad*Mathf.PI*2) );
				CreatAvatarAsync(info, eAnimatorType.WORLD,Camera.main.transform.position + offset, (int)Define.eLayer.CHARACTER, "PC01_"+assetType).Forget();
			}
		}
		
		[MetaverseCheat("Cheat/Rendering/[ AVATAR ] GhostAvatar Toggle")] [HelpText("true/false")]
		private static void GhostAvatarEnable(string toggle = "false")
		{
			GhostAvatarController.EnableGhostFeature = toggle.Equals("true");
		}

		[MetaverseCheat("Cheat/Rendering/[ AVATAR ] EnableAvatarFashionnPooling Toggle")] [HelpText("true/false")]
		private static void EnableAvatarFashionnPooling(string toggle = "false")
		{
			AvatarCustomizeLayer.EnablePooling = toggle.Equals("true");
		}
		
		[MetaverseCheat("Cheat/Rendering/GraphicsSettings Enable")]
		private static void OpenGraphicsSettings()
		{
			Rendering.GraphicsSettingsManager.Instance.OpenWindow();
		}

		[MetaverseCheat("Cheat/Rendering/Enable GPU Instancing Debugger")] [HelpText("true / false")]
		private static void ToggleGPUInstancingDebugger(string toggle = "true")
		{
			if (toggle.Equals("true"))
			{
				if (GPUInstancingManager.InstanceExists)
					GPUInstancingManager.Instance.Debug(true);
			}
			else if (toggle.Equals("false"))
			{
				if (GPUInstancingManager.InstanceExists)
					GPUInstancingManager.Instance.Debug(false);
			}
		}

		[MetaverseCheat("Cheat/Rendering/[ AVATAR ] Character Dissolve Enable")] [HelpText("true / false")]
		public static void CharacterDissolve(string toggle = "true")
		{
			if (toggle.Equals("true"))
			{
				MaterialEffectManager.Instance.RequestGlobalMaterialEffect(
					"Avatar_Dissolve_In.prefab",
					() =>
					{
						// C2VDebug.Log("CharacterDissolve.EffectComplete");
						CameraManager.Instance.OffCullingMaskLayer((int)Define.eLayer.CHARACTER);
					},
					() => {  });
			}
			else if (toggle.Equals("false"))
			{
				MaterialEffectManager.Instance.RequestGlobalMaterialEffect(
					"Avatar_Dissolve_Out.prefab",
					() => {  },
					() =>
					{
						//C2VDebug.Log("CharacterDissolve.EffectStart");
						CameraManager.Instance.OnCullingMaskLayer((int)Define.eLayer.CHARACTER);
					});
			}
		}
		[MetaverseCheat("Cheat/Rendering/[ AVATAR ] Ghost Avatar Texture Size")] [HelpText("1~4")]
		public static void GhostAvatarTextureMode(string exp = "1")
		{
			if (int.TryParse(exp, out int expon))
			{
				GPUInstancingManager.Instance.GetSpacialColorHandler().ResetGhostTexture(expon);
			}
		}

		[MetaverseCheat("Cheat/Rendering/[ WORLD ] Enable Wide-World Rendering Feature")]
		public static void EnableWideWorldRenderer()
		{
			var mainCamera = CameraManager.Instance.MainCamera;
			if (mainCamera.IsUnityNull())
				return;

			var cameraObject = mainCamera!.gameObject;

			cameraObject.AddComponent<Com2Verse.Rendering.World.WorldRenderPositionHandler>();
		}


		private static CinemachineVirtualCamera _storedVirtualCamera;
		[MetaverseCheat("Cheat/Rendering/[ TOOL ] Enable Free Camera")][HelpText("true / false")]
		public static void EnableFreeCamera(string toggle = "true")
		{
			var mainCamera = CameraManager.Instance.MainCamera;
			if (mainCamera.IsUnityNull())
				return;

			var cameraObject = mainCamera!.gameObject;

			if (toggle.Equals("true"))
			{
				FreeCamera fc = cameraObject.GetComponent<FreeCamera>();
				if (fc != null)
					return;

				cameraObject.AddComponent<FreeCamera>();
				cameraObject.GetComponent<CinemachineBrain>().enabled = false;
				_storedVirtualCamera = cameraObject.GetComponent<CinemachineBrain>().ActiveVirtualCamera
					.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
				_storedVirtualCamera.enabled = false;
			}
			else
			{
				FreeCamera fc = cameraObject.GetComponent<FreeCamera>();
				if (fc != null)
					Object.Destroy(fc);
				cameraObject.GetComponent<CinemachineBrain>().enabled = true;
				if (_storedVirtualCamera != null)
					_storedVirtualCamera.enabled = true;
			}
		}
		
		[MetaverseCheat("Cheat/Rendering/Enable Blob Checker")]
		private static void MakeSmalltalkTester()
		{
			CreateBlobTesterAsync().Forget();
		}
		
		private static async UniTask CreateBlobTesterAsync()
		{
			var controller = await C2VAddressables.LoadAssetAsync<GameObject>("UI_SmallTalk_BlobConnector.prefab").ToUniTask();
			GameObject blobTestBehavior = new GameObject("BlobTestDummyBehavior");
			blobTestBehavior.transform.position = User.Instance.CharacterObject.transform.position;
			//controller.transform.position = Vector3.zero;
			GameObject icm = GameObject.Instantiate(controller);
			
			BlobConnectionCheckMode cm = blobTestBehavior.AddComponent<BlobConnectionCheckMode>();
			cm.targetBlob = icm.GetComponent<BlobConnection>();
			cm.enabled = false;
			cm.enabled = true;
		}
#if ENABLE_MAP_DATA_RENDER
		[MetaverseCheat("Cheat/Rendering/Enable Map Data Viewer")] [HelpText("true / false")]
        private static void ToggleMapDataViewer(string toggle = "true")
        {
            if (toggle.Equals("true"))
            {
                GameObject drawerObject = GameObject.Instantiate(Resources.Load<GameObject>("MapDataRenderer"));
                drawerObject.transform.position = Vector3.zero;
            }
            else if (toggle.Equals("false"))
            {
                MapDataRenderer dataRenderer = GameObject.FindObjectOfType<MapDataRender.MapDataRenderer>();
                if(dataRenderer != null)
                    GameObject.Destroy(dataRenderer.gameObject);
            }
        }
#endif
#endregion Rendering

	}
}
#endif
