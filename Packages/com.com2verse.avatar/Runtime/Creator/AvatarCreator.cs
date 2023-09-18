/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarCreator.cs
* Developer:	tlghks1009
* Date:			2022-08-03 18:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Threading;
using Com2Verse.AvatarAnimation;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Avatar
{
	public static partial class AvatarCreator
	{
		/// <summary>
		/// 아바타 생성한다
		/// </summary>
		/// <param name="avatarInfo"></param>
		/// <param name="animatorType"></param>
		/// <param name="initialPosition"></param>
		/// <param name="layer"></param>
		/// <param name="immediately">즉시 생성할지 아니면 로딩큐에서 아바타 분산해서 로드할지</param>
		/// <param name="onCompleted">모든 파츠, 애니까지 완료되면 호출</param>
		/// <returns></returns>
		public static async UniTask<AvatarController?> CreateAvatarAsync(AvatarInfo         avatarInfo, eAnimatorType animatorType, Vector3 initialPosition, int layer, bool immediately = true,
		                                                                 Action<GameObject> onCompleted = null)
		{
			AvatarController? avatarController = CreateBaseAvatarAsync(avatarInfo, animatorType, initialPosition, layer);
			if (avatarController == null) return null;

			await UpdateAvatarParts(avatarController, immediately, onCompleted);
			return avatarController;
		}

		private static AvatarController? CreateBaseAvatarAsync(AvatarInfo avatarInfo, eAnimatorType animatorType, Vector3 initialPosition, int layer)
		{
			string addressableKey = $"{avatarInfo.AvatarTypePath}_BASE.prefab";
			GameObject? baseBodyInstance = AvatarBaseBodyCreator.OnLoadBaseOriginBodyAsync(addressableKey, initialPosition, Quaternion.identity);
			if (baseBodyInstance == null) return null;

			var baseBody = baseBodyInstance.transform.Find(MetaverseAvatarDefine.BaseBodyObjectName);
			if (baseBody.IsUnityNull())
			{
				C2VDebug.LogError(nameof(AvatarCreator), $"Unable to find base body. {avatarInfo.AvatarTypePath}");
				return null;
			}

			AvatarController avatarController = baseBody.gameObject.GetOrAddComponent<AvatarController>();
			avatarController.Create(avatarInfo, animatorType, layer);

			return avatarController;
		}

		public static async UniTask UpdateAvatarParts(AvatarController avatarController, bool immediately = false, Action<GameObject> onCompleted = null)
		{
			if (immediately)
			{
				await UpdateAvatar(avatarController, onCompleted);
			}
			else
			{
				int distance = (int)Vector3.SqrMagnitude(avatarController.transform.parent.position - AvatarCreatorQueue.Instance.MyAvatarPosition);
				AvatarCreatorQueue.Instance.Enqueue(new AvatarLoadInfo(avatarController, distance, UpdateAvatar, onCompleted));
			}
		}

		public static async UniTask UpdateAvatar(AvatarController avatarController, Action<GameObject> onCompleted = null, CancellationTokenSource cancellationTokenSource = null)
		{
			if (avatarController == null || avatarController.Info == null)
			{
				C2VDebug.LogErrorCategory(nameof(AvatarCreator), "avatarController is null or AvatarInfo is null");
				return;
			}

			//애니메이션 로드
			await LoadRuntimeAnimatorController(avatarController, cancellationTokenSource);
			// C2VDebug.LogCategory("[AvatarLoading]", $"Complete - LoadRuntimeAnimatorController {avatarController.Info.AvatarId}");

			// 파츠 로드
			await AvatarBaseBodyCreator.SetupAvatarItems(avatarController, cancellationTokenSource);
			// C2VDebug.LogCategory("[AvatarLoading]", $"Complete - AvatarBaseBodyCreator.SetupAvatarItems {avatarController.Info.AvatarId}");

			onCompleted?.Invoke(avatarController.gameObject);
		}
		public static async UniTask<CreatedSmrObjectItem?> CreateSmrItemAsync(AvatarItemInfo itemInfo, CancellationTokenSource cancellationTokenSource = null)
		{
			return await SmrObjectCreator.CreateAsync(itemInfo, cancellationTokenSource);
		}

		private static async UniTask LoadRuntimeAnimatorController(AvatarController avatarController, CancellationTokenSource cancellationTokenSource = null)
		{
			try
			{
				var info = avatarController.Info;
				var animatorFullIdentifier = AnimationManager.Instance.GetFullAnimatorIdentifier(info.AvatarType, avatarController.AnimatorType);
				var animatorController = avatarController.AvatarAnimatorController;
				if (avatarController.IsUnityNull())
				{
					C2VDebug.LogErrorCategory(nameof(AvatarCreator), "AvatarController is null");
					return;
				}

				var animator = await animatorController!.LoadAnimatorAsync(animatorFullIdentifier, null, cancellationTokenSource);
				if (animator.IsUnityNull())
				{
					C2VDebug.LogErrorCategory(nameof(AvatarCreator), $"animator is null. animatorFullIdentifier : {animatorFullIdentifier}");
					return;
				}

				avatarController.SetRuntimeAnimatorController(animator!);
			}
			catch (Exception e)
			{
				if (e is OperationCanceledException)
					C2VDebug.LogCategory("[AvatarLoading]","Cancel LoadRuntimeAnimatorController");
				else
					C2VDebug.LogErrorCategory("[AvatarLoading]",$"Error LoadRuntimeAnimatorController {e.Message}");
				throw;
			}
			
		}
	}
}
