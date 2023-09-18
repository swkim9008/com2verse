/*===============================================================
* Product:		Com2Verse
* File Name:	LoadingViewModel.cs
* Developer:	tlghks1009
* Date:			2022-06-10 18:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using eLoadingType = Com2Verse.Data.eLoadingType;
using Random = UnityEngine.Random;

namespace Com2Verse.UI
{
	public sealed class LoadingViewModel : ViewModelBase
	{
		private static readonly string ImageResourceNameFormat = "Loading_{0}_{1}_{2}.png";

		private TableLoadingRes _tableRes;
		private TableLoadingMsg _tableMsg;
		private TableLoadingTip _tableTip;

		private long   _toLoadCount     = 0;
		private long   _loadedCount     = 0;
		private float  _percent         = 0;
		private string _txtPercent      = "0%";
		private string _txtLoading      = string.Empty;
		private string _txtLoadingTip   = string.Empty;
		private Sprite _loadingImage    = null;
		private string _txtLoadingTitle = string.Empty;
		private string _txtLoadingDesc  = string.Empty;
		private bool   _isAdImage       = false;
		private bool   _isShortLoading  = false;

		public override void OnInitialize()
		{
			base.OnInitialize();

			_tableRes = TableDataManager.Instance.Get<TableLoadingRes>();
			_tableMsg = TableDataManager.Instance.Get<TableLoadingMsg>();
			_tableTip = TableDataManager.Instance.Get<TableLoadingTip>();

			Percent         = 0;
			ToLoadCount     = 0;
			LoadedCount     = 0;
			TxtPercent      = "0%";
			TxtLoading      = "";
			TxtLoadingTip   = "";
			IsAdImage       = false;
			IsShortLoading  = false;
			TxtLoadingTitle = "";
			TxtLoadingDesc  = "";
			ImgLoading      = null;

			RegisterLoadingEvent();
		}
		
		public string TxtLoading
		{
			get => _txtLoading;
			set => SetProperty(ref _txtLoading, value);
		}

		public string TxtLoadingTip
		{
			get => _txtLoadingTip;
			set => SetProperty(ref _txtLoadingTip, value);
		}

		public bool IsShortLoading
		{
			get => _isShortLoading;
			set => SetProperty(ref _isShortLoading, value);
		}

		public bool IsAdImage
		{
			get => _isAdImage;
			set => SetProperty(ref _isAdImage, value);
		}

		public Sprite ImgLoading
		{
			get => _loadingImage;
			set => SetProperty(ref _loadingImage, value);
		}

		public string TxtLoadingTitle
		{
			get => _txtLoadingTitle;
			set => SetProperty(ref _txtLoadingTitle, value);
		}
		
		public string TxtLoadingDesc
		{
			get => _txtLoadingDesc;
			set => SetProperty(ref _txtLoadingDesc, value);
		}

		public long ToLoadCount
		{
			get => _toLoadCount;
			set => SetProperty(ref _toLoadCount, value);
		}

		public long LoadedCount
		{
			get => _loadedCount;
			set => SetProperty(ref _loadedCount, value);
		}

		public float Percent
		{
			get => _percent;
			set => SetProperty(ref _percent, value);
		}

		public string TxtPercent
		{
			get => _txtPercent;
			set => SetProperty(ref _txtPercent, value);
		}
		
		private void OnLoadingReset() => OnInitialize();

		private void OnInitLoading()
		{
			LoadingManager.Instance.OnInitLoadingEvent -= OnInitLoading;
			InitLoadingResourcesAsync().Forget();
		}

		private async UniTask InitLoadingResourcesAsync()
		{
			await SetLoadingResources();
			SetLoadingTip();
		}

		private void OnStartLoading(long toLoadCount)
		{
			LoadingManager.Instance.OnStartTaskProcEvent -= OnStartLoading;

			ToLoadCount = toLoadCount;
			
			OnLoadingProc().Forget();
		}

		private void OnCurrentLoading(string currentLoadingTxt)
		{
			if (_tableMsg!.Datas.TryGetValue(currentLoadingTxt, out var msg))
				TxtLoading = Localization.Instance.GetLoadingString(msg.Msg);
		}

		private void OnLoadingCompleted(long loadedCount)
		{
			LoadedCount += loadedCount;
		}

		private void OnLoadingFinished()
		{
			TxtPercent      = "0%";
			TxtLoading      = "";
			TxtLoadingTip   = "";
			IsAdImage       = false;
			IsShortLoading  = false;
			TxtLoadingTitle = "";
			TxtLoadingDesc  = "";
			ImgLoading      = null;
		}

		private async UniTask OnLoadingProc()
		{
			Percent = 0f;
			float toPercent = 0f;
			float progress = 0f;
			int preLoadedCount = 0;

			while (true)
			{
				if (preLoadedCount < LoadedCount)
				{
					if (progress > 1)
					{
						progress = 0;
						preLoadedCount++;
						Percent = toPercent;
					}

					progress += Time.deltaTime * 3f;
					toPercent = LoadedCount / (float) ToLoadCount;
					Percent = Mathf.Lerp(Percent, toPercent, progress);
					TxtPercent = $"{(int) (Percent * 100f)}%";
				}

				if (Percent >= 1f)
					break;

				await UniTask.Yield();
			}

			Percent = 1f;
			TxtPercent = "100%";
			LoadingManager.Instance.OnLoadingCompletedEvent -= OnLoadingCompleted;
		}
		
		private async UniTask SetLoadingResources()
		{
			int              type       = 0;
			List<LoadingRes> targetList = null;
			
			while (LoadingManager.Instance.LoadingSceneProperty == null)
			{
				if (CurrentScene.SceneName.Equals("SceneLogin") || CurrentScene.SceneName.Equals("SceneAvatarSelection"))
				{
					// FIXME: temp code
					type           = (int)eLoadingType.COMMON;
					targetList     = _tableRes!.Datas.Values.Where(res => res.LoadingType == (eLoadingType)type).ToList();
					IsShortLoading = true;
					IsAdImage      = false;

					var tempLoadingRes = targetList[Random.Range(0, targetList.Count)];
					SetImageResourcesName(tempLoadingRes!.LoadingType.ToString(), tempLoadingRes.Res);
					TxtLoadingTitle = Localization.Instance.GetLoadingString(tempLoadingRes.Title);
					TxtLoadingDesc  = Localization.Instance.GetLoadingString(tempLoadingRes.Desc);
					return;
				}
				await UniTask.Yield();
			}
			
			List<int> randomList = new();
			randomList.AddRange(new[] { 0, 1, 2 });
			while ((targetList == null || targetList.Count == 0) && randomList.Count > 0)
			{
				type = randomList[Random.Range(0, randomList.Count)];
				switch (type)
				{
					case 0:
						type = (int)eLoadingType.COMMON;
						break;
					case 1:
						type = (int)LoadingManager.Instance.LoadingSceneProperty!.ServiceType!;
						break;
					case 2:
						type = (int)eLoadingType.AD;
						break;
				}
				targetList = _tableRes!.Datas.Values.Where(res => res.LoadingType == (eLoadingType)type).ToList();
				if (targetList.Count == 0)
					randomList.Remove(type);
			}
			if (targetList!.Count == 0) return;
			var loadingRes = targetList[Random.Range(0, targetList.Count)];
			IsAdImage = type == (int)eLoadingType.AD;
			SetImageResourcesName(loadingRes!.LoadingType.ToString(), loadingRes.Res);
			TxtLoadingTitle = Localization.Instance.GetLoadingString(loadingRes.Title);
			TxtLoadingDesc  = Localization.Instance.GetLoadingString(loadingRes.Desc);
		}

		private void SetImageResourcesName(string type, string res)
		{
			var imagePath = ZString.Format(ImageResourceNameFormat!, type, res, "All");
			if (!C2VAddressables.AddressableResourceExists<Sprite>(imagePath))
			{
				eLanguageCode langCode = (eLanguageCode)Localization.Instance.CurrentLanguage;
				imagePath = ZString.Format(ImageResourceNameFormat!, type, res, langCode.ToString());
			}
			if (!C2VAddressables.AddressableResourceExists<Sprite>(imagePath))
			{
				C2VDebug.LogErrorCategory("LoadingViewModel", $"Image {imagePath} is doesn't exist asset!");
				return;
			}
			C2VAddressables.LoadAssetAsync<Sprite>(imagePath).OnCompleted += (handle) =>
			{
				var loadedAsset = handle.Result;
				if (loadedAsset.IsUnityNull()) return;
				ImgLoading = loadedAsset;
				handle.Release();
			};
		}

		private void SetLoadingTip()
		{
			int              type       = 0;
			List<LoadingTip> targetList = null;
			if (IsShortLoading)
			{
				// FIXME: temp code
				type       = (int)eLoadingType.COMMON;
				targetList = _tableTip!.Datas.Values.Where((res => res.LoadingType == (eLoadingType)type)).ToList();
			}
			else
			{
				List<int> randomList = new();
				randomList.AddRange(new[] { 0, 1 });
				while ((targetList == null || targetList.Count == 0) && randomList.Count > 0)
				{
					type = randomList[Random.Range(0, randomList.Count)];
					switch (type)
					{
						case 0:
							type = (int)eLoadingType.COMMON;
							break;
						case 1:
							type = (int)LoadingManager.Instance.LoadingSceneProperty!.ServiceType!;
							break;
					}

					targetList = _tableTip!.Datas.Values.Where((res => res.LoadingType == (eLoadingType)type)).ToList();
					if (targetList.Count == 0)
						randomList.Remove(type);
				}
				if (targetList!.Count == 0) return;
			}
			var loadingTip = targetList[Random.Range(0, targetList.Count)];
			TxtLoadingTip = Localization.Instance.GetLoadingString(loadingTip!.Tip);
		}
		
		private void RegisterLoadingEvent()
		{
			UnregisterLoadingEvent();

			LoadingManager.Instance.OnLoadingResetEvent     += OnLoadingReset;
			LoadingManager.Instance.OnInitLoadingEvent      += OnInitLoading;
			LoadingManager.Instance.OnCurrentLoadingEvent   += OnCurrentLoading;
			LoadingManager.Instance.OnStartTaskProcEvent    += OnStartLoading;
			LoadingManager.Instance.OnLoadingCompletedEvent += OnLoadingCompleted;
			LoadingManager.Instance.OnLoadingFinishedEvent  += OnLoadingFinished;
		}

		private void UnregisterLoadingEvent()
		{
			LoadingManager.Instance.OnLoadingResetEvent     -= OnLoadingReset;
			LoadingManager.Instance.OnInitLoadingEvent      -= OnInitLoading;
			LoadingManager.Instance.OnCurrentLoadingEvent   -= OnCurrentLoading;
			LoadingManager.Instance.OnStartTaskProcEvent    -= OnStartLoading;
			LoadingManager.Instance.OnLoadingCompletedEvent -= OnLoadingCompleted;
			LoadingManager.Instance.OnLoadingFinishedEvent  -= OnLoadingFinished;
		}
	}
}
