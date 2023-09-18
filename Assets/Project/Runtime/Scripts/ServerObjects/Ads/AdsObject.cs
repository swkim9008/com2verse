/*===============================================================
* Product:		Com2Verse
* File Name:	AdsObject.cs
* Developer:	haminjeong
* Date:			2023-05-19 17:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.EventTrigger;
using Com2Verse.Extension;
using Com2Verse.PhysicsAssetSerialization;
using Com2Verse.Sound;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Com2Verse.Network
{
	[Serializable]
	public sealed class URLList
	{
		public List<URLPair> URLPairs;
	}
	
	[Serializable]
	public sealed class URLPair
	{
		public string LinkAddress;
		public string DisplayURL;
	}

	[Serializable]
	public sealed class DisplayTime
	{
		public long StartTime;
		public long EndTime;
	}
	
	[Serializable]
	public class AdsData : IDisposable
	{
		public AdsObject.eAdsType AdsType;
		public bool               IsLoaded;
		public string             URL;
		public string             AddressablePath;
		public string             Link;
		[field: ReadOnly]
		public Texture ImageTexture;
		public long         DisplayTime;
		public eAdsLinkType LinkType;
		[field: ReadOnly]
		public Texture ThumbnailTexture;
		public bool    IsThumbnailLoaded;
		public AdsData() { }
		
		public AdsData(string url, string link, AdsObject.eAdsType adsType)
		{
			IsLoaded        = false;
			URL             = url;
			Link            = link;
			AddressablePath = null;
			AdsType         = adsType;
		}

		public void ImageInitialize()
		{
			if (!string.IsNullOrEmpty(AddressablePath))
			{
				C2VAddressables.LoadAssetAsync<Texture2D>(AddressablePath).OnCompleted += (operationHandle) =>
				{
					var loadedAsset = operationHandle.Result;
					if (loadedAsset.IsUnityNull()) return;
					ImageTexture = loadedAsset;
					IsLoaded     = true;
					operationHandle.Release();
				};
			}
			else if (!string.IsNullOrEmpty(URL))
				TextureCache.Instance.GetOrDownloadTextureAsync(URL, (success, texture) =>
				{
					if (success)
						ImageTexture = texture;
					IsLoaded = true;
				}).Forget();
			else
				IsLoaded = true;
		}

		public void YoutubeThumbnailInitialize()
		{
			if (!string.IsNullOrEmpty(URL))
			{
				var youtubeID    = GetYoutubeID();
				var thumbnailURL = string.Format("https://img.youtube.com/vi/{0}/0.jpg", youtubeID);
				TextureCache.Instance.GetOrDownloadTextureAsync(youtubeID, thumbnailURL, (success, texture) =>
				{
					if (success)
						ThumbnailTexture = texture;
					IsThumbnailLoaded = true;
				}).Forget();
			}
		}

		public string GetYoutubeID()
		{
			if (AdsType != AdsObject.eAdsType.YOUTUBE) return string.Empty;
			if (string.IsNullOrEmpty(URL)) return string.Empty;
			var idString = string.Empty;
			if (URL.Contains("youtu.be"))
				idString = URL.Substring(URL.LastIndexOf('/') + 1);
			else if (URL.Contains("www.youtube.com"))
				idString = URL.Substring(URL.LastIndexOf("watch?v=") + 8);
			return idString;
		}

		public void Dispose()
		{
			ImageTexture = null;
		}
	}
	
	public class AdsObject : MonoBehaviour, IClickableObject
	{
		public enum eAdsType
		{
			NONE = 0,
			VIDEO,
			YOUTUBE,
			IMAGE,
		}

#region TableData
		private static TableAdsInfo _tableAdsInfo;
		public static  void         Initialize() => LoadTable();
		private static void LoadTable()
		{
			if (_tableAdsInfo != null) return;
			_tableAdsInfo = TableDataManager.Instance.Get<TableAdsInfo>();
		}

		private static List<AdsData> GetTableDataByBaseObjectID(long objectID)
		{
			if (_tableAdsInfo is not { Datas: { } }) return null;
			List<AdsData> result = new List<AdsData>();
			foreach (var adsData in _tableAdsInfo.Datas!.Values)
			{
				if (adsData!.BaseObjectID == objectID)
					result.Add(new AdsData
					{
						AdsType = (eAdsType)adsData.AdsType,
						AddressablePath = adsData.AddressablePath,
						Link = adsData.Link,
						URL = adsData.URL,
						DisplayTime = adsData.PlayTime,
						LinkType = adsData.LinkType,
					});
			}
			return result;
		}
#endregion

#if UNITY_EDITOR && !METAVERSE_RELEASE
		[SerializeField] protected bool          _useClientTestData = false;
		[SerializeField] protected List<AdsData> _testTagData       = new();
#endif
		[SerializeField] protected eAdsType DefaultAdsType  = eAdsType.NONE;
		[SerializeField] protected bool     UseAdsThumbnail = false;
		protected                  eAdsType CurrentAdsType => CurrentIndex != -1 ? AdsDatas?[CurrentIndex]?.AdsType ?? eAdsType.NONE : eAdsType.NONE;

		private static readonly int    ExposureTimePerImage = 5000; // ms
		private static readonly string AdsMaterialPath      = "AdsDefault.mat";
		private static readonly string ClickableFxTransform = "Renderer/fx_W_Click_01";

		[SerializeField] protected Transform            RendererTransform;
		protected                  MetaverseVideoSource VideoPlayer   { get; set; }
		protected                  YoutubeObject        YoutubeObject { get; set; }
		protected                  MeshRenderer         MeshRenderer  { get; set; }

		private Material _adsMaterial;
		private Material _originMaterial;

		private long _startTime   = -1;
		private long _expiredTime = -1;

		[SerializeField] protected List<AdsData> AdsDatas = new();

		protected int  CurrentIndex { get; set; } = -1;
		private   long _currentExpireTime = 0;

		/// <summary>
		/// Enable 시 테이블로부터 자료를 읽을 지 여부를 결정, false는 기본 프리팹 정보로 로딩한다.
		/// </summary>
		[Tooltip("Enable 시 테이블로부터 자료를 읽을 지 여부를 결정, false는 기본 프리팹 정보로 로딩한다.")]
		[SerializeField] protected bool InitOnEnable = false;

		[Tooltip("비디오 광고에 대한 Mute 공통 설정 - 최초 1회만 적용됩니다.")]
		[SerializeField] protected bool IsVideoMute = true;

		protected string  CurrentYoutubeID   => CurrentIndex != -1 ? (AdsDatas?.Count > 0 ? AdsDatas?[CurrentIndex]?.GetYoutubeID() : null) : null;
		protected string  CurrentURL         => CurrentIndex != -1 ? (AdsDatas?.Count > 0 ? AdsDatas?[CurrentIndex]?.URL : null) : null;
		protected Texture CurrentTexture     => CurrentIndex != -1 ? (AdsDatas?.Count > 0 ? AdsDatas?[CurrentIndex]?.ImageTexture : null) : null;
		protected long    CurrentDisplayTime => (CurrentIndex                         != -1 ? (AdsDatas?.Count > 0 ? AdsDatas?[CurrentIndex]?.DisplayTime : 0) : 0) ?? 0;
		protected eAdsLinkType? CurrentLinkType => CurrentIndex != -1 ? (AdsDatas?.Count > 0 ? AdsDatas?[CurrentIndex]?.LinkType : eAdsLinkType.WEB_VIEW) : eAdsLinkType.WEB_VIEW;

		protected bool IsInitialized { get; set; } = false;

		private C2VEventTrigger _trigger;
		private bool            _hasTriggerEvent = false;

		private bool IsAllDataLoaded       => AdsDatas?.Count > 0   && AdsDatas.Find((ads) => !ads.IsLoaded) == null;
		private bool IsAllTagValuePrepared => _startTime      != -1 && _expiredTime                          != -1 && CurrentIndex == -1 && _currentExpireTime == 0;
		public  bool IsPlaying             { get; protected set; }

		/// <summary>
		/// 동영상 광고에서 플레이 상태를 동기화 해주는 함수.
		/// </summary>
		/// <param name="isPlaying">현재 플레이 상태</param>
		public void SetPlaying(bool isPlaying)
		{
			if (CurrentAdsType == eAdsType.IMAGE) return;
			IsPlaying = isPlaying;
			if (isPlaying)
				ResumeVideo();
			else
				SetDefaultState(CurrentAdsType);
		}

		protected string LinkAddress => CurrentIndex != -1 ? (AdsDatas?.Count > 0 ? AdsDatas?[CurrentIndex]?.Link : null) : null;
		
		private AdsData GetAdsData(int index) => index != -1 ? AdsDatas?[index] : null;

		private eAdsType GetAdsType(int index) => GetAdsData(index)?.AdsType ?? eAdsType.NONE;

#region IClickableObject Implements
		public bool          IsClickableEnable { get; } = true;
		public ClickDelegate OnClickDelegate   { get; set; }

		[SerializeField] private Collider _clickCollider;
		public                   Collider ClickCollider => _clickCollider;

		private bool _isShowThumbnail = false;

		public void InitCollider(bool isContainsLink)
		{
			if (ClickCollider.IsUnityNull()) return;
			var clickableFx = transform.Find(ClickableFxTransform);
			if (!clickableFx.IsUnityNull())
				clickableFx!.gameObject.SetActive(isContainsLink);
			if (isContainsLink)
			{
				var clickDelegate = Util.GetOrAddComponent<ClickDelegate>(ClickCollider!.gameObject);
				clickDelegate.SetDelegate(this);
				ClickCollider.gameObject.layer = LayerMask.NameToLayer("Object");
			}
			else
			{
				var clickDelegate = ClickCollider!.gameObject.GetComponent<ClickDelegate>();
				Destroy(clickDelegate);
			}
		}

		public void OnClickObject()
		{
			if (!IsPlaying) return;
			if (string.IsNullOrEmpty(LinkAddress)) return;

			if (CurrentLinkType.HasValue)
			{
				switch (CurrentLinkType)
				{
					case eAdsLinkType.EXTERNAL_BROWSER:
						Application.OpenURL(LinkAddress);
						break;
					case eAdsLinkType.WEB_VIEW:
					default:
						ShowWebView();
						break;
				}
			}
			else
			{
				ShowWebView();
			}

			void ShowWebView() => UIManager.Instance.ShowPopupWebView(false, new Vector2(1300, 800), LinkAddress);
		}
#endregion

		private void Awake()
		{
			_trigger         = transform.GetComponentInChildren<C2VEventTrigger>();
			_hasTriggerEvent = !_trigger.IsUnityNull();

			if (RendererTransform.IsUnityNull())
				RendererTransform = transform;
		}

		private void Start()
		{
			C2VAddressables.LoadAssetAsync<Material>(AdsMaterialPath).OnCompleted += (operationHandle) =>
			{
				var loadedAsset = operationHandle.Result;
				if (loadedAsset.IsUnityNull()) return;
				_adsMaterial = loadedAsset;
				operationHandle.Release();
			};
		}

		private void InitDefaultExpiredTime()
		{
			_startTime         = MetaverseWatch.Time;
			_expiredTime       = MetaverseWatch.Time + 100000000;
			_currentExpireTime = 0;
		}

		protected void OnEnable()
		{
			InitDefaultExpiredTime();
			if (InitOnEnable)
				InitWithTable().Forget();
			else
				InitData();
		}

		protected void OnDisable()
		{
			SetDefaultState(CurrentAdsType);
			IsInitialized      = false;
			IsPlaying          = false;
			CurrentIndex       = -1;
			_currentExpireTime = 0;
		}

		private async UniTaskVoid InitWithTable()
		{
			if (_tableAdsInfo is not { Datas: { } }) return;
			var mapObject = transform.GetComponent<BaseMapObject>();
			while (mapObject.IsUnityNull())
			{
				await UniTask.Yield();
				mapObject = transform.GetComponent<BaseMapObject>();
			}
			var dataList = GetTableDataByBaseObjectID(mapObject!.ObjectTypeId);
			if (dataList is not { Count: not 0 })
			{
				InitData(); // 테이블 데이터가 없어도 기존 데이터가 있다면 재생할 수 있도록
				return;
			}
			AdsDatas?.Clear();
			AdsDatas?.AddRange(dataList);
			InitData();
		}

		protected void InitComponents(eAdsType type)
		{
			MeshRenderer = Util.GetOrAddComponent<MeshRenderer>(RendererTransform!.gameObject);
			if (!MeshRenderer.IsUnityNull() && _originMaterial.IsUnityNull())
				_originMaterial = MeshRenderer.material;

			if (type == eAdsType.VIDEO)
			{
				if (VideoPlayer.IsUnityNull())
				{
					VideoPlayer         = MetaverseVideoSource.CreateNew(RendererTransform!.gameObject);
					VideoPlayer!.Source = VideoSource.Url;
					VideoPlayer.Mute    = IsVideoMute;
					VideoPlayer.Loop    = true;
				}
				else
					DelayedPreparedPlayer().Forget();
			}
			if (type == eAdsType.YOUTUBE)
			{
				if (YoutubeObject.IsUnityNull())
				{
					YoutubeObject = Util.GetOrAddComponent<YoutubeObject>(gameObject);
					YoutubeObject.SetRenderer(RendererTransform!.GetComponent<Renderer>());
				}
				else if (YoutubeObject!.HasController)
					YoutubeObject.Pause();
			}
		}

		private async UniTask DelayedPreparedPlayer()
		{
			// 비디오플레이어가 Enable된 후 기존 영상을 자동 재생하는 버그가 있어 대기 후 처리
			while (!VideoPlayer.IsUnityNull() && !VideoPlayer!.IsPrepared)
				await UniTask.Yield();
			if (!VideoPlayer.IsUnityNull())
				VideoPlayer!.Pause();
		}

		protected void InitData()
		{
			IsInitialized      = false;
			IsPlaying          = false;
			CurrentIndex       = -1;
			_currentExpireTime = 0;
			bool isContainsLink = false;
			foreach (var data in AdsDatas!)
			{
				if (data == null) continue;
				if (!string.IsNullOrEmpty(data.Link))
					isContainsLink = true;
				InitComponents(data.AdsType);
				if (data.AdsType == eAdsType.IMAGE)
					data.ImageInitialize();
				else
				{
					if (data.AdsType == eAdsType.YOUTUBE && UseAdsThumbnail)
						data.YoutubeThumbnailInitialize();
					data.IsLoaded = true;
				}
			}
			InitCollider(isContainsLink);
		}

		/// <summary>
		/// URLList 자료형으로 태그로 부터 읽어온 광고데이터를 입력합니다.
		/// </summary>
		/// <param name="urlList">url 정보들이 담긴 데이터</param>
		public void InitURL(URLList urlList)
		{
			AdsDatas?.Clear();
			foreach (var pair in urlList?.URLPairs)
			{
				AdsData data = new(pair.DisplayURL, pair.LinkAddress, DefaultAdsType);
				AdsDatas?.Add(data);
			}
			InitData();
		}

		/// <summary>
		/// 광고 노출 정보를 입력합니다.
		/// </summary>
		/// <param name="start">노출 시작 시간</param>
		/// <param name="end">노출 종료 시간</param>
		public void InitTimes(long start, long end)
		{
			IsInitialized      = false;
			IsPlaying          = false;
			_startTime         = start;
			_expiredTime       = end;
			_currentExpireTime = 0;
		}

		protected virtual void Update()
		{
#if UNITY_EDITOR && !METAVERSE_RELEASE
			if (_useClientTestData && _testTagData != null)
			{
				AdsDatas?.Clear();
				AdsDatas?.AddRange(_testTagData);
				InitData();
				_useClientTestData = false;
			}
#endif
			if (!IsInitialized)
			{
				if (IsAllTagValuePrepared)
					IsInitialized = true;
				return;
			}
			if (!IsAllDataLoaded) return;
			if (MetaverseWatch.Time < _startTime) return;

			bool checkThumbnail = false;
			if (!IsPlaying)
			{
				var firstAdsType = GetAdsType(0);
				if ((CurrentAdsType == eAdsType.NONE && firstAdsType == eAdsType.IMAGE) || CurrentAdsType == eAdsType.IMAGE)
					IsPlaying = true;
				else if (CurrentAdsType == eAdsType.NONE && firstAdsType == eAdsType.YOUTUBE && UseAdsThumbnail)
					checkThumbnail = true;
				else
					return;
			}

			if (MetaverseWatch.Time > _expiredTime)
			{
				OnDisable();
				return;
			}

			if (UseAdsThumbnail)
			{
				if (checkThumbnail)
				{
					if (_isShowThumbnail) return;

					var firstAdsData = GetAdsData(0);
					if (!firstAdsData!.IsThumbnailLoaded) return;

					SetAdsThumbnail(0);
					_isShowThumbnail = true;
					return;
				}
				else
					_isShowThumbnail = false;
			}

			if (_currentExpireTime == 0 || _currentExpireTime > (CurrentDisplayTime == 0 ? ExposureTimePerImage : CurrentDisplayTime))
			{
				SetDefaultState(CurrentAdsType);
				CurrentIndex = (CurrentIndex + 1) % AdsDatas?.Count ?? 0;
				SetAds();
			}

			_currentExpireTime += (long)(Time.deltaTime * 1000);
		}

		protected void SetDefaultState(eAdsType prevType)
		{
			switch (prevType)
			{
				case eAdsType.IMAGE:
					if (MeshRenderer.IsUnityNull())
						InitComponents(prevType);
					MeshRenderer!.material = _originMaterial;
					break;
				case eAdsType.VIDEO:
					if (VideoPlayer.IsUnityNull())
						InitComponents(prevType);
					VideoPlayer!.Pause();
					break;
				case eAdsType.YOUTUBE:
					if (YoutubeObject.IsUnityNull())
						InitComponents(prevType);
					if (YoutubeObject!.HasController)
						YoutubeObject.Pause();
					break;
			}
		}

		private void ResumeVideo()
		{
			switch (CurrentAdsType)
			{
				case eAdsType.VIDEO:
					if (VideoPlayer.IsUnityNull()) return;
					VideoPlayer!.Play();
					break;
				case eAdsType.YOUTUBE:
					if (YoutubeObject.IsUnityNull()) return;

					if (YoutubeObject!.HasController)
						YoutubeObject.Resume();
					else
					{
						YoutubeObject.Create(CurrentYoutubeID, IsVideoMute, () =>
						{
							if (_hasTriggerEvent && !TriggerEventManager.Instance.IsInTrigger(_trigger, 0))
								SetDefaultState(CurrentAdsType);
						});
					}
					break;
			}
		}

		/// <summary>
		/// 여러 개의 광고가 있을 때 다음 광고로 넘깁니다.
		/// </summary>
		private void SetAds()
		{
			switch (CurrentAdsType)
			{
				case eAdsType.IMAGE:
					if (MeshRenderer.IsUnityNull()) return;

					if (CurrentTexture.IsUnityNull())
					{
						SetDefaultState(CurrentAdsType);
						return;
					}

					if (!MeshRenderer.IsUnityNull())
						MeshRenderer!.material = _adsMaterial;

					if (MeshRenderer!.material!.HasTexture("_BaseMap"))
						MeshRenderer!.material!.SetTexture("_BaseMap", CurrentTexture);
					else
						MeshRenderer!.material!.mainTexture = CurrentTexture;
					break;
				case eAdsType.VIDEO:
					if (VideoPlayer.IsUnityNull()) return;
					VideoPlayer!.enabled = true;
					VideoPlayer!.Url = CurrentURL;
					
					WaitPlayForPrepareVideo().Forget();
					break;
				case eAdsType.YOUTUBE:
					if (YoutubeObject.IsUnityNull() || RendererTransform.IsUnityNull()) return;

					if (!VideoPlayer.IsUnityNull())
						VideoPlayer!.enabled = false;

					if (YoutubeObject!.HasController)
					{
						YoutubeObject.SetRenderer(RendererTransform!.GetComponent<Renderer>());
						YoutubeObject.Play(CurrentYoutubeID, IsVideoMute);
						if (_hasTriggerEvent && !TriggerEventManager.Instance.IsInTrigger(_trigger, 0))
							SetDefaultState(CurrentAdsType);
					}
					else
					{
						YoutubeObject.Create(CurrentYoutubeID, IsVideoMute, () =>
						{
							if (_hasTriggerEvent && !TriggerEventManager.Instance.IsInTrigger(_trigger, 0))
								SetDefaultState(CurrentAdsType);
						});
					}

					break;
			}

			_currentExpireTime = 0;
		}

		private async UniTask WaitPlayForPrepareVideo()
		{
			if (!VideoPlayer!.IsPrepared)
				VideoPlayer.Prepare();
			
			await UniTask.WaitUntil(() => VideoPlayer.IsPrepared);
			
			if (!MeshRenderer.IsUnityNull())
				MeshRenderer!.material = _adsMaterial;

			VideoPlayer.Play();
			if (_hasTriggerEvent && !TriggerEventManager.Instance.IsInTrigger(_trigger, 0))
				SetDefaultState(CurrentAdsType);
		}

		private void SetAdsThumbnail(int index)
		{
			var adsData = GetAdsData(index);
			if (adsData == null)
			{
				SetDefaultState(eAdsType.IMAGE);
				return;
			}

			switch (adsData.AdsType)
			{
				case eAdsType.YOUTUBE:
					if (MeshRenderer.IsUnityNull()) return;

					if (adsData.ThumbnailTexture.IsUnityNull())
					{
						SetDefaultState(eAdsType.IMAGE);
						return;
					}

					if (!MeshRenderer.IsUnityNull())
						MeshRenderer!.material = _adsMaterial;

					if (MeshRenderer!.material!.HasTexture("_BaseMap"))
						MeshRenderer!.material!.SetTexture("_BaseMap", adsData.ThumbnailTexture);
					else
						MeshRenderer!.material!.mainTexture = adsData.ThumbnailTexture;
					break;
			}

			_currentExpireTime = 0;
		}

		protected virtual void OnDestroy()
		{
			OnDisable();
			if (!VideoPlayer.IsUnityNull())
			{
				Destroy(VideoPlayer);
				VideoPlayer = null;
			}
			if (!YoutubeObject.IsUnityNull())
			{
				Destroy(YoutubeObject);
				YoutubeObject = null;
			}
			MeshRenderer = null;
			AdsDatas?.Clear();
			AdsDatas = null;
		}
	}
}
