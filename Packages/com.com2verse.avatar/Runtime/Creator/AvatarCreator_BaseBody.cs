/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarCreator_BaseBody.cs
* Developer:	tlghks1009
* Date:			2022-08-10 18:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Avatar
{
	public static partial class AvatarCreator
	{
		/// <summary>
		/// 아바타 한 셋트 생성 클래스<br/>
		/// 1. 베이스 바디 로드<br/>
		/// 2. 패션 아이템 순차적으로 로드, 블랜드 쉐입 셋팅
		/// </summary>
		public static class AvatarBaseBodyCreator
		{
			/// <summary>
			/// 베이스 바디 먼저 로드
			/// </summary>
			// public static async UniTask<AvatarUpdateData?> OnLoadBaseBodyAsync(string addressableKey, Vector3 initialPosition, Quaternion rot)
			// {
			// 	var avatarController = await OnLoadBaseOriginBodyAsync(addressableKey, initialPosition, rot);
			// 	if (avatarController == null) return null;
			//
			// 	var updatePartsData = new AvatarUpdateData(avatarController, data.AnimatorType, data.AvatarInfo, data.Layer);
			// 	await SetupAvatarItems(avatarController);
			//
			// 	return updatePartsData;
			// }
			public static GameObject? OnLoadBaseOriginBodyAsync(string addressableKey, Vector3 initialPosition, Quaternion rot)
			{
				var loadedAsset = RuntimeObjectManager.Instance.LoadAsset<GameObject>(addressableKey);
				if (loadedAsset.IsReferenceNull())
				{
					C2VDebug.LogError(nameof(AvatarCreator), $"Unable to load avatar. {addressableKey}");
					return null;
				}

				return RuntimeObjectManager.Instance.Instantiate<GameObject>(loadedAsset!, initialPosition, rot);
			}

			public static async UniTask SetupAvatarItems(AvatarController avatarController, CancellationTokenSource cancellationTokenSource)
			{
				avatarController.OnStartLoadAvatarParts();
				await CreateFashionItemAsync(avatarController, cancellationTokenSource);

				var faceItemList = avatarController.Info?.GetFaceOptionList();
				await SetFaceItems(faceItemList, avatarController, cancellationTokenSource);

				avatarController.SetBodyShapeIndex(avatarController.Info?.BodyShape ?? 0);
				avatarController.OnCompleteLoadAvatarParts();
				// C2VDebug.LogCategory("[AvatarLoading]", $"Complete - SetupAvatarItems {avatarController.Info?.AvatarId ?? 0}");
			}

			private static async UniTask SetFaceItems(List<FaceItemInfo>? data, AvatarController avatarController, CancellationTokenSource? cancellationTokenSource = null)
			{
				if (data == null || data.Count == 0)
					return;

				foreach (var faceItem in data)
				{
					if (faceItem == null) continue;

					await avatarController.SetFaceOption(faceItem, cancellationTokenSource);
				}
			}

			/// <summary>
			/// 파츠를 순차적으로 로드
			/// </summary>
			private static async UniTask CreateFashionItemAsync(AvatarController avatarController, CancellationTokenSource cancellationTokenSource)
			{
				var fashionItemList = avatarController.Info?.GetFashionItemList();
				if (fashionItemList == null || fashionItemList.Count == 0)
					return;

				foreach (var fashionItem in fashionItemList)
				{
					var fashionItemTable = AvatarTable.GetFashionItem(fashionItem.ItemId);
					if (fashionItemTable == null)
						continue;

					await avatarController.SetFashionMenu(fashionItem, cancellationTokenSource);
					// if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
					// 	await UniTask.Delay(TimeSpan.FromSeconds(1.0f), DelayType.DeltaTime, PlayerLoopTiming.Update, cancellationToken: cancellationTokenSource.Token);
				}
			}
		}
	}
}