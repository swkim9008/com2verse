/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarController.cs
* Developer:	tlghks1009
* Date:			2022-05-25 14:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Com2Verse.CustomizeLayer;
using Com2Verse.CustomizeLayer.FaceCustomize.Decal;
using Com2Verse.CustomizeLayer.LipsDeco;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Com2Verse.Rendering.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2Verse.Avatar
{
	public partial class AvatarController : MonoBehaviour
	{
		private struct FashionItemData
		{
			public readonly AvatarItemInfo   ItemInfo;
			public readonly FashionItemParts Parts;
			public readonly GameObject       FashionItemPrefab;

			public FashionItemData(AvatarItemInfo itemInfo, FashionItemParts parts, GameObject fashionItemPrefab)
			{
				ItemInfo          = itemInfo;
				Parts             = parts;
				FashionItemPrefab = fashionItemPrefab;
			}
		}

		private readonly string _headBoneName = "Bip001 Head";
		private readonly string _eyeBoneName  = "eye_all_jnt";

#region SerializeField
		[SerializeField] private bool _isMine;

		[SerializeField] private float _lookAtSmoothFactor = 11f;
#endregion SerializeField

#region Events
		private Action? _onCompletedLoadAvatarParts;
		public event Action OnCompletedLoadAvatarParts
		{
			add
			{
				_onCompletedLoadAvatarParts -= value;
				_onCompletedLoadAvatarParts += value;
			}
			remove => _onCompletedLoadAvatarParts -= value;
		}

		private Action? _onClearAvatarParts;

		public event Action OnClearAvatarParts
		{
			add
			{
				_onClearAvatarParts -= value;
				_onClearAvatarParts += value;
			}
			remove => _onClearAvatarParts -= value;
		}

		private Action<bool>? _onGameObjectActive;

		public event Action<bool> OnGameObjectActive
		{
			add
			{
				_onGameObjectActive -= value;
				_onGameObjectActive += value;
			}
			remove => _onGameObjectActive -= value;
		}

		public event Action? OnAvatarBodyChanged;
#endregion Events

#region Properties
		public AvatarInfo? Info { get; private set; }

		public eAnimatorType AnimatorType { get; private set; }

		public bool IsMine
		{
			get => _isMine;
			set
			{
				// IsMine이 true로 설정되기 전에 아바타 파츠가 전부 로드된 경우 렌더링레이어 플래그 재설정
				if (value && IsCompletedLoadAvatarParts)
					FlagRenderingLayerMask();
				_isMine = value;
			}
		}

		/// <summary>
		/// 아바타 파츠가 로드중인지 여부 체크
		/// </summary>
		public bool IsCompletedLoadAvatarParts
		{
			get => _isCompletedLoadAvatarParts;
			private set
			{
				if (value == _isCompletedLoadAvatarParts)
					return;

				_isCompletedLoadAvatarParts = value;

				if (value)
				{
					SetFashionItems();

					_onCompletedLoadAvatarParts?.Invoke();
					SetOverrideEyeBlink(true);
					FlagRenderingLayerMask();
				}
				else
				{
					_onClearAvatarParts?.Invoke();
					SetOverrideEyeBlink(false);
				}

				RefreshSkinnedMeshRendererList();
			}
		}

		public bool IsOnLookAtDelay { get; set; }

		public CancellationTokenSource? LoadingCancellationTokenSource => _loadingCancellationTokenSource;
#endregion Properties

#region Fields
		private readonly Queue<FashionItemData> _willUpdateFashionItemData = new();

		/// <summary>
		/// 현재 아바타의 FashionItem Object 캐싱
		/// </summary>
		private readonly Dictionary<FashionItemParts, GameObject> _avatarFashionItemObjectDict = new();
		private readonly List<SkinnedMeshRenderer> _skinnedMeshRendererList = new();

		private AvatarCustomizeLayer? _avatarCustomizeLayer;
		private bool                  _isCompletedLoadAvatarParts;

		private Transform?            _headBone;
		private Transform?            _eyeBone;

		private Transform? _lookAtTargetOverride;

		public Transform? HeadBone => _headBone;

		private bool       _isOnLookAtEye;
		private Transform? _lookAtTarget;

		private CancellationTokenSource? _loadingCancellationTokenSource;
#endregion Fields

		private void NewLoadingCancellationTokenSource()
		{
			DeleteLoadingCancellationTokenSource();
			_loadingCancellationTokenSource = new CancellationTokenSource();
		}

		private void DeleteLoadingCancellationTokenSource()
		{
			if (_loadingCancellationTokenSource is { IsCancellationRequested: false })
			{
				_loadingCancellationTokenSource?.Cancel();
				_loadingCancellationTokenSource?.Dispose();
				_loadingCancellationTokenSource = null;
			}

			// 아바타 로딩큐에 있는 정보 삭제하여 로딩하지 않게 설정
			AvatarCreatorQueue.Instance.RemoveAvatarLoadInfo(this);
		}

		private static FashionItemParts GetFashionItemParts(eFashionSubMenu fashionType) => fashionType switch
		{
			eFashionSubMenu.TOP => FashionItemParts.Top,
			eFashionSubMenu.BOTTOM => FashionItemParts.Bottom,
			eFashionSubMenu.SHOE => FashionItemParts.Shoe,
			eFashionSubMenu.GLASSES => FashionItemParts.Glass,
			eFashionSubMenu.BAG => FashionItemParts.Bag,
			eFashionSubMenu.HAT => FashionItemParts.Hat,
			_ => throw new ArgumentOutOfRangeException(nameof(fashionType), fashionType, "Invalid FashionType"),
		};

#region UnityEventFuncs
		private void Awake()
		{
			var tr = transform;
			_headBone = tr.FindRecursive(_headBoneName);
			_eyeBone  = tr.FindRecursive(_eyeBoneName);

			_lookAtTargetOverride = new GameObject("EyeLookAtTargetOverride").transform;
			_lookAtTargetOverride.SetParent(tr);

			ResetLookAtTargetOverride();
		}

		private void Update()
		{
			if (_isOnLookAtEye) UpdateLookAtTargetPosition();
		}
#endregion UnityEventFuncs

		private void RefreshSkinnedMeshRendererList()
		{
			_skinnedMeshRendererList.Clear();
			var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
			if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
				return;

			foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
				_skinnedMeshRendererList.Add(skinnedMeshRenderer);
		}

		/// <summary>
		/// 현재 체형에서의 대략적인 키(높이) 값을 구합니다<br/>
		/// </summary>
		/// <returns>현재 체형에서의 대략적인 키(높이) 값을 구합니다</returns>
		public float GetCombinedSkinnedMeshHeight()
		{
			var maxY = 0f;
			if (_skinnedMeshRendererList.Count == 0)
				return maxY;

			foreach (var smr in _skinnedMeshRendererList)
			{
				if (smr.IsUnityNull())
					continue;

				if (!smr!.name.ToUpper().Contains("HEAD")) continue;

				var bounds = smr.bounds;
				maxY = Mathf.Max(maxY, bounds.max.y - transform.position.y);
			}
			return maxY;
		}

		public float GetHeadMeshHeight()
		{
			var height = 0f;
			if (_skinnedMeshRendererList.Count == 0)
				return height;

			foreach (var smr in _skinnedMeshRendererList)
			{
				if (smr.IsUnityNull())
					continue;

				if (!smr!.name.ToUpper().Contains("HEAD")) continue;

				var bounds = smr.bounds;
				height = Mathf.Max(height, bounds.max.y - bounds.min.y);
			}
			return height;
		}

		public void SetBodyShapeIndex(int index)
		{
			if (ErrorIfCustomizeLayerNull()) return;

			var id = AvatarTable.GetBodyShapeResIdToInt(index);
			_avatarCustomizeLayer!.BodyShapeIndex = id;
			_avatarCustomizeLayer.SetBodyShapeIndex(id);
			RefreshSkinnedMeshRendererList();

			Info?.UpdateBodyShapeItem(index);
			OnAvatarBodyChanged?.Invoke();
		}

		/// <summary>
		/// 생성할때 한번만 호출한다. 주로 캐싱할것들 풀에서 가져올때 초기화
		/// </summary>
		public void Create(AvatarInfo avatarInfo, eAnimatorType animatorType, int layer)
		{
			AnimatorType = animatorType;

			Initialize();
			SetAvatarInfo(avatarInfo);
			Utils.Util.ChangeLayersRecursively(transform, layer);
			SetRenderingLayerMaskOfAllSkinnedMeshRenderer(AvatarManager.Instance.AvatarRenderingLayerMask);
			SetActive(false);

			NewLoadingCancellationTokenSource();
		}

		private void Initialize()
		{
			_avatarFashionItemObjectDict.Clear();

			Info = null;
			_avatarCustomizeLayer      = null;
			IsCompletedLoadAvatarParts = false;

			FindAnimator();

			_avatarCustomizeLayer = GetComponent<AvatarCustomizeLayer>() ?? GetComponentInChildren<AvatarCustomizeLayer>();
			if (_avatarCustomizeLayer.IsUnityNull()) return;

			_avatarCustomizeLayer!.OnAwake();
			_avatarCustomizeLayer.UseEyeTarget  = false;
			_avatarCustomizeLayer.UseHeadTarget = false;
		}

		public void OnRelease()
		{
			_willUpdateFashionItemData.Clear();

			DeleteLoadingCancellationTokenSource();
			RemoveAllFashionItem();
			_onGameObjectActive?.Invoke(false);
		}

		public void SetAvatarInfo(AvatarInfo avatarInfo) => Info = avatarInfo;

		public void SetActive(bool active)
		{
			gameObject.SetActive(active);
			_onGameObjectActive?.Invoke(active);
			if (active)
			{
				if (_avatarCustomizeLayer.IsUnityNull()) return;
				_avatarCustomizeLayer!.OnAwake();
			}
		}

#region Apply Fashion Object
		/// <summary>
		/// 루트 부모가 비활성화이면(풀로 돌아갔다면) 파츠 유효하지 않기에 삭제한다
		/// </summary>
		/// <returns>루트 부모의 활성화 여부</returns>
		private bool IsValidFashionItem()
		{
			var parent = transform.parent;
			return !parent.IsUnityNull() && parent!.gameObject.activeInHierarchy;
		}

		private void SetFashionItems()
		{
			if (ErrorIfCustomizeLayerNull()) return;

			var list = new List<(GameObject, FashionItemParts)>();
			while (_willUpdateFashionItemData.TryDequeue(out var result))
			{
				list.Add((result.FashionItemPrefab, result.Parts));
				UpdateAvatarItemInfo(result.ItemInfo);
			}

			var partInstancePairs = _avatarCustomizeLayer!.AddFashionItems(list);
			if (partInstancePairs == null)
				return;

			// TODO: move to customize layer, and remove instance references requirement
			var layer = gameObject.layer;
			foreach (var partInstancePair in partInstancePairs)
			{
				var smr = partInstancePair.instance.GetComponentInChildren<SkinnedMeshRenderer>();
				if (smr.IsReferenceNull())
					continue;

				smr!.renderingLayerMask         = AvatarManager.Instance.AvatarRenderingLayerMask;
				smr.gameObject.layer            = layer;
				partInstancePair.instance.layer = layer;

				// 비동기 완료후 파츠붙일때 부모가 비활성화이면(이미 삭제되어 풀로 돌아갔다면) 파츠 유효하지 않기에 삭제한다.
				if (IsValidFashionItem())
				{
					SetSkinnedMeshRenderer(partInstancePair.p, partInstancePair.instance);
				}
				else
				{
					C2VDebug.LogWarningCategory("[AvatarLoading]", $"이미 아바타가 비활성되어 생성한 파츠 삭제함 - IsCancellationRequested {_loadingCancellationTokenSource?.IsCancellationRequested}");
					if (partInstancePair.instance) Destroy(partInstancePair.instance);
					return;
				}
			}
		}

		private void SetSkinnedMeshRenderer(FashionItemParts fashionItemParts, GameObject fashionItemObject)
		{
			if (_avatarFashionItemObjectDict.TryGetValue(fashionItemParts, out var avatarItemInfo) && !avatarItemInfo.IsUnityNull())
			{
				if (avatarItemInfo) Destroy(avatarItemInfo);
				_avatarFashionItemObjectDict[fashionItemParts] = fashionItemObject;
			}
			else
			{
				_avatarFashionItemObjectDict.TryAdd(fashionItemParts, fashionItemObject);
			}
		}
#endregion Apply Fashion Object

		public async UniTask SetFaceOption(FaceItemInfo faceItemInfo, CancellationTokenSource? cancellationTokenSource = null)
		{
			var value = faceItemInfo.ResId;
			switch (faceItemInfo.FaceOption)
			{
				case eFaceOption.FACE_SHAPE:
					SetFaceShapeConfig(JointCategory.Cheek, value);
					break;
				case eFaceOption.SKIN_TYPE:
					SetFaceTexConfig(FaceTexCategory.SkinColor, value);
					break;
				case eFaceOption.EYE_SHAPE:
					SetFaceShapeConfig(JointCategory.Eye, value);
					break;
				case eFaceOption.PUPIL_TYPE:
					SetFaceTexConfig(FaceTexCategory.Pupil, value);
					break;
				case eFaceOption.EYE_BROW_OPTION:
					SetFaceTexConfig(FaceTexCategory.Eyebrow,      value);
					SetFaceTexConfig(FaceTexCategory.EyebrowColor, faceItemInfo.ColorId);
					break; 
				case eFaceOption.NOSE_SHAPE:
					SetFaceShapeConfig(JointCategory.Nose, value);
					break;
				case eFaceOption.MOUTH_SHAPE:
					SetFaceShapeConfig(JointCategory.Mouth, value);
					break;
				case eFaceOption.EYE_MAKE_UP_TYPE:
					SetFaceTexConfig(FaceTexCategory.DecoEyeShades, value);
					break;
				case eFaceOption.EYE_LASH:
					SetFaceTexConfig(FaceTexCategory.Eyelash,      value);
					SetFaceTexConfig(FaceTexCategory.EyelashColor, faceItemInfo.ColorId);
					break;
				case eFaceOption.CHEEK_TYPE:
					SetFaceTexConfig(FaceTexCategory.DecoCheeks, value);
					break;
				case eFaceOption.LIP_TYPE:
					await SetLipsDecoConfig(faceItemInfo, cancellationTokenSource);
					break;
				case eFaceOption.TATTOO_OPTION:
					await SetDecalConfig(faceItemInfo, cancellationTokenSource);
					break;
				case eFaceOption.HAIR_STYLE:
					await CreateHairItemAsync(faceItemInfo, cancellationTokenSource);
					break;
				case eFaceOption.PRESET_LIST:
				default:
					break;
			}
		}

#region Remove Customize
		/// <summary>
		/// 패션 아이템 모두 제거
		/// </summary>
		private void RemoveAllFashionItem()
		{
			using var iter = _avatarFashionItemObjectDict.GetEnumerator();
			while (iter.MoveNext()) if (iter.Current.Value) Destroy(iter.Current.Value);
			_avatarFashionItemObjectDict.Clear();
			_onClearAvatarParts?.Invoke();
			_avatarCustomizeLayer!.Dispose();
		}

		public void RemoveFaceOption(eFaceOption faceOption)
		{
			if (faceOption == eFaceOption.HAIR_STYLE) RemoveSkinnedMeshRenderer(FashionItemParts.Hair);
			Info?.RemoveFaceItem(faceOption);
		}

		public void RemoveFashionItem(eFashionSubMenu fashionSubMenu)
		{
			var fashionItemParts = GetFashionItemParts(fashionSubMenu);
			if (null != _avatarCustomizeLayer && _avatarCustomizeLayer) _avatarCustomizeLayer!.OnRemovedItem(fashionItemParts);
			else RemoveSkinnedMeshRenderer(fashionItemParts);
			Info?.RemoveFashionItem(fashionSubMenu);
		}

		private void RemoveSkinnedMeshRenderer(FashionItemParts fashionItemParts)
		{
			if (_avatarFashionItemObjectDict.TryGetValue(fashionItemParts, out var avatarItemInfo) && !avatarItemInfo.IsUnityNull())
				if (avatarItemInfo) Destroy(avatarItemInfo);
			_avatarFashionItemObjectDict.Remove(fashionItemParts);
		}

#endregion Remove Customize

		private async UniTask CreateHairItemAsync(FaceItemInfo itemInfo, CancellationTokenSource cancellationTokenSource = null)
		{
			var result = await AvatarCreator.CreateSmrItemAsync(itemInfo, cancellationTokenSource);
			if (result == null)
			{
				var avatarType  = AvatarTable.FaceIdToAvatarType(itemInfo.ItemId);
				result = await AvatarCreator.CreateSmrItemAsync(FaceItemInfo.GetDefaultItemInfo(avatarType, itemInfo.FaceOption), cancellationTokenSource);

				if (result == null)
				{
					C2VDebug.LogErrorCategory(GetType().Name, "CreateHairItemAsync failed.");
					return;
				}

				C2VDebug.LogErrorCategory(GetType().Name, "CreateHairItemAsync result is null. so load default hair.");
			}

			var fashionItem = result.Value;
			var data        = new FashionItemData(fashionItem.AvatarItemInfo, FashionItemParts.Hair, fashionItem.FashionItemPrefab);
			_willUpdateFashionItemData.Enqueue(data);
		}

		public async UniTask SetFashionMenu(FashionItemInfo fashionItemInfo, CancellationTokenSource cancellationTokenSource = null)
		{
			var result = await AvatarCreator.CreateSmrItemAsync(fashionItemInfo, cancellationTokenSource);
			if (result == null)
			{
				var avatarType = AvatarTable.FaceIdToAvatarType(fashionItemInfo.ItemId);
				result = await AvatarCreator.CreateSmrItemAsync(FashionItemInfo.GetDefaultItemInfo(avatarType, fashionItemInfo.FashionSubMenu), cancellationTokenSource);

				if (result == null)
				{
					C2VDebug.LogErrorCategory(GetType().Name, $"CreateHairItemAsync failed. itemId : {fashionItemInfo.ItemId}");
					return;
				}

				C2VDebug.LogErrorCategory(GetType().Name, "CreateHairItemAsync result is null. so load default item.");
			}

			var fashionItem = result.Value;
			var data        = new FashionItemData(fashionItem.AvatarItemInfo, GetFashionItemParts(fashionItemInfo.FashionSubMenu), fashionItem.FashionItemPrefab);
			_willUpdateFashionItemData.Enqueue(data);
		}

#region Apply FaceOption
		private void SetFaceShapeConfig(JointCategory category, int value)
		{
			if (_avatarCustomizeLayer) _avatarCustomizeLayer!.SetFaceShapeConfig(category, value);
		}

		private void SetFaceTexConfig(FaceTexCategory category, int value)
		{
			if (_avatarCustomizeLayer) _avatarCustomizeLayer!.SetFaceTexConfig(category, value);
		}

		private async UniTask SetLipsDecoConfig(FaceItemInfo faceItemInfo, CancellationTokenSource? cancellationTokenSource = null)
		{
			var addressableName   = AvatarTable.GetLipsDecoAddressableName(faceItemInfo);
			var lipsDecoInfoAsset = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<LipsDecoInfoAsset>(addressableName, cancellationTokenSource);
			if (lipsDecoInfoAsset.IsUnityNull() || lipsDecoInfoAsset!.DecoInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"lipsDecoInfoAsset is null. addressableName : {addressableName}");
				return;
			}

			if (ErrorIfCustomizeLayerNull()) return;

			_avatarCustomizeLayer!.SetLipsDecoConfig(lipsDecoInfoAsset.DecoInfo);
		}

		private async UniTask SetDecalConfig(FaceItemInfo faceItemInfo, CancellationTokenSource? cancellationTokenSource = null)
		{
			var addressableName = AvatarTable.GetDecalAddressableName(faceItemInfo);
			var decalInfoAsset  = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<FaceDecalInfoAsset>(addressableName, cancellationTokenSource);
			if (decalInfoAsset.IsUnityNull() || decalInfoAsset!.DecalInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"decalInfoAsset is null. addressableName : {addressableName}");
				return;
			}

			if (ErrorIfCustomizeLayerNull()) return;

			_avatarCustomizeLayer!.SetDecalConfig(decalInfoAsset.DecalInfo);
		}
#endregion Apply FaceOption

		public void OnStartLoadAvatarParts()
		{
			IsCompletedLoadAvatarParts = false;
		}

		public void OnCompleteLoadAvatarParts()
		{
			IsCompletedLoadAvatarParts = true;
		}

		public void SetFaceCustomizeEnable(bool value)
		{
			if (ErrorIfCustomizeLayerNull()) return;

			_avatarCustomizeLayer!.IsFaceCustomizeEnabled = value;
		}

		private bool ErrorIfCustomizeLayerNull()
		{
			if (!_avatarCustomizeLayer.IsUnityNull()) return false;
			C2VDebug.LogWarningCategory(GetType().Name, $"AvatarCustomizeLayer is null");
			return true;
		}

#region Layer Setting
		private void SetRenderingLayerMaskOfAllSkinnedMeshRenderer(uint layerMask)
		{
			var smrArr = GetComponentsInChildren<SkinnedMeshRenderer>();
			if (smrArr == null) return;
			foreach (var smr in smrArr) smr.renderingLayerMask = layerMask;
		}

		private void FlagRenderingLayerMask()
		{
			gameObject.FlagRenderingLayerMask(IsMine);
			gameObject.FlagRenderingLayerMask(!IsMine, RenderStateUtility.eRenderingLayerMask.UNUSED_9);
		}
#endregion Layer Setting

#region AvatarInfo
		private void UpdateAvatarItemInfo(AvatarItemInfo? avatarItemInfo)
		{
			if (avatarItemInfo == null)
				return;

			if (avatarItemInfo is FashionItemInfo fashionItemInfo)
				UpdateFashionItem(fashionItemInfo);
			else if (avatarItemInfo is FaceItemInfo faceItemInfo)
				UpdateFaceItem(faceItemInfo);
		}

		private void UpdateFashionItem(FashionItemInfo? avatarItemInfo)
		{
			if (avatarItemInfo == null)
				return;

			Info?.UpdateFashionItem(avatarItemInfo);
		}

		private void UpdateFaceItem(FaceItemInfo? avatarItemInfo)
		{
			if (avatarItemInfo == null)
				return;

			Info?.UpdateFaceItem(avatarItemInfo);
		}
#endregion AvatarInfo

#region Directing
		private void UpdateLookAtTargetPosition()
		{
			if (ErrorIfCustomizeLayerNull()) return;
			if (_lookAtTarget.IsUnityNull() || _lookAtTargetOverride.IsUnityNull())
				return;

			if (IsOnLookAtDelay)
			{
				var targetPos = _eyeBone!.position + -_eyeBone.up;
				_lookAtTargetOverride!.position = Vector3.Lerp(_lookAtTargetOverride.position, targetPos, Time.deltaTime * _lookAtSmoothFactor);
			}
			else
			{
				_lookAtTargetOverride!.position = Vector3.Lerp(_lookAtTargetOverride.position, _lookAtTarget!.position, Time.deltaTime * _lookAtSmoothFactor);
			}

			_avatarCustomizeLayer!.LookAtTargetEye = _lookAtTargetOverride;
		}

		public void EnableLookAtEye(Transform target)
		{
			if (ErrorIfCustomizeLayerNull()) return;

			ResetLookAtTargetOverride();
			_avatarCustomizeLayer!.UseEyeTarget = true;

			_isOnLookAtEye = true;
			_lookAtTarget  = target;
		}

		public void DisableLookAtEye()
		{
			if (ErrorIfCustomizeLayerNull()) return;

			_avatarCustomizeLayer!.UseEyeTarget = false;

			_isOnLookAtEye = false;
			_lookAtTarget  = null;

			ResetLookAtTargetOverride();
		}

		private void ResetLookAtTargetOverride()
		{
			if (_eyeBone.IsUnityNull() || _lookAtTargetOverride.IsUnityNull())
				return;

			_lookAtTargetOverride!.position = _eyeBone!.position + -_eyeBone.up;
		}

		public void SetOverrideEyeBlink(bool value)
		{
			if (ErrorIfCustomizeLayerNull()) return;

			_avatarCustomizeLayer!.OverrideEyeBlink = value;
		}

		public void SetUseFadeIn(bool value)
		{
			if (ErrorIfCustomizeLayerNull()) return;

			_avatarCustomizeLayer!.UseFadeIn = value;
		}
#endregion Directing

#if ENABLE_CHEATING
		[ContextMenu("PrintAvatarCustomizeItemList")]
		public void PrintCustomizeItemList()
		{
			if (Info == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarController is null");
				return;
			}

			var header = new StringBuilder();
			var body   = new StringBuilder();

			header.AppendLine("AvatarInfo");
			header.AppendLine("-----------------");
			header.Append("AvatarType, ");
			body.Append($"{Info.AvatarType}, ");

			foreach (eFaceOption faceOption in Enum.GetValues(typeof(eFaceOption)))
			{
				if (faceOption == eFaceOption.PRESET_LIST)
					continue;

				header.Append($"{faceOption}, ");
				var item = Info.GetFaceOption(faceOption);
				body.Append(item != null ? $"{item.ItemId}, " : "0, ");
			}

			header.Append("BodyShapeId, ");
			body.Append($"{Info.BodyShape}, ");

			foreach (eFashionSubMenu fashionSubMenu in Enum.GetValues(typeof(eFashionSubMenu)))
			{
				header.Append($"{fashionSubMenu}, ");
				var item = Info.GetFashionItem(fashionSubMenu);
				body.Append(item != null ? $"{item.ItemId}, " : "0, ");
			}

			C2VDebug.Log($"{header}\n{body}");
		}
#endif // ENABLE_CHEATING
	}
}