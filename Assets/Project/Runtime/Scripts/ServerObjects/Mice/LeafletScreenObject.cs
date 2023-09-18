/*===============================================================
* Product:		Com2Verse
* File Name:	LeafletScreenObject.cs
* Developer:	ikyoung
* Date:			2023-06-12 19:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;
using UnityEngine.UI;
using Com2Verse.Mice;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Project.InputSystem;
using Com2Verse.EventTrigger;
using Com2Verse.Interaction;
using Com2Verse.Logger;
using Com2Verse.UI;
using Com2Verse.InputSystem;

namespace Com2Verse.Network
{
	


	[Serializable]
	public class UILeafletScreen
	{
		public RawImage rawImage;
		public MetaverseButton buttonShowDetail;
        public GameObject goHighlighted;
    }


	public sealed class LeafletScreenObject : MonoBehaviour
	{
		[SerializeField] GameObject _go3Root;
        [SerializeField] private List<UILeafletScreen> _uiLeaflet3 = new List<UILeafletScreen>();

        [SerializeField] GameObject _go7Root;
        [SerializeField] private List<UILeafletScreen> _uiLeaflet7 = new List<UILeafletScreen>();

        private Dictionary<string, object> _tags = new Dictionary<string, object>();

		private bool _isActiveMode = false;
		private TriggerInEventParameter _triggerEvent;

        private void Awake()
        {
			_isActiveMode = false;

			_go3Root.SetActive(false);
            _uiLeaflet3.ForEach(a => a.goHighlighted.SetActive(false));

            _go7Root.SetActive(false);
            _uiLeaflet7.ForEach(a => a.goHighlighted.SetActive(false));
        }

        public void UpdateTagValue<T>(object tagValue)
		{
			var type = typeof(T); 
			_tags.Remove(type.Name);
			_tags.Add(type.Name, tagValue);
		}

        public bool HasValidTagValue()
        {
	        var tagValue = GetTagValue<LeafletScreenTag>();
	        return tagValue != null;
        }
		public T GetTagValue<T>()
		{
			if (_tags.TryGetValue(typeof(T).Name, out var res))
			{
				return (T)res;
			}
			return default(T);
		}

		public async UniTask LoadAsync()
		{
			Type tagValueType = typeof(LeafletScreenTag);
			if (_tags.TryGetValue(tagValueType.Name, out object res))
			{
				LeafletScreenTag tagValue = (LeafletScreenTag)res;

                var under3 = 3 >= tagValue.leaflets.Count;
				_go3Root.SetActive(under3);
                _go7Root.SetActive(!under3);
				var _curUiLeaflet = under3 ? _uiLeaflet3 : _uiLeaflet7;

                for (int i = 0; i < _curUiLeaflet.Count; i++)
				{
                    int index = i;
					var current = _curUiLeaflet[index];

					if(tagValue.leaflets.Count > index)
					{
						var leaflet = tagValue.leaflets[index];
						var thumbnail = await TextureCache.Instance.GetOrDownloadTextureAsync(leaflet.thumbnailImageUrl);
                        current.rawImage.gameObject.SetActive(true);

                        current.rawImage.texture = thumbnail;

                        current.buttonShowDetail.onClick.RemoveAllListeners();
                        current.buttonShowDetail.onClick.AddListener(()=>
						{
							if (!_isActiveMode) return;
							MiceService.Instance.ShowPDFView(leaflet.pdfLinkUrl);
						});
                        current.buttonShowDetail.OnHighlightedEvent += () =>
						{
                            if (!_isActiveMode) return;
                            current.goHighlighted.SetActive(true);
                        };
                        current.buttonShowDetail.OnNormalEvent += () =>
                        {
                            current.goHighlighted.SetActive(false);
                        };

                    }
					else
					{
                        current.rawImage.gameObject.SetActive(false);
					}
				}
				await UniTask.CompletedTask;
			}
		}

        public void StartLeafletStandInteraction(TriggerInEventParameter triggerInParameter)
        {
            C2VDebug.LogMethod(GetType().Name);
			_triggerEvent = triggerInParameter;

            var vCam = triggerInParameter.ParentMapObject.GetComponentInChildren<FixedCameraJig>();
            CameraManager.Instance.ChangeState(eCameraState.FIXED_CAMERA);
            FixedCameraManager.Instance.SwitchCamera(vCam);
            InteractionManager.Instance.UnsetInteractionUI(triggerInParameter.SourceTrigger, triggerInParameter.CallbackIndex);

			_isActiveMode = true;

            MiceService.Instance.SetUserInteractionState(eMiceUserInteractionState.WithWorldObject);

            UIPopupExitServerObjectViewModel.ShowView(this.gameObject).Forget();

            // 툴바에 있던 UI들을 닫는다.
            ViewModelManager.Instance.Get<MiceToolBarViewModel>()?.CloseAllUI();

        }

        public void StopLeafletStandInteraction()
        {
            C2VDebug.LogMethod(GetType().Name);

            MiceService.Instance.SetUserInteractionState(eMiceUserInteractionState.None);
            CameraManager.Instance.ChangeState(eCameraState.FOLLOW_CAMERA);

            if (_triggerEvent != null)
			{
                InteractionManager.Instance.SetInteractionUI(_triggerEvent.SourceTrigger, _triggerEvent.CallbackIndex, () =>
                {
					StartLeafletStandInteraction(_triggerEvent);
                });
            }

            _isActiveMode = false;
        }
    }
}
